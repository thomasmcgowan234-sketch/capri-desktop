# Releases, backups and recovery

Companion release 0.1.0 is a separately maintained Capri fork. Its binary informational version is `0.8.9-companion.0.1.0`; the assembly compatibility version retains Capri 0.8.9. Do not install an official Capri executable over it. Production upstream update check/download/install paths are blocked; Check updates points to the manual companion process. This release does not implement a signed automatic fork updater.

## Data and application directories

| Platform | Companion data | Replaceable application |
| --- | --- | --- |
| Windows | `%LOCALAPPDATA%\CapriCompanion\data` | `%LOCALAPPDATA%\CapriCompanion\app` for the fork installer; development builds can run from their build directory |
| Linux/macOS | `Environment.SpecialFolder.ApplicationData/CapriCompanion` (typically `~/.config/CapriCompanion` on Linux) | Extracted release/build directory |

Original Capri and previous Python companion profiles are not automatically read/copied. Keep those installations intact. Selected Obsidian vault notes and externally configured model/voice locations need separate backups. Ollama manages its own model files.

The companion data directory can contain settings, account-bound encrypted Windows secrets or private Cross secret files, notes, transcripts, identity state/backups, mods and appearance history. Never publish a real profile with a release or commit it to Git.

## Build and install on Windows

1. Install the Windows 10/11 SDK and use Windows 11's .NET Framework 4.8. Obtain this branch's complete source, assets and licence files.
2. In PowerShell from the repository root, run:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\ci\build-windows.ps1 -OutputDirectory .\out\companion-windows -SmokeTest -SmokeSeconds 900
   ```

3. Inspect smoke results. Run `Capri.exe` for development or this fork's `Capri-Setup.exe` for installation. Existing filenames/some shell labels remain Capri for compatibility; they do not set the character's chosen identity.
4. Configure Ollama/local model in Mainframe. Check `ollama list` and the exact installed tag. Cloud brain/voice configuration is optional; read [PRIVACY.md](PRIVACY.md).

A second build route cross-compiles the actual Windows WinForms application with an installed .NET SDK and official NuGet reference packages:

```bash
dotnet build ci/Companion.Windows.csproj -c Release
```

Output is `ci/bin/Release/net48/Capri.exe`. Copy it as `Capri-Setup.exe` for the inherited filename-selected installer, retaining the configuration and required notices. The original native PowerShell build also carries those resources and remains the Windows acceptance route. No SDK runtime/reference-pack DLLs are shipped as substitute Windows components.

The Windows executable/installer must be validated on Windows. The privacy-repair checkpoint has passed native hosted CI; each later build needs its own evidence. A Linux build cannot establish Windows acceptance. [WINDOWS_TEST_BUILD.md](WINDOWS_TEST_BUILD.md) explains testing the packaged Windows artifact on your PC.

## Update

1. Exit the companion and wait for owned workers to stop.
2. Copy the entire companion data directory to a dated private backup, including any separately stored vault notes/custom assets. A rolling identity backup is not a full-profile backup.
3. Keep the previous working release and source commit. Compare the new artifact's SHA-256 against a checksum from a trusted release channel; a checksum beside a compromised executable is not an independent signature.
4. Install/extract the new companion build into its program directory. Do not overwrite data or use upstream Capri's updater/installer.
5. Restart and check identity, memories, settings, providers and skin; complete applicable [acceptance checks](TESTING.md).

Settings keep backward-compatible fields. Identity schema 2 adds automatic-development policy, cooldown and proposal provenance. Loading a valid schema-1 file first preserves its exact bytes as `identity.schema1-<UTC>-<GUID>.json`, then atomically upgrades it; the rolling `identity.previous.json` also retains the displaced schema-1 file. Migration does not import old Capri/Python data or establish model proposals. A fresh/upgraded companion enables local automatic development; `/identity development off` pauses it. Invalid/future records fail read-only without replacement. Future transformations must retain the same backup discipline. Arbitrary downgrades across schemas are not promised.

## Rollback and recovery

- **Program regression:** exit and restore the prior release. Restore its matching backed-up profile if a new schema was introduced; preserve the failed profile separately. Never let an older program rewrite an unknown schema.
- **Identity:** inspect `/identity`; use correction/rejection/restore commands. `identity.previous.json` is one prior valid atomic snapshot. `/identity restore` preserves displaced state as `identity.recovery-*.json`; a valid schema-1 backup is upgraded safely when restored by this release. For rollback to a schema-1 program, close the app and restore its matching full profile or preserved raw `identity.schema1-*.json` as `identity.json`; retain the schema-2 profile separately. Corrupt/future schema fails read-only. Forgotten records can remain in backups, which need separate deliberate deletion for erasure.
- **Appearance:** Mods offers Revert when history exists. It validates/reinstalls the most recent archive and retains the displaced package. Import any older known good `.capri-skin` archive for a specific historical version. Keep original artwork licences.
- **Profile:** close the app, preserve current data elsewhere, then restore the complete dated backup. Windows DPAPI secrets are account-bound; a different machine/account may need keys entered again.

The Windows installer preserves the displaced executable as `app/Capri.exe.previous`; this is a convenience recovery copy, not a complete profile/version backup. Uninstall preserves sibling `data`; inspect it deliberately when deciding whether to erase personal information.

## Selected upstream fixes and releases

In this fork, fetch the original as a source remote, inspect selected commits, and cherry-pick/merge on an integration branch. Resolve conflicts around identity/context, screen gates, skin transactions, profile paths, updater guards and build lists deliberately. Run regressions and Windows acceptance before tagging. Do not re-enable upstream executable updates.

Use companion tags such as `companion-v0.1.0`, record source commit/toolchain, and ship `LICENSE.md`, `THIRD-PARTY-NOTICES.md` and dependency notices. Capri's PolyForm Noncommercial licence/Required Notices still apply, including the anonymous artist and other asset credits.

## Offline full-profile backup tool

With the application closed, Python 3.9+ can make a private archive, verify it, and restore it into a **new** directory. These commands perform no network requests:

```powershell
python .\tools\companion_backup.py backup --profile "$env:LOCALAPPDATA\CapriCompanion\data" --destination "D:\CompanionBackups\before-update.zip"
python .\tools\companion_backup.py verify "D:\CompanionBackups\before-update.zip"
python .\tools\companion_backup.py restore "D:\CompanionBackups\before-update.zip" --destination "$env:LOCALAPPDATA\CapriCompanion\recovered-data"
```

The destination archive must not already exist. Backup records per-file SHA-256, verifies captured bytes and publishes without overwriting an older backup. Restore checks version, paths, sizes, duplicates and hashes, re-verifies staged files, and refuses existing target profiles. To recover the active profile, close the app, rename/preserve its current `data` directory, then move verified `recovered-data` into its place. Backups contain credentials and personal records in plaintext; store them privately. Symlinked/external vaults are deliberately excluded and must be backed up separately. Windows DPAPI keys remain bound to the original account.

## Package an accepted source snapshot

With clean committed source and completed builds, run:

```bash
python3 ci/package-companion.py --windows ci/bin/Release/net48/Capri.exe --linux /absolute/linux-publish-directory --output /absolute/new-release-directory
```

The optional Linux argument can be omitted. To include native runtime evidence, also pass `--windows-smoke /absolute/path/to/smoke/runtime/smoke-test.txt` from the complete successful native run, retaining its sibling completion markers. The packager rejects failed/incomplete reports and records their totals and hashes alongside the executable hash. Without that argument it labels the Windows binary unvalidated.

The script packages the Windows app/installer, required notices and guides, committed source ZIP, optional Linux runtime, release provenance and SHA-256 sums. CI retains the original binary artifact and adds a complete `companion-test-build` artifact only after successful native checks. Packaging performs no release publication. Checksums are integrity evidence, not a code-signing certificate; CI smoke evidence and target-PC/hardware acceptance remain separate manifest fields.
