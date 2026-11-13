# Privacy and network audit

This is a source audit, not a packet capture or a guarantee about providers. Local inference does not make every retained Capri integration offline. The original security document is preserved at [docs/UPSTREAM_SECURITY.md](docs/UPSTREAM_SECURITY.md).

## Local data

The separate `CapriCompanion` profile paths are in [UPDATE_GUIDE.md](UPDATE_GUIDE.md). Identity, notes, summaries, transcripts, settings, skins/history and diagnostic text are ordinary local files, not generally encrypted. Backups can contain forgotten private records. A chosen Obsidian vault follows its own sync configuration.

Windows provider keys use DPAPI `CurrentUser` (`app/Voice.cs`); Remote pairing likewise uses account-bound protection. Code already running as the user can still access them. Cross provider secrets are plaintext private files, mode 0600 on Linux/macOS (`cross/Pet.cs`); the custom server address/key pair is atomically replaced together. Protect the account and backups. Never commit a real profile or share raw logs/keys/transcripts in an issue.

## External connections

| Feature | Destination / trigger | Data and implications |
| --- | --- | --- |
| Ollama | Loopback `127.0.0.1:11434` when selected | Chat/context/tool results/approved pictures go to the local service; Ollama downloads models separately |
| Local engine/voice | Owned loopback ports or configured local address | Local inference; entering a nonlocal server changes that boundary |
| Automatic personality drafts | Selected local/loopback model only, after eligible direct conversation and idle/resource gates | Bounded captured owner turns and identity continuity data; no screenshots, tools, imported transcripts or call-participant evidence. Cloud/signed-in providers are excluded from this automatic path |
| Cloud brain | OpenAI, Anthropic, DeepSeek, NVIDIA, Google AI, OpenRouter, Capri brain-code endpoint or compatible provider, explicitly configured | Chats/transcripts/retrieved identity, notes and memories/tool results/consented screenshots. OpenRouter may route to another provider. Selected background summaries/greetings can send contextual requests |
| Cloud voice | ElevenLabs, Fish Audio, OpenAI or configured endpoint when selected | Spoken text/settings; ElevenLabs conversation agent also mic audio/context/tool results; Fish voice creation uploads selected clip |
| Windows online speech | Microsoft via Windows when enabled/local ears unavailable | Mic audio may use OS speech service |
| Model/voice/ears packs | GitHub releases, Hugging Face and download/CDN hosts during setup or configured-feature recovery | Model/assets, IP/request metadata. Retained setup can retrieve missing enabled-feature packs |
| Executable updates | Production upstream updater blocked | Companion releases are manually obtained/verified; official Capri must not overwrite this fork |
| Mods directory | GitHub API/raw and `capri-reactions.lachydotmcg.workers.dev/mods` when browsing/installing/voting | Catalogue/previews/packages/stats/download/vote requests and connection metadata |
| Search/music/links | DuckDuckGo, YouTube and requested/opened sites | Search words, URLs and browser/site metadata |
| Discord presence | Local Discord RPC when enabled | Activity/version/time/links can appear as Discord status; distinct from call audio |
| Calls | Selected Discord/other call app when authorised | That app distributes audio to participants under its service behavior; participants cannot approve privileged actions |
| Capri Remote | `capri-remote.lachydotmcg.workers.dev`, off until enabled | Encrypted messages/audio/screens; relay still sees room/device/token-hash metadata/timestamps/traffic sizes |

Hosts/transports are in `app/Brain.cs`, `app/Voice.cs`; packs in `app/Engine.cs`, `app/Ears.cs`, `app/VoicePack.cs`; mods in `app/ModDirectory.cs`; relay in `app/RemoteLink.cs`. “OpenAI compatible” does not imply a server is local.

Remote brain selection is explicit in Mainframe and sends contextual conversation to that service. Screen observation has separate 30-minute consent: `/observe start` permits local observation; `/observe start cloud` permits screenshots to the selected remote provider during that lease; `/observe stop` revokes. It starts denied, expires, and cannot be authorised by screen/call/model text. Observation does not grant input actions. Revocation cannot retract a request already sent.

This fork adds no analytics/hidden remote-control service. Retained providers/Remote/mods/browser integrations communicate when used. The separate upstream phone-only website has a daily aggregate counter; it is not a new desktop companion feature.

## Permissions and extensions

Keep Mainframe security/direct-user confirmation. Enforce permissions in code, not only a prompt. Screen leases gate capture; computer actions have separate checks. Another voice participant, webpage, imported memory or displayed instruction cannot approve actions/change permissions.

`.capri-code` mods compile/run in process with application/user privileges after digest approval; changed code needs reapproval. This is not a sandbox. Use trusted source or keep code mods disabled. Settings mods and skins are data packages with validation/size/path limits.

Remote retains Capri's optional pairing, revocation and encrypted transport. It is not redesigned/physically accepted here; read preserved upstream Remote protocol/security before enabling. Do not deploy workers or publish credentials to test local functions.

## Release trust

Blocking upstream executable replacement is not automatic cryptographic signature verification. Build reviewed source or obtain artifacts/checksums through a trusted channel; keep licence notices. [UPDATE_GUIDE.md](UPDATE_GUIDE.md) covers backup/rollback and selected upstream source fixes.

Structured identity correction/forgetting affects its own current records. Inherited `me.md`, `memories.md`, daily transcripts and historical backups are separate stores; use their editors/settings when correcting or erasing the corresponding text. Quiet-hour settings contain schedule/resource fields, separate from personal memories.

Turning off Remember our chats stops transcript recording, summary/transcript retrieval and new background memory folding. Queued folding checks the setting again after its idle wait; active folds are cancelled where the transport supports it and their results cannot rewrite summaries or consume queued lines after revocation. Switching off then on creates a new consent generation, so an older result remains invalid. Already-sent requests cannot be retracted. Explicit editable `me.md` notes and current chat remain separate; the switch does not erase saved files.

Automatic development buffers up to eight distinct qualifying direct-owner turns while memory/development are enabled. A single 20-second idle callback may request one local draft, with a persisted 30-minute attempt cooldown, a 240-token/30-second transport budget and a three-pending-draft limit. It has no tools/images, cloud fallback, automatic retry or continuous inference loop. Memory-off clears the buffer and invalidates active work; source, input, profile/revision and policy checks also fence completion. `/identity development off` pauses this path without erasing established identity.

Saved automatic drafts identify `model-reflection` separately from the actual captured `owner-turn` text, UTC and ID. That quotation motivates an idea; it does not confirm a preference, verify an experience or grant permission. Only direct review establishes a draft. Model output cannot replace the captured citation. Common credential/contact patterns and oversized inputs are excluded, but stored citations and migration/recovery backups remain ordinary plaintext identity data.

Companion management commands display their responses as text; they do not synthesize identity records or privacy status through local/cloud voice services or call routing. Normal conversation still uses the voice provider you selected.

The personality journal is a local text-only view of current identity records and bounded retained evidence. Opening, filtering and selecting records makes no network/model/voice request and writes no files. Disabling Remember our chats hides its contents and clears visible search/correction drafts; profile, revision and consent-generation checks reject stale review clicks. Confirm/reject/correct buttons perform the same direct-owner operations as identity commands, including existing backup behavior. It is not an export or a complete conversation/edit history.
