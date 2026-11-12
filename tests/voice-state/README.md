# Voice state and low-resource regression checks

Run `python3 tests/voice-state/run.py` from an activated .NET 10 environment.
No NuGet test packages are required. The runner extracts the current `TalkOver`
class and bounded Windows low-resource guard methods into a unique temporary
project, logs whole-file SHA256 hashes, compiles with C# 5 language rules, runs
89 checks, and deletes its temporary output. It fails closed if extraction
boundaries change.

Synthetic levels verify echo learning, sustained direct/call talk-over, isolated
bumps separated by speaker silence, call evidence expiry through silence, and
reset. Production guards verify Power saver Off/In games/Always and restoration
for follow-ups, game comments/reactions, watch alerts, watch-along and pop-ins.
The fixture stubs unrelated UI/device/context state into a free/idle baseline.
It does not measure recognition accuracy, physical echo cancellation, real
Discord audio separation, Windows UI, CPU/GPU use, or end-to-end capture pause.
Run Windows acceptance checks for those behaviors. This remains an energy/echo
gate, not a trained speech/noise classifier; push to talk is available for noisy
rooms.
