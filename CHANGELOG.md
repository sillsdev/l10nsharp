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

- BREAKING CHANGE: Move code that depends on Windows.Forms or System.Drawing into an 		L10NSharp.Windows.Forms namespace. Rename L10NSharp.UI as L10NSharp.Windows.Forms.UIComponents. Move L10NExtender out of UI subfolder into L10NSharp.Windows.Forms. Move Winforms related tests to L10NSharp.Windows.Forms.Tests. Change the folder for L10NSharp tests to match its namespace L10NSharp.Tests.

   Classes that contain some properties or methods that depend on Windows.Forms are split into Winforms-dependent and Winforms-independent classes. The Winforms-dependent classes subclass the Winforms-independent ones and can be found in the L10NSharp.Windows.Forms namespace. (e.g. LocalizationManagerWinforms in the L10NSharp.Windows.Forms namespace is a subclass of LocalizationManager in the L10NSharp namespace.)

   To handle Windows forms related objects, call the Winforms versions of these classes and methods (e.g. LocalizationManagerWinforms.Create() instead of LocalizationManager.Create()). Each affected interface or class and its affected properties and methods are listed below.

  - Split ILocalizationManagerInternal into ILocalizationManagerInternal and ILocalizationManagerInternalWinforms.

    MOVED: The properties ToolTipCtrls, LocalizableComponents, and ApplicationIcon; and the method RegisterComponentForLocalizing are moved to ILocalizationManagerInternalWinforms.

  - Split ILocalizedStringCache into ILocalizedStringCache and ILocalizedStringCacheWinforms.

    MOVED: The property LeafNodeList, and the methods GetShortcutKeys and LoadGroupNodes are moved to ILocalizedStringCacheWinforms.

  - Split LocalizationManager into LocalizationManager and LocalizationManagerWinforms.

    CHANGED: Remove static designation from LocalizationManager class in order for LocalizationManagerWinforms to subclass it and share its properties.

    MOVED: The method GetLocalisedToolTipForControl as well as the non-obsolete Create methods that contain an Icon argument are moved to LocalizationManagerWinforms. (The two obsolete Create methods, which included a TranslationMemory argument are removed.)

  - Split LocalizationManagerInternal into LocalizationManagerInternal and LocalizationManagerInternalWinforms.

    CHANGED: Remove static designation from LocalizationManagerInternal class in order for LocalizationManagerInternalWinforms to subclass it and share its s_loadedManagers property.

    CHANGED: Use different handling for ChooseFallbackLanguage in LocalizationManagerInternal that omits use of a Windows forms dialog for choosing the fallback. Retain original Winforms-dependent handling for ChooseFallbackLanguage in LocalizationManagerInternalWinforms.

    CHANGED: Use different handling for GetString in LocalizationManagerInternal that omits handling of Winforms objects and methods. Retain original Winforms-dependent handling of GetString in LocalizationManagerInternalWinforms.

    MOVED: The methods GetLocalizationManagerForComponent, GetLocalizationManagerForString, GetLocalizedToolTipForControl, GetRealTopLevelControl, and the non-deprecated Create methods with an Icon argument are moved to LocalizationManagerInternalWinforms. Deprecated Create methods are retained in LocalizationManagerWinforms with the Icon argument removed.

  - Split LocalizingInfo into LocalizingInfo and LocalizingInfoWinforms.

    CHANGED: Make private properties protected. LocalizingInfo returns null for Id while LocalizingInfoWinforms retains method to make an Id from a winforms component.

    MOVED: The get methods for ShortcutKeys and Id properties are moved to LocalizingInfoWinforms, since they involve winforms components; LocalizingInfo will return null for ShortcutKeys and Id. The methods UpdateTextFromObject, CreateIdIfMissing, MakeId, MakeIdForCtrl, MakeIdForColumnHeader, MakeIdForDataGridViewColumn, GetIdPrefix, OwningFormName and GetCategory are moved to LocalizingInfoWinforms

  - Split Utils into Utils and UtilsWinforms.

    MOVED: The methods SendMessage, SendMessageWindows and SetWindowRedraw are moved to UtilsWinforms.

  - Split XliffLocalizationManager into XliffLocalizationManager and XliffLocalizationManagerWinforms.
 
    MOVED: The following are moved to XliffLocalizationManagerWinforms:

    - The properties ApplicationIcon, ToolTipCtrls, LocalizableComponents and StringCache.
    - The methods RegisterComponentForLocalizing, GetShortcutKeyFromStringCache, ApplyLocalizationToIlocalizableComponent, ReapplyLocalizationsToAllComponents, RefreshTooltips, ApplyLocalization, ApplyLocalizationsToILocalizableComponent, ApplyLocalizationsToControl, ApplyLocalizedToolTipToControl, HandleToolTipRefChanged, HandleToolTipRefDestroyed, ApplyLocalizationsToToolStripItem, ApplyLocalizationToListViewColumnHeader, and ApplyLocalizationToDataGridViewColumn.

  - Split XliffLocalizedStringCache into XliffLocalizedStringCacheWinforms and XliffLocalizedStringCache.

    MOVED: The LeafNodeList property and the methods LoadGroupNodes and GetShortcutKeys are moved to XliffLocalizedStringCacheWinforms.
   
### Removed

- BREAKING CHANGE: Remove code related to doing one's own localization at runtime. Also remove obsolete create methods from LocalizationManager.

    In particular:

    - Remove the LocalizeItemDlg designer, cs, resx, and viewmodel. 
    - Remove ShowLocalizationDialogBox from LocalizationManager and LocalizationManagerInternal. 
    - Remove the following runtime-localization related methods from XliffLocalizationManager:
    PrepareComponentForRuntimeLocalization, HandleToolStripItemMouseDown, DoHandleMouseDown, HandeToolStripItemDisposed, HandleControlMouseDouwn, HandleControlDisposed, HandleTabPageDisposed, HandleDataGridViewDisposed, HandleListViewColumnHeaderClicked, HandleListViewDisposed, HandleListViewColumnDisposed, HandleDataGridViewCellMouseDown, HandleColumnDisposed, and ShowLocalizationDialogBox.
    - Remove obsolete Create methods from LocalizationManager. These are the two Create methods that included a TranslationMemory argument.


## [8.0.0] - 2025-03-12

### Changed

-   BREAKING CHANGE: If no `LocalizationManager`s have been created, but the client asks for a string to be localized, an `InvalidOperationException` is thrown. This is to prevent an invalid state where language IDs get mapped incorrectly at the beginning and then never get updated which can cause us to fail to return properly localized strings when requested (see BL-13245). This is a breaking change because it may cause existing code to throw an exception. The fix is to ensure that a LocalizationManager is created before calling any localization methods. Or, to maintain existing behavior, set `LocalizationManager.StrictInitializationMode` to false.
-   BREAKING CHANGE: Changed the signature of StringExtractor.DoExtractingWork to return an IReadOnlyList instead of an IEnumerable. It is doubtful that anything outside L10nSharp is actually using this, but it is a breaking change because it may cause existing code to fail to compile. The fix is to change the type of the variable that receives the return value to IReadOnlyList.
  - Reduced the number of different types of exceptions likely to be thrown as a result of passing an invalid appVersion to the XLiffLocalizationManager constructor. Now, if the appVersion is invalid, an ArgumentException is thrown and the original exception from the call to Version.Parse is the internal exception. This is a technically a breaking change because if any existing code was catching the more specific exception types, the logic would need to be changed to catch only the ArgumentException and then do more specific processing based on the type of the inner exception. But since the details of the typesof exceptions was never explicitly documented and since applications would not be likely to be able to recover from such exceptions, it's probably very unlikely that any code was actually handling these exceptions in this way.

## [7.0.0] - 2023-11-03

### Added

-   `LocalizationManager.Create` methods without `TranslationMemory kind` parameter

### Fixed

-   `LocalizationManager.Create("es"` loads `es-ES` if it is the best match (previously, this resulted in a dialog making the user choose)

### Deprecated

-   `LocalizationManager.Create` methods with `TranslationMemory kind` parameter

## [6.0.0] - 2022-11-21

### Changed

-   Added cleanUpTmx parameter to LocalizationManager.DeleteOldTranslationFiles to allow for cleanup of old TMX files.

### Removed

-   TMX-based localization no longer supported
-   LocalizationManager.GetTranslationFileNameForLanguage is no longer public.

## [5.0.0] - 2022-07-08

### Added

-   option `LocalizationManager.ThrowIfManagerDisposed` to not throw if LM disposed (BL-9904)
-   XliffBody.TransUnitsUnordered for where you just need to enumerate all of them.
-   (Made public) XliffBody.AddTransUnit and .RemoveTransUnit for where you need to modify.
-   XliffBody.TransUnitsForXml. This is necessarily public to support (backwards-compatible)
    serialization and deserialization in XML, but is not intended for any other purpose.

### Changed

-   Scanning resources for strings no longer rethrows unexpected exceptions. It now writes the
    exception (and stack trace) using a (conditional) Console.WriteLine and a Debug.WriteLine.
    Rethrowing the exception leads to creating a zero-length xliff file which causes another
    exception. Swallowing the exception allows the scanning process to continue and complete.
    The old behavior has been an endless source of periodic instability in using L10NSharp over
    the years.
-   remove progress dialog when initializating Xliff localization managers (BL-11157)
-   Made string retrieval operations on Xliff-based LocalizationManagers thread-safe
-   Added ILocalizationManager parameter to StringsLocalizedHandler
-   It's long been a convention that xliff file names are module.lang.xlf (e.g., Bloom.fr.xlf)
    or else kept in language-code folders (.../en/Bloom.xlf) if UseLanguageCodeFolders is set.
    With the latest changes, this is required: the language name indicated in these ways in the file
    name must match the language declared in the target-language attribute, or at least match the
    first element of the target-language (e.g., a file with target-languge es-ES may be stored in
    file like Bloom.es.xlf or .../es/Bloom.xlf).
-   Progress dialogs are no longer shown when initializing XLIFF-based LocalizationManagers.
-   Made it possible for caller to specify the file extension of the "original" executable file
    when constructing an XLIFF-based LocalizationManager.
-   Changed the way the "original" attribute is set in XLIFF files. It used to be based on the
    Name, but changed it to use Id instead.
-   Added optional owner parameter to methods that show dialog boxes so that they can be
    displayed centered on a parent window (and not appear off-screen).

### Removed

-   XliffBody.TransUnits, as there is no good way to make this thread-safe for all the ways
    it could be used, such as adding items to the list. (See Added for replacements.)

## [4.1.0] - 2021-03-04

### Changed

-   Add `ExtractXliff` tool as nuget package
-   Add `CheckOrFixXliff` tool as nuget package
-   Added version of LocalizationManager.Create to allow "custom" localization methods
-   Added -m switch to ExtractXliff command-line to allow caller to pass additional string-localization methods

## [4.0.3] - 2020-01-21

### Changed

-   Add build number to AssemblyFileVersion

## [4.0.2] - 2019-07-09

### Fixed

-   If translator returns an unmodified source string, don't substitute the English language name for the vernacular name.

## [4.0.1] - 2019-07-08

### Added

-   create symbol nuget package

### Fixed

-   Find TMX files in `Generated` and `User Modified` directories

-   Don't ask Bing translator to translate language names: https://github.com/sillsdev/l10nsharp/issues/66.
    Also don't display the name a second time in parentheses if English and native name are identical.

## [4.0.0] - 2019-05-16

### Changed

-   Allow to select translation memory (TMX or XLIFF). This changed a few APIs.
    To create a `LocalizationManager` you now pass a `TranslationMemory` parameter
    (cf. [migration](https://github.com/sillsdev/l10nsharp/wiki/Migration) guide):

        LocalizationManager.Create(TranslationMemory.XLiff, lang, "SampleApp", "SampleApp",
          Application.ProductVersion, directoryOfInstalledXliffFiles, "MyCompany/L10NSharpSample",
          icon, "sample@example.com", "SampleApp");

-   Nuget package is now called `L10NSharp` instead of `L10NSharp.xliff` or `L10NSharp.tmx`

## [3.1.1] - 2019-04-26

### Fixed

-   Create .exe for `CheckOrFixXliff` and `ExtractXliff` instead of .dll

## [3.1.0] - 2019-04-16

### Changed

-   Create nuget package
-   Strong-name assembly
