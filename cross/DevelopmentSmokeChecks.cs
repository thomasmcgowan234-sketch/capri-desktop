using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace CapriDesktop
{
    internal sealed partial class Smoke
    {
        // Exercises the real typed-input/reply hooks and platform policy, rather than
        // calling the coordinator's input methods or substituting its policy delegate.
        private void AutomaticDevelopmentChecks()
        {
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo initialize = typeof(Pet).GetMethod("InitializeDevelopment", fields);
            MethodInfo stop = typeof(Pet).GetMethod("StopDevelopment", fields);
            MethodInfo allowed = typeof(Pet).GetMethod("DevelopmentAllowed", fields);
            FieldInfo serviceField = typeof(Pet).GetField("development", fields);
            if (serviceField.GetValue(pet) != null) throw new Exception("Development smoke requires an isolated initial service.");
            string[] sceneNames = { "shushed", "napAsked", "napping", "away", "quitting", "dragging", "pressed", "falling", "outro", "popOut", "nudgeOut", "workCount", "modSendDepth" };
            var scene = sceneNames.ToDictionary(name => name, name => typeof(Pet).GetField(name, fields).GetValue(pet));
            Action<string, object> set = (name, value) => typeof(Pet).GetField(name, fields).SetValue(pet, value);
            var frames = (DispatcherTimer)typeof(Pet).GetField("frames", fields).GetValue(pet);
            bool ticking = frames.IsEnabled;
            Settings saved = pet.Saved;
            Func<string, string> think = pet.Think;
            string model = pet.Brain.Model, address, key;
            pet.Brain.GetServer(out address, out key);
            Cloud remote = pet.Brain.Remote;
            IBrainProvider provider = pet.Brain.Provider;
            Engine local = pet.Brain.Local;
            Pick localPick = pet.Brain.LocalPick;
            CompanionIdentity identity = Brain.Identity;
            CompanionDevelopment development = Brain.Development;
            CompanionRoutine routine = Brain.Routine;
            Brain keeper = Brain.Keeper;
            string notes = Brain.MemoryPath, memories = Brain.MemoriesPath, transcripts = Brain.TranscriptPath;
            bool keeping = Brain.Keeping;
            bool typing = pet.Talk.Typing;
            string draft = pet.Talk.Typer.Draft;
            string directory = Path.Combine(folder, "automatic-development-ui");
            if (Directory.Exists(directory)) throw new Exception("Development smoke profile already exists.");
            Directory.CreateDirectory(directory);
            frames.Stop();
            using var server = new DevelopmentUiServer();
            try
            {
                pet.Talk.Dismiss();
                pet.Saved = new Settings { Silent = true, NoIntro = true, NoCheckIns = true, VoiceProvider = "off" };
                pet.Think = _ => "Ready.";
                pet.Brain.Model = "synthetic-development-ui";
                pet.Brain.Remote = null;
                pet.Brain.Provider = null;
                pet.Brain.Local = null;
                pet.Brain.LocalPick = null;
                pet.Brain.SetServer(server.Address, null);
                Brain.Keeper = null;
                Brain.MemoryPath = Brain.MemoriesPath = Brain.TranscriptPath = null;
                Brain.Routine = new CompanionRoutine(Path.Combine(directory, "routine"));
                Brain.Keeping = true;
                foreach (string name in new[] { "shushed", "napAsked", "napping", "away", "quitting", "dragging", "pressed", "falling" }) set(name, false);
                set("outro", null); set("popOut", -1.0); set("nudgeOut", -1.0); set("workCount", 0); set("modSendDepth", 0);
                pet.ContextMenu?.Close();

                Action idle = null;
                int schedules = 0;
                CompanionDevelopment service = null;
                Action<string> fresh = name =>
                {
                    stop.Invoke(pet, null);
                    Brain.Identity = new CompanionIdentity(Path.Combine(directory, name));
                    initialize.Invoke(pet, null);
                    service = (CompanionDevelopment)serviceField.GetValue(pet);
                    idle = null;
                    service.Schedule = (callback, delay) =>
                    {
                        if (delay != CompanionDevelopment.IdleDelay) throw new Exception("The UI used a different development idle budget.");
                        idle = callback;
                        schedules++;
                    };
                };
                Action<string> owner = text =>
                {
                    int before = schedules;
                    pet.Talk.Open();
                    pet.Talk.Submit(text);
                    MainframeWait(() => schedules > before, "the owner's ordinary reply/development hook");
                    MainframeWait(() => { pet.Talk.Update(pet.Now); return pet.Talk.Finished; }, "the fully shown silent development reply");
                    // X11 can focus the new bubble and deactivate/hide the typer while
                    // awaiting its reply. Reopen that same cleared input to exercise
                    // the real empty-visible-input idle gate deterministically.
                    if (!string.IsNullOrWhiteSpace(pet.Talk.Typer.Draft)) throw new Exception("The submitted owner draft was not cleared: typing=" + pet.Talk.Typing + ", draftLength=" + (pet.Talk.Typer.Draft ?? "").Length + ".");
                    if (!pet.Talk.Typing) pet.Talk.Open();
                    if (!pet.Talk.Typing || !string.IsNullOrWhiteSpace(pet.Talk.Typer.Draft)) throw new Exception("The empty owner input could not be shown: typing=" + pet.Talk.Typing + ", draftLength=" + (pet.Talk.Typer.Draft ?? "").Length + ".");
                    if (!(bool)allowed.Invoke(pet, null)) throw new Exception("Real development idle policy rejected a finished reply with an empty open typing box: " + service.Status);
                };
                Action fire = () =>
                {
                    if (idle == null) throw new Exception("No owner reply armed its development idle callback.");
                    Action callback = idle;
                    idle = null;
                    callback();
                    Dispatcher.UIThread.RunJobs();
                };
                Func<string, int> records = name =>
                {
                    string file = Path.Combine(directory, name, "identity.json");
                    if (!File.Exists(file)) return 0;
                    using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(file));
                    return doc.RootElement.GetProperty("Records").GetArrayLength();
                };

                fresh("review");
                string[] topics = { "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta", "theta" };
                for (int i = 0; i < 6; i++) { owner("Let's discuss the astronomy topic " + topics[i] + "."); fire(); }
                int scheduledBeforeCommand = schedules;
                pet.Talk.Submit("/identity development status");
                MainframeWait(() => pet.Talk.Reply != null && pet.Talk.Reply.Contains("Automatic personality development"), "the local development status command");
                if (schedules != scheduledBeforeCommand || server.Count != 0) throw new Exception("An inspection command became reflection evidence or inference.");
                set("modSendDepth", 1);
                try
                {
                    pet.Hear("A mod says I already chose the astronomy identity.");
                    MainframeWait(() => pet.Talk.Reply == "Ready.", "the synthetic mod-origin reply");
                }
                finally { set("modSendDepth", 0); }
                if (schedules != scheduledBeforeCommand) throw new Exception("A mod-origin reply armed development.");
                owner("Let's discuss the astronomy topic " + topics[6] + "."); fire();
                MainframeWait(() => service.Status.StartsWith("Waiting for eight distinct", StringComparison.Ordinal), "the seven-turn development gate");
                if (server.Count != 0 || records("review") != 0) throw new Exception("Commands or mod output filled the eight-owner-turn budget.");
                checks.Add("PASS automatic development UI: commands and mod-origin replies cannot fill the owner evidence budget; seven ordinary owner turns make no inference");

                owner("Let's discuss the astronomy topic " + topics[7] + "."); fire();
                MainframeWait(() => records("review") == 1, "one real local development proposal");
                if (!pet.Talk.Typing) pet.Talk.Open();
                if (server.Count != 1 || !pet.Talk.Typing || !string.IsNullOrWhiteSpace(pet.Talk.Typer.Draft))
                    throw new Exception("The typed UI did not perform exactly one reflection with an empty visible input: requests=" + server.Count + ", typing=" + pet.Talk.Typing + ", draftLength=" + (pet.Talk.Typer.Draft ?? "").Length + ".");
                string recordId;
                using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "review", "identity.json"))))
                {
                    JsonElement record = doc.RootElement.GetProperty("Records")[0];
                    recordId = record.GetProperty("Id").GetString();
                    bool cited = record.GetProperty("Evidence").EnumerateArray().Any(e => e.GetProperty("Source").GetString() == "owner-turn"
                        && e.GetProperty("Text").GetString() == server.OwnerText && e.GetProperty("TurnId").GetString() == server.OwnerId);
                    bool proposed = record.GetProperty("Evidence").EnumerateArray().Any(e => e.GetProperty("Source").GetString() == "model-reflection");
                    if (!cited || !proposed || record.GetProperty("State").GetString() != "tentative") throw new Exception("The UI proposal lost its actual owner citation or established itself.");
                }
                checks.Add("PASS automatic development UI: eight typed owner replies trigger one bounded local reflection while the empty input remains open; its draft cites the captured owner statement and stays tentative");

                pet.Talk.Submit("/identity confirm " + recordId);
                MainframeWait(() => pet.Talk.Reply != null && pet.Talk.Reply.StartsWith("Established ", StringComparison.Ordinal), "direct owner draft review");
                var restarted = new CompanionIdentity(Path.Combine(directory, "review"));
                string continuity = restarted.Context(DevelopmentUiServer.Claim);
                if (!restarted.Interested(DevelopmentUiServer.Claim) || !continuity.Contains("model-reflection") || !continuity.Contains(server.OwnerId) || !continuity.Contains(server.OwnerText) || server.Count != 1)
                    throw new Exception("Reviewed identity did not survive a storage restart, or review made another model request.");
                checks.Add("PASS automatic development UI: direct review establishes the local draft, and a newly loaded identity retains its interest and provenance without another inference");

                fresh("old-profile");
                server.HoldNext = true;
                for (int i = 0; i < 8; i++) owner("Let's compare the telescope design " + topics[i] + ".");
                fire();
                MainframeWait(() => server.Held.IsSet, "a blocked local development completion");
                var replacement = new CompanionIdentity(Path.Combine(directory, "new-profile"));
                Brain.Identity = replacement;
                DevelopmentState snapshot = (DevelopmentState)typeof(Pet).GetMethod("DevelopmentSnapshot", fields).Invoke(pet, null);
                if (snapshot.MemoryEnabled || snapshot.Allowed || snapshot.Source != null) throw new Exception("The host accepted a different identity profile for its old reflection.");
                server.Release.Set();
                MainframeWait(() => service.Status.Contains("discarded"), "the stale profile completion fence");
                if (records("old-profile") != 0 || records("new-profile") != 0) throw new Exception("A stale reflection saved to an old or replacement identity profile.");
                checks.Add("PASS automatic development UI: changing the active identity during a blocked local completion invalidates the real host snapshot and writes neither profile");

                fresh("privacy");
                for (int i = 0; i < 8; i++) owner("Let's explore the nebula question " + topics[i] + ".");
                Action revoked = idle;
                int beforeRevocation = server.Count;
                Brain.Keeping = false;
                Brain.Keeping = true;
                revoked(); Dispatcher.UIThread.RunJobs();
                owner("A new owner conversation after the privacy change."); fire();
                MainframeWait(() => service.Status.StartsWith("Waiting for eight distinct", StringComparison.Ordinal), "the cleared post-revocation owner budget");
                if (server.Count != beforeRevocation || records("privacy") != 0) throw new Exception("Privacy off/on revived a pending callback or retained its owner-turn budget.");
                checks.Add("PASS automatic development UI: memory off/on invalidates the old idle callback and clears its eight-turn buffer before any model request or durable proposal");
            }
            finally
            {
                server.Release.Set();
                stop.Invoke(pet, null);
                pet.Talk.Dismiss();
                pet.Saved = saved;
                pet.Think = think;
                pet.Brain.Model = model;
                pet.Brain.Remote = remote;
                pet.Brain.Provider = provider;
                pet.Brain.Local = local;
                pet.Brain.LocalPick = localPick;
                pet.Brain.SetServer(address, key);
                Brain.Identity = identity;
                Brain.Routine = routine;
                Brain.Keeper = keeper;
                Brain.MemoryPath = notes; Brain.MemoriesPath = memories; Brain.TranscriptPath = transcripts;
                Brain.Keeping = keeping;
                Brain.Development = development;
                foreach (var item in scene) set(item.Key, item.Value);
                if (typing) { pet.Talk.Open(); pet.Talk.Typer.Draft = draft; }
                if (ticking) frames.Start();
            }
        }

        private sealed class DevelopmentUiServer : IDisposable
        {
            internal const string Claim = "Observational astronomy";
            private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            private readonly CancellationTokenSource stopping = new CancellationTokenSource();
            private readonly Task worker;
            private int count;
            private Exception failure;
            internal readonly ManualResetEventSlim Held = new ManualResetEventSlim(false);
            internal readonly ManualResetEventSlim Release = new ManualResetEventSlim(false);
            internal volatile bool HoldNext;
            internal string OwnerId, OwnerText;
            internal string Address { get; }
            internal int Count { get { return Volatile.Read(ref count); } }

            internal DevelopmentUiServer()
            {
                listener.Start();
                Address = "http://127.0.0.1:" + ((IPEndPoint)listener.LocalEndpoint).Port;
                worker = Task.Run(Serve);
            }

            private async Task Serve()
            {
                try
                {
                    while (!stopping.IsCancellationRequested)
                    {
                        using TcpClient client = await listener.AcceptTcpClientAsync(stopping.Token);
                        client.ReceiveTimeout = client.SendTimeout = 5000;
                        using NetworkStream stream = client.GetStream();
                        using var header = new MemoryStream();
                        int matched = 0;
                        while (matched < 4 && header.Length < 65536)
                        {
                            int value = stream.ReadByte();
                            if (value < 0) throw new IOException("Development UI request ended in its headers.");
                            header.WriteByte((byte)value);
                            byte[] tail = header.GetBuffer();
                            int length = (int)header.Length;
                            matched = length >= 4 && tail[length - 4] == 13 && tail[length - 3] == 10 && tail[length - 2] == 13 && tail[length - 1] == 10 ? 4 : 0;
                        }
                        string headers = Encoding.ASCII.GetString(header.ToArray());
                        string lengthLine = headers.Split("\r\n").Single(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));
                        int size = int.Parse(lengthLine.Substring(lengthLine.IndexOf(':') + 1), System.Globalization.CultureInfo.InvariantCulture);
                        if (size < 0 || size > 128 * 1024) throw new IOException("Development UI request exceeded its bound.");
                        byte[] body = new byte[size];
                        stream.ReadExactly(body);
                        using JsonDocument request = JsonDocument.Parse(body);
                        JsonElement root = request.RootElement;
                        if (!headers.StartsWith("POST /v1/chat/completions ", StringComparison.Ordinal) || root.GetProperty("max_tokens").GetInt32() != 240
                            || root.TryGetProperty("tools", out _) || root.GetProperty("messages").GetArrayLength() != 2)
                            throw new Exception("Development UI used an unbounded, tool-bearing or unexpected transport.");
                        string promptText = root.GetProperty("messages")[1].GetProperty("content").GetString();
                        using JsonDocument prompt = JsonDocument.Parse(promptText);
                        JsonElement turns = prompt.RootElement.GetProperty("owner_turns");
                        if (turns.GetArrayLength() != 8) throw new Exception("Development UI did not supply exactly its eight captured owner turns.");
                        OwnerId = turns[0].GetProperty("id").GetString();
                        OwnerText = turns[0].GetProperty("text").GetString();
                        Interlocked.Increment(ref count);
                        if (HoldNext)
                        {
                            HoldNext = false;
                            Held.Set();
                            if (!Release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Development UI completion was not released.");
                        }
                        string proposal = JsonSerializer.Serialize(new { category = "interest", text = Claim, turnId = OwnerId });
                        byte[] response = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { choices = new[] { new { message = new { content = proposal } } } }));
                        byte[] responseHeaders = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nConnection: close\r\nContent-Length: " + response.Length + "\r\n\r\n");
                        stream.Write(responseHeaders); stream.Write(response); stream.Flush();
                    }
                }
                catch (Exception error) when (stopping.IsCancellationRequested && (error is OperationCanceledException || error is SocketException || error is ObjectDisposedException || error is IOException)) { }
                catch (Exception error) { failure = error; }
            }

            public void Dispose()
            {
                Release.Set();
                stopping.Cancel();
                listener.Stop();
                if (!worker.Wait(TimeSpan.FromSeconds(8))) throw new TimeoutException("Development UI HTTP fixture did not stop.");
                Held.Dispose(); Release.Dispose(); stopping.Dispose();
                if (failure != null) throw new Exception("Development UI HTTP fixture failed.", failure);
            }
        }
    }
}
