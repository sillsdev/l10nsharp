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

### Changed

- Allow to select translation memory (TMX or XLIFF). This changed a few APIs.
  To create a `LocalizationManager` you now pass a `TranslationMemory` parameter:
  
      LocalizationManager.Create(TranslationMemory.XLiff, lang, "SampleApp", "SampleApp",
        Application.ProductVersion, directoryOfInstalledXliffFiles, "MyCompany/L10NSharpSample",
        icon, "sample@example.com", "SampleApp");

## [3.1.1] - 2019-04-26

### Fixed

- Create .exe for `CheckOrFixXliff` and `ExtractXliff` instead of .dll

## [3.1.0] - 2019-04-16

### Changed

- Create nuget package
- Strong-name assembly
