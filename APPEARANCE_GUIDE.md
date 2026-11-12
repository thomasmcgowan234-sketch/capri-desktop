# External artwork and appearance versions

The conversation chooses the direction; the application imports supplied artwork. Ask the companion to discuss a change, approve the idea, and copy its detailed prompt into ChatGPT or another external artwork tool. No image-generation service is installed or called by this workflow.

A useful external prompt asks for an original full-body 2D character, transparent background, consistent proportions/lighting/palette, and clear idle/talking/blinking/sitting/dragged/sleep expressions. Do not include the companion's surrounding desktop, speech bubble or text. Keep the same frame canvas and registration point across states. A single portrait can establish the design but cannot supply all of Capri's animations: additional frames/strips are required.

## Package and import

1. Obtain artwork you may use. Preserve its licence and attribution in a separate project note/release notice. Do not place executable code in a skin.
2. Export RGBA PNG strips at exactly the matching dimensions below. Preserve frame order and alignment from the existing strip template. Flipped/perch/sleep/talk strips are separate named assets. Do not stretch a portrait across the full strip; each animation cell must contain an aligned frame.
3. Create a top-level `skin.json`, for example:

   ```json
   {"format":1,"id":"original-companion","name":"Original Companion","version":"1.0.0","author":"Your artist","description":"Original external artwork; licence recorded separately"}
   ```

4. Put the manifest and PNGs directly at the root of a ZIP, with no enclosing directory. Name the package `original-companion.capri-skin`. Use the same `id` for later versions.
5. Open Mainframe → Mods → Import; select the package and enable it. Restart if a platform renderer still holds old resources. Existing dragging, placement, show/hide and multi-monitor behavior stays with the renderer.
6. Replacing that ID archives its prior package under the profile's `mods/appearance-history/<id>/`. Use **Revert** in Mods for the preceding version; a revert is itself reversible. Import an older archived package for a specific historical version.

Missing strips fall back to licensed original Capri art. Supply every used strip for a complete replacement. Accessory skins (`"accessory":true`) intentionally layer transparent additions; they are not complete character replacements. See [mods/MODDING.md](mods/MODDING.md) for inherited layering/extension conventions.

Packages are limited to 96 MiB, 256 MiB unpacked, 32 MiB per picture, 64 entries and a 64 KiB manifest. The application rejects traversal, unknown file names, wrong geometry and images that cannot actually decode. Import transactions keep prior artwork/package pairs and recover interrupted replacement on the next Mods load. Unknown damaged transaction directories are preserved for manual recovery rather than deleted.

## Current strip dimensions

These are whole-strip dimensions from this source version, not individual frame sizes. Inspect the existing animation definitions/templates when authoring frame counts or timing. `idle.png` also accepts the inherited legacy 3840×384 strip, with documented legacy stand behavior.

| File | Width × height |
| --- | --- |
| `arrive.png` | 3360 × 384 |
| `blink.png` | 960 × 384 |
| `blink_glitch.png` | 960 × 384 |
| `click.png` | 2880 × 144 |
| `dance.png` | 3840 × 384 |
| `dangle.png` | 5760 × 384 |
| `face.png` | 400 × 400 |
| `form.png` | 5184 × 516 |
| `glitch.png` | 2400 × 384 |
| `grin.png` | 3840 × 384 |
| `idle.png` | 6960 × 384 |
| `idle_blink.png` | 928 × 384 |
| `idle_blink_flipped.png` | 928 × 384 |
| `idle_blink_glitch.png` | 928 × 384 |
| `idle_blink_glitch_flipped.png` | 928 × 384 |
| `idle_flipped.png` | 6960 × 384 |
| `idle_legacy.png` | 3840 × 384 |
| `idle_mouth.png` | 580 × 384 |
| `laydown.png` | 22320 × 250 |
| `laydown_blink.png` | 2976 × 250 |
| `laydown_blink_flipped.png` | 2976 × 250 |
| `laydown_blink_glitch.png` | 2976 × 250 |
| `laydown_blink_glitch_flipped.png` | 2976 × 250 |
| `laydown_flipped.png` | 22320 × 250 |
| `laydown_glitch.png` | 4400 × 250 |
| `laydown_glitch_flipped.png` | 4400 × 250 |
| `laydown_leave.png` | 5280 × 250 |
| `laydown_leave_flipped.png` | 5280 × 250 |
| `laydown_sleep.png` | 22320 × 250 |
| `laydown_sleep_flipped.png` | 22320 × 250 |
| `laydown_talk.png` | 22320 × 250 |
| `laydown_talk_flipped.png` | 22320 × 250 |
| `leave.png` | 2880 × 384 |
| `mouth.png` | 1200 × 384 |
| `perch.png` | 11760 × 384 |
| `perch_arrive.png` | 4032 × 384 |
| `perch_arrive_flipped.png` | 4032 × 384 |
| `perch_blink.png` | 1568 × 384 |
| `perch_blink_flipped.png` | 1568 × 384 |
| `perch_blink_glitch.png` | 1568 × 384 |
| `perch_blink_glitch_flipped.png` | 1568 × 384 |
| `perch_flipped.png` | 11760 × 384 |
| `perch_glitch.png` | 2880 × 384 |
| `perch_glitch_flipped.png` | 2880 × 384 |
| `perch_leave.png` | 3456 × 384 |
| `perch_leave_flipped.png` | 3456 × 384 |
| `perch_music.png` | 11760 × 384 |
| `perch_music_flipped.png` | 11760 × 384 |
| `perch_talk.png` | 11760 × 384 |
| `perch_talk_flipped.png` | 11760 × 384 |
| `sleep.png` | 3840 × 384 |
| `wake.png` | 3120 × 384 |
| `walk.png` | 2880 × 384 |
