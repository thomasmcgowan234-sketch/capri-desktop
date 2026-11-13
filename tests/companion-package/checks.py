#!/usr/bin/env python3
"""Validate full native-smoke evidence without builds, personal profiles or uploads."""
import importlib.util
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("companion_package", ROOT / "ci/package-companion.py")
package = importlib.util.module_from_spec(spec)
spec.loader.exec_module(package)
checks = 0


def check(value, description):
    global checks
    if not value:
        raise AssertionError(description)
    checks += 1
    print("PASS", description)


def write_receipt(report, executable, commit, run_url=None):
    receipt = {
        "schema": 1,
        "source_commit": commit,
        "windows_executable_sha256": package.sha256(executable),
        "reports_sha256": {name: package.sha256(report.with_name(name)) for name in
                           ["smoke-test.txt", "steps-done.txt", "sections-done.txt", "progress.txt"]},
        "windows_smoke_source_run_url": run_url,
    }
    report.with_name(package.SMOKE_RECEIPT).write_text(json.dumps(receipt))


# PowerShell execution belongs to native CI; keep the stale-evidence launch ordering
# checked on Linux as well, where a stopped/no-op runtime cannot be reproduced natively.
build_script = (ROOT / "ci/build-windows.ps1").read_text()
cleanup = re.search(r"foreach \(\$name in @\(([^\n]+)\)\) \{\n    \$oldEvidence = Join-Path \$runtime \$name\n    if \(Test-Path -LiteralPath \$oldEvidence\) \{ Remove-Item -LiteralPath \$oldEvidence -Force \}\n\}", build_script)
check(cleanup is not None and set(re.findall(r"'([^']+)'", cleanup.group(1))) ==
      {"smoke-test.txt", "steps-done.txt", "sections-done.txt", "progress.txt", "failure.txt"}
      and cleanup.end() < build_script.index("$check = Start-Process")
      and build_script.index("Runtime smoke test wrote no results.") < build_script.index("$verify = Start-Process"),
      "native script removes only prior completion outputs before launch and rejects a fresh run with no report")


with tempfile.TemporaryDirectory(prefix="companion-package-checks-") as temporary:
    folder = Path(temporary)
    report = folder / "smoke-test.txt"
    executable = folder / "Capri.exe"
    commit = "a" * 40
    run_url = "https://github.com/synthetic/companion/actions/runs/123"
    valid = "\n".join("PASS synthetic check " + str(i) for i in range(1000)) + "\nLIMIT synthetic hardware unavailable\nTOTAL PASS 1000 / LIMIT 1 / FAIL 0 / SKIP 0\n"
    steps = "\n".join("PASS " + str(i) for i in range(23)) + "\n"

    def reset():
        executable.write_bytes(b"MZ synthetic executable A")
        report.write_text(valid, encoding="utf-8-sig")
        (folder / "steps-done.txt").write_text(steps)
        (folder / "sections-done.txt").write_text("CheckOcr\n")
        (folder / "progress.txt").write_text("finished passed\nsynthetic process\n")
        write_receipt(report, executable, commit, run_url)

    def change_receipt(field, value):
        path = folder / package.SMOKE_RECEIPT
        receipt = json.loads(path.read_text())
        receipt[field] = value
        path.write_text(json.dumps(receipt))

    def rejects(mutate, description):
        reset()
        mutate()
        try:
            package.windows_smoke(report, executable, commit, run_url)
        except (ValueError, OSError):
            check(True, description)
        else:
            check(False, description)

    reset()
    stats, evidence, executable_hash = package.windows_smoke(report, executable, commit, run_url)
    check(stats == {"pass": 1000, "limit": 1, "fail": 0, "skip": 0} and len(evidence) == 5
          and executable_hash == package.sha256(executable),
          "complete unfiltered native report with BOM retains explicit hardware limits")
    rejects(lambda: report.write_text(valid.replace("TOTAL PASS 1000", "TOTAL PASS 999")), "mismatched pass total is rejected")
    rejects(lambda: report.write_text(valid.replace("LIMIT 1", "LIMIT 0")), "mismatched limitation total is rejected")
    rejects(lambda: report.write_text(valid.replace("TOTAL PASS", "FAIL synthetic failure\nTOTAL PASS").replace("FAIL 0", "FAIL 1")), "failed native report cannot mark a package validated")
    rejects(lambda: report.write_text(valid.replace("TOTAL PASS", "SKIP synthetic dependency\nTOTAL PASS").replace("SKIP 0", "SKIP 1")), "skipped native stages cannot mark a package validated")
    rejects(lambda: report.write_text("PASS filtered check\nTOTAL PASS 1 / LIMIT 0 / FAIL 0 / SKIP 0\n"), "small filtered report is rejected")
    rejects(lambda: report.write_text(valid + "TOTAL PASS 1000 / LIMIT 1 / FAIL 0 / SKIP 0\n"), "duplicate final total is rejected")
    rejects(lambda: report.write_text(valid.split("TOTAL PASS")[0]), "missing final total is rejected")
    rejects(lambda: (folder / "steps-done.txt").write_text(steps.replace("PASS 22\n", "")), "missing final GUI stage is rejected")
    rejects(lambda: (folder / "steps-done.txt").write_text(steps + "PASS 22\n"), "duplicate GUI completion cannot replace full evidence")
    rejects(lambda: (folder / "steps-done.txt").write_text(steps.replace("PASS 12", "FAIL 12")), "failed GUI completion is rejected")
    rejects(lambda: (folder / "sections-done.txt").write_text("FilteredSection\n"), "filtered section selection is rejected")
    rejects(lambda: (folder / "progress.txt").write_text("running step 22\n"), "unfinished native run is rejected")
    rejects(lambda: (folder / "progress.txt").write_text("finished failed\n"), "failed completion marker is rejected")
    rejects(lambda: (folder / "steps-done.txt").unlink(), "missing completion evidence is rejected")
    rejects(lambda: (folder / package.SMOKE_RECEIPT).unlink(), "legacy unbound reports cannot validate a newly packaged executable")
    rejects(lambda: executable.write_bytes(b"MZ synthetic executable B"), "old passing evidence cannot validate a changed executable")
    rejects(lambda: change_receipt("source_commit", "b" * 40), "old passing evidence cannot validate a different source commit")
    rejects(lambda: report.write_text(valid.replace("synthetic check 0\n", "different check 0\n")), "edited report with unchanged passing totals is rejected")
    rejects(lambda: (folder / "steps-done.txt").write_text("\n".join(reversed(steps.splitlines())) + "\n"), "changed but otherwise complete GUI markers are rejected")
    rejects(lambda: (folder / "sections-done.txt").write_text("CheckOcr\nAnotherSection\n"), "changed section evidence is rejected")
    rejects(lambda: (folder / "progress.txt").write_text("finished passed\ndifferent process\n"), "changed completion process evidence is rejected")
    rejects(lambda: change_receipt("schema", 2), "future native receipt schema is rejected")
    rejects(lambda: change_receipt("schema", True), "boolean is not accepted as a receipt schema number")
    rejects(lambda: (folder / package.SMOKE_RECEIPT).write_text("{"), "truncated native receipt is rejected")
    rejects(lambda: change_receipt("reports_sha256", {}), "receipt must bind every report")
    rejects(lambda: change_receipt("windows_smoke_source_run_url", "https://example.org/run/123"), "invalid receipt CI location is rejected")
    rejects(lambda: change_receipt("windows_smoke_source_run_url", run_url + "4"), "a different valid CI run URL cannot be attached to evidence")
    rejects(lambda: change_receipt("windows_smoke_source_run_url", None), "local smoke cannot be relabelled as CI by supplying a run URL")

    reset()
    write_receipt(report, executable, commit)
    stats, evidence, _ = package.windows_smoke(report, executable, commit)
    check(stats["pass"] == 1000 and len(evidence) == 5, "matching local native receipt remains supported without a CI claim")

    # Exercise the actual CLI, Git clean-source guard, archives and manifest together.
    repository = folder / "repository"
    repository.mkdir()
    (repository / "ci").mkdir()
    shutil.copy2(ROOT / "ci/package-companion.py", repository / "ci/package-companion.py")
    for name in package.DOCS + ["tools/companion_backup.py", "assets/fonts/OFL-ShareTechMono.txt", "assets/fonts/Barlow-OFL.txt"]:
        target = repository / name
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text("Synthetic packaging fixture; no personal data.\n")
    (repository / ".gitignore").write_text("out/\n")

    def fixture_git(*args):
        return subprocess.check_output(["git", "-C", str(repository), *args], text=True).strip()

    fixture_git("init", "-q")
    fixture_git("add", ".")
    fixture_git("-c", "user.name=Synthetic Package Test", "-c", "user.email=package-test@example.invalid", "commit", "-qm", "synthetic package fixture")
    fixture_commit = fixture_git("rev-parse", "HEAD")
    runtime = repository / "out/runtime"
    runtime.mkdir(parents=True)
    fixture_report = runtime / "smoke-test.txt"
    fixture_executable = repository / "out/Capri.exe"
    fixture_executable.write_bytes(b"MZ synthetic package executable A")
    for name in ["smoke-test.txt", "steps-done.txt", "sections-done.txt", "progress.txt"]:
        shutil.copy2(folder / name, runtime / name)
    write_receipt(fixture_report, fixture_executable, fixture_commit, run_url)

    def cli(name, smoke=False, url=None):
        output = repository / "out" / name
        args = [sys.executable, str(repository / "ci/package-companion.py"),
                "--windows", str(fixture_executable), "--output", str(output)]
        if smoke:
            args += ["--windows-smoke", str(fixture_report)]
        if url:
            args += ["--windows-smoke-run-url", url]
        result = subprocess.run(args, capture_output=True, text=True)
        return result, output

    result, output = cli("validated", True, run_url)
    check(result.returncode == 0, "actual CLI packages matching native source/executable/evidence")
    manifest = json.loads((output / "RELEASE.json").read_text())
    check(manifest["windows_ci_smoke_passed"] and manifest["windows_smoke_passed"]
          and not manifest["windows_runtime_accepted"] and not manifest["hardware_accepted"]
          and manifest["source_commit"] == fixture_commit,
          "manifest distinguishes bound automated CI evidence from target-PC acceptance")
    with zipfile.ZipFile(output / ("companion-v" + package.VERSION + "-windows.zip")) as archive:
        prefix = "companion-v" + package.VERSION + "/"
        check(archive.read(prefix + "Capri.exe") == fixture_executable.read_bytes()
              and archive.read(prefix + "Capri-Setup.exe") == fixture_executable.read_bytes()
              and archive.read(prefix + "validation/windows-smoke-receipt.json") == (runtime / package.SMOKE_RECEIPT).read_bytes(),
              "Windows archive carries the exact tested app, installer and binding receipt")
    check(manifest["artifacts"]["validation/windows-smoke-receipt.json"] == package.sha256(runtime / package.SMOKE_RECEIPT),
          "outer artifact manifest retains the validation receipt digest")
    write_receipt(fixture_report, fixture_executable, fixture_commit)
    result, output = cli("local-native", True)
    check(result.returncode == 0, "actual CLI accepts matching local native evidence without a CI URL")
    manifest = json.loads((output / "RELEASE.json").read_text())
    check(manifest["windows_smoke_passed"] and not manifest["windows_ci_smoke_passed"],
          "local native validation remains separate from CI validation in the manifest")
    write_receipt(fixture_report, fixture_executable, "b" * 40, run_url)
    result, output = cli("other-source", True, run_url)
    check(result.returncode != 0 and "different source" in result.stderr and not output.exists(),
          "CLI rejects evidence for other committed source before creating output")
    write_receipt(fixture_report, fixture_executable, fixture_commit, run_url)
    fixture_executable.write_bytes(b"MZ synthetic package executable B")
    result, output = cli("stale", True, run_url)
    check(result.returncode != 0 and "different executable" in result.stderr and not output.exists(),
          "CLI rejects stale reports before creating a falsely validated package")
    result, output = cli("cross-build")
    check(result.returncode == 0, "cross-compiled/no-smoke package remains supported")
    manifest = json.loads((output / "RELEASE.json").read_text())
    check(not manifest["windows_ci_smoke_passed"] and not manifest["windows_smoke_passed"]
          and manifest["windows_smoke_receipt_sha256"] is None and not manifest["windows_runtime_accepted"],
          "no-smoke package stays explicitly unvalidated even beside older native evidence")

print("PASS", checks, "companion package validation checks")
