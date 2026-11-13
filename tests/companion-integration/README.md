# Integrated companion checks

`dotnet run --project tests/companion-integration/CompanionIntegrationChecks.csproj -c Release`

References the production Cross assembly and calls its real Brain/command/identity/consent/routine paths. Synthetic providers and disposable loopback HTTP fixtures record requests; no real model, account, external network, desktop or audio service is used. Covers relevant persistent retrieval and model switching, privacy pause, signed-in-provider screenshot consent before transmission, revocation, overnight quiet-hour boundaries, restart, invalid policy protection and low-resource persistence.

Memory privacy checks use unique markers found only in saved summaries/transcripts. They verify attributed, escaped retrieval and its removal from the next actual provider request when memory is disabled, without resetting the active chat. Both explicit and incidental recall preserve speaker attribution and never certify assistant claims as events. Disabled transcription preserves all existing files and creates none.

Ten additional production-linked checks hold a synthetic fold provider open through privacy revocation. Disabled folding makes no provider request; turning memory off attempts cancellation, and both off and off-then-on reject the old result without changing a byte of memories.md or unfolded.txt. A fresh re-enabled fold succeeds. The provider deliberately ignores cancellation, so stale-result rejection is required.

Actual development transport checks exercise Ollama, compatible-server and local-wrapper requests. They verify the fixed 240-token budget despite long normal reply settings, one request without reasoning retries, no tools/images, cloud/provider exclusion, explicit-server precedence, source changes, owned cancellation that leaves normal chat working, and refusal to forward loopback redirects.
