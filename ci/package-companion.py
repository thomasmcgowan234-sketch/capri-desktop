#!/usr/bin/env python3
"""Package reviewed companion builds, notices and clean committed source; no upload."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import tarfile
import zipfile

ROOT = Path(__file__).resolve().parents[1]
VERSION = "0.1.0"
DOCS = ["README.md", "LICENSE.md", "THIRD-PARTY-NOTICES.md", "UPDATE_GUIDE.md", "WINDOWS_TEST_BUILD.md", "TESTING.md", "PRIVACY.md", "SECURITY.md", "DEVELOPMENT_STATUS.md", "APPEARANCE_GUIDE.md", "ARCHITECTURE.md", "AGENTS.md", "PROJECT_VISION.md", "PROGRESS.md", "docs/UPSTREAM_README.md", "docs/UPSTREAM_SECURITY.md", "mods/MODDING.md"]


def git(*args):
    return subprocess.check_output(["git", *args], cwd=ROOT, text=True).strip()


def notices(destination):
    for name in DOCS + ["tools/companion_backup.py"]:
        target = destination / name
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(ROOT / name, target)
    for name in ["OFL-ShareTechMono.txt", "Barlow-OFL.txt"]:
        target = destination / "licenses" / name
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(ROOT / "assets" / "fonts" / name, target)


def sha256(path):
    result = hashlib.sha256()
    with path.open("rb") as source:
        for block in iter(lambda: source.read(1024 * 1024), b""):
            result.update(block)
    return result.hexdigest()


def windows_smoke(report):
    """Validate the complete native run and its completion markers, not a log excerpt."""
    if report.stat().st_size > 4 * 1024 * 1024:
        raise ValueError("Windows smoke report exceeds the size limit")
    lines = report.read_text(encoding="utf-8-sig").splitlines()
    lines = [line for line in lines if line.strip()]
    totals = [line for line in lines if line.startswith("TOTAL ")]
    if len(totals) != 1 or not lines or totals[0] != lines[-1]:
        raise ValueError("Windows smoke report has no unique final total")
    total = re.fullmatch(r"TOTAL PASS (\d+) / LIMIT (\d+) / FAIL (\d+) / SKIP (\d+)", totals[0])
    if total is None:
        raise ValueError("Windows smoke report has an invalid total")
    stats = dict(zip(("pass", "limit", "fail", "skip"), map(int, total.groups())))
    for label, count in stats.items():
        if sum(line.startswith(label.upper() + " ") for line in lines) != count:
            raise ValueError("Windows smoke report totals do not match its checks")
    if stats["pass"] < 1000 or stats["fail"] or stats["skip"]:
        raise ValueError("Windows smoke report is failed, skipped or smaller than the full native suite")

    # Filtered --only/--steps/--no-steps runs are useful checks, but are not full acceptance.
    steps = report.with_name("steps-done.txt")
    completed = steps.read_text(encoding="utf-8-sig").splitlines()
    expected = {"PASS " + str(step) for step in range(23)}
    if len(completed) != len(expected) or set(completed) != expected:
        raise ValueError("Windows smoke report is missing successful GUI steps")
    sections = report.with_name("sections-done.txt")
    if "CheckOcr" not in sections.read_text(encoding="utf-8-sig").splitlines():
        raise ValueError("Windows smoke report is missing the unfiltered sections")
    progress = report.with_name("progress.txt")
    if progress.read_text(encoding="utf-8-sig").splitlines()[0:1] != ["finished passed"]:
        raise ValueError("Windows smoke run did not finish successfully")
    return stats, [(report, "windows-smoke-test.txt"), (steps, "windows-smoke-steps.txt"),
                   (sections, "windows-smoke-sections.txt"), (progress, "windows-smoke-progress.txt")]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--windows", required=True, type=Path, help="compiled Windows Capri.exe")
    parser.add_argument("--linux", type=Path, help="optional self-contained Linux publish directory")
    parser.add_argument("--windows-smoke", type=Path, help="full native smoke-test.txt with sibling completion markers")
    parser.add_argument("--windows-smoke-run-url", help="GitHub Actions run that produced the native smoke report")
    parser.add_argument("--output", required=True, type=Path, help="new output directory")
    args = parser.parse_args()
    if git("status", "--porcelain", "--untracked-files=normal"):
        parser.error("Commit source changes and new files before packaging")
    executable = args.windows.resolve(strict=True)
    with executable.open("rb") as binary:
        if binary.read(2) != b"MZ":
            parser.error("Windows input is not a PE executable")
    if args.windows_smoke_run_url and not args.windows_smoke:
        parser.error("A smoke run URL requires --windows-smoke")
    if args.windows_smoke_run_url and not re.fullmatch(r"https://github\.com/[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+/actions/runs/\d+", args.windows_smoke_run_url):
        parser.error("Windows smoke run URL must identify a GitHub Actions run")
    stats, evidence = None, []
    if args.windows_smoke:
        try:
            stats, evidence = windows_smoke(args.windows_smoke.resolve(strict=True))
        except (OSError, UnicodeError, ValueError) as error:
            parser.error("Windows smoke validation failed: " + str(error))
    if args.output.exists():
        parser.error("Choose a new output directory")
    output = args.output.absolute()
    output.mkdir(parents=True)
    source_commit = git("rev-parse", "HEAD")
    info = {"companion_version": VERSION, "foundation_version": "0.8.9", "source_commit": source_commit,
            "branch": git("branch", "--show-current"), "windows_executable_sha256": sha256(executable),
            "windows_runtime_accepted": False, "windows_runtime_acceptance_scope": "Manual target-PC acceptance; automated smoke evidence is separate",
            "hardware_accepted": False, "windows_smoke_passed": stats is not None,
            "windows_ci_smoke_passed": stats is not None and bool(args.windows_smoke_run_url),
            "windows_smoke_stats": stats, "windows_smoke_source_run_url": args.windows_smoke_run_url,
            "windows_smoke_report_sha256": sha256(args.windows_smoke) if stats is not None else None,
            "validation_document": "PROGRESS.md"}
    windows = output / "windows"
    windows.mkdir()
    for name in ["Capri.exe", "Capri-Setup.exe"]:
        shutil.copy2(executable, windows / name)
        config = executable.with_name(executable.name + ".config")
        if config.exists():
            shutil.copy2(config, windows / (name + ".config"))
    notices(windows)
    for source, name in evidence:
        for destination in [output / "validation", windows / "validation"]:
            destination.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, destination / name)
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
    artifacts += sorted((output / "validation").glob("*"))
    hashes = {file.relative_to(output).as_posix(): sha256(file) for file in artifacts}
    (output / "RELEASE.json").write_text(json.dumps(dict(info, artifacts=hashes), indent=2) + "\n")
    checksums = dict(hashes, **{"RELEASE.json": sha256(output / "RELEASE.json")})
    (output / "SHA256SUMS").write_text("".join(value + "  " + name + "\n" for name, value in checksums.items()))
    print(json.dumps({"output": str(output), "source_commit": source_commit, "artifacts": [{"name": file.relative_to(output).as_posix(), "bytes": file.stat().st_size} for file in artifacts]}, indent=2))


if __name__ == "__main__":
    main()
