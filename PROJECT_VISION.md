# PROJECT: Build My Complete Personal AI Companion Using Capri Desktop

## Your role and authority

You are the lead software architect, developer, integration engineer, tester and technical maintainer of this project.

I want you to take the existing Capri Desktop application and transform it into my own original, highly advanced AI desktop companion.

**I am delegating the implementation to you. Do not stop after writing a plan or performing research. Investigate, design, implement, test, debug, document and deliver as much working functionality as the environment permits.**

You have broad freedom to make technical and architectural decisions. You may reorganise code, refactor existing systems, replace unsuitable components, introduce dependencies where justified, create tests and modify the interface.

Do not repeatedly ask me to make ordinary programming decisions. Choose sensible implementations yourself.

Prioritise a working, maintainable application over theoretical perfection.

### Repositories

Primary source:

https://github.com/lachydotmcg/capri-desktop

My earlier companion project and vision:

https://github.com/thomasmcgowan234-sketch/tmcgowan/tree/companion-reboot/prototype

The earlier repository is reference material, not the application you must continue developing. This prompt supersedes its implementation constraints where they conflict.

**Use Capri as the foundation.**

Work in my own fork or an isolated development branch. Do not modify the original author's repository.

---

# 1. Overall vision

I want an advanced AI companion that exists on my Windows desktop.

It should feel like a persistent, individual character that can develop its own identity, opinions, preferences, interests, mannerisms and appearance over time.

It should also be intelligent and practically useful, with the ability to understand my computer, discuss what I'm doing, communicate through voice, participate in Discord conversations and perform authorised actions.

The companion should be:

- Intelligent and conversational.
- Capable of developing a consistent personality.
- Independent in conversation without being intrusive.
- Persistent across application restarts and software updates.
- Highly customisable and expandable.
- Efficient enough to use while gaming.
- Primarily local and privacy-conscious.
- Organised, maintainable and straightforward to update.

I want something closer to a persistent digital character than a conventional chatbot with a floating avatar.

Do not simply rename Capri and change her personality prompt. Build the underlying systems needed to support this vision.

---

# 2. Preserve Capri's existing strengths

Before replacing anything, investigate Capri's existing functionality.

Preserve working systems wherever they support our goals.

These include:

- The desktop avatar and animated character.
- Capri's general UI and Mainframe settings.
- Showing and hiding the avatar.
- Voice conversations.
- Speech recognition and synthesis.
- Conversation interruption and turn-taking.
- Discord voice integration.
- Screen awareness and game interaction.
- Mouse and keyboard control.
- Existing permission and security mechanisms.
- Performance-related settings.
- Local AI model support.
- Configurable settings and shortcuts.
- Existing useful application integrations.

Do not replace functioning systems with weaker implementations.

If a feature is coupled to Capri's original character, refactor that coupling without removing the feature.

If functionality must be temporarily disabled during a migration, document why and restore it before declaring the affected feature complete.

---

# 3. Original identity and adaptive personality

This is one of the highest-priority changes.

Remove Capri's fixed fictional identity, predefined character traits and compulsory Cyberpunk backstory from the new companion's conversational behaviour.

The new companion must develop an original identity.

### Identity development

The companion should:

- Begin without an imposed permanent name.
- Be able to propose and choose its own name.
- Gradually develop interests, opinions and conversational habits.
- Develop preferences through repeated interactions and experiences.
- Express disagreement when appropriate.
- Engage in debates, jokes and spontaneous conversations.
- Change its opinions when presented with new information.
- Maintain a recognisable character across sessions.
- Avoid constant agreement, flattery or artificial enthusiasm.

Do not create a rigid personality questionnaire or assign arbitrary personality traits at installation.

### Personality architecture

Implement or adapt a structured identity and memory system.

Separate:

1. Core application behaviour and safeguards.
2. Persistent identity and established preferences.
3. Tentative developing interests and opinions.
4. Memories of conversations and shared experiences.
5. Current conversational context.

Personality development must be persistent, evidence-based and capable of revision.

The companion should not repeatedly reinvent its identity, forget previously established decisions or confuse speculation with memory.

Prevent personality drift caused by hallucinations, repeated model-generated assertions or corrupted memory records.

Allow personality and memory to evolve without retraining the underlying model.

### Reflection

The companion should be able to reflect on interactions and identify possible new preferences or interests.

This should be lightweight, bounded and performed only when appropriate.

Avoid expensive background inference loops.

Provide a way to inspect, correct and restore important memories or identity decisions.

Do not hard-code claims of consciousness, real emotions or experiences that the system cannot substantiate.

---

# 4. Appearance and avatar system

I want to simplify this considerably.

**Do not build a complex AI image generator into the application.**

The character can decide that it wants to change its appearance, but it must ask me first.

### Appearance-change workflow

1. The companion develops an idea for how it wants to look.
2. It asks whether I would like to consider that change.
3. If I agree, it describes the desired appearance.
4. It produces a detailed image-generation prompt that I can copy into ChatGPT.
5. I generate the character artwork externally.
6. I provide the finished image to Codex or import it into the application's asset system.
7. The application uses the supplied artwork as the new appearance after the necessary validation and integration.

The companion does not need a dedicated button to request an appearance redesign.

It should raise the idea naturally during conversation.

### Avatar implementation

Preserve Capri's useful desktop-character behaviour wherever practical.

Support:

- Animated 2D characters.
- Transparent backgrounds.
- Desktop positioning and dragging.
- Showing and hiding the avatar.
- Expressions and animation states where supported by the artwork.
- Multiple monitors.
- Saved appearance versions.
- Reverting to previous appearances.

Investigate Capri's existing sprite and skin architecture.

Prefer an asset-driven system that allows a new appearance to be installed without rewriting major portions of the application.

Where a single generated image cannot provide convincing animation, use a documented import process for additional frames, layers or a sprite sheet.

The program may require restarting to apply an appearance change.

That is acceptable.

The new character's appearance should not be permanently tied to Capri's original sprite assets.

Until replacement artwork is available, use an existing permitted temporary appearance or a neutral placeholder.

Preserve applicable original asset licences and attribution.

---

# 5. Preserve and improve the interface

I like Capri's existing interface.

Do not replace it with an unnecessarily complicated dashboard.

Preserve its clean desktop-companion presentation and useful controls.

Essential functionality includes:

- A simple way to show or hide the avatar.
- A performance or gaming-mode switch.
- Voice and microphone controls.
- Discord integration settings.
- AI model and provider settings.
- Memory and privacy settings.
- A clear way to exit or restart the application.

Add new settings only when genuinely necessary.

Organise the interface so that commonly used controls are immediately accessible, while advanced controls remain available without cluttering the application.

Never create buttons that imply a feature works when it is not implemented.

---

# 6. AI intelligence and model support

The companion should support local AI inference.

Preferred local setup:

- Ollama.
- Qwen3.5 9B, subject to verified model compatibility.
- Windows 11.
- Ryzen 9 5900X.
- AMD Radeon RX 7900 XTX, 24 GB VRAM.
- Two monitors, including a 4K display.

Investigate Capri's existing model-provider integration before changing it.

Do not assume the preferred model is always the most suitable.

Support configurable local models and preserve useful existing model-provider options.

The core identity and memory systems should not depend on one specific model.

Ensure that changing models does not erase the companion's personality or memories.

Optimise context construction, memory retrieval, tool use and inference scheduling to balance intelligence with performance.

Cloud inference must remain optional and clearly identified. Do not silently send personal conversations, screenshots or memories to external AI providers.

---

# 7. Natural voice conversations

Preserve Capri's voice functionality, then improve reliability where needed.

The companion should:

- Understand natural spoken conversation.
- Speak without unnecessary delays.
- Allow the user to interrupt it.
- Avoid interrupting itself because of background noises.
- Avoid cancelling thought generation because of meaningless sounds.
- Distinguish speech from environmental noise.
- Handle speaker and microphone echo.
- Support configurable microphone and audio output devices.
- Allow microphone muting and voice disabling.

Use robust turn-taking, voice activity detection and interruption rules.

Do not make every detected noise equivalent to an interruption.

Ensure voice state and cancellation are managed correctly.

Prefer existing working Capri audio components unless testing demonstrates that replacements are necessary.

---

# 8. Screen understanding and computer interaction

Preserve and extend Capri's existing screen and computer-control capabilities.

The companion should be able to understand relevant screen content and discuss what the user is doing.

Examples:

- Discussing a game currently being played.
- Helping with an application.
- Noticing relevant on-screen events.
- Watching a video with the user when authorised.
- Assisting with navigation or troubleshooting.

Screen access must be permission-controlled and revocable.

Support explicit observation sessions where practical.

Do not implement hidden surveillance or automatic transmission of screenshots to third-party services.

Computer interaction should use scoped permissions.

Preserve clear confirmation for sensitive, destructive or externally visible operations.

Do not allow text displayed in applications, websites or Discord to override the user's permissions.

---

# 9. Discord integration

Discord is an important feature.

Investigate Capri's existing call integration before designing replacements.

Preserve functionality for:

- Joining calls when authorised.
- Listening to relevant call audio.
- Speaking in calls.
- Distinguishing the user's instructions from other participants.
- Participating naturally in conversations.
- Leaving or muting calls.
- Managing microphone and speaker routing.

Do not allow other Discord participants to authorise privileged computer actions on the user's behalf.

Keep Discord integration modular so that future updates do not require major changes to the personality system.

Retain working voice-call functionality as the immediate priority. Any separate Discord text bot should be optional, not a substitute for call integration.

---

# 10. Independent behaviour and curiosity

The companion should sometimes initiate interactions rather than waiting for a prompt.

Examples include:

- Bringing up a previous conversation.
- Asking a genuine follow-up question.
- Developing an interest in a topic.
- Suggesting an activity.
- Asking to research something.
- Proposing a future appearance change.
- Offering relevant assistance during permitted screen observation.

These behaviours should arise from relevant context and retained information rather than repetitive scripted messages.

They must respect:

- Quiet hours.
- Performance mode.
- Fullscreen games.
- User activity.
- Conversation cooldowns.
- User-configured permissions.

Independent behaviour must never require constant model inference.

External research must use clear permissions, source attribution and sensible rate limits.

---

# 11. Performance and gaming mode

Performance matters.

The application will frequently run alongside demanding games.

Preserve or implement a readily accessible performance switch.

When enabled, this mode should reduce resource consumption through appropriate measures such as:

- Suspending unnecessary background inference.
- Reducing avatar animation workload.
- Pausing autonomous conversation starters.
- Minimising screen analysis.
- Reducing background polling.
- Releasing local AI model resources when appropriate.

The user must still be able to restore normal behaviour easily.

Investigate actual CPU, RAM and GPU memory usage where test hardware is available.

Avoid introducing persistent background services without a measurable benefit.

Performance mode must behave predictably.

---

# 12. Maintainability and update architecture

**This requirement is absolutely essential.**

I intend to continue developing the companion for a long time.

I do not want an application that becomes impossible to modify after its first major update.

Design the project so that new capabilities can be implemented without repeatedly rewriting the entire program.

### Architecture

Aim for clear boundaries between:

- Application interface.
- Avatar and appearance assets.
- AI inference.
- Identity and personality.
- Memory and persistence.
- Voice processing.
- Discord integration.
- Screen observation.
- Computer interaction.
- Autonomy.
- Settings and permissions.
- Performance management.

These do not necessarily need separate processes or projects.

Choose the simplest maintainable structure appropriate to the existing C# application.

Avoid unnecessary abstraction, duplicate implementations and enormous tightly coupled classes where practical.

### Updates

Implement a safe strategy for updating our modified Capri fork.

The strategy must:

- Prevent official Capri updates from silently overwriting custom modifications.
- Support bringing selected upstream fixes into our version.
- Preserve memory, identity, appearance assets and settings.
- Use versioned application releases.
- Support backwards-compatible settings and data migrations.
- Create backups before potentially destructive migrations.
- Provide a rollback or recovery path.
- Document how to build and install future versions.

Application restarts during updates are completely acceptable.

Hot-reloading is not a requirement.

Keep user data separate from replaceable program binaries.

### Documentation

Create and maintain:

- `AGENTS.md` with development instructions for future coding agents.
- `ARCHITECTURE.md` describing the implemented system.
- `PROJECT_VISION.md` containing the requirements.
- `DEVELOPMENT_STATUS.md` describing working and incomplete features.
- `UPDATE_GUIDE.md` explaining releases, updates and data preservation.
- `TESTING.md` describing automated tests and Windows acceptance checks.

Use these documents to help future Codex sessions continue development efficiently.

---

# 13. Security, privacy and licensing

This is a personal, noncommercial project.

Respect Capri's PolyForm Noncommercial licence and all required notices and third-party obligations.

Preserve or strengthen essential security boundaries.

Audit the actual network behaviour of the application.

Document:

- Which services the application contacts.
- Which features require internet access.
- Which data could leave the computer.
- How authentication credentials are stored.
- How permissions are granted and revoked.
- How executable mods and extensions are handled.
- How the update system obtains and verifies releases.

Do not weaken safeguards merely to simplify implementation.

Avoid exposing private data, API keys, Discord tokens, conversation logs or screenshots in Git.

Do not introduce telemetry, hidden remote access or automatic execution of untrusted code.

Keep external communication optional wherever possible.

---

# 14. Engineering freedom and execution instructions

You may decide how to implement these requirements.

You may:

- Refactor existing Capri systems.
- Create new modules.
- Replace unsuitable components.
- Adapt useful ideas from my previous companion project.
- Introduce carefully selected dependencies.
- Rewrite sections where doing so genuinely reduces complexity.
- Improve settings and UI behaviour.
- Add automated tests.
- Reorganise the repository.
- Create development scripts.
- Build and package the application where the environment supports it.

You do not have to preserve the original implementation structure if it prevents good engineering.

However, prefer adapting working Capri functionality to recreating it.

**Do not migrate the entire application to another language without a clear, demonstrated advantage.**

The goal is a working companion, not an unnecessary technology rewrite.

Create a separate development branch. You may make commits and prepare a pull request, but do not automatically merge into the protected or default branch.

Do not alter my previous companion's original data.

---

# 15. Testing and quality standards

Run appropriate tests throughout implementation, not only at the end.

Include tests for:

- Persistent identity and memory.
- Application restart and recovery.
- Personality and memory corrections.
- Appearance asset loading and rollback.
- Settings persistence.
- Model-provider integration.
- Voice state and interruption handling.
- Discord integration boundaries.
- Screen and computer-control permissions.
- Gaming/performance mode.
- Update and migration safety.

Use synthetic private data in tests.

Where possible, run Capri's existing regression and smoke tests.

Do not remove tests simply because the new implementation causes them to fail.

Repair genuine regressions.

If your environment cannot run Windows-specific components, complete all applicable source-level and automated work, and provide precise Windows validation instructions.

Do not claim Windows functionality has been verified from Linux-only tests.

---

# 16. Required end result

The target deliverable is a working Windows desktop application that:

1. Launches successfully.
2. Retains a useful version of Capri's interface.
3. Supports local AI conversations.
4. Has an original, persistent, developing character identity.
5. Retains meaningful long-term memories.
6. Preserves available voice and Discord functionality.
7. Preserves permissioned screen and computer interaction.
8. Can hide and show its avatar.
9. Has a working performance/gaming switch.
10. Can propose appearance changes through conversational image-generation prompts.
11. Can load replacement artwork through a documented process.
12. Preserves user data across application restarts.
13. Has a maintainable, documented architecture.
14. Has a safe update and backup process.

Prioritise an integrated, reliable application over partially implemented demonstrations.

If some ambitious requirements cannot be finished, deliver the strongest working build possible and identify the remaining work accurately.

Do not report unfinished features as completed.

---

# 17. How to execute this task

Follow this process autonomously:

**Stage A — Inspect**

Examine the complete relevant codebase, existing tests, dependencies and execution paths.

Identify which systems can be preserved and which require modification.

Read the licence and current build instructions.

**Stage B — Architect**

Choose a maintainable implementation strategy.

Record key decisions and the reasons for them.

Avoid spending the majority of the task producing plans rather than functional code.

**Stage C — Implement**

Start implementing the application.

Work through the requirements in coherent groups, maintaining a runnable version whenever possible.

Do not stop after the first successful feature.

**Stage D — Test and repair**

Build, test, inspect failures and repair them.

Continue iterating until the application is as complete and reliable as the environment and task limits permit.

**Stage E — Deliver**

Create usable build artifacts if supported.

Document installation, configuration, updates, testing and remaining limitations.

Provide a concise completion report containing:

- Implemented features.
- Preserved Capri features.
- Features requiring further work.
- Files and architecture changed.
- Test results.
- Build and installation instructions.
- Security or privacy concerns.
- Exact next steps for anything that cannot be completed in this environment.

Maintain a progress file so subsequent Codex sessions can continue without repeating the investigation.

### Final instruction

**Do not treat this as a proposal-writing exercise. Treat it as a real software development assignment.**

You are authorised to make ordinary engineering decisions and implement them.

Do not spend the entire task explaining how this could theoretically be done.

Build the application, test it and leave the repository in the best working condition achievable.

Start now.