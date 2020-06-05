using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class LocalizationManagerXliffTests
	{
		[Test]
		[Timeout(3000)] // This ensures that if the UI pops up, a developer will not have time to respond and the test won't hang.
		public void Create_PreferredUiLanguageIsGenericVariant_CreatesLocalizationManagerForSpecificVariant()
		{
			var dir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			var lm = LocalizationManager.Create(TranslationMemory.XLiff, "es", "Test", "Test", "1.0",
				Path.Combine(dir, "../../../src/L10NSharpTests/TestXliff"), "", null, "");
			Assert.IsFalse(lm.GetAvailableUILanguageTags().Contains("es"));
			Assert.IsTrue(lm.GetAvailableUILanguageTags().Contains("es-ES"));
		}
	}
}
