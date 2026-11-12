# Windows development test build

Use the **companion-test-build** artifact from a successful CI run for the intended source revision. It contains the Windows ZIP, committed source ZIP, `RELEASE.json`, `SHA256SUMS` and the synthetic Windows validation report. This guide describes tests to perform on your PC; it is not evidence that your audio devices, local model or hardware have passed.

`windows_ci_smoke_passed` records validated automated evidence. `windows_runtime_accepted` and `hardware_accepted` remain false until manual target-PC acceptance is recorded. The manifest records the actual checked-out Git commit, which may be a pull-request merge commit. These are development builds; no code-signing certificate is claimed.

## Before starting

1. Exit every companion instance. Keep the current working executable and its complete matching data backup.
2. If `%LOCALAPPDATA%\CapriCompanion\data` already exists, copy the entire directory to a dated private backup **outside** the replaceable `app` directory. Include external Obsidian vaults separately. The supplied `tools/companion_backup.py` provides optional verified backup/restore with Python 3.9+; [UPDATE_GUIDE.md](UPDATE_GUIDE.md) gives commands.
3. Compare the downloaded Windows ZIP's SHA-256 with its entry in `SHA256SUMS` from that CI run:

   ```powershell
   Get-FileHash .\companion-v0.1.0-windows.zip -Algorithm SHA256
   ```

   Checksums detect changed bytes; they are not a code signature or an independent guarantee of a download's origin.
4. Extract the Windows ZIP into a new folder. Keep the notices, guides, `licenses` and `validation` directories with it. Read `PROGRESS.md` and `RELEASE.json` for that build's actual tests and limitations.

For a fresh synthetic test profile, preserve the backup and rename the existing `data` directory while the application is closed. The application creates a new one. Do not merge profile contents or overwrite a real profile with a synthetic one.

## Launch and configure

Run `Capri.exe` first for a portable development session. It uses `%LOCALAPPDATA%\CapriCompanion\data`; **portable does not mean a separate profile beside the executable**. `Capri-Setup.exe` installs this fork under `%LOCALAPPDATA%\CapriCompanion\app` and uses the same sibling `data` directory. Original Capri and the earlier Python companion are not automatically imported or changed.

The app uses Windows 11's .NET Framework 4.8; the Windows package is the native WinForms application. Configure the microphone, output device and speech controls in Mainframe. Keep the microphone muted or speech disabled while checking text-only behavior if desired.

For local inference, start Ollama and run `ollama list` to find the exact installed model tag. Select the local instance/model in Mainframe. Qwen3.5 9B, its image/tool capabilities and RX 7900 XTX acceleration must be verified on your PC; no model or Radeon benchmark is inferred from a compile or synthetic test. Cloud brain and voice providers are optional and can receive contextual text when selected. [PRIVACY.md](PRIVACY.md) lists the actual external-service paths.

Screen capture starts denied. Leave it off for initial chat tests. `/observe start` grants a 30-minute local observation lease; `/observe start cloud` separately permits screenshots to the selected remote provider. `/observe stop`, expiry or restart revokes the lease. Observation permission does not grant mouse/keyboard actions.

## Initial checks

Use ordinary synthetic topics and inspect the results rather than using private credentials or real destructive actions.

| Check | Expected behavior |
| --- | --- |
| Launch and UI | Mainframe, tray, show/hide, drag, microphone/speaker controls and clean exit work. Existing Capri installations and data remain intact. |
| Identity continuity | `/identity` starts unnamed in a fresh profile. Propose/review a name, restart, and verify that the reviewed name and records persist. Changing the model preserves them. |
| Automatic development | For a build recording this feature as validated in PROGRESS.md, use `/identity development status` and `/identity development on`. Have a normal conversation containing at least eight distinct short owner messages, then leave the app idle for about 20 seconds. With a local model and policy gates clear, at most one reflection attempt may produce a tentative proposal. An attempt can validly produce none; it is not retried in a loop. |
| Development limits | Attempts have a 30-minute cooldown and at most three pending drafts. Quiet hours, Power saver, fullscreen activity, active work/calls/voice and memory-off suppress automatic inference. Remote/signed-in providers do not run automatic reflection. `/identity development off` disables it. |
| Review and revision | Inspect the proposal ID, model origin and cited owner statement. `/identity confirm ID` establishes a reviewed choice; `/identity reject ID` excludes it. Correct an appropriate record with `/identity correct ID: text`. Assistant repetition does not establish evidence or permissions. |
| Memory privacy | Switch off Remember our chats. New transcript recording, summary/transcript retrieval and background folding stop. Off then on cannot revive an older fold result. Existing files, editable `me.md` notes and current chat are separate; the switch does not erase them. Restart and inspect synthetic saved files when checking recording. |
| Resource controls | Power saver → Always or `/performance on` pauses unsolicited work; `/performance off` restores normal operation. Direct chat remains usable. In-flight inference may finish and Ollama is not forcibly unloaded. Gaming Mode retains its separate game-interaction meaning. |
| Observation | Verify capture is denied initially. Start a local lease only when wanted, discuss actually supplied screen content, stop it, and verify later capture is denied again. |
| Appearance | Existing licensed artwork is temporary. A supplied compatible `.capri-skin` can replace it; replacement retains history and Mods → Revert restores the preceding version. A single portrait is not a complete animated skin. |
| Restart and rollback | Exit normally, restart and check settings, identity, memory and appearance. Before downgrading across data schemas, restore the matching backed-up profile as described in UPDATE_GUIDE.md. |

Automatic-development steps above are acceptance instructions for candidate builds, not a claim that every local model will return a valid useful proposal. Inspect development status and pending records; do not interpret an invented experience as verified memory.

## Target-PC acceptance still required

Complete the detailed checklist in [TESTING.md](TESTING.md): physical speech and interruptions, fan/click/echo rejection, microphone/output changes, authorised Discord routing and call participant boundaries, revocation and injection checks, two monitors with 4K/mixed DPI, and measured gaming CPU/RAM/VRAM/frame times. Hosted synthetic checks cannot replace these observations.

Identity, notes, transcripts and backups are local plaintext. Windows provider secrets use account-bound DPAPI. Approved code mods execute with your user privileges; they are not sandboxed. Keep real profiles, keys and private transcripts out of Git and shared test reports. Preserve the failed profile privately before recovery; keep original notices and licences with copied builds.
