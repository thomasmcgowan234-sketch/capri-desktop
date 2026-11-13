# Personal AI Companion

A personal, noncommercial companion fork of [Capri Desktop](https://github.com/lachydotmcg/capri-desktop), release **companion 0.1.1**, based on Capri 0.8.9. It keeps Capri's Mainframe, desktop avatar, local-model support, speech, voice-call routing and computer tools while adding an initially unnamed character, persistent reviewed identity, bounded automatic personality proposals, artwork history and fork-safe data/update boundaries.

This is a development build. The preceding automatic-development revision passed native Windows and Linux CI; each later revision needs its own recorded validation. Automated tests do not establish physical audio, Discord or Radeon acceptance. See [DEVELOPMENT_STATUS.md](DEVELOPMENT_STATUS.md) for remaining work and [PROGRESS.md](PROGRESS.md) for the exact tested revision.

## Start on Windows

Extract the companion Windows release with its notices. Run `Capri.exe` for a portable development session or `Capri-Setup.exe` to install this fork. It uses `%LOCALAPPDATA%\CapriCompanion\data`; installed binaries use the separate `CapriCompanion\app` directory. Original Capri and the previous companion's data are not imported or changed automatically.

[WINDOWS_TEST_BUILD.md](WINDOWS_TEST_BUILD.md) covers the CI test artifact, checksum verification, backups and a first PC test. Use the artifact's validation report and source commit to identify what was actually tested.

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
| `/identity development status` | Inspect automatic-development controls, pending drafts and runtime status |
| `/identity development on` / `/identity development off` | Enable or pause automatic local personality proposals |
| `/observe start` | Grant a 30-minute observation session; image inference stays on loopback/local paths |
| `/observe start cloud` | Explicitly permit screenshots to the selected remote AI provider for 30 minutes |
| `/observe stop` | Revoke screen observation; restart also revokes it |
| `/quiet hours 22:00-08:00` | Pause independent activity during these hours in your PC's local time |
| `/quiet hours off` / `/quiet` | Disable or inspect quiet hours |
| `/performance on` / `/performance off` | Toggle low-resource behavior; available on Windows and the Cross preview |

Automatic development starts enabled and collects at most eight distinct qualifying direct-owner turns while memory is on. After an eligible reply and a 20-second idle delay, quiet/resource/fullscreen/job/voice gates may permit one local reflection. Attempts have a persisted 30-minute cooldown and stop at three pending drafts. The request has no tools or images, a fixed 240-token budget and a 30-second timeout; it does not use cloud or signed-in providers, continuously poll, retrain the model or retry invalid output automatically.

An automatic idea stays tentative and cites the captured owner statement that motivated it. That statement does not prove a companion preference or a claimed experience. Inspect `/identity`, then confirm, reject or correct the idea; only direct review establishes it. Automatic name proposals are limited to an unnamed companion. Turning memory off clears buffered turns, cancels work where supported and rejects stale results even after off-then-on. Inherited editable notes, transcripts and backups are separate stores; edit/erase them separately when needed. Some inherited wake phrases and gesture dialogue still use Capri terminology.

Open **Mainframe → Settings → Her personality → Personality journal** to search and filter current identity records, inspect dates and every retained evidence entry, and confirm, reject or correct a selected record. The Windows and Cross journal stays local and text-only, hides its content while Remember our chats is off, and refuses stale review actions after a profile, revision or privacy change. It shows bounded retained evidence rather than a complete edit history.

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
