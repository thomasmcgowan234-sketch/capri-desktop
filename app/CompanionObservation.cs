using System;

namespace CapriDesktop
{
    // Ephemeral consent: restarting never silently resumes screen observation.
    // Screen data cannot grant this lease; only the direct input handlers call Command.
    internal static class CompanionObservation
    {
        private static readonly object gate = new object();
        private static bool cloud;
        private static DateTime until;
        internal static Func<DateTime> Now = delegate { return DateTime.UtcNow; };
        internal const string Denied = "Screen observation is off. Type /observe start for a 30-minute local session, or /observe start cloud to allow the selected remote AI provider. Type /observe stop to revoke it.";

        internal static void Configure() { lock (gate) { until = DateTime.MinValue; cloud = false; } }
        internal static bool Allowed { get { lock (gate) return Now() < until; } }
        internal static string Status
        {
            get
            {
                lock (gate) return Now() >= until ? "Screen observation is off."
                    : "Screen observation is on until " + until.ToLocalTime().ToString("HH:mm") + (cloud ? "; the selected remote AI provider may receive screenshots." : "; inference must stay on this PC.");
            }
        }
        internal static string Command(string text)
        {
            string command = (text ?? "").Trim().ToLowerInvariant();
            if (command != "/observe" && !command.StartsWith("/observe ", StringComparison.Ordinal)) return null;
            lock (gate)
            {
                if (command == "/observe stop") { until = DateTime.MinValue; cloud = false; return Status; }
                if (command == "/observe start" || command == "/observe start cloud")
                {
                    cloud = command.EndsWith(" cloud", StringComparison.Ordinal);
                    until = Now().AddMinutes(30);
                    return Status + " Stop any time with /observe stop.";
                }
                return Status + " " + Denied;
            }
        }
        internal static void RequireCapture()
        {
            if (!Allowed) throw new InvalidOperationException(Denied);
        }
        internal static void RequireInference(bool local)
        {
            lock (gate)
            {
                if (Now() >= until) throw new InvalidOperationException(Denied);
                if (!local && !cloud) throw new InvalidOperationException("This observation session allows local inference only. Type /observe start cloud to consent to screenshots reaching the selected remote AI provider.");
            }
        }
        internal static bool Loopback(string address)
        {
            Uri uri;
            return Uri.TryCreate(address, UriKind.Absolute, out uri) && (uri.Scheme == "http" || uri.Scheme == "https") && uri.IsLoopback;
        }
    }
}
