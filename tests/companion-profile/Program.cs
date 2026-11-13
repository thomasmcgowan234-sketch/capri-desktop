using System;
using System.IO;

namespace CapriDesktop
{
    internal sealed class BrainException : Exception
    {
        internal BrainException(string message) : base(message) { }
    }

    // UI/OS adapters only. All update download/install decisions are extracted
    // from the production app; no network or native code is invoked here.
    internal static class Packaged
    {
        internal static bool Is;
        internal const string Store = "the Microsoft Store";
    }

    internal static class Companion
    {
        internal static Release FetchLatestRelease() { throw new Exception("Unexpected release request."); }
        internal static bool ReleaseVersion(string tag, out Version version)
        {
            return Version.TryParse((tag ?? "").TrimStart('v', 'V'), out version);
        }
    }

    internal static class Program
    {
        private static void Must(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        private static void Blocked(Action action)
        {
            try { action(); }
            catch (BrainException error)
            {
                Must(error.Message == CompanionProfile.UpdateMessage, "The updater did not explain its fork policy.");
                return;
            }
            throw new Exception("An official update was allowed in the companion fork.");
        }

        private static int Main()
        {
            string root = Path.Combine(Path.GetTempPath(), "companion-profile-check-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(root);
                string legacy = Path.Combine(root, "Capri");
                Directory.CreateDirectory(legacy);
                File.WriteAllText(Path.Combine(legacy, "settings.json"), "private legacy sentinel");
                string data = CompanionProfile.WindowsData(root);
                string app = CompanionProfile.WindowsInstall(root);
                Must(data == Path.Combine(root, "CapriCompanion", "data"), "Windows data path changed.");
                Must(app == Path.Combine(root, "CapriCompanion", "app"), "Windows program path changed.");
                Must(!data.StartsWith(app + Path.DirectorySeparatorChar, StringComparison.Ordinal), "Data lives below replaceable binaries.");
                Must(CompanionProfile.CrossData(root) == Path.Combine(root, "CapriCompanion"), "Cross-platform data is not isolated.");
                Must(!Directory.Exists(data) && File.ReadAllText(Path.Combine(legacy, "settings.json")) == "private legacy sentinel", "Path selection imported or changed legacy data.");
                Must(Startup.Name == "CapriCompanion", "Startup overwrites the official Capri entry.");
                Console.WriteLine("PASS separate program/data roots and startup name; no implicit legacy import");

                Directory.CreateDirectory(app);
                File.WriteAllText(Path.Combine(app, ".capri-install"), "synthetic companion install");
                File.WriteAllText(Path.Combine(legacy, ".capri-install"), "synthetic upstream install");
                Must(SetupFiles.Closable(app), "The companion installer cannot close its own folder.");
                Must(!SetupFiles.Closable(legacy) && !SetupFiles.Closable(root) && !SetupFiles.Closable(null), "Installer claims another program's folder.");
                Console.WriteLine("PASS privileged installer close is scoped to the marked companion app folder");

                var updater = new Updater(data, Path.Combine(app, "Capri.exe"), false);
                int starts = 0;
                updater.Run = _ => starts++;
                // Both invalid and realistic cached files must be blocked before
                // a network/file/signature operation or process start occurs.
                Blocked(() => updater.Download(null));
                Blocked(() => updater.Install(null, 1, 1));
                string cached = Path.Combine(root, "cached.exe");
                File.WriteAllText(cached, "synthetic cached upstream installer");
                Blocked(() => updater.Download(new Release { Tag = "v99.0.0", Assets = new[] { new ReleaseAsset { Name = "Capri-Setup.exe", Size = 12, Url = "https://github.com/lachydotmcg/capri-desktop/releases/download/v99/Capri-Setup.exe" } } }));
                Blocked(() => updater.Install(new UpdateFile { Path = cached, Size = new FileInfo(cached).Length }, 1, 1));
                Must(starts == 0 && !Directory.Exists(data), "The blocked update created files or started a process.");
                Console.WriteLine("PASS production official download/install reject before network, file creation or process start");

                var smoke = new Updater(data, cached, true);
                try { smoke.Download(null); throw new Exception("Invalid fixture release accepted."); }
                catch (BrainException error) { Must(error.Message == "This release can't be installed.", "Smoke mode stopped exercising the original validator."); }
                Console.WriteLine("PASS injected smoke mode retains the original release validation");
                return 0;
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        }
    }
}
