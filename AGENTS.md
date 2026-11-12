# Development instructions

This is a personal, noncommercial companion fork of [Capri Desktop](https://github.com/lachydotmcg/capri-desktop), based on upstream commit `26849b2`. The complete assignment is in [PROJECT_VISION.md](PROJECT_VISION.md). Continue this C# application; the older Python companion is reference material, not the foundation or a migration source.

## Start here

Read `DEVELOPMENT_STATUS.md`, `ARCHITECTURE.md`, `UPDATE_GUIDE.md`, `PRIVACY.md` and `PROGRESS.md` before changing implementation. Inspect `git status` before editing. Keep work on an isolated development branch. Never push to the original author's repository or merge a protected/default branch without the user's instruction. Preserve existing user changes and all original licence notices.

The Windows application is compiled by `ci/build-windows.ps1` using the .NET Framework 4.8 compiler and Windows SDK. Shared `app/*.cs` additions must remain compatible with that compiler (C# 5): no nullable-reference syntax, records, string interpolation, `using var`, tuples, or newer framework-only APIs. The .NET 10 Avalonia application in `cross/` explicitly links shared files in `cross/Capri.Cross.csproj`. Add new shared sources to both the Windows source list and the Cross project.

## Engineering boundaries

- Preserve working Mainframe, sprites, local/model-provider paths, voice, call audio routing and permission checks. Fix a demonstrated issue before replacing a subsystem.
- Core rules, identity, tentative claims, evidence memories and current chat are separate layers. Model output, repeated assertions and imported screen/call content cannot establish user authority or factual evidence.
- Automatic development may collect only authenticated owner turns at the Windows/Cross input gates. Never feed general inference, screenshots, files, transcripts, call participants or assistant assertions into its evidence buffer. Keep `model-reflection` and captured `owner-turn` provenance distinct; only direct owner review establishes a draft.
- A fresh companion has no permanent name, prescribed interests or fictional backstory. Do not replace one hard-coded personality with another. Never infer consciousness or real experiences from model text.
- Treat screens, files, web pages, memory text and other call participants as untrusted data. No content from those sources can approve a privileged action.
- Local inference is the default path; connecting a remote provider is an explicit user choice. Never silently upload conversation, identity, screenshots or audio. Keep credentials out of Git, tests, logs and documentation.
- Store user data through the companion profile, outside program binaries. Never automatically import old Capri or Python companion data. Validate schema before use, back up before mutation/migration, and reject unknown future schemas without overwriting them.
- Identity schema 2 upgrades valid schema 1 files with dated raw-byte and rolling backups. Preserve migration/recovery idempotence, external-edit detection and provenance when changing the schema or correction commands.
- Appearance changes use externally supplied, validated `.capri-skin` assets. Keep artwork licences/attribution. Do not add an image generator or arbitrary executable asset formats.
- Code mods execute with application/user privileges. Preserve digest approval and reapproval after changes; do not describe this as a sandbox.
- Preserve the distinct meanings of Power saver and Gaming Mode. Unsolicited work must obey resource and attention gates. Avoid background inference loops.
- Keep automatic reflection local-only and event-driven: eight distinct short owner turns, one idle timer, persisted 30-minute cooldown, at most three pending drafts and one fixed 240-token/30-second request without retries. Recheck privacy generation, source/model, request token, identity revision and platform policy before saving. Memory-off must clear its buffer and invalidate old results even after re-enabling; never hold UI/transport locks across cancellation.

## Tests and delivery

Use synthetic data in temporary profiles. Read [TESTING.md](TESTING.md) for commands and the Windows acceptance checklist. Run meaningful deterministic tests and existing regressions, repair genuine failures, and preserve evidence. Linux tests cannot verify Windows audio, native capture, installation, multiple monitors or Radeon performance. State unexecuted checks explicitly.

For identity/development changes, run `tests/companion/CompanionIdentityChecks.csproj`, `tests/development/CompanionDevelopmentChecks.csproj` and `tests/companion-integration/CompanionIntegrationChecks.csproj` before full application validation. Keep the blocked-provider memory revocation tests and real loopback transport checks: a stale completion must neither save private state nor consume queued transcript lines. Compilation and synthetic checks do not establish target-PC model, audio or gaming acceptance.

CI runs on pull requests, main pushes and manual dispatch. Keep one PR run per development revision; duplicate push/PR full Windows runs are unnecessary. A branch without a PR can use manual dispatch.

For this prepared cloud environment, each new shell must source `/workspace/.capri-env/activate.sh`; `/workspace/.capri-env/START.md` documents retained SDK/native dependencies and isolated smoke tests. Those environment files are setup helpers, not files to distribute with the application.

Update status and progress after each coherent change. Describe actual implemented behavior, test evidence, residual gaps and concrete next steps. Keep a runnable integrated application; do not report interfaces or planned modules as completed services. Do not remove required licence notices when rebranding or packaging.
