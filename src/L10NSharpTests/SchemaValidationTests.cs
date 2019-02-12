// ---------------------------------------------------------------------------------------------
#region
// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#endregion
//
// File: SchemaValidationTests.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.IO;
using NUnit.Framework;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class SchemaValidationTests
	{
		[Test]
		public void ValidateInFoldersAgainstSchema()
		{
			LocalizationManager.UseLanguageCodeFolders = true;
			var folder = new TempFolder("FileLocation");
			Directory.CreateDirectory(folder.Path);
			LocalizationManagerTests.SetupManager(folder);
			var installedXliffDir = "../../../src/L10NSharpTests/TestXliff";

			var schemaLocation = Path.Combine(installedXliffDir, "xliff-core-1.2-transitional.xsd");
			var schemas = new XmlSchemaSet();
			using (var reader = XmlReader.Create(schemaLocation))
			{
				schemas.Add("urn:oasis:names:tc:xliff:document:1.2", reader);

				//English
				var filename = LocalizationManager.GetXliffFileNameForLanguage("test", "en");
				Assert.AreEqual(Path.Combine("en", "test.xlf"), filename);
				var filepath = Path.Combine(folder.Path, filename);
				var document = XDocument.Load(filepath);
				document.Validate(schemas, (sender, args) =>
					Assert.Fail("Xliff saved at {0} did not validate against schema: {1}", filepath, args.Message));

				//French
				filename = LocalizationManager.GetXliffFileNameForLanguage("test", "fr");
				Assert.AreEqual(Path.Combine("fr", "test.xlf"), filename);
				filepath = Path.Combine(folder.Path, filename);
				document = XDocument.Load(filepath);
				document.Validate(schemas, (sender, args) =>
					Assert.Fail("Xliff saved at {0} did not validate against schema: {1}", filepath, args.Message));

				//Arabic
				filename = LocalizationManager.GetXliffFileNameForLanguage("test", "ar");
				Assert.AreEqual(Path.Combine("ar", "test.xlf"), filename);
				filepath = Path.Combine(folder.Path, filename);
				document = XDocument.Load(filepath);
				document.Validate(schemas, (sender, args) =>
					Assert.Fail("Xliff saved at {0} did not validate against schema: {1}", filepath, args.Message));
			}
		}

		[Test]
		public void ValidateFlatFilesAgainstSchema()
		{
			var folder = new TempFolder("FileLocation");
			Directory.CreateDirectory(folder.Path);
			LocalizationManagerTests.SetupManager(folder);
			var installedXliffDir = "../../../src/L10NSharpTests/TestXliff";

			var schemaLocation = Path.Combine(installedXliffDir, "xliff-core-1.2-transitional.xsd");
			var schemas = new XmlSchemaSet();
			using (var reader = XmlReader.Create(schemaLocation))
			{
				schemas.Add("urn:oasis:names:tc:xliff:document:1.2", reader);

				//English
				var filename = LocalizationManager.GetXliffFileNameForLanguage("test", "en");
				Assert.AreEqual("test.en.xlf", filename);
				var filepath = Path.Combine(folder.Path, filename);
				var document = XDocument.Load(filepath);
				document.Validate(schemas, (sender, args) =>
					Assert.Fail("Xliff saved at {0} did not validate against schema: {1}", filepath, args.Message));

				//French
				filename = LocalizationManager.GetXliffFileNameForLanguage("test", "fr");
				Assert.AreEqual("test.fr.xlf", filename);
				filepath = Path.Combine(folder.Path, filename);
				document = XDocument.Load(filepath);
				document.Validate(schemas, (sender, args) =>
					Assert.Fail("Xliff saved at {0} did not validate against schema: {1}", filepath, args.Message));

				//Arabic
				filename = LocalizationManager.GetXliffFileNameForLanguage("test", "ar");
				Assert.AreEqual("test.ar.xlf", filename);
				filepath = Path.Combine(folder.Path, filename);
				document = XDocument.Load(filepath);
				document.Validate(schemas, (sender, args) =>
					Assert.Fail("Xliff saved at {0} did not validate against schema: {1}", filepath, args.Message));
			}
		}
	}
}
