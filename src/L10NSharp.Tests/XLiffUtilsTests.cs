using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using L10NSharp.XLiffUtils;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class XLiffUtilsTests
	{
		private readonly string _testFolder;

		public XLiffUtilsTests()
		{
			var asmFile = Assembly.GetExecutingAssembly().Location;
			var folder = Path.GetDirectoryName(asmFile);	// will be something like <repodir>/output/Debug
			folder = Path.GetDirectoryName(folder);
			folder = Path.GetDirectoryName(folder);
			folder = Path.GetDirectoryName(folder);
			_testFolder = Path.Combine(folder, "src", "L10NSharp.Tests", "TestXliff");
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
			// Code at the LocalizationManagerInternal level will make English look okay for display by
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

		[Test]
		public void TestReturningAllStrings()
		{
			var enfile = Path.Combine(_testFolder, "Test.en.xlf");
			var endoc = XLiffDocument.Read(enfile);
			// These numbers may be counter-intuitive, but then the English isn't translated, is it?
			// Code at the LocalizationManagerInternal level will make English look okay for display by
			// faking the approved and translated counts.
			Assert.AreEqual(4, endoc.StringCount);
			string source;
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Crow", out source));
			Assert.AreEqual("It's a crow", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Raven", out source));
			Assert.AreEqual("It's not a crow", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Chicken.Rooster", out source));
			Assert.AreEqual("It's a chicken", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Eagle", out source));
			Assert.AreEqual("Fish-eating bird", source);

			var frfile = Path.Combine(_testFolder, "Test.fr.xlf");
			var frdoc = XLiffDocument.Read(frfile);
			Assert.AreEqual(4, frdoc.StringCount);
			Assert.IsTrue(frdoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Crow", out source));
			Assert.AreEqual("C'est un corbeau", source);
			Assert.IsTrue(frdoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Raven", out source));
			Assert.AreEqual("Ce n'est pas un corbeau", source);
			Assert.IsTrue(frdoc.File.Body.TranslationsById.TryGetValue("TestItem.Chicken.Rooster", out source));
			Assert.AreEqual("C'est un poulet", source);
			Assert.IsTrue(frdoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Eagle", out source));
			Assert.AreEqual("Un oiseau qui mange des poissons", source);

			var esfile = Path.Combine(_testFolder, "Test.es.xlf");
			var esdoc = XLiffDocument.Read(esfile);
			Assert.AreEqual(4, esdoc.StringCount);
			Assert.IsTrue(esdoc.File.Body.TranslationsById.TryGetValue("SettingsProtection.CtrlShiftHint", out source));
			Assert.AreEqual("El botón se mostrará cuando se mantengan presionadas juntas las teclas Ctrl y Mayús.", source);
			Assert.IsTrue(esdoc.File.Body.TranslationsById.TryGetValue("SettingsProtection.LauncherButtonLabel", out source));
			Assert.AreEqual("Protección de configuraciones...", source);
			Assert.IsTrue(esdoc.File.Body.TranslationsById.TryGetValue("SettingsProtection.NormallyHiddenCheckbox", out source));
			Assert.AreEqual("Ocultar el botón que abre la configuración.", source);
			Assert.IsTrue(esdoc.File.Body.TranslationsById.TryGetValue("SettingsProtection.PasswordDialog.FactoryPassword", out source));
			Assert.AreEqual("Factory Password", source);	// note it's stored and returned even though marked as needing translation
		}

		[Test]
		public void TestReturningOnlyApprovedStrings()
		{
			try
			{
				LocalizationManager.ReturnOnlyApprovedStrings = true;
				var enfile = Path.Combine(_testFolder, "Test.en.xlf");
				var endoc = XLiffDocument.Read(enfile);
				string source;
				Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Crow", out source));
				Assert.AreEqual("It's a crow", source);
				Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Raven", out source));
				Assert.AreEqual("It's not a crow", source);
				Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Chicken.Rooster", out source));
				Assert.AreEqual("It's a chicken", source);
				Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Eagle", out source));
				Assert.AreEqual("Fish-eating bird", source);

				var frfile = Path.Combine(_testFolder, "Test.fr.xlf");
				var frdoc = XLiffDocument.Read(frfile);
				Assert.IsFalse(frdoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Crow", out source));
				Assert.IsFalse(frdoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Raven", out source));
				Assert.IsFalse(frdoc.File.Body.TranslationsById.TryGetValue("TestItem.Chicken.Rooster", out source));
				Assert.IsFalse(frdoc.File.Body.TranslationsById.TryGetValue("TestItem.Bird.Eagle", out source));

				var esfile = Path.Combine(_testFolder, "Test.es.xlf");
				var esdoc = XLiffDocument.Read(esfile);
				Assert.IsTrue(esdoc.File.Body.TranslationsById.TryGetValue("SettingsProtection.CtrlShiftHint", out source));
				Assert.AreEqual("El botón se mostrará cuando se mantengan presionadas juntas las teclas Ctrl y Mayús.", source);
				Assert.IsFalse(esdoc.File.Body.TranslationsById.TryGetValue("SettingsProtection.LauncherButtonLabel", out source));
				Assert.IsTrue(esdoc.File.Body.TranslationsById.TryGetValue("SettingsProtection.NormallyHiddenCheckbox", out source));
				Assert.AreEqual("Ocultar el botón que abre la configuración.", source);
				Assert.IsFalse(esdoc.File.Body.TranslationsById.TryGetValue("SettingsProtection.PasswordDialog.FactoryPassword", out source));
			}
			finally
			{
				LocalizationManager.ReturnOnlyApprovedStrings = false;	// restore default for other tests
			}
		}

		[Test]
		public void TestReturningHtmlMarkup()
		{
			var enfile = Path.Combine(_testFolder, "Test2.en.xlf");
			var endoc = XLiffDocument.Read(enfile);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.PlainText", out var source));
			Assert.AreEqual("This is plain text.", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Bold", out source));
			Assert.AreEqual("This is <strong>bold</strong>.", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Italic", out source));
			Assert.AreEqual("This is <em>italic</em>.", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Superscript", out source));
			Assert.AreEqual("This is <sup>superscript</sup>.", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Link", out source));
			Assert.AreEqual("This is a <a href=\"https://sil.org\" id=\"note1\">link</a>.", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Image", out source));
			Assert.AreEqual("This has an embedded image: <img src=\"images/test.png\" alt=\"This is a test.\"/>.", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Paragraph", out source));
			Assert.AreEqual("<p>This is a paragraph.</p>", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Div", out source));
			Assert.AreEqual("<div class=\"author\">This is a div.</div>", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Blockquote", out source));
			Assert.AreEqual("<blockquote class=\"poetry\">This is a block quote.</blockquote>", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Pre", out source));
			Assert.AreEqual("<pre>This is in"+Environment.NewLine+"presentation"+Environment.NewLine+"display.</pre>", source);
			Assert.IsTrue(endoc.File.Body.TranslationsById.TryGetValue("TestItem.Multiple", out source));
			Assert.AreEqual("<em>This</em> is more <a href=\"https://mit.edu\">complex</a><strong>!!</strong>", source);
		}
	}
}
