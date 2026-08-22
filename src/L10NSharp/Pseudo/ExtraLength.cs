// Vendored from the MIT-licensed PseudoLocalizer project, Copyright (C) 2012, Anders Kaplan.
// See README.md in this folder for provenance, license, and local changes.

using System.Linq;
using System.Text;

namespace L10NSharp.Pseudo
{
	/// <summary>
	/// A transform which makes all words approximately one third longer.
	/// </summary>
	internal static class ExtraLength
	{
		private const char LengthenCharacter = 'x';

		public static string Transform(string value)
		{
			// Slower path to not break formatting strings by removing their digits or break HTML tags
			if (EscapeHelpers.MayNeedEscaping(value))
			{
				var src = value.ToCharArray();

				var builder = new StringBuilder(value.Length * 2);
				var current = new StringBuilder(value.Length);

				for (int i = 0; i < value.Length; i++)
				{
					char ch = value[i];
					int indexBefore = i;

					if (EscapeHelpers.ShouldTransform(src, ch, ref i))
					{
						current.Append(ch);
					}
					else
					{
						// Transformation should be skipped due to formatting placeholder or HTML
						if (current.Length > 0)
						{
							builder.Append(Lengthen(current));
							current.Clear();
						}

						// Add the skipped range
						for (int j = indexBefore; j < i + 1; j++)
							builder.Append(value[j]);
					}
				}

				if (current.Length > 0)
					builder.Append(Lengthen(current));

				return builder.ToString();
			}

			return string.Join(" ", value.Split(' ').Select(Lengthen));
		}

		private static string Lengthen(StringBuilder builder)
			=> string.Join(" ", builder.ToString().Split(' ').Select(Lengthen));

		private static string Lengthen(string word)
		{
			var count = (word.Length + 2) / 3;
			return word + new string(LengthenCharacter, count);
		}
	}
}
