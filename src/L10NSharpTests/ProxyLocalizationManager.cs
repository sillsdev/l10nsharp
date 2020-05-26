// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace L10NSharp.Tests
{
	class ProxyLocalizationManager
	{
		public static string MyOwnGetString(string id, string english, string comment)
		{
			return LocalizationManager.GetString(id, english, comment);
		}
	}
}
