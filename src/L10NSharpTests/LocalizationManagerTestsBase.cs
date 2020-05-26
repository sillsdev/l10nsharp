// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using L10NSharp.XLiffUtils;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	public abstract class LocalizationManagerTestsBase<T> where T: IDocument
	{
		protected const string AppId = "test";
		protected const string AppName = "unit test";
		protected const string AppVersion = "1.0.0";
		protected const string HigherVersion = "2.0.0";
		protected const string LowerVersion = "0.0.1";
		protected const string LiteralNewline = "\\n";

		internal abstract ILocalizationManagerInternal<T> CreateLocalizationManager(string appId,
			string appName, string appVersion, string directoryOfInstalledTmxFiles,
			string directoryForGeneratedDefaultTmxFile, string directoryOfUserModifiedTranslationFiles,
			IEnumerable<MethodInfo> additionalGetStringMethodInfo = null,
			params string[] namespaceBeginnings);
		internal abstract ILocalizationManagerInternal<T> CreateLocalizationManager(string appId,
			string appName, string appVersion);
		protected abstract T CreateNewDocument(string productVersion, string sourceLang,
			string targetLang = null, string otherAppVersion = null);

		protected abstract ITransUnit CreateTransUnit(string id, bool dynamic,
			ITransUnitVariant sourceVariant, ITransUnitVariant target = null, string noteText =
			 null, TranslationStatus? translationStatus = null, bool? noLongerUsed = null);

		protected abstract ITransUnitVariant CreateTransUnitVariant(string lang, string value);

		[TearDown]
		public void TearDownLocalizationManagers()
		{
			LocalizationManager.UseLanguageCodeFolders = false;
			LocalizationManagerInternal<T>.LoadedManagers.Clear();
			LocalizationManagerInternal<T>.MapToExistingLanguage.Clear();
			LocalizationManager.SetUILanguage(LocalizationManager.kDefaultLang, false);
		}

		/// <summary>
		/// If there is no GeneratedDefault Translation file, but the file we need has been installed, copy the installed version to circumvent
		/// a crash trying to generate this Translation file on Linux.
		/// </summary>
		[Test]
		public void CreateOrUpdateDefaultTranslationFileIfNecessary_CopiesInstalledIfAvailable()
		{
			using(var folder = new TempFolder())
			{
				// SUT (buried down in there somewhere)
				SetupManager(folder);

				// verify presence and identicality of the generated file to the installed file
				var filename = LocalizationManager.GetTranslationFileNameForLanguage(AppId,
					LocalizationManager.kDefaultLang);
				var installedFilePath = Path.Combine(GetInstalledDirectory(folder), filename);
				var generatedFilePath = Path.Combine(GetGeneratedDirectory(folder), filename);
				Assert.That(File.Exists(generatedFilePath), "Generated file {0} should exist", generatedFilePath);
				FileAssert.AreEqual(installedFilePath, generatedFilePath, "Generated file should be copied from and identical to Installed file");
			}
		}

		/// <summary>
		/// On Linux, we crash trying to generate Translation files, leaving an empty file in the Generated folder.
		/// Copy the installed file over the empty generated file in this case.
		/// </summary>
		[Test]
		public void CreateOrUpdateDefaultTranslationFileIfNecessary_CopiesOverEmptyGeneratedFile()
		{
			using(var folder = new TempFolder())
			{
				var filename = LocalizationManager.GetTranslationFileNameForLanguage(AppId,
					LocalizationManager.kDefaultLang);
				var installedFilePath = Path.Combine(GetInstalledDirectory(folder), filename);
				var generatedFilePath = Path.Combine(GetGeneratedDirectory(folder), filename);

				// generate an empty English Translation file
				Directory.CreateDirectory(GetGeneratedDirectory(folder));
				var fileStream = File.Open(generatedFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
				fileStream.Close();

				// SUT (buried down in there somewhere)
				SetupManager(folder);

				// verify identicality of the generated file to the installed file
				FileAssert.AreEqual(installedFilePath, generatedFilePath, "Generated file should be copied from and identical to Installed file");
			}
		}

		/// <summary>
		/// If there is an existing Translation file of the same (or higher) version, it should not be overwritten when initializing the localizer.
		/// </summary>
		[Test]
		public void CreateOrUpdateDefaultTranslationFileIfNecessary_UpToDate_DoesNotOverwriteUpToDateGeneratedFile()
		{
			using(var folder = new TempFolder())
			{
				var filename = LocalizationManager.GetTranslationFileNameForLanguage(AppId,
					LocalizationManager.kDefaultLang);
				var generatedFilePath = Path.Combine(GetGeneratedDirectory(folder), filename);

				// "generate" an English Translation for a higher version
				Directory.CreateDirectory(GetGeneratedDirectory(folder));
				AddEnglishTranslation(GetGeneratedDirectory(folder), HigherVersion);
				var generatedTranslationContents = File.ReadAllText(generatedFilePath);

				// SUT (buried down in there somewhere)
				SetupManager(folder);

				// verify identicality of the generated file to its previous state
				Assert.AreEqual(generatedTranslationContents, File.ReadAllText(generatedFilePath), "Generated file should not have been overwritten");
			}
		}

		/// <summary>
		/// If the GeneratedDefault Translation file is out of date, it should be brought up to the current version
		/// (sorry, this doesn't test the contents, just the version number).
		/// </summary>
		[Test]
		public void CreateOrUpdateDefaultTranslationFileIfNecessary_Outdated_OverwritesOutdatedGeneratedFile()
		{
			using (var folder = new TempFolder())
			{
				var filename = LocalizationManager.GetTranslationFileNameForLanguage(AppId,
					LocalizationManager.kDefaultLang);
				var generatedFilePath = Path.Combine(GetGeneratedDirectory(folder), filename);

				// "generate" an English Translation for a lower version
				Directory.CreateDirectory(GetGeneratedDirectory(folder));
				AddEnglishTranslation(GetGeneratedDirectory(folder), LowerVersion);

				// SUT (buried down in there somewhere)
				SetupManager(folder);

				// verify that the generated file has been updated to the current version
				var xmlDoc = XElement.Load(generatedFilePath);
				var generatedVersion = GetGeneratedVersion(xmlDoc);

				Assert.That(generatedVersion, Is.EqualTo(new Version(AppVersion).ToString()), "Generated file should have been updated to the current version");
			}
		}

		/// <summary>
		/// Ensure that the generated file includes additional strings from additional localization methods.
		/// </summary>
		[Test]
		public void CreateOrUpdateDefaultTranslationFileIfNecessary_Missing_IncludesStringFromCustomLocalizationMethod()
		{
			using(var folder = new TempFolder())
			{
				// SUT (Down in StringExtractor.DoExtractingWork, etc.)
				var manager = CreateLocalizationManager(AppId, AppName, AppVersion,
					GetInstalledDirectory(folder), GetGeneratedDirectory(folder), GetUserModifiedDirectory(folder),
						typeof(ProxyLocalizationManager)
						.GetMethods(BindingFlags.Static | BindingFlags.Public)
						.Where(m => m.Name == "MyOwnGetString"));

				// verify that the generated file has includes the string from the call to MyOwnGetString (below).
				Assert.AreEqual("My Own English String",
					manager.StringCache.GetString("en", "myOwn.English.String.Id"));
				
				Assert.AreEqual("My Own English String", ProxyLocalizationManager.MyOwnGetString("myOwn.English.String.Id", "My Own English String",
					"This is used to tests the case where MyOwnGetString is passed as an extra method to use for extraction."));
			}
		}

		protected abstract string GetGeneratedVersion(XElement xmlDoc);

		/// <summary>
		/// This is a regression test. As of Nov 2014, if we updated an English string, it would
		/// get overwritten by the old version, found in another language's Translation.
		/// </summary>
		[Test]
		public void GetDynamicString_EnglishAndArabicHaveDifferentValuesOfEnglishString_EnglishTranslationWins()
		{
			using(var folder = new TempFolder())
			{
				SetupManager(folder, "en");
				//This was the original assertion, and it worked:
				//   Assert.AreEqual("from English Translation", LocalizationManager.GetDynamicString(AppId, "theId", "some default"));
				//However, later I decided, I don't care what is in the English Translation, either. If the c# code just gave a new
				// value for this, what the c# code said should win. Who cares what's in the English Translation. It's only real
				// purpose in life is to provide a list of strings that have been discovered dynamically when the translator
				// needs a list of strings to translate (without it, the translator would have to cause the program to visit
				// each part of the UI so as to trip over all the GetDynamicString() calls.

				//So now, this is correct:

				Assert.AreEqual("from the c# code", LocalizationManager.GetDynamicString(AppId, "theId", "from the c# code"));
			}
		}
		/// <summary>
		/// This is a regression test. If the English Translation is out of date, too bad. The c# code always wins.
		/// </summary>
		[Test]
		public void GetDynamicString_EnglishTranslationHasDIfferentStringThanParamater_ParameterWins()
		{
			using(var folder = new TempFolder())
			{
				SetupManager(folder, "en");
				Assert.AreEqual("from c# code", LocalizationManager.GetDynamicString(AppId, "theId", "from c# code"));
			}
		}

		[Test]
		public void GetDynamicString_ArabicTranslationHasArabicValue_ArabicTranslationWins()
		{
			using(var folder = new TempFolder())
			{
				SetupManager(folder, "ar");
				Assert.AreEqual("inArabic", LocalizationManager.GetDynamicString(AppId, "theId", "from c# code"));
			}
		}

		[Test]
		public void GetDynamicStringInEnglish_NoDefault_FindsEnglish()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder, "en");
				Assert.That(LocalizationManager.GetDynamicString(AppId, "blahId", null), Is.EqualTo("blah"), "With no default supplied, should find saved English");
			}
		}

		[Test]
		public void GetDynamicStringOrEnglish_LmDisposed_GivesUsefulException()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder, "en");
				Assert.That(LocalizationManager.GetDynamicString(AppId, "blahId", null), Is.EqualTo("blah"), "With no default supplied, should find saved English");
				using (var extra = CreateLocalizationManager("nonsense", "more nonsense", "1.0"))
				{
					LocalizationManagerInternal<T>.LoadedManagers.Add(extra.Id, extra);
					LocalizationManagerInternal<T>.LoadedManagers[AppId].Dispose();
					Assert.Throws<ObjectDisposedException>(() =>
						LocalizationManager.GetDynamicString(AppId, "blahId", null));
				}

				// A different path when there are none left
				Assert.Throws<ObjectDisposedException>(() => LocalizationManager.GetDynamicString(AppId, "blahId", null));
			}
		}

		[TestCase("en", "en", ExpectedResult = "blahInEnglishInCode", Description = "If asked for English, should give whatever is in the code.")]
		[TestCase("ar", "en", ExpectedResult = "blah",                Description = "We don't have Arabic so should get the English from code.")] // The original test before refactoring expected "blahInEnglishInCode", but that got returned only because the previous test changed what was in the data!
		[TestCase("en", "fr", ExpectedResult = "blahInEnglishInCode", Description = "If asked for English, should give whatever is in the code.")]
		[TestCase("fr", "fr", ExpectedResult = "blahInFrench",        Description = "We do have French for this, should have found it.")]
		public string GetDynamicStringOrEnglish(string langId, string uiLangId)
		{
			using(var folder = new TempFolder())
			{
				SetupManager(folder, "ar");
				LocalizationManager.SetUILanguage(uiLangId, true);
				return LocalizationManager.GetDynamicStringOrEnglish(AppId, "blahId", "blahInEnglishInCode", "comment", langId);
			}
		}

		//NOTE: the TestName parameter is only here to work around an NUnit bug in which
		//NUnit doesn't run all the test cases when some differ only by the values in an array parameter
		//cases where we expect to get back the english in the code
		[TestCase(new[] { "en" }, "blahInEnglishCode", "en", TestName = "GetString_OverloadThatTakesListOfLanguages_Works_1")]
		[TestCase(new[] { "en", "fr" }, "blahInEnglishCode", "en", TestName = "GetString_OverloadThatTakesListOfLanguages_Works_2")]
		[TestCase(new[] { "ar", "en" }, "blahInEnglishCode", "en", TestName = "GetString_OverloadThatTakesListOfLanguages_Works_3")] // our arabic doesn't have a translation of 'blah', so fall to the code's English
		[TestCase(new[] { "zz", "en", "fr" }, "blahInEnglishCode", "en", TestName = "GetString_OverloadThatTakesListOfLanguages_Works_4")]
		//cases where we expect to get back the French
		[TestCase(new[] { "fr" }, "blahInFrench", "fr", TestName = "GetString_OverloadThatTakesListOfLanguages_Works_5")]
		[TestCase(new[] { "fr", "en" }, "blahInFrench", "fr", TestName = "GetString_OverloadThatTakesListOfLanguages_Works_6")]
		[TestCase(new[] { "ar", "fr", "en" }, "blahInFrench", "fr", TestName = "GetString_OverloadThatTakesListOfLanguages_Works_7")] // our arabic doesn't have a translation of 'blah', so fall to French
		public void GetString_OverloadThatTakesListOfLanguages_Works(IEnumerable<string> preferredLangIds,  string expectedResult, string expectedLanguage)
		{
			using(var folder = new TempFolder())
			{
				AddRandomTranslation("ii", GetInstalledDirectory(folder));
				SetupManager(folder, "ii" /* UI language not important */);

				// SUT
				var result = LocalizationManager.GetString("blahId", "blahInEnglishCode", "comment", preferredLangIds, out var languageFound);

				Assert.AreEqual(expectedResult, result);
				Assert.AreEqual(expectedLanguage, languageFound);
			}
		}

		[Test]
		public void GetUiLanguages_EnglishIsThere()
		{
			var cultures = LocalizationManager.GetUILanguages(false);
			Assert.AreEqual("English", cultures.Where(c => c.Name == "en").Select(c => c.NativeName).FirstOrDefault());
		}

		[Test]
		public void GetUiLanguages_FindsAll()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				var cultures = new List<L10NCultureInfo>(LocalizationManager.GetUILanguages(true));
				Assert.AreEqual(4, cultures.Count);
				Assert.AreEqual("ar", cultures[0].IetfLanguageTag);		// Arabic
				Assert.AreEqual("en", cultures[1].IetfLanguageTag);		// English
				Assert.AreEqual("fr", cultures[2].IetfLanguageTag);		// French
				Assert.AreEqual("es", cultures[3].IetfLanguageTag);		// Spanish
			}
		}

		[Test]
		public void GetUiLanguages_FindsAllWithFolders()
		{
			try
			{
				LocalizationManager.UseLanguageCodeFolders = true;
				using (var folder = new TempFolder())
				{
					SetupManager(folder);
					var cultures = new List<L10NCultureInfo>(LocalizationManager.GetUILanguages(true));
					Assert.AreEqual(4, cultures.Count);
					Assert.AreEqual("ar", cultures[0].IetfLanguageTag);		// Arabic
					Assert.AreEqual("en", cultures[1].IetfLanguageTag);		// English
					Assert.AreEqual("fr", cultures[2].IetfLanguageTag);		// French
					Assert.AreEqual("es", cultures[3].IetfLanguageTag);		// Spanish
				}
			}
			finally
			{
				LocalizationManager.UseLanguageCodeFolders = false;
			}
		}

		int compareCultureTags(CultureInfo first, CultureInfo second)
		{
			return first.IetfLanguageTag.CompareTo(second.IetfLanguageTag);
		}

		[Test]
		public void GetDynamicStringInEnglish_NoDefault_FindsEnglishWithFolders()
		{
			try
			{
				LocalizationManager.UseLanguageCodeFolders = true;
				using (var folder = new TempFolder())
				{
					SetupManager(folder);
					Assert.That(LocalizationManager.GetDynamicString(AppId, "blahId", null), Is.EqualTo("blah"), "With no default supplied, should find saved English");
				}
			}
			finally
			{
				LocalizationManager.UseLanguageCodeFolders = false;
			}
		}

		//NOTE: the TestName parameter is only here to work around an NUnit bug in which
		//NUnit doesn't run all the test cases when some differ only by the values in an array parameter
		//cases where we expect to get back the english in the code
		[TestCase(new[] { "en" }, "blahInEnglishCode", "en", TestName = "GetString_OverloadThatTakesListOfLanguages_WorksWithFolders_1")]
		[TestCase(new[] { "en", "fr" }, "blahInEnglishCode", "en", TestName = "GetString_OverloadThatTakesListOfLanguages_WorksWithFolders_2")]
		[TestCase(new[] { "ar", "en" }, "blahInEnglishCode", "en", TestName = "GetString_OverloadThatTakesListOfLanguages_WorksWithFolders_3")] // our arabic doesn't have a translation of 'blah', so fall to the code's English
		[TestCase(new[] { "zz", "en", "fr" }, "blahInEnglishCode", "en", TestName = "GetString_OverloadThatTakesListOfLanguages_WorksWithFolders_4")]
		//cases where we expect to get back the French
		[TestCase(new[] { "fr" }, "blahInFrench", "fr", TestName = "GetString_OverloadThatTakesListOfLanguages_WorksWithFolders_5")]
		[TestCase(new[] { "fr", "en" }, "blahInFrench", "fr", TestName = "GetString_OverloadThatTakesListOfLanguages_WorksWithFolders_6")]
		[TestCase(new[] { "ar", "fr", "en" }, "blahInFrench", "fr", TestName = "GetString_OverloadThatTakesListOfLanguages_WorksWithFolders_7")] // our arabic doesn't have a translation of 'blah', so fall to French
		public void GetString_OverloadThatTakesListOfLanguages_WorksWithFolders(IEnumerable<string> preferredLangIds,  string expectedResult, string expectedLanguage)
		{
			LocalizationManager.UseLanguageCodeFolders = true;
			using (var folder = new TempFolder())
			{
				AddRandomTranslation("ii", GetInstalledDirectory(folder));
				SetupManager(folder, "ii" /* UI language not important */);

				// SUT
				var result = LocalizationManager.GetString("blahId", "blahInEnglishCode", "comment", preferredLangIds, out var languageFound);

				Assert.AreEqual(expectedResult, result);
				Assert.AreEqual(expectedLanguage, languageFound);
			}
		}

		[Test]
		public void GetUiLanguages_AzeriHasHackedNativeName()
		{
			// Check if the OS includes 'az' culture - Ubuntu 12.04 Precise doesn't
			if (!CultureInfo.GetCultures(CultureTypes.NeutralCultures).Any(c => c.Name == "az"))
				Assert.Ignore("Test requires the availability of the 'az' culture");

			var cultures = LocalizationManager.GetUILanguages(false);
			Assert.AreEqual("Azərbaycan dili", cultures.Where(c => c.Name == "az").Select(c => c.NativeName).FirstOrDefault());
		}

		/// <summary>
		/// - "Installs" English, Arabic, and French
		/// - Sets the UI language
		/// - Constructs a LocalizationManager and adds it to LocalizationManagerInternal<T>.LoadedManagers[AppId]
		/// </summary>
		public void SetupManager(TempFolder folder, string uiLanguageId = LocalizationManager.kDefaultLang)
		{
			LocalizationManagerInternal<T>.LoadedManagers.Clear();
			AddEnglishTranslation(GetInstalledDirectory(folder), AppVersion);
			AddArabicTranslation(GetInstalledDirectory(folder));
			AddFrenchTranslation(GetInstalledDirectory(folder));
			AddSpanishTranslation(GetInstalledDirectory(folder));

			LocalizationManager.SetUILanguage(uiLanguageId, true);
			var manager = CreateLocalizationManager(AppId, AppName, AppVersion,
				GetInstalledDirectory(folder), GetGeneratedDirectory(folder), GetUserModifiedDirectory(folder));

			// REVIEW (Hasso) 2015.01: Since AppId is static, I wonder what conflicts this may cause (even though each test has its own TempFolder)
			LocalizationManagerInternal<T>.LoadedManagers[AppId] = manager;
		}

		protected static string GetInstalledDirectory(TempFolder parentDir)
		{
			return parentDir.Path; // no reason we can't use the root temp dir for the "installed" Translation's
		}

		protected static string GetGeneratedDirectory(TempFolder parentDir)
		{
			return parentDir.Combine("generated");
		}

		protected static string GetUserModifiedDirectory(TempFolder parentDir)
		{
			return parentDir.Combine("userModified");
		}

		private void AddEnglishTranslation(string folderPath, string appVersion)
		{
			var englishDoc = CreateNewDocument(appVersion, "en");

			// first unit
			var tu = CreateTransUnit("theId", false,
				CreateTransUnitVariant("en", "from English Translation"),
				null, "Test");
			englishDoc.AddTransUnit(tu);
			// second unit
			var tu2 = CreateTransUnit("notUsedId", false,
				CreateTransUnitVariant("en", "no longer used English text"),
				noLongerUsed: true);
			englishDoc.AddTransUnit(tu2);
			// third unit
			var tu3 = CreateTransUnit("blahId", false,
				CreateTransUnitVariant("en", "blah"));
			englishDoc.AddTransUnit(tu3);
			englishDoc.Save(Path.Combine(folderPath, LocalizationManager.GetTranslationFileNameForLanguage(AppId, "en")));
		}

		private void AddArabicTranslation(string folderPath)
		{
			var arabicDoc = CreateNewDocument(null, "en", "ar");
			// first unit
			var tu = CreateTransUnit("theId", false,
				CreateTransUnitVariant("en", "wrong"),
				CreateTransUnitVariant("ar", "inArabic"),
				"Test", TranslationStatus.Approved);
			arabicDoc.AddTransUnit(tu);
			// second unit
			var tu2 = CreateTransUnit("notUsedId", false,
				CreateTransUnitVariant("en", "inEnglishpartofArabicTranslation"),
				CreateTransUnitVariant("ar", "inArabic"),
				null, TranslationStatus.Approved);
			arabicDoc.AddTransUnit(tu2);
			arabicDoc.Save(Path.Combine(folderPath, LocalizationManager.GetTranslationFileNameForLanguage(AppId, "ar")));
		}

		private void AddFrenchTranslation(string folderPath)
		{
			var doc = CreateNewDocument(null, "en", "fr");
			// first unit
			var tu = CreateTransUnit("blahId", true,
				CreateTransUnitVariant("en", "blah"),
				CreateTransUnitVariant("fr", "blahInFrench"),
				"Test");
			doc.AddTransUnit(tu);
			doc.Save(Path.Combine(folderPath, LocalizationManager.GetTranslationFileNameForLanguage(AppId, "fr")));
		}

		private void AddSpanishTranslation(string folderPath)
		{
			var spanishDoc = CreateNewDocument(null, "en", "es");
			// first unit
			var tu = CreateTransUnit("theId", false,
				CreateTransUnitVariant("en", "from English Translation"),
				CreateTransUnitVariant("es", "from Spanish Translation"),
				"Test", TranslationStatus.Approved);
			spanishDoc.AddTransUnit(tu);
			// second unit
			var tu2 = CreateTransUnit("notUsedId", false,
				CreateTransUnitVariant("en", "no longer used English text"),
				CreateTransUnitVariant("es", "no longer used Spanish text"),
				null, TranslationStatus.Approved);
			spanishDoc.AddTransUnit(tu2);
			// third unit
			var tu3 = CreateTransUnit("blahId", false,
				CreateTransUnitVariant("en", "blah"),
				CreateTransUnitVariant("es", "bleah"));
			spanishDoc.AddTransUnit(tu3);
			spanishDoc.Save(Path.Combine(folderPath, LocalizationManager.GetTranslationFileNameForLanguage(AppId, "es")));
		}

		protected void AddRandomTranslation(string langId, string folderPath)
		{
			var doc = CreateNewDocument(null, "en", langId);
			// only unit
			var tu2 = CreateTransUnit("notUsedId", false,
				CreateTransUnitVariant("en", "no longer used English text"),
				CreateTransUnitVariant(langId, "no longer used Random text"),
				null, TranslationStatus.Approved);
			doc.AddTransUnit(tu2);
			doc.Save(Path.Combine(folderPath, LocalizationManager.GetTranslationFileNameForLanguage(AppId, langId)));
		}

		/// <summary>
		/// Although it would normally be plausible that AnotherContext.AnotherDialog.Title results from a renaming of
		/// SomeContext.SomeDialog.Title, in the English Translation we don't allow this. We expect the installed English Translation
		/// to be up-to-date.
		/// </summary>
		[Test]
		public void OrphanLogicNotUsedForEnglish()
		{
			using (var folder = new TempFolder())
			{
				MakeEnglishTranslationWithApparentOrphan(folder);
				var manager = CreateLocalizationManager(AppId, AppName, AppVersion,
					GetInstalledDirectory(folder), GetUserModifiedDirectory(folder), GetUserModifiedDirectory(folder));

				LocalizationManagerInternal<T>.LoadedManagers[AppId] = manager;

				Assert.That(LocalizationManager.GetDynamicStringOrEnglish(AppId, "SuperClassMethod.TestId", null, null, "en"), Is.EqualTo("Title"));
				// This will fail if we treated it as an orphan: the ID will not occur at all in the Translation.
				Assert.That(LocalizationManager.GetDynamicStringOrEnglish(AppId, "AnotherContext.AnotherDialog.TestId", null, null, "en"), Is.EqualTo("Title"));
			}
		}

		/// <summary>
		/// Make sure the orphan logic IS used where we want it. Arabic is a good test because it will be loaded before the English.
		/// </summary>
		[Test]
		public void OrphanLogicUsedForArabic_ButNotIfFoundInEnglishTranslation()
		{
			using (var folder = new TempFolder())
			{
				MakeEnglishTranslationWithApparentOrphan(folder);
				MakeArabicTranslationWithApparentOrphans(folder, "Title");

				var manager = CreateLocalizationManager(AppId, AppName, AppVersion,
					GetInstalledDirectory(folder), GetUserModifiedDirectory(folder), GetUserModifiedDirectory(folder));

				LocalizationManagerInternal<T>.LoadedManagers[AppId] = manager;
				LocalizationManager.SetUILanguage("ar", false);

				Assert.That(LocalizationManager.GetDynamicStringOrEnglish(AppId, "SuperClassMethod.TestId", null, null, "en"), Is.EqualTo("Title"));
				Assert.That(LocalizationManager.GetDynamicStringOrEnglish(AppId, "AnotherContext.AnotherDialog.TestId", null, null, "en"), Is.EqualTo("Title"));
				Assert.That(LocalizationManager.GetDynamicStringOrEnglish(AppId, "SuperClassMethod.TestId", null, null, "ar"), Is.EqualTo("Title in Arabic")); // remapped AnObsoleteNameForSuperclass.TestId
				Assert.That(LocalizationManager.GetDynamicStringOrEnglish(AppId, "AnotherContext.AnotherDialog.TestId", null, null, "ar"), Is.EqualTo("Title in Arabic")); // not remapped, since English Translation validates it
			}
		}

		[Test]
		public void OrphanLogicNotUsed_WithWrongEnglishSource()
		{
			using (var folder = new TempFolder())
			{
				MakeEnglishTranslationWithApparentOrphan(folder);
				// The critical difference compared to OrphanLogicUsedForArabic_ButNotIfFoundInEnglishTranslation is that the English version of the orphan doesn't match
				MakeArabicTranslationWithApparentOrphans(folder, "Some other Title, unrelated to SuperclassMethod.TestId");

				var manager = CreateLocalizationManager(AppId, AppName, AppVersion,
					GetInstalledDirectory(folder), GetUserModifiedDirectory(folder), GetUserModifiedDirectory(folder));

				LocalizationManagerInternal<T>.LoadedManagers[AppId] = manager;
				LocalizationManager.SetUILanguage("ar", false);

				Assert.That(LocalizationManager.GetDynamicStringOrEnglish(AppId, "SuperClassMethod.TestId", null, null, "en"), Is.EqualTo("Title"));
				Assert.That(LocalizationManager.GetDynamicStringOrEnglish(AppId, "AnotherContext.AnotherDialog.TestId", null, null, "en"), Is.EqualTo("Title"));
				Assert.That(LocalizationManager.GetDynamicStringOrEnglish(AppId, "SuperClassMethod.TestId", null, null, "ar"), Is.EqualTo("Title")); // no remapping, English doesn't match, so we have no Arabic, use English
				Assert.That(LocalizationManager.GetDynamicStringOrEnglish(AppId, "AnotherContext.AnotherDialog.TestId", null, null, "ar"), Is.EqualTo("Title in Arabic")); // not remapped, since English Translation validates it
				// We don't really care what happens to the entry for AnObsoleteNameForSuperclass.TestId, since presumably it is truly obsolete.
			}
		}

		private void MakeEnglishTranslationWithApparentOrphan(TempFolder folder)
		{
			var englishDoc = CreateNewDocument(AppVersion, "en", otherAppVersion: LowerVersion);
			englishDoc.AddTransUnit(CreateTransUnit("SuperClassMethod.TestId", false,
				CreateTransUnitVariant("en", "Title"),
				CreateTransUnitVariant("en", null))); // This is the one ID found in our test code
			englishDoc.AddTransUnit(CreateTransUnit("AnotherContext.AnotherDialog.TestId", true,
				CreateTransUnitVariant("en", "Title"))); // Simulates an 'orphan' that we can't otherwise tell we need.
			Directory.CreateDirectory(GetInstalledDirectory(folder));
			englishDoc.Save(Path.Combine(GetInstalledDirectory(folder), LocalizationManager.GetTranslationFileNameForLanguage(AppId, "en")));
		}

		private void MakeArabicTranslationWithApparentOrphans(TempFolder folder, string englishForObsoleteTitle)
		{
			var arabicDoc = CreateNewDocument(null, "en", "ar", LowerVersion);
			// Note that we do NOT have arabic for SuperClassMethod.TestId. We may end up getting a translation from the orphan, however.
			arabicDoc.AddTransUnit(CreateTransUnit("AnotherContext.AnotherDialog.TestId", true,
				CreateTransUnitVariant("en", "Title"),
				CreateTransUnitVariant("ar", "Title in Arabic"))); // Not an orphan, because English Translation has this too
			// Interpreted as an orphan iff englishForObsoleteTitle is "Title" (matching the English for SuperClassMethod.TestId)
			arabicDoc.AddTransUnit(CreateTransUnit("AnObsoleteNameForSuperclass.TestId", true,
				CreateTransUnitVariant("en", englishForObsoleteTitle),
				CreateTransUnitVariant("ar", "Title in Arabic")));
			Directory.CreateDirectory(GetInstalledDirectory(folder));
			arabicDoc.Save(Path.Combine(GetInstalledDirectory(folder), LocalizationManager.GetTranslationFileNameForLanguage(AppId, "ar")));
		}

		[Test]
		public void GetAvailableUILanguageTags_FindsThreeLanguages()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder, "en");
				var lm = LocalizationManagerInternal<T>.LoadedManagers.Values.First();
				var tags = lm.GetAvailableUILanguageTags().ToArray();
				Assert.That(tags.Length, Is.EqualTo(4));
				Assert.That(tags.Contains("ar"), Is.True);
				Assert.That(tags.Contains("en"), Is.True);
				Assert.That(tags.Contains("es"), Is.True);
				Assert.That(tags.Contains("fr"), Is.True);
			}
		}

		protected XLiffTransUnit CreateTestTransUnit(string id, string source, string[] notes, bool
		isDynamic)
		{
			var tu = new XLiffTransUnit();
			tu.Id = id;
			tu.Source = new XLiffTransUnitVariant();
			tu.Source.Lang = "en";
			tu.Source.Value = source;
			tu.AddNote(String.Format("ID: {0}", id));
			for (int i = 0; i < notes.Length; ++i)
				tu.AddNote("en", notes[i]);
			tu.Dynamic = isDynamic;
			return tu;
		}

		[Test]
		public void TranslationWithNewlineReplacement_YieldsStandardizedNewline()
		{
			LocalizationManagerInternal<T>.LoadedManagers.Clear();
			using (var folder = new TempFolder())
			{
				AddArabicTranslation(GetInstalledDirectory(folder));
				AddFrenchTranslation(GetInstalledDirectory(folder));
				AddSpanishTranslation(GetInstalledDirectory(folder));

				var englishDoc = CreateNewDocument(null, "en");
				// first unit
				var tu = CreateTransUnit("theId", false,
					CreateTransUnitVariant("en", "from English\\n Translation"));
				englishDoc.AddTransUnit(tu);
				englishDoc.Save(Path.Combine(folder.Path, LocalizationManager.GetTranslationFileNameForLanguage(AppId, "en")));

				var doc = CreateNewDocument(null, "en", "fr");
				// first unit
				var tuF = CreateTransUnit("theId", true,
					CreateTransUnitVariant("en", "from English\\n Translation"),
					CreateTransUnitVariant("fr", "from French\\n Translation"));
				doc.AddTransUnit(tuF);
				doc.Save(Path.Combine(folder.Path, LocalizationManager.GetTranslationFileNameForLanguage(AppId, "fr")));

				LocalizationManager.SetUILanguage("fr", true);
				var manager = CreateLocalizationManager(AppId, AppName, AppVersion,
					GetInstalledDirectory(folder), GetGeneratedDirectory(folder), GetUserModifiedDirectory(folder));
				LocalizationManagerInternal<T>.LoadedManagers[AppId] = manager;
				Assert.That(LocalizationManager.GetString("theId", "from English\\n Translation"), Is.EqualTo("from French" + LocalizedStringCache.kOSRealNewline + " Translation"));

				// what about one added dynamically, as if discovered in code?
				// We'd like it to correctly handle both the current environment.Newline and a literal \r\n
				// The latter may well fail on Linux though passing on Windows.
				var li = new LocalizingInfo("anotherId") { Text = "Three\r\nlines of" + Environment.NewLine + "French", LangId = "fr" };
				manager.StringCache.UpdateLocalizedInfo(li);
				// The actual stored value, that would be written to the Translation, should have the replacement text in it.
				Assert.That(manager.StringCache.GetValueForExactLangAndId("fr", "anotherId", false), Is.EqualTo("Three\\nlines of\\nFrench"));
				Assert.That(LocalizationManager.GetString("anotherId", "Three\r\nlines of" + Environment.NewLine + "English"), Is.EqualTo("Three" + LocalizedStringCache.kOSRealNewline + "lines of" + LocalizedStringCache.kOSRealNewline + "French"));
			}
		}

		[Test]
		public void TestInexactLanguageMatching()
		{
			// Note that there are no loaded localization managers, so the initial processing
			// of setting the UI language will result in the default fallback list and an
			// unchanged UI language.  Calling the method to set the fallback languages will
			// possibly change the UI language to something less (or more) specific, and
			// possibly have a longer fallback list.
			LocalizationManager.SetUILanguage("es", true);
			Assert.AreEqual("es", LocalizationManager.UILanguageId);
			Assert.AreEqual(1, LocalizationManager.FallbackLanguageIds.Count());
			Assert.AreEqual("en", LocalizationManager.FallbackLanguageIds.First());
			LocalizationManagerInternal<T>.SetAvailableFallbackLanguageIds(new [] {"en", "es-ES",
			"fr", "pt-PT", "zh-CN"});
			Assert.AreEqual("es-ES", LocalizationManager.UILanguageId);
			Assert.AreEqual(1, LocalizationManager.FallbackLanguageIds.Count());
			Assert.AreEqual("en", LocalizationManager.FallbackLanguageIds.First());

			LocalizationManager.SetUILanguage("fr-FR", true);
			Assert.AreEqual("fr-FR", LocalizationManager.UILanguageId);
			Assert.AreEqual(1, LocalizationManager.FallbackLanguageIds.Count());
			Assert.AreEqual("en", LocalizationManager.FallbackLanguageIds.First());
			LocalizationManagerInternal<T>.SetAvailableFallbackLanguageIds(new [] {"en", "es-ES", "fr", "pt-PT", "zh-CN"});
			Assert.AreEqual("fr", LocalizationManager.UILanguageId);
			Assert.AreEqual(1, LocalizationManager.FallbackLanguageIds.Count());
			Assert.AreEqual("en", LocalizationManager.FallbackLanguageIds.First());

			LocalizationManager.SetUILanguage("en-GB", true);
			Assert.AreEqual("en-GB", LocalizationManager.UILanguageId);
			Assert.AreEqual(1, LocalizationManager.FallbackLanguageIds.Count());
			Assert.AreEqual("en", LocalizationManager.FallbackLanguageIds.First());
			LocalizationManagerInternal<T>.SetAvailableFallbackLanguageIds(new [] {"en", "en-GB", "en-US", "es-ES", "fr", "pt-PT", "zh-CN"});
			Assert.AreEqual("en-GB", LocalizationManager.UILanguageId);
			var fallbacks = LocalizationManager.FallbackLanguageIds.ToList();
			Assert.AreEqual(2, fallbacks.Count);
			Assert.AreEqual("en", fallbacks[0]);
			Assert.AreEqual("en-US", fallbacks[1]);
			LocalizationManagerInternal<T>.SetAvailableFallbackLanguageIds(new [] {"en", "es-ES", "fr", "pt-PT", "zh-CN"});
			Assert.AreEqual("en", LocalizationManager.UILanguageId);
			Assert.AreEqual(1, LocalizationManager.FallbackLanguageIds.Count());
			Assert.AreEqual("en", LocalizationManager.FallbackLanguageIds.First());

			LocalizationManager.SetUILanguage("en-GB", true);
			Assert.AreEqual("en-GB", LocalizationManager.UILanguageId);
			Assert.AreEqual(1, LocalizationManager.FallbackLanguageIds.Count());
			Assert.AreEqual("en", LocalizationManager.FallbackLanguageIds.First());
			LocalizationManagerInternal<T>.SetAvailableFallbackLanguageIds(new [] {"en", "en-US", "es-ES", "fr", "pt-PT", "zh-CN"});
			Assert.AreEqual("en", LocalizationManager.UILanguageId);
			fallbacks = LocalizationManager.FallbackLanguageIds.ToList();
			Assert.AreEqual(2, fallbacks.Count);
			Assert.AreEqual("en-US", fallbacks[0]);
			Assert.AreEqual("en", fallbacks[1]);
		}

		[Test]
		public void GetString_UsesFallbackLanguages()
		{
			using (var folder = new TempFolder())
			{
				LocalizationManagerInternal<T>.LoadedManagers.Clear();
				var doc = CreateNewDocument(null, "en", "ha");
				// first unit
				var tu = CreateTransUnit("blahId", true,
					CreateTransUnitVariant("en", "blah"),
					CreateTransUnitVariant("ha", "blahInHausa"));
				doc.AddTransUnit(tu);
				doc.Save(Path.Combine(folder.Path, LocalizationManager.GetTranslationFileNameForLanguage(AppId, "ha")));

				AddEnglishTranslation(GetInstalledDirectory(folder), AppVersion);
				AddFrenchTranslation(GetInstalledDirectory(folder));
				AddSpanishTranslation(GetInstalledDirectory(folder));

				LocalizationManager.SetUILanguage("ha", true);
				var manager = CreateLocalizationManager(AppId, AppName, AppVersion,
					GetInstalledDirectory(folder), GetGeneratedDirectory(folder), GetUserModifiedDirectory(folder));
				LocalizationManagerInternal<T>.LoadedManagers[AppId] = manager;

				Assert.That(LocalizationManager.GetString("blahId", "blah"), Is.EqualTo("blahInHausa"));
				Assert.That(LocalizationManager.GetString("theId", "program English Id"), Is.EqualTo("program English Id")); // Translation has "from English Translation" but we prefer the program-supplied value

				LocalizationManager.FallbackLanguageIds = new[] {"es", "en"};
				Assert.That(LocalizationManager.GetString("blahId", "blah"), Is.EqualTo("blahInHausa")); // still from primary
				Assert.That(LocalizationManager.GetString("theId", "nonsense"), Is.EqualTo("from Spanish Translation")); // from fallback

				LocalizationManager.FallbackLanguageIds = new[] { "fr", "en" };
				Assert.That(LocalizationManager.GetString("blahId", "blah"), Is.EqualTo("blahInHausa")); // still from primary
				Assert.That(LocalizationManager.GetString("theId", "program English Id"), Is.EqualTo("program English Id")); // French doesn't have this either.

				LocalizationManager.FallbackLanguageIds = new[] { "fr", "es", "en" };
				Assert.That(LocalizationManager.GetString("blahId", "blah"), Is.EqualTo("blahInHausa")); // still from primary
				Assert.That(LocalizationManager.GetString("theId", "nonsense"), Is.EqualTo("from Spanish Translation")); // from 2nd fallback
			}
		}

		// This mimics the situation we have in Bloom for our Spanish translations from Crowdin.
		internal void AddOverSpecifiedSpanishTranslation(string folderPath)
		{
			var spanishDoc = CreateNewDocument(null, "en", "es-ES");
			// first unit
			var tu = CreateTransUnit("theId", false,
				CreateTransUnitVariant("en", "from English Translation"),
				CreateTransUnitVariant("es-ES", "from Spanish Translation"),
				"Test", TranslationStatus.Approved);
			spanishDoc.AddTransUnit(tu);
			// second unit
			var tu2 = CreateTransUnit("notUsedId", false,
				CreateTransUnitVariant("en", "no longer used English text"),
				CreateTransUnitVariant("es-ES", "no longer used Spanish text"),
				null, TranslationStatus.Approved);
			spanishDoc.AddTransUnit(tu2);
			// third unit
			var tu3 = CreateTransUnit("blahId", false,
				CreateTransUnitVariant("en", "blah"),
				CreateTransUnitVariant("es-ES", "bleah"));
			spanishDoc.AddTransUnit(tu3);
			spanishDoc.Save(Path.Combine(folderPath, LocalizationManager.GetTranslationFileNameForLanguage(AppId, "es")));
		}

		[Test]
		public void TestMappingLanguageCodesToAvailable()
		{
			LocalizationManager.SetUILanguage("en", true);
			LocalizationManagerInternal<T>.LoadedManagers.Clear();
			using (var folder = new TempFolder())
			{
				AddEnglishTranslation(GetInstalledDirectory(folder), "1.0");
				AddArabicTranslation(GetInstalledDirectory(folder));
				AddFrenchTranslation(GetInstalledDirectory(folder));
				AddOverSpecifiedSpanishTranslation(GetInstalledDirectory(folder));
				var manager = CreateLocalizationManager(AppId, AppName, AppVersion,
					GetInstalledDirectory(folder), GetGeneratedDirectory(folder), GetUserModifiedDirectory(folder));
				LocalizationManagerInternal<T>.LoadedManagers[AppId] = manager;

				var langs = LocalizationManager.GetAvailableLocalizedLanguages();
				// REVIEW: currently TMX returns 'es' instead of 'es-ES'. Should this be changed?
				var es = LocalizationManager.TranslationMemoryKind == TranslationMemory.Tmx
					? "es"
					: "es-ES";
				Assert.That(langs, Is.EquivalentTo(new[] { "en", "ar", "fr", es}));

				Assert.IsTrue(LocalizationManager.GetIsStringAvailableForLangId("theId", "es-ES"));
				Assert.IsTrue(LocalizationManager.GetIsStringAvailableForLangId("theId", "es"));
				Assert.IsTrue(LocalizationManager.GetIsStringAvailableForLangId("theId", "es-MX"));
				Assert.IsTrue(LocalizationManager.GetIsStringAvailableForLangId("theId", "ar"));
				Assert.IsTrue(LocalizationManager.GetIsStringAvailableForLangId("theId", "ar-AR"));
				Assert.IsFalse(LocalizationManager.GetIsStringAvailableForLangId("theId", "fr"));
				Assert.IsFalse(LocalizationManager.GetIsStringAvailableForLangId("theId", "fr-FR"));

				// Check that we return the provided string for English.
				var str = LocalizationManager.GetString("theId", "This is a test!", "This is only a test?", new[]{ "en", "fr", "ar" }, out var languageIdUsed);
				Assert.AreEqual("This is a test!", str);
				Assert.AreEqual("en", languageIdUsed);

				// Check that asking for a specific form of English still returns the provided string.
				str = LocalizationManager.GetString("theId", "This is a test!", "This is only a test?", new []{ "en-US", "es-ES", "fr-FR" }, out languageIdUsed);
				Assert.AreEqual("This is a test!", str);
				Assert.AreEqual("en", languageIdUsed);

				// Check that we return the string from the second language when the first language doesn't have the string.
				str = LocalizationManager.GetString("theId", "This is a test!", "This is only a test?", new[]{ "fr", "ar", "es" }, out languageIdUsed);
				Assert.AreEqual("inArabic", str);
				Assert.AreEqual("ar", languageIdUsed);

				// Check that we return the string from the first language when it exists.
				str = LocalizationManager.GetString("theId", "This is a test!", "This is only a test?", new []{ "es-ES", "en", "fr" }, out languageIdUsed);
				Assert.AreEqual("from Spanish Translation", str);
				Assert.AreEqual("es-ES", languageIdUsed);

				// Check asking for the general form of the language when we have only a specific form.
				str = LocalizationManager.GetString("theId", "This is a test!", "This is only a test?", new []{ "es", "en", "fr" }, out languageIdUsed);
				Assert.AreEqual("from Spanish Translation", str);
				Assert.AreEqual("es-ES", languageIdUsed);

				// Check asking for a specific form of the language when we have only the general form.
				str = LocalizationManager.GetString("theId", "This is a test!", "This is only a test?", new []{ "ar-AR", "en", "fr" }, out languageIdUsed);
				Assert.AreEqual("inArabic", str);
				Assert.AreEqual("ar", languageIdUsed);

				// Check asking for a specific form of the language when we have only a different specific form.
				str = LocalizationManager.GetString("theId", "This is a test!", "This is only a test?", new []{ "es-MX", "en-GB", "fr-FR" }, out languageIdUsed);
				Assert.AreEqual("from Spanish Translation", str);
				Assert.AreEqual("es-ES", languageIdUsed);
			}
		}
	}
}
