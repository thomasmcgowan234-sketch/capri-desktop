# Appearance regression checks

The suite references the production Cross assembly and tests its real shared
`.capri-skin` importer. Fixtures contain bundled licensed sprite artwork and
synthetic names/version metadata. No provider, model, microphone, network call,
screen observation, or personal profile is used.

```bash
dotnet build tests/appearance/AppearanceChecks.csproj -c Release
# Supply a real or disposable X11/Wayland desktop for Avalonia's image backend:
dotnet run --project tests/appearance/AppearanceChecks.csproj -c Release --no-build
```

In the prepared cloud environment:

```bash
source /workspace/.capri-env/activate.sh
dotnet build tests/appearance/AppearanceChecks.csproj -c Release -p:NuGetLockFilePath=obj/onboarding.packages.lock.json
bash /workspace/.capri-env/run-x11.sh dotnet run --project /workspace/capri-desktop/tests/appearance/AppearanceChecks.csproj -c Release --no-build
```

Covers exact preceding package preservation, appearance rollback and undo,
stack order and disabled-state preservation on update/revert, restart, installed
package self-import, malformed PNGs, invalid/unknown zip paths, history identity
mismatch, all six replacement and four first-install transaction boundaries,
archive write failure and unrecognised transaction directory safety.

This validates shared production asset and recovery code on Linux. It does not
verify Windows drawing, DPI, display positioning or native Windows buttons; use
the Windows acceptance checks in TESTING.md for those.
