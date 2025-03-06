// Copyright © 2019-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace L10NSharp
{
	internal class LocalizedStringCache
	{
		internal const string kToolTipSuffix  = "_ToolTip_";
		internal const string kShortcutSuffix = "_ShortcutKeys_";

		// Cannot use Environment.NewLine because that also includes a carriage return
		// character which, when included, messes up the display of text in controls.
		internal const string kOSRealNewline = "\n";

		internal const  string kDefaultAmpersandReplacement = "|amp|";
		internal static string _ampersandReplacement        = kDefaultAmpersandReplacement;

		// This is the symbol for a newline that users put in their localized text when
		// they want a real newline inserted. The program will replace literal newlines
		// with the value of kOSNewline.
		internal const  string kDefaultNewlineReplacement = "\\n";
		internal static string s_literalNewline           = kDefaultNewlineReplacement;

	}
}
