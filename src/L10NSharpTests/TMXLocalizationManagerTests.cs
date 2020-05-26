// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using L10NSharp.TMXUtils;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class TMXLocalizationManagerTests : LocalizationManagerTestsBase<TMXDocument>
	{
		[SetUp]
		public void Setup()
		{
			LocalizationManager.TranslationMemoryKind = TranslationMemory.Tmx;
		}

		internal override ILocalizationManagerInternal<TMXDocument> CreateLocalizationManager(
			string          appId,                               string appName, string appVersion, string directoryOfInstalledTmxFiles,
			string          directoryForGeneratedDefaultTmxFile, string directoryOfUserModifiedXliffFiles,
			IEnumerable<MethodInfo> additionalGetStringMethodInfo = null,
			params string[] namespaceBeginnings)
		{
			return new TMXLocalizationManager(appId, appName, appVersion, directoryOfInstalledTmxFiles,
				directoryForGeneratedDefaultTmxFile, directoryOfUserModifiedXliffFiles,
				additionalGetStringMethodInfo, namespaceBeginnings);
		}

		internal override ILocalizationManagerInternal<TMXDocument> CreateLocalizationManager(
			string appId, string appName, string appVersion)
		{
			return new TMXLocalizationManager(appId, appName, appVersion);
		}

		protected override TMXDocument CreateNewDocument(string productVersion,    string englishLang,
			string                                              sourceLang = null, string otherAppVersion = null)
		{
			var doc = new TMXDocument { Header = { SourceLang = sourceLang } };
			if (!string.IsNullOrEmpty(otherAppVersion))
				doc.Header.SetPropValue(LocalizationManager.kAppVersionPropTag, otherAppVersion);
			else if (!string.IsNullOrEmpty(productVersion))
				doc.Header.SetPropValue(LocalizationManager.kAppVersionPropTag, productVersion);
			return doc;
		}

		protected override ITransUnit CreateTransUnit(string id,              bool               dynamic,
			ITransUnitVariant                                sourceVariant,   ITransUnitVariant  targetVariant     = null,
			string                                           noteText = null, TranslationStatus? translationStatus = null, bool? noLongerUsed = null)
		{
			var variants = new List<TMXTransUnitVariant> { (TMXTransUnitVariant) sourceVariant };
			if (targetVariant != null)
				variants.Add((TMXTransUnitVariant) targetVariant);
			var props = noLongerUsed.HasValue
				? new List<TMXProp> { new TMXProp {
					Type = TMXLocalizedStringCache.kNoLongerUsedPropTag,
					Value =noLongerUsed.Value.ToString()
				}}
				: null;
			var tu = new TMXTransUnit {
				Id = id,
				Variants = variants,
				Props = props
			};
			tu.AddProp(TMXLocalizedStringCache.kDiscoveredDynamically, dynamic.ToString());

			return tu;
		}

		protected override ITransUnitVariant CreateTransUnitVariant(string lang, string value)
		{
			return new TMXTransUnitVariant { Lang = lang, Value = value };
		}

		protected override string GetGeneratedVersion(XElement xmlDoc)
		{
			var headerElt = xmlDoc.Element("header");
			var verElement = headerElt?.Elements("prop").FirstOrDefault(e => (string)e.Attribute("type") == LocalizationManager.kAppVersionPropTag);
			return verElement == null ? null : new Version(verElement.Value).ToString();
		}

		[Test]
		public void GetAvailableUILanguageTags_AddRandomTranslation_FindsTmxFileInUserModifiedDirectory()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);

				// add a custom localization in the User Modified directory
				AddRandomTranslation("ii", GetUserModifiedDirectory(folder));

				// load available localizations
				var lm = LocalizationManager.LoadedManagers.Values.First();
				var tags = lm.GetAvailableUILanguageTags().ToArray();

				// was the 'ii' tag found?
				Assert.That(tags.Contains("ii"), Is.True,
					"Tag 'ii' not found.");

				// is the TMX file in the User Modified directory?
				Assert.That(File.Exists(Path.Combine(GetUserModifiedDirectory(folder), "test.ii.tmx")), Is.True,
					"File 'test.ii.tmx' not found in User Modified directory.");
			}
		}

		[Test]
		public void GetAvailableUILanguageTags_FindsEnglishTmxFileInGeneratedDirectory()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);

				// remove the installed English TMX file
				var installedEnglishFile = Path.Combine(GetInstalledDirectory(folder), "test.en.tmx");
				Assert.That(File.Exists(installedEnglishFile), Is.True,
					"File 'test.en.tmx' not found in Installed directory.");

				File.Delete(installedEnglishFile);
				Assert.That(File.Exists(installedEnglishFile), Is.False,
					"File 'test.en.tmx' was not deleted.");

				// load the remaining localizations
				var lm = LocalizationManager.LoadedManagers.Values.First();
				var tags = lm.GetAvailableUILanguageTags().ToArray();

				// was the 'en' tag found?
				Assert.That(tags.Contains("en"), Is.True,
					"Tag 'en' not found.");

				// is the TMX file in the Generated directory?
				Assert.That(File.Exists(Path.Combine(GetGeneratedDirectory(folder), "test.en.tmx")), Is.True,
					"File 'test.en.tmx' not found in Generated directory.");
			}
		}
	}
}
