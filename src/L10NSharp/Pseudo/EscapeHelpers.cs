// Adapted from the MIT-licensed PseudoLocalizer project, Copyright (C) 2012, Anders Kaplan.
// See README.md in this folder for provenance, license, and local changes.

namespace L10NSharp.Pseudo
{
	internal static class EscapeHelpers
	{
		// Local addition (not upstream): placeholder names may be alphanumeric/underscore,
		// not just digits — consumers substitute named placeholders like "{app_title}".
		private static bool IsPlaceholderNameChar(char c)
			=> char.IsLetterOrDigit(c) || c == '_';

		internal static bool ShouldTransform(char[] array, char ch, ref int i)
		{
			// Are we at the start of a potential placeholder (e.g. "{?...}")
			if (ch == '{' && i < array.Length - 2)
			{
				int j = i;

				while (j < array.Length - 1 && IsPlaceholderNameChar(array[++j]))
				{
					// Consume the placeholder name (digits for "{0}", or a name like "{lang}")
				}

				if (array[j] == ',')
				{
					// Local addition (not upstream): consume an alignment segment (e.g. "{0,-10}"
					// or "{0, 10}"; .NET allows whitespace around the alignment)
					while (j < array.Length - 1 && (array[j + 1] == '-' || char.IsDigit(array[j + 1])
						|| char.IsWhiteSpace(array[j + 1])))
						j++;
					if (j < array.Length - 1)
						j++;
				}

				if (array[j] == ':')
				{
					while (j < array.Length - 1 && array[++j] != '}')
					{
						// Consume all of any format specifier (e.g. "{0:yyyy}" for a DateTime)
					}
				}

				if (array[j] == '}')
				{
					i = j;
					return false;
				}
			}
			else if (ch == '%' && i < array.Length - 1 && char.IsDigit(array[i + 1]))
			{
				// Local addition (not upstream): "%0"-style placeholders, substituted by
				// consumers' front ends (e.g. Bloom's simpleFormat), pass through untouched.
				int j = i;

				while (j < array.Length - 1 && char.IsDigit(array[j + 1]))
					j++;

				i = j;
				return false;
			}
			else if (ch == '<' && i < array.Length - 2)
			{
				// Are we at the start of a potential HTML tag (e.g. "<a/>")
				int j = i;

				char next = array[i + 1];

				if ((next >= 'a' && next <= 'z') || (next >= 'A' && next <= 'Z') || next == '/')
				{
					while (j < array.Length - 1 && array[++j] != '>')
					{
						// Consume all of the tag
					}

					if (array[j] == '>')
					{
						i = j;
						return false;
					}
				}
			}

			return true;
		}
	}
}
