# Pseudolocalization support for L10NSharp — implementation plan

**Goal:** Let any L10NSharp-based application offer a "pseudo-English" UI language so testers can
verify that its strings are properly internationalized. When the UI language is set to the
standard pseudo-locale **`qps-ploc`**, every string that flows through L10NSharp is returned as
readable, diacritic-decorated, padded English (e.g. `[Ĉóóǩ Ƀööǩ one two]` for "Cook Book").
Anything that still appears as plain English is a hard-coded, non-internationalized string.

This document is the plan for implementing that in this repository (L10NSharp). It was written
from a survey of both this codebase and BloomDesktop (the first intended consumer); the
Bloom-specific follow-up work is collected in the final section and is **not** part of the
L10NSharp change.

**Status:** plan only — nothing implemented yet.

## Background: what pseudolocalization is

The industry-standard technique (Microsoft's `qps-ploc` pseudo-locale, Android's `en-XA`):
transform the *English* text of every localizable string at lookup time —

1. **Accented substitution** (`a→á, e→é, C→Ç, …`) — readable as English but unmistakably
   "translated". Catches hard-coded strings and font/encoding problems.
2. **Expansion padding** (~30–40% longer) — simulates German/French length. Catches clipped
   labels and rigid layouts.
3. **Enclosing brackets** (`[…]`) — catches truncation (missing `]`) and runtime string
   concatenation (brackets mid-sentence).
4. **Placeholder/markup preservation** — text inside `{0}`-style format placeholders, HTML/XML
   tags, and entities passes through untouched. A literal `{0}` visible on screen in pseudo mode
   is itself a formatting bug worth catching.

The transform must be deterministic (same input → same output) so screenshots are comparable.

Because the pseudo text is derived from the live English string at lookup time, **no pseudo XLIFF
files are generated, shipped, or kept in sync** — the pseudo-locale is always exactly as complete
as the English source strings.

## Use an existing library: PseudoLocalizer.Core

Do **not** invent the transforms. [`PseudoLocalizer.Core`](https://www.nuget.org/packages/PseudoLocalizer.Core/)
(MIT, [martincostello/Pseudolocalizer](https://github.com/martincostello/Pseudolocalizer),
targets `netstandard2.0` — same as L10NSharp) already provides exactly this:

- `ITransformer` with `string Transform(string value)`;
- `Accents`, `ExtraLength`, `Brackets`, `Mirror`, `Underscores` transformer classes, each with a
  static `Instance`;
- `Pipeline` to compose them (e.g. `new Pipeline(Accents.Instance, ExtraLength.Instance, Brackets.Instance)`);
- `EscapeHelpers` logic already used by the transformers to skip .NET format placeholders and
  HTML/XML markup;
- it explicitly generates for the `qps-Ploc` pseudo-locale (it is also what several ASP.NET
  pseudo-localization writeups use).

Add it as a `PackageReference` to `src/L10NSharp/L10NSharp.csproj`.

Notes / verifications for the implementer:

- PseudoLocalizer.Core drags in one dependency of its own (`Karambolo.PO.Compact`, for PO-file
  processing we don't use). If adding that transitive dependency to every L10NSharp consumer is
  deemed unacceptable, the fallback is to vendor just the transformer classes
  (`ITransformer`, `Accents`, `ExtraLength`, `Brackets`, `EscapeHelpers`, `Pipeline`) into
  `src/L10NSharp/Pseudo/` with MIT attribution — they are small, dependency-free files. Prefer
  the package reference first.
- Write unit tests that pin down the behaviors we rely on: `{0}`/`{1:n0}` placeholders survive,
  HTML tags and entities (`&amp;`, `<strong>`) survive, WinForms accelerator ampersands (`&File`)
  survive usably, empty/whitespace strings, determinism. If `EscapeHelpers` turns out not to
  cover a case we need (e.g. literal `%0`-style placeholders some consumers use), wrap the
  pipeline in our own transformer that protects those spans before/after.

## Design

### Activation: setting the language code is (almost) sufficient

There is no separate "enable pseudolocalization" switch for *lookups*: any request for language
`qps-ploc` — whether via `LocalizationManager.SetUILanguage("qps-ploc")` or a per-call language
list — gets pseudolocalized English. This is harmless when unused and means a consumer can adopt
the feature simply by setting the language code.

One static opt-in **is** needed, but only for *advertising* the locale:

```csharp
/// <summary>
/// When true, the qps-ploc pseudo-locale is included in GetAvailableLocalizedLanguages()
/// and offered by the built-in language chooser dialog. Lookups for qps-ploc work
/// regardless of this setting. Default: false.
/// </summary>
public static bool OfferPseudoLocalization { get; set; }
```

Rationale: `GetAvailableLocalizedLanguages()` feeds consumer language menus and L10NSharp's own
WinForms `LanguageChoosingDialog`; end users of a release build should not see a test locale
there unless the application deliberately turns it on (e.g. only in its alpha channel).

### Locale tag: `qps-ploc`

Use the Windows-standard pseudo-locale tag, canonical casing `qps-ploc` (define
`LocalizationManager.PseudoLocalizationLanguageId` as a public const; compare
case-insensitively like other tags). `CultureInfo.GetCultureInfo("qps-ploc")` resolves on
Windows; verify `L10NCultureInfo.GetCultureInfo` (see `src/L10NSharp/L10NCultureInfo.cs`, which
exists precisely to cope with cultures the OS doesn't know) yields something usable on
Linux/Mono, where the culture will be synthesized/unknown. `SetUILanguage(pseudoTag)` must not
throw anywhere (`LocalizationManager.TrySetUILanguage`, `src/L10NSharp/LocalizationManager.cs`
~line 143, does a culture lookup — make sure the pseudo tag survives it, falling back to
`InvariantCulture` for `Thread.CurrentUICulture` if needed).

Display name: hard-code something self-explanatory, e.g. **"Pseudo-English (qps-ploc)"** —
don't rely on the OS to produce a sensible native name for it.

### Where to hook the lookups

All public `GetString`/`GetDynamicString`/`GetStringForObject` overloads in
`LocalizationManager` delegate to `LocalizationManagerInternal<T>`
(`src/L10NSharp/LocalizationManagerInternal.cs`), and those funnel into a small set of methods:

- `GetStringFromAnyLocalizationManager(stringId, …)` (~line 622 and ~645) — the static-string
  path. When the effective language (current UI language or a per-call preferred language) is
  `qps-ploc`: resolve the **English** text as usual (the caller-supplied `englishText` if
  present, else the `en` entry from the string cache) and return
  `PseudoTransform(english)`; report `languageIdUsed = "qps-ploc"`.
- `GetDynamicStringInternal` / `GetDynamicStringOrEnglish` (~line 460–552) — same rule. Note the
  existing special case "for langId `en`, the caller-supplied englishText wins over the cache";
  `qps-ploc` should behave identically except for the transform applied at return.
- `GetIsStringAvailableForLangId(id, "qps-ploc")` (~line 606) must return what it would return
  for `"en"` — consumers (e.g. Bloom's server-side i18n API) use this to distinguish
  "translated" from "fallback", and every English string is by definition available in the
  pseudo-locale.
- `MapToExistingLanguageIfPossible` (~line 566): make sure `qps-ploc` maps to itself (never to
  `qps` or to some real language) and, conversely, is never offered as a fallback mapping for
  any real language request.

Guard rails:

- The pseudo language must never engage the write-back machinery: no dynamic-string collection
  into XLIFF (`CollectUpNewStringsDiscoveredDynamically` path, `LocalizationManagerInternal`
  ~line 532–551), no participation in `MergeExistingEnglishTranslationFileIntoNew`, no attempt to
  load or save a `qps-ploc` XLIFF file.
- `StringCount("qps-ploc")` / `NumberApproved` / `NumberTranslated` should report the `en`
  counts (consumers use approved-fraction thresholds to filter their language menus; the
  pseudo-locale should look 100% complete).
- Transform exactly once (beware paths where one Get method calls another).

Implementation shape: a small internal static class (e.g. `PseudoLocalization`) owning the
configured `Pipeline` and a `bool IsPseudoLanguage(string langId)` helper, so the funnel methods
each add one short, obvious branch. Also expose the transform publicly —
`public static string LocalizationManager.PseudoLocalize(string english)` — so consumers can
apply the same transform to strings that take paths around L10NSharp (Bloom has one such case,
see the last section).

### Transform configuration

Default pipeline: `Accents` + `ExtraLength` + `Brackets`. No `Mirror`/`Underscores` by default
(those are for RTL/completeness testing and destroy readability). If we want configurability,
expose the pipeline as a settable static property typed as `Func<string, string>` (avoids
leaking PseudoLocalizer.Core types into the public API); default it to the pipeline above.

### WinForms

`LocalizationManagerWinforms` / the `L10NSharpExtender` localize controls through the same
string-cache lookups, so pseudo text should flow through with no changes — verify with
`src/SampleApp`. The `LanguageChoosingDialog` and any other place that enumerates
`GetAvailableLocalizedLanguages()` must include `qps-ploc` (with the hard-coded display name)
**only** when `OfferPseudoLocalization` is true.

### Tests

In `src/L10NSharp.Tests`:

- transform unit tests (see PseudoLocalizer.Core notes above);
- `GetString`/`GetDynamicStringOrEnglish`/`GetIsStringAvailableForLangId` behavior with UI
  language `qps-ploc`, including: string present in `en` XLIFF, string supplied only as
  `englishText`, string missing entirely;
- `GetAvailableLocalizedLanguages()` with `OfferPseudoLocalization` true/false;
- fallback behavior: a real-language lookup must never accidentally receive pseudo text;
- no XLIFF file writes occur for `qps-ploc` even with dynamic-string collection enabled.

### Delivery

1. PR to `sillsdev/l10nsharp` with the above; update `CHANGELOG.md`.
2. New NuGet release (9.1.0 — additive, no breaking changes).

## Suggested commit/work breakdown

1. Add PseudoLocalizer.Core dependency + internal `PseudoLocalization` helper class + transform
   tests (self-contained, no behavior change).
2. Hook the lookup funnels + availability/count reporting + `OfferPseudoLocalization` +
   manager-level tests.
3. WinForms verification (SampleApp) + `LanguageChoosingDialog` gating.
4. CHANGELOG + release.

## Open questions

1. Is the `Karambolo.PO.Compact` transitive dependency acceptable, or should we vendor the
   transformer classes? (Recommendation: take the dependency; revisit if a consumer objects.)
2. Should `ExtraLength` padding be on by default? (Recommendation: yes — layout stress is half
   the point; consumers can override the pipeline.)
3. Exact public-API naming (`OfferPseudoLocalization`, `PseudoLocalize`,
   `PseudoLocalizationLanguageId`) — align with existing L10NSharp naming conventions at review
   time.

---

## Next steps for Bloom (separate follow-up, in the BloomDesktop repo)

For the agent doing the Bloom side after the L10NSharp release. Context: Bloom consumes
L10NSharp 9.0.0 via NuGet (`src/BloomExe/BloomExe.csproj` ~line 244); all of its front-end
(React) strings funnel through `I18NApi.GetLocalizedStringInOneLanguage`
(`src/BloomExe/web/I18NApi.cs` ~line 273) into `LocalizationManager.GetDynamicStringOrEnglish`,
so both the WinForms and browser UI get pseudolocalized automatically once the library supports
it.

1. **Bump** the L10NSharp package to the release containing this feature.
2. **Enable on alpha/dev:** in `Program.SetUpLocalization()` (`src/BloomExe/Program.cs`
   ~line 1903), when `ApplicationUpdateSupport.IsDevOrAlpha`
   (`src/BloomExe/ApplicationUpdateSupport.cs` ~line 624), set
   `LocalizationManager.OfferPseudoLocalization = true`.
3. **UI language menu:** `WorkspaceView.GetLanguageItems`
   (`src/BloomExe/Workspace/WorkspaceView.cs` ~line 793) filters languages by
   fraction-approved and builds display names via Palaso's `IetfLanguageTag` — special-case
   `qps-ploc`: include it whenever offered, label it "Pseudo-English (i18n test)", place it last
   in the menu. Note the React menu round-trips selections by *display name*
   (`WorkspaceView.cs` ~line 835), so the label must map back to the tag.
4. **Persistence guard:** `Settings.Default.UserInterfaceLanguage` will store `qps-ploc`; if that
   setting is carried onto a channel where the feature is off, `SetUpLocalization` should fall
   back to English rather than presenting an inexplicable pseudo UI (or just allow it — lookups
   work regardless; decide at implementation time).
5. **Strings missing from the English XLIFF:** for those, I18NApi returns the client-supplied
   English fallback untouched, so they will show as plain English just like hard-coded strings.
   That's acceptable (the existing alpha missing-string toast disambiguates), or Bloom can call
   `LocalizationManager.PseudoLocalize` on that fallback in `I18NApi` — decide at implementation
   time.
6. **Smoke tests:** switch to Pseudo-English on an alpha build; sweep Collections/Edit/Publish
   tabs, dialogs, toolbox, hint bubbles, template page labels
   (`TemplateBooks.PageLabel.*` path via `HtmlDom.LocalizePageLabel`), and the injected
   book-page dictionary (`RuntimeInformationInjector`). Verify the front-end HTML-decode +
   `{0}`/`%0` substitution path (`localizationManager.ts` `simpleFormat`) renders pseudo text
   correctly.
7. **Docs/QA:** short note in `DistFiles/localization/README.md` (no pseudo XLF files should
   ever be created — the unused Crowdin `qaa` folder there is unrelated) and a QA checklist on
   the tracker card: plain English = not internationalized; visible `{0}` = broken placeholder;
   missing `]` = truncation; brackets mid-sentence = concatenation; clipped layout = expansion
   problem.
