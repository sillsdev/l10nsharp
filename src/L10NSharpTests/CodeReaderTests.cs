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
			// Test that the attribute behaves properly on all currently tested platforms
			Assert.That(StringExtractor.MethodNeedsLocalization(skipOnAll), Is.False);
			if(Environment.OSVersion.Platform == PlatformID.Unix)
			{
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnWindows), Is.False);
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnWindowsAndLinux), Is.True);
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnLinux), Is.True);
			}
			else if(Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnWindows), Is.True);
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnWindowsAndLinux), Is.True);
				Assert.That(StringExtractor.MethodNeedsLocalization(skipOnLinux), Is.False);
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
		}
	}
}
