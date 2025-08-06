// Copyright © 2022-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Windows.Forms;
using L10NSharp.XLiffUtils;
using L10NSharp;
using System.Reflection;
using System.Drawing;

namespace L10NSharp.Windows.Forms
{
	public class LocalizationManagerWinforms : LocalizationManager
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a new instance of a localization manager for the specified application id.
		/// If a localization manager has already been created for the specified id, then
		/// that is returned.
		/// </summary>
		/// <param name="desiredUiLangId">The language code of the desired UI language. If
		/// there are no translations for that ID, a message is displayed and the UI language
		/// falls back to the default.</param>
		/// <param name="appId">The application Id (e.g. 'Pa' for Phonology Assistant).
		/// This should be a unique name that identifies the manager for an assembly or
		/// application. May include an optional file extension, which will be stripped off but
		/// used to correctly set the "original" attribute when persisting an XLIFF file. The
		/// base portion must still be unique (i.e., it is not valid to create a LM for
		/// "Blah.exe" and another for "Blah.dll").</param>
		/// <param name="appName">The application's name. This will appear to the user
		/// in the localization dialog box as a parent item in the tree.</param>
		/// <param name="appVersion"></param>
		/// <param name="directoryOfInstalledFiles">The full folder path of the original l10n
		/// files installed with the application.</param>
		/// <param name="relativeSettingPathForLocalizationFolder">The path, relative to
		/// %localappdata%, where your application stores user settings (e.g., "SIL\SayMore").
		/// A folder named "localizations" will be created there.</param>
		/// <param name="applicationIcon"> </param>
		/// <param name="emailForSubmissions">This will be used in UI that helps the translator
		/// know what to do with their work</param>
		/// <param name="namespaceBeginnings">A list of namespace beginnings indicating
		/// what types to scan for localized string calls. For example, to only scan
		/// types found in Pa.exe and assuming all types in that assembly begin with
		/// 'Pa', then this value would only contain the string 'Pa'.</param>
		/// <param name="additionalLocalizationMethods">MethodInfo objects representing
		/// additional methods that should be regarded as calls to get localizations. If the method
		/// is named "Localize", the extractor will attempt to parse its signature as an extension
		/// method with the parameters (this string s, string separateId="", string comment="").
		/// Otherwise, it will be treated like a L10nSharp GetString method if its signature
		/// matches one of the following: (string stringId, string englishText),
		/// (string stringId, string englishText, string comment), or
		/// (string stringId, string englishText, string comment, string englishToolTipText,
		/// string englishShortcutKey, IComponent component).</param>
		/// ------------------------------------------------------------------------------------
		public static ILocalizationManager Create(string desiredUiLangId,
			string appId, string appName, string appVersion, string directoryOfInstalledFiles,
			string relativeSettingPathForLocalizationFolder,
			Icon applicationIcon, string emailForSubmissions,
			string[] namespaceBeginnings,
			IEnumerable<MethodInfo> additionalLocalizationMethods = null)
		{
			LocalizationManager.TranslationMemoryKind = TranslationMemory.XLiff;
			LocalizationManager.EmailForSubmissions = emailForSubmissions;
			return LocalizationManagerInternalWinforms<XLiffDocument>.CreateXliff(desiredUiLangId,
				appId, appName, appVersion, directoryOfInstalledFiles,
				relativeSettingPathForLocalizationFolder, applicationIcon,
				additionalLocalizationMethods,
				namespaceBeginnings);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a new instance of a localization manager for the specified application id.
		/// If a localization manager has already been created for the specified id, then
		/// that is returned.
		/// </summary>
		/// <param name="desiredUiLangId">The language code of the desired UI language. If
		/// there are no translations for that ID, a message is displayed and the UI language
		/// falls back to the default.</param>
		/// <param name="appId">The application Id (e.g. 'Pa' for Phonology Assistant).
		/// This should be a unique name that identifies the manager for an assembly or
		/// application. May include an optional file extension, which will be stripped off but
		/// used to correctly set the "original" attribute when persisting an XLIFF file. The
		/// base portion must still be unique (i.e., it is not valid to create a LM for
		/// "Blah.exe" and another for "Blah.dll").</param>
		/// <param name="appName">The application's name. This will appear to the user
		/// in the localization dialog box as a parent item in the tree.</param>
		/// <param name="appVersion"></param>
		/// <param name="directoryOfInstalledFiles">The full folder path of the original l10n
		/// files installed with the application.</param>
		/// <param name="relativeSettingPathForLocalizationFolder">The path, relative to
		/// %localappdata%, where your application stores user settings (e.g., "SIL\SayMore").
		/// A folder named "localizations" will be created there.</param>
		/// <param name="emailForSubmissions">This will be used in UI that helps the translator
		/// know what to do with their work</param>
		/// <param name="namespaceBeginnings">A list of namespace beginnings indicating
		/// what types to scan for localized string calls. For example, to only scan
		/// types found in Pa.exe and assuming all types in that assembly begin with
		/// 'Pa', then this value would only contain the string 'Pa'.</param>
		/// <param name="additionalLocalizationMethods">MethodInfo objects representing
		/// additional methods that should be regarded as calls to get localizations. If the method
		/// is named "Localize", the extractor will attempt to parse its signature as an extension
		/// method with the parameters (this string s, string separateId="", string comment="").
		/// Otherwise, it will be treated like a L10nSharp GetString method if its signature
		/// matches one of the following: (string stringId, string englishText),
		/// (string stringId, string englishText, string comment), or
		/// (string stringId, string englishText, string comment, string englishToolTipText,
		/// string englishShortcutKey, IComponent component).</param>
		public new static ILocalizationManager Create(string desiredUiLangId,
			string appId, string appName, string appVersion, string directoryOfInstalledFiles,
			string relativeSettingPathForLocalizationFolder,
			string emailForSubmissions,
			string[] namespaceBeginnings,
			IEnumerable<MethodInfo> additionalLocalizationMethods = null)
		{
			LocalizationManager.TranslationMemoryKind = TranslationMemory.XLiff;
			LocalizationManager.EmailForSubmissions = emailForSubmissions;
			return LocalizationManagerInternalWinforms<XLiffDocument>.CreateXliff(desiredUiLangId,
				appId, appName, appVersion, directoryOfInstalledFiles,
				relativeSettingPathForLocalizationFolder, null,
				additionalLocalizationMethods,
				namespaceBeginnings);
		}

		public static string GetLocalizedToolTipForControl(Control ctrl)
		{
			switch (LocalizationManager.TranslationMemoryKind)
			{
				default:
				case TranslationMemory.XLiff:
					return LocalizationManagerInternalWinforms<XLiffDocument>.GetLocalizedToolTipForControl(ctrl);
			}
		}

		/// ------------------------------------------------------------------------------------
		internal new static Dictionary<string, ILocalizationManagerInternalWinforms> LoadedManagers
		{
			get
			{
				switch (LocalizationManager.TranslationMemoryKind)
				{
					default:
					case TranslationMemory.XLiff:
					{
						var loadedManagers = new Dictionary<string, ILocalizationManagerInternalWinforms>();
						foreach (var keyValuePair in LocalizationManagerInternalWinforms<XLiffDocument>.LoadedManagers)
						{
							if(keyValuePair.Value is ILocalizationManagerInternalWinforms)
								loadedManagers.Add(keyValuePair.Key, (ILocalizationManagerInternalWinforms)keyValuePair.Value);
						}

						return loadedManagers;
					}
				}
			}
		}

		/*/// <summary>
		/// True (default) to throw if we try to get a string from a particular manager
		/// and it has been disposed. When false, we will instead just return the English string,
		/// or if none, the ID. This is useful in some apps (e.g., Bloom) which may
		/// accidentally request a localized string during shutdown after disposing of
		/// the localization managers.
		/// </summary>
		public static bool ThrowIfManagerDisposed = true;

		/// <summary>
		/// True (default) to throw if we try to get a localized string before creating any localization managers.
		/// This is to prevent an invalid state where language IDs get mapped incorrectly at the beginning and
		/// then never get updated which can cause us to fail to return properly localized strings when requested (see BL-13245).
		/// The fix is to ensure that a LocalizationManager is created before calling any localization methods.
		/// Or, to maintain prior behavior, set this to false.
		/// </summary>
		public static bool StrictInitializationMode = true;*/
	}
}
