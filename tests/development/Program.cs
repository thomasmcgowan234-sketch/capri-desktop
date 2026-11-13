using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CapriDesktop;

internal static class Program
{
    private static int checks;
    private static void Check(bool value, string description)
    {
        if (!value) throw new Exception(description);
        checks++;
        Console.WriteLine("PASS " + description);
    }

    private sealed class Fixture : IDisposable
    {
        internal readonly string DirectoryPath;
        internal readonly CompanionIdentity Identity;
        internal readonly CompanionDevelopment Development;
        internal DateTime Time = new DateTime(2026, 11, 12, 12, 0, 0, DateTimeKind.Utc);
        internal bool Memory = true, Allowed = true, ConsentPermitted = true, Blocked;
        internal int Epoch = 1, Calls, Cancellations, WrongThreadSnapshots;
        internal string Source = "local-model-v1", Claim = "astronomy", Category = "interest", LastUser, LastSystem;
        internal Func<string, string> Response;
        internal readonly ManualResetEventSlim Started = new ManualResetEventSlim(false);
        internal readonly ManualResetEventSlim Release = new ManualResetEventSlim(false);
        internal readonly ManualResetEventSlim WorkerPosted = new ManualResetEventSlim(false);
        private readonly ConcurrentQueue<Action> posted = new ConcurrentQueue<Action>();
        private readonly List<Action> scheduled = new List<Action>();
        private readonly int uiThread = Thread.CurrentThread.ManagedThreadId;
        private readonly object consent = new object();

        internal Fixture()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), "companion-development-check-" + Guid.NewGuid().ToString("N"));
            Identity = new CompanionIdentity(DirectoryPath);
            Development = new CompanionDevelopment(Identity, Snapshot, Create, Dispatch,
                delegate(int generation, Func<bool> action)
                {
                    lock (consent) return ConsentPermitted && Memory && generation == Epoch && action();
                });
            Development.Now = delegate { return Time; };
            Development.Schedule = delegate(Action action, TimeSpan delay) { scheduled.Add(action); };
        }

        private DevelopmentState Snapshot()
        {
            if (Thread.CurrentThread.ManagedThreadId != uiThread)
            {
                Interlocked.Increment(ref WrongThreadSnapshots);
                throw new Exception("A development worker accessed UI-owned state.");
            }
            return new DevelopmentState { MemoryEnabled = Memory, PrivacyGeneration = Epoch, Source = Source, Allowed = Allowed };
        }

        private DevelopmentInference Create()
        {
            return new DevelopmentInference
            {
                Complete = delegate(string system, string user)
                {
                    LastSystem = system;
                    LastUser = user;
                    Interlocked.Increment(ref Calls);
                    Started.Set();
                    if (Blocked && !Release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Synthetic completion not released.");
                    return Response == null ? Candidate(user, Category, Claim) : Response(user);
                },
                Cancel = delegate { Interlocked.Increment(ref Cancellations); }
            };
        }

        private void Dispatch(Action action)
        {
            posted.Enqueue(action);
            if (Thread.CurrentThread.ManagedThreadId != uiThread) WorkerPosted.Set();
        }

        internal void Pump()
        {
            Action action;
            while (posted.TryDequeue(out action)) action();
        }

        internal void Until(Func<bool> condition)
        {
            DateTime until = DateTime.UtcNow.AddSeconds(10);
            while (!condition())
            {
                Pump();
                if (DateTime.UtcNow >= until) throw new TimeoutException("Synthetic development action did not complete: " + Development.Status);
                Thread.Sleep(5);
            }
            Pump();
        }

        internal void Fire()
        {
            Action[] work = scheduled.ToArray();
            scheduled.Clear();
            foreach (Action action in work) action();
            Pump();
        }

        internal long Input(int number)
        {
            return Development.Input("Owner synthetic turn " + number + " discusses marigold topic " + number, true);
        }

        internal long Feed(int first, int count)
        {
            long token = 0;
            for (int i = 0; i < count; i++) token = Input(first + i);
            return token;
        }

        internal void Start(long token)
        {
            Started.Reset();
            WorkerPosted.Reset();
            Release.Reset();
            Development.Replied(token);
            Fire();
            Until(delegate { return Started.IsSet; });
        }

        internal void Finish()
        {
            Release.Set();
            Until(delegate { return WorkerPosted.IsSet; });
            Pump();
        }

        internal bool HasClaim { get { return Identity.Context(Claim).Contains(Claim); } }

        public void Dispose()
        {
            Development.Dispose();
            Release.Set();
            if (Started.IsSet && !WorkerPosted.IsSet) Until(delegate { return WorkerPosted.IsSet; });
            Pump();
            Started.Dispose(); Release.Dispose(); WorkerPosted.Dispose();
            if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, true);
        }
    }

    private static string CapturedId(string user)
    {
        Match match = Regex.Match(user ?? "", "\\\"id\\\"\\s*:\\s*\\\"([a-f0-9]{16})\\\"");
        if (!match.Success) throw new Exception("No captured owner-turn ID in bounded request.");
        return match.Groups[1].Value;
    }
    private static string Candidate(string user, string category, string claim)
    {
        return "{\"category\":\"" + category + "\",\"text\":\"" + claim + "\",\"turnId\":\"" + CapturedId(user) + "\"}";
    }

    public static int Main()
    {
        try { Run(); Console.WriteLine("PASS " + checks + " automatic development checks; synthetic local inference only"); return 0; }
        catch (Exception error) { Console.Error.WriteLine("FAIL " + error); return 1; }
    }

    private static void Run()
    {
        using (Fixture f = new Fixture())
        {
            long token = f.Feed(0, 7);
            f.Development.Replied(token); f.Fire();
            Check(f.Calls == 0 && !Directory.Exists(f.DirectoryPath), "seven distinct owner turns trigger no inference or storage");
            token = f.Input(7);
            f.Start(token); f.Finish();
            Check(f.Calls == 1 && f.HasClaim, "eight distinct owner turns produce one durable proposal");
            string context = f.Identity.Context("astronomy");
            Check(context.Contains("tentative") && context.Contains("model-reflection") && context.Contains("SupportingOwnerTurn") && !context.Contains("\"ConfirmedByOwner\":true"), "automatic output stays tentative with model and supporting owner-turn provenance");
            string stored = File.ReadAllText(Path.Combine(f.DirectoryPath, "identity.json"));
            Check(stored.Contains("owner-turn") && stored.Contains("Owner synthetic turn 0 discusses marigold topic 0") && !stored.Contains("owner-proposal"), "stored citation comes from the captured owner turn, never fabricated owner approval");
            Check(f.LastUser.Length <= 16000 && Regex.Matches(f.LastUser, "\\\"id\\\"").Count == 8 && f.LastSystem.Length < 5000, "reflection request contains a bounded immutable owner evidence batch");
            Check(f.WrongThreadSnapshots == 0, "worker inference never reads UI-owned policy state");
            f.Development.Replied(token); f.Fire(); f.Pump();
            Check(f.Calls == 1, "repeated reply delivery cannot repeat a consumed inference attempt");
            string id = Regex.Match(f.Identity.HandleCommand("/identity"), @"\b[a-f0-9]{12}\b").Value;
            f.Identity.HandleCommand("/identity confirm " + id);
            Check(f.Identity.Context("astronomy").Contains("\"ConfirmedByOwner\":true"), "only direct owner review establishes an automatically proposed preference");
        }

        using (Fixture f = new Fixture())
        {
            long token = 0;
            for (int i = 0; i < 12; i++) token = f.Development.Input(i % 2 == 0 ? "Repeated owner thought" : "  REPEATED   OWNER thought ", true);
            f.Development.Replied(token); f.Fire();
            Check(f.Calls == 0, "case and whitespace repetition supply only one distinct evidence turn");
            f.Feed(0, 6);
            foreach (string excluded in new[] { "/identity confirm 123456abcdef", "My password is synthetic-secret", "test.person@example.com", new string('a', 601) })
            {
                token = f.Development.Input(excluded, true);
                f.Development.Replied(token); f.Fire();
            }
            token = f.Development.Input("Untrusted participant statement", false);
            f.Development.Replied(token); f.Fire();
            Check(f.Calls == 0, "commands, secrets, oversized text and unauthenticated input add no evidence");
            token = f.Input(6); f.Start(token); f.Finish();
            Check(!f.LastUser.Contains("synthetic-secret") && !f.LastUser.Contains("test.person@example.com") && !f.LastUser.Contains("Untrusted participant statement") && !f.LastUser.Contains("/identity confirm"), "later inference cannot recover excluded secret, command or participant input");
        }

        foreach (string blocked in new[] { "resource", "remote", "consent", "development" })
        {
            using (Fixture f = new Fixture())
            {
                if (blocked == "resource") f.Allowed = false;
                if (blocked == "remote") f.Source = null;
                if (blocked == "consent") f.ConsentPermitted = false;
                if (blocked == "development") f.Identity.HandleCommand("/identity development off");
                long token = f.Feed(0, 8);
                f.Development.Replied(token); f.Fire();
                Check(f.Calls == 0 && !f.HasClaim, blocked + " gate prevents automatic inference");
            }
        }

        using (Fixture f = new Fixture())
        {
            long token = f.Feed(0, 7);
            f.Development.Replied(token); f.Fire();
            Check(f.Calls == 0 && !Directory.Exists(f.DirectoryPath), "private-input fixture begins with only seven eligible owner turns");
            foreach (string privateTurn in new[]
            {
                "I live at 14 Carrington Street.",
                "Meet by 14 Carrington Street.",
                "My school is called Hillcrest High.",
                "My full name is Morgan Example.",
                "My date of birth is 12/03/2001.",
                "Call 0412345678 after our telescope discussion."
            })
            {
                token = f.Development.Input(privateTurn, true);
                f.Development.Replied(token); f.Fire();
                Check(token == 0 && f.Calls == 0 && !Directory.Exists(f.DirectoryPath),
                    "literal personal details are excluded before buffering can trigger inference or storage");
            }
            token = f.Development.Input("Could your companion name be Rowan?", true);
            f.Start(token); f.Finish();
            Check(f.Calls == 1 && f.LastUser.Contains("Could your companion name be Rowan?") && !f.LastUser.Contains("Carrington")
                && !f.LastUser.Contains("Hillcrest") && !f.LastUser.Contains("Morgan Example") && !f.LastUser.Contains("date of birth") && !f.LastUser.Contains("0412345678"),
                "a later benign turn triggers reflection without recovering excluded personal citations");
        }

        using (Fixture f = new Fixture())
        {
            long token = f.Feed(0, 20);
            Check(f.Calls == 0, "collecting owner turns alone never starts background inference");
            f.Start(token); f.Finish();
            Check(Regex.Matches(f.LastUser, "\\\"id\\\"").Count == 8 && !f.LastUser.Contains("Owner synthetic turn 0 ") && f.LastUser.Contains("Owner synthetic turn 19 "), "long conversations retain only the bounded eight most recent distinct evidence turns");
        }

        using (Fixture f = new Fixture())
        {
            f.Feed(0, 7);
            f.Memory = false; f.Epoch++;
            long ignored = f.Development.Input("private-off text", true);
            f.Development.Replied(ignored); f.Fire();
            f.Memory = true; f.Epoch++;
            long token = f.Feed(20, 7);
            f.Development.Replied(token); f.Fire();
            Check(f.Calls == 0, "memory revocation clears buffered evidence before memory is re-enabled");
            token = f.Input(27); f.Start(token); f.Finish();
            Check(f.HasClaim && !f.LastUser.Contains("private-off text") && !f.LastUser.Contains("synthetic turn 0 "), "fresh evidence after re-enable excludes both revoked and off-period input");
        }

        string[] invalid = { "malformed", "unknown-field", "duplicate-field", "missing-field", "array", "trailing-data", "unknown-category", "unknown-evidence", "secret", "private-address", "private-name", "oversized", "null", "forged-quote" };
        foreach (string kind in invalid)
        {
            using (Fixture f = new Fixture())
            {
                f.Response = delegate(string user)
                {
                    string valid = Candidate(user, "interest", "astronomy");
                    if (kind == "malformed") return "{not json";
                    if (kind == "unknown-field") return valid.TrimEnd('}') + ",\"execute\":true}";
                    if (kind == "duplicate-field") return valid.TrimEnd('}') + ",\"category\":\"name\"}";
                    if (kind == "missing-field") return "{\"category\":\"interest\",\"text\":\"astronomy\"}";
                    if (kind == "array") return "[" + valid + "]";
                    if (kind == "trailing-data") return valid + " " + valid;
                    if (kind == "unknown-category") return Candidate(user, "memory", "A made-up event happened");
                    if (kind == "unknown-evidence") return "{\"category\":\"interest\",\"text\":\"astronomy\",\"turnId\":\"0000000000000000\"}";
                    if (kind == "secret") return Candidate(user, "interest", "My password is raw-private-marker");
                    if (kind == "private-address") return Candidate(user, "interest", "Meet by 14 Carrington Street.");
                    if (kind == "private-name") return Candidate(user, "interest", "My full name is Morgan Example.");
                    if (kind == "oversized") return new string('x', 4097);
                    if (kind == "forged-quote") return valid.TrimEnd('}') + ",\"ownerText\":\"Owner confirmed this\"}";
                    return "null";
                };
                long token = f.Feed(0, 8);
                f.Start(token); f.Finish();
                Check(!f.HasClaim && File.ReadAllText(Path.Combine(f.DirectoryPath, "identity.json")).Contains("\"Records\":[]"), kind + " response creates no proposal");
                f.Development.Replied(token); f.Fire();
                Check(f.Calls == 1 && !f.Development.Status.Contains("raw-private-marker"), kind + " response causes no automatic retry or private output in status");
            }
        }

        using (Fixture f = new Fixture())
        {
            long token = f.Feed(0, 8); f.Start(token); f.Finish();
            token = f.Feed(20, 8); f.Development.Replied(token); f.Fire();
            Check(f.Calls == 1, "persisted cooldown prevents a second immediate inference despite eight new turns");
            f.Time = f.Time.AddMinutes(31); f.Claim = "collaborative puzzles";
            token = f.Input(28);
            f.Start(token); f.Finish();
            Check(f.Calls == 2 && f.HasClaim, "new distinct evidence can develop another tentative idea after cooldown");
        }

        using (Fixture f = new Fixture())
        {
            for (int i = 0; i < 3; i++) f.Identity.HandleCommand("/identity propose interest: pending " + i + " | owner evidence " + i);
            long token = f.Feed(0, 8); f.Development.Replied(token); f.Fire();
            Check(f.Calls == 0, "three pending drafts stop further automatic inference");
        }

        foreach (string change in new[] { "new-input", "source", "privacy", "invalidate", "identity-edit", "policy", "consent", "dispose" })
        {
            using (Fixture f = new Fixture())
            {
                f.Blocked = true;
                long token = f.Feed(0, 8); f.Start(token);
                if (change == "new-input") f.Input(30);
                if (change == "source") f.Source = "local-model-v2";
                if (change == "privacy") { f.Memory = false; f.Epoch++; f.Development.Invalidate(true); f.Memory = true; f.Epoch++; }
                if (change == "invalidate") f.Development.Invalidate(false);
                if (change == "identity-edit") f.Identity.HandleCommand("/identity remember Owner supplied a newer correction");
                if (change == "policy") f.Allowed = false;
                if (change == "consent") f.ConsentPermitted = false;
                if (change == "dispose") f.Development.Dispose();
                f.Finish();
                Check(!f.HasClaim, change + " invalidates a returned in-flight result even when transport ignores cancellation");
                if (change == "new-input" || change == "privacy" || change == "invalidate" || change == "dispose")
                    Check(f.Cancellations > 0, change + " attempts owned transport cancellation");
            }
        }

        using (Fixture f = new Fixture())
        {
            long token = f.Feed(0, 8);
            f.Development.Replied(token);
            f.Development.Dispose(); f.Fire();
            Check(f.Calls == 0 && !Directory.Exists(f.DirectoryPath), "disposing cancels pending idle work without later inference or storage");
        }
    }
}
