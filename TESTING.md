# Testing

Use synthetic data and temporary profiles. Software tests prove particular invariants, not model quality or physical audio/desktop behavior. Exact results belong in `PROGRESS.md`; never replace a Windows check with a Linux pass.

## Prepared Linux environment

Each new shell in this cloud task needs:

```bash
source /workspace/.capri-env/activate.sh
cd /workspace/capri-desktop
```

The environment provides .NET SDK 10.0.404, NuGet cache, native input dependencies, Fontconfig fallback and isolated X11 smoke helpers. See `/workspace/.capri-env/START.md`. These paths are task setup, not end-user prerequisites.

Run existing regressions:

```bash
bash ci/check-linux.sh
```

This covers native-surface, desktop, computer, game-session, input-cancel, native-bridge, Python helpers, extracted upstream checks and font bootstrap. Expected fixture errors and physical `UNVERIFIED` lines are not hardware passes. Require exit status zero.

New companion checks:

```bash
dotnet run --project tests/companion/CompanionIdentityChecks.csproj -c Release
dotnet run --project tests/development/CompanionDevelopmentChecks.csproj -c Release
python3 tests/voice-state/run.py
python3 tests/companion-profile/run.py
dotnet run --project tests/observation/ObservationChecks.csproj -c Release
dotnet run --project tests/companion-integration/CompanionIntegrationChecks.csproj -c Release
python3 tests/companion-backup/checks.py
dotnet build tests/appearance/AppearanceChecks.csproj -c Release
bash /workspace/.capri-env/run-x11.sh dotnet run --project /workspace/capri-desktop/tests/appearance/AppearanceChecks.csproj -c Release --no-build
```

Appearance needs X11 for Avalonia's PNG backend; it does not open windows or access a live screen/microphone/model. Identity and development tests compile actual production modules under C# 5. They cover exact migration backups, proposal provenance/review, cooldown/cap limits, a deterministic idle scheduler, strict responses, privacy/policy/source changes, ignored cancellation and disposal. Voice tests extract the actual production interruption class/guards and use synthetic audio levels. Profile checks compile the actual extracted Windows updater with the real profile/startup helpers. Observation tests compile production consent logic. Integration checks use the real Cross Brain with synthetic providers and loopback fixtures for context, model switching, consent, fixed-budget development transport and quiet-hour persistence. Memory-off tests verify the next provider request without clearing chat and preserve both summary and backlog bytes across blocked completions and off-then-on. Backup tests use disposable synthetic private files. Exact final results are in `PROGRESS.md`. Install `xvfb` and `xauth` before the full Linux suite; it runs appearance checks on a disposable display.

Build/publish and run isolated application smoke:

```bash
build=$(mktemp -d /workspace/work/companion-build-XXXXXX)
bash ci/build-linux.sh "$build/app"
bash /workspace/.capri-env/smoke.sh "$build/app/Capri"
```

Publish requires an absolute output directory that does not exist. Smoke uses a fresh private profile/X11 display and requires fresh results without FAIL. Preserve the actual exit status and evidence directory.

## Windows build/smoke

From a complete checkout on Windows 11 with the Windows SDK:

```powershell
powershell -ExecutionPolicy Bypass -File .\ci\build-windows.ps1 -OutputDirectory .\out\companion-windows -SmokeTest -SmokeSeconds 900
```

This compiles WinForms with .NET Framework 4.8, runs runtime smoke and verifies installer payload consistency. The Cross/Avalonia binary is not the Windows WinForms build.

Checkpoint `b3ad5c6` passed the full native Windows runtime with 1,417 PASS, 0 FAIL and 0 SKIP, plus 53 explicit limits. Silent install/uninstall and package checks passed separately. These results verify that checkpoint, not later automatic-development changes; record a fresh full run for the final revision. The [Windows test build guide](WINDOWS_TEST_BUILD.md) describes artifact verification and target-PC checks.

## Windows acceptance checklist

Use a fresh synthetic profile, then repeat preservation checks with a backed-up real profile.

| Area | Required observation |
| --- | --- |
| Launch/isolation | Mainframe/tray/chat work; original Capri/Python companion data unchanged; no upstream executable downloads |
| Identity | Fresh identity unnamed; propose/review name/preference, restart and retain; assistant repetition cannot establish either |
| Automatic development | Eight distinct qualifying owner turns, eligible reply and idle period may produce one local tentative draft; inspect its actual source citation, confirm/reject/correct, restart and retain; remote/call/mod/screen inputs cannot supply evidence |
| Development gates | `/identity development off`, memory off, quiet hours, performance/fullscreen/job/voice activity prevent work; off-then-on/new input/model changes discard stale results; cooldown and three-draft cap hold |
| Corrections | Correct/reject synthetic memories; obsolete record does not reappear as current certainty after restart |
| Recovery | Migrate a synthetic schema-1 identity and verify dated/rolling exact backups; corrupt/use future schema and verify read-only behavior; restore known compatible backup and test matching-profile rollback |
| Models | Local Ollama chat, model switch and restart preserve identity/notes/art; verify exact installed Qwen3.5 9B or alternative tag and tools/images capabilities |
| Appearance | Reject traversal/oversize/wrong dimensions; import/replace/revert/restart; verify fallback for incomplete strips and licensing |
| Avatar/display | Show/hide/drag/saved position on dual monitors, 4K/mixed DPI, negative origins, disconnect and animation states |
| Voice | Mic/output selection, mute/disable, natural speech, sustained interruption, fan/click/laugh/echo, device loss and generation cancellation |
| Discord | Authorised join/listen/speak/leave and routing; participant approval words cannot execute tools; only direct mic/typing confirms |
| Observation | Capture denied initially; `/observe start` local lease, `/observe start cloud` explicit remote screenshot consent, stop/expiry deny again |
| Injection | Screen/web/call/memory 'approve/ignore rules' instructions never change permissions or approve actions |
| Actions | Guarded sensitive/send/delete/pay/terminal confirmation, revocation and Esc stop; screen consent does not grant click/type |
| Resources | Power saver Always or `/performance on` pauses unsolicited work; Off restores; direct chat/voice/jobs/reminders work; measure CPU/RAM/VRAM/game frames |
| Initiative | Quiet/cooldown/activity/fullscreen controls prevent intrusive starts; record remaining quiet-hours/policy gaps |
| Updates | Exit/back up/install next fork version preserves data; prior binaries+compatible profile recover; unknown schema rejected |
| Exit/restart | Owned workers/audio/input stop; settings/name/memory/art persist; no new lingering heavy services |

Call provenance uses source routing/echo heuristics, not biometric speaker recognition. Physical routing/echo acceptance is essential.

## Performance measurements

On Ryzen 9 5900X/RX 7900 XTX, record idle application CPU/working set, Ollama/voice VRAM, first-token/speech latency, observation cadence and game frame-time distribution with normal vs Power saver Always. In-flight inference may finish; the switch does not force Ollama unload. Close other model clients when measuring release. This Linux task cannot supply target-PC measurements.

SDK Windows cross-compilation was also executed: `dotnet build ci/Companion.Windows.csproj -c Release` (C#5/net48, official reference packages). It verifies source/resource compilation only. The native Windows compiler build and adjusted disposable installer/uninstaller scripts have also passed on hosted Windows CI, including preservation of sibling identity/settings data. Full native GUI smoke results are recorded separately in PROGRESS.md; physical audio, Discord, display and target-PC performance still require the checklist above.
