namespace L10NSharp
{
	public static class L10NStringExtensions
	{
		public static string Localize(this string s, string separateId="", string comment="")
		{
			if (string.IsNullOrEmpty(separateId))
				separateId = s;
			return LocalizationManager.GetString(separateId, s, comment);
		}
	}
}
