#!/usr/bin/env python3
"""Exercise real offline backup/restore with disposable synthetic private data."""
import importlib.util
import json
from pathlib import Path
import tempfile
import zipfile

root = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("companion_backup", root / "tools/companion_backup.py")
backup = importlib.util.module_from_spec(spec)
spec.loader.exec_module(backup)
checks = 0

def check(value, name):
    global checks
    assert value, name
    checks += 1
    print("PASS", name)

def rejects(action, name):
    try:
        action()
    except (OSError, ValueError, KeyError, zipfile.BadZipFile):
        check(True, name)
    else:
        raise AssertionError(name)

with tempfile.TemporaryDirectory(prefix="companion-backup-fixture-") as folder:
    temp = Path(folder)
    profile = temp / "profile"
    (profile / "companion").mkdir(parents=True)
    (profile / "companion/identity.json").write_text('{"SchemaVersion":1,"Name":"Synthetic"}')
    (profile / "settings.json").write_text('{"Volume":70}')
    (profile / "private-key.bin").write_bytes(b"SYNTHETIC-NOT-A-REAL-SECRET")
    archive = backup.backup(profile, temp / "backup.zip")
    info = backup.verify(archive)
    check(len(info["files"]) == 3, "backup contains every synthetic profile file with checksums")
    check(archive.stat().st_mode & 0o077 == 0, "private archive has no group/other access on POSIX")
    restored = backup.restore(archive, temp / "restored")
    check(all((restored / n).read_bytes() == (profile / n).read_bytes() for n in info["files"]), "verified restore reproduces identity, settings and private bytes")
    check(not list(temp.glob('.companion-*')), "successful backup and restore leave no staging artifacts")
    previous = archive.read_bytes()
    rejects(lambda: backup.backup(profile, archive), "backup refuses to overwrite an existing archive")
    check(archive.read_bytes() == previous, "rejected backup leaves original archive unchanged")
    rejects(lambda: backup.restore(archive, profile), "restore refuses a live/existing profile")
    check((profile / "private-key.bin").read_bytes() == b"SYNTHETIC-NOT-A-REAL-SECRET", "rejected restore preserves original private data")
    rejects(lambda: backup.backup(profile, profile / "recursive.zip"), "backup refuses recursive archive inside its input profile")
    (profile / "outside-link").symlink_to(temp / "backup.zip")
    rejects(lambda: backup.backup(profile, temp / "linked.zip"), "backup rejects symlinks instead of following external data")
    (profile / "outside-link").unlink()
    for label, name, content, manifest in [
        ("traversal", "../escaped", b"bad", {"format": 1, "files": {}}),
        ("digest", "private-key.bin", b"tampered", {"format": 1, "files": {"private-key.bin": {"size": 8, "sha256": "bad"}}}),
        ("future", "synthetic", b"data", {"format": 2, "files": {"synthetic": {"size": 4, "sha256": "bad"}}}),
    ]:
        corrupted = temp / (label + ".zip")
        with zipfile.ZipFile(corrupted, "w") as z:
            z.writestr(name, content)
            z.writestr(backup.MANIFEST, json.dumps(manifest))
        target = temp / (label + "-restore")
        rejects(lambda: backup.restore(corrupted, target), "restore rejects " + label + " archive")
        check(not target.exists() and not (temp / "escaped").exists(), "failed " + label + " restore publishes no partial profile or escaped file")
    check(not list(temp.glob('.companion-*')), "rejected operations leave no staging artifacts")
print("PASS", checks, "backup/recovery checks")
