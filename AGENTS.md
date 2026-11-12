# Development instructions

This is a personal, noncommercial companion fork of [Capri Desktop](https://github.com/lachydotmcg/capri-desktop), based on upstream commit `26849b2`. The complete assignment is in [PROJECT_VISION.md](PROJECT_VISION.md). Continue this C# application; the older Python companion is reference material, not the foundation or a migration source.

## Start here

Read `DEVELOPMENT_STATUS.md`, `ARCHITECTURE.md`, `UPDATE_GUIDE.md`, `PRIVACY.md` and `PROGRESS.md` before changing implementation. Inspect `git status` before editing. Keep work on an isolated development branch. Never push to the original author's repository or merge a protected/default branch without the user's instruction. Preserve existing user changes and all original licence notices.

The Windows application is compiled by `ci/build-windows.ps1` using the .NET Framework 4.8 compiler and Windows SDK. Shared `app/*.cs` additions must remain compatible with that compiler (C# 5): no nullable-reference syntax, records, string interpolation, `using var`, tuples, or newer framework-only APIs. The .NET 10 Avalonia application in `cross/` explicitly links shared files in `cross/Capri.Cross.csproj`. Add new shared sources to both the Windows source list and the Cross project.

## Engineering boundaries

- Preserve working Mainframe, sprites, local/model-provider paths, voice, call audio routing and permission checks. Fix a demonstrated issue before replacing a subsystem.
- Core rules, identity, tentative claims, evidence memories and current chat are separate layers. Model output, repeated assertions and imported screen/call content cannot establish user authority or factual evidence.
- A fresh companion has no permanent name, prescribed interests or fictional backstory. Do not replace one hard-coded personality with another. Never infer consciousness or real experiences from model text.
- Treat screens, files, web pages, memory text and other call participants as untrusted data. No content from those sources can approve a privileged action.
- Local inference is the default path; connecting a remote provider is an explicit user choice. Never silently upload conversation, identity, screenshots or audio. Keep credentials out of Git, tests, logs and documentation.
- Store user data through the companion profile, outside program binaries. Never automatically import old Capri or Python companion data. Validate schema before use, back up before mutation/migration, and reject unknown future schemas without overwriting them.
- Appearance changes use externally supplied, validated `.capri-skin` assets. Keep artwork licences/attribution. Do not add an image generator or arbitrary executable asset formats.
- Code mods execute with application/user privileges. Preserve digest approval and reapproval after changes; do not describe this as a sandbox.
- Preserve the distinct meanings of Power saver and Gaming Mode. Unsolicited work must obey resource and attention gates. Avoid background inference loops.

## Tests and delivery

Use synthetic data in temporary profiles. Read [TESTING.md](TESTING.md) for commands and the Windows acceptance checklist. Run meaningful deterministic tests and existing regressions, repair genuine failures, and preserve evidence. Linux tests cannot verify Windows audio, native capture, installation, multiple monitors or Radeon performance. State unexecuted checks explicitly.

For this prepared cloud environment, each new shell must source `/workspace/.capri-env/activate.sh`; `/workspace/.capri-env/START.md` documents retained SDK/native dependencies and isolated smoke tests. Those environment files are setup helpers, not files to distribute with the application.

Update status and progress after each coherent change. Describe actual implemented behavior, test evidence, residual gaps and concrete next steps. Keep a runnable integrated application; do not report interfaces or planned modules as completed services. Do not remove required licence notices when rebranding or packaging.
