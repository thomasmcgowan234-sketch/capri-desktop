#!/usr/bin/env python3
"""Validate full native-smoke evidence without builds, personal profiles or uploads."""
import importlib.util
from pathlib import Path
import tempfile

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


with tempfile.TemporaryDirectory(prefix="companion-package-checks-") as temporary:
    folder = Path(temporary)
    report = folder / "smoke-test.txt"
    valid = "\n".join("PASS synthetic check " + str(i) for i in range(1000)) + "\nLIMIT synthetic hardware unavailable\nTOTAL PASS 1000 / LIMIT 1 / FAIL 0 / SKIP 0\n"
    steps = "\n".join("PASS " + str(i) for i in range(23)) + "\n"

    def reset():
        report.write_text(valid, encoding="utf-8-sig")
        (folder / "steps-done.txt").write_text(steps)
        (folder / "sections-done.txt").write_text("CheckOcr\n")
        (folder / "progress.txt").write_text("finished passed\nsynthetic process\n")

    def rejects(mutate, description):
        reset()
        mutate()
        try:
            package.windows_smoke(report)
        except (ValueError, OSError):
            check(True, description)
        else:
            check(False, description)

    reset()
    stats, evidence = package.windows_smoke(report)
    check(stats == {"pass": 1000, "limit": 1, "fail": 0, "skip": 0} and len(evidence) == 4,
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

print("PASS", checks, "companion package validation checks")
