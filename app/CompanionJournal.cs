using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace CapriDesktop
{
    // A private display snapshot, never a provider prompt or a second identity store.
    internal sealed class CompanionJournalSnapshot
    {
        internal readonly CompanionIdentity Owner;
        internal readonly int ConsentGeneration;
        internal readonly long Revision;
        internal readonly bool Available;
        internal readonly string Status, DevelopmentStatus;
        internal readonly ReadOnlyCollection<CompanionJournalRecord> Records;

        internal CompanionJournalSnapshot(CompanionIdentity owner, int generation, long revision, bool available,
            string status, string developmentStatus, List<CompanionJournalRecord> records)
        {
            Owner = owner; ConsentGeneration = generation; Revision = revision; Available = available;
            Status = status; DevelopmentStatus = developmentStatus;
            Records = records.AsReadOnly();
        }

        internal static CompanionJournalSnapshot Unavailable(string status, int generation)
        {
            return new CompanionJournalSnapshot(null, generation, -1, false, status, "", new List<CompanionJournalRecord>());
        }

        internal List<CompanionJournalRecord> Find(string search, string state, string category)
        {
            string query = (search ?? "").Trim();
            List<CompanionJournalRecord> found = new List<CompanionJournalRecord>();
            foreach (CompanionJournalRecord record in Records)
            {
                if (!All(state) && !string.Equals(state, record.State, StringComparison.OrdinalIgnoreCase)
                    || !All(category) && !string.Equals(category, record.Category, StringComparison.OrdinalIgnoreCase)) continue;
                if (query.Length == 0 || record.Detail.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) found.Add(record);
            }
            return found;
        }

        private static bool All(string value) { return string.IsNullOrEmpty(value) || value.Equals("all", StringComparison.OrdinalIgnoreCase); }
    }

    internal sealed class CompanionJournalEvidence
    {
        internal readonly string Source, Text, Utc, TurnId;
        internal CompanionJournalEvidence(IdentityEvidence evidence)
        {
            Source = evidence.Source; Text = evidence.Text; Utc = evidence.Utc; TurnId = evidence.TurnId;
        }
    }

    internal sealed class CompanionJournalRecord
    {
        internal readonly string Id, Category, State, Text, CreatedUtc, UpdatedUtc, Detail;
        internal readonly ReadOnlyCollection<CompanionJournalEvidence> Evidence;

        internal CompanionJournalRecord(IdentityRecord record)
        {
            Id = record.Id; Category = record.Category; State = record.State; Text = record.Text;
            CreatedUtc = record.CreatedUtc; UpdatedUtc = record.UpdatedUtc;
            List<CompanionJournalEvidence> evidence = new List<CompanionJournalEvidence>();
            foreach (IdentityEvidence item in record.Evidence) evidence.Add(new CompanionJournalEvidence(item));
            Evidence = evidence.AsReadOnly();
            StringBuilder detail = new StringBuilder();
            detail.Append(Text).Append("\n\nID: ").Append(Id).Append("\nCategory: ").Append(Category)
                .Append("\nState: ").Append(State).Append("\nCreated (UTC): ").Append(CreatedUtc)
                .Append("\nUpdated (UTC): ").Append(UpdatedUtc).Append("\n\n");
            if (State == "tentative") detail.Append("Tentative suggestion. It is not established until you confirm it.\n");
            else if (State == "rejected" || State == "superseded") detail.Append("Inactive record. It is excluded from conversational identity context.\n");
            else detail.Append("Reviewed by the owner. Recorded statements do not prove an experience occurred.\n");
            detail.Append("\nRetained evidence (oldest first):\n");
            foreach (CompanionJournalEvidence item in Evidence)
            {
                detail.Append("\n").Append(item.Source).Append(" at ").Append(item.Utc).Append(" (UTC)");
                if (item.Source == "model-reflection") detail.Append("\nModel suggestion; not a verified experience.");
                if (item.Source == "owner-turn") detail.Append("\nCaptured owner citation ").Append(item.TurnId)
                    .Append("; supports a suggestion without confirming it.");
                detail.Append("\n").Append(item.Text).Append("\n");
            }
            detail.Append("\nThis view shows the current record and its bounded retained evidence, not a complete edit history. Previous and recovery backups are separate.");
            Detail = detail.ToString();
        }

        public override string ToString() { return "[" + State + " " + Category + "] " + Text; }
    }

    internal sealed partial class CompanionIdentity
    {
        internal CompanionJournalSnapshot ReadJournal(int generation)
        {
            lock (gate)
            {
                bool available = Enabled && !readOnly;
                List<CompanionJournalRecord> records = new List<CompanionJournalRecord>();
                if (available)
                    foreach (IdentityRecord record in document.Records) records.Add(new CompanionJournalRecord(record));
                records.Sort(delegate(CompanionJournalRecord left, CompanionJournalRecord right)
                {
                    int order = string.Compare(right.UpdatedUtc, left.UpdatedUtc, StringComparison.Ordinal);
                    return order != 0 ? order : string.Compare(left.Id, right.Id, StringComparison.Ordinal);
                });
                return new CompanionJournalSnapshot(this, generation, document.Revision, available, Status,
                    available ? DevelopmentStatus() : "", records);
            }
        }

        internal bool JournalChanged(CompanionJournalSnapshot snapshot)
        {
            lock (gate) return snapshot == null || snapshot.Owner != this || snapshot.Revision != document.Revision
                || snapshot.Available != (Enabled && !readOnly) || snapshot.Status != Status;
        }

        internal string ReviewJournal(long expectedRevision, string action, string id, string replacement)
        {
            lock (gate)
            {
                if (!Enabled) return "Identity storage is paused by your memory/privacy settings.";
                if (readOnly) return Status;
                if (expectedRevision != document.Revision) return "Identity changed since this journal was shown. Refresh and review the current record before changing it.";
                if (action != "confirm" && action != "reject" && action != "correct" || !Regex.IsMatch(id ?? "", "^[a-f0-9]{12}$"))
                    return "Choose a journal record and a review action.";
                return HandleCommand("/identity " + action + " " + id + (action == "correct" ? ": " + replacement : ""));
            }
        }
    }
}
