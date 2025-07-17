// Copyright © 2022-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Windows.Forms;
using L10NSharp.XLiffUtils;
using L10NSharp;

namespace L10NSharpWinforms
{
	public static class LocalizationManagerWinforms
	{

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
		internal static Dictionary<string, ILocalizationManagerInternalWinforms> LoadedManagers
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
							loadedManagers.Add(keyValuePair.Key, keyValuePair.Value);
						}

						return loadedManagers;
					}
				}
			}
		}

		/// <summary>
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
		public static bool StrictInitializationMode = true;
	}
}
