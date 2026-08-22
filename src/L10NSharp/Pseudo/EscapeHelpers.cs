// Vendored from the MIT-licensed PseudoLocalizer project, Copyright (C) 2012, Anders Kaplan.
// See README.md in this folder for provenance, license, and local changes.

namespace L10NSharp.Pseudo
{
	internal static class EscapeHelpers
	{
		internal static bool MayNeedEscaping(string value)
		{
			return (value.IndexOf('{') >= 0 && value.IndexOf('}') >= 0) ||
				(value.IndexOf('<') >= 0 && value.IndexOf('>') >= 0);
		}

		internal static bool ShouldTransform(char[] array, char ch, ref int i)
		{
			// Are we at the start of a potential placeholder (e.g. "{?...}")
			if (ch == '{' && i < array.Length - 2)
			{
				int j = i;

				while (j < array.Length - 1 && char.IsDigit(array[++j]))
				{
					// Consume all the digits
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
