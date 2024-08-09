// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using NUnit.Framework;
using System;
using System.Threading;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class XLiffLocalizationManagerTests_NoManagersLoaded
	{
		[OneTimeSetUp]
		public void Setup()
		{
			LocalizationManager.TranslationMemoryKind = TranslationMemory.XLiff;
			LocalizationManager.ClearLoadedManagers();
		}

		/// <summary>
		/// According to a comment in GetDynamicStringOrEnglish, in unit test environments,
		/// it's possible for no LM to be loaded, but according to the description of this
		/// method, there is a "Special case" for English whereby the English string should
		/// ALWAYS be returned. This test covers the intersection of those two special cases.
		/// </summary>
		[Test]
		public void GetDynamicString_NoManagerLoaded_EnglishNotNull_ReturnsEnglishString()
		{
			try
			{
				LocalizationManager.UILanguageId = "en";
				Assert.That(
					LocalizationManager.GetDynamicString("Glom", "prefix.data", "data"),
					Is.EqualTo("data"));
			}
			finally
			{
				// reset
				LocalizationManager.UILanguageId = null;
			}
		}

		[TestCase(null)]
		[TestCase("en")]
		[TestCase("es")]
		public void GetDynamicString_NoManagerLoaded_EnglishNull_ReturnsId(string uiLanguageId)
		{
			try
			{
				LocalizationManager.UILanguageId = uiLanguageId;
				Assert.That(
					LocalizationManager.GetDynamicString("Glom", "prefix.data", null),
					Is.EqualTo("prefix.data"));
			}
			finally
			{
				// reset
				LocalizationManager.UILanguageId = null;
			}
		}

		[TestCase(null)]
		[TestCase("en")]
		[TestCase("es")]
		public void GetDynamicStringOrEnglish_NoManagerLoaded_NonEnglish_ReturnsId(string uiLanguageId)
		{
			try
			{
				LocalizationManager.UILanguageId = uiLanguageId;
				Assert.That(
					LocalizationManager.GetDynamicStringOrEnglish("Glom", "prefix.data", "data", "no comment", "es"),
					Is.EqualTo("prefix.data"));
			}
			finally
			{
				// reset
				LocalizationManager.UILanguageId = null;
			}
		}

		[TestCase("en")]
		[TestCase("en-US")]
		[TestCase("es")]
		[TestCase("es-ES")]
		[TestCase("es-MX")]
		public void GetString_NoManagerLoaded_StrictInitializationModeTrue_Throws(string cultureName)
		{
			System.Globalization.CultureInfo previousCurrentCulture = null;
			try
			{
				previousCurrentCulture = Thread.CurrentThread.CurrentUICulture;

				Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(cultureName);

				// default is true
				Assert.Throws<InvalidOperationException>(() => LocalizationManager.GetString("prefix.id", "data"));

			}
			finally
			{
				Thread.CurrentThread.CurrentUICulture = previousCurrentCulture;
				LocalizationManager.UILanguageId = null;
			}
		}

		[TestCase("en")]
		[TestCase("en-US")]
		[TestCase("es")]
		[TestCase("es-ES")]
		[TestCase("es-MX")]
		public void GetString_NoManagerLoaded_StrictInitializationModeFalse_DoesNotThrow(string cultureName)
		{
			System.Globalization.CultureInfo previousCurrentCulture = null;
			try
			{
				previousCurrentCulture = Thread.CurrentThread.CurrentUICulture;

				LocalizationManager.StrictInitializationMode = false;
				Assert.DoesNotThrow(() => LocalizationManager.GetString("prefix.id", "data"));
			}
			finally
			{
				Thread.CurrentThread.CurrentUICulture = previousCurrentCulture;
				LocalizationManager.UILanguageId = null;

				LocalizationManager.StrictInitializationMode = true;
			}
		}
	}
}
