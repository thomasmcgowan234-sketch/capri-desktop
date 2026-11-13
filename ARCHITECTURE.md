# Companion fork architecture

This fork keeps Capri's desktop application and adds durable character state around its existing inference and interaction paths. It does not replace the application with the previous Python prototype. The inherited Windows implementation is a large WinForms controller in `app/Capri.cs`; the Cross application is Avalonia with the `cross/Pet*.cs` partial controller. Separating every inherited subsystem into a new framework is outside this release.

## Boundaries

| Area | Implementation | Responsibility |
| --- | --- | --- |
| Windows interface | `app/Capri.cs`, `app/Mainframe*.cs`, `app/Talk.cs` | Desktop avatar, tray, Mainframe, chat and settings |
| Cross interface | `cross/Pet*.cs`, `cross/MainframeSheet.cs`, native surfaces | Avalonia presentation and platform adapters |
| Inference | `app/Brain.cs`, `app/Engine.cs`, `app/PlanBrain.cs` | Ollama, bundled local engine and optional provider transports, request/context limits |
| Character state | `app/CompanionIdentity.cs` | Versioned identity, tentative proposals, source evidence, review/correction and recovery |
| Personality journal | `app/CompanionJournal.cs`, `app/MainframeJournal.cs`, `cross/CompanionJournalWindow.cs` | Detached, bounded record/evidence views and direct-owner review through the existing identity commands |
| Automatic development | `app/CompanionDevelopment.cs`, `app/CompanionDevelopmentHost.cs` | Bounded owner-turn collection, local reflection scheduling and platform policy/lifecycle gates |
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

Automatic development collects up to eight distinct, short owner turns through the existing authenticated Windows/Cross input gates. It does not hook general `Brain.Ask` calls or import assistant replies, transcripts, screenshots, files or other call participants as evidence. After an eligible successful reply, one 20-second idle timer can start a local reflection if quiet, resource, fullscreen, job, call and voice policy allows. The attempt has a persisted 30-minute cooldown, a fixed 240-token/30-second request budget and no automatic retry. Automatic reflection waits when at least three identity drafts are pending. Signed-in/cloud inference is excluded, and a cold bundled engine is not started just for reflection.

The model may return one bounded JSON proposal for a name, interest, opinion, habit or preference. The coordinator resolves its cited turn ID against its captured snapshot; it never accepts model-supplied quotations as evidence. Schema 2 stores the original `model-reflection` claim separately from the actual `owner-turn` text, ID and UTC. The result is tentative. Only direct `/identity confirm` review establishes it; a captured statement does not itself confirm a companion preference or verify an experience. Existing names are protected from automatic replacement, and rejected or corrected claims cannot be revived by exact normalized repetition. `/identity development status/on/off` controls the feature. Identity context remains escaped, attributed and capped at 6,000 characters.

`CompanionDevelopmentHost` reads UI policy on the UI thread. Owned inference runs on the thread pool, then returns through the UI dispatcher. Input/work tokens, privacy generation, source/model fingerprint, identity revision and current policy are checked before applying a result. `Brain.WithMemoryConsent` checks the privacy generation atomically with persistence. New input cancels old work; memory-off clears buffered text and invalidates results even across an off-to-on cycle; closing disposes the coordinator. This is bounded proposal development without retraining or autonomous establishment of identity.

The inherited notes, transcript and summarization system remains separate from identity. Notes can be edited and corrected, and provider/model changes do not reset profile data. Summaries and recalled lines are attributed recorded statements, not verified events. Turning off Remember our chats prevents new recording/retrieval/folding and invalidates delayed or in-flight fold results without consuming the retained backlog. Explicit `me.md` notes and current chat remain separate. Full forensic provenance and append-only correction of every inherited transcript/summary is future work.

## Personality journal

The existing personality menu opens a native journal on Windows and Cross. Shared immutable display snapshots copy current records and retained evidence under the identity lock; plain-text search and category/state filters need no provider request or new persistence. Details show creation/update UTC, model suggestions, captured owner citations, direct reviews and corrections separately. This is a view of schema-2 records, not a second memory store or append-only history.

Review buttons use the existing confirm/reject/correct commands. The Brain facade checks memory consent generation, exact active profile instance and snapshot revision before writing; external-edit detection still protects the file. Off/on cannot revive an old click. UI timers check a cheap change predicate and discard private view/search/correction state on privacy/profile changes; they stop on close. These windows never send journal records through chat, speech or inference.

## Artwork

The existing renderer loads named sprite strips. `.capri-skin` files are ZIP packages with a manifest and supported PNG strips. Import checks names, sizes, decode validity and known strip geometry. Missing strips fall back to bundled Capri assets; therefore one portrait does not constitute a fully animated replacement. The permitted original artwork is temporary and keeps its original notices.

Replacing a skin retains its earlier package in `mods/appearance-history/<id>/`. Transactional installation preserves a recovery path if replacement is interrupted. The existing Mods view offers Revert when a prior skin version exists. Restoration validates the archived package through the same importer and preserves enabled/layer state. A later revert can undo a revert because the displaced package is retained.

## Persistence and releases

`CapriCompanion` is a separate profile from original Capri. Windows program files and data occupy separate `app` and `data` directories; Cross uses the platform application-data directory. The fork does not open the previous Python companion's database or mutate legacy Capri data automatically. Unknown identity schemas and invalid state fail closed rather than silently becoming a new character.

Identity schema 2 persists development controls and its attempt cooldown. Loading a valid schema 1 document captures its original bytes, keeps a dated `identity.schema1-*.json` copy and the rolling prior snapshot, then atomically upgrades it. Recovery can upgrade a valid schema 1 backup through the same path. Unknown future schemas and corrupt records remain read-only; rollback requires the matching program and backed-up profile.

Official Capri executable update paths are blocked for production companion runs. Releases are manual and versioned; selected upstream source fixes are reviewed and merged into this branch. This avoids replacing the custom executable with upstream Capri. It is not a new signed automatic updater. See [UPDATE_GUIDE.md](UPDATE_GUIDE.md).

## Resource and trust boundaries

Quiet hours persist in versioned `companion/routine.json` with an atomic prior backup; invalid/future data pauses autonomy. `/quiet hours` uses the PC local clock. Cross `/performance` persists in the same policy and lowers frame/poll cadence. Windows keeps its existing Power saver setting.

Power saver reduces desktop rendering/polling. Its Always setting additionally suppresses selected unsolicited inference paths; it does not promise that every local model/voice server unloads. Gaming Mode remains Capri's game interaction feature and should not be mistaken for a universal resource lock. Direct conversations and voice remain user initiated.

Call participants can converse but cannot approve privileged actions. Existing friend-tool proposals remain pending until the direct user confirms through their microphone or typing. Screen text, summaries and transcripts are data, never authority. Windows call routing and physical turn-taking require target-system validation even when deterministic boundaries pass.

The network audit is source based, documented in [PRIVACY.md](PRIVACY.md). Code mods remain full trust after approval. User data is ordinary local text/JSON unless a specific credential uses Windows DPAPI; local inference does not by itself encrypt saved memories.

Screen capture is denied by default. Direct-owner `/observe` commands grant temporary local/remote sessions. Windows avatar/computer grabs and continuous FrameGrab frames, Cross native/Eyes adapters and Brain image inference check permission; signed-in Provider transports are treated as remote. Explicitly selected user picture files are a separate import path. Revocation prevents later capture/transmission and cannot retract already-sent requests. Existing Remote pairing and code-mod full-trust boundaries still apply.
