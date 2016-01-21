using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using L10NSharp.TMXUtils;
using L10NSharp.UI;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class LocalizationManagerTests
	{
		private const string AppId = "test";
		private const string AppName = "unit test";
		private const string AppVersion = "1.0.0";
		private const string HigherVersion = "2.0.0";
		private const string LowerVersion = "0.0.1";

		/// <summary>
		/// If there is no GeneratedDefault TMX file, but the file we need has been installed, copy the installed version to circumvent
		/// a crash trying to generate this TMX file on Linux.
		/// </summary>
		[Test]
		public void CreateOrUpdateDefaultTmxFileIfNecessary_CopiesInstalledIfAvailable()
		{
			using(var folder = new TempFolder("CreateOrUpdate_CopiesInstalledIfAvailable"))
			{
				// SUT (buried down in there somewhere)
				SetupManager(folder);

				// verify presence and identicality of the generated file to the installed file
				var filename = LocalizationManager.GetTmxFileNameForLanguage(AppId, LocalizationManager.kDefaultLang);
				var installedFilePath = Path.Combine(GetInstalledDirectory(folder), filename);
				var generatedFilePath = Path.Combine(GetGeneratedDirectory(folder), filename);
				Assert.That(File.Exists(generatedFilePath), "Generated file {0} should exist", generatedFilePath);
				FileAssert.AreEqual(installedFilePath, generatedFilePath, "Generated file should be copied from and identical to Installed file");
			}
		}

		/// <summary>
		/// On Linux, we crash trying to generate TMX files, leaving an empty file in the Generated folder.
		/// Copy the installed file over the empty generated file in this case.
		/// </summary>
		[Test]
		public void CreateOrUpdateDefaultTmxFileIfNecessary_CopiesOverEmptyGeneratedFile()
		{
			using(var folder = new TempFolder("CreateOrUpdate_CopiesOverEmptyGeneratedFile"))
			{
				var filename = LocalizationManager.GetTmxFileNameForLanguage(AppId, LocalizationManager.kDefaultLang);
				var installedFilePath = Path.Combine(GetInstalledDirectory(folder), filename);
				var generatedFilePath = Path.Combine(GetGeneratedDirectory(folder), filename);

				// generate an empty English TMX file
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
		/// If there is an existing TMX file of the same (or higher) version, it should not be overwritten when initializing the localizer.
		/// </summary>
		[Test]
		public void CreateOrUpdateDefaultTmxFileIfNecessary_DoesNotOverwriteUpToDateGeneratedFile()
		{
			using(var folder = new TempFolder("CreateOrUpdate_DoesNotOverwriteUpToDateGeneratedFile"))
			{
				var filename = LocalizationManager.GetTmxFileNameForLanguage(AppId, LocalizationManager.kDefaultLang);
				var generatedFilePath = Path.Combine(GetGeneratedDirectory(folder), filename);

				// "generate" an English TMX for a higher version
				Directory.CreateDirectory(GetGeneratedDirectory(folder));
				AddEnglishTmx(GetGeneratedDirectory(folder), HigherVersion);
				var generatedTmxContents = File.ReadAllText(generatedFilePath);

				// SUT (buried down in there somewhere)
				SetupManager(folder);

				// verify identicality of the generated file to its previous state
				Assert.AreEqual(generatedTmxContents, File.ReadAllText(generatedFilePath), "Generated file should not have been overwritten");
			}
		}

		/// <summary>
		/// If the GeneratedDefault TMX file is out of date, it should be brought up to the current version
		/// (sorry, this doesn't test the contents, just the version number).
		/// </summary>
		[Test]
		public void CreateOrUpdateDefaultTmxFileIfNecessary_OverwritesOutdatedGeneratedFile()
		{
			using(var folder = new TempFolder("CreateOrUpdate_OverwritesOutdatedGeneratedFile"))
			{
				var filename = LocalizationManager.GetTmxFileNameForLanguage(AppId, LocalizationManager.kDefaultLang);
				var generatedFilePath = Path.Combine(GetGeneratedDirectory(folder), filename);

				// "generate" an English TMX for a lower version
				Directory.CreateDirectory(GetGeneratedDirectory(folder));
				AddEnglishTmx(GetGeneratedDirectory(folder), LowerVersion);

				// SUT (buried down in there somewhere)
				SetupManager(folder);

				// verify that the generated file has been updated to the current version
				var headerElt = XElement.Load(generatedFilePath).Element("header");
				var verElement = headerElt == null ? null
					: headerElt.Elements("prop").FirstOrDefault(e => (string)e.Attribute("type") == LocalizationManager.kAppVersionPropTag);
				var generatedVersion = verElement == null ? null : new Version(verElement.Value);
				Assert.AreEqual(new Version(AppVersion), generatedVersion, "Generated file should have been updated to the current version");
			}
		}

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
				//   Assert.AreEqual("from English TMX", LocalizationManager.GetDynamicString(AppId, "theId", "some default"));
				//However, later I decided, I don't care what is in the English TMX, either. If the c# code just gave a new
				// value for this, what the c# code said should win. Who cares what's in the English TMX. It's only real
				// purpose in life is to provide a list of strings that have been disovered dynamically when the translator
				// needs a list of strings to translate (without it, the translator would have to cause the program to visit
				// each part of the UI so as to trip over all the GetDynamicString() calls.

				//So now, this is correct:

				Assert.AreEqual("from the c# code", LocalizationManager.GetDynamicString(AppId, "theId", "from the c# code"));
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
				Assert.AreEqual("from c# code", LocalizationManager.GetDynamicString(AppId, "theId", "from c# code"));
			}
		}

		[Test]
		public void GetDynamicString_ArabicTMXHasArabicValue_ArabicTMXWins()
		{
			using(var folder = new TempFolder("GetDynamicString_EnglishTMXHasDIfferentStringThanParameter_ParameterWins"))
			{
				SetupManager(folder, "ar");
				Assert.AreEqual("inArabic", LocalizationManager.GetDynamicString(AppId, "theId", "from c# code"));
			}
		}
		[Test]
		public void GetDynamicStringOrEnglish()
		{
			using(var folder = new TempFolder("GetDynamicString_EnglishTMXHasDIfferentStringThanParameter_ParameterWins"))
			{
				SetupManager(folder, "ar");
				Assert.AreEqual("blahInEnglishInCode", GetBlah("en"), "If asked for English, should give whatever is in the code.");
				Assert.AreEqual("blahInFrench", GetBlah("fr"), "We do have french for this, should have found it.");
				Assert.AreEqual("blahInEnglishInCode", GetBlah("ar"), "We don't have french so should get the English from code.");
			}
		}

		private string GetBlah(string langId)
		{
			return LocalizationManager.GetDynamicStringOrEnglish(AppId, "blahId", "blahInEnglishInCode", "comment", langId);
		}

		[Test]
		public void GetDynamicStringInEnglish_NoDefault_FindsEnglish()
		{
			using (var folder = new TempFolder("GetDynamicStringInEnglish_NoDefault_FindsEnglish"))
			{
				SetupManager(folder, "en");
				Assert.That(LocalizationManager.GetDynamicString(AppId, "blahId", null), Is.EqualTo("blah"), "With no default supplied, should find saved English");
			}
		}

		[Test]
		public void GetUiLanguages_EnglishIsThere()
		{
			var cultures = LocalizationManager.GetUILanguages(false);
			Assert.AreEqual("English", cultures.Where(c => c.Name == "en").Select(c => c.NativeName).FirstOrDefault());
		}

		[Test]
		public void GetUiLanguages_AzeriHasHackedNativeName()
		{
			var cultures = LocalizationManager.GetUILanguages(false);
			Assert.AreEqual("Azərbaycan dili", cultures.Where(c => c.Name == "az").Select(c => c.NativeName).FirstOrDefault());
		}

		[Test]
		public void LocalizedStringCache_LoadGroupNodes_DoesntLoadNoLongerUsedUnits()
		{
			using (var folder = new TempFolder("LoadGroupNodes_EnglishTMXHasNoLongerUsedProperty_ArabicDoesnt_NoLongerUsedWins_ArabicUI"))
			{
				// Add a "generated" TMX for a lower version to force regeneration
				Directory.CreateDirectory(GetGeneratedDirectory(folder));
				AddEnglishTmx(GetGeneratedDirectory(folder), LowerVersion);

				SetupManager(folder, "ar");
				var mgr = LocalizationManager.LoadedManagers[AppId];
				var treeView = new TreeView();
				var node = new LocTreeNode(mgr, mgr.Name, null, mgr.Name);
				treeView.Nodes.Add(node);
				var treeNodes = node.Nodes;
				mgr.StringCache.LoadGroupNodes(treeNodes);
				Assert.IsFalse(treeNodes.ContainsKey("notUsedId"));
				Assert.IsTrue(treeNodes.ContainsKey("theId"));
			}
		}

		[Test]
		public void LocalizedStringCache_LoadGroupNodes_DoesntLoadNoLongerUsedUnitsUIEnglish()
		{
			using (var folder = new TempFolder("LoadGroupNodes_EnglishTMXHasNoLongerUsedProperty_ArabicDoesnt_NoLongerUsedWins_EnglishUI"))
			{
				// Add a "generated" TMX for a lower version to force regeneration
				Directory.CreateDirectory(GetGeneratedDirectory(folder));
				AddEnglishTmx(GetGeneratedDirectory(folder), LowerVersion);

				SetupManager(folder, "en");
				var mgr = LocalizationManager.LoadedManagers[AppId];
				var treeView = new TreeView();
				var node = new LocTreeNode(mgr, mgr.Name, null, mgr.Name);
				treeView.Nodes.Add(node);
				var treeNodes = node.Nodes;
				mgr.StringCache.LoadGroupNodes(treeNodes);
				Assert.IsFalse(treeNodes.ContainsKey("notUsedId"));
				Assert.IsTrue(treeNodes.ContainsKey("theId"));
			}
		}

		/// <summary>
		/// - "Installs" English, Arabic, and French
		/// - Sets the UI language
		/// - Constructs a LocalizationManager and adds it to LocalizationManager.LoadedManagers[AppId]
		/// </summary>
		private static void SetupManager(TempFolder folder, string uiLanguageId = null)
		{
			AddEnglishTmx(GetInstalledDirectory(folder), AppVersion);
			AddArabicTmx(GetInstalledDirectory(folder));
			AddFrenchTmx(GetInstalledDirectory(folder));

			LocalizationManager.SetUILanguage(uiLanguageId, true);
			var manager = new LocalizationManager(AppId, AppName, AppVersion,
				GetInstalledDirectory(folder), GetGeneratedDirectory(folder), GetUserModifiedDirectory(folder));

			// REVIEW (Hasso) 2015.01: Since AppId is static, I wonder what conflicts this may cause (even though each test has its own TempFolder)
			LocalizationManager.LoadedManagers[AppId] = manager;
		}

		private static string GetInstalledDirectory(TempFolder parentDir)
		{
			return parentDir.Path; // no reason we can't use the root temp dir for the "installed" Tmx's
		}

		private static string GetGeneratedDirectory(TempFolder parentDir)
		{
			return parentDir.Combine("generated");
		}

		private static string GetUserModifiedDirectory(TempFolder parentDir)
		{
			return parentDir.Combine("userModified");
		}

		private static void AddEnglishTmx(string folderPath, string appVersion)
		{
			var englishDoc = new TMXDocument {Header = {SourceLang = "en"}};
			if (!String.IsNullOrEmpty(appVersion))
				englishDoc.Header.SetPropValue(LocalizationManager.kAppVersionPropTag, appVersion);
			// first unit
			var variants = new List<TransUnitVariant> {new TransUnitVariant{Lang = "en", Value = "from English TMX"}};
			var tu = new TransUnit
			{
				Id = "theId",
				Variants = variants
			};
			tu.AddProp("en", LocalizedStringCache.kDiscoveredDyanmically, "true");
			englishDoc.AddTransUnit(tu);
			// second unit
			var variants2 = new List<TransUnitVariant> {new TransUnitVariant {Lang = "en", Value = "no longer used English text"}};
			var tu2 = new TransUnit
			{
				Id = "notUsedId",
				Variants = variants2
			};
			englishDoc.AddTransUnit(tu2);
			// third unit
			var variants3 = new List<TransUnitVariant> {new TransUnitVariant {Lang = "en", Value = "blah"}};
			var tu3 = new TransUnit
			{
				Id = "blahId",
				Variants = variants3
			};
			englishDoc.AddTransUnit(tu3);
			englishDoc.Save(Path.Combine(folderPath, LocalizationManager.GetTmxFileNameForLanguage(AppId, "en")));
		}

		private static void AddArabicTmx(string folderPath)
		{
			var arabicDoc = new TMXDocument {Header = {SourceLang = "ar"}};

			// first unit
			var variants = new List<TransUnitVariant>
			{
				new TransUnitVariant {Lang = "en", Value = "wrong"},
				new TransUnitVariant {Lang = "ar", Value = "inArabic"}
			};
			var tu = new TransUnit
			{
				Id = "theId",
				Variants = variants
			};
			tu.AddProp("ar", LocalizedStringCache.kDiscoveredDyanmically, "true");
			tu.AddProp("en", LocalizedStringCache.kDiscoveredDyanmically, "true");
			arabicDoc.AddTransUnit(tu);
			// second unit
			var variants2 = new List<TransUnitVariant>
			{
				new TransUnitVariant {Lang = "en", Value = "inEnglishpartofArabicTMX"},
				new TransUnitVariant {Lang = "ar", Value = "inArabic"}
			};
			var tu2 = new TransUnit
			{
				Id = "notUsedId",
				Variants = variants2
			};
			tu2.AddProp("ar", LocalizedStringCache.kDiscoveredDyanmically, "true");
			tu2.AddProp("en", LocalizedStringCache.kDiscoveredDyanmically, "true");
			// Note: we are NOT adding a NoLongerUsed property to the Arabic TMX
			arabicDoc.AddTransUnit(tu2);
			arabicDoc.Save(Path.Combine(folderPath, LocalizationManager.GetTmxFileNameForLanguage(AppId, "ar")));
		}

		private static void AddFrenchTmx(string folderPath)
		{
			var doc = new TMXDocument {Header = {SourceLang = "fr"}};

			// first unit
			var variants = new List<TransUnitVariant>
			{
				new TransUnitVariant {Lang = "en", Value = "blah"},
				new TransUnitVariant {Lang = "fr", Value = "blahInFrench"}
			};
			var tu = new TransUnit
			{
				Id = "blahId",
				Variants = variants
			};
			tu.AddProp("ar", LocalizedStringCache.kDiscoveredDyanmically, "true");
			tu.AddProp("en", LocalizedStringCache.kDiscoveredDyanmically, "true");
			doc.AddTransUnit(tu);
			doc.Save(Path.Combine(folderPath, LocalizationManager.GetTmxFileNameForLanguage(AppId, "fr")));
		}
	}
}