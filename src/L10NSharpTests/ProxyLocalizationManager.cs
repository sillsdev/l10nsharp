// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.ComponentModel;

namespace L10NSharp.Tests
{
	public class ProxyLocalizationManager
	{
		public static string MyOwnGetString(string id, string english)
		{
			return LocalizationManager.GetString(id, english);
		}

		public static string MyOwnGetString(string id, string english, string comment)
		{
			return LocalizationManager.GetString(id, english, comment);
		}

		public static string MyOwnGetString(string id, string english, string comment,
			string englishToolTipText, string englishShortcutKey, IComponent component)
		{
			return LocalizationManager.GetString(id, english, comment, englishToolTipText,
				englishShortcutKey, component);
		}
	}

	public static class ProxyLocalizationStringExtensions
	{
		public static string Localize(this string s, string separateId="", string comment="")
		{
			return L10NStringExtensions.Localize(s, separateId, comment);
		}
	}
}
