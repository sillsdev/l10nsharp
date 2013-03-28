using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Localization
{
	public static class LocalizationExtensions
	{
		public static string Localize(this string s, string separateId="", string comment="")
		{
			if (string.IsNullOrEmpty(separateId))
				separateId = s;
			return LocalizationManager.GetString(separateId, s, comment);
		}
	}
}
