// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using L10NSharp.XLiffUtils;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class XLiffLocalizationManagerTests_NoManagersLoaded : LocalizationManagerTests_NoManagersLoaded
	{
		[TestFixtureSetUp]
		public override void Setup()
		{
			LocalizationManager.TranslationMemoryKind = TranslationMemory.XLiff;
			base.Setup();
		}
	}

	public abstract class LocalizationManagerTests_NoManagersLoaded
	{
		public virtual void Setup()
		{
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
			Assert.That(
				LocalizationManager.GetDynamicString("Glom", "prefix.data", "data"),
				Is.EqualTo("data"));
		}

		[Test]
		public void GetDynamicString_NoManagerLoaded_EnglishNull_ReturnsId()
		{
			Assert.That(
				LocalizationManager.GetDynamicString("Glom", "prefix.data", null),
				Is.EqualTo("prefix.data"));
		}

		[Test]
		public void GetDynamicStringOrEnglish_NoManagerLoaded_NonEnglish_ReturnsId()
		{
			Assert.That(
				LocalizationManager.GetDynamicStringOrEnglish("Glom", "prefix.data", "data", "no comment", "es"),
				Is.EqualTo("prefix.data"));
		}
	}}
