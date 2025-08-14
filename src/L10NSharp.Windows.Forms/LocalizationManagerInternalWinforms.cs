// Copyright © 2022-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using L10NSharp.Windows.Forms.XLiffUtils;
using L10NSharp.Windows.Forms.UIComponents;
// ReSharper disable StaticMemberInGenericType - these static fields are parameter-independent

namespace L10NSharp.Windows.Forms
{
	internal class LocalizationManagerInternalWinforms<T> : LocalizationManagerInternal<T>
	{

		/// <summary>
		/// Function to choose a fallback language during construction. Overridable by unit tests.
		/// </summary>
		internal static Func<string, Icon, string> ChooseFallbackLanguageWinforms = (desiredUiLangId, icon) =>
		{
			using (var dlg = new LanguageChoosingDialog(L10NCultureInfo.GetCultureInfo(desiredUiLangId), icon))
			{
				dlg.ShowDialog();
				return dlg.SelectedLanguage;
			}
		};

		#region Static methods for creating a LocalizationManagerInternal
		private static ILocalizationManager Create(string desiredUiLangId, string appId,
			string appName, string relativeSettingPathForLocalizationFolder,
			Icon applicationIcon, Func<string, ILocalizationManagerInternalWinforms<T>> createMethod)
		{
			if (string.IsNullOrEmpty(relativeSettingPathForLocalizationFolder))
				relativeSettingPathForLocalizationFolder = appName;
			else if (Path.IsPathRooted(relativeSettingPathForLocalizationFolder))
				throw new ArgumentException(@"Relative (non-rooted) path expected", nameof(relativeSettingPathForLocalizationFolder));

			var directoryOfWritableTranslationFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				relativeSettingPathForLocalizationFolder, "localizations");

			if (!LoadedManagers.TryGetValue(appId, out var lm))
			{
				lm = createMethod(directoryOfWritableTranslationFiles);

				LoadedManagers[appId] = lm;
				PreviouslyLoadedManagers.Remove(appId);
			}

			if (lm is ILocalizationManagerInternalWinforms<T>)
				((ILocalizationManagerInternalWinforms<T>)lm).ApplicationIcon = applicationIcon;

			if (string.IsNullOrEmpty(desiredUiLangId))
			{
				desiredUiLangId = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
			}

			if (!LocalizationManagerInternalWinforms<T>.IsDesiredUiCultureAvailable(desiredUiLangId))
			{
				desiredUiLangId = ChooseFallbackLanguageWinforms(desiredUiLangId, applicationIcon);
			}

			L10NSharp.LocalizationManager.SetUILanguage(desiredUiLangId, false);

			L10NSharp.LocalizationManager.EnableClickingOnControlToBringUpLocalizationDialog = true;

			return lm;
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
		/// <param name="directoryOfInstalledXliffFiles">The full folder path of the original Xliff files
		/// installed with the application.</param>
		/// <param name="relativeSettingPathForLocalizationFolder">The path, relative to
		/// %appdata%, where your application stores user settings (e.g., "SIL\SayMore").
		/// A folder named "localizations" will be created there.</param>
		/// <param name="applicationIcon"> </param>
		/// <param name="additionalLocalizationMethods">MethodInfo objects representing
		/// additional methods that should be regarded as calls to get localizations. If the method
		/// is named "Localize", the extractor will attempt to parse its signature as an extension
		/// method with the parameters (this string s, string separateId="", string comment="").
		/// Otherwise, it will be treated like a L10nSharp GetString method if its signature
		/// matches one of the following: (string stringId, string englishText),
		/// (string stringId, string englishText, string comment), or
		/// (string stringId, string englishText, string comment, string englishToolTipText,
		/// string englishShortcutKey, IComponent component).</param>
		/// <param name="namespaceBeginnings">A list of namespace beginnings indicating
		/// what types to scan for localized string calls. For example, to only scan
		/// types found in Pa.exe and assuming all types in that assembly begin with
		/// 'Pa', then this value would only contain the string 'Pa'.</param>
		/// ------------------------------------------------------------------------------------
		public static ILocalizationManager CreateXliff(string desiredUiLangId, string appId,
			string appName, string appVersion, string directoryOfInstalledXliffFiles,
			string relativeSettingPathForLocalizationFolder,
			Icon applicationIcon,
			IEnumerable<MethodInfo> additionalLocalizationMethods,
			params string[] namespaceBeginnings)
		{
			if (string.IsNullOrWhiteSpace(appId))
				throw new ArgumentNullException(nameof(appId));
			var origExeExtension = Path.GetExtension(appId);
			if (origExeExtension == string.Empty)
				origExeExtension = ".dll";
			appId = Path.GetFileNameWithoutExtension(appId);

			return Create(desiredUiLangId, appId, appName,
				relativeSettingPathForLocalizationFolder, applicationIcon,
				directoryOfWritableXliffFiles =>
					(ILocalizationManagerInternalWinforms<T>)new XliffLocalizationManagerWinforms(appId, origExeExtension, appName,
						appVersion, directoryOfInstalledXliffFiles,
						directoryOfWritableXliffFiles, directoryOfWritableXliffFiles,
						additionalLocalizationMethods,
						namespaceBeginnings));
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
		/// <param name="directoryOfInstalledXliffFiles">The full folder path of the original Xliff files
		/// installed with the application.</param>
		/// <param name="relativeSettingPathForLocalizationFolder">The path, relative to
		/// %appdata%, where your application stores user settings (e.g., "SIL\SayMore").
		/// A folder named "localizations" will be created there.</param>
		/// <param name="applicationIcon"> </param>
		/// <param name="additionalLocalizationMethods">MethodInfo objects representing
		/// additional methods that should be regarded as calls to get localizations. If the method
		/// is named "Localize", the extractor will attempt to parse its signature as an extension
		/// method with the parameters (this string s, string separateId="", string comment="").
		/// Otherwise, it will be treated like a L10nSharp GetString method if its signature
		/// matches one of the following: (string stringId, string englishText),
		/// (string stringId, string englishText, string comment), or
		/// (string stringId, string englishText, string comment, string englishToolTipText,
		/// string englishShortcutKey, IComponent component).</param>
		/// <param name="namespaceBeginnings">A list of namespace beginnings indicating
		/// what types to scan for localized string calls. For example, to only scan
		/// types found in Pa.exe and assuming all types in that assembly begin with
		/// 'Pa', then this value would only contain the string 'Pa'.</param>
		/// ------------------------------------------------------------------------------------
		public static ILocalizationManager CreateXliff(string desiredUiLangId, string appId,
			string appName, string appVersion, string directoryOfInstalledXliffFiles,
			string relativeSettingPathForLocalizationFolder,
			IEnumerable<MethodInfo> additionalLocalizationMethods,
			params string[] namespaceBeginnings)
		{
			if (string.IsNullOrWhiteSpace(appId))
				throw new ArgumentNullException(nameof(appId));
			var origExeExtension = Path.GetExtension(appId);
			if (origExeExtension == string.Empty)
				origExeExtension = ".dll";
			appId = Path.GetFileNameWithoutExtension(appId);

			return Create(desiredUiLangId, appId, appName,
				relativeSettingPathForLocalizationFolder, 
				directoryOfWritableXliffFiles =>
					(ILocalizationManagerInternalWinforms<T>)new XliffLocalizationManagerWinforms(appId, origExeExtension, appName,
						appVersion, directoryOfInstalledXliffFiles,
						directoryOfWritableXliffFiles, directoryOfWritableXliffFiles,
						additionalLocalizationMethods,
						namespaceBeginnings));
		}

		/// ------------------------------------------------------------------------------------
		internal static ILocalizationManagerInternalWinforms<T> GetLocalizationManagerForComponentWinforms(
			IComponent component)
		{
			return (ILocalizationManagerInternalWinforms<T>)LoadedManagers.Values.FirstOrDefault(lm => lm.ComponentCache.ContainsKey(component));
		}

		/// ------------------------------------------------------------------------------------
		internal static ILocalizationManagerInternalWinforms<T> GetLocalizationManagerForStringWinforms(string id)
		{
			return (ILocalizationManagerInternalWinforms<T>)LoadedManagers.Values.FirstOrDefault(
				lm => lm.StringCache.GetString(LocalizationManager.UILanguageId, id) != null);
		}

		#endregion
		/// ------------------------------------------------------------------------------------
		public static string GetLocalizedToolTipForControl(Control ctrl)
		{
			var lm = LocalizationManagerInternalWinforms<T>.GetLocalizationManagerForComponentWinforms(ctrl);
			var topCtrl = GetRealTopLevelControl(ctrl);
			if (topCtrl == null || lm == null)
				return null;

			return lm.ToolTipCtrls.TryGetValue(topCtrl, out var ttctrl) ? ttctrl.GetToolTip(ctrl)
			 : null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the real top level control (using the control's TopLevelControl property
		/// seems to return null until the control is on a form).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static Control GetRealTopLevelControl(Control ctrl)
		{
			var parentControl = ctrl;
			while (parentControl.Parent != null)
				parentControl = parentControl.Parent;

			return parentControl;
		}
	}
}
