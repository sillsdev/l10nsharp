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

		[Test]
		public void TestCounts()
		{
			var enfile = Path.Combine(_testFolder, "Test.en.xlf");
			var endoc = XLiffDocument.Read(enfile);
			// These numbers may be counter-intuitive, but then the English isn't translated, is it?
			// Code at the LocalizationManager level will make English look okay for display by
			// faking the approved and translated counts.
			Assert.AreEqual(4, endoc.StringCount);
			Assert.AreEqual(0, endoc.NumberApproved);
			Assert.AreEqual(0, endoc.NumberTranslated);

			var frfile = Path.Combine(_testFolder, "Test.fr.xlf");
			var frdoc = XLiffDocument.Read(frfile);
			Assert.AreEqual(4, frdoc.StringCount);
			Assert.AreEqual(0, frdoc.NumberApproved);
			Assert.AreEqual(4, frdoc.NumberTranslated);

			var esfile = Path.Combine(_testFolder, "Test.es.xlf");
			var esdoc = XLiffDocument.Read(esfile);
			Assert.AreEqual(4, esdoc.StringCount);
			Assert.AreEqual(2, esdoc.NumberApproved);
			Assert.AreEqual(3, esdoc.NumberTranslated);
		}
	}
}

