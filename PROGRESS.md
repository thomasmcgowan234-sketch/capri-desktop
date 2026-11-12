# Progress and continuation record

## 12 November 2026 — companion 0.1.0

The uploaded assignment is preserved verbatim in `PROJECT_VISION.md`. Work continues in the prepared `/workspace/capri-desktop` checkout, whose origin is the user's fork, on `companion/persistent-identity`. Original foundation: `26849b2`. No merge to main or change to the original author's repository was made. The earlier Python project was inspected read-only; no old personal data was imported.

Checkpoint `82b9415` contains identity/evidence state, data/install isolation, transactional skin history/revert, voice interruption correction and initial resource controls. Subsequent integration adds owner command routing, bounded relevant identity context in actual requests, default-denied expiring observation with separate cloud image consent, persistent quiet hours, Cross performance controls, a full-profile verified backup tool, Windows SDK cross-build and updated documentation/CI fixtures. Remaining fixed model prompts in voice, screen watches, translation and game jobs were decoupled from the original fictional persona. Temporary artwork and some wake/UI/gesture terminology remain Capri.

## Executed validation

- `source /workspace/.capri-env/activate.sh && bash ci/check-linux.sh` — **exit 0**. Existing native-surface, desktop, computer, game-session, input cancellation, native bridge, native helper/Python/upstream release/font suites pass. Historical NOT_APPLICABLE and physical UNVERIFIED records remain explicitly limited.
- New production-linked/extracted checks: **24 identity**, **38 appearance**, **27 observation**, **89 voice/performance**, **23 integrated Brain/quiet-hour/provider**, **17 full-profile backup/recovery**, plus **4 profile/update check groups** pass. No real provider/account/model/microphone is used by these synthetic suites.
- `dotnet build ci/Companion.Windows.csproj -c Release` — **exit 0**, C#5 / actual .NET Framework 4.8 target, official reference packages, **12 inherited warnings, zero errors**. Built `ci/bin/Release/net48/Capri.exe`. This is a Windows executable, not the Avalonia build; it was not launched on Windows here.
- `bash ci/build-linux.sh /workspace/work/companion-linux-v0.1.0` — **exit 0**, self-contained Linux application with pinned dependencies/notices. Four inherited Cross warnings.
- `bash /workspace/.capri-env/smoke.sh /workspace/work/companion-linux-v0.1.0/Capri` — **exit 0**, **248 PASS**, no FAIL; fresh isolated X11 profile. Evidence: `/workspace/.capri-env/smoke-Dirdgy/results`. Inspected saved Mainframe/settings and avatar images. No old smoke output was used as acceptance.
- `git diff --check` — passed.

A final Windows-only fix sets Guarded before the inherited legacy-permission migration can assign Trusted. Windows compilation was repeated successfully afterward. Disposable Windows installer tests now target the companion's paths/startup entry and verify sibling identity/settings data survives uninstall; these scripts were source-reviewed, not executed on Linux.

## Environment limitations

The shared-desktop tool failed at initialization with `the gaas-browser node_repl sandbox cannot be disabled`; status reports the selected environment running and desktop capability enabled/state unknown. The isolated X11 application smoke and saved screenshots succeeded independently. No Windows launch/install, physical speech/echo/Discord routing, live Ollama/Qwen3.5 9B, RX 7900 XTX resource measurement or dual-monitor/mixed-DPI acceptance was performed.

The designated projectless `/codex/.../output` directory is not writable in this cloud executor, even with the execution tool's escalation mode. Deliverable archives are therefore staged under the repository's ignored `out/companion-v0.1.0/` directory. Source commits remain the authoritative continuation state.

## Next work

1. Run `ci/build-windows.ps1 -OutputDirectory out/companion-windows -SmokeTest -SmokeSeconds 900` and the disposable installer tests on Windows; fix any native-only failure without weakening assertions.
2. Use a backed-up profile to accept local Ollama/tool/image behavior and microphone/speaker/Discord provenance and echo rules. Confirm the exact model tag and Radeon acceleration on the target PC.
3. Complete dual-monitor/DPI and measured gaming CPU/RAM/VRAM/frame-time checks from TESTING.md.
4. Build on reviewed identity commands for optional bounded automatic proposal extraction/reflection and a journal UI. No automatic personality development model loop is claimed in this release.
5. Review large inherited controllers incrementally; preserve working components and explicit trust/data boundaries. Do not restart or redo completed investigation.

## Saved branch and packaging

The development branch was pushed successfully to `https://github.com/thomasmcgowan234-sketch/capri-desktop/tree/companion/persistent-identity`; main is unchanged. `ci/package-companion.py` creates source/Windows/Linux archives with notices, release provenance and checksums without publishing a release. Windows PE metadata has IL-only AnyCPU flags with neither 32-bit-required nor prefer-32-bit set.

## Hosted CI follow-up

Draft PR #1 is open in the user's fork, with main unchanged. The first hosted Linux CI run (43214876843) exposed a scheduling race in the inherited game cancellation fixture: its fake model watched the parent token and could unwind/dispose the linked callback before the cancellation callback ran. The fixture now waits for the actual `CancelInference` callback, matching the real transport, and still requires prompt completion and zero input delivery. The repaired game-session suite passes locally. The fix is pushed for a complete hosted rerun; Windows/macOS jobs were still queued at inspection and are not counted as verified.

Hosted run 43215638448 passed the native Windows compiler build and both silent installer/uninstaller suites, including preserved sibling identity/settings fixtures. Its Linux job exposed a separate real native-surface cancellation race. A deterministic delayed-transport fixture failed against the old code; production now checks cancellation after compositor policy awaits and cancels hidden-session fences. The entire Linux regression script passes again with this repair. Windows runtime smoke is being observed separately; physical device/model/GPU acceptance remains unverified.

After the native-surface fix, the fresh self-contained Linux publish and isolated application smoke were repeated successfully: 248 PASS, exit 0, evidence `/workspace/.capri-env/smoke-11F8Un/results`. A Windows file-editor test now compares normalized personality text, because the editable template deliberately inserts blank lines; this preserves the exact default-content assertion without assuming its file formatting. Windows CI installer success is real execution evidence; GUI/runtime acceptance remains separate until its full job finishes.

The first complete Windows GUI smoke run executed 973 PASS and reported six stale expectation failures after deliberate persona/menu changes (translation slang, invented gamer history, template blank lines, fabricated call excuses, tray entries and old safety/prompt snapshots). Those assertions were updated to enforce the new neutral/revisable/truthful behavior, retain exact safety snapshot checking, and preserve all tests. The separate Windows package-marker and silent install/uninstall suites passed. A full native rerun follows; the development release remains labeled pending GUI acceptance until that run is green.

Hosted Linux run 43217554378 is fully green after both cancellation repairs: regression suites, build and full application smoke all passed. Its Windows silent install/uninstall job also passed. Release packaging was tightened to reject newly added uncommitted files as well as tracked edits; no personal profiles are packaged.

The second Windows runtime run reached 1,109 PASS with one remaining stale assertion requiring the old chaos-gremlin identity. It was replaced with explicit unnamed/independent/no-imposed-traits assertions. A source-linked integration check now compares the Windows persona snapshot against the actual shared production prompt and verifies its migration stamp, so future mismatches fail quickly without waiting for the long GUI suite.

The next Windows run reached 1,121 PASS with one remaining old-biography marker in the request/result routing tests. All such routing markers now use the new unnamed-identity marker, retaining positive and negative tool/persona separation checks. Identity/privacy command replies are explicitly text-only, preventing an inspection from being voiced into Discord or sent to a speech provider; the Cross application smoke exercises that with a real loopback voice-server fixture.

Current functional revision `ece0d70` passes the complete local Linux regression suite, Windows SDK cross-compilation, and a fresh self-contained Linux application smoke (248 PASS, `/workspace/.capri-env/smoke-GtfRdT/results`). Its live voice-server fixture verifies that companion management replies send no synthesis request. Hosted run 43224068947 has green Linux and native Windows installer/uninstaller jobs; the full Windows GUI job is the remaining active validation at this checkpoint.

That Windows GUI job completed with 1,191 PASS and one remaining stale default-name marker in the DeepSeek/NVIDIA request assertions. Both cloud request expectations now check the unnamed companion marker. A second read-only audit found no other default-persona expectations to update; explicit custom-personality/provider fixtures retain their chosen text. Native Windows GUI acceptance still awaits a complete green rerun; no check was removed or weakened.

Independent release review found that Windows binary packaging linked the QR encoder's MIT licence without carrying its full text and omitted the embedded font's OFL text. The full retained QR notice is now in THIRD-PARTY-NOTICES.md; native Windows builds, CI artifacts and the versioned packager carry font licence files. The committed source archive retains original headers/assets and contains no tracked personal profile.
