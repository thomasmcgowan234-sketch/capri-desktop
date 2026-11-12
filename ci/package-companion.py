#!/usr/bin/env python3
"""Package reviewed companion builds, notices and clean committed source; no upload."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import tarfile
import zipfile

ROOT = Path(__file__).resolve().parents[1]
VERSION = "0.1.0"
DOCS = ["README.md", "LICENSE.md", "THIRD-PARTY-NOTICES.md", "UPDATE_GUIDE.md", "TESTING.md", "PRIVACY.md", "SECURITY.md", "DEVELOPMENT_STATUS.md", "APPEARANCE_GUIDE.md", "ARCHITECTURE.md", "AGENTS.md", "PROJECT_VISION.md", "PROGRESS.md", "docs/UPSTREAM_README.md", "docs/UPSTREAM_SECURITY.md", "mods/MODDING.md"]


def git(*args):
    return subprocess.check_output(["git", *args], cwd=ROOT, text=True).strip()


def notices(destination):
    for name in DOCS + ["tools/companion_backup.py"]:
        target = destination / name
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(ROOT / name, target)


def sha256(path):
    result = hashlib.sha256()
    with path.open("rb") as source:
        for block in iter(lambda: source.read(1024 * 1024), b""):
            result.update(block)
    return result.hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--windows", required=True, type=Path, help="compiled Windows Capri.exe")
    parser.add_argument("--linux", type=Path, help="optional self-contained Linux publish directory")
    parser.add_argument("--output", required=True, type=Path, help="new output directory")
    args = parser.parse_args()
    if git("status", "--porcelain", "--untracked-files=no"):
        parser.error("Commit tracked source changes before packaging")
    executable = args.windows.resolve(strict=True)
    with executable.open("rb") as binary:
        if binary.read(2) != b"MZ":
            parser.error("Windows input is not a PE executable")
    if args.output.exists():
        parser.error("Choose a new output directory")
    output = args.output.absolute()
    output.mkdir(parents=True)
    source_commit = git("rev-parse", "HEAD")
    info = {"companion_version": VERSION, "foundation_version": "0.8.9", "source_commit": source_commit, "branch": git("branch", "--show-current"), "windows_runtime_accepted": False, "hardware_accepted": False, "validation_document": "PROGRESS.md"}
    windows = output / "windows"
    windows.mkdir()
    for name in ["Capri.exe", "Capri-Setup.exe"]:
        shutil.copy2(executable, windows / name)
        config = executable.with_name(executable.name + ".config")
        if config.exists():
            shutil.copy2(config, windows / (name + ".config"))
    notices(windows)
    (windows / "RELEASE.json").write_text(json.dumps(info, indent=2) + "\n")
    win_zip = output / ("companion-v" + VERSION + "-windows.zip")
    with zipfile.ZipFile(win_zip, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as archive:
        for file in sorted(windows.rglob("*")):
            if file.is_file():
                archive.write(file, "companion-v" + VERSION + "/" + file.relative_to(windows).as_posix())
    with zipfile.ZipFile(win_zip) as archive:
        if archive.testzip() is not None:
            raise ValueError("Windows package failed CRC validation")
    if args.linux:
        linux = args.linux.resolve(strict=True)
        if not (linux / "Capri").is_file():
            parser.error("Linux directory has no Capri executable")
        notices(linux)
        (linux / "RELEASE.json").write_text(json.dumps(info, indent=2) + "\n")
        with tarfile.open(output / ("companion-v" + VERSION + "-linux-x64.tar.gz"), "w:gz") as archive:
            archive.add(linux, arcname="companion-v" + VERSION)
    source_zip = output / ("companion-v" + VERSION + "-source.zip")
    subprocess.run(["git", "archive", "--format=zip", "--prefix=companion-v" + VERSION + "-source/", "--output=" + str(source_zip), "HEAD"], cwd=ROOT, check=True)
    artifacts = [file for file in sorted(output.iterdir()) if file.is_file()]
    hashes = {file.name: sha256(file) for file in artifacts}
    (output / "SHA256SUMS").write_text("".join(value + "  " + name + "\n" for name, value in hashes.items()))
    (output / "RELEASE.json").write_text(json.dumps(dict(info, artifacts=hashes), indent=2) + "\n")
    print(json.dumps({"output": str(output), "source_commit": source_commit, "artifacts": [{"name": file.name, "bytes": file.stat().st_size} for file in artifacts]}, indent=2))


if __name__ == "__main__":
    main()
