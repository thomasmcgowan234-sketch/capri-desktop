using System.IO.Compression;
using System.Text;
using Avalonia;
using CapriDesktop;

// Exercises the production Mods/SkinMods code against synthetic local packages.
// X11 provides only the PNG decoding backend: no window, provider, microphone or screen capture is opened.
internal static class AppearanceChecks
{
    private static int checks;
    private static byte[] art;
    private static readonly string root = Path.Combine(Path.GetTempPath(), "companion-appearance-" + Guid.NewGuid().ToString("N"));

    private static void Check(bool yes, string detail)
    {
        checks++;
        if (!yes) throw new Exception(detail);
    }
    private static void Refused(Action run, string detail)
    {
        try { run(); }
        catch (ModException) { checks++; return; }
        throw new Exception(detail);
    }
    private static void Entry(ZipArchive zip, string name, byte[] bytes)
    {
        using var output = zip.CreateEntry(name).Open();
        output.Write(bytes);
    }
    private static string Package(string id, string version, string imageName = "idle.png", bool corrupt = false)
    {
        string path = Path.Combine(root, Guid.NewGuid().ToString("N") + Mods.SkinExtension);
        using var zip = ZipFile.Open(path, ZipArchiveMode.Create);
        Entry(zip, "skin.json", Encoding.UTF8.GetBytes("{\"format\":1,\"id\":\"" + id + "\",\"name\":\"Synthetic appearance\",\"version\":\"" + version + "\"}"));
        Entry(zip, imageName, corrupt ? art.Take(24).ToArray() : art);
        return path;
    }
    private static Mod Find(Mods mods, string id = "fixture") => mods.All.Single(mod => mod.File.Id == id);
    private static string Profile(string name)
    {
        string path = Path.Combine(root, name);
        Directory.CreateDirectory(path);
        return path;
    }
    private static void Lifecycle()
    {
        string profile = Profile("lifecycle"), v1 = Package("fixture", "1"), v2 = Package("fixture", "2");
        Mods mods = new Mods(profile);
        Mod first = mods.Import(v1, null);
        Check(first.On && first.File.Version == "1" && !mods.CanRestoreSkin(first), "Initial appearance should be on, without invented history");
        mods.Import(Package("top", "1"), null);
        string order = string.Join(",", mods.SkinStack.Select(mod => mod.File.Id));
        mods.Update(v2, null);
        Check(Find(mods).File.Version == "2" && string.Join(",", mods.SkinStack.Select(mod => mod.File.Id)) == order, "An update changed appearance layer order");
        Check(mods.SkinVersions(Find(mods)).Length == 1 && File.ReadAllBytes(mods.SkinVersions(Find(mods))[0]).SequenceEqual(File.ReadAllBytes(v1)), "Previous package was not preserved exactly");
        mods.Set(Find(mods), false, null);
        mods.Update(Package("fixture", "3"), null);
        Check(!Find(mods).On, "Updating a disabled skin enabled it");
        Mod restored = mods.RestoreSkin(Find(mods));
        Check(!restored.On && restored.File.Version == "2", "Revert did not preserve disabled state or restore previous appearance");
        restored = mods.RestoreSkin(restored);
        Check(!restored.On && restored.File.Version == "3", "Reverting again cannot undo a revert");
        mods = new Mods(profile);
        Check(!Find(mods).On && Find(mods).File.Version == "3" && mods.CanRestoreSkin(Find(mods)), "Appearance/history/state did not survive restart");
        string kept = Find(mods).Path;
        mods.Import(kept, null);
        Check(Find(mods).On && Find(mods).File.Version == "3", "Importing the installed package did not work");
        int before = mods.SkinVersions(Find(mods)).Length;
        foreach (string bad in new[] { Package("fixture", "4", "../idle.png"), Package("fixture", "4", "unrecognised.png"), Package("fixture", "4", corrupt: true) })
            Refused(() => mods.Import(bad, null), "Invalid artwork was accepted");
        Check(Find(mods).File.Version == "3" && mods.SkinVersions(Find(mods)).Length == before, "Invalid import changed active art or history");
        Check(Directory.GetDirectories(mods.Folder, "skin-import-*").Length == 0 && Directory.GetDirectories(mods.Folder, "*-skin-transaction").Length == 0, "Import left pending temporary files");
        string history = mods.SkinVersions(Find(mods))[0];
        File.Copy(Package("different", "1"), history, true);
        Refused(() => mods.RestoreSkin(Find(mods)), "History for another identity was accepted");
        Check(Find(mods).File.Version == "3" && !mods.All.Any(mod => mod.File.Id == "different"), "Rejected history changed the installed appearances");
        Console.WriteLine("PASS appearance import, exact version archive, stacked/disabled updates, revert/undo, restart, self-import, invalid PNG/path/unknown-strip/history identity rejection");
    }
    private static void InterruptedReplacements()
    {
        for (int phase = 0; phase <= 5; phase++)
        {
            string profile = Profile("replace-phase-" + phase);
            Mods mods = new Mods(profile);
            mods.Import(Package("fixture", "old"), null);
            string folder = mods.Folder, home = Path.Combine(folder, "fixture-skin"), kept = Find(mods).Path;
            string pending = Path.Combine(folder, "fixture-skin-transaction");
            Directory.CreateDirectory(pending);
            File.WriteAllText(Path.Combine(pending, "intent"), "fixture");
            File.WriteAllText(Path.Combine(pending, "had-art"), "");
            File.WriteAllText(Path.Combine(pending, "had-package"), "");
            string newZip = Path.Combine(pending, "new" + Mods.SkinExtension), newArt = Path.Combine(pending, "new-art");
            File.Copy(Package("fixture", "new"), newZip);
            ZipFile.ExtractToDirectory(newZip, newArt);
            if (phase >= 1) Directory.Move(home, Path.Combine(pending, "old-art"));
            if (phase >= 2) File.Move(kept, Path.Combine(pending, "old" + Mods.SkinExtension));
            if (phase >= 3) Directory.Move(newArt, home);
            if (phase >= 4) File.Move(newZip, kept);
            if (phase == 5) File.WriteAllText(Path.Combine(pending, "committed"), "");
            mods = new Mods(profile);
            Check(Find(mods).File.Version == (phase == 5 ? "new" : "old") && !Directory.Exists(pending), "Replacement recovery failed at phase " + phase);
            mods = new Mods(profile);
            Check(Find(mods).File.Version == (phase == 5 ? "new" : "old"), "Recovery was not stable on second load at phase " + phase);
        }
        Console.WriteLine("PASS restart recovery at all six replacement transaction boundaries");
    }
    private static void InterruptedFirstInstall()
    {
        for (int phase = 0; phase <= 3; phase++)
        {
            string profile = Profile("first-phase-" + phase), folder = Path.Combine(profile, "mods");
            string pending = Path.Combine(folder, "fixture-skin-transaction");
            Directory.CreateDirectory(pending);
            File.WriteAllText(Path.Combine(pending, "intent"), "fixture");
            string newZip = Path.Combine(pending, "new" + Mods.SkinExtension), newArt = Path.Combine(pending, "new-art");
            File.Copy(Package("fixture", "new"), newZip);
            ZipFile.ExtractToDirectory(newZip, newArt);
            if (phase >= 1) Directory.Move(newArt, Path.Combine(folder, "fixture-skin"));
            if (phase >= 2) File.Move(newZip, Path.Combine(folder, "fixture" + Mods.SkinExtension));
            if (phase == 3) File.WriteAllText(Path.Combine(pending, "committed"), "");
            Mods mods = new Mods(profile);
            Check(mods.All.Count == (phase == 3 ? 1 : 0) && !Directory.Exists(pending), "First-install recovery failed at phase " + phase);
            Check(phase == 3 || !Directory.Exists(Path.Combine(folder, "fixture-skin")), "First-install recovery left orphaned new artwork");
        }
        Console.WriteLine("PASS restart recovery at all four first-install transaction boundaries");
    }
    private static void FilesystemFailures()
    {
        string profile = Profile("io-failure");
        Mods mods = new Mods(profile);
        mods.Import(Package("fixture", "old"), null);
        Directory.CreateDirectory(Path.Combine(mods.Folder, "appearance-history"));
        File.WriteAllText(Path.Combine(mods.Folder, "appearance-history", "fixture"), "synthetic obstruction");
        Refused(() => mods.Import(Package("fixture", "new"), null), "A blocked archive path was ignored");
        Check(Find(mods).File.Version == "old" && Directory.GetDirectories(mods.Folder, "skin-import-*").Length == 0, "I/O failure damaged installed appearance");
        // Unknown directories are never treated as permission to remove an installed skin.
        string unknown = Path.Combine(mods.Folder, "fixture-skin-transaction");
        Directory.CreateDirectory(unknown);
        mods = new Mods(profile);
        Check(Find(mods).File.Version == "old" && Directory.Exists(unknown) && mods.Problems.Count > 0, "Unrecognised transaction damaged installed art or hid its recovery warning");
        Console.WriteLine("PASS archive write failure preserves old art; unknown recovery directories remain intact with a problem report");
    }
    [STAThread]
    private static int Main()
    {
        Directory.CreateDirectory(root);
        try
        {
            NativeFontBootstrap.Initialize();
            AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
            using (var stream = typeof(Pet).Assembly.GetManifestResourceStream("Capri.Sprite.idle.png"))
            using (var memory = new MemoryStream()) { stream.CopyTo(memory); art = memory.ToArray(); }
            Lifecycle(); InterruptedReplacements(); InterruptedFirstInstall(); FilesystemFailures();
            Console.WriteLine("PASS " + checks + " appearance assertions (real production asset importer)");
            return 0;
        }
        finally { Directory.Delete(root, true); }
    }
}
