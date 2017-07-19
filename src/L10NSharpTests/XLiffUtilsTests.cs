using System;
using NUnit.Framework;
using L10NSharp.XLiffUtils;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class XLiffUtilsTests
	{
		string _testFolder;

		public XLiffUtilsTests()
		{
			var asmFile = Assembly.GetExecutingAssembly().CodeBase.Replace("file://", String.Empty);
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				asmFile = asmFile.TrimStart('/');
			var folder = Path.GetDirectoryName(asmFile);	// will be something like <repodir>/output/Debug
			folder = Path.GetDirectoryName(folder);
			folder = Path.GetDirectoryName(folder);
			_testFolder = Path.Combine(folder, "src", "L10NSharpTests", "TestXliff");
		}

		[Test]
		public void TestHeaderNotes()
		{
			var enfile = Path.Combine(_testFolder, "Test.en.xlf");
			var doc = XLiffDocument.Read(enfile);

			Assert.AreEqual(2, doc.File.Header.Notes.Count);
			Assert.AreEqual("This is a test.  This is only a test.", doc.File.Header.Notes[0].Text);
			Assert.AreEqual("I'm not sure I agree with the previous note.", doc.File.Header.Notes[1].Text);

			// Test that what once was passed in a note is still being read correctly.
			Assert.AreEqual("\\n", doc.File.HardLineBreakReplacement);
		}
	}
}

