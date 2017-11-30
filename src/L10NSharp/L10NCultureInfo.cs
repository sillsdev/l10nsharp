using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
			catch (CultureNotFoundException ex)
			{
				RawCultureInfo = null;
			}
			if (RawCultureInfo == null || RawCultureInfo.EnglishName.StartsWith("Unknown Language"))
			{
				Name = name;
				IsNeutralCulture = !Name.Contains("-");
				if (IsNeutralCulture)
				{
					IetfLanguageTag = name;
				}
				else
				{
					var idx = name.IndexOf("-");
					IetfLanguageTag = name.Substring(0, idx);
				}
				// CultureInfo returns 3-letter tags when a 2-letter tag doesn't exist.
				// If it's a minor enough language to be unknown to .Net or Mono, it's
				// not worth trying to find a 2-letter tag that usually wouldn't exist.
				TwoLetterISOLanguageName = IetfLanguageTag;
				ThreeLetterISOLanguageName = IetfLanguageTag;
				switch (name)
				{
				case "pbu":
					EnglishName = "Northern Pashto";
					DisplayName = "Northern Pashto";
					NativeName = "پښتو";
					NumberFormat = CultureInfo.GetCultureInfo("ar").NumberFormat;
					break;
				case "prs":
					EnglishName = "Dari";
					DisplayName = "Dari";
					NativeName = "دری";
					NumberFormat = CultureInfo.GetCultureInfo("ar").NumberFormat;
					break;
				case "tpi":
					EnglishName = "New Guinea Pidgin English";
					DisplayName = "Tok Pisin";
					NativeName = "Tok Pisin";
					NumberFormat = CultureInfo.GetCultureInfo("en").NumberFormat;
					break;
				default:
					EnglishName = string.Format("Unknown Language ({0})", name);
					DisplayName = EnglishName;
					NativeName = EnglishName;
					NumberFormat = CultureInfo.InvariantCulture.NumberFormat;
					break;
				}
			}
			else
			{
				InitializeFromRawCultureInfo();
				// The Windows .Net runtime returns 'Azərbaycan dili (Azərbaycan)' for az-Latn, and
				// something totally different for az.
				// The Mono runtime returns "azərbaycan" for both az and az-Latn.
				// So we just set this to what we "know" is the right value for az.
				if (name == "az")
					NativeName = "Azərbaycan dili";
				// The Windows .Net runtime returns 'Indonesia' for id.  It should be 'Bahasa Indonesia'.
				else if (name == "id")
					NativeName = "Bahasa Indonesia";
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
		public CultureInfo RawCultureInfo { get; private set; }

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

		public override string ToString()
		{
			return string.Format("[L10NCultureInfo: Name={0}, EnglishName={1}]", Name, EnglishName);
		}

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
	}
}
