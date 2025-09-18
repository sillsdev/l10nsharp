// // Copyright © 2019-2025 SIL Global
// // This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;

namespace L10NSharp
{
	internal interface ILocalizedStringCache<T>
	{
		bool TryGetDocument(string langId, out T doc);
		IEnumerable<string> AvailableLangKeys { get; }
		string GetString(string langId, string id);
		string GetString(string langId, string id, bool formatForDisplay);
		string GetToolTipText(string langId, string id);
		string GetToolTipText(string langId, string id, bool formatForDisplay);
		string GetShortcutKeysText(string langId, string id);
		string GetComment(string id);
		string GetValueForExactLangAndId(string langId, string id, bool formatForDisplay);

		void UpdateLocalizedInfo(LocalizingInfo locInfo);

		int NumberApproved(string lang);
		int NumberTranslated(string lang);
		int StringCount(string lang);

		bool DoTranslationsExist(string langId, string id);
	}
}
