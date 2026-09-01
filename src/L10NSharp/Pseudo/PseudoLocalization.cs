namespace L10NSharp.Pseudo
{
	/// <summary>
	/// Produces the pseudolocalized ("qps-ploc") form of English strings: every vowel is
	/// doubled (accented on the first of the pair) for ~30-40% expansion, and the whole
	/// string is bracketed, with format placeholders and HTML/XML markup passed through
	/// untouched. E.g. "Title Missing" becomes "[Tîitlée Mîissîing]". The transform is
	/// deterministic. See README.md in this folder.
	/// </summary>
	internal static class PseudoLocalization
	{
		public static string Transform(string english)
		{
			if (string.IsNullOrEmpty(english))
				return english;
			return "[" + VowelStretch.Transform(english) + "]";
		}
	}
}
