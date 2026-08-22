using PseudoLocalizer.Core;

namespace L10NSharp
{
	/// <summary>
	/// Produces the pseudolocalized ("qps-ploc") form of English strings: accented substitution,
	/// expansion padding, and enclosing brackets, with .NET format placeholders and HTML/XML
	/// markup passed through untouched. The transform is deterministic.
	/// </summary>
	internal static class PseudoLocalization
	{
		private static readonly Pipeline Transformers =
			new Pipeline(Accents.Instance, ExtraLength.Instance, Brackets.Instance);

		public static string Transform(string english)
		{
			return string.IsNullOrEmpty(english) ? english : Transformers.Transform(english);
		}
	}
}
