using System.Collections.Generic;
using System.Text;

namespace L10NSharp.Pseudo
{
	/// <summary>
	/// Doubles every vowel, accenting the first of each pair, and leaves consonants,
	/// digits, and punctuation untouched: "Title Missing" becomes "Tîitlée Mîissîing".
	/// This keeps the pseudo text easy to read while still being unmistakably transformed
	/// (no real language systematically produces the accented-plain vowel pairs), and it
	/// provides the ~30-40% expansion inside the words themselves, with no filler
	/// characters. Format placeholders and HTML/XML tags pass through untouched
	/// (via EscapeHelpers).
	/// </summary>
	internal static class VowelStretch
	{
		private static readonly Dictionary<char, char> AccentedVowels = new Dictionary<char, char>
		{
			{ 'a', 'å' },
			{ 'e', 'é' },
			{ 'i', 'î' },
			{ 'o', 'ö' },
			{ 'u', 'û' },
			{ 'A', 'Å' },
			{ 'E', 'É' },
			{ 'I', 'Î' },
			{ 'O', 'Ö' },
			{ 'U', 'Û' },
		};

		public static string Transform(string value)
		{
			var array = value.ToCharArray();
			var builder = new StringBuilder(value.Length * 2);

			for (int i = 0; i < array.Length; i++)
			{
				char ch = array[i];
				int indexBefore = i;

				if (EscapeHelpers.ShouldTransform(array, ch, ref i))
				{
					if (AccentedVowels.TryGetValue(ch, out var accented))
					{
						// Each vowel doubles for expansion, accented on the first of the
						// pair so the text stays easy to read.
						builder.Append(accented);
						builder.Append(ch);
					}
					else
					{
						builder.Append(ch);
					}
				}
				else
				{
					// Skipped span (placeholder or markup): copy it through untouched.
					for (int j = indexBefore; j < i + 1; j++)
						builder.Append(array[j]);
				}
			}

			return builder.ToString();
		}
	}
}
