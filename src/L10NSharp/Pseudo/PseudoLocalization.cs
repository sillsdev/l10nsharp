namespace L10NSharp.Pseudo
{
	/// <summary>
	/// Produces the pseudolocalized ("qps-ploc") form of English strings: accented substitution,
	/// expansion padding, and enclosing brackets, with .NET format placeholders and HTML/XML
	/// markup passed through untouched. The transform is deterministic.
	/// The transforms are vendored from the MIT-licensed PseudoLocalizer project (see README.md
	/// in this folder), so L10NSharp carries no extra dependency for this feature.
	/// </summary>
	internal static class PseudoLocalization
	{
		public static string Transform(string english)
		{
			if (string.IsNullOrEmpty(english))
				return english;
			return "[" + ExtraLength.Transform(Accents.Transform(english)) + "]";
		}
	}
}
