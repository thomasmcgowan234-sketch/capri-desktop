using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace CapriDesktop
{
    // Local presentation of a bounded identity snapshot. Review never enters chat,
    // speech or a model request; Brain checks consent, profile and revision again.
    internal sealed class CompanionJournalWindow : MainframeWindow
    {
        private const int Wide = 760, Inner = 704;
        private readonly Pet pet;
        private readonly DispatcherTimer refresh = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        private readonly NeonEntry search, correction;
        private readonly NeonPicker state, category, records;
        private readonly SelectableTextBlock detail;
        private readonly TextBlock status;
        private readonly NeonCaption correctionLabel, reviewNote;
        private readonly NeonButton confirm, reject, correct;
        private CompanionJournalSnapshot snapshot;
        private List<CompanionJournalRecord> shown = new List<CompanionJournalRecord>();
        private bool syncing, closed;

        internal CompanionJournalWindow(Pet owner) : base("Personality journal")
        {
            pet = owner;
            ShowInTaskbar = false;
            var title = Put(Page, new NeonCaption { Text = "PERSONALITY JOURNAL", Size = 20, Bold = true, Display = true }, 24, 15, 620, 29);
            var close = Button("", 710, 20, 26, 22, Close); close.Glyph = 2;
            PinHeader(title); PinHeader(close);
            search = Put(Page, new NeonEntry("Search claims, IDs or retained evidence"), 28, 64, Inner, 28, "Search personality journal");
            search.Box.MaxLength = 200;
            search.Box.TextChanging += (_, _) => { if (!syncing) Program.Guard(FillRecords); };
            state = Put(Page, new NeonPicker(), 28, 104, 344, 26, "Identity state");
            state.Options(new[] { "All states", "Tentative", "Established", "Rejected", "Superseded" });
            state.Changed += _ => { if (!syncing) FillRecords(); };
            category = Put(Page, new NeonPicker(), 388, 104, 344, 26, "Identity category");
            category.Options(new[] { "All categories", "Name", "Interest", "Opinion", "Habit", "Preference", "Memory" });
            category.Changed += _ => { if (!syncing) FillRecords(); };
            records = Put(Page, new NeonPicker { Ellipsis = true }, 28, 144, Inner, 28, "Identity record");
            records.Changed += _ => { if (!syncing) SelectRecord(); };
            status = Put(Page, new TextBlock { FontFamily = MainframeTheme.Text, FontSize = 34.0 / 3, Foreground = MainframeTheme.Faint, TextWrapping = TextWrapping.Wrap }, 28, 186, Inner, 60, "Journal status");
            detail = Put(Page, new SelectableTextBlock { FontFamily = MainframeTheme.Text, FontSize = MainframeTheme.Size, Foreground = MainframeTheme.White, TextWrapping = TextWrapping.Wrap }, 28, 254, Inner, 240, "Identity record and retained evidence");
            correctionLabel = Put(Page, new NeonCaption { Text = "Correction", Ink = MainframeTheme.Cyan }, 28, 506, Inner, 20);
            correction = Put(Page, new NeonEntry("Correct the selected claim"), 28, 530, Inner, 70, "Corrected identity claim");
            correction.Box.AcceptsReturn = true;
            correction.Box.TextWrapping = TextWrapping.Wrap;
            correction.Box.VerticalContentAlignment = VerticalAlignment.Top;
            correction.Box.MaxLength = 600;
            correction.Box.TextChanging += (_, _) => { if (!syncing) Program.Guard(EnableReview); };
            confirm = Button("Confirm proposal", 28, 612, 220, 28, () => Review("confirm"));
            confirm.Accent = MainframeTheme.Cyan;
            reject = Button("Reject record", 268, 612, 220, 28, () => Review("reject"));
            correct = Button("Save correction", 508, 612, 224, 28, () => Review("correct"));
            reviewNote = Put(Page, new NeonCaption { Text = "Review changes keep the existing identity backup. Forgotten records may remain in backups.", Ink = MainframeTheme.Faint, Size = 34.0 / 3, Ellipsis = true }, 28, 652, Inner, 22);
            refresh.Tick += (_, _) => Program.Guard(() => Sync());
            Opened += (_, _) => { Sync(true); refresh.Start(); };
            Closed += (_, _) =>
            {
                closed = true; refresh.Stop(); state.CloseMenu(); category.CloseMenu(); records.CloseMenu();
                ClearPrivate(); snapshot = null;
            };
            Sync(true);
        }

        private NeonButton Button(string text, int x, int y, int width, int height, Action action)
        {
            var button = Put(Page, new NeonButton { Text = text }, x, y, width, height, text.Length == 0 ? "Close" : text);
            button.Click += (_, _) => action();
            return button;
        }

        internal void Sync(bool force = false)
        {
            if (closed || syncing || !force && !Brain.IdentityJournalChanged(snapshot)) return;
            syncing = true;
            try
            {
                // Drop search, selected text and unsaved corrections when consent,
                // profile or revision changes, including an off/on cycle.
                ClearPrivate();
                snapshot = Brain.ReadIdentityJournal();
                status.Text = snapshot.Status + "\n" + snapshot.DevelopmentStatus;
                search.IsEnabled = state.IsEnabled = category.IsEnabled = snapshot.Available;
            }
            finally { syncing = false; }
            FillRecords();
        }

        private void ClearPrivate()
        {
            bool before = syncing; syncing = true;
            try
            {
                state.CloseMenu(); category.CloseMenu(); records.CloseMenu();
                search.Box.IsUndoEnabled = correction.Box.IsUndoEnabled = false;
                search.Box.Text = correction.Box.Text = "";
                search.Box.IsUndoEnabled = correction.Box.IsUndoEnabled = true;
                detail.Text = status.Text = "";
                detail.SelectionStart = detail.SelectionEnd = 0;
                shown.Clear(); records.Options(Array.Empty<string>());
                records.Text = "No identity records";
                confirm.IsEnabled = reject.IsEnabled = correct.IsEnabled = correction.IsEnabled = false;
            }
            finally { syncing = before; }
        }

        private void FillRecords()
        {
            if (closed || syncing || snapshot == null) return;
            syncing = true;
            try
            {
                shown = snapshot.Find(search.Box.Text ?? "", state.Selected == 0 ? "all" : state.Choices[state.Selected].ToLowerInvariant(), category.Selected == 0 ? "all" : category.Choices[category.Selected].ToLowerInvariant());
                var names = new string[shown.Count];
                for (int i = 0; i < shown.Count; i++)
                {
                    string claim = shown[i].Text;
                    if (claim.Length > 84) claim = claim.Substring(0, 81) + "...";
                    names[i] = "[" + shown[i].State + " " + shown[i].Category + "] " + claim;
                }
                records.Options(names); records.Selected = 0;
                records.IsEnabled = shown.Count > 0;
                if (shown.Count == 0) records.Text = snapshot.Available ? "No matching identity records" : "Journal unavailable";
            }
            finally { syncing = false; }
            SelectRecord();
        }

        private CompanionJournalRecord Selected
        {
            get { return records.Selected >= 0 && records.Selected < shown.Count ? shown[records.Selected] : null; }
        }

        private void SelectRecord()
        {
            if (closed || syncing) return;
            detail.SelectionStart = detail.SelectionEnd = 0;
            detail.Text = Selected == null ? "No record selected." : Selected.Detail;
            correction.Box.Text = "";
            EnableReview();
            LayoutRecord();
        }

        private void EnableReview()
        {
            var selected = Selected;
            bool active = snapshot != null && snapshot.Available && selected != null && (selected.State == "tentative" || selected.State == "established");
            confirm.IsEnabled = active && selected.State == "tentative";
            reject.IsEnabled = correction.IsEnabled = active;
            correct.IsEnabled = active && !string.IsNullOrWhiteSpace(correction.Box.Text);
        }

        private void LayoutRecord()
        {
            // The detail grows with the bounded evidence record. The normal page
            // scroll exposes every line, without a second competing scroll region.
            status.Height = double.NaN;
            status.Measure(new Size(Inner, double.PositiveInfinity));
            status.Height = Math.Max(38, status.DesiredSize.Height);
            double detailTop = 198 + status.Height;
            Canvas.SetTop(detail, detailTop);
            detail.Height = double.NaN;
            detail.Measure(new Size(Inner, double.PositiveInfinity));
            detail.Height = Math.Max(180, detail.DesiredSize.Height);
            double below = detailTop + detail.Height + 18;
            Canvas.SetTop(correctionLabel, below);
            Canvas.SetTop(correction, below + 26);
            Canvas.SetTop(confirm, below + 108); Canvas.SetTop(reject, below + 108); Canvas.SetTop(correct, below + 108);
            Canvas.SetTop(reviewNote, below + 148);
            double zoom = Pet.FrameSize(pet.Saved.MainframeSize) == 0 ? 1 : pet.Saved.MainframeSize / 100.0;
            Fit(Wide, below + 192, zoom);
        }

        private void Review(string action)
        {
            if (closed || syncing || Selected == null) return;
            string id = Selected.Id;
            string result = Brain.ReviewIdentityJournal(snapshot, action, id, correction.Box.Text ?? "");
            if (Brain.IdentityJournalChanged(snapshot))
            {
                Sync(true);
                int selected = shown.FindIndex(record => record.Id == id);
                if (selected >= 0) { records.Selected = selected; SelectRecord(); }
            }
            status.Text = result + "\n" + snapshot.Status;
            LayoutRecord();
        }

        // The smoke uses the actual controls and review event handlers.
        internal NeonEntry SearchEntry { get { return search; } }
        internal NeonEntry CorrectionEntry { get { return correction; } }
        internal NeonPicker StatePicker { get { return state; } }
        internal NeonPicker CategoryPicker { get { return category; } }
        internal NeonPicker RecordPicker { get { return records; } }
        internal NeonButton ConfirmButton { get { return confirm; } }
        internal NeonButton RejectButton { get { return reject; } }
        internal NeonButton CorrectButton { get { return correct; } }
        internal string DetailText { get { return detail.Text ?? ""; } }
        internal string StatusText { get { return status.Text ?? ""; } }
        internal int ShownCount { get { return shown.Count; } }
        internal bool Refreshing { get { return refresh.IsEnabled; } }
    }
}
