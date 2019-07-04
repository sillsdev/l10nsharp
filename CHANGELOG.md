# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

<!-- Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
-->

## [Unreleased]

### Added

- create symbol nuget package

### Fixed

- Find TMX files in `Generated` and `User Modified` directories

## [4.0.0] - 2019-05-16

### Changed

- Allow to select translation memory (TMX or XLIFF). This changed a few APIs.
  To create a `LocalizationManager` you now pass a `TranslationMemory` parameter
  (cf. [migration](https://github.com/sillsdev/l10nsharp/wiki/Migration) guide):

      LocalizationManager.Create(TranslationMemory.XLiff, lang, "SampleApp", "SampleApp",
        Application.ProductVersion, directoryOfInstalledXliffFiles, "MyCompany/L10NSharpSample",
        icon, "sample@example.com", "SampleApp");

- Nuget package is now called `L10NSharp` instead of `L10NSharp.xliff` or `L10NSharp.tmx`

## [3.1.1] - 2019-04-26

### Fixed

- Create .exe for `CheckOrFixXliff` and `ExtractXliff` instead of .dll

## [3.1.0] - 2019-04-16

### Changed

- Create nuget package
- Strong-name assembly
