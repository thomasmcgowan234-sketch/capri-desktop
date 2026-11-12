# Development status

Status date: 12 November 2026. Development branch: `companion/persistent-identity`. Foundation: Capri fork checkout at upstream commit `26849b2`; source continues to credit [Capri Desktop](https://github.com/lachydotmcg/capri-desktop).

This is a development release. Source-level integration and Linux automated validation are separate from Windows hardware acceptance. The target remains an integrated Windows companion; do not read retained Capri features as a claim that physical Windows behavior was tested here.

## Changes in this branch

- Original, initially unnamed conversational behavior replaces compulsory Cyberpunk biography and fixed tastes.
- Structured persistent identity and evidenced-memory records, tentative proposals and explicit direct-user review/correction/recovery commands.
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
| Windows launch/install | Both SDK cross-build and native Windows CI build pass; silent install/uninstall with data preservation passes on Windows CI. Runtime smoke and target-PC acceptance are recorded separately in PROGRESS.md |
| Natural identity growth | Reviewed explicit commands provide a conservative durable path; automatic evidence extraction/reflection and rich journal UI require further development |
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

1. Run the native Windows build and runtime/installer smoke checks with a synthetic companion profile. Check that original Capri and the previous companion remain unchanged.
2. Configure a local Ollama model; confirm the exact installed tag and image/tool capability instead of assuming the requested model supports all paths.
3. Exercise identity proposal/review/correction/restart/recovery, appearance replacement/revert and interrupted-update recovery.
4. Run physical audio, call-origin, observation revocation, dual-monitor/mixed-DPI and low-resource acceptance checks in [TESTING.md](TESTING.md).
5. Record actual CPU/RAM/VRAM and latency, then address measured regressions before adding more background autonomy.

Automated evidence: full `ci/check-linux.sh` passes; isolated self-contained application smoke passes 248 checks; C#5/net48 SDK Windows compilation passes (12 inherited warnings, zero errors). Cross build has four inherited warnings. No live model, physical audio/Discord, Windows desktop, AMD GPU or dual-monitor test was performed.
