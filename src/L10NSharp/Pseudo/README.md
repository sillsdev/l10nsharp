# Pseudolocalization transform

Everything in this folder is `internal`, in the `L10NSharp.Pseudo` namespace; the only way
in is `PseudoLocalization.Transform` (exposed publicly as `LocalizationManager.PseudoLocalize`).

## The transform

`[Tîitlée Mîissîing]` for "Title Missing":

- **Every vowel is doubled, accented on the first of the pair** (`VowelStretch`). The
  doubling provides the ~30–40% expansion (inside the words, where it stresses layout the
  way real translations do), and the accented-plain pairs (`îi`, `öo`, `ée`) make the text
  unmistakably transformed while staying easy to read — no real language systematically
  produces that pattern. Consonants, digits, and punctuation are untouched.
- **The whole string is bracketed.** A missing `]` reveals truncation; brackets mid-sentence
  reveal runtime string concatenation.
- **Placeholders and markup pass through untouched** (`EscapeHelpers`): `{0}`/`{0:fmt}`
  format placeholders, named `{app_title}`-style and `%0`-style placeholders (both
  substituted by consumers' front ends, e.g. Bloom's), and HTML/XML tags.

The transform is deterministic, so screenshots are comparable across runs. The behaviors
consumers rely on are pinned by `PseudoLocalizationTests`.

Design notes: the doubled-vowel expansion follows Mozilla's pseudolocalization approach
(Fluent's "accented" locale); brackets are common to Microsoft's qps-ploc, Android's en-XA,
and others. Earlier iterations of this feature (see git history) used the full accent map
and per-word padding of the PseudoLocalizer project, which we used as a starting point but
replaced for readability.

## Provenance

`EscapeHelpers` (the placeholder/markup-skipping logic) is adapted from the MIT-licensed
[PseudoLocalizer](https://github.com/martincostello/Pseudolocalizer) project, Copyright (C)
2012, Anders Kaplan, and extended here to also recognize `%0`-style and named
`{app_title}`-style placeholders. The rest of the folder is original to L10NSharp.

Upstream license for the adapted code:

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
