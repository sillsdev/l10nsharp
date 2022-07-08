using System;
using System.IO;
using System.Reflection;
using L10NSharp.TMXUtils;
using L10NSharp.UI;
using L10NSharp.XLiffUtils;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class ILocalizableComponentXLiffTests : ILocalizableComponentTests<XLiffDocument>
	{
		[SetUp]
		public void TestSetup()
		{
			TestSetup(TranslationMemory.XLiff, "../../../src/L10NSharpTests/TestXliff");
		}

	}

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// These tests need to create a "real" Localization Manager, but with the capability of
	/// removing all trace of it after the tests.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public abstract class ILocalizableComponentTests<T>
	{
		private ILocalizationManagerInternal<T> m_manager;
		private L10NSharpExtender m_extender;
		private string m_translationPath;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Setup for each test.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected void TestSetup(TranslationMemory kind, string installedTranslationDir)
		{
			var dir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			m_manager = LocalizationManager.Create(kind, "en", "Test", "Test", "1.0",
					Path.Combine(dir, installedTranslationDir),
					"", null, "")
				as ILocalizationManagerInternal<T>;
			m_translationPath = m_manager.GetPathForLanguage("en", true);
			m_extender = new L10NSharpExtender { LocalizationManagerId = "Test" };
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
			var localAppDataDir = Directory.GetParent(Path.GetDirectoryName(m_translationPath));
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
			var locComponent = new MockLocalizableComponent();
			m_extender.SetLocalizingId(locComponent, "TestLocalizableComponent");

			// SUT
			m_extender.EndInit();

			// Verify English
			Assert.AreEqual("It's a crow", locComponent.GetLocalizedStringFromMock(locComponent.BirdButton, "TestItem.Bird.Crow"));
			Assert.AreEqual("It's not a crow", locComponent.GetLocalizedStringFromMock(locComponent.BirdButton, "TestItem.Bird.Raven"));
			Assert.AreEqual("It's a chicken", locComponent.GetLocalizedStringFromMock(locComponent.ChickenButton, "TestItem.Chicken.Rooster"));
			Assert.AreEqual("Fish-eating bird", locComponent.GetLocalizedStringFromMock(locComponent.BirdButton, "TestItem.Bird.Eagle"));

			// SUT2
			LocalizationManager.SetUILanguage("fr", true);

			// Verify French
			Assert.AreEqual("C'est un corbeau", locComponent.GetLocalizedStringFromMock(locComponent.BirdButton, "TestItem.Bird.Crow"));
			Assert.AreEqual("Ce n'est pas un corbeau", locComponent.GetLocalizedStringFromMock(locComponent.BirdButton, "TestItem.Bird.Raven"));
			Assert.AreEqual("C'est un poulet", locComponent.GetLocalizedStringFromMock(locComponent.ChickenButton, "TestItem.Chicken.Rooster"));
			Assert.AreEqual("Un oiseau qui mange des poissons", locComponent.GetLocalizedStringFromMock(locComponent.BirdButton, "TestItem.Bird.Eagle"));

			// SUT3 (I don't like doing multiple tests in one test method, but when I tried to make a different test
			//       I got some test interaction because of setup/teardown. That's easily avoidable by putting the new test here.)
			var result = m_extender.CanExtend(locComponent);

			// Verify
			Assert.IsTrue(result, "an ILocalizableComponent ought to be extendable by the L10NSharpExtender");
		}
	}
}
