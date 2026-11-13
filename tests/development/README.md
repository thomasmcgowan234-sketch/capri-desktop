# Automatic personality development checks

Run `dotnet run --project tests/development/CompanionDevelopmentChecks.csproj -c Release` with the prepared environment activated.

The C#5 project compiles the actual identity and development modules. It uses synthetic temporary profiles, a deterministic idle scheduler, an explicit UI dispatch queue and a fake local inference transport. No model, account, network, desktop or audio device is contacted.

Checks cover evidence budgets and repetition, authenticated input, privacy/resource/local-provider gates, strict response parsing and citation provenance, tentative review, cooldown/pending limits, revocation and stale-completion races, ignored transport cancellation, atomic consent guards, and disposal.
