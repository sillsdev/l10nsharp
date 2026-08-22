# Pseudolocalization transforms (vendored)

The transform code in this folder is vendored from the MIT-licensed
[PseudoLocalizer](https://github.com/martincostello/Pseudolocalizer) project
(`PseudoLocalizer.Core` v0.12.2) so that L10NSharp does not impose the package —
and its transitive `Karambolo.PO.Compact` dependency — on every consumer.

Everything in this folder is `internal` and lives in the `L10NSharp.Pseudo`
namespace; the only way in is `PseudoLocalization.Transform` (exposed publicly as
`LocalizationManager.PseudoLocalize`). Changes from upstream:

- Namespace changed from `PseudoLocalizer.Core` to `L10NSharp.Pseudo`; classes made
  `internal` and `static`.
- Only the transforms L10NSharp uses were taken (`Accents`, `ExtraLength`, brackets,
  `EscapeHelpers`); `Mirror`, `Underscores`, `Pipeline`, `ITransformer`, the file-format
  processors, and the mutable configuration APIs (`Accents.AddReplacement`,
  `ExtraLength.LengthenCharacter`) were dropped.
- Minor syntax downgrades for this repository's C# 8 / net461 targets
  (collection expressions, `string.Contains(char)`).
- `EscapeHelpers` preserves more placeholder shapes than upstream (which only skips
  `{digits}`/`{digits:fmt}` and HTML tags): named braces like `{app_title}` and
  `%0`-style placeholders, both used by L10NSharp consumers (e.g. Bloom's front end
  substitutes `{name}` and `%N` at render time).

The behaviors L10NSharp relies on are pinned by `PseudoLocalizationTests`.

## Upstream license

```
The MIT License (MIT)

Copyright (C) 2012, Anders Kaplan

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```
