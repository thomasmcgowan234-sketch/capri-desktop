# Development status

Status date: 12 November 2026. Development branch: `companion/persistent-identity`. Foundation: Capri fork checkout at upstream commit `26849b2`; source continues to credit [Capri Desktop](https://github.com/lachydotmcg/capri-desktop).

This is a development release. Source-level integration and Linux automated validation are separate from Windows hardware acceptance. The target remains an integrated Windows companion; do not read retained Capri features as a claim that physical Windows behavior was tested here.

## Changes in this branch

- Original, initially unnamed conversational behavior replaces compulsory Cyberpunk biography and fixed tastes.
- Structured persistent identity and evidenced-memory records, tentative proposals and explicit direct-user review/correction/recovery commands.
- Automatic local personality proposals from bounded authenticated owner conversation, with source citations, review, cooldowns and privacy/resource gates.
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
| Windows launch/install | Privacy checkpoint `b3ad5c6` passed native runtime smoke (1,417 PASS, 0 FAIL, 0 SKIP; 53 explicit limits), installer/payload and silent install/uninstall checks. Automatic-development integration requires its own final CI result in PROGRESS.md; target-PC acceptance remains separate |
| Natural identity growth | Bounded local reflection now creates cited tentative proposals after direct conversation; only owner review establishes them. Focused schema/coordinator checks pass. Rich journal UI and model-quality/long-session acceptance remain |
| Complete original appearance | Current art is the licensed temporary Capri appearance; import a complete compatible sprite set for full replacement |
| Physical voice/Discord | Echo cancellation, device routing, interruptions and real call participation need Windows audio/hardware tests |
| Observation permissions | Verify every capture and tool path with the Windows checklist; inherited features must be exercised under revocation and prompt injection |
| Quiet hours/activity/fullscreen autonomy | Persistent overnight quiet-hour policy and low-resource gates implemented; inherited activity/cooldown/fullscreen gates remain, with target-system acceptance required |
| Model compatibility | Qwen3.5 9B is configurable but not validated here against a live Ollama/Radeon installation |
| Gaming performance | No RX 7900 XTX/5900X measurements; model/voice memory release and impact need recorded target-PC measurements |
| Update automation | Manual releases and backups protect the fork; there is no new signed automatic companion updater |
| Storage security | Memory/identity/transcripts are local plaintext; Windows provider secrets use account-bound DPAPI, Cross secrets use private plaintext files |
| Upstream modularity | New state/profile modules have narrow responsibilities; large inherited controller classes remain |

## Next acceptance sequence

1. Finish the current revision's full native runtime/installer validation and use the resulting [Windows test build](WINDOWS_TEST_BUILD.md). Keep original Capri and the previous companion unchanged.
2. Configure a local Ollama model; confirm the exact installed tag and image/tool capability instead of assuming the requested model supports all paths.
3. Exercise automatic draft generation and its off/quiet/performance gates, direct review/correction, schema-1 migration, restart/recovery, appearance replacement/revert and interrupted-update recovery.
4. Run physical audio, call-origin, observation revocation, dual-monitor/mixed-DPI and low-resource acceptance checks in [TESTING.md](TESTING.md).
5. Record actual CPU/RAM/VRAM and latency, then address measured regressions before adding more background autonomy.

The privacy-repair checkpoint has full Linux regression/application-smoke and native Windows runtime/installer evidence. Current automatic-development work has 65 C#5 identity checks, 59 C#5 coordinator checks and a successful C#5/net48 SDK compile (12 inherited warnings, zero errors); final integrated counts and CI results belong in `PROGRESS.md`. Cross build has four inherited warnings. No live Qwen3.5/Ollama setup, physical audio/Discord, AMD GPU or dual-monitor acceptance has been performed.
