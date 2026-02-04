// Copyright © 2022-2026 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace L10NSharp
{
	public interface ILocalizationManager: IDisposable
	{
		event EventHandler UiLanguageChanged;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This is what identifies a localization manager for a particular set of
		/// localized strings. This would likely be a DLL or EXE name like 'PA' or 'SayMore'.
		/// This will be the file name of the portion of the XLIFF file in which localized
		/// strings are stored. This would usually be the name of the assembly that owns a
		/// set of localized strings.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		string Id { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This is the presentable name for the set of localized strings. For example, the
		/// ID might be 'PA' but the LocalizationSetName might be 'Phonology Assistant'.
		/// This should be a name presentable to the user.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		string Name { get; }

		/// <summary>
		/// Set this to false if you don't want users to pollute l10n files they might send
		/// to you with strings that are unique to their documents. For example, Bloom looks for
		/// strings in html that might have been localized; but Bloom doesn't want to ship an
		/// ever-growing list of discovered strings for people to translate that aren't actually
		/// part of what you get with Bloom. So it sets this to <c>false</c> unless the app was
		/// compiled in DEBUG mode. Default is <c>true</c>.
		/// </summary>
		bool CollectUpNewStringsDiscoveredDynamically { get; set; }

		/// <summary>
		/// Return the language tags for those languages that have been localized for the given
		/// program.
		/// </summary>
		IEnumerable<string> GetAvailableUILanguageTags();

		bool IsUILanguageAvailable(string langId);

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Although L10nSharp no longer provides a mechanism by which users may customize
		/// localizations, it does still allow for their existence, although it is unlikely.
		/// This both provides for backwards compatibility and allows for the possibility of
		/// clients that may provide a mechanism for users to customize localizations.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		bool DoesCustomizedTranslationExistForLanguage(string langId);

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Add a localized string to the string cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		void AddString(string id, string defaultText, string defaultTooltip,
			string defaultShortcutKeys, string comment);

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized text for the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		string GetLocalizedString(IComponent component, string id, string defaultText,
			string defaultTooltip, string defaultShortcutKeys, string comment);

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized text for the specified id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		string GetLocalizedString(string id, string defaultText);
	}
}
