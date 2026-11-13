using System;
using CapriDesktop;

internal static class Program
{
    private static int passed;
    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        passed++;
        Console.WriteLine("PASS " + description);
    }
    private static bool Denied(Action action)
    {
        try { action(); return false; }
        catch (InvalidOperationException) { return true; }
    }
    public static int Main()
    {
        Func<DateTime> realClock = CompanionObservation.Now;
        DateTime now = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        CompanionObservation.Now = delegate { return now; };
        try
        {
            Check(!CompanionObservation.Allowed && Denied(CompanionObservation.RequireCapture)
                && Denied(delegate { CompanionObservation.RequireInference(true); })
                && Denied(delegate { CompanionObservation.RequireInference(false); }), "observation is denied before any desktop configuration");
            CompanionObservation.Command("/observe stop");
            Check(!CompanionObservation.Allowed && Denied(CompanionObservation.RequireCapture), "revocation before configuration cannot accidentally grant access");
            CompanionObservation.Configure();
            Check(!CompanionObservation.Allowed && CompanionObservation.Status == "Screen observation is off.", "configuration starts with no remembered observation lease");
            Check(CompanionObservation.Command("hello") == null && CompanionObservation.Command("/observer start") == null && !CompanionObservation.Allowed, "unrelated text cannot be interpreted as observation permission");
            string local = CompanionObservation.Command("/observe start");
            Check(CompanionObservation.Allowed && local.Contains("inference must stay on this PC") && !Denied(CompanionObservation.RequireCapture)
                && !Denied(delegate { CompanionObservation.RequireInference(true); }), "explicit local observation allows capture and local inference");
            Check(Denied(delegate { CompanionObservation.RequireInference(false); }), "local consent never authorizes remote screenshot inference");
            now = now.AddMinutes(30).AddTicks(-1);
            Check(CompanionObservation.Allowed && !Denied(CompanionObservation.RequireCapture), "lease remains valid immediately before its expiry boundary");
            now = now.AddTicks(1);
            Check(!CompanionObservation.Allowed && Denied(CompanionObservation.RequireCapture)
                && Denied(delegate { CompanionObservation.RequireInference(true); })
                && Denied(delegate { CompanionObservation.RequireInference(false); }), "capture and every inference route are denied at the exact expiry boundary");
            string cloud = CompanionObservation.Command(" /OBSERVE START CLOUD ");
            Check(CompanionObservation.Allowed && cloud.Contains("remote AI provider may receive screenshots")
                && !Denied(delegate { CompanionObservation.RequireInference(false); }), "separate explicit cloud lease discloses remote transmission and permits it");
            CompanionObservation.Command("/observe start");
            Check(Denied(delegate { CompanionObservation.RequireInference(false); }) && !Denied(delegate { CompanionObservation.RequireInference(true); }), "starting local session revokes a prior cloud allowance");
            CompanionObservation.Command("/observe start cloud");
            CompanionObservation.Command("/observe stop");
            Check(!CompanionObservation.Allowed && Denied(CompanionObservation.RequireCapture)
                && Denied(delegate { CompanionObservation.RequireInference(false); }), "stop revokes capture and remote inference immediately");
            string invalid = CompanionObservation.Command("/observe start cloud extra");
            Check(invalid != null && invalid.Contains("Screen observation is off") && !CompanionObservation.Allowed, "unknown observation subcommands cannot grant a lease");
            CompanionObservation.Command("/observe start cloud");
            CompanionObservation.Configure();
            Check(!CompanionObservation.Allowed && Denied(delegate { CompanionObservation.RequireInference(false); }), "desktop reconfiguration never resumes a previous cloud observation session");
            string[] localEndpoints = { "http://127.0.0.1:11434", "https://localhost:8080/v1", "http://[::1]:1234", "http://127.0.0.2" };
            foreach (string endpoint in localEndpoints) Check(CompanionObservation.Loopback(endpoint), "loopback endpoint recognized: " + endpoint);
            string[] otherEndpoints = { null, "", "localhost:11434", "https://api.example.com", "http://192.168.1.3:11434", "http://0.0.0.0:11434", "https://localhost.example.com", "https://localhost@api.example.com", "file://localhost/screen.png", "ftp://127.0.0.1" };
            foreach (string endpoint in otherEndpoints) Check(!CompanionObservation.Loopback(endpoint), "non-loopback or unsupported endpoint rejected: " + (endpoint ?? "null"));
            Console.WriteLine("PASS " + passed + " observation permission checks");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine("FAIL " + error); return 1; }
        finally { CompanionObservation.Now = realClock; CompanionObservation.Configure(); }
    }
}
