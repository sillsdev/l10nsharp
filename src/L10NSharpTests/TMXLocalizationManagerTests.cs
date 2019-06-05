// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
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
			params string[] namespaceBeginnings)
		{
			return new TMXLocalizationManager(appId, appName, appVersion, directoryOfInstalledTmxFiles,
				directoryForGeneratedDefaultTmxFile, directoryOfUserModifiedXliffFiles,
				namespaceBeginnings);
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
		public void GetCustomUILanguageTags_FindsFiveLanguages()
		{
			using (var folder = new TempFolder())
			{
				SetupManager(folder);
				AddRandomTranslation("ii", GetUserModifiedDirectory(folder));
				var lm = LocalizationManager.LoadedManagers.Values.First();
				var tags = lm.GetAvailableUILanguageTags().ToArray();
				Assert.That(tags.Length, Is.EqualTo(5));
				Assert.That(tags.Contains("ii"), Is.True);
			}
		}
	}
}
