using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	/// <summary>
	/// Tests that only apply to Xliff, not TMX.
	/// </summary>
	[TestFixture]
	public class LocalizationManagerXliffTests
	{
		/// <summary>
		/// Indirectly, this is a test of LocalizationManagerInternal&lt;XLiffDocument&gt;.IsDesiredUiCultureAvailable,
		/// which is called by its Create method. This test was written to ensure that a dialog is not displayed
		/// incorrectly telling the user that the requested UI language is unavailable and asking them which UI
		/// Language to use instead. If the implementation is incorrect, we don't want the test to hang because
		/// of the dialog box but rather to fail. And of course, if the test is being run in an interactive
		/// environment, we also don't want the developer to interact with the dialog, which could give a false
		/// success. So we set the timeout to 3 seconds, which seems to be long enough to let the test run
		/// successfully but short enough to prevent a developer from interacting with it.
		/// </summary>
		/// <remarks>If you're wondering why this is XLIFF-only, it probably wouldn't have to be. The SUT runs
		/// for both varieties of LM. However, in practice, there will probably never be a case where this
		/// behavior is needed for TMX. It was written to make it so that apps that used to use TMX, which
		/// typically used the "generic" two-letter language ID will work seamlessly when the app starts
		/// using XLIFF, because crowdin.com seems to insist on using country-specific variants for some
		/// languages. (For some reason, it does allow French to just be "fr".)  </remarks>
		[TestCase("es", "es-ES")]
		[TestCase("pt", "pt-PT")]
		[Timeout(300000)]
		public void Create_PreferredUiLanguageIsGenericVariant_CreatesLocalizationManagerForSpecificVariant(
			string genericLocaleId, string countrySpecificLocalId)
		{
			LocalizationManager.ClearLoadedManagers();
			var dir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			var lm = LocalizationManager.Create(TranslationMemory.XLiff, genericLocaleId, "Test", "Test", "1.0",
				Path.Combine(dir, "../../../src/L10NSharpTests/TestXliff2"), "", null, "");
			Assert.AreEqual($"Protección de configuraciones ({genericLocaleId})...",
				lm.GetLocalizedString("SettingsProtection.LauncherButtonLabel", "don't use this"));
			// The next two lines prove that the test data was not changed in a way that nullifies the expected pre-conditions
			Assert.IsFalse(lm.GetAvailableUILanguageTags().Contains(genericLocaleId));
			Assert.IsTrue(lm.GetAvailableUILanguageTags().Contains(countrySpecificLocalId));
		}
	}
}
