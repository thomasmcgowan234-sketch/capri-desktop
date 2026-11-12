# Integrated companion checks

`dotnet run --project tests/companion-integration/CompanionIntegrationChecks.csproj -c Release`

References the production Cross assembly and calls its real Brain/command/identity/consent/routine paths. A synthetic in-process provider records requests; no model, account, network, desktop or audio service is used. Covers relevant persistent retrieval and model switching, privacy pause, signed-in-provider screenshot consent before transmission, revocation, overnight quiet-hour boundaries, restart, invalid policy protection and low-resource persistence.
