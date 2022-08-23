// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class XLiffSchemaValidationTestsXliff
	{
		private static string SchemaLocation
		{
			get
			{
				var dir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
				var installedXliffDir = "../../../src/L10NSharpTests/TestXliff";

				var schemaLocation = Path.Combine(dir, installedXliffDir,
					"xliff-core-1.2-transitional.xsd");
				return schemaLocation;
			}
		}

		[SetUp]
		public void Setup()
		{
			LocalizationManager.TranslationMemoryKind = TranslationMemory.XLiff;
		}

		[Test]
		public void ValidateInFoldersAgainstSchema()
		{
			LocalizationManager.UseLanguageCodeFolders = true;
			using (var folder = new TempFolder("FileLocation"))
			{
				Directory.CreateDirectory(folder.Path);
				new XLiffLocalizationManagerTests().SetupManager(folder);

				var schemas = new XmlSchemaSet();
				using (var reader = XmlReader.Create(SchemaLocation))
				{
					schemas.Add("urn:oasis:names:tc:xliff:document:1.2", reader);

					//English
					var filename =
						LocalizationManager.GetTranslationFileNameForLanguage("test", "en");
					Assert.AreEqual(Path.Combine("en", "test.xlf"), filename);
					var filepath = Path.Combine(folder.Path, filename);
					var document = XDocument.Load(filepath);
					document.Validate(schemas, (sender, args) =>
						Assert.Fail("Xliff saved at {0} did not validate against schema: {1}",
							filepath, args.Message));

					//French
					filename =
						LocalizationManager.GetTranslationFileNameForLanguage("test", "fr");
					Assert.AreEqual(Path.Combine("fr", "test.xlf"), filename);
					filepath = Path.Combine(folder.Path, filename);
					document = XDocument.Load(filepath);
					document.Validate(schemas, (sender, args) =>
						Assert.Fail("Xliff saved at {0} did not validate against schema: {1}",
							filepath, args.Message));

					//Arabic
					filename =
						LocalizationManager.GetTranslationFileNameForLanguage("test", "ar");
					Assert.AreEqual(Path.Combine("ar", "test.xlf"), filename);
					filepath = Path.Combine(folder.Path, filename);
					document = XDocument.Load(filepath);
					document.Validate(schemas, (sender, args) =>
						Assert.Fail("Xliff saved at {0} did not validate against schema: {1}",
							filepath, args.Message));
				}
			}
		}

		[Test]
		public void ValidateFlatFilesAgainstSchema()
		{
			using (var folder = new TempFolder("FileLocation"))
			{
				Directory.CreateDirectory(folder.Path);
				new XLiffLocalizationManagerTests().SetupManager(folder);

				var schemas = new XmlSchemaSet();
				using (var reader = XmlReader.Create(SchemaLocation))
				{
					schemas.Add("urn:oasis:names:tc:xliff:document:1.2", reader);

					//English
					var filename =
						LocalizationManager.GetTranslationFileNameForLanguage("test", "en");
					Assert.AreEqual("test.en.xlf", filename);
					var filepath = Path.Combine(folder.Path, filename);
					var document = XDocument.Load(filepath);
					document.Validate(schemas, (sender, args) =>
						Assert.Fail("Xliff saved at {0} did not validate against schema: {1}",
							filepath, args.Message));

					//French
					filename =
						LocalizationManager.GetTranslationFileNameForLanguage("test", "fr");
					Assert.AreEqual("test.fr.xlf", filename);
					filepath = Path.Combine(folder.Path, filename);
					document = XDocument.Load(filepath);
					document.Validate(schemas, (sender, args) =>
						Assert.Fail("Xliff saved at {0} did not validate against schema: {1}",
							filepath, args.Message));

					//Arabic
					filename =
						LocalizationManager.GetTranslationFileNameForLanguage("test", "ar");
					Assert.AreEqual("test.ar.xlf", filename);
					filepath = Path.Combine(folder.Path, filename);
					document = XDocument.Load(filepath);
					document.Validate(schemas, (sender, args) =>
						Assert.Fail("Xliff saved at {0} did not validate against schema: {1}",
							filepath, args.Message));
				}
			}
		}
	}
}
