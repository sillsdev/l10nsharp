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
		public void ValidateAgainstSchema()
		{
			var folder = new TempFolder("FileLocation");
			Directory.CreateDirectory(folder.Path);
			LocalizationManagerTests.SetupManager(folder);
			var installedXliffDir = "../../src/L10NSharpTests/TestXliff";

			var schemaLocation = Path.Combine(installedXliffDir, "xliff-core-1.2-strict.xsd");
			var schemas = new XmlSchemaSet();
			using (var reader = XmlReader.Create(schemaLocation))
			{
				schemas.Add("urn:oasis:names:tc:xliff:document:1.2", reader);

				//English
				var document = XDocument.Load(Path.Combine(folder.Path, "test.en.xlf"));
				document.Validate(schemas, (sender, args) =>
					Assert.Fail("Xliff saved at {0} did not validate against schema: {1}", Path.Combine(folder.Path, "test.en.xlf"), args.Message));

				//French
				document = XDocument.Load(Path.Combine(folder.Path, "test.fr.xlf"));
				document.Validate(schemas, (sender, args) =>
					Assert.Fail("Xliff saved at {0} did not validate against schema: {1}", Path.Combine(folder.Path, "test.fr.xlf"), args.Message));

				//Arabic
				document = XDocument.Load(Path.Combine(folder.Path, "test.ar.xlf"));
				document.Validate(schemas, (sender, args) =>
					Assert.Fail("Xliff saved at {0} did not validate against schema: {1}", Path.Combine(folder.Path, "test.ar.xlf"), args.Message));
			}
		}
	}
}
