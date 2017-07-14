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
		public void TestGetAllVariantLanguagesFound()
		{
			var enfile = Path.Combine(_testFolder, "Test.en.xlf");
			var doc = XLiffDocument.Read(enfile);
			var variantsList = new List<string>();
			variantsList.AddRange(doc.GetAllVariantLanguagesFound(false));
			Assert.AreEqual(1, variantsList.Count);
			Assert.AreEqual("en", variantsList[0]);
			variantsList.Clear();

			variantsList.AddRange(doc.GetAllVariantLanguagesFound(true));
			Assert.AreEqual(1, variantsList.Count);
			Assert.AreEqual("en", variantsList[0]);
			variantsList.Clear();

			var frfile = Path.Combine(_testFolder, "Test.fr.xlf");
			doc = XLiffDocument.Read(frfile);
			variantsList.AddRange(doc.GetAllVariantLanguagesFound(false));
			Assert.AreEqual(1, variantsList.Count);
			Assert.AreEqual("en", variantsList[0]);
			variantsList.Clear();

			variantsList.AddRange(doc.GetAllVariantLanguagesFound(true));
			Assert.AreEqual(2, variantsList.Count);
			variantsList.Sort();
			Assert.AreEqual("en", variantsList[0]);	// redundant to be sure...
			Assert.AreEqual("fr", variantsList[1]);
			variantsList.Clear();
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

