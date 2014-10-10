using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using L10NSharp.UI;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace L10NSharp.Tests
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// These tests need to create a "real" Localization Manager, but with the capability of
	/// removing all trace of it after the tests.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	class ILocalizableComponentTests
	{
		private LocalizationManager m_manager;
		private L10NSharpExtender m_extender;
		private string m_tmxPath;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Setup for each test.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void TestSetup()
		{
			var installedTmxDir = "../../src/L10NSharpTests/TestTmx";
			m_manager = LocalizationManager.Create("en", "Test", "Test", "1.0", installedTmxDir, "", null, "");
			m_tmxPath = m_manager.GetTmxPathForLanguage("en", true);
			m_extender = new L10NSharpExtender();
			m_extender.LocalizationManagerId = "Test";
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Teardown for each test.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[TearDown]
		public void TestTeardown()
		{
			m_extender = null;
			m_manager = null;
			var localAppDataDir = Directory.GetParent(Path.GetDirectoryName(m_tmxPath));
			Directory.Delete(localAppDataDir.FullName, true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests that we can localize an ILocalizableComponent object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestLocalizingALocalizableComponent()
		{
			// Setup test
			m_extender.BeginInit(); // Doesn't currently do anything, but for completeness...
			var myMsc = new MockLocalizableComponent(m_extender);

			// SUT
			// this line calls ILocalizableComponent.GetAllLocalizingInfoObjects()
			m_extender.AddMultipleStrings(myMsc);
			// this line calls ILocalizableComponent.ApplyLocalizationToString(obj, id, localization)
			m_extender.EndInit();

			// Verify English
			Assert.AreEqual("It's a crow", myMsc.GetLocalizedStringFromMock(myMsc.BirdButton, "TestItem.Bird.Crow"));
			Assert.AreEqual("It's not a crow", myMsc.GetLocalizedStringFromMock(myMsc.BirdButton, "TestItem.Bird.Raven"));
			Assert.AreEqual("It's a chicken", myMsc.GetLocalizedStringFromMock(myMsc.ChickenButton, "TestItem.Chicken.Rooster"));
			Assert.AreEqual("Fish-eating bird", myMsc.GetLocalizedStringFromMock(myMsc.BirdButton, "TestItem.Bird.Eagle"));

			// SUT2
			LocalizationManager.SetUILanguage("fr", true);

			// Verify French
			Assert.AreEqual("C'est un corbeau", myMsc.GetLocalizedStringFromMock(myMsc.BirdButton, "TestItem.Bird.Crow"));
			Assert.AreEqual("Ce n'est pas un corbeau", myMsc.GetLocalizedStringFromMock(myMsc.BirdButton, "TestItem.Bird.Raven"));
			Assert.AreEqual("C'est un poulet", myMsc.GetLocalizedStringFromMock(myMsc.ChickenButton, "TestItem.Chicken.Rooster"));
			Assert.AreEqual("Un oiseau qui mange des poissons", myMsc.GetLocalizedStringFromMock(myMsc.BirdButton, "TestItem.Bird.Eagle"));
		}
	}
}
