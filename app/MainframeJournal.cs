using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CapriDesktop
{
    internal sealed partial class Mainframe
    {
        private void OpenIdentityJournal()
        {
            // This is a direct local owner surface. Never route its contents through chat,
            // speech, a provider or the editable personality file.
            using (JournalWindow journal = new JournalWindow()) journal.ShowDialog(this);
        }

        internal sealed class JournalWindow : Form, LookWindow
        {
            private readonly TextBox summary, search, detail, correction;
            private readonly ComboBox state, category;
            private readonly ListBox records;
            private readonly Label status;
            private readonly Button confirm, reject, correct;
            private readonly Timer refresh = new Timer { Interval = 500 };
            private CompanionJournalSnapshot snapshot;
            private bool filling, released;

            private sealed class Row
            {
                internal readonly CompanionJournalRecord Record;
                internal Row(CompanionJournalRecord record) { Record = record; }
                public override string ToString() { return "[" + Record.State + " " + Record.Category + "] " + Record.Text; }
            }

            internal JournalWindow()
            {
                Text = "Personality journal";
                AutoScaleMode = AutoScaleMode.Dpi;
                StartPosition = FormStartPosition.CenterParent;
                ShowInTaskbar = false;
                MinimizeBox = false;
                BackColor = Ground;
                ForeColor = White;
                Font = Look.Mono(10f, FontStyle.Regular, GraphicsUnit.Point);
                ClientSize = new Size(940, 700);
                MinimumSize = new Size(800, 620);

                Label title = new Label { Text = "PERSONALITY JOURNAL", Bounds = new Rectangle(16, 12, 908, 26), ForeColor = Rose, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                summary = Words("Journal status", new Rectangle(16, 42, 908, 65), true);
                summary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                search = Words("Search identity and retained evidence", new Rectangle(16, 118, 300, 26), false);
                search.MaxLength = 160;
                state = Filter("Record state", new Rectangle(336, 118, 154, 26), new[] { "All states", "Tentative", "Established", "Rejected", "Superseded" });
                category = Filter("Record category", new Rectangle(498, 118, 160, 26), new[] { "All categories", "Name", "Interest", "Opinion", "Habit", "Preference", "Memory" });
                Button reload = DialogButton("Refresh", 820, 116, 104, Edge);
                reload.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                reload.AccessibleName = "Refresh personality journal";
                records = new ListBox { Bounds = new Rectangle(16, 154, 300, 426), BackColor = Field, ForeColor = White, BorderStyle = BorderStyle.FixedSingle, IntegralHeight = false,
                    HorizontalScrollbar = true, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left, AccessibleName = "Identity records" };
                detail = Words("Selected identity record and retained evidence", new Rectangle(336, 154, 588, 302), true);
                detail.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                Label correctionLabel = new Label { Text = "Owner correction (up to 600 characters; names up to 40)", Bounds = new Rectangle(336, 464, 588, 20), ForeColor = Faint,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
                correction = Words("Correct the selected identity record", new Rectangle(336, 488, 588, 80), false);
                correction.Multiline = true;
                correction.AcceptsReturn = true;
                correction.ScrollBars = ScrollBars.Vertical;
                correction.MaxLength = 600;
                correction.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                confirm = DialogButton("Confirm", 336, 580, 120, Cyan);
                reject = DialogButton("Reject", 464, 580, 100, Hot);
                correct = DialogButton("Save correction", 572, 580, 172, Cyan);
                confirm.Anchor = reject.Anchor = correct.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                confirm.AccessibleName = "Confirm selected tentative identity proposal";
                reject.AccessibleName = "Reject selected active identity record";
                correct.AccessibleName = "Save owner correction to selected identity record";
                Button close = DialogButton("Close", 820, 648, 104, Edge);
                close.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                status = new Label { Bounds = new Rectangle(16, 620, 788, 58), ForeColor = Faint, Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, AccessibleName = "Journal review result" };
                Controls.AddRange(new Control[] { title, summary, search, state, category, reload, records, detail, correctionLabel, correction, confirm, reject, correct, close, status });
                CancelButton = close;
                close.Click += delegate { Close(); };
                reload.Click += delegate { Reload(null); };
                search.TextChanged += delegate { FilterRecords(null); };
                state.SelectedIndexChanged += delegate { FilterRecords(null); };
                category.SelectedIndexChanged += delegate { FilterRecords(null); };
                records.SelectedIndexChanged += delegate { if (!filling) SelectRecord(); };
                correction.TextChanged += delegate { ReviewAvailability(); };
                confirm.Click += delegate { Review("confirm"); };
                reject.Click += delegate { Review("reject"); };
                correct.Click += delegate { Review("correct"); };
                refresh.Tick += delegate { RefreshIfChanged(); };
                Shown += delegate { RefreshIfChanged(); refresh.Start(); };
                Reload(null);
            }

            private static TextBox Words(string name, Rectangle bounds, bool readOnly)
            {
                return new TextBox { Bounds = bounds, ReadOnly = readOnly, Multiline = readOnly, WordWrap = true, ScrollBars = readOnly ? ScrollBars.Vertical : ScrollBars.None,
                    BorderStyle = BorderStyle.FixedSingle, BackColor = Field, ForeColor = White, MaxLength = int.MaxValue, AccessibleName = name };
            }

            private static ComboBox Filter(string name, Rectangle bounds, string[] choices)
            {
                ComboBox picker = new ComboBox { Bounds = bounds, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Field, ForeColor = White, AccessibleName = name };
                picker.Items.AddRange(choices);
                picker.SelectedIndex = 0;
                return picker;
            }

            private Row Selected { get { return records.SelectedItem as Row; } }

            private static void ClearPrivateText(TextBox box)
            {
                box.Clear();
                box.ClearUndo();
            }

            private void Reload(string selectedId)
            {
                filling = true;
                try
                {
                    CompanionIdentity previousOwner = snapshot == null ? null : snapshot.Owner;
                    int previousConsent = snapshot == null ? -1 : snapshot.ConsentGeneration;
                    // Drop the old record/evidence objects before acquiring a new profile snapshot.
                    snapshot = null;
                    records.Items.Clear();
                    ClearPrivateText(detail);
                    ClearPrivateText(correction);
                    snapshot = Brain.ReadIdentityJournal();
                    // Every refresh path, including a stale review click, clears search on a
                    // consent/profile transition. The timer cannot detect it a second time.
                    if (!snapshot.Available || snapshot.Owner != previousOwner || snapshot.ConsentGeneration != previousConsent) ClearPrivateText(search);
                    summary.Text = snapshot.Status + Environment.NewLine + snapshot.DevelopmentStatus;
                    summary.ClearUndo();
                    status.Text = snapshot.Available ? "Select a record to inspect its retained evidence. Confirming is an explicit owner review; rejecting removes it from conversation context." : snapshot.Status;
                    search.Enabled = state.Enabled = category.Enabled = snapshot.Available;
                }
                finally { filling = false; }
                FilterRecords(selectedId);
            }

            private void FilterRecords(string selectedId)
            {
                if (filling || snapshot == null) return;
                if (Brain.IdentityJournalChanged(snapshot)) { Reload(null); return; }
                if (selectedId == null && Selected != null) selectedId = Selected.Record.Id;
                filling = true;
                try
                {
                    records.Items.Clear();
                    if (snapshot.Available)
                    {
                        string stateFilter = state.SelectedIndex <= 0 ? "all" : state.Text.ToLowerInvariant();
                        string categoryFilter = category.SelectedIndex <= 0 ? "all" : category.Text.ToLowerInvariant();
                        foreach (CompanionJournalRecord record in snapshot.Find(search.Text, stateFilter, categoryFilter))
                        {
                            int index = records.Items.Add(new Row(record));
                            if (record.Id == selectedId) records.SelectedIndex = index;
                        }
                    }
                }
                finally { filling = false; }
                SelectRecord();
            }

            private void SelectRecord()
            {
                ClearPrivateText(correction);
                Row row = Selected;
                detail.Text = row != null ? row.Record.Detail : snapshot != null && snapshot.Available
                    ? "No record selected. Search includes retained evidence; state and category filters can narrow the list. This is a bounded record journal, not a complete conversation or append-only audit history." : "";
                detail.ClearUndo();
                ReviewAvailability();
            }

            private void ReviewAvailability()
            {
                Row row = Selected;
                bool active = snapshot != null && snapshot.Available && row != null && (row.Record.State == "tentative" || row.Record.State == "established");
                confirm.Enabled = active && row.Record.State == "tentative";
                reject.Enabled = active;
                correction.Enabled = active;
                correct.Enabled = active && !string.IsNullOrWhiteSpace(correction.Text);
            }

            private void Review(string action)
            {
                Row row = Selected;
                if (snapshot == null || row == null) return;
                // The shared API checks current memory consent, profile identity and revision.
                // Do not silently refresh a stale selection and then apply its old click.
                string id = row.Record.Id;
                CompanionIdentity owner = snapshot.Owner;
                int generation = snapshot.ConsentGeneration;
                string result = Brain.ReviewIdentityJournal(snapshot, action, id, correction.Text);
                Reload(id);
                status.Text = snapshot.Available && snapshot.Owner == owner && snapshot.ConsentGeneration == generation ? result
                    : snapshot.Available ? "Identity or privacy settings changed. Review the refreshed journal before changing a record." : snapshot.Status;
                status.ForeColor = Cyan;
            }

            internal void RefreshIfChanged()
            {
                if (snapshot == null || Brain.IdentityJournalChanged(snapshot)) Reload(null);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && !released)
                {
                    released = true;
                    refresh.Stop();
                    refresh.Dispose();
                    filling = true;
                    snapshot = null;
                    records.Items.Clear();
                    ClearPrivateText(detail);
                    ClearPrivateText(correction);
                    ClearPrivateText(search);
                    ClearPrivateText(summary);
                    status.Text = "";
                }
                base.Dispose(disposing);
            }
            internal static void Checks(List<string> checks)
            {
                string folder = Path.Combine(Path.GetTempPath(), "companion-native-journal-" + Guid.NewGuid().ToString("N"));
                CompanionIdentity identityWas = Brain.Identity;
                CompanionDevelopment developmentWas = Brain.Development;
                bool keepingWas = Brain.Keeping;
                try
                {
                    Brain.Development = null;
                    Brain.Keeping = true;
                    CompanionIdentity identity = new CompanionIdentity(Path.Combine(folder, "first"));
                    Brain.Identity = identity;
                    string proposed = identity.HandleCommand("/identity propose interest: Collaborative puzzles | Synthetic native journal evidence marker");
                    string id = Regex.Match(proposed, "[a-f0-9]{12}").Value;
                    if (id.Length == 0) throw new Exception("The native journal fixture could not create its proposal.");
                    string path = Path.Combine(folder, "first", "identity.json"), before = File.ReadAllText(path);
                    using (JournalWindow journal = new JournalWindow())
                    {
                        if (journal.records.Items.Count != 1 || File.ReadAllText(path) != before) throw new Exception("Opening the native journal changed identity or lost its record.");
                        journal.records.SelectedIndex = 0;
                        if (!journal.detail.ReadOnly || !journal.detail.Text.Contains("owner-proposal") || !journal.detail.Text.Contains("Created (UTC)") || !journal.detail.Text.Contains("Synthetic native journal evidence marker") || !journal.confirm.Enabled)
                            throw new Exception("The native journal did not show complete readonly proposal evidence and review controls.");
                        checks.Add("PASS native personality journal opens without writes and shows readonly state, timestamps and retained evidence");

                        journal.search.Text = "JOURNAL EVIDENCE";
                        if (journal.records.Items.Count != 1) throw new Exception("The native journal could not search its retained evidence.");
                        journal.state.SelectedIndex = 2;
                        if (journal.records.Items.Count != 0 || journal.confirm.Enabled || journal.reject.Enabled || journal.correct.Enabled) throw new Exception("The native journal state filter left an actionable hidden record.");
                        journal.state.SelectedIndex = 1;
                        journal.category.SelectedIndex = 3;
                        if (journal.records.Items.Count != 0) throw new Exception("The native journal category filter did not narrow the records.");
                        journal.category.SelectedIndex = 2;
                        if (journal.records.Items.Count != 1) throw new Exception("The native journal could not combine its evidence, category and state filters.");
                        journal.records.SelectedIndex = 0;
                        journal.Review("confirm");
                        if (identity.ReadJournal(Brain.MemoryGeneration).Records[0].State != "established" || journal.records.Items.Count != 0) throw new Exception("Native journal confirmation did not preserve owner review or update its state filter.");
                        checks.Add("PASS native journal evidence search and state/category filters retain only matching records and confirmation refreshes the view");

                        journal.search.Clear();
                        journal.state.SelectedIndex = 0;
                        journal.records.SelectedIndex = 0;
                        journal.correction.Text = "A stale native correction must not save";
                        identity.HandleCommand("/identity remember Independent native revision");
                        before = File.ReadAllText(path);
                        journal.Review("correct");
                        if (File.ReadAllText(path) != before || !journal.status.Text.Contains("changed") || journal.correction.Text.Length != 0) throw new Exception("The native journal applied a stale revision or retained its correction draft.");
                        checks.Add("PASS native journal rejects a stale review click and clears the obsolete correction draft");

                        journal.correction.Text = "Collaborative strategy puzzles";
                        journal.Review("correct");
                        CompanionJournalRecord changed = identity.ReadJournal(Brain.MemoryGeneration).Find("Collaborative strategy puzzles", "established", "interest")[0];
                        if (changed.Id != id || !changed.Detail.Contains("owner-correction") || !changed.Detail.Contains("owner-confirmation")) throw new Exception("A native owner correction lost its earlier evidence or state.");
                        journal.Review("reject");
                        if (journal.reject.Enabled || journal.confirm.Enabled || journal.correction.Enabled || identity.Context("puzzles").Contains("Collaborative strategy puzzles")) throw new Exception("The native journal left a rejected record actionable or in conversational context.");
                        checks.Add("PASS native journal correction retains review provenance and rejection removes the active claim from conversational context");

                        journal.category.SelectedIndex = 0;
                        journal.state.SelectedIndex = 2;
                        journal.search.Text = "Independent native";
                        journal.records.SelectedIndex = 0;
                        journal.correction.Text = "Never save while privacy is off";
                        before = File.ReadAllText(path);
                        Brain.Keeping = false;
                        journal.Review("correct");
                        if (File.ReadAllText(path) != before || journal.records.Items.Count != 0 || journal.detail.Text.Length != 0 || journal.correction.Text.Length != 0 || journal.search.Text.Length != 0 || journal.snapshot.Available
                            || journal.search.CanUndo || journal.correction.CanUndo || journal.detail.CanUndo || journal.summary.CanUndo)
                            throw new Exception("A stale native review click retained private search or record content while privacy was off.");
                        checks.Add("PASS native journal stale review clears a populated private search and draft immediately when privacy is off");

                        Brain.Keeping = true;
                        journal.RefreshIfChanged();
                        journal.search.Text = "Independent native";
                        journal.records.SelectedIndex = 0;
                        journal.correction.Text = "Never retain this refresh draft";
                        Brain.Keeping = false;
                        journal.Reload(null);
                        if (journal.search.Text.Length != 0 || journal.correction.Text.Length != 0 || journal.detail.Text.Length != 0 || journal.records.Items.Count != 0 || journal.snapshot.Available || File.ReadAllText(path) != before
                            || journal.search.CanUndo || journal.correction.CanUndo || journal.detail.CanUndo || journal.summary.CanUndo)
                            throw new Exception("An explicit native journal refresh retained private search or drafts after privacy revocation.");
                        checks.Add("PASS explicit native journal refresh after privacy revocation clears all private view text without writes");

                        Brain.Keeping = true;
                        journal.RefreshIfChanged();
                        journal.search.Text = "Independent native";
                        journal.records.SelectedIndex = 0;
                        journal.correction.Text = "Never retain this timer draft";
                        Brain.Keeping = false;
                        journal.RefreshIfChanged();
                        if (journal.records.Items.Count != 0 || journal.detail.Text.Length != 0 || journal.correction.Text.Length != 0 || journal.search.Text.Length != 0 || journal.reject.Enabled || journal.confirm.Enabled || journal.correct.Enabled || journal.snapshot.Available
                            || journal.search.CanUndo || journal.correction.CanUndo || journal.detail.CanUndo || journal.summary.CanUndo)
                            throw new Exception("The native journal retained private records or drafts after memory was disabled.");
                        checks.Add("PASS native journal privacy refresh clears private records, detail, search and correction text");

                        Brain.Keeping = true;
                        journal.RefreshIfChanged();
                        journal.category.SelectedIndex = 0;
                        journal.state.SelectedIndex = 2;
                        journal.search.Text = "Independent native";
                        journal.records.SelectedIndex = 0;
                        string selected = journal.Selected.Record.Id;
                        before = File.ReadAllText(path);
                        Brain.Keeping = false;
                        Brain.Keeping = true;
                        journal.correction.Text = "Do not revive stale consent";
                        journal.Review("correct");
                        if (File.ReadAllText(path) != before || !journal.status.Text.Contains("changed") || journal.correction.Text.Length != 0 || journal.search.Text.Length != 0) throw new Exception("A native journal click survived memory off-then-on.");
                        checks.Add("PASS native journal review cannot survive a privacy off-then-on cycle before its timer refresh");

                        journal.Reload(selected);
                        journal.search.Text = "Independent native";
                        journal.correction.Text = "Do not cross profiles";
                        Brain.Identity = new CompanionIdentity(Path.Combine(folder, "second"));
                        journal.Review("correct");
                        if (File.ReadAllText(path) != before || Directory.Exists(Path.Combine(folder, "second")) || journal.records.Items.Count != 0 || journal.correction.Text.Length != 0 || journal.search.Text.Length != 0 || !journal.status.Text.Contains("changed"))
                            throw new Exception("The native journal applied a stale profile review or retained the prior profile's records.");
                        checks.Add("PASS native journal rejects a profile-swap review without writing either profile");

                        Directory.CreateDirectory(Path.Combine(folder, "future"));
                        File.WriteAllText(Path.Combine(folder, "future", "identity.json"), "{\"SchemaVersion\":99,\"Revision\":0,\"Records\":[]}");
                        Brain.Identity = new CompanionIdentity(Path.Combine(folder, "future"));
                        journal.RefreshIfChanged();
                        if (journal.snapshot.Available || journal.records.Items.Count != 0 || journal.detail.Text.Length != 0 || !journal.summary.Text.Contains("read-only") || journal.confirm.Enabled || journal.reject.Enabled || journal.correct.Enabled)
                            throw new Exception("The native journal exposed a failed-closed identity store for review.");
                        checks.Add("PASS native journal hides unavailable future-schema storage and disables every review control");
                    }
                }
                finally
                {
                    Brain.Identity = identityWas;
                    Brain.Keeping = keepingWas;
                    Brain.Development = developmentWas;
                    if (Directory.Exists(folder)) Directory.Delete(folder, true);
                }
            }
        }

        internal static void JournalChecks(List<string> checks) { JournalWindow.Checks(checks); }
    }
}
