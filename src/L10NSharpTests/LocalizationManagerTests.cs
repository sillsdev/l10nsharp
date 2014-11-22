using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using L10NSharp.TMXUtils;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class LocalizationManagerTests
	{
		/// <summary>
		/// This is a regression test. As of Nov 2014, if we updated an English string, it would
		/// get overwritten by the old version, found in another language's TMX.
		/// </summary>
		[Test]
		public void GetDynamicString_EnglishAndArabicHaveDifferentValuesOfEnglishString_EnglishTMXWins()
		{
			using(var folder = new TempFolder("GetString_EnglishAndArabicHaveDifferentValuesOfEnglishString_EnglishTMXWins"))
			{
				SetupManager(folder, "en");
				//This was the original assertion, and it worked:
				//   Assert.AreEqual("from English TMX", LocalizationManager.GetDynamicString("test","theId", "some default"));
				//However, later I decided, I don't care what is in the English TMX, either. If the c# code just gave a new
				// value for this, what the c# code said should win. Who cares what's in the English TMX. It's only real
				// purpose in life is to provide a list of strings that have been disovered dynamically when the translator
				// needs a list of strings to translate (without it, the translator would have to cause the program to visit
				// each part of the UI so as to trip over all the GetDynamicString() calls.

				//So now, this is correct:

				Assert.AreEqual("from the c# code", LocalizationManager.GetDynamicString("test", "theId", "from the c# code"));
			}
		}
		/// <summary>
		/// This is a regression test. If the English TMX is out of date, too bad. The c# code always wins.
		/// </summary>
		[Test]
		public void GetDynamicString_EnglishTMXHasDIfferentStringThanParamater_ParameterWins()
		{
			using(var folder = new TempFolder("GetDynamicString_EnglishTMXHasDIfferentStringThanParameter_ParameterWins"))
			{
				SetupManager(folder, "en");
				Assert.AreEqual("from c# code", LocalizationManager.GetDynamicString("test", "theId", "from c# code"));
			}
		}

		[Test]
		public void GetDynamicString_ArabicTMXHasArabicValue_ArabicTMXWins()
		{
			using(var folder = new TempFolder("GetDynamicString_EnglishTMXHasDIfferentStringThanParameter_ParameterWins"))
			{
				SetupManager(folder, "ar");
				Assert.AreEqual("inArabic", LocalizationManager.GetDynamicString("test", "theId", "from c# code"));
			}
		}

		private static void SetupManager(TempFolder folder, string uiLanguageId)
		{
			AddEnglishTMX(folder);
			AddArabicTMX(folder);

			string directoryOfGeneratedDefaultTmxFile = folder.Combine("generated");
			string directoryOfUserModifiedTmxFiles = folder.Combine("userModified");

			LocalizationManager.SetUILanguage(uiLanguageId, true);
			var manager = new LocalizationManager("test", "unit test", "1.0.0", folder.Path, directoryOfGeneratedDefaultTmxFile,
				directoryOfUserModifiedTmxFiles,
				new string[] {});

			LocalizationManager.LoadedManagers["test"] = manager;
		}

		private static void AddEnglishTMX(TempFolder folder)
		{
			var englishDoc = new TMXDocument();
			englishDoc.Header.SourceLang = "en";
			var variants = new List<TransUnitVariant>();
			variants.Add(new TransUnitVariant() {Lang = "en", Value = "from English TMX"});
			var tu = new TransUnit()
			{
				Id = "theId",
				Variants = variants
			};
			tu.AddProp("en", LocalizedStringCache.kDiscoveredDyanmically, "true");
			englishDoc.AddTransUnit(tu);
			englishDoc.Save(folder.Combine("test.en.tmx"));
		}

		private static void AddArabicTMX(TempFolder folder)
		{
			List<TransUnitVariant> variants;
			TransUnit tu;
			var arabicDoc = new TMXDocument();
			arabicDoc.Header.SourceLang = "ar";

			variants = new List<TransUnitVariant>();
			variants.Add(new TransUnitVariant() {Lang = "en", Value = "wrong"});
			variants.Add(new TransUnitVariant() {Lang = "ar", Value = "inArabic"});
			tu = new TransUnit()
			{
				Id = "theId",
				Variants = variants
			};
			tu.AddProp("ar", LocalizedStringCache.kDiscoveredDyanmically, "true");
			tu.AddProp("en", LocalizedStringCache.kDiscoveredDyanmically, "true");
			arabicDoc.AddTransUnit(tu);
			arabicDoc.Save(folder.Combine("test.ar.tmx"));
		}
	}
}