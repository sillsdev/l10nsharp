using System.IO;
using System.Linq;
using L10NSharp.XLiffUtils;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	/// <summary>
	/// Tests for the qps-ploc pseudo-locale: lookups for it return the English text
	/// pseudolocalized (accented, padded, bracketed) at runtime.
	/// </summary>
	[TestFixture]
	public class PseudoLocalizationTests
	{
		private const string AppId = "test";
		private const string AppName = "unit test";
		private const string AppVersion = "1.0.0";
		private const string Pseudo = LocalizationManager.PseudoLocalizationLanguageId;

		[SetUp]
		public void Setup()
		{
			LocalizationManager.TranslationMemoryKind = TranslationMemory.XLiff;
		}

		[TearDown]
		public void TearDown()
		{
			LocalizationManager.OfferPseudoLocalization = false;
			LocalizationManagerInternal<XLiffDocument>.LoadedManagers.Clear();
			LocalizationManagerInternal<XLiffDocument>.MapToExistingLanguage.Clear();
			LocalizationManager.SetUILanguage(LocalizationManager.kDefaultLang);
		}

		#region Transform behavior we rely on

		[Test]
		public void PseudoLocalize_PlainEnglish_IsAccentedPaddedAndBracketed()
		{
			const string english = "Cook Book";
			var result = LocalizationManager.PseudoLocalize(english);

			Assert.That(result, Does.StartWith("["));
			Assert.That(result, Does.EndWith("]"));
			Assert.That(result.Length, Is.GreaterThan(english.Length),
				"expansion padding should make the string longer");
			Assert.That(result, Does.Not.Contain("Cook"),
				"letters should have been replaced by accented variants");
		}

		[Test]
		public void PseudoLocalize_IsDeterministic()
		{
			const string english = "The quick brown fox jumps over the lazy dog";
			Assert.That(LocalizationManager.PseudoLocalize(english),
				Is.EqualTo(LocalizationManager.PseudoLocalize(english)));
		}

		[TestCase("Page {0} of {1}", "{0}", "{1}")]
		[TestCase("Showing {0:n0} items", "{0:n0}")]
		public void PseudoLocalize_FormatPlaceholders_SurviveUntouched(string english,
			params string[] placeholders)
		{
			var result = LocalizationManager.PseudoLocalize(english);
			foreach (var placeholder in placeholders)
				Assert.That(result, Does.Contain(placeholder));
		}

		[TestCase("<strong>Bold</strong> text", "<strong>", "</strong>")]
		[TestCase("A <a href=\"x\">link</a>.", "<a href=\"x\">", "</a>")]
		public void PseudoLocalize_HtmlTags_SurviveUntouched(string english,
			params string[] tags)
		{
			var result = LocalizationManager.PseudoLocalize(english);
			foreach (var tag in tags)
				Assert.That(result, Does.Contain(tag));
		}

		[TestCase(null)]
		[TestCase("")]
		public void PseudoLocalize_NullOrEmpty_ReturnedAsIs(string english)
		{
			Assert.That(LocalizationManager.PseudoLocalize(english), Is.EqualTo(english));
		}

		#endregion

		#region Manager-level behavior

		[Test]
		public void GetString_UILanguageIsPseudo_ReturnsPseudolocalizedEnglishText()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder, Pseudo);
				Assert.That(LocalizationManager.GetString("blahId", "blah"),
					Is.EqualTo(LocalizationManager.PseudoLocalize("blah")));
			}
		}

		[Test]
		public void GetString_UILanguageIsPseudo_StringMissingEverywhere_PseudolocalizesSuppliedEnglish()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder, Pseudo);
				Assert.That(LocalizationManager.GetString("no.such.id", "only in code"),
					Is.EqualTo(LocalizationManager.PseudoLocalize("only in code")));
			}
		}

		[Test]
		public void GetString_UILanguageIsRealLanguage_NeverGetsPseudoText()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder, "fr");
				Assert.That(LocalizationManager.GetString("blahId", "blah"),
					Is.EqualTo("blahInFrench"));
			}
		}

		[Test]
		public void GetDynamicStringOrEnglish_PseudoLangId_PseudolocalizesSuppliedEnglish()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				Assert.That(
					LocalizationManager.GetDynamicStringOrEnglish(AppId, "blahId", "from the code",
						null, Pseudo),
					Is.EqualTo(LocalizationManager.PseudoLocalize("from the code")),
					"the code-supplied English should win over the cache, as for 'en'");
			}
		}

		[Test]
		public void GetDynamicStringOrEnglish_PseudoLangIdNoDefault_PseudolocalizesCachedEnglish()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				Assert.That(
					LocalizationManager.GetDynamicStringOrEnglish(AppId, "blahId", null, null,
						Pseudo),
					Is.EqualTo(LocalizationManager.PseudoLocalize("blah")));
			}
		}

		[Test]
		public void GetDynamicString_UILanguageIsPseudo_DoesNotCreatePseudoTranslationFile()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder, Pseudo);
				LocalizationManager.GetDynamicString(AppId, "brand.new.id", "novel string");
				var pseudoFiles = Directory
					.GetFiles(folder.Path, "*", SearchOption.AllDirectories)
					.Where(f => f.IndexOf(Pseudo, System.StringComparison.OrdinalIgnoreCase) >= 0);
				Assert.That(pseudoFiles, Is.Empty);
			}
		}

		[TestCase(Pseudo, null, Description = "pseudo alone")]
		[TestCase(Pseudo, "fr", Description = "pseudo preferred over French")]
		public void GetString_PreferredLanguagesStartingWithPseudo_ReturnsPseudolocalizedEnglish(
			string firstLangId, string secondLangId)
		{
			var preferredLangIds = secondLangId == null
				? new[] { firstLangId }
				: new[] { firstLangId, secondLangId };
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				var result = LocalizationManager.GetString("blahId", "blah", null,
					preferredLangIds, out var languageIdUsed);
				Assert.That(result, Is.EqualTo(LocalizationManager.PseudoLocalize("blah")));
				Assert.That(languageIdUsed, Is.EqualTo(Pseudo));
			}
		}

		[Test]
		public void GetString_LanguageWithTranslationPreferredOverPseudo_ReturnsTranslation()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				var result = LocalizationManager.GetString("blahId", "blah", null,
					new[] { "fr", Pseudo }, out var languageIdUsed);
				Assert.That(result, Is.EqualTo("blahInFrench"));
				Assert.That(languageIdUsed, Is.EqualTo("fr"));
			}
		}

		[Test]
		public void GetString_EnglishPreferredOverPseudo_ReturnsPlainEnglish()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				var result = LocalizationManager.GetString("blahId", "blah", null,
					new[] { "en", Pseudo }, out var languageIdUsed);
				Assert.That(result, Is.EqualTo("blah"));
				Assert.That(languageIdUsed, Is.EqualTo("en"));
			}
		}

		[Test]
		public void GetIsStringAvailableForLangId_Pseudo_ReportsSameAsEnglish()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				Assert.That(LocalizationManager.GetIsStringAvailableForLangId("blahId", Pseudo),
					Is.True);
				Assert.That(LocalizationManager.GetIsStringAvailableForLangId("no.such.id", Pseudo),
					Is.False);
			}
		}

		[Test]
		public void GetAvailableLocalizedLanguages_RespectsOfferPseudoLocalization()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				Assert.That(LocalizationManager.GetAvailableLocalizedLanguages(),
					Does.Not.Contain(Pseudo), "pseudo-locale should not be advertised by default");

				LocalizationManager.OfferPseudoLocalization = true;
				Assert.That(LocalizationManager.GetAvailableLocalizedLanguages(),
					Does.Contain(Pseudo));
			}
		}

		[Test]
		public void StringCountAndFractions_Pseudo_ReportEnglishCompleteness()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				Assert.That(LocalizationManager.StringCount(Pseudo),
					Is.EqualTo(LocalizationManager.StringCount(LocalizationManager.kDefaultLang)));
				Assert.That(LocalizationManager.FractionApproved(Pseudo), Is.EqualTo(1.0f));
				Assert.That(LocalizationManager.FractionTranslated(Pseudo), Is.EqualTo(1.0f));
			}
		}

		[Test]
		public void GetUILanguages_PseudoOffered_HasHardCodedDisplayName()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				LocalizationManager.OfferPseudoLocalization = true;
				var pseudoCulture = LocalizationManager.GetUILanguages(true)
					.FirstOrDefault(c => c.Name == Pseudo);
				Assert.That(pseudoCulture, Is.Not.Null);
				Assert.That(pseudoCulture.DisplayName, Is.EqualTo("Pseudo-English (qps-ploc)"));
				Assert.That(pseudoCulture.NativeName, Is.EqualTo("Pseudo-English (qps-ploc)"));
			}
		}

		[Test]
		public void SetUILanguage_Pseudo_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => LocalizationManager.SetUILanguage(Pseudo));
			Assert.That(LocalizationManager.UILanguageId, Is.EqualTo(Pseudo));
		}

		#endregion

		/// <summary>
		/// Installs English (blahId="blah", theId="from English Translation") and French
		/// (blahId="blahInFrench") translations, sets the UI language, and loads a manager.
		/// </summary>
		private static void SetupManager(TempFolder folder,
			string uiLanguageId = LocalizationManager.kDefaultLang)
		{
			LocalizationManagerInternal<XLiffDocument>.LoadedManagers.Clear();

			var englishDoc = CreateDocument(AppVersion, "en");
			englishDoc.AddTransUnit(CreateTransUnit("theId", "en", "from English Translation"));
			englishDoc.AddTransUnit(CreateTransUnit("blahId", "en", "blah"));
			englishDoc.Save(Path.Combine(folder.Path,
				LocalizationManager.GetTranslationFileNameForLanguage(AppId, "en")));

			var frenchDoc = CreateDocument(null, "en", "fr");
			var frenchTu = CreateTransUnit("blahId", "en", "blah");
			frenchTu.Target = new XLiffTransUnitVariant { Lang = "fr", Value = "blahInFrench" };
			frenchTu.TranslationStatus = TranslationStatus.Approved;
			frenchDoc.AddTransUnit(frenchTu);
			frenchDoc.Save(Path.Combine(folder.Path,
				LocalizationManager.GetTranslationFileNameForLanguage(AppId, "fr")));

			LocalizationManager.SetUILanguage(uiLanguageId);
			var manager = new XliffLocalizationManager(AppId, null, AppName, AppVersion,
				folder.Path, folder.Combine("generated"), folder.Combine("userModified"), null);
			LocalizationManagerInternal<XLiffDocument>.LoadedManagers[AppId] = manager;
		}

		private static XLiffDocument CreateDocument(string productVersion, string sourceLang,
			string targetLang = null)
		{
			var doc = new XLiffDocument { File = { SourceLang = sourceLang } };
			if (!string.IsNullOrEmpty(productVersion))
				doc.File.ProductVersion = productVersion;
			if (!string.IsNullOrEmpty(targetLang))
				doc.File.TargetLang = targetLang;
			doc.File.Original = "test.dll";
			return doc;
		}

		private static XLiffTransUnit CreateTransUnit(string id, string lang, string value)
		{
			return new XLiffTransUnit
			{
				Id = id,
				Source = new XLiffTransUnitVariant { Lang = lang, Value = value }
			};
		}
	}
}
