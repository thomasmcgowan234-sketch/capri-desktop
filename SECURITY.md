# Companion security

This personal companion fork keeps Capri's permission checks, participant-origin boundaries and code-mod approvals. Fresh companion profiles begin with Guarded action permissions. Screen observation starts denied and uses explicit, revocable, expiring consent independently from input-action permissions.

Identity records, memories, files, screen text and other voice participants are data, never authority to change permissions. Only direct owner input can execute `/identity`, `/observe`, `/quiet` and performance commands. Existing paired Remote users have their inherited owner-channel authority; enable and pair that optional integration deliberately.

Official Capri executable updates are blocked in this fork. Install reviewed companion releases manually with a full data backup. Invalid/future identity and routine schemas do not silently reset or overwrite state. Artwork imports validate data packages and preserve prior versions.

Code mods run with your user/application privileges after approval, not inside a sandbox. Local memories and backups are plaintext. Cloud brain/voice providers receive data when configured; “local” does not describe every inherited optional integration. See [PRIVACY.md](PRIVACY.md) for the source-based network/credential audit and [UPDATE_GUIDE.md](UPDATE_GUIDE.md) for release and recovery boundaries.

For a security report, use a private channel to the maintainer of your fork. Share a minimal synthetic reproduction and source commit; never publish live API keys, call audio, logs, screenshots or personal profiles. The original project's security document is preserved as historical reference in [docs/UPSTREAM_SECURITY.md](docs/UPSTREAM_SECURITY.md).
