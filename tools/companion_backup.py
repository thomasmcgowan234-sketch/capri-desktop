#!/usr/bin/env python3
"""Private, offline profile backup and verified restore into a NEW directory."""
import argparse
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import shutil
import stat
import tempfile
import uuid
import zipfile
from datetime import datetime, timezone

MANIFEST = "COMPANION_BACKUP.json"
LIMIT = 16 * 1024**3
MAX_FILES = 100000


def digest(stream):
    result = hashlib.sha256()
    for block in iter(lambda: stream.read(1024 * 1024), b""):
        result.update(block)
    return result.hexdigest()


def safe_name(name):
    path = PurePosixPath(name)
    if not name or "\\" in name or ":" in name or path.is_absolute() or any(p in ("", ".", "..") for p in name.split("/")):
        raise ValueError("Unsafe archive path")
    return path


def backup(profile, destination):
    profile, destination = Path(profile).resolve(strict=True), Path(destination).absolute()
    if not profile.is_dir() or destination.exists():
        raise ValueError("Profile must be a directory and backup destination must not exist")
    if destination.resolve().is_relative_to(profile):
        raise ValueError("Save backups outside the profile")
    destination.parent.mkdir(parents=True, exist_ok=True)
    files = sorted(profile.rglob("*"))
    if any(p.is_symlink() for p in files):
        raise ValueError("Symlinks are not included; back up external vaults separately")
    files = [p for p in files if p.is_file()]
    if len(files) > MAX_FILES or sum(p.stat().st_size for p in files) > LIMIT:
        raise ValueError("Profile exceeds backup limits")
    handle, temporary = tempfile.mkstemp(prefix=".companion-backup-", suffix=".zip", dir=destination.parent)
    try:
        os.close(handle)
        manifest = {"format": 1, "created_utc": datetime.now(timezone.utc).isoformat(), "files": {}}
        with zipfile.ZipFile(temporary, "w", zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
            for file in files:
                name = file.relative_to(profile).as_posix()
                safe_name(name)
                if name == MANIFEST:
                    raise ValueError("Profile contains the reserved backup manifest name")
                before = file.stat()
                with file.open("rb") as source:
                    checksum = digest(source)
                archive.write(file, name)
                after = file.stat()
                if (before.st_size, before.st_mtime_ns) != (after.st_size, after.st_mtime_ns):
                    raise ValueError("Profile changed while backing up; exit the app and retry")
                manifest["files"][name] = {"size": after.st_size, "sha256": checksum}
            archive.writestr(MANIFEST, json.dumps(manifest, indent=2))
        # Verify captured bytes too: an active writer might change content without changing size.
        verify(temporary)
        with open(temporary, "r+b") as finished:
            os.fsync(finished.fileno())
        os.chmod(temporary, 0o600)
        # Atomic publication on the same filesystem, without replacing prior backups.
        os.link(temporary, destination)
    finally:
        Path(temporary).unlink(missing_ok=True)
    return destination


def verify(file):
    with zipfile.ZipFile(file) as archive:
        entries = archive.infolist()
        names = [entry.filename for entry in entries]
        if len(names) != len(set(names)) or len(entries) > MAX_FILES + 1 or sum(e.file_size for e in entries) > LIMIT:
            raise ValueError("Duplicate entries or archive exceeds limits")
        for entry in entries:
            safe_name(entry.filename)
            if entry.is_dir() or stat.S_ISLNK(entry.external_attr >> 16):
                raise ValueError("Unexpected directory or symlink entry")
        info = archive.getinfo(MANIFEST)
        if info.file_size > 32 * 1024**2:
            raise ValueError("Backup manifest is too large")
        manifest = json.loads(archive.read(MANIFEST))
        if manifest.get("format") != 1 or not isinstance(manifest.get("files"), dict) or set(manifest["files"]) != set(names) - {MANIFEST}:
            raise ValueError("Invalid or unsupported backup manifest")
        for name, expected in manifest["files"].items():
            if not isinstance(expected, dict) or archive.getinfo(name).file_size != expected.get("size"):
                raise ValueError("Backup size mismatch")
            with archive.open(name) as source:
                if digest(source) != expected.get("sha256"):
                    raise ValueError("Backup checksum mismatch")
        return manifest


def restore(file, destination):
    destination = Path(destination).absolute()
    if destination.exists() or destination.is_symlink():
        raise ValueError("Restore requires a NEW directory; preserve or rename the existing profile first")
    # Each extracted file is checked again before the staged profile is published.
    manifest = verify(file)
    destination.parent.mkdir(parents=True, exist_ok=True)
    staging = destination.parent / (".companion-restore-" + uuid.uuid4().hex)
    staging.mkdir(mode=0o700)
    try:
        with zipfile.ZipFile(file) as archive:
            for name, expected in manifest["files"].items():
                target = staging.joinpath(*safe_name(name).parts)
                target.parent.mkdir(parents=True, exist_ok=True, mode=0o700)
                with archive.open(name) as source, target.open("xb") as output:
                    shutil.copyfileobj(source, output)
                os.chmod(target, 0o600)
                with target.open("rb") as restored:
                    if target.stat().st_size != expected["size"] or digest(restored) != expected["sha256"]:
                        raise ValueError("Restored data failed verification")
        if destination.exists():
            raise ValueError("Restore target appeared during restore; refusing to overwrite it")
        staging.rename(destination)
    finally:
        if staging.exists():
            shutil.rmtree(staging)
    return destination


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    save = commands.add_parser("backup", help="exit the app before backing up")
    save.add_argument("--profile", required=True, type=Path)
    save.add_argument("--destination", required=True, type=Path)
    check = commands.add_parser("verify")
    check.add_argument("archive", type=Path)
    load = commands.add_parser("restore", help="restore into a new directory only")
    load.add_argument("archive", type=Path)
    load.add_argument("--destination", required=True, type=Path)
    args = parser.parse_args()
    try:
        if args.command == "backup": print(backup(args.profile, args.destination))
        elif args.command == "verify": print("Verified", len(verify(args.archive)["files"]), "files")
        else: print(restore(args.archive, args.destination))
        return 0
    except (OSError, ValueError, KeyError, zipfile.BadZipFile) as error:
        parser.exit(1, "Backup operation failed: " + str(error) + "\n")


if __name__ == "__main__":
    raise SystemExit(main())
