using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using L10NSharp.CodeReader;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	class CodeReaderTests
	{
		[Test]
		public void MethodNeedsLocalization_Uses_NoLocalizableStringsPresentAttribute_Test()
		{
			var testPlatformType = typeof(TestPlatformSkipping);
			var skipOnWindows = testPlatformType.GetMethod("SkipOnWindows");
			var skipOnWindowsAndLinux = testPlatformType.GetMethod("SkipOnWindowsAndLinux");
			var skipOnLinux = testPlatformType.GetMethod("SkipOnLinux");
			var skipOnAll = testPlatformType.GetMethod("SkipOnAll");
			var skipOnNone = testPlatformType.GetMethod("SkipOnNone");
			// Test that the attribute behaves properly on all currently tested platforms
			Assert.That(StringExtractor.MethodNeedsLocalization(skipOnAll), Is.False, "NoLocalizableStrings without argument is not working");
			Assert.That(StringExtractor.MethodNeedsLocalization(skipOnNone), Is.True, "A method without NoLocalizableStrings should be localized");

			if(Environment.OSVersion.Platform == PlatformID.Unix)
			{
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnWindows), Is.True, "NoLocalizableStrings for Windows should localize on linux");
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnWindowsAndLinux), Is.False, "Should not be localized on linux");
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnLinux), Is.False, "Should not be localized on linux");
			}
			else if(Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnWindows), Is.False, "Should not be localized on Windows");
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnWindowsAndLinux), Is.False, "Should not be localized on Windows");
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnLinux), Is.True, "NoLocalizableStrings for Linux should localize on Windows");
			}
		}

		class TestPlatformSkipping
		{
			[NoLocalizableStringsPresent(NoLocalizableStringsPresent.OS.Windows)]
			public void SkipOnWindows()
			{
				Console.WriteLine(@"skip");
			}

			[NoLocalizableStringsPresent(NoLocalizableStringsPresent.OS.Windows | NoLocalizableStringsPresent.OS.Linux)]
			public void SkipOnWindowsAndLinux()
			{
				Console.WriteLine(@"skip");
			}

			[NoLocalizableStringsPresent(NoLocalizableStringsPresent.OS.Linux)]
			public void SkipOnLinux()
			{
				Console.WriteLine(@"skip");
			}

			[NoLocalizableStringsPresent]
			public void SkipOnAll()
			{
				Console.WriteLine(@"to my Lou");
			}

			public void SkipOnNone()
			{
				Console.WriteLine(@"my darling");
			}
		}
	}
}
