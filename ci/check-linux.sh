#!/usr/bin/env bash
set -euo pipefail
root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
unset WAYLAND_DISPLAY HYPRLAND_INSTANCE_SIGNATURE DISPLAY DBUS_SESSION_BUS_ADDRESS
for project in tests/native-surface/NativeSurface.Tests.csproj tests/desktop/Capri.Desktop.Tests.csproj tests/computer/ComputerChecks.csproj tests/game-session/GameSessionChecks.csproj tests/input-cancel/InputCancelChecks.csproj tests/native-bridge/NativeBridgeChecks.csproj; do
    dotnet run --project "$root/$project" -c Release
done
make -C "$root/cross/native"
python3 "$root/tests/computer/native_input_checks.py" "$root/cross/native/capri-input"
python3 "$root/tests/native-bridge/terminal_supervisor_checks.py"
python3 "$root/tests/native-bridge/mac_process_checks.py"
python3 "$root/tests/native-bridge/mac_sandbox_checks.py"
python3 "$root/tests/upstream-release/run.py"
python3 "$root/tests/voice-state/run.py"

python3 "$root/tests/native-bridge/name_listening_checks.py"
python3 "$root/tests/game-session/escape_watch_checks.py"

dotnet build "$root/tests/font-bootstrap/FontBootstrapChecks.csproj" -c Release --nologo
python3 "$root/tests/font-bootstrap/run.py" "$root/tests/font-bootstrap/bin/Release/net10.0/NativeBridgeChecks.dll"

# Companion fork identity, update isolation, observation and appearance regressions.
dotnet run --project "$root/tests/companion/CompanionIdentityChecks.csproj" -c Release
xvfb-run -a dotnet run --project "$root/tests/appearance/AppearanceChecks.csproj" -c Release
python3 "$root/tests/companion-profile/run.py"
dotnet run --project "$root/tests/observation/ObservationChecks.csproj" -c Release
dotnet run --project "$root/tests/companion-integration/CompanionIntegrationChecks.csproj" -c Release
python3 "$root/tests/companion-backup/checks.py"
