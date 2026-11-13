using System.Text.RegularExpressions;
using CapriDesktop;

internal static class JournalChecks
{
    internal static void Run(Action<bool, string> check)
    {
        string root = Path.Combine(Path.GetTempPath(), "companion-journal-integration-" + Guid.NewGuid().ToString("N"));
        CompanionIdentity previous = Brain.Identity;
        bool keeping = Brain.Keeping;
        try
        {
            Brain.Keeping = true;
            var identity = new CompanionIdentity(Path.Combine(root, "first"));
            Brain.Identity = identity;
            string response = Brain.CompanionCommand("/identity propose preference: Consider quiet puzzles | We compared synthetic puzzle modes");
            string id = Regex.Match(response, "[a-f0-9]{12}").Value;
            string file = Path.Combine(root, "first", "identity.json");
            string before = File.ReadAllText(file);
            var snapshot = Brain.ReadIdentityJournal();
            check(snapshot.Available && snapshot.Records.Count == 1 && !Brain.IdentityJournalChanged(snapshot), "production journal reads current identity without changing its revision");
            Brain.Keeping = false;
            var paused = Brain.ReadIdentityJournal();
            check(!paused.Available && paused.Records.Count == 0 && Brain.IdentityJournalChanged(snapshot) && !Brain.IdentityJournalChanged(paused), "memory revocation immediately invalidates a visible journal and hides its records");
            check(Brain.ReviewIdentityJournal(snapshot, "confirm", id, "").Contains("paused") && File.ReadAllText(file) == before, "revoked consent blocks journal review without modifying stored identity");
            Brain.Keeping = true;
            check(Brain.ReviewIdentityJournal(snapshot, "confirm", id, "").Contains("changed") && File.ReadAllText(file) == before, "memory off-on cannot reactivate an earlier journal review");
            snapshot = Brain.ReadIdentityJournal();
            string other = Path.Combine(root, "second");
            Directory.CreateDirectory(other);
            File.Copy(file, Path.Combine(other, "identity.json"));
            Brain.Identity = new CompanionIdentity(other);
            check(Brain.IdentityJournalChanged(snapshot) && Brain.ReviewIdentityJournal(snapshot, "confirm", id, "").Contains("changed") && File.ReadAllText(file) == before && File.ReadAllText(Path.Combine(other, "identity.json")) == before, "profile instance change blocks journal review even with identical revision and record IDs");
            snapshot = Brain.ReadIdentityJournal();
            check(Brain.ReviewIdentityJournal(snapshot, "confirm", id, "").StartsWith("Established", StringComparison.Ordinal) && new CompanionIdentity(other).ReadJournal(0).Records.Single().State == "established", "fresh production journal review persists explicit owner confirmation");
            string established = File.ReadAllText(Path.Combine(other, "identity.json"));
            check(Brain.ReviewIdentityJournal(snapshot, "reject", id, "").Contains("changed") && File.ReadAllText(Path.Combine(other, "identity.json")) == established, "production journal revision fence preserves a newer owner review");
            snapshot = Brain.ReadIdentityJournal();
            File.AppendAllText(Path.Combine(other, "identity.json"), " ");
            string external = File.ReadAllText(Path.Combine(other, "identity.json"));
            check(Brain.ReviewIdentityJournal(snapshot, "correct", id, "Consider collaborative puzzles").Contains("changed outside") && File.ReadAllText(Path.Combine(other, "identity.json")) == external, "journal review preserves concurrent external identity edits");
            Brain.Identity = null;
            check(!Brain.ReadIdentityJournal().Available && Brain.IdentityJournalChanged(snapshot), "journal handles identity removal without reusing stale profile data");
        }
        finally
        {
            Brain.Identity = previous;
            Brain.Keeping = keeping;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
