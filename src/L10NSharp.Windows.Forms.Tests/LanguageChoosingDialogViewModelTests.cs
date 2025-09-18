using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using L10NSharp;
using L10NSharp.Translators;
using L10NSharp.Windows.Forms.UIComponents;
using NUnit.Framework;

namespace L10NSharp.Windows.Forms.Tests
{
	[TestFixture]
	class LanguageChoosingDialogViewModelTests
	{
		[Test]
		public void Constructor_RequestedCultureEnglish_MessageOnlyHasEnglishOnceAndNonEnglishActionIsNotRun()
		{
			var model = new LanguageChoosingDialogViewModel("Blah {0} ({1})", "OK", "Choose a Language",
				L10NCultureInfo.GetCultures(CultureTypes.NeutralCultures).First(c => c.TwoLetterISOLanguageName == "en"),
				() => throw new Exception("Non-english UI Action should not have been called"));
			Assert.AreEqual("Blah English", model.Message);
			// We'll go ahead and confirm that the other properties come through as expected also.
			Assert.AreEqual("en", model.RequestedCultureTwoLetterISOLanguageName);
			Assert.AreEqual("OK", model.AcceptButtonText);
			Assert.AreEqual("Choose a Language", model.WindowTitle);
		}

		[Test]
		public void Constructor_RequestedCultureTypicalNonEnglish_MessageHasNativeNameAndEnglishNameAndNonEnglishActionIsRun()
		{
			var nonEnglishActionGotCalled = false;
			var model = new LanguageChoosingDialogViewModel("Blah {0} ({1})", "OK", "Choose a Language",
				L10NCultureInfo.GetCultures(CultureTypes.NeutralCultures).First(c => c.TwoLetterISOLanguageName == "de"),
				() => nonEnglishActionGotCalled = true);
			Assert.AreEqual("Blah German (Deutsch)", model.Message);
			Assert.IsTrue(nonEnglishActionGotCalled);
			// We'll go ahead and confirm that the other properties come through as expected also.
			Assert.AreEqual("de", model.RequestedCultureTwoLetterISOLanguageName);
			Assert.AreEqual("OK", model.AcceptButtonText);
			Assert.AreEqual("Choose a Language", model.WindowTitle);
		}

		[Test]
		public void Constructor_RequestedCultureWithNativeNameEqualToEnglishName_MessageOnlyLanguageNameOnceAndNonEnglishActionIsRun()
		{
			var nonEnglishActionGotCalled = false;
			var culture = L10NCultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(c => c.EnglishName == c.NativeName && c.TwoLetterISOLanguageName != "en");
			Assume.That(culture, Is.Not.Null, "Didn't find culture where native name is equal to English name");

			var model = new LanguageChoosingDialogViewModel("Blah {0} ({1})", "OK", "Choose a Language",
				culture,
				() => nonEnglishActionGotCalled = true);

			Assert.That(model.Message, Is.EqualTo($"Blah {culture.EnglishName}"));
			Assert.That(nonEnglishActionGotCalled, Is.True, "Action didn't get called");
			// We'll go ahead and confirm that the other properties come through as expected also.
			Assert.That(model.RequestedCultureTwoLetterISOLanguageName, Is.EqualTo(culture.TwoLetterISOLanguageName));
			Assert.That(model.AcceptButtonText, Is.EqualTo("OK"));
			Assert.That(model.WindowTitle, Is.EqualTo("Choose a Language"));
		}

		[Test]
		public void Constructor_RequestedCultureEnglishFormatStringHasNoParam1_MessageHasEnglishSubstitutedAsAparam0()
		{
			var model = new LanguageChoosingDialogViewModel("Blah {0} yup", "OK", "Choose a Language",
				L10NCultureInfo.GetCultures(CultureTypes.NeutralCultures).First(c => c.TwoLetterISOLanguageName == "en"),
				null);
			Assert.AreEqual("Blah English yup", model.Message);
		}

		[TestCase("en")]
		[TestCase("fr")]
		public void Constructor_FormatStringHasNoParams_MessageEqualsFormatString(string languageCode)
		{
			var model = new LanguageChoosingDialogViewModel("No format parameters", "OK", "Choose a Language",
				L10NCultureInfo.GetCultures(CultureTypes.NeutralCultures).First(c => c.TwoLetterISOLanguageName == languageCode),
				null);
			Assert.AreEqual("No format parameters", model.Message);
		}

		[TestCase("Blah {2}")]
		[TestCase("Blah {-1}")]
		[TestCase("Blah {a}")]
		public void Constructor_BogusFormatString_Throws(string format)
		{
			Assert.Throws<FormatException>(() => new LanguageChoosingDialogViewModel(format, "OK", "Choose a Language",
				L10NCultureInfo.CurrentCulture, null));
		}

		[Test]
		public void TranslateStrings_RequestedCultureEnglish_TranslationAppliedOnceToEachString()
		{
			var model = new LanguageChoosingDialogViewModel("Blah {0} ({1}) yup!", "OK", "Choose a Language",
				L10NCultureInfo.GetCultures(CultureTypes.NeutralCultures).First(c => c.TwoLetterISOLanguageName == "en"),
				null);
			Assert.AreEqual("Blah English yup!", model.Message);
			var translator = new TestTranslatorBumpyFrog();
			model.TranslateStrings(translator);
			Assert.AreEqual("Bumpy frog Blah English yup!", model.Message);
			Assert.AreEqual("Bumpy frog OK", model.AcceptButtonText);
			Assert.AreEqual("Bumpy frog Choose a Language", model.WindowTitle);
			Assert.IsTrue(translator.SourceStrings.SequenceEqual(new[] { "Blah English yup!", "OK", "Choose a Language" }));
		}

		[Test]
		public void TranslateStrings_RequestedCultureGermanNormalTranslation_TranslationAppliedOnceToEachString()
		{
			var model = new LanguageChoosingDialogViewModel("No localization for {0} ({1})", "OK", "Choose a Language",
				L10NCultureInfo.GetCultures(CultureTypes.NeutralCultures).First(c => c.TwoLetterISOLanguageName == "de"),
				null);
			Assert.AreEqual("No localization for German (Deutsch)", model.Message);
			var translator = new TestTranslatorGerman();
			model.TranslateStrings(translator);
			Assert.AreEqual("Ich spreche No localization for Deutsch (German)", model.Message);
			Assert.AreEqual("Ich spreche OK", model.AcceptButtonText);
			Assert.AreEqual("Ich spreche Choose a Language", model.WindowTitle);
			Assert.IsTrue(translator.SourceStrings.SequenceEqual(new[] { "No localization for German ({0})", "OK", "Choose a Language" }));
		}

		[Test]
		public void TranslateStrings_ChangeTranslator_TranslationAppliedToOriginalString()
		{
			var model = new LanguageChoosingDialogViewModel("No localization for {0} ({1})", "OK", "Choose a Language",
				L10NCultureInfo.GetCultures(CultureTypes.NeutralCultures).First(c => c.TwoLetterISOLanguageName == "de"),
				null);
			Assert.AreEqual("No localization for German (Deutsch)", model.Message);
			model.TranslateStrings(new TestTranslatorBumpyFrog());
			model.TranslateStrings(new TestTranslatorGerman());
			Assert.AreEqual("Ich spreche No localization for Deutsch (German)", model.Message);
			Assert.AreEqual("Ich spreche OK", model.AcceptButtonText);
			Assert.AreEqual("Ich spreche Choose a Language", model.WindowTitle);
		}

		[Test]
		public void TranslateStrings_RequestedCultureSpanishChokesOnFormatParam_TranslationReappliedToStringWithoutParam()
		{
			var model = new LanguageChoosingDialogViewModel("No localization for {0} ({1})", "OK", "Choose a Language",
				L10NCultureInfo.GetCultures(CultureTypes.NeutralCultures).First(c => c.TwoLetterISOLanguageName == "es"),
				null);
			Assert.AreEqual("No localization for Spanish (español)", model.Message);
			var translator = new TestTranslatorSpanishChokesOnFormatParam();
			model.TranslateStrings(translator);
			// Note: the test translator mimics Bing's behavior of replacing the English name of the requested language with the word "English" in the translation.
			Assert.AreEqual("No choke No localization for English (español)", model.Message);
			Assert.AreEqual("No choke OK", model.AcceptButtonText);
			Assert.AreEqual("No choke Choose a Language", model.WindowTitle);
			Assert.IsTrue(translator.SourceStrings.SequenceEqual(new[] { "No localization for Spanish ({0})", "No localization for Spanish (español)", "OK", "Choose a Language" }));
		}

		[Test]
		public void TranslateStrings_TranslatorThrowsException_ExceptionSwallowedAndNoAttemptToTranslateOtherStrings()
		{
			var model = new LanguageChoosingDialogViewModel("No localization for {0} ({1})", "Okey-dokey", "Select a Language",
				L10NCultureInfo.GetCultures(CultureTypes.NeutralCultures).First(c => c.TwoLetterISOLanguageName == "es"),
				null);
			var translator = new TestTranslatorThrowsException();
			model.TranslateStrings(translator);
			Assert.AreEqual("No localization for Spanish (español)", model.Message);
			Assert.AreEqual("Okey-dokey", model.AcceptButtonText);
			Assert.AreEqual("Select a Language", model.WindowTitle);
			Assert.IsTrue(translator.SourceStrings.SequenceEqual(new[] { "No localization for Spanish ({0})" }));
		}

		[Test]
		public void TranslateStrings_TranslatorReturnsSourceString_NoAttemptToSubstituteEnglishNameOrTranslateOtherStrings()
		{
			var model = new LanguageChoosingDialogViewModel("No localization for {0} ({1})", "Okey-dokey", "Select a Language",
				L10NCultureInfo.GetCultures(CultureTypes.NeutralCultures).First(c => c.TwoLetterISOLanguageName == "es"),
				null);
			var translator = new TestTranslatorBase(); // This translator just returns unmodified source string
			model.TranslateStrings(translator);
			Assert.AreEqual("No localization for Spanish (español)", model.Message);
			Assert.AreEqual("Okey-dokey", model.AcceptButtonText);
			Assert.AreEqual("Select a Language", model.WindowTitle);
			Assert.IsTrue(translator.SourceStrings.SequenceEqual(new[] { "No localization for Spanish ({0})" }));
		}

		private class TestTranslatorBase : TranslatorBase
		{
			public List<string> SourceStrings { get; }

			public TestTranslatorBase()
			{
				SourceStrings = new List<string>();
			}

			protected override string InternalTranslate(string srcText)
			{
				SourceStrings.Add(srcText);
				return srcText;
			}
		}

		private class TestTranslatorBumpyFrog : TestTranslatorBase
		{
			protected override string InternalTranslate(string srcText)
			{
				return "Bumpy frog " + base.InternalTranslate(srcText);
			}
		}

		private class TestTranslatorGerman : TestTranslatorBase
		{
			protected override string InternalTranslate(string srcText)
			{
				return "Ich spreche " + base.InternalTranslate(srcText).Replace("German", "Deutsch");
			}
		}

		private class TestTranslatorSpanishChokesOnFormatParam : TestTranslatorBase
		{
			protected override string InternalTranslate(string srcText)
			{
				var s = base.InternalTranslate(srcText);
				if (s.Contains("{0}"))
					return "";
				return "No choke " + s.Replace("Spanish", "English"); // Looks weird, but Bing actually does this!
			}
		}

		private class TestTranslatorThrowsException : TestTranslatorBase
		{
			protected override string InternalTranslate(string srcText)
			{
				base.InternalTranslate(srcText);
				throw new Exception("This should get swallowed");
			}
		}
	}
}
