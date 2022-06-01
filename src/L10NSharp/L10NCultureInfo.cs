using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static System.String;

namespace L10NSharp
{
	/// <summary>
	/// This class exists only to overcome the way CultureInfo is implemented to handle
	/// only those cultures known to the authors of the .Net or Mono runtime.  On Linux,
	/// trying to create a CultureInfo for an unknown culture throws an exception.  On
	/// Windows for .Net 4.6, an empty object is created that cannot be modified.  This
	/// class tries to create a CultureInfo object, and uses its values (for the most
	/// part) if successful.  If it fails, well, there are a few languages we know
	/// about that have been used for localization that we can handle in addition.
	/// </summary>
	/// <remarks>
	/// L10NCultureInfo objects will not provide any useful information for languages
	/// that are not known to Windows .Net or to Mono (typically those without 2-letter
	/// codes) and for which this class does not have any embedded information.
	/// Therefore, this class should only be used for situations where we know only
	/// a limited set of languages will occur, such as Bloom's UI languages.
	/// It should not be used where the name passed in will be an arbitrary vernacular
	/// language code.  (It won't crash, but it won't provide any information.)
	/// </remarks>
	public class L10NCultureInfo
	{
		public L10NCultureInfo(string name)
		{
			try
			{
				RawCultureInfo = CultureInfo.GetCultureInfo(name);
			}
			catch (CultureNotFoundException)
			{
				RawCultureInfo = null;
			}
			if (RawCultureInfo == null || RawCultureInfo.EnglishName.StartsWith("Unknown Language"))
			{
				Name = name;
				var idx = name.IndexOf("-", StringComparison.Ordinal);
				IsNeutralCulture = idx >= 0;
				IetfLanguageTag = IsNeutralCulture ? name : name.Substring(0, idx);

				// CultureInfo returns 3-letter tags when a 2-letter tag doesn't exist.
				// If it's a minor enough language to be unknown to .Net or Mono, it's
				// not worth trying to find a 2-letter tag that usually wouldn't exist.
				TwoLetterISOLanguageName = IetfLanguageTag;
				ThreeLetterISOLanguageName = IetfLanguageTag;
				DisplayName = GetNativeLanguageNameWithEnglishSubtitle(name);
				// REVIEW: for "pbu", EnglishName used to get set to "Northern Pashto" instead of
				// just Pashto.
				EnglishName = GetEnglishNameIfKnown(name) ?? $"Unknown Language ({name})";
				NativeName = GetNativeNameIfKnown(name) ?? EnglishName;
				switch (name)
				{
				case "pbu":
					NumberFormat = CultureInfo.GetCultureInfo("ar").NumberFormat;
					break;
				case "prs":
					NumberFormat = CultureInfo.GetCultureInfo("ar").NumberFormat;
					break;
				case "tpi":
					NumberFormat = CultureInfo.GetCultureInfo("en").NumberFormat;
					break;
				default:
					NumberFormat = CultureInfo.InvariantCulture.NumberFormat;
					break;
				}
			}
			else
			{
				InitializeFromRawCultureInfo();
				// TODO: Try to move this logic into FixBotchedNativeName
				// The Windows .Net runtime returns 'Azərbaycan dili (Azərbaycan)' for az-Latn, and
				// something totally different for az.
				// The Mono runtime returns "azərbaycan" for both az and az-Latn.
				// So we just set this to what we "know" is the right value for az.
				if (name == "az")
					NativeName = "Azərbaycan dili";
				else
					NativeName = FixBotchedNativeName(NativeName);
			}
		}

		private L10NCultureInfo(CultureInfo ci)
		{
			RawCultureInfo = ci;
			InitializeFromRawCultureInfo();
		}

		private void InitializeFromRawCultureInfo()
		{
			EnglishName = RawCultureInfo.EnglishName;
			DisplayName = RawCultureInfo.DisplayName;
			NativeName = RawCultureInfo.NativeName;
			IsNeutralCulture = RawCultureInfo.IsNeutralCulture;
			NumberFormat = RawCultureInfo.NumberFormat;
			Name = RawCultureInfo.Name;
			TwoLetterISOLanguageName = RawCultureInfo.TwoLetterISOLanguageName;
			ThreeLetterISOLanguageName = RawCultureInfo.ThreeLetterISOLanguageName;
			IetfLanguageTag = RawCultureInfo.IetfLanguageTag;
		}

		/// <summary>
		/// Provide access to the underlying CultureInfo object if it exists.
		/// </summary>
		public CultureInfo RawCultureInfo { get; }

		// The following properties mimic those provided by CultureInfo.

		public string NativeName { get; private set; }

		public string DisplayName { get; private set; }

		public string EnglishName { get; private set; }

		public bool IsNeutralCulture { get; private set; }

		public string Name { get; private set; }

		public string TwoLetterISOLanguageName { get; private set; }

		public string ThreeLetterISOLanguageName { get; private set; }

		public string IetfLanguageTag { get; private set; }

		public NumberFormatInfo NumberFormat { get; set; }

		private static L10NCultureInfo _currentInfo;
		public static L10NCultureInfo CurrentCulture
		{
			get
			{
				if (_currentInfo == null)
					_currentInfo = new L10NCultureInfo(CultureInfo.CurrentCulture);
				return _currentInfo;
			}
			internal set
			{
				_currentInfo = value;
			}
		}

		/// <summary>
		/// Gets the list of supported cultures in the form of L10NCultureInfo objects.
		/// There is some danger in calling this repeatedly in that it creates new objects,
		/// whereas the CultureInfo version appears to return cached objects.
		/// </summary>
		public static IEnumerable<L10NCultureInfo> GetCultures(CultureTypes types)
		{
			var list = CultureInfo.GetCultures(types).Select(culture => new L10NCultureInfo(culture.Name)).ToList();
			if ((types & CultureTypes.NeutralCultures) == CultureTypes.NeutralCultures)
			{
				bool havePbu = false;
				bool havePrs = false;
				bool haveTpi = false;
				foreach (var ci in list)
				{
					if (ci.Name == "pbu")
						havePbu = true;
					else if (ci.Name == "prs")
						havePrs = true;
					else if (ci.Name == "tpi")
						haveTpi = true;
				}
				if (!havePbu)
					list.Add(GetCultureInfo("pbu"));
				if (!havePrs)
					list.Add(GetCultureInfo("prs"));
				if (!haveTpi)
					list.Add(GetCultureInfo("tpi"));
			}
			return list;
		}

		/// <summary>
		/// Retrieves a new instance of a culture by using the specified culture name.
		/// The CultureInfo version of this method returns a cached read-only instance.
		/// </summary>
		public static L10NCultureInfo GetCultureInfo(string culture)
		{
			return new L10NCultureInfo(culture);
		}

		public override bool Equals(object obj)
		{
			var that = obj as L10NCultureInfo;
			if (ReferenceEquals(that, null))
				return false;
			return (that.Name == this.Name) &&
				(that.EnglishName == this.EnglishName);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode() + EnglishName.GetHashCode();
		}

		public override string ToString() =>
			$"[L10NCultureInfo: Name={Name}, EnglishName={EnglishName}]";

		public static bool operator ==(L10NCultureInfo ci1, L10NCultureInfo ci2)
		{
			if (ReferenceEquals(ci1, ci2))
				return true;
			if (ReferenceEquals(ci1, null))
				return false;
			if (ReferenceEquals(ci2, null))
				return false;
			return ci1.Equals(ci2);
		}

		// this is second one '!='
		public static bool operator !=(L10NCultureInfo ci1, L10NCultureInfo ci2)
		{
			return !(ci1 == ci2);
		}

		#region From Bloom (LanguageLookupModelExtensions) and then (in modified form) from Palaso's IetfLanguageTag class
		private static readonly ISet<string> LanguagesNotToSimplify = new HashSet<string>(new [] { "zh" });

		private static readonly Dictionary<string, string> MapIsoCodeToSubtitledLanguageName =
			new Dictionary<string, string>();

		/// <summary>
		/// Clients can set this if they have a way to possibly come up with a good name for
		/// a language code that is not known to the OS. Implementations should return null
		/// if a name cannot be found.
		/// </summary>
		/// <remarks>This should be set very early on on since any L10nCultureInfo object
		/// previously created for an unknown language will already have its display name
		/// and English names set using the ISO tag.
		/// </remarks>
		public static Func<string, string> GetLanguageNameForUnknownIsoCode { get; set; } = isoCode => null;

		/// <summary>
		/// Add a general language tag (e.g., "en") that should not be simplified to just a base
		/// language when determining the display name (i.e., when calling
		/// GetNativeLanguageNameWithEnglishSubtitle. Such languages should retain country/script/
		/// variant specific details. This is useful when caller needs to distinguish between more
		/// than one variant of a language. Note that Chinese (zh-CN/zh-TW) will always retain the
		/// distinction because it is generally important to distinguish between simplified and
		/// traditional Chinese.
		/// </summary>
		/// <remarks>Typically, calls to this method should happen very early on since any
		/// L10nCultureInfo object previously created using a tag with this language will already
		/// have its display name set using the simplified language tag.
		/// </remarks>
		public void AddLanguageNotToSimplify(string languageTag)
		{
			LanguagesNotToSimplify.Add(languageTag);
			// See remarks above. 
			System.Diagnostics.Debug.Assert(!MapIsoCodeToSubtitledLanguageName.Remove(languageTag));
		}

		/// <summary>
		/// Get the language name in its own language and script if possible. If it's not a Latin
		/// script, add an English name suffix.
		/// If we don't know a native name, but do know an English name, return the language code
		/// with an English name suffix.
		/// If we know nothing, return the language code.
		/// </summary>
		/// <remarks>
		/// This might be easier to implement reliably with Full ICU for a larger set of languages,
		/// but we if only the Min ICU DLL is included, that information is definitely not
		/// available.
		/// This method is suitable to generate menu labels in the UI language chooser menu, which
		/// are most likely to be major languages known to both Windows and Linux.
		/// GetEnglishNameIfKnown and GetNativeNameIfKnown may need to be updated if localizations
		/// are done into regional (or national) languages of some countries.
		/// </remarks>
		public string GetNativeLanguageNameWithEnglishSubtitle()
		{
			if (MapIsoCodeToSubtitledLanguageName.TryGetValue(IetfLanguageTag, out var langName))
				return langName;
			string nativeName;

			var generalCode = GetGeneralCode(code);
			var useFullCode = LanguagesNotToSimplify.Contains(generalCode);
			try
			{
				// englishNameSuffix is always an empty string if we don't need it.
				string englishNameSuffix = Empty;
				var ci = CultureInfo.GetCultureInfo(useFullCode ? code : generalCode); // this may throw or produce worthless empty object
				if (NeedEnglishSuffixForLanguageName(ci))
					englishNameSuffix = $" ({GetManuallyOverriddenEnglishNameIfNeeded(code, ()=>ci.EnglishName)})";

				nativeName = FixBotchedNativeName(ci.NativeName);
				if (IsNullOrWhiteSpace(nativeName))
					nativeName = code;
				if (!useFullCode)
				{
					// Remove any country (or script?) names.
					var idxCountry = englishNameSuffix.LastIndexOf(" (", StringComparison.Ordinal);
					if (englishNameSuffix.Length > 0 && idxCountry > 0)
						englishNameSuffix = englishNameSuffix.Substring(0, idxCountry) + ")";
					idxCountry = nativeName.IndexOf(" (", StringComparison.Ordinal);
					if (idxCountry > 0)
						nativeName = nativeName.Substring(0, idxCountry);
				}
				else if (englishNameSuffix.Length > 0)
				{
					// I have seen more cruft after the country name a few times, so remove that
					// as well. The parenthetical expansion always seems to start "(Simplified" or,
					// which we want to keep. We need double close parentheses because there's one
					// open parenthesis before "Chinese" and another open parenthesis before
					// "Simplified" (which precedes ", China" or ", PRC"). Also, we don't worry
					// about the parenthetical content of the native Chinese name.
					var idxCountry = englishNameSuffix.IndexOf(", ", StringComparison.Ordinal);
					if (idxCountry > 0)
						englishNameSuffix = englishNameSuffix.Substring(0, idxCountry) + "))";
				}
				langName = nativeName + englishNameSuffix;
				if (!ci.EnglishName.StartsWith("Unknown Language"))	// Windows .Net behavior
				{
					MapIsoCodeToSubtitledLanguageName.Add(code, langName);
					return langName;
				}
			}
			catch (Exception e)
			{
				// ignore exception, but log on terminal.
				System.Diagnostics.Debug.WriteLine($"GetNativeLanguageNameWithEnglishSubtitle ignoring exception: {e.Message}");
			}
			// We get here after either an exception was thrown or the returned CultureInfo
			// helpfully told us it is for an unknown language (instead of throwing).
			// Handle a few languages that we do know the English and native names for
			// (that are being localized for Bloom).
			if (IsUnlistedCode(generalCode) && GetBestLanguageName(code, out langName))
				return langName;
			var englishName = GetManuallyOverriddenEnglishNameIfNeeded(code, () => GetEnglishNameIfKnown(generalCode));
			nativeName = GetNativeNameIfKnown(generalCode);
			if (IsNullOrWhiteSpace(nativeName) && IsNullOrWhiteSpace(englishName))
				langName = code;
			else if (IsNullOrWhiteSpace(nativeName))
				langName = code + " (" + englishName + ")";
			else if (IsNullOrWhiteSpace(englishName))
			{
				// I don't think this will ever happen...
				if (IsLatinChar(nativeName[0]))
					langName = nativeName;
				else
					langName = nativeName + $" ({code})";
			}
			else
			{
				if (IsLatinChar(nativeName[0]))
					langName = nativeName;
				else
					langName = nativeName + $" ({englishName})";
			}
			MapIsoCodeToSubtitledLanguageName.Add(code, langName);
			return langName;
		}

		private static bool GetBestLanguageName(string isoCode, out string name)
		{
			name = GetLanguageNameForUnknownIsoCode(isoCode);
			if (name != null)
				return true;
			name = isoCode;
			return false;
		}

		public static string GetManuallyOverriddenEnglishNameIfNeeded(string code, Func<string> defaultOtherwise)
		{
			// We used pbu in Crowdin for some reason which is "Northern Pashto,"
			// but we want this label to just be the generic macrolanguage "Pashto."
			return code == "pbu" ? "Pashto" : defaultOtherwise();
		}

		/// <summary>
		/// Check whether we need to add an English suffix to the native language name. This is true if we don't know
		/// the native name at all or if the native name is not in a Latin alphabet.
		/// </summary>
		private static bool NeedEnglishSuffixForLanguageName(CultureInfo ci)
		{
			if (IsNullOrWhiteSpace(ci.NativeName))
				return true;
			var testChar = ci.NativeName[0];
			return ci.EnglishName != ci.NativeName && !IsLatinChar(testChar);
		}

		/// <summary>
		/// Get the language part of the given tag, except leave zh-CN alone.
		/// </summary>
		public static string GetGeneralCode(string code)
		{
			// Though you might be tempted to simplify this by using GetLanguagePart, don't: this
			// methods works with three-letter codes even if there is a valid 2-letter code that
			// should be used instead.
			var idxCountry = code.IndexOf("-", StringComparison.Ordinal);
			if (idxCountry == -1 || code == "zh-CN")
				return code;
			return code.Substring(0, idxCountry);
		}

		/// <summary>
		/// For what languages we know about, return the English name. If we don't know anything, return null.
		/// This is called only when CultureInfo doesn't supply the information we need.
		/// </summary>
		private static string GetEnglishNameIfKnown(string code)
		{
			if (!GetBestLanguageName(code, out var englishName))
			{
				switch (code)
				{
					case "pbu":  englishName = "Pashto";  break;
					case "prs":  englishName = "Dari";             break;
					case "tpi":  englishName = "New Guinea Pidgin English"; break;
					default:     englishName = null;               break;
				}
			}
			return englishName;
		}

		/// <summary>
		/// For the languages we know about, return the native name. If we don't know anything, return null.
		/// (This applies only to languages that CultureInfo doesn't know about on at least one of Linux and
		/// Windows.)
		/// </summary>
		private static string GetNativeNameIfKnown(string code)
		{
			switch (code)
			{
				case "pbu":  return "پښتو";
				case "prs":  return "دری";
				case "tpi":  return "Tok Pisin";
				default:     return null;
			}
		}

		/// <summary>
		/// Fix any native language names that we know either .Net or Mono gets wrong.
		/// </summary>
		private static string FixBotchedNativeName(string name)
		{
			// See http://issues.bloomlibrary.org/youtrack/issue/BL-5223.
			switch (name)
			{
				// .Net gets this one wrong,but Mono gets it right.
				case "Indonesia": return "Bahasa Indonesia";

				// Although these look the same, what Windows supplies as the "Native Name"
				// Wiktionary lists it as a different word and says that the word we have
				// hardcoded here (and above in GetNativeNameIfKnown) is the correct name.
				// Wikipedia seems to agree. Interestingly, Google brings up the Wikipedia
				// info for Dari when you search for either one, even though the presumably
				// incorrect version does not actually appear on that Wikipedia page. It
				// would be nice to find someone who is an authority on this, so we could
				// report it to Microsoft as a bug if it is indeed incorrect.
				case "درى": return "دری";
				// Incorrect capitalization on older Windows OS versions.
				case "Português": return "português";
				// REVIEW: For Chinese, older Windows OS versions return 中文(中华人民共和国) instead
				// of 中文(中国) {i.e., Chinese (People's Republic of China) instead of
				// Chinese (China). Do we consider that "botched"?

				default: return name;
			}
		}

		/// <summary>
		/// Return true for ASCII, Latin-1, Latin Ext. A, Latin Ext. B, IPA Extensions, and Spacing Modifier Letters.
		/// </summary>
		private static bool IsLatinChar(char test) => test <= 0x02FF;

		/// <summary>
		/// Test whether the given ISO 639-3 code is one reserved for unlisted languages ("qaa" - "qtz").
		/// </summary>
		public bool IsUnlistedCode()
		{
			if (IsNullOrEmpty(ThreeLetterISOLanguageName) || ThreeLetterISOLanguageName.Length != 3)
				return false;
			if (ThreeLetterISOLanguageName[0] != 'q')
				return false;
			if (ThreeLetterISOLanguageName[1] < 'a' || ThreeLetterISOLanguageName[1] > 't')
				return false;
			return ThreeLetterISOLanguageName[2] >= 'a' && ThreeLetterISOLanguageName[2] <= 'z';
		}
		#endregion

	}
}
