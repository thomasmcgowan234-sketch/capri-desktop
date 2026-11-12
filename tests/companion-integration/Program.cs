using System.Text.RegularExpressions;
using CapriDesktop;

sealed class FakeProvider : IBrainProvider
{
    public string Identity => "synthetic-provider";
    public int Generation => 0;
    public int Calls;
    public string Prompt;
    public string Complete(string model, List<ChatMessage> messages, string tools, List<KeyValuePair<string, ToolArguments>> calls, int limit, Action<string> streamed, string purpose, string parts = null)
    {
        Calls++;
        Prompt = string.Join("\n", messages.Select(m => m.Content));
        return "I remember the topic.";
    }
    public void Cancel() { }
}
static class Program
{
    static int checks;
    static void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    static bool Denied(Action action) { try { action(); return false; } catch (InvalidOperationException) { return true; } }
    static int Main()
    {
        string directory = Path.Combine(Path.GetTempPath(), "companion-integrated-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            Brain.Keeping = true;
            Brain.Identity = new CompanionIdentity(Path.Combine(directory, "identity"));
            Brain.PersonalityPath = Brain.MemoryPath = Brain.MemoriesPath = Brain.TranscriptPath = null;
            string proposed = Brain.CompanionCommand("/identity name Rowan");
            string id = Regex.Match(proposed, "[a-f0-9]{12}").Value;
            Check(id.Length == 12 && Brain.Identity.Name == "", "desktop command path keeps names tentative");
            Brain.CompanionCommand("/identity confirm " + id);
            for (int i = 0; i < 18; i++) Brain.CompanionCommand("/identity remember Synthetic topic number " + i + ".");
            Brain.CompanionCommand("/identity remember We discussed the amber telescope.");
            for (int i = 0; i < 18; i++) Brain.CompanionCommand("/identity remember Later synthetic note " + i + ".");
            var provider = new FakeProvider();
            var brain = new Brain { Model = "fixture-model", Provider = provider };
            brain.Ask("What about the amber telescope?", false, out _);
            Check(provider.Calls == 1 && provider.Prompt.Contains("Rowan") && provider.Prompt.Contains("amber telescope"), "real Brain request retrieves relevant persistent identity and older memory");
            Check(Brain.DefaultPersonality.Contains("without a permanent name") && !Brain.DefaultPersonality.Contains("Arasaka"), "default prompt has no compulsory fictional biography");
            brain.Model = "another-fixture-model";
            brain.Ask("What is my companion's name?", false, out _);
            Check(provider.Prompt.Contains("Rowan") && new CompanionIdentity(Path.Combine(directory, "identity")).Name == "Rowan", "model switch leaves disk identity intact and present in next request");
            Brain.Keeping = false;
            Check(Brain.IdentityContext() == "" && Brain.CompanionCommand("/identity name Other").Contains("paused"), "memory privacy switch suppresses retrieval and command mutation");
            Brain.Keeping = true;
            CompanionObservation.Configure();
            int before = provider.Calls;
            Check(Denied(() => brain.See(new byte[] { 1 }, "system", "fixture", 10, selected: true)) && provider.Calls == before, "screen denial occurs before provider sees any image");
            CompanionObservation.Command("/observe start");
            Check(Denied(() => brain.See(new byte[] { 1 }, "system", "fixture", 10, selected: true)) && provider.Calls == before, "local screenshot lease cannot escape through signed-in provider transport");
            Check(Denied(() => brain.Quick("system", "fixture", 10, new byte[] { 1 })) && provider.Calls == before, "background image requests obey same provider consent boundary");
            CompanionObservation.Command("/observe start cloud");
            brain.See(new byte[] { 1 }, "system", "fixture", 10, selected: true);
            Check(provider.Calls == before + 1, "explicit remote screenshot lease reaches synthetic provider");
            CompanionObservation.Command("/observe stop");
            Check(Denied(() => brain.See(new byte[] { 1 }, "system", "fixture", 10, selected: true)) && provider.Calls == before + 1, "revocation blocks the next image before transmission");

            var routine = new CompanionRoutine(Path.Combine(directory, "routine"));
            DateTime now = new(2026, 11, 12, 21, 59, 0);
            routine.Now = () => now;
            Check(!routine.Paused && !Directory.Exists(Path.Combine(directory, "routine")), "fresh routine imposes no hours or eager persistence");
            routine.Command("/quiet hours 22:00-08:00", false);
            Check(!routine.QuietNow, "overnight quiet window excludes minute before start");
            now = now.AddMinutes(1); Check(routine.QuietNow, "quiet window includes exact start");
            now = now.AddHours(9).AddMinutes(59); Check(routine.QuietNow, "overnight quiet window includes minute before end");
            now = now.AddMinutes(1); Check(!routine.QuietNow, "quiet window excludes exact end");
            var restored = new CompanionRoutine(Path.Combine(directory, "routine")) { Now = () => new DateTime(2026, 11, 12, 23, 0, 0) };
            Check(restored.Paused && restored.Status.Contains("22:00-08:00"), "quiet hours survive restart in PC-local time");
            string old = File.ReadAllText(Path.Combine(directory, "routine", "routine.json"));
            foreach (string bad in new[] { "/quiet hours 22:00-22:00", "/quiet hours 25:00-09:00", "/quiet hours 9:00-10:00", "/quiet hours nonsense" })
                routine.Command(bad, false);
            Check(File.ReadAllText(Path.Combine(directory, "routine", "routine.json")) == old, "invalid quiet hours never mutate saved policy");
            routine.Command("/quiet hours off", false); routine.Command("/performance on", true);
            Check(routine.LowResource && routine.Paused && File.Exists(Path.Combine(directory, "routine", "routine.json.previous")), "Cross performance switch persists atomically with recovery backup");
            routine.Command("/performance off", true); Check(!routine.Paused, "performance off restores normal independent activity");
            Check(routine.Command("/performance on", false) == null, "Windows retains its existing performance-setting authority");
            File.WriteAllText(Path.Combine(directory, "routine", "routine.json"), "{\"Schema\":999}");
            var future = new CompanionRoutine(Path.Combine(directory, "routine"));
            Check(future.Paused && future.Command("/quiet hours off", false).Contains("invalid") && File.ReadAllText(Path.Combine(directory, "routine", "routine.json")) == "{\"Schema\":999}", "future routine schema pauses autonomy without overwriting data");
            Console.WriteLine("PASS " + checks + " integrated companion checks; synthetic provider only");
            return 0;
        }
        finally { Brain.Identity = null; Brain.Routine = null; CompanionObservation.Configure(); Directory.Delete(directory, true); }
    }
}
