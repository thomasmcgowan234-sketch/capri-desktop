# Development status

Status date: 13 November 2026. Development branch: `companion/persistent-identity`. Foundation: Capri fork checkout at upstream commit `26849b2`; source continues to credit [Capri Desktop](https://github.com/lachydotmcg/capri-desktop).

This is a development release. Source-level integration and Linux automated validation are separate from Windows hardware acceptance. The target remains an integrated Windows companion; do not read retained Capri features as a claim that physical Windows behavior was tested here.

## Changes in this branch

- Original, initially unnamed conversational behavior replaces compulsory Cyberpunk biography and fixed tastes.
- Structured persistent identity and evidenced-memory records, tentative proposals and explicit direct-user review/correction/recovery commands.
- Automatic local personality proposals from bounded authenticated owner conversation, with source citations, review, cooldowns and privacy/resource gates.
- Local searchable personality journal in the existing Windows/Cross menus, showing dates and retained provenance with guarded confirm/reject/correct controls.
- Identity schema 2 with preserved schema-1 migration snapshots, persistent development controls and stale-result protection.
- Isolated companion profile and program directories, with production upstream executable updates blocked to protect fork changes.
- Transactional skin replacement with archived prior packages and Revert in the existing Mods interface.
- Expiring observation leases, separate remote image consent, persistent quiet hours and verified offline full-profile backup/restore tooling.
- Targeted voice interruption correction and additional low-resource gating through existing Power saver controls.
- Maintainer instructions, architecture, preserved requirements, source-based privacy audit, manual release/recovery guidance and Windows test checklist.

Read [PROGRESS.md](PROGRESS.md) for exact executed commands/results and the final integration state. During active development a listed change is not acceptance evidence until its tests/build result are recorded there.

## Preserved application paths

Capri's Mainframe, desktop sprite/drag/show-hide behavior, local Ollama and other model-provider options, speech components, call integration, tools, mods and security mechanisms remain the foundation. Cross remains a preview platform and does not replace Windows call functionality. The project does not introduce a separate Discord text bot as a substitute for voice calls.

## Incomplete or requiring acceptance

| Requirement | Current limit / next work |
| --- | --- |
| Windows launch/install | Automatic-development revision `dada185` passed native runtime smoke (1,417 PASS, 0 FAIL, 0 SKIP; 53 explicit limits), installer/payload, silent install/uninstall and complete test-package validation in [run 43324949376](https://github.com/thomasmcgowan234-sketch/capri-desktop/actions/runs/43324949376). Target-PC acceptance remains separate |
| Natural identity growth | Bounded local reflection creates cited tentative proposals after direct conversation; only owner review establishes them. Focused tests and the live typed-input/review/reload/profile/privacy smoke pass. Searchable journal with evidence and direct review is implemented; model-quality/long-session acceptance remains |
| Complete original appearance | Current art is the licensed temporary Capri appearance; import a complete compatible sprite set for full replacement |
| Physical voice/Discord | Echo cancellation, device routing, interruptions and real call participation need Windows audio/hardware tests |
| Observation permissions | Verify every capture and tool path with the Windows checklist; inherited features must be exercised under revocation and prompt injection |
| Quiet hours/activity/fullscreen autonomy | Persistent overnight quiet-hour policy and low-resource gates implemented; inherited activity/cooldown/fullscreen gates remain, with target-system acceptance required |
| Model compatibility | Qwen3.5 9B is configurable but not validated here against a live Ollama/Radeon installation |
| Gaming performance | No RX 7900 XTX/5900X measurements; model/voice memory release and impact need recorded target-PC measurements |
| Update automation | Manual releases and backups protect the fork; there is no new signed automatic companion updater |
| Storage security | Memory/identity/transcripts are local plaintext; Windows provider secrets use account-bound DPAPI, Cross secrets use private plaintext files |
| Upstream modularity | New state/profile modules have narrow responsibilities; large inherited controller classes remain |

## Next acceptance sequence (when target-PC testing is wanted)

1. Download the validated [Windows test package](https://github.com/thomasmcgowan234-sketch/capri-desktop/actions/runs/43324949376/artifacts/13991699793) and follow the [PC guide](WINDOWS_TEST_BUILD.md). Keep original Capri and the previous companion unchanged.
2. Configure a local Ollama model; confirm the exact installed tag and image/tool capability instead of assuming the requested model supports all paths.
3. Exercise automatic draft generation and its off/quiet/performance gates, direct review/correction, schema-1 migration, restart/recovery, appearance replacement/revert and interrupted-update recovery.
4. Run physical audio, call-origin, observation revocation, dual-monitor/mixed-DPI and low-resource acceptance checks in [TESTING.md](TESTING.md).
5. Record actual CPU/RAM/VRAM and latency, then address measured regressions before adding more background autonomy.

Previous checkpoint validation: 80 C#5 identity checks, 71 C#5 coordinator checks, 59 integrated production/loopback checks, 15 package-evidence checks, the full local regression script, and 253 fresh application smoke checks pass. C#5/net48 SDK compilation has 12 inherited warnings and zero errors; the Cross publish has four inherited warnings. Hosted Linux, native Windows runtime/installer and complete package validation pass for `dada185`. The macOS job was cancelled without acceptance. No live Qwen3.5/Ollama setup, physical audio/Discord, AMD GPU or dual-monitor acceptance has been performed.

## Resumed development — companion 0.1.1

The branch was recovered at `22aad07`, without recreating completed systems. Independent GitHub log review confirms the Windows/Linux/installer results above; the overall workflow conclusion was failure with macOS cancelled, so platform success is reported separately. Fresh required suites initially reproduced 80 identity, 71 coordinator and 59 integrated passes.

New work adds the bounded journal without a schema migration and binds native Windows package evidence to its exact executable, source and reports. Final validation and package links for this revision are recorded in PROGRESS.md and WINDOWS_TEST_BUILD.md. Physical microphone/Discord, live Ollama/Qwen/Radeon, gaming measurements, multiple monitors and complete original artwork remain unfinished; no installation or launch on the owner PC is required for this development work.

Latest 0.1.1 validation: full Linux regression exits 0; **94 identity / 71 coordinator / 68 integrated / 41 package checks** pass; fresh self-contained application smoke **258 PASS / 0 FAIL / 0 LIMIT / 0 SKIP**; Windows C#5/net48 compilation passes with 12 inherited warnings and zero errors. Fresh native Windows runtime/installer execution remains blocked by workflow-dispatch availability in this session. An explicitly unvalidated SDK Windows package is staged in ignored `out/companion-v0.1.1/`; the prior 0.1.0 native-tested artifact remains linked for historical acceptance. The 10 new native journal test groups must pass in the next Windows run before a validated 0.1.1 package is claimed.
