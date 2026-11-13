# Third-party notices

## Husk mode

Took inspiration from Mochiiwuff's work when it was under an MIT Licence (capri-desktop 0.4.3, commit f839580), exclusively for husk mode.

## Speech models

Her own ears run [NVIDIA Parakeet TDT 0.6B v2](https://huggingface.co/nvidia/parakeet-tdt-0.6b-v2) for English and Parakeet TDT 0.6B Ultra for her other 24 languages: Ultra's weights are by Moondream, trained from [nvidia/parakeet-tdt-0.6b-v3](https://huggingface.co/nvidia/parakeet-tdt-0.6b-v3). Both are licensed [CC-BY-4.0](https://creativecommons.org/licenses/by/4.0/), and the int8 ONNX exports are by sherpa-onnx (Apache-2.0). Neither NVIDIA nor Moondream endorses Capri.

## QR codes

Capri Remote's pairing QR code (app/RemoteQr.cs) is ported from Project Nayuki's [QR Code generator library](https://www.nayuki.io/page/qr-code-generator-library), Copyright (c) Project Nayuki, under the [MIT License](https://opensource.org/licenses/MIT). The licence text is retained in the file's header and reproduced here for binary distributions:

Copyright (c) Project Nayuki. (MIT License)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
Software.

The Software is provided "as is", without warranty of any kind, express or implied, including but not limited to the
warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or
copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or
otherwise, arising from, out of or in connection with the Software or the use or other dealings in the Software.

## Fonts

Share Tech Mono and Barlow font licence texts accompany binary releases in `licenses/OFL-ShareTechMono.txt` and `licenses/Barlow-OFL.txt`. Their originals are retained under `assets/fonts/` in the source.

## Arch v0.8.6.1 native adaptations

The click animation is by **mixmix312352**, reused from the pinned upstream click strip. The stand/orientation, editor PanelArt and CapriMainframe-Bold font are reused from lachydotmcg/capri-desktop v0.8.6.1 under the existing project terms. Preserve upstream LICENSE.md and notices. Original Mochiiwuff source attribution remains https://github.com/mochiiwuff/capri-desktop. No generated visual media was added.

The Linux native font bootstrap adapts SkiaSharp’s MIT-licensed LibraryLoader isolation flag from PR2247, merge `482e6ee2913a08a7cad76520ccf5fbce97c7c23b`. Its source provenance and MIT license are retained in `docs/provenance/SkiaSharp`; packaged HarfBuzzSharp and SkiaSharp dependency pins are unchanged.
