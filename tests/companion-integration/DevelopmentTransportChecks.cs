using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CapriDesktop;

internal static class DevelopmentTransportChecks
{
    private sealed class Request
    {
        internal string Line, Body;
        internal Dictionary<string, string> Headers;
    }

    private sealed class Response
    {
        internal string Body = Completion;
        internal int Status = 200;
        internal string Location;
    }

    private const string Completion = "{\"choices\":[{\"finish_reason\":\"length\",\"message\":{\"content\":\"null\",\"reasoning_content\":\"synthetic truncated reasoning\"}}]}";
    private const string OllamaCompletion = "{\"done_reason\":\"length\",\"message\":{\"role\":\"assistant\",\"content\":\"null\",\"thinking\":\"synthetic truncated reasoning\"}}";

    // A disposable loopback HTTP fixture. It has no model, download, account or external route.
    private sealed class Server : IDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource stopping = new();
        private readonly List<Request> requests = new();
        private readonly Func<Request, Response> respond;
        private readonly Task worker;
        private Exception failure;
        internal readonly ManualResetEventSlim Received = new(false);

        internal Server(Func<Request, Response> respond = null)
        {
            this.respond = respond ?? (_ => new Response());
            listener.Start();
            Port = ((IPEndPoint)listener.LocalEndpoint).Port;
            worker = Task.Run(Serve);
        }

        internal int Port { get; }
        internal string Url => "http://127.0.0.1:" + Port;
        internal Request[] Requests { get { lock (requests) return requests.ToArray(); } }

        private async Task Serve()
        {
            try
            {
                while (!stopping.IsCancellationRequested)
                {
                    using TcpClient client = await listener.AcceptTcpClientAsync(stopping.Token);
                    client.ReceiveTimeout = client.SendTimeout = 5000;
                    using NetworkStream stream = client.GetStream();
                    Request request = Read(stream);
                    lock (requests) requests.Add(request);
                    Received.Set();
                    Response response = respond(request);
                    byte[] body = Encoding.UTF8.GetBytes(response.Body ?? "");
                    string headers = "HTTP/1.1 " + response.Status + (response.Status == 200 ? " OK" : " Redirect")
                        + "\r\nContent-Type: application/json\r\nConnection: close\r\nContent-Length: " + body.Length
                        + (response.Location == null ? "" : "\r\nLocation: " + response.Location) + "\r\n\r\n";
                    stream.Write(Encoding.ASCII.GetBytes(headers));
                    stream.Write(body);
                    stream.Flush();
                }
            }
            catch (Exception error) when (stopping.IsCancellationRequested && (error is OperationCanceledException || error is SocketException || error is ObjectDisposedException)) { }
            catch (Exception error) { failure = error; }
        }

        private static Request Read(NetworkStream stream)
        {
            var header = new List<byte>();
            while (header.Count < 65536)
            {
                int next = stream.ReadByte();
                if (next < 0) throw new IOException("The synthetic HTTP request ended before its headers.");
                header.Add((byte)next);
                int n = header.Count;
                if (n >= 4 && header[n - 4] == 13 && header[n - 3] == 10 && header[n - 2] == 13 && header[n - 1] == 10) break;
            }
            string[] lines = Encoding.ASCII.GetString(header.ToArray()).Split("\r\n");
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in lines.Skip(1))
            {
                int colon = line.IndexOf(':');
                if (colon > 0) headers.Add(line.Substring(0, colon), line.Substring(colon + 1).Trim());
            }
            int length = headers.TryGetValue("Content-Length", out string value) ? int.Parse(value, System.Globalization.CultureInfo.InvariantCulture) : 0;
            if (length < 0 || length > 1024 * 1024) throw new IOException("Synthetic request exceeds its body bound.");
            byte[] body = new byte[length];
            stream.ReadExactly(body);
            return new Request { Line = lines[0], Headers = headers, Body = Encoding.UTF8.GetString(body) };
        }

        public void Dispose()
        {
            stopping.Cancel();
            listener.Stop();
            if (!worker.Wait(TimeSpan.FromSeconds(8))) throw new TimeoutException("Synthetic HTTP fixture did not stop.");
            Received.Dispose();
            stopping.Dispose();
            if (failure != null) throw new InvalidOperationException("Synthetic HTTP fixture failed.", failure);
        }
    }

    private sealed class RemoteProvider : IBrainProvider
    {
        public string Identity => "synthetic-signed-in-provider";
        public int Generation => 0;
        public string Complete(string model, List<ChatMessage> messages, string tools, List<KeyValuePair<string, ToolArguments>> calls,
            int limit, Action<string> streamed, string purpose, string parts = null) => throw new Exception("Remote development must never be invoked.");
        public void Cancel() { }
    }

    private static bool Cancelled(Action action)
    {
        try { action(); return false; }
        catch (WebException error) { return error.Status == WebExceptionStatus.RequestCanceled; }
    }

    private static bool Denied(Action action)
    {
        try { action(); return false; }
        catch (InvalidOperationException) { return true; }
    }

    private static bool NoToolsOrImages(JsonElement root)
    {
        return !root.TryGetProperty("tools", out _) && !root.TryGetProperty("tool_choice", out _)
            && root.GetProperty("messages").EnumerateArray().All(message => !message.TryGetProperty("images", out _)
                && message.GetProperty("content").ValueKind == JsonValueKind.String);
    }

    internal static void Run(Action<bool, string> check)
    {
        string ollamaWas = Brain.Ollama;
        int replyWas = Brain.ReplyLength, memorySizeWas = Brain.MemorySize;
        bool keepingWas = Brain.Keeping;
        CompanionIdentity identityWas = Brain.Identity;
        CompanionDevelopment developmentWas = Brain.Development;
        string transcriptWas = Brain.TranscriptPath, memoriesWas = Brain.MemoriesPath, notesWas = Brain.MemoryPath;
        string directory = Path.Combine(Path.GetTempPath(), "companion-development-transport-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            Brain.Development = null;
            Brain.Identity = null;
            Brain.TranscriptPath = Brain.MemoriesPath = Brain.MemoryPath = null;
            Brain.Keeping = false;
            Brain.ReplyLength = 3; // Very long normal replies must not enlarge reflection's budget.
            Brain.MemorySize = 32768;

            var remote = new Brain { Model = "synthetic-model", Remote = new Cloud("sk-proj-synthetic", "synthetic-model") };
            check(remote.DevelopmentSource == null && remote.CreateDevelopmentInference() == null, "automatic development excludes configured cloud brains without contacting them");
            remote.Remote = null;
            remote.Provider = new RemoteProvider();
            check(remote.DevelopmentSource == null && remote.CreateDevelopmentInference() == null, "automatic development excludes signed-in provider transports");
            remote.Provider = null;
            remote.Local = new Engine(directory);
            remote.SetServer("https://example.invalid", null);
            check(remote.DevelopmentSource == null && remote.CreateDevelopmentInference() == null, "an explicit remote server outranks a retained local engine for the local-only boundary");
            CompanionObservation.Command("/observe start");
            check(Denied(() => remote.See(new byte[] { 1 }, "synthetic system", "synthetic screen", 10, selected: true)), "a local screenshot lease cannot use an explicit remote server through a retained local engine");
            CompanionObservation.Configure();

            using (var server = new Server())
            using (var other = new Server())
            {
                var brain = new Brain { Model = "synthetic-model" };
                brain.SetServer(server.Url, "synthetic-key-one");
                string source = brain.DevelopmentSource;
                brain.Model = "another-synthetic-model";
                check(source != null && brain.DevelopmentSource != source, "development source fingerprint changes with the selected model");
                brain.Model = "synthetic-model";
                brain.SetServer(server.Url, "synthetic-key-two");
                check(brain.DevelopmentSource != source, "development source fingerprint changes with the selected local server credential");
                brain.SetServer(other.Url, "synthetic-key-one");
                check(brain.DevelopmentSource != source, "development source fingerprint changes with the selected local endpoint");
                brain.SetServer(server.Url, "synthetic-key-one");
                var old = brain.CreateDevelopmentInference();
                brain.SetServer(other.Url, null);
                check(Cancelled(() => old.Complete("synthetic system", "synthetic input")) && server.Requests.Length == 0 && other.Requests.Length == 0,
                    "a source change before dispatch cancels the frozen reflection without contacting either endpoint");
                brain.SetServer(server.Url, "synthetic-key-one");
                string said = brain.CreateDevelopmentInference().Complete("synthetic reflection system", "synthetic owner data");
                Request request = server.Requests.Single();
                using JsonDocument json = JsonDocument.Parse(request.Body);
                check(said == "null" && request.Line == "POST /v1/chat/completions HTTP/1.1" && json.RootElement.GetProperty("max_tokens").GetInt32() == 240,
                    "actual local compatible-server reflection uses exactly one fixed 240-token request despite very long reply settings");
                check(NoToolsOrImages(json.RootElement) && json.RootElement.GetProperty("messages").GetArrayLength() == 2 && request.Headers["Authorization"] == "Bearer synthetic-key-one",
                    "actual reflection request has only its supplied system and owner data, no tools or images, and the captured local credential");
                check(server.Requests.Length == 1, "a length-limited response with reasoning does not trigger an expanded reflection retry");
            }

            using (var server = new Server(_ => new Response { Body = OllamaCompletion }))
            {
                Brain.Ollama = server.Url;
                var brain = new Brain { Model = "synthetic-ollama-model" };
                string said = brain.CreateDevelopmentInference().Complete("synthetic system", "synthetic owner data");
                Request request = server.Requests.Single();
                using JsonDocument json = JsonDocument.Parse(request.Body);
                JsonElement root = json.RootElement;
                check(said == "null" && request.Line == "POST /api/chat HTTP/1.1" && root.GetProperty("options").GetProperty("num_predict").GetInt32() == 240
                    && root.GetProperty("options").GetProperty("num_ctx").GetInt32() <= 8192,
                    "actual Ollama reflection fixes output at 240 tokens and caps context at 8192");
                check(root.GetProperty("think").ValueKind == JsonValueKind.False && NoToolsOrImages(root) && server.Requests.Length == 1,
                    "actual Ollama reflection disables thinking, tools and images and never retries a truncated reasoning response");
            }

            using (var server = new Server())
            {
                var brain = new Brain { Model = "synthetic-local-cloud", Remote = new Cloud(server.Port, "synthetic-local-cloud", server.Url) };
                brain.CreateDevelopmentInference().Complete("synthetic system", "synthetic owner data");
                Request request = server.Requests.Single();
                using JsonDocument json = JsonDocument.Parse(request.Body);
                check(request.Line.StartsWith("POST /v1/chat/completions ", StringComparison.Ordinal) && json.RootElement.GetProperty("max_tokens").GetInt32() == 240 && NoToolsOrImages(json.RootElement),
                    "a loopback Cloud wrapper also uses the single bounded local reflection transport");
            }

            using (var release = new ManualResetEventSlim(false))
            using (var server = new Server(_ =>
            {
                if (!release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("The ordinary synthetic reply was not released.");
                return new Response { Body = "{\"choices\":[{\"finish_reason\":\"stop\",\"message\":{\"content\":\"ordinary fixture reply.\"}}]}" };
            }))
            {
                var brain = new Brain { Model = "synthetic-model" };
                brain.SetServer(server.Url, null);
                Task<string> ordinary = Task.Run(() => brain.Ask("hello", false, out _));
                try
                {
                    if (!server.Received.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("The ordinary synthetic request did not start.");
                    var reflection = brain.CreateDevelopmentInference();
                    reflection.Cancel();
                    check(Cancelled(() => reflection.Complete("synthetic system", "synthetic owner data")) && server.Requests.Length == 1,
                        "cancelling an owned reflection before dispatch sends no new request");
                    release.Set();
                    if (!ordinary.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("The ordinary synthetic reply did not finish.");
                    check(ordinary.GetAwaiter().GetResult() == "ordinary fixture reply.", "reflection cancellation leaves the concurrent ordinary owner reply intact");
                }
                finally
                {
                    release.Set();
                    if (!ordinary.IsCompleted) ordinary.Wait(TimeSpan.FromSeconds(10));
                }
            }

            using (var target = new Server())
            using (var redirect = new Server(_ => new Response { Status = 307, Location = target.Url + "/receive", Body = "{}" }))
            {
                var brain = new Brain { Model = "synthetic-model" };
                brain.SetServer(redirect.Url, null);
                try { brain.CreateDevelopmentInference().Complete("synthetic system", "synthetic owner data"); }
                catch (Exception error) when (error is BrainException || error is WebException) { }
                check(redirect.Requests.Length == 1 && target.Requests.Length == 0, "a redirect cannot forward automatic owner evidence beyond its captured loopback endpoint");
            }
        }
        finally
        {
            CompanionObservation.Configure();
            Brain.Ollama = ollamaWas;
            Brain.ReplyLength = replyWas;
            Brain.MemorySize = memorySizeWas;
            Brain.Identity = identityWas;
            Brain.TranscriptPath = transcriptWas;
            Brain.MemoriesPath = memoriesWas;
            Brain.MemoryPath = notesWas;
            Brain.Keeping = keepingWas;
            Brain.Development = developmentWas;
            Directory.Delete(directory, true);
        }
    }
}
