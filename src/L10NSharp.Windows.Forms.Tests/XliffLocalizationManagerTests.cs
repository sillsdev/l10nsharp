// Copyright © 2022-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using L10NSharp.XLiffUtils;
using L10NSharp.Windows.Forms.XLiffUtils;
using NUnit.Framework;

namespace L10NSharp.Windows.Forms.Tests
{
	public class XliffLocalizationManagerTests
	{
		protected const string AppId = "test";
		protected const string AppName = "unit test";
		protected const string AppVersion = "1.0.0";
		protected const string HigherVersion = "2.0.0";
		protected const string LowerVersion = "0.0.1";
		protected const string LiteralNewline = "\\n";

		internal ILocalizationManagerInternalWinforms<XLiffDocument> CreateLocalizationManager(
			string appId, string appName, string appVersion, string directoryOfInstalledLocFiles,
			string directoryForGeneratedDefaultFile, string directoryOfUserModifiedXliffFiles,
			IEnumerable<MethodInfo> additionalGetStringMethodInfo = null,
			params string[] namespaceBeginnings)
		{
			var manager = new XliffLocalizationManagerWinforms(appId, null, appName, appVersion, directoryOfInstalledLocFiles,
				directoryForGeneratedDefaultFile, directoryOfUserModifiedXliffFiles, additionalGetStringMethodInfo,
				namespaceBeginnings);
			Assert.That(manager.OriginalExecutableFile, Is.EqualTo(appId + ".dll"));
			LocalizationManagerInternalWinforms<XLiffDocument>.LoadedManagers.Add("myAppId", manager);
			return manager;
		}

		internal ILocalizationManagerInternalWinforms<XLiffDocument> CreateLocalizationManager(
			string appId, string appName, string appVersion)
		{
			return new XliffLocalizationManagerWinforms(appId, appName, appVersion);
		}

		protected XLiffDocument CreateNewDocument(string productVersion, string sourceLang,
			string targetLang = null, string otherAppVersion = null)
		{
			var doc = new XLiffDocument { File = { SourceLang = sourceLang } };
			if (!string.IsNullOrEmpty(productVersion))
				doc.File.ProductVersion = productVersion;
			if (!string.IsNullOrEmpty(targetLang))
				doc.File.TargetLang = targetLang;
			if (!string.IsNullOrEmpty(otherAppVersion))
				doc.File.SetPropValue(LocalizationManager.kAppVersionPropTag, otherAppVersion);
			doc.File.HardLineBreakReplacement = LiteralNewline;
			doc.File.Original = "test.dll";
			return doc;
		}

		protected ITransUnit CreateTransUnit(string id, bool dynamic,
			ITransUnitVariant sourceVariant, ITransUnitVariant targetVariant = null,
			string noteText = null, TranslationStatus? translationStatus = null, bool? noLongerUsed = null)
		{
			var tu = new XLiffTransUnit
			{
				Id = id,
				Source = (XLiffTransUnitVariant)sourceVariant,
				Target = (XLiffTransUnitVariant)targetVariant

			};

			if (dynamic)
				tu.Dynamic = true;

			if (!string.IsNullOrEmpty(noteText))
				tu.Notes = new List<XLiffNote> { new XLiffNote { Text = noteText } };

			if (translationStatus.HasValue)
				tu.TranslationStatus = translationStatus.Value;
			return tu;
		}

		protected ITransUnitVariant CreateTransUnitVariant(string lang, string value)
		{
			return new XLiffTransUnitVariant { Lang = lang, Value = value };
		}

		[TearDown]
		public void TearDownLocalizationManagers()
		{
			LocalizationManager.UseLanguageCodeFolders = false;
			LocalizationManagerInternalWinforms<XLiffDocument>.LoadedManagers.Clear();
			LocalizationManagerInternalWinforms<XLiffDocument>.MapToExistingLanguage.Clear();
			LocalizationManager.SetUILanguage(LocalizationManager.kDefaultLang, false);
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

		private void AddChineseOfChinaTranslation(string folderPath)
		{
			var chineseDoc = CreateNewDocument(null, "en", "zh-CN");
			// first unit
			var tu = CreateTransUnit("theId", false,
				CreateTransUnitVariant("en", "from English Translation"),
				CreateTransUnitVariant("zh-CN", "from Chinese (China) Translation"),
				"Test", TranslationStatus.Approved);
			chineseDoc.AddTransUnit(tu);
			// second unit
			var tu2 = CreateTransUnit("notUsedId", false,
				CreateTransUnitVariant("en", "no longer used English text"),
				CreateTransUnitVariant("zh-CN", "no longer used Chinese (China) text"),
				null, TranslationStatus.Approved);
			chineseDoc.AddTransUnit(tu2);
			// third unit
			var tu3 = CreateTransUnit("blahId", false,
				CreateTransUnitVariant("en", "blah"),
				CreateTransUnitVariant("zh-CN", "中文(中国) blah"));
			chineseDoc.AddTransUnit(tu3);
			chineseDoc.Save(Path.Combine(folderPath, LocalizationManager.GetTranslationFileNameForLanguage(AppId, "zh-CN")));
		}

		private void AddChineseOfTaiwanTranslation(string folderPath)
		{
			var chineseDoc = CreateNewDocument(null, "en", "zh-TW");
			// first unit
			var tu = CreateTransUnit("theId", false,
				CreateTransUnitVariant("en", "from English Translation"),
				CreateTransUnitVariant("zh-TW", "from Chinese (Taiwan) Translation"),
				"Test", TranslationStatus.Approved);
			chineseDoc.AddTransUnit(tu);
			// second unit
			var tu2 = CreateTransUnit("notUsedId", false,
				CreateTransUnitVariant("en", "no longer used English text"),
				CreateTransUnitVariant("zh-TW", "no longer used Chinese (Taiwan) text"),
				null, TranslationStatus.Approved);
			chineseDoc.AddTransUnit(tu2);
			// third unit
			var tu3 = CreateTransUnit("blahId", false,
				CreateTransUnitVariant("en", "blah"),
				CreateTransUnitVariant("zh-TW", "中文(Taiwan) blah"));
			chineseDoc.AddTransUnit(tu3);
			chineseDoc.Save(Path.Combine(folderPath, LocalizationManager.GetTranslationFileNameForLanguage(AppId, "zh-TW")));
		}

		[Test]
		public void TestMappingLanguageCodesToAvailable_AmbiguousOptions_PromptsUser([Values("zh-CN", "zh-TW")] string choice)
		{
			LocalizationManager.SetUILanguage("en", true);
			LocalizationManagerInternalWinforms<XLiffDocument>.LoadedManagers.Clear();
			using (var folder = new L10NSharp.Tests.TempFolder())
			{
				var installedFolder = Path.Combine(folder.Path, "installed");
				// ReSharper disable once AssignNullToNotNullAttribute
				var userRelativeFolder = Path.Combine("Temp", Path.GetFileName(Path.GetDirectoryName(folder.Path)),
					Path.GetFileName(folder.Path), "user");
				AddEnglishTranslation(installedFolder, null);
				AddChineseOfChinaTranslation(installedFolder);
				AddChineseOfTaiwanTranslation(installedFolder);
				var userPromptCount = 0;
				LocalizationManagerInternalWinforms<XLiffDocument>.ChooseFallbackLanguage = (langTag, icon) =>
				{
					userPromptCount++;
					Assert.That(langTag, Is.EqualTo("zh"));
					return choice;
				};
				var manager = LocalizationManagerWinforms.Create("zh", AppId, AppName, AppVersion, installedFolder,
					userRelativeFolder, null, null, new string[] { });
				Assert.That(userPromptCount, Is.EqualTo(1));
				LocalizationManagerInternal<XLiffDocument>.LoadedManagers[AppId] = (ILocalizationManagerInternal<XLiffDocument>)manager;

				var langs = LocalizationManager.GetAvailableLocalizedLanguages();
				Assert.That(langs, Is.EquivalentTo(new[] { "en", "zh-CN", "zh-TW" }));
				Assert.That(LocalizationManager.UILanguageId, Is.EqualTo(choice));

				Assert.That(LocalizationManager.GetIsStringAvailableForLangId("theId", "zh"), Is.False, "zh is ambiguous");
				Assert.That(LocalizationManager.GetIsStringAvailableForLangId("theId", "zh-CN"), Is.True, "zh-CN should find zh-CN");
				Assert.That(LocalizationManager.GetIsStringAvailableForLangId("theId", "zh-TW"), Is.True, "zh-TW should find zh-TW");
				Assert.That(LocalizationManager.GetIsStringAvailableForLangId("theId", "en"), Is.True, "en should find en");
			}
		}
	}
}
