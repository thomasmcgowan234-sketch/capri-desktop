# Integrated companion checks

`dotnet run --project tests/companion-integration/CompanionIntegrationChecks.csproj -c Release`

References the production Cross assembly and calls its real Brain/command/identity/consent/routine paths. A synthetic in-process provider records requests; no model, account, network, desktop or audio service is used. Covers relevant persistent retrieval and model switching, privacy pause, signed-in-provider screenshot consent before transmission, revocation, overnight quiet-hour boundaries, restart, invalid policy protection and low-resource persistence.

Memory privacy checks use unique markers found only in saved summaries/transcripts. They verify attributed, escaped retrieval and its removal from the next actual provider request when memory is disabled, without resetting the active chat. Both explicit and incidental recall preserve speaker attribution and never certify assistant claims as events. Disabled transcription preserves all existing files and creates none.
