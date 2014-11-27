using System.Collections.Generic;
using System.Windows.Forms;
using L10NSharp.TMXUtils;
using L10NSharp.UI;
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

		[Test]
		public void LocalizedStringCache_LoadGroupNodes_DoesntLoadNoLongerUsedUnits()
		{
			using (var folder = new TempFolder("LoadGroupNodes_EnglishTMXHasNoLongerUsedProperty_ArabicDoesnt_NoLongerUsedWins"))
			{
				SetupManager(folder, "ar");
				var mgr = LocalizationManager.LoadedManagers["test"];
				var treeView = new TreeView();
				var node = new LocTreeNode(mgr, mgr.Name, null, mgr.Name);
				treeView.Nodes.Add(node);
				var treeNodes = node.Nodes;
				mgr.StringCache.LoadGroupNodes(treeNodes);
				Assert.IsFalse(treeNodes.ContainsKey("notUsedId"));
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
			// first unit
			var variants = new List<TransUnitVariant>();
			variants.Add(new TransUnitVariant() {Lang = "en", Value = "from English TMX"});
			var tu = new TransUnit()
			{
				Id = "theId",
				Variants = variants
			};
			tu.AddProp("en", LocalizedStringCache.kDiscoveredDyanmically, "true");
			englishDoc.AddTransUnit(tu);
			// second unit
			var variants2 = new List<TransUnitVariant>();
			variants2.Add(new TransUnitVariant() { Lang = "en", Value = "no longer used English text" });
			var tu2 = new TransUnit()
			{
				Id = "notUsedId",
				Variants = variants2
			};
			tu2.AddProp("en", LocalizedStringCache.kDiscoveredDyanmically, "true");
			tu2.AddProp("en", LocalizedStringCache.kNoLongerUsedPropTag, "true");
			englishDoc.AddTransUnit(tu2);
			englishDoc.Save(folder.Combine("test.en.tmx"));
		}

		private static void AddArabicTMX(TempFolder folder)
		{
			var arabicDoc = new TMXDocument();
			arabicDoc.Header.SourceLang = "ar";

			// first unit
			var variants = new List<TransUnitVariant>();
			variants.Add(new TransUnitVariant() {Lang = "en", Value = "wrong"});
			variants.Add(new TransUnitVariant() {Lang = "ar", Value = "inArabic"});
			var tu = new TransUnit()
			{
				Id = "theId",
				Variants = variants
			};
			tu.AddProp("ar", LocalizedStringCache.kDiscoveredDyanmically, "true");
			tu.AddProp("en", LocalizedStringCache.kDiscoveredDyanmically, "true");
			arabicDoc.AddTransUnit(tu);
			// second unit
			var variants2 = new List<TransUnitVariant>();
			variants2.Add(new TransUnitVariant() { Lang = "en", Value = "inEnglishpartofArabicTMX" });
			variants2.Add(new TransUnitVariant() { Lang = "ar", Value = "inArabic" });
			var tu2 = new TransUnit()
			{
				Id = "notUsedId",
				Variants = variants2
			};
			tu2.AddProp("ar", LocalizedStringCache.kDiscoveredDyanmically, "true");
			tu2.AddProp("en", LocalizedStringCache.kDiscoveredDyanmically, "true");
			// Note: we are NOT adding a NoLongerUsed property to the Arabic TMX
			arabicDoc.AddTransUnit(tu2);
			arabicDoc.Save(folder.Combine("test.ar.tmx"));
		}
	}
}