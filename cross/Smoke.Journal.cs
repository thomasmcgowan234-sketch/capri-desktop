using System;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Threading;

namespace CapriDesktop
{
    internal sealed partial class Smoke
    {
        private void JournalChecks()
        {
            CompanionIdentity beforeIdentity = Brain.Identity;
            bool beforeKeeping = Brain.Keeping;
            string directory = Path.Combine(folder, "journal-ui");
            if (Directory.Exists(directory)) throw new Exception("Journal smoke profile already exists.");
            CompanionJournalWindow journal = null;
            try
            {
                Brain.Keeping = true;
                CompanionIdentity identity = Brain.Identity = new CompanionIdentity(directory);
                string proposed = identity.HandleCommand("/identity propose opinion: Explore synthetic strategy games | We compared two fictional strategies.");
                string direct = Regex.Match(proposed, "[a-f0-9]{12}").Value;
                DateTime now = DateTime.UtcNow;
                long revision;
                string result;
                const string citation = "We compared fictional red telescopes and discussed astronomy together.";
                if (!identity.TryBeginDevelopment(now, out revision) || !identity.TryAddDevelopment(revision, "interest", "Consider fictional astronomy", "0123456789abcdef", citation, now.AddSeconds(-1), out result))
                    throw new Exception("Journal fixture could not create a cited automatic draft.");
                string automatic = Regex.Match(result, "[a-f0-9]{12}").Value;
                string retired = Regex.Match(identity.HandleCommand("/identity remember A synthetic retired note"), "[a-f0-9]{12}").Value;
                identity.HandleCommand("/identity reject " + retired);
                string file = Path.Combine(directory, "identity.json");
                string initial = File.ReadAllText(file);
                journal = new CompanionJournalWindow(pet);
                journal.Show();
                Dispatcher.UIThread.RunJobs();
                journal.SearchEntry.Box.Text = "red telescopes";
                Dispatcher.UIThread.RunJobs();
                if (journal.ShownCount != 1 || !journal.DetailText.Contains(citation) || !journal.DetailText.Contains("model-reflection") || !journal.DetailText.Contains("owner-turn") || !journal.DetailText.Contains("0123456789abcdef") || !journal.DetailText.Contains("Created (UTC)") || !journal.DetailText.Contains("not a complete edit history") || File.ReadAllText(file) != initial)
                    throw new Exception("Journal search or full retained provenance changed/misrepresented identity.");
                journal.SavePicture(Path.Combine(folder, "personality-journal.png"));
                checks.Add("PASS personality journal searches captured evidence and shows dates, tentative/model/owner provenance and the retained-history limit without changing identity or contacting a provider");

                journal.StatePicker.Pick(2);
                if (journal.ShownCount != 0) throw new Exception("Journal state filter exposed a tentative draft as established.");
                journal.StatePicker.Pick(1); journal.CategoryPicker.Pick(2);
                if (journal.ShownCount != 1 || !journal.ConfirmButton.IsEnabled || !journal.CorrectionEntry.IsEnabled) throw new Exception("Journal state/category filters or tentative review controls failed.");
                journal.ConfirmButton.Press();
                if (!journal.StatusText.StartsWith("Established", StringComparison.Ordinal) || journal.ShownCount != 0 || !identity.Context("astronomy").Contains("\"ConfirmedByOwner\":true")) throw new Exception("Journal confirmation did not use direct owner review.");
                journal.StatePicker.Pick(0); journal.CategoryPicker.Pick(0);
                journal.SearchEntry.Box.Text = automatic;
                Dispatcher.UIThread.RunJobs();
                journal.CorrectionEntry.Box.Text = "Consider fictional orbital mechanics";
                journal.CorrectButton.Press();
                if (!journal.StatusText.StartsWith("Corrected", StringComparison.Ordinal) || !journal.DetailText.Contains("owner-correction") || !journal.DetailText.Contains("Consider fictional orbital mechanics") || !journal.DetailText.Contains(citation) || !journal.DetailText.Contains("Consider fictional astronomy"))
                    throw new Exception("Journal correction lost the selected record or its original automatic provenance.");
                string corrected = File.ReadAllText(file);
                journal.CorrectionEntry.Box.Text = "My password is a synthetic rejected fixture";
                journal.CorrectButton.Press();
                if (File.ReadAllText(file) != corrected || journal.CorrectionEntry.Box.Text != "My password is a synthetic rejected fixture" || !journal.StatusText.Contains("without private credentials"))
                    throw new Exception("Journal validation changed identity or discarded a repairable correction draft.");
                checks.Add("PASS personality journal preserves a repairable correction draft when identity validation refuses it, without changing saved records");
                journal.RejectButton.Press();
                if (!journal.StatusText.StartsWith("Rejected", StringComparison.Ordinal) || journal.ConfirmButton.IsEnabled || journal.CorrectionEntry.IsEnabled || identity.Context("orbital mechanics").Contains("Consider fictional orbital mechanics")) throw new Exception("Journal rejection retained active controls or conversational identity.");
                checks.Add("PASS personality journal filters states/categories and confirms, corrects and rejects through existing identity review while preserving source evidence and excluding inactive records");

                journal.SearchEntry.Box.Text = direct;
                Dispatcher.UIThread.RunJobs();
                identity.HandleCommand("/identity remember A separate direct owner change");
                journal.ConfirmButton.Press();
                if (!journal.StatusText.Contains("changed") || new CompanionIdentity(directory).ReadJournal(0).Find(direct, "tentative", "opinion").Count != 1) throw new Exception("A stale journal button confirmed a changed identity.");
                checks.Add("PASS personality journal rejects stale review buttons after an intervening identity change and refreshes current records");

                journal.SearchEntry.Box.Text = direct;
                journal.CorrectionEntry.Box.Text = "Private unsaved journal correction";
                Brain.Keeping = false;
                journal.Sync();
                if (journal.ShownCount != 0 || journal.SearchEntry.Box.Text.Length != 0 || journal.CorrectionEntry.Box.Text.Length != 0 || journal.SearchEntry.Box.CanUndo || journal.CorrectionEntry.Box.CanUndo || journal.DetailText.Contains("synthetic") || journal.RecordPicker.Choices.Length != 0 || journal.ConfirmButton.IsEnabled || journal.CorrectionEntry.IsEnabled)
                    throw new Exception("Privacy pause retained private journal controls or text.");
                Brain.Keeping = true;
                journal.Sync();
                if (journal.ShownCount != 4 || journal.CorrectionEntry.Box.Text.Length != 0) throw new Exception("Journal privacy resume revived a draft or lost retained records.");
                Brain.Identity = new CompanionIdentity(Path.Combine(folder, "journal-ui-swapped"));
                journal.Sync();
                if (journal.ShownCount != 0 || journal.DetailText.Contains("fictional") || Directory.Exists(Path.Combine(folder, "journal-ui-swapped"))) throw new Exception("Journal profile switch displayed/wrote the prior profile.");
                string broken = Path.Combine(folder, "journal-ui-future");
                Directory.CreateDirectory(broken);
                const string future = "{\"SchemaVersion\":99,\"Revision\":0,\"Records\":[]}";
                File.WriteAllText(Path.Combine(broken, "identity.json"), future);
                Brain.Identity = new CompanionIdentity(broken);
                journal.Sync();
                if (journal.ShownCount != 0 || !journal.StatusText.Contains("read-only") || journal.ConfirmButton.IsEnabled || File.ReadAllText(Path.Combine(broken, "identity.json")) != future) throw new Exception("Journal changed or disclosed unsupported identity storage.");
                journal.Close();
                if (journal.Refreshing || journal.DetailText.Length != 0 || journal.SearchEntry.Box.Text.Length != 0) throw new Exception("Closed journal kept private text or its refresh timer.");
                checks.Add("PASS personality journal clears search, details, popups and corrections on privacy/profile changes, hides read-only storage and stops its timer on close");
            }
            finally
            {
                if (journal != null && journal.IsVisible) journal.Close();
                Brain.Identity = beforeIdentity;
                Brain.Keeping = beforeKeeping;
            }
        }
    }
}
