# Companion fork architecture

This fork keeps Capri's desktop application and adds durable character state around its existing inference and interaction paths. It does not replace the application with the previous Python prototype. The inherited Windows implementation is a large WinForms controller in `app/Capri.cs`; the Cross application is Avalonia with the `cross/Pet*.cs` partial controller. Separating every inherited subsystem into a new framework is outside this release.

## Boundaries

| Area | Implementation | Responsibility |
| --- | --- | --- |
| Windows interface | `app/Capri.cs`, `app/Mainframe*.cs`, `app/Talk.cs` | Desktop avatar, tray, Mainframe, chat and settings |
| Cross interface | `cross/Pet*.cs`, `cross/MainframeSheet.cs`, native surfaces | Avalonia presentation and platform adapters |
| Inference | `app/Brain.cs`, `app/Engine.cs`, `app/PlanBrain.cs` | Ollama, bundled local engine and optional provider transports, request/context limits |
| Character state | `app/CompanionIdentity.cs` | Versioned identity, tentative proposals, source evidence, review/correction and recovery |
| Attention policy | `app/CompanionRoutine.cs` | Versioned quiet hours, Cross low-resource setting, cheap time/resource gates |
| Screen consent | `app/CompanionObservation.cs` | Ephemeral 30-minute leases, capture gates and local/remote image-transmission consent |
| Existing memory | `app/Brain.cs` | Editable `me.md`, `memories.md`, transcripts and bounded retrieval |
| Artwork | `app/SkinMods.cs`, `app/Mods.cs`, platform renderers | Validated sprite-strip skins, layering and appearance history |
| Voice | `app/Voice.cs`, `app/Ears.cs`, `app/Asides.cs`, `app/Agent.cs` | Speech providers/decoding, playback, echo and interruption handling |
| Calls | `app/CallMode.cs`, `app/CallEars.cs`, `app/CallPanel.cs`, `app/MicMix.cs` | Application call audio, participant origin and direct-user confirmation boundaries |
| Screen and actions | `app/Hands.cs`, `app/Computer.cs`, capture/watch adapters | Tool permission checks and platform capture/control |
| Initiative | `app/FollowUps.cs`, `app/Commentary.cs`, `app/Reactions.cs`, `app/Lookout.cs` | Existing event-based follow-ups and observation reactions |
| Profile/releases | `app/CompanionProfile.cs`, `app/Packaged.cs`, `app/Setup.cs`, updater entry points | Separate fork data/program paths and manual fork release policy |
| Extensions | `app/CodeMods.cs`, `app/ModApi.cs`, `mods/MODDING.md` | Existing approval-based in-process extensions |

## Character and memory layers

Application safety and permissions stay in code. The initial conversational prompt removes the compulsory Capri/Cyberpunk origin and arbitrary tastes. The durable identity layer supplies reviewed name/preferences and tentative proposals as bounded data; current chat remains session context. An editable personality file is customization, not evidence that an experience occurred or permission to bypass safeguards.

Identity development is deliberately conservative. The explicit `/identity` review workflow records proposals with source evidence and requires direct user review before establishing them. It prevents repeated assistant assertions from becoming independent evidence. This release does not implement an autonomous personality-reflection model loop. Natural conversation can propose an interest or a name; durable acceptance must pass the review path. Context has a size budget and is data-delimited rather than inserted as executable instructions.

The inherited notes, transcript and summarization system remains separate from identity. Notes can be edited and corrected, and provider/model changes do not reset profile data. A model-generated summary can still be wrong; it is not promoted into established identity merely because it is present in a memory file. Full forensic provenance and append-only correction of every inherited transcript/summary is future work.

## Artwork

The existing renderer loads named sprite strips. `.capri-skin` files are ZIP packages with a manifest and supported PNG strips. Import checks names, sizes, decode validity and known strip geometry. Missing strips fall back to bundled Capri assets; therefore one portrait does not constitute a fully animated replacement. The permitted original artwork is temporary and keeps its original notices.

Replacing a skin retains its earlier package in `mods/appearance-history/<id>/`. Transactional installation preserves a recovery path if replacement is interrupted. The existing Mods view offers Revert when a prior skin version exists. Restoration validates the archived package through the same importer and preserves enabled/layer state. A later revert can undo a revert because the displaced package is retained.

## Persistence and releases

`CapriCompanion` is a separate profile from original Capri. Windows program files and data occupy separate `app` and `data` directories; Cross uses the platform application-data directory. The fork does not open the previous Python companion's database or mutate legacy Capri data automatically. Unknown identity schemas and invalid state fail closed rather than silently becoming a new character.

Official Capri executable update paths are blocked for production companion runs. Releases are manual and versioned; selected upstream source fixes are reviewed and merged into this branch. This avoids replacing the custom executable with upstream Capri. It is not a new signed automatic updater. See [UPDATE_GUIDE.md](UPDATE_GUIDE.md).

## Resource and trust boundaries

Quiet hours persist in versioned `companion/routine.json` with an atomic prior backup; invalid/future data pauses autonomy. `/quiet hours` uses the PC local clock. Cross `/performance` persists in the same policy and lowers frame/poll cadence. Windows keeps its existing Power saver setting.

Power saver reduces desktop rendering/polling. Its Always setting additionally suppresses selected unsolicited inference paths; it does not promise that every local model/voice server unloads. Gaming Mode remains Capri's game interaction feature and should not be mistaken for a universal resource lock. Direct conversations and voice remain user initiated.

Call participants can converse but cannot approve privileged actions. Existing friend-tool proposals remain pending until the direct user confirms through their microphone or typing. Screen text, summaries and transcripts are data, never authority. Windows call routing and physical turn-taking require target-system validation even when deterministic boundaries pass.

The network audit is source based, documented in [PRIVACY.md](PRIVACY.md). Code mods remain full trust after approval. User data is ordinary local text/JSON unless a specific credential uses Windows DPAPI; local inference does not by itself encrypt saved memories.

Screen capture is denied by default. Direct-owner `/observe` commands grant temporary local/remote sessions. Windows avatar/computer grabs and continuous FrameGrab frames, Cross native/Eyes adapters and Brain image inference check permission; signed-in Provider transports are treated as remote. Explicitly selected user picture files are a separate import path. Revocation prevents later capture/transmission and cannot retract already-sent requests. Existing Remote pairing and code-mod full-trust boundaries still apply.
