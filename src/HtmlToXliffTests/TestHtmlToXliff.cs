using System;
using System.Xml;
using HtmlAgilityPack;
using HtmlToXliff;
using NUnit.Framework;
using System.IO;
using System.Xml.Schema;

namespace HtmlToXliffTests
{
	/// <summary>
	/// Test the conversions from HTML to XLIFF.  The examples, unless otherwise noted, are adapted from
	/// http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html.
	/// </summary>
	/// <remarks>
	/// I know some of the test code might possibly be simplified by methods, but it's easier to track
	/// explicit test failures with straight line code.
	/// </remarks>
	[TestFixture]
	public class TestHtmlToXliff
	{
		[TestFixtureSetUp]
		public void FixHtmlParsing()
		{
			HtmlToXliffConverter.FixHtmlParserBug();	// call before loading any HtmlDocument!
		}

		[Test]
		public void TestTableRepresentation()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
 <body>
  <h1 class=""title"">Report</h1>
  <table border=""1"" width=""100%"">
   <tr>
    <td valign=""top"">Text in cell r1-c1</td>
    <td valign=""top"">Text in cell r1-c2</td>
   </tr>
   <tr>
    <td bgcolor=""#C0C0C0"">Text in cell r2-c1</td>
    <td>Text in cell r2-c2</td>
   </tr>
  </table>
  <p>All rights reserved (c) Gandalf Inc.</p>
 </body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestTableRepresentation did not validate against schema: {0}");

			var fileX = xmlDoc.SelectSingleNode("/xliff/file");
			Assert.IsNotNull(fileX);
			Assert.AreEqual(3, fileX.Attributes.Count);
			Assert.AreEqual("sample.htm", fileX.Attributes["original"].Value);
			Assert.AreEqual("html", fileX.Attributes["datatype"].Value);
			Assert.AreEqual("en", fileX.Attributes["source-language"].Value);

			var bodyX = fileX.SelectSingleNode("body");
			Assert.IsNotNull(bodyX);
			Assert.AreEqual(0, bodyX.Attributes.Count);
			Assert.LessOrEqual(1, bodyX.ChildNodes.Count);
			int count0 = 0;
			foreach (XmlNode n0 in bodyX.ChildNodes)
			{
				++count0;
				Assert.AreEqual("group", n0.Name);
				Assert.AreEqual(1, n0.Attributes.Count);
				Assert.AreEqual("x-html-html", n0.Attributes["restype"].Value);
				int count1 = 0;
				foreach (XmlNode n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("group", n1.Name);
					Assert.AreEqual(1, n1.Attributes.Count);
					Assert.AreEqual("x-html-body", n1.Attributes["restype"].Value);
					int count2 = 0;
					foreach (XmlNode n2 in n1.ChildNodes)
					{
						++count2;
						int count3 = 0;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("trans-unit", n2.Name);
							Assert.AreEqual(3, n2.Attributes.Count);
							Assert.AreEqual("genid-1", n2.Attributes["id"].Value);
							Assert.AreEqual("x-html-h1", n2.Attributes["restype"].Value);
							Assert.AreEqual("title", n2.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
							foreach (XmlNode n3 in n2.ChildNodes)
							{
								++count3;
								Assert.AreEqual("source", n3.Name);
								Assert.AreEqual(1, n3.Attributes.Count);
								Assert.AreEqual("en", n3.Attributes["xml:lang"].Value);
								Assert.AreEqual("Report", n3.InnerText);
							}
							Assert.AreEqual(count3, 1);
							break;
						case 2:
							Assert.AreEqual("group", n2.Name);
							Assert.AreEqual(3, n2.Attributes.Count);
							Assert.AreEqual("table", n2.Attributes["restype"].Value);
							Assert.AreEqual("1", n2.Attributes["border", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual("100%", n2.Attributes["width", HtmlToXliffConverter.kHtmlNamespace].Value);
							foreach (XmlNode n3 in n2.ChildNodes)
							{
								++count3;
								Assert.AreEqual("group", n3.Name);
								Assert.AreEqual(1, n3.Attributes.Count);
								Assert.AreEqual("row", n3.Attributes["restype"].Value);
								int count4 = 0;
								switch (count3)
								{
								case 1:
									foreach (XmlNode n4 in n3.ChildNodes)
									{
										++count4;
										Assert.AreEqual("trans-unit", n4.Name);
										Assert.AreEqual(3, n4.Attributes.Count);
										Assert.AreEqual("cell", n4.Attributes["restype"].Value);
										Assert.AreEqual("top", n4.Attributes["valign", HtmlToXliffConverter.kHtmlNamespace].Value);
										int count5 = 0;
										switch (count4)
										{
										case 1:
											Assert.AreEqual("genid-2", n4.Attributes["id"].Value);
											foreach (XmlNode n5 in n4.ChildNodes)
											{
												++count5;
												Assert.AreEqual("source", n5.Name);
												Assert.AreEqual(1, n5.Attributes.Count);
												Assert.AreEqual("en", n5.Attributes["xml:lang"].Value);
												Assert.AreEqual("Text in cell r1-c1", n5.InnerText);
											}
											Assert.AreEqual(1, count5);
											break;
										case 2:
											Assert.AreEqual("genid-3", n4.Attributes["id"].Value);
											foreach (XmlNode n5 in n4.ChildNodes)
											{
												++count5;
												Assert.AreEqual("source", n5.Name);
												Assert.AreEqual(1, n5.Attributes.Count);
												Assert.AreEqual("en", n5.Attributes["xml:lang"].Value);
												Assert.AreEqual("Text in cell r1-c2", n5.InnerText);
											}
											break;
										}
										Assert.AreEqual(1, count5);
									}
									break;
								case 2:
									foreach (XmlNode n4 in n3.ChildNodes)
									{
										++count4;
										Assert.AreEqual("trans-unit", n4.Name);
										Assert.AreEqual("cell", n4.Attributes["restype"].Value);
										int count5 = 0;
										switch (count4)
										{
										case 1:
											Assert.AreEqual(3, n4.Attributes.Count);
											Assert.AreEqual("genid-4", n4.Attributes["id"].Value);
											Assert.AreEqual("#C0C0C0", n4.Attributes["bgcolor", HtmlToXliffConverter.kHtmlNamespace].Value);
											foreach (XmlNode n5 in n4.ChildNodes)
											{
												++count5;
												Assert.AreEqual("source", n5.Name);
												Assert.AreEqual(1, n5.Attributes.Count);
												Assert.AreEqual("en", n5.Attributes["xml:lang"].Value);
												Assert.AreEqual("Text in cell r2-c1", n5.InnerText);
											}
											Assert.AreEqual(1, count5);
											break;
										case 2:
											Assert.AreEqual(2, n4.Attributes.Count);
											Assert.AreEqual("genid-5", n4.Attributes["id"].Value);
											foreach (XmlNode n5 in n4.ChildNodes)
											{
												++count5;
												Assert.AreEqual("source", n5.Name);
												Assert.AreEqual(1, n5.Attributes.Count);
												Assert.AreEqual("en", n5.Attributes["xml:lang"].Value);
												Assert.AreEqual("Text in cell r2-c2", n5.InnerText);
											}
											break;
										}
										Assert.AreEqual(1, count5);
									}
									break;
								}
								Assert.AreEqual(2, count4);
							}
							Assert.AreEqual(2, count3);
							break;
						case 3:
							Assert.AreEqual("trans-unit", n2.Name);
							Assert.AreEqual(2, n2.Attributes.Count);
							Assert.AreEqual("genid-6", n2.Attributes["id"].Value);
							Assert.AreEqual("x-html-p", n2.Attributes["restype"].Value);
							foreach (XmlNode n3 in n2.ChildNodes)
							{
								++count3;
								Assert.AreEqual("source", n3.Name);
								Assert.AreEqual(1, n3.Attributes.Count);
								Assert.AreEqual("en", n3.Attributes["xml:lang"].Value);
								Assert.AreEqual("All rights reserved (c) Gandalf Inc.", n3.InnerText);
							}
							Assert.AreEqual(1, count3);
							break;
						}
					}
					Assert.AreEqual(3, count2);
				}
				Assert.AreEqual(1, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestInlineSpans()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
<body>
<p>Questions will appear in <span fontcolor=""#339966"">Green
face</span>, while answers will appear in <span fontcolor=""#333399"">Indigo
face</span>.</p>
</body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestInlineSpans did not validate against schema: {0}");

			var fileX = xmlDoc.SelectSingleNode("/xliff/file");
			Assert.IsNotNull(fileX);
			Assert.AreEqual(3, fileX.Attributes.Count);
			Assert.AreEqual("sample.htm", fileX.Attributes["original"].Value);
			Assert.AreEqual("html", fileX.Attributes["datatype"].Value);
			Assert.AreEqual("en", fileX.Attributes["source-language"].Value);

			var bodyX = fileX.SelectSingleNode("body");
			Assert.IsNotNull(bodyX);
			Assert.AreEqual(0, bodyX.Attributes.Count);
			Assert.LessOrEqual(1, bodyX.ChildNodes.Count);
			int count0 = 0;
			foreach (XmlNode n0 in bodyX.ChildNodes)
			{
				++count0;
				Assert.AreEqual("group", n0.Name);
				Assert.AreEqual(1, n0.Attributes.Count);
				Assert.AreEqual("x-html-html", n0.Attributes["restype"].Value);
				int count1 = 0;
				foreach (XmlNode n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("group", n1.Name);
					Assert.AreEqual(1, n1.Attributes.Count);
					Assert.AreEqual("x-html-body", n1.Attributes["restype"].Value);
					int count2 = 0;
					foreach (XmlNode n2 in n1.ChildNodes)
					{
						++count2;
						Assert.AreEqual("trans-unit", n2.Name);
						Assert.AreEqual(2, n2.Attributes.Count);
						Assert.AreEqual("genid-1", n2.Attributes["id"].Value);
						Assert.AreEqual("x-html-p", n2.Attributes["restype"].Value);
						int count3 = 0;
						foreach (XmlNode n3 in n2.ChildNodes)
						{
							++count3;
							Assert.AreEqual("source", n3.Name);
							Assert.AreEqual(1, n3.Attributes.Count);
							Assert.AreEqual("en", n3.Attributes["xml:lang"].Value);
							// partial check ignoring internal elements
							Assert.AreEqual(@"Questions will appear in Green
face, while answers will appear in Indigo
face.", n3.InnerText);
							int count4 = 0;
							foreach (XmlNode n4 in n3.ChildNodes)
							{
								++count4;
								switch (count4)
								{
								case 1:		// "Questions will appear in "
									Assert.AreEqual("#text", n4.Name);
									Assert.AreEqual("Questions will appear in ", n4.InnerText);
									break;
								case 2:		// "<span fontcolor="#339966">Green\nface</span>"
									Assert.AreEqual("g", n4.Name);
									Assert.AreEqual(3, n4.Attributes.Count);
									Assert.AreEqual("genid-2", n4.Attributes["id"].Value);
									Assert.AreEqual("x-html-span", n4.Attributes["ctype"].Value);
									Assert.AreEqual("#339966", n4.Attributes["fontcolor", HtmlToXliffConverter.kHtmlNamespace].Value);
									Assert.AreEqual(1, n4.ChildNodes.Count);
									Assert.AreEqual("#text", n4.FirstChild.Name);
									Assert.AreEqual(@"Green
face", n4.InnerText);
									break;
								case 3:		// ", while answers will appear in "
									Assert.AreEqual("#text", n4.Name);
									Assert.AreEqual(", while answers will appear in ", n4.InnerText);
									break;
								case 4:		// "<span fontcolor="#333399">Indigo\nface</span>"
									Assert.AreEqual("g", n4.Name);
									Assert.AreEqual("genid-3", n4.Attributes["id"].Value);
									Assert.AreEqual("x-html-span", n4.Attributes["ctype"].Value);
									Assert.AreEqual("#333399", n4.Attributes["fontcolor", HtmlToXliffConverter.kHtmlNamespace].Value);
									Assert.AreEqual(1, n4.ChildNodes.Count);
									Assert.AreEqual("#text", n4.FirstChild.Name);
									Assert.AreEqual(@"Indigo
face", n4.InnerText);
									break;
								case 5:		// "."
									Assert.AreEqual("#text", n4.Name);
									Assert.AreEqual(".", n4.InnerText);
									break;
								}
							}
							Assert.AreEqual(5, count4);
						}
						Assert.AreEqual(1, count3);
					}
					Assert.AreEqual(1, count2);
				}
				Assert.AreEqual(1, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestInlineElementWithLang()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
<body>
<P>The words <Q lang=""fr"">Je me souviens</Q> are the motto of Québec.</P>
</body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestInlineElementWithLang did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(body);
			int count0 = 0;
			foreach (XmlNode n0 in body.ChildNodes)
			{
				++count0;
				Assert.AreEqual("trans-unit", n0.Name);
				Assert.AreEqual(2, n0.Attributes.Count);
				Assert.AreEqual("genid-1", n0.Attributes["id"].Value);
				Assert.AreEqual("x-html-p", n0.Attributes["restype"].Value);
				int count1 = 0;
				foreach (XmlNode n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("source", n1.Name);
					Assert.AreEqual(1, n1.Attributes.Count);
					Assert.AreEqual("en", n1.Attributes["xml:lang"].Value);
					// partial check ignoring internal elements
					Assert.AreEqual("The words Je me souviens are the motto of Québec.", n1.InnerText);
					int count2 = 0;
					foreach (XmlNode n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("The words ", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("g", n2.Name);
							Assert.AreEqual(3, n2.Attributes.Count);
							Assert.AreEqual("genid-2", n2.Attributes["id"].Value);
							Assert.AreEqual("x-html-q", n2.Attributes["ctype"].Value);
							Assert.AreEqual("fr", n2.Attributes["xml:lang"].Value);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual("Je me souviens", n2.InnerText);
							break;
						case 3:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(" are the motto of Québec.", n2.InnerText);
							break;
						}
					}
					Assert.AreEqual(3, count2);
				}
				Assert.AreEqual(1, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestInlineElements()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
<body>
<p>In Portland, Oregon one may <i>ski</i> on the mountain, <b>wind surf</b> in the gorge, and <i>surf</i> in the ocean, all on the same day.</p>
</body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestInlineElements did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(body);
			int count0 = 0;
			foreach (XmlNode n0 in body.ChildNodes)
			{
				++count0;
				Assert.AreEqual("trans-unit", n0.Name);
				Assert.AreEqual(2, n0.Attributes.Count);
				Assert.AreEqual("genid-1", n0.Attributes["id"].Value);
				Assert.AreEqual("x-html-p", n0.Attributes["restype"].Value);
				int count1 = 0;
				foreach (XmlNode n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("source", n1.Name);
					Assert.AreEqual(1, n1.Attributes.Count);
					Assert.AreEqual("en", n1.Attributes["xml:lang"].Value);
					// preliminary check
					Assert.AreEqual("In Portland, Oregon one may ski on the mountain, wind surf in the gorge, and surf in the ocean, all on the same day.", n1.InnerText);
					int count2 = 0;
					foreach (XmlNode n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("In Portland, Oregon one may ", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("g", n2.Name);
							Assert.AreEqual(2, n2.Attributes.Count);
							Assert.AreEqual("genid-2", n2.Attributes["id"].Value);
							Assert.AreEqual("italic", n2.Attributes["ctype"].Value);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual("ski", n2.FirstChild.InnerText);
							break;
						case 3:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(" on the mountain, ", n2.InnerText);
							break;
						case 4:
							Assert.AreEqual("g", n2.Name);
							Assert.AreEqual(2, n2.Attributes.Count);
							Assert.AreEqual("genid-3", n2.Attributes["id"].Value);
							Assert.AreEqual("bold", n2.Attributes["ctype"].Value);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual("wind surf", n2.FirstChild.InnerText);
							break;
						case 5:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(" in the gorge, and ", n2.InnerText);
							break;
						case 6:
							Assert.AreEqual("g", n2.Name);
							Assert.AreEqual(2, n2.Attributes.Count);
							Assert.AreEqual("genid-4", n2.Attributes["id"].Value);
							Assert.AreEqual("italic", n2.Attributes["ctype"].Value);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual("surf", n2.FirstChild.InnerText);
							break;
						case 7:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(" in the ocean, all on the same day.", n2.InnerText);
							break;
						}
					}
					Assert.AreEqual(7, count2);
				}
				Assert.AreEqual(1, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestImg()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
<body>
<p>This is Mount Hood: <img src='mthood.jpg'></p>
</body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestImg did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(body);
			int count0 = 0;
			foreach (XmlNode n0 in body.ChildNodes)
			{
				++count0;
				Assert.AreEqual("trans-unit", n0.Name);
				Assert.AreEqual(2, n0.Attributes.Count);
				Assert.AreEqual("genid-1", n0.Attributes["id"].Value);
				Assert.AreEqual("x-html-p", n0.Attributes["restype"].Value);
				int count1 = 0;
				foreach (XmlNode n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("source", n1.Name);
					Assert.AreEqual(1, n1.Attributes.Count);
					Assert.AreEqual("en", n1.Attributes["xml:lang"].Value);
					// preliminary check
					Assert.AreEqual("This is Mount Hood: ", n1.InnerText);
					int count2 = 0;
					foreach (XmlNode n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("This is Mount Hood: ", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("x", n2.Name);
							Assert.AreEqual("", n2.InnerText);
							Assert.AreEqual(3, n2.Attributes.Count);
							Assert.AreEqual("genid-2", n2.Attributes["id"].Value);
							Assert.AreEqual("image", n2.Attributes["ctype"].Value);
							Assert.AreEqual("mthood.jpg", n2.Attributes["src", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual(0, n2.ChildNodes.Count);
							break;
						}
					}
					Assert.AreEqual(2, count2);
				}
				Assert.AreEqual(1, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestClassAttribute()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
<body>
<h2 class=""article-title"">Life and Habitat of the Marmot</h2>
</body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestClassAttribute did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(body);
			int count0 = 0;
			foreach (XmlNode n0 in body.ChildNodes)
			{
				++count0;
				Assert.AreEqual("trans-unit", n0.Name);
				Assert.AreEqual(3, n0.Attributes.Count);
				Assert.AreEqual("genid-1", n0.Attributes["id"].Value);
				Assert.AreEqual("x-html-h2", n0.Attributes["restype"].Value);
				Assert.AreEqual("article-title", n0.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
				Assert.AreEqual(1, n0.ChildNodes.Count);
				Assert.AreEqual("source", n0.FirstChild.Name);
				Assert.AreEqual("Life and Habitat of the Marmot", n0.FirstChild.InnerText);
				Assert.AreEqual(1, n0.FirstChild.ChildNodes.Count);
				Assert.AreEqual("#text", n0.FirstChild.FirstChild.Name);
				Assert.AreEqual("Life and Habitat of the Marmot", n0.FirstChild.FirstChild.InnerText);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestImgWithTitleandAlt()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
<body>
<p title='Information about Mount Hood'>This is Mount Hood: <img src=""mthood.jpg"" alt=""Mount Hood with its snow-covered top""></p>
</body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestImgWithTitleandAlt did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(body);
			int count0 = 0;
			foreach (XmlNode n0 in body.ChildNodes)
			{
				++count0;
				Assert.AreEqual("trans-unit", n0.Name);
				Assert.AreEqual(2, n0.Attributes.Count);
				Assert.AreEqual("genid-1", n0.Attributes["id"].Value);
				Assert.AreEqual("x-html-p", n0.Attributes["restype"].Value);
				int count1 = 0;
				foreach (XmlNode n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("source", n1.Name);
					Assert.AreEqual(1, n1.Attributes.Count);
					Assert.AreEqual("en", n1.Attributes["xml:lang"].Value);
					int count2 = 0;
					foreach (XmlNode n2 in n1.ChildNodes)
					{
						++count2;
						int count3 = 0;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("ph", n2.Name);
							Assert.AreEqual(1, n2.Attributes.Count);
							Assert.AreEqual("genid-2", n2.Attributes["id"].Value);
							// quick check
							Assert.AreEqual("Information about Mount Hood", n2.InnerText);
							foreach (XmlNode n3 in n2.ChildNodes)
							{
								++count3;
								Assert.AreEqual("sub", n3.Name);
								Assert.AreEqual(1, n3.Attributes.Count);
								Assert.AreEqual("x-html-p-title", n3.Attributes["ctype"].Value);
								Assert.AreEqual(1, n3.ChildNodes.Count);
								Assert.AreEqual("#text", n3.FirstChild.Name);
								Assert.AreEqual("Information about Mount Hood", n3.InnerText);
							}
							Assert.AreEqual(1, count3);
							break;
						case 2:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("This is Mount Hood: ", n2.InnerText);
							break;
						case 3:
							Assert.AreEqual("ph", n2.Name);
							Assert.AreEqual(3, n2.Attributes.Count);
							Assert.AreEqual("genid-3", n2.Attributes["id"].Value);
							Assert.AreEqual("image", n2.Attributes["ctype"].Value);
							Assert.AreEqual("mthood.jpg", n2.Attributes["src", HtmlToXliffConverter.kHtmlNamespace].Value);
							// quick check
							Assert.AreEqual("Mount Hood with its snow-covered top", n2.InnerText);
							foreach (XmlNode n3 in n2.ChildNodes)
							{
								++count3;
								Assert.AreEqual("sub", n3.Name);
								Assert.AreEqual(1, n3.Attributes.Count);
								Assert.AreEqual("x-html-img-alt", n3.Attributes["ctype"].Value);
								Assert.AreEqual(1, n3.ChildNodes.Count);
								Assert.AreEqual("#text", n3.FirstChild.Name);
								Assert.AreEqual("Mount Hood with its snow-covered top", n3.InnerText);
							}
							Assert.AreEqual(1, count3);
							break;
						}
					}
					Assert.AreEqual(3, count2);
				}
				Assert.AreEqual(1, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestImgWithAlt()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
<body>
<p>My picture,
<img src=""mthood.jpg"" alt=""This is a shot of Mount Hood"" />
and there you have it.</p>
</body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestImgWithAlt did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(body);
			int count0 = 0;
			foreach (XmlNode n0 in body.ChildNodes)
			{
				++count0;
				Assert.AreEqual("trans-unit", n0.Name);
				Assert.AreEqual(2, n0.Attributes.Count);
				Assert.AreEqual("genid-1", n0.Attributes["id"].Value);
				Assert.AreEqual("x-html-p", n0.Attributes["restype"].Value);
				Assert.AreEqual(1, n0.ChildNodes.Count);
				Assert.AreEqual("source", n0.FirstChild.Name);
				Assert.AreEqual(1, n0.FirstChild.Attributes.Count);
				Assert.AreEqual("en", n0.FirstChild.Attributes["xml:lang"].Value);
				int count1 = 0;
				foreach (XmlNode n1 in n0.FirstChild.ChildNodes)
				{
					++count1;
					switch (count1)
					{
					case 1:
						Assert.AreEqual("#text", n1.Name);
						Assert.AreEqual(@"My picture,
", n1.InnerText);
						break;
					case 2:
						Assert.AreEqual("ph", n1.Name);
						Assert.AreEqual(3, n1.Attributes.Count);
						Assert.AreEqual("genid-2", n1.Attributes["id"].Value);
						Assert.AreEqual("image", n1.Attributes["ctype"].Value);
						Assert.AreEqual("mthood.jpg", n1.Attributes["src", HtmlToXliffConverter.kHtmlNamespace].Value);
						// quick check
						Assert.AreEqual("This is a shot of Mount Hood", n1.InnerText);
						int count2 = 0;
						foreach (XmlNode n2 in n1.ChildNodes)
						{
							++count2;
							Assert.AreEqual("sub", n2.Name);
							Assert.AreEqual(1, n2.Attributes.Count);
							Assert.AreEqual("x-html-img-alt", n2.Attributes["ctype"].Value);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual("This is a shot of Mount Hood", n2.FirstChild.InnerText);
						}
						Assert.AreEqual(1, count2);
						break;
					case 3:
						Assert.AreEqual("#text", n1.Name);
						Assert.AreEqual(@"
and there you have it.", n1.InnerText);
						break;
					}
				}
				Assert.AreEqual(3, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestBrInText()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
<body>
<p>First line<br>second line</p>
</body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestBrInText did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(body);
			int count0 = 0;
			foreach (XmlNode n0 in body.ChildNodes)
			{
				++count0;
				Assert.AreEqual("trans-unit", n0.Name);
				Assert.AreEqual(2, n0.Attributes.Count);
				Assert.AreEqual("genid-1", n0.Attributes["id"].Value);
				Assert.AreEqual("x-html-p", n0.Attributes["restype"].Value);
				Assert.AreEqual(1, n0.ChildNodes.Count);
				Assert.AreEqual("source", n0.FirstChild.Name);
				Assert.AreEqual(1, n0.FirstChild.Attributes.Count);
				Assert.AreEqual("en", n0.FirstChild.Attributes["xml:lang"].Value);
				int count1 = 0;
				foreach (XmlNode n1 in n0.FirstChild.ChildNodes)
				{
					++count1;
					switch (count1)
					{
					case 1:
						Assert.AreEqual("#text", n1.Name);
						Assert.AreEqual("First line", n1.InnerText);
						break;
					case 2:
						Assert.AreEqual("x", n1.Name);
						Assert.AreEqual(3, n1.Attributes.Count);
						Assert.AreEqual("genid-2", n1.Attributes["id"].Value);
						Assert.AreEqual("lb", n1.Attributes["ctype"].Value);
						Assert.AreEqual("\n", n1.Attributes["equiv-text"].Value);
						Assert.AreEqual(0, n1.ChildNodes.Count);
						break;
					case 3:
						Assert.AreEqual("#text", n1.Name);
						Assert.AreEqual("second line", n1.InnerText);
						break;
					}
				}
				Assert.AreEqual(3, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestSpanWithLang()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
<body>
<p>She added that ""<span lang='fr'>je ne sais quoi</span>"" that made her casserole absolutely delicious.</p>
</body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestSpanWithLang did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(body);
			int count0 = 0;
			foreach (XmlNode n0 in body.ChildNodes)
			{
				++count0;
				Assert.AreEqual("trans-unit", n0.Name);
				Assert.AreEqual(2, n0.Attributes.Count);
				Assert.AreEqual("genid-1", n0.Attributes["id"].Value);
				Assert.AreEqual("x-html-p", n0.Attributes["restype"].Value);
				Assert.AreEqual(1, n0.ChildNodes.Count);
				Assert.AreEqual("source", n0.FirstChild.Name);
				Assert.AreEqual(1, n0.FirstChild.Attributes.Count);
				Assert.AreEqual("en", n0.FirstChild.Attributes["xml:lang"].Value);
				int count1 = 0;
				foreach (XmlNode n1 in n0.FirstChild.ChildNodes)
				{
					++count1;
					switch (count1)
					{
					case 1:
						Assert.AreEqual("#text", n1.Name);
						Assert.AreEqual("She added that \"", n1.InnerText);
						break;
					case 2:
						Assert.AreEqual("g", n1.Name);
						Assert.AreEqual(3, n1.Attributes.Count);
						Assert.AreEqual("genid-2", n1.Attributes["id"].Value);
						Assert.AreEqual("x-html-span", n1.Attributes["ctype"].Value);
						Assert.AreEqual("fr", n1.Attributes["xml:lang"].Value);
						Assert.AreEqual(1, n1.ChildNodes.Count);
						Assert.AreEqual("#text", n1.FirstChild.Name);
						Assert.AreEqual("je ne sais quoi", n1.FirstChild.InnerText);
						break;
					case 3:
						Assert.AreEqual("#text", n1.Name);
						Assert.AreEqual("\" that made her casserole absolutely delicious.", n1.InnerText);
						break;
					}
				}
				Assert.AreEqual(3, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestBrBetweenParas()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
<body>
<p>This is a test.</p>
<br/>
<p>This is only a test.</p>
</body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestBrBetweenParas did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(body);
			int count0 = 0;
			foreach (XmlNode n0 in body.ChildNodes)
			{
				++count0;
				switch (count0)
				{
				case 1:
					Assert.AreEqual("trans-unit", n0.Name);
					Assert.AreEqual(2, n0.Attributes.Count);
					Assert.AreEqual("genid-1", n0.Attributes["id"].Value);
					Assert.AreEqual("x-html-p", n0.Attributes["restype"].Value);
					Assert.AreEqual(1, n0.ChildNodes.Count);
					Assert.AreEqual("source", n0.FirstChild.Name);
					Assert.AreEqual(1, n0.FirstChild.Attributes.Count);
					Assert.AreEqual("en", n0.FirstChild.Attributes["xml:lang"].Value);
					Assert.AreEqual(1, n0.FirstChild.ChildNodes.Count);
					Assert.AreEqual("#text", n0.FirstChild.FirstChild.Name);
					Assert.AreEqual("This is a test.", n0.FirstChild.FirstChild.InnerText);
					break;
				case 2:
					Assert.AreEqual("group", n0.Name);
					Assert.AreEqual(1, n0.Attributes.Count);
					Assert.AreEqual("x-html-br", n0.Attributes["restype"].Value);
					Assert.AreEqual("", n0.InnerXml);
					break;
				case 3:
					Assert.AreEqual("trans-unit", n0.Name);
					Assert.AreEqual(2, n0.Attributes.Count);
					Assert.AreEqual("genid-2", n0.Attributes["id"].Value);
					Assert.AreEqual("x-html-p", n0.Attributes["restype"].Value);
					Assert.AreEqual(1, n0.ChildNodes.Count);
					Assert.AreEqual("source", n0.FirstChild.Name);
					Assert.AreEqual(1, n0.FirstChild.Attributes.Count);
					Assert.AreEqual("en", n0.FirstChild.Attributes["xml:lang"].Value);
					Assert.AreEqual(1, n0.FirstChild.ChildNodes.Count);
					Assert.AreEqual("#text", n0.FirstChild.FirstChild.Name);
					Assert.AreEqual("This is only a test.", n0.FirstChild.FirstChild.InnerText);
					break;
				}
			}
			Assert.AreEqual(3, count0);
		}

		[Test]
		public void TestHtmlFragment()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"
<div class=""bloom-ui bloomDialogContainer"" id=""text-properties-dialog"" style=""visibility: hidden;"">
  <div class=""bloomDialogTitleBar"" data-i18n=""EditTab.TextBoxProperties.Title"">Text Box Properties</div>
  <div class=""hideWhenFormattingEnabled bloomDialogMainPage"">
    <p data-i18n=""BookEditor.FormattingDisabled"">Sorry, Reader Templates do not allow changes to formatting.</p>
  </div>
</div>
");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestHtmlFragment did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body");
			Assert.IsNotNull(body);
			Assert.AreEqual(1, body.ChildNodes.Count);
			Assert.AreEqual("group", body.FirstChild.Name);
			Assert.AreEqual(4, body.FirstChild.Attributes.Count);
			Assert.AreEqual("x-html-div", body.FirstChild.Attributes["restype"].Value);
			Assert.AreEqual("bloom-ui bloomDialogContainer", body.FirstChild.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
			Assert.AreEqual("text-properties-dialog", body.FirstChild.Attributes["id", HtmlToXliffConverter.kHtmlNamespace].Value);
			Assert.AreEqual("visibility: hidden;", body.FirstChild.Attributes["style", HtmlToXliffConverter.kHtmlNamespace].Value);
			int count0 = 0;
			foreach (XmlNode n0 in body.FirstChild.ChildNodes)
			{
				++count0;
				switch (count0)
				{
				case 1:
					Assert.AreEqual("trans-unit", n0.Name);
					Assert.AreEqual(4, n0.Attributes.Count);
					Assert.AreEqual("EditTab.TextBoxProperties.Title", n0.Attributes["id"].Value);
					Assert.AreEqual("x-html-div", n0.Attributes["restype"].Value);
					Assert.AreEqual("bloomDialogTitleBar", n0.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
					Assert.AreEqual("EditTab.TextBoxProperties.Title", n0.Attributes["data-i18n", HtmlToXliffConverter.kHtmlNamespace].Value);
					Assert.AreEqual(1, n0.ChildNodes.Count);
					Assert.AreEqual("source", n0.FirstChild.Name);
					Assert.AreEqual(1, n0.FirstChild.Attributes.Count);
					Assert.AreEqual("en", n0.FirstChild.Attributes["xml:lang"].Value);
					Assert.AreEqual("Text Box Properties", n0.FirstChild.InnerXml);
					break;
				case 2:
					Assert.AreEqual("group", n0.Name);
					Assert.AreEqual(2, n0.Attributes.Count);
					Assert.AreEqual("x-html-div", n0.Attributes["restype"].Value);
					Assert.AreEqual("hideWhenFormattingEnabled bloomDialogMainPage", n0.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
					Assert.AreEqual(1, n0.ChildNodes.Count);
					var n1 = n0.FirstChild;
					Assert.AreEqual("trans-unit", n1.Name);
					Assert.AreEqual(3, n1.Attributes.Count);
					Assert.AreEqual("BookEditor.FormattingDisabled", n1.Attributes["id"].Value);
					Assert.AreEqual("x-html-p", n1.Attributes["restype"].Value);
					Assert.AreEqual("BookEditor.FormattingDisabled", n1.Attributes["data-i18n", HtmlToXliffConverter.kHtmlNamespace].Value);
					Assert.AreEqual(1, n1.ChildNodes.Count);
					Assert.AreEqual("source", n1.FirstChild.Name);
					Assert.AreEqual(1, n1.FirstChild.Attributes.Count);
					Assert.AreEqual("en", n1.FirstChild.Attributes["xml:lang"].Value);
					Assert.AreEqual("Sorry, Reader Templates do not allow changes to formatting.", n1.FirstChild.InnerXml);
					break;
				}
			}
			Assert.AreEqual(2, count0);
		}

		[Test]
		public void TestFormInDiv()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
 <head>
  <script src=""/bloom/bookEdit/toolbox/bookSettings/bookSettings.js""></script>
  <link rel=""stylesheet"" href=""/bloom/bookEdit/toolbox/bookSettings/bookSettings.css""/>
 </head>
 <body>
  <h3 data-panelId=""bookSettingsTool"" data-order=""100""><img src=""/bloom/bookEdit/toolbox/bookSettings/icon.svg""/></h3>
  <div data-panelId=""bookSettingsTool"">
   <form id=""bookSettings"">
    <div class=""showOnlyWhenBookWouldNormallyBeLocked"">
     <p data-i18n=""EditTab.Toolbox.Settings.UnlockShellBookIntroductionText"">Bloom normally prevents most changes to shellbooks. If you need to add pages, change images, etc., tick the box below.</p>
     <input type=""checkbox"" name=""unlockShellBook"" onClick=""FrameExports.handleBookSettingCheckboxClick(this);""/>
     <label data-i18n=""EditTab.Toolbox.Settings.Unlock"">Allow changes to this shellbook</label>
    </div>
   </form>
  </div>
 </body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestFormInDiv did not validate against schema: {0}");

			var body = xmlDoc.SelectSingleNode("/xliff/file/body");
			Assert.IsNotNull(body);
			Assert.AreEqual(0, body.Attributes.Count);
			Assert.AreEqual(1, body.ChildNodes.Count);
			Assert.AreEqual(1, body.FirstChild.Attributes.Count);
			Assert.AreEqual("x-html-html", body.FirstChild.Attributes["restype"].Value);
			Assert.AreEqual(2, body.FirstChild.ChildNodes.Count);
			int count0 = 0;
			foreach (XmlNode n0 in body.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("group", n0.Name);
				Assert.AreEqual(1, n0.Attributes.Count);
				Assert.AreEqual(2, n0.ChildNodes.Count);
				int count1 = 0;
				switch (count0)
				{
				case 1:
					Assert.AreEqual("header", n0.Attributes["restype"].Value);
					foreach (XmlNode n1 in n0.ChildNodes)
					{
						++count1;
						Assert.AreEqual("group", n1.Name);
						Assert.AreEqual(0, n1.ChildNodes.Count);
						switch (count1)
						{
						case 1:
							Assert.AreEqual(2, n1.Attributes.Count);
							Assert.AreEqual("x-html-script", n1.Attributes["restype"].Value);
							Assert.AreEqual("/bloom/bookEdit/toolbox/bookSettings/bookSettings.js", n1.Attributes["src", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual(0, n1.ChildNodes.Count);
							break;
						case 2:
							Assert.AreEqual(3, n1.Attributes.Count);
							Assert.AreEqual("x-html-link", n1.Attributes["restype"].Value);
							Assert.AreEqual("stylesheet", n1.Attributes["rel"].Value);
							Assert.AreEqual("/bloom/bookEdit/toolbox/bookSettings/bookSettings.css", n1.Attributes["href", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual(0, n1.ChildNodes.Count);
							break;
						}
					}
					break;
				case 2:
					Assert.AreEqual("x-html-body", n0.Attributes["restype"].Value);
					foreach (XmlNode n1 in n0.ChildNodes)
					{
						++count1;
						switch (count1)
						{
						case 1:
							Assert.AreEqual("trans-unit", n1.Name);
							Assert.AreEqual(4, n1.Attributes.Count);
							Assert.AreEqual("genid-1", n1.Attributes["id"].Value);
							Assert.AreEqual("x-html-h3", n1.Attributes["restype"].Value);
							Assert.AreEqual("bookSettingsTool", n1.Attributes["data-panelid", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual("100", n1.Attributes["data-order", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual(1, n1.ChildNodes.Count);
							Assert.AreEqual("source", n1.FirstChild.Name);
							Assert.AreEqual(1, n1.FirstChild.Attributes.Count);
							Assert.AreEqual("en", n1.FirstChild.Attributes["xml:lang"].Value);
							Assert.AreEqual(1, n1.FirstChild.ChildNodes.Count);
							Assert.AreEqual("x", n1.FirstChild.FirstChild.Name);
							Assert.AreEqual(0, n1.FirstChild.FirstChild.ChildNodes.Count);
							Assert.AreEqual(3, n1.FirstChild.FirstChild.Attributes.Count);
							Assert.AreEqual("genid-2", n1.FirstChild.FirstChild.Attributes["id"].Value);
							Assert.AreEqual("image", n1.FirstChild.FirstChild.Attributes["ctype"].Value);
							Assert.AreEqual("/bloom/bookEdit/toolbox/bookSettings/icon.svg", n1.FirstChild.FirstChild.Attributes["src", HtmlToXliffConverter.kHtmlNamespace].Value);
							break;
						case 2:
							Assert.AreEqual("group", n1.Name);
							Assert.AreEqual(2, n1.Attributes.Count);
							Assert.AreEqual("x-html-div", n1.Attributes["restype"].Value);
							Assert.AreEqual("bookSettingsTool", n1.Attributes["data-panelid", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual(1, n1.ChildNodes.Count);
							Assert.AreEqual("group", n1.FirstChild.Name);
							Assert.AreEqual(2, n1.FirstChild.Attributes.Count);
							Assert.AreEqual("dialog", n1.FirstChild.Attributes["restype"].Value);
							Assert.AreEqual("bookSettings", n1.FirstChild.Attributes["id", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual(1, n1.FirstChild.ChildNodes.Count);
							Assert.AreEqual("group", n1.FirstChild.FirstChild.Name);
							Assert.AreEqual(2, n1.FirstChild.FirstChild.Attributes.Count);
							Assert.AreEqual("x-html-div", n1.FirstChild.FirstChild.Attributes["restype"].Value);
							Assert.AreEqual("showOnlyWhenBookWouldNormallyBeLocked", n1.FirstChild.FirstChild.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
							int count2 = 0;
							foreach (XmlNode n2 in n1.FirstChild.FirstChild.ChildNodes)
							{
								++count2;
								switch (count2)
								{
								case 1:
									Assert.AreEqual("trans-unit", n2.Name);
									Assert.AreEqual(3, n2.Attributes.Count);
									Assert.AreEqual("EditTab.Toolbox.Settings.UnlockShellBookIntroductionText", n2.Attributes["id"].Value);
									Assert.AreEqual("x-html-p", n2.Attributes["restype"].Value);
									Assert.AreEqual("EditTab.Toolbox.Settings.UnlockShellBookIntroductionText", n2.Attributes["data-i18n", HtmlToXliffConverter.kHtmlNamespace].Value);
									Assert.AreEqual(1, n2.ChildNodes.Count);
									Assert.AreEqual("source", n2.FirstChild.Name);
									Assert.AreEqual(1, n2.FirstChild.Attributes.Count);
									Assert.AreEqual("en", n2.FirstChild.Attributes["xml:lang"].Value);
									Assert.AreEqual(1, n2.FirstChild.ChildNodes.Count);
									Assert.AreEqual(XmlNodeType.Text, n2.FirstChild.FirstChild.NodeType);
									Assert.AreEqual("Bloom normally prevents most changes to shellbooks. If you need to add pages, change images, etc., tick the box below.", n2.FirstChild.FirstChild.InnerText);
									break;
								case 2:
									Assert.AreEqual("group", n2.Name);
									Assert.AreEqual(4, n2.Attributes.Count);
									Assert.AreEqual("x-html-input", n2.Attributes["restype"].Value);
									Assert.AreEqual("checkbox", n2.Attributes["type", HtmlToXliffConverter.kHtmlNamespace].Value);
									Assert.AreEqual("unlockShellBook", n2.Attributes["name", HtmlToXliffConverter.kHtmlNamespace].Value);
									Assert.AreEqual("FrameExports.handleBookSettingCheckboxClick(this);", n2.Attributes["onclick", HtmlToXliffConverter.kHtmlNamespace].Value);
									Assert.AreEqual(0, n2.ChildNodes.Count);
									break;
								case 3:
									Assert.AreEqual("trans-unit", n2.Name);
									Assert.AreEqual(3, n2.Attributes.Count);
									Assert.AreEqual("EditTab.Toolbox.Settings.Unlock", n2.Attributes["id"].Value);
									Assert.AreEqual("x-html-label", n2.Attributes["restype"].Value);
									Assert.AreEqual("EditTab.Toolbox.Settings.Unlock", n2.Attributes["data-i18n", HtmlToXliffConverter.kHtmlNamespace].Value);
									Assert.AreEqual(1, n2.ChildNodes.Count);
									Assert.AreEqual("source", n2.FirstChild.Name);
									Assert.AreEqual(1, n2.FirstChild.Attributes.Count);
									Assert.AreEqual("en", n2.FirstChild.Attributes["xml:lang"].Value);
									Assert.AreEqual(1, n2.FirstChild.ChildNodes.Count);
									Assert.AreEqual(XmlNodeType.Text, n2.FirstChild.FirstChild.NodeType);
									Assert.AreEqual("Allow changes to this shellbook", n2.FirstChild.FirstChild.InnerText);
									break;
								}
							}
							Assert.AreEqual(3, count2);
							break;
						}
					}
					break;
				}
				Assert.AreEqual(2, count1);
			}
			Assert.AreEqual(2, count0);
		}

		[Test]
		public void TestDuplicateI18nStrings()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<html>
 <body>
  <table class=""statistics clear"" style=""margin-left: 6px"">
   <tr>
    <td class=""tableTitle thisPageSection"" data-i18n=""EditTab.Toolbox.LeveledReaderTool.ThisPage"">This Page</td>
   </tr>
   <tr>
    <td class=""statistics-max"" data-i18n=""EditTab.Toolbox.LeveledReaderTool.Max"">Maximum</td>
   </tr>
  </table>
  <table class=""statistics clear"" style=""margin-left: 6px"">
   <tr>
    <td class=""tableTitle"" data-i18n=""EditTab.Toolbox.LeveledReaderTool.ThisBook"">This Book</td>
   </tr>
   <tr>
    <td class=""statistics-max"" data-i18n=""EditTab.Toolbox.LeveledReaderTool.Max"">Maximum</td>
   </tr>
  </table>
 </body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestDuplicateI18nStrings did not validate against schema: {0}");
			var hbody = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(hbody);
			Assert.AreEqual(2, hbody.ChildNodes.Count);
			int count0 = 0;
			foreach (XmlNode n0 in hbody.ChildNodes)
			{
				++count0;
				Assert.AreEqual("group", n0.Name);
				Assert.AreEqual(3, n0.Attributes.Count);
				Assert.AreEqual("table", n0.Attributes["restype"].Value);
				Assert.AreEqual("statistics clear", n0.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
				Assert.AreEqual("margin-left: 6px", n0.Attributes["style", HtmlToXliffConverter.kHtmlNamespace].Value);
				Assert.AreEqual(2, n0.ChildNodes.Count);
				int count1 = 0;
				foreach (XmlNode n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("group", n1.Name);
					Assert.AreEqual(1, n1.Attributes.Count);
					Assert.AreEqual("row", n1.Attributes["restype"].Value);
					Assert.AreEqual(1, n1.ChildNodes.Count);
					Assert.AreEqual("trans-unit", n1.FirstChild.Name);
					Assert.AreEqual(4, n1.FirstChild.Attributes.Count);
					Assert.AreEqual("cell", n1.FirstChild.Attributes["restype"].Value);
					Assert.AreEqual(1, n1.FirstChild.ChildNodes.Count);
					Assert.AreEqual("source", n1.FirstChild.FirstChild.Name);
					Assert.AreEqual(1, n1.FirstChild.FirstChild.Attributes.Count);
					Assert.AreEqual("en", n1.FirstChild.FirstChild.Attributes["xml:lang"].Value);
					switch (count0)
					{
					case 1:
						switch (count1)
						{
						case 1:
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.ThisPage", n1.FirstChild.Attributes["id"].Value);
							Assert.AreEqual("tableTitle thisPageSection", n1.FirstChild.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.ThisPage", n1.FirstChild.Attributes["data-i18n", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual(1, n1.FirstChild.FirstChild.ChildNodes.Count);
							Assert.AreEqual(XmlNodeType.Text, n1.FirstChild.FirstChild.FirstChild.NodeType);
							Assert.AreEqual("This Page", n1.FirstChild.FirstChild.InnerXml);
							break;
						case 2:
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.Max", n1.FirstChild.Attributes["id"].Value);
							Assert.AreEqual("statistics-max", n1.FirstChild.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.Max", n1.FirstChild.Attributes["data-i18n", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual(1, n1.FirstChild.FirstChild.ChildNodes.Count);
							Assert.AreEqual(XmlNodeType.Text, n1.FirstChild.FirstChild.FirstChild.NodeType);
							Assert.AreEqual("Maximum", n1.FirstChild.FirstChild.InnerXml);
							break;
						}
						break;
					case 2:
						switch (count1)
						{
						case 1:
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.ThisBook", n1.FirstChild.Attributes["id"].Value);
							Assert.AreEqual("tableTitle", n1.FirstChild.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.ThisBook", n1.FirstChild.Attributes["data-i18n", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual(1, n1.FirstChild.FirstChild.ChildNodes.Count);
							Assert.AreEqual(XmlNodeType.Text, n1.FirstChild.FirstChild.FirstChild.NodeType);
							Assert.AreEqual("This Book", n1.FirstChild.FirstChild.InnerXml);
							break;
						case 2:
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.Max-1", n1.FirstChild.Attributes["id"].Value);
							Assert.AreEqual("statistics-max", n1.FirstChild.Attributes["class", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.Max", n1.FirstChild.Attributes["data-i18n", HtmlToXliffConverter.kHtmlNamespace].Value);
							Assert.AreEqual(1, n1.FirstChild.FirstChild.ChildNodes.Count);
							Assert.AreEqual(XmlNodeType.Text, n1.FirstChild.FirstChild.FirstChild.NodeType);
							Assert.AreEqual("Maximum", n1.FirstChild.FirstChild.InnerXml);
							break;
						}
						break;
					}
				}
			}
		}

		[Test]
		public void TestBareAmpersand()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(@"<!DOCTYPE html>
<html>
 <head>
  <meta charset=""UTF-8"">
  <title>Null Front & Back Matter for books where you don't want any</title>
 </head>
 <body>
 </body>
</html>");
			var converter = new HtmlToXliffConverter(htmlDoc, "sample.htm");
			var xmlDoc = converter.Convert();
			Assert.IsNotNull(xmlDoc);
			ValidateXliffOutput(xmlDoc.OuterXml, "Xliff for TestBareAmpersand did not validate against schema: {0}");

			var header = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='header']");
			Assert.IsNotNull(header);
			Assert.AreEqual(1, header.Attributes.Count);
			Assert.AreEqual(2, header.ChildNodes.Count);
			int count0 = 0;
			foreach (XmlNode n0 in header.ChildNodes)
			{
				++count0;
				switch (count0)
				{
				case 1:
					Assert.AreEqual("group", n0.Name);
					Assert.AreEqual(2, n0.Attributes.Count);
					Assert.AreEqual("x-html-meta", n0.Attributes["restype"].Value);
					Assert.AreEqual("UTF-8", n0.Attributes["charset", HtmlToXliffConverter.kHtmlNamespace].Value);
					Assert.AreEqual("", n0.InnerXml);
					break;
				case 2:
					Assert.AreEqual("trans-unit", n0.Name);
					Assert.AreEqual(2, n0.Attributes.Count);
					Assert.AreEqual("genid-1", n0.Attributes["id"].Value);
					Assert.AreEqual("x-html-title", n0.Attributes["restype"].Value);
					Assert.AreEqual(1, n0.ChildNodes.Count);
					Assert.AreEqual("source", n0.FirstChild.Name);
					Assert.AreEqual(1, n0.FirstChild.Attributes.Count);
					Assert.AreEqual("en", n0.FirstChild.Attributes["xml:lang"].Value);
					Assert.AreEqual(1, n0.FirstChild.ChildNodes.Count);
					Assert.AreEqual(XmlNodeType.Text, n0.FirstChild.FirstChild.NodeType);
					Assert.AreEqual("Null Front &amp; Back Matter for books where you don't want any", n0.FirstChild.FirstChild.OuterXml);
					break;
				}
			}
			var body = xmlDoc.SelectSingleNode("/xliff/file/body/group[@restype='x-html-html']/group[@restype='x-html-body']");
			Assert.IsNotNull(body);
			Assert.AreEqual(0, body.ChildNodes.Count);
		}

		/// <summary>
		/// Validate the xliff output.  Calling xmlDoc.Validate() in the test never caught
		/// a validation error even after assigning a schema for xliff.  Loading the XML into
		/// a new document with a schema assigned a priori does catch invalid xliff, and
		/// this approach presumably sets up Validate to catch validation warnings that don't
		/// cause an exception during loading.
		/// </summary>
		private void ValidateXliffOutput(string xliff, string errorMessageFormat)
		{
			try
			{
				var settings = new XmlReaderSettings();
				settings.Schemas.Add(null, "../../src/L10NSharpTests/TestXliff/xliff-core-1.2-transitional.xsd");
				settings.ValidationType = ValidationType.Schema;
				var reader = XmlReader.Create(new StringReader(xliff), settings);
				var document = new XmlDocument();
				document.Load(reader);		// throws here if invalid.  Validate() catches warnings as well as errors.
				document.Validate((sender, e) => { Assert.Fail(errorMessageFormat, e.Message); });
			}
			catch (System.Xml.Schema.XmlSchemaValidationException e)
			{
				Assert.Fail(errorMessageFormat, e.Message);
			}
		}
	}
}

