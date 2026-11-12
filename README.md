# Personal AI Companion

A personal, noncommercial companion fork of [Capri Desktop](https://github.com/lachydotmcg/capri-desktop), release **companion 0.1.0**, based on Capri 0.8.9. It keeps Capri's Mainframe, desktop avatar, local-model support, speech, voice-call routing and computer tools while adding an initially unnamed character, reviewed persistent identity, artwork history and fork-safe data/update boundaries.

This is a development build. Windows cross-compilation and Linux regression/smoke tests do not establish physical Windows audio, Discord or Radeon acceptance. See [DEVELOPMENT_STATUS.md](DEVELOPMENT_STATUS.md) for precise remaining work and [PROGRESS.md](PROGRESS.md) for executed validation.

## Start on Windows

Extract the companion Windows release with its notices. Run `Capri.exe` for a portable development session or `Capri-Setup.exe` to install this fork. It uses `%LOCALAPPDATA%\CapriCompanion\data`; installed binaries use the separate `CapriCompanion\app` directory. Original Capri and the previous companion's data are not imported or changed automatically.

In Mainframe, select your Ollama instance and an installed model. Verify the exact model tag using `ollama list`; Qwen3.5 9B, vision/tools and RX 7900 XTX acceleration still need testing on your PC. Cloud brain/voice options are optional and transmit contextual data when selected. Read [PRIVACY.md](PRIVACY.md).

The existing show/hide, microphone, speaker, model, security, gaming and exit controls remain. **Power saver → Always** or `/performance on` pauses background inference and unsolicited interaction while direct conversation stays available. Gaming Mode retains its separate game-interaction meaning.

## Character and privacy commands

Commands work in the typing box without a model. Only direct user input can change these settings; screen content, models, mods and call participants cannot approve them.

| Command | Result |
| --- | --- |
| `/companion help` | Show available companion controls |
| `/identity` | Inspect identity records, IDs and available commands |
| `/identity name Rowan` | Create a tentative name proposal; no permanent name is imposed |
| `/identity propose interest: astronomy \| we discussed telescopes` | Record a tentative interest with attributed evidence |
| `/identity confirm ID` | Establish a reviewed proposal |
| `/identity correct ID: corrected text` | Correct a record with a saved prior snapshot |
| `/identity reject ID` / `/identity forget ID` | Exclude a record from current context; backups may still contain it |
| `/identity remember FACT` | Save an explicit long-term memory |
| `/identity restore` | Restore the previous valid snapshot and retain displaced state |
| `/observe start` | Grant a 30-minute observation session; image inference stays on loopback/local paths |
| `/observe start cloud` | Explicitly permit screenshots to the selected remote AI provider for 30 minutes |
| `/observe stop` | Revoke screen observation; restart also revokes it |
| `/quiet hours 22:00-08:00` | Pause independent activity during these hours in your PC's local time |
| `/quiet hours off` / `/quiet` | Disable or inspect quiet hours |
| `/performance on` / `/performance off` | Toggle low-resource behavior; available on Windows and the Cross preview |

The existing memory/privacy switch also pauses structured identity retrieval and edits. Inherited notes, transcripts and backups are separate; edit/erase them separately when needed. The character can propose names/preferences naturally, but this release uses explicit review to make them durable. There is no background personality-training loop or guarantee that model-generated opinions represent real experiences. Some inherited wake phrases and gesture dialogue still use Capri terminology.

## Appearance

The licensed Capri art is temporary. No image generator is embedded. Discuss a redesign, approve its direction, generate artwork externally and import a validated `.capri-skin` through Mods. Replacements preserve prior packages; **Revert** restores the preceding appearance. [APPEARANCE_GUIDE.md](APPEARANCE_GUIDE.md) explains sprite strips, transparency and the external artwork workflow.

## Build, maintain and recover

- [UPDATE_GUIDE.md](UPDATE_GUIDE.md): builds, manual updates, checksums and backup/restore commands. Upstream executable replacement is blocked.
- [TESTING.md](TESTING.md): regressions and Windows acceptance.
- [ARCHITECTURE.md](ARCHITECTURE.md), [AGENTS.md](AGENTS.md), [PROJECT_VISION.md](PROJECT_VISION.md): maintainers, boundaries and complete original requirements.
- [SECURITY.md](SECURITY.md), [PRIVACY.md](PRIVACY.md): permissions, external services, credentials and full-trust code mods.

A reproducible SDK-based Windows cross-build is available with `dotnet build ci/Companion.Windows.csproj -c Release`. The native Windows script is `ci/build-windows.ps1`; run it with `-SmokeTest` on Windows before treating a release as accepted. The Avalonia/Linux preview is built with `ci/build-linux.sh`.

## Licence and attribution

Required Notice: Copyright 2026 Lachy (https://github.com/lachydotmcg)

Required Notice: Anyone who uses Capri's code, art, animations or any of its other assets must credit the Capri project (https://github.com/lachydotmcg/capri-desktop). Capri's art and animations are by her artist, who prefers to stay private. Parts made by others keep their own licences, listed in its README.

[PolyForm Noncommercial 1.0.0](LICENSE.md) and all [third-party notices](THIRD-PARTY-NOTICES.md) remain applicable. Original project details, asset credits and provider/licence information are preserved in [docs/UPSTREAM_README.md](docs/UPSTREAM_README.md). This unofficial fan-derived project is not endorsed by CD PROJEKT RED, the anime rights holders or voice actors.
