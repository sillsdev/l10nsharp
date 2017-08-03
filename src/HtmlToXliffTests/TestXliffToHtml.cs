using System;
using System.Xml;
using HtmlAgilityPack;
using XliffToHtml;
using NUnit.Framework;

namespace HtmlToXliffTests
{
	/// <summary>
	/// Test the conversions from XLIFF back to HTML.  The examples, unless otherwise noted, are adapted from
	/// http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html.
	/// </summary>
	/// <remarks>
	/// I know some of the test code might possibly be simplified by methods, but it's easier to track
	/// explicit test failures with straight line code.
	/// The translations in these tests are courtesy of Google Translate.  I figure that's good enough for
	/// testing that would otherwise just be using total nonsense for the translations.  We at least see
	/// some non-English (and non-Roman) characters this way.
	/// </remarks>
	[TestFixture]
	public class TestXliffToHtml
	{
		[Test]
		public void TestBrBetweenParas_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-p"">
      <source xml:lang=""en"">This is a test.</source>
     </trans-unit>
     <group restype=""x-html-br"" />
     <trans-unit id=""genid-2"" restype=""x-html-p"">
      <source xml:lang=""en"">This is only a test.</source>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual("#document", htmlDoc.DocumentNode.Name);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			// we pick up the source language if no target language exists in the file.
			Assert.AreEqual("en", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("en", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			Assert.AreEqual(1, htmlDoc.DocumentNode.FirstChild.ChildNodes.Count);
			Assert.AreEqual("body", htmlDoc.DocumentNode.FirstChild.FirstChild.Name);
			Assert.AreEqual(0, htmlDoc.DocumentNode.FirstChild.FirstChild.Attributes.Count);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.FirstChild.ChildNodes)
			{
				++count0;
				switch (count0)
				{
				case 1:
					Assert.AreEqual("p", n0.Name);
					Assert.AreEqual(0, n0.Attributes.Count);
					Assert.AreEqual(1, n0.ChildNodes.Count);
					Assert.AreEqual("#text", n0.FirstChild.Name);
					Assert.AreEqual("This is a test.", n0.FirstChild.InnerText);
					break;
				case 2:
					Assert.AreEqual("br", n0.Name);
					Assert.AreEqual(0, n0.Attributes.Count);
					Assert.AreEqual(0, n0.ChildNodes.Count);
					break;
				case 3:
					Assert.AreEqual("p", n0.Name);
					Assert.AreEqual(0, n0.Attributes.Count);
					Assert.AreEqual(1, n0.ChildNodes.Count);
					Assert.AreEqual("#text", n0.FirstChild.Name);
					Assert.AreEqual("This is only a test.", n0.FirstChild.InnerText);
					break;
				}
			}
			Assert.AreEqual(3, count0);
		}

		[Test]
		public void TestBrInText_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""es"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-p"">
      <source xml:lang=""en"">First line<x id=""genid-2"" ctype=""lb"" equiv-text=""&#xA;"" />second line</source>
      <target xml:lang=""es"">Primera linea<x id=""genid-2"" ctype=""lb"" equiv-text=""&#xA;"" />segunda linea</target>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual("#document", htmlDoc.DocumentNode.Name);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			Assert.AreEqual("es", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("es", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("body", n0.Name);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("p", n1.Name);
					Assert.AreEqual(0, n1.Attributes.Count);
					int count2 = 0;
					foreach (var n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("Primera linea", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("br", n2.Name);
							Assert.AreEqual(0, n2.Attributes.Count);
							Assert.AreEqual(0, n2.ChildNodes.Count);
							break;
						case 3:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("segunda linea", n2.InnerText);
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
		public void TestClassAttribute_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""de"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-h2"" html:class=""article-title"">
      <source xml:lang=""en"">Life and Habitat of the Marmot</source>
      <target xml:lang=""de"">Leben und Lebensraum des Murmeltieres</target>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			Assert.AreEqual("de", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("de", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("body", n0.Name);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("h2", n1.Name);
					Assert.AreEqual(1, n1.Attributes.Count);
					Assert.AreEqual("article-title", n1.Attributes["class"].Value);
					Assert.AreEqual(1, n1.ChildNodes.Count);
					Assert.AreEqual("#text", n1.FirstChild.Name);
					Assert.AreEqual("Leben und Lebensraum des Murmeltieres", n1.FirstChild.InnerText);
				}
				Assert.AreEqual(1, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestImg_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-p"">
      <source xml:lang=""en"">This is Mount Hood: <x id=""genid-2"" ctype=""image"" html:src=""mthood.jpg"" /></source>
      <target xml:lang=""fr"">C'est Mount Hood: <x id=""genid-2"" ctype=""image"" html:src=""mthood.jpg"" /></target>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			Assert.AreEqual("fr", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("fr", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("body", n0.Name);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("p", n1.Name);
					Assert.AreEqual(0, n1.Attributes.Count);
					int count2 = 0;
					foreach (var n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("C'est Mount Hood: ", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("img", n2.Name);
							Assert.AreEqual(1, n2.Attributes.Count);
							Assert.AreEqual("mthood.jpg", n2.Attributes["src"].Value);
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
		public void TestImgWithAlt_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""ko"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-p"">
      <source xml:lang=""en"">My picture,
<ph id=""genid-2"" ctype=""image"" html:src=""mthood.jpg""><sub ctype=""x-html-img-alt"">This is a shot of Mount Hood</sub></ph>
and there you have it.</source>
      <target xml:lang=""ko"">내 그림,
<ph id=""genid-2"" ctype=""image"" html:src=""mthood.jpg""><sub ctype=""x-html-img-alt"">이것은 마운트 후드의 총입니다</sub></ph>
거기에 당신이 그것을 가지고 있습니다.</target>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			Assert.AreEqual("ko", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("ko", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("body", n0.Name);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("p", n1.Name);
					Assert.AreEqual(0, n1.Attributes.Count);
					int count2 = 0;
					foreach (var n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(@"내 그림,
", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("img", n2.Name);
							Assert.AreEqual(2, n2.Attributes.Count);
							Assert.AreEqual("mthood.jpg", n2.Attributes["src"].Value);
							Assert.AreEqual("이것은 마운트 후드의 총입니다", n2.Attributes["alt"].Value);
							break;
						case 3:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(@"
거기에 당신이 그것을 가지고 있습니다.", n2.InnerText);
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
		public void TestImgWithTitleAndAlt_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""pt"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-p"">
      <source xml:lang=""en""><ph id=""genid-2""><sub ctype=""x-html-p-title"">Information about Mount Hood</sub></ph>This is Mount Hood: <ph id=""genid-3"" ctype=""image"" html:src=""mthood.jpg""><sub ctype=""x-html-img-alt"">Mount Hood with its snow-covered top</sub></ph></source>
      <target xml:lang=""pt""><ph id=""genid-2""><sub ctype=""x-html-p-title"">Informações sobre Mount Hood</sub></ph>Este é Mount Hood: <ph id=""genid-3"" ctype=""image"" html:src=""mthood.jpg""><sub ctype=""x-html-img-alt"">Mount Hood com o seu topo coberto de neve</sub></ph></target>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			Assert.AreEqual("pt", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("pt", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("body", n0.Name);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("p", n1.Name);
					Assert.AreEqual(1, n1.Attributes.Count);
					Assert.AreEqual("Informações sobre Mount Hood", n1.Attributes["title"].Value);
					int count2 = 0;
					foreach (var n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("Este é Mount Hood: ", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("img", n2.Name);
							Assert.AreEqual(2, n2.Attributes.Count);
							Assert.AreEqual("mthood.jpg", n2.Attributes["src"].Value);
							Assert.AreEqual("Mount Hood com o seu topo coberto de neve", n2.Attributes["alt"].Value);
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
		public void TestInlineElements_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""af"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-p"">
      <source xml:lang=""en"">In Portland, Oregon one may <g id=""genid-2"" ctype=""italic"">ski</g> on the mountain, <g id=""genid-3"" ctype=""bold"">wind surf</g> in the gorge, and <g id=""genid-4"" ctype=""italic"">surf</g> in the ocean, all on the same day.</source>
      <target xml:lang=""af"">In Portland, Oregon kan mens op die berg <g id=""genid-2"" ctype=""italic"">skiet</g>, <g id=""genid-3"" ctype=""bold"">windswaai</g> in die kloof, en op dieselfde dag in die see <g id=""genid-4"" ctype=""italic"">rondbeweeg</g>.</target>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual("#document", htmlDoc.DocumentNode.Name);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			Assert.AreEqual("af", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("af", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("body", n0.Name);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("p", n1.Name);
					Assert.AreEqual(0, n1.Attributes.Count);
					int count2 = 0;
					foreach (var n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("In Portland, Oregon kan mens op die berg ", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("i", n2.Name);
							Assert.AreEqual(0, n2.Attributes.Count);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual("skiet", n2.FirstChild.InnerText);
							break;
						case 3:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(", ", n2.InnerText);
							break;
						case 4:
							Assert.AreEqual("b", n2.Name);
							Assert.AreEqual(0, n2.Attributes.Count);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual("windswaai", n2.FirstChild.InnerText);
							break;
						case 5:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(" in die kloof, en op dieselfde dag in die see ", n2.InnerText);
							break;
						case 6:
							Assert.AreEqual("i", n2.Name);
							Assert.AreEqual(0, n2.Attributes.Count);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual("rondbeweeg", n2.FirstChild.InnerText);
							break;
						case 7:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(".", n2.InnerText);
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
		public void TestInlineElementWithLang_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""it"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-p"">
      <source xml:lang=""en"">The words <g id=""genid-2"" ctype=""x-html-q"" xml:lang=""fr"">Je me souviens</g> are the motto of Québec.</source>
      <target xml:lang=""it"">Le parole <g id=""genid-2"" ctype=""x-html-q"" xml:lang=""fr"">Je me souviens</g> sono il motto di Québec.</target>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual("#document", htmlDoc.DocumentNode.Name);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			Assert.AreEqual("it", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("it", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("body", n0.Name);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("p", n1.Name);
					Assert.AreEqual(0, n1.Attributes.Count);
					int count2 = 0;
					foreach (var n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("Le parole ", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("q", n2.Name);
							Assert.AreEqual(1, n2.Attributes.Count);
							Assert.AreEqual("fr", n2.Attributes["lang"].Value);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual("Je me souviens", n2.FirstChild.InnerText);
							break;
						case 3:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(" sono il motto di Québec.", n2.InnerText);
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
		public void TestInlineSpans_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""ru"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-p"">
      <source xml:lang=""en"">Questions will appear in <g id=""genid-2"" ctype=""x-html-span"" html:fontcolor=""#339966"">Green
face</g>, while answers will appear in <g id=""genid-3"" ctype=""x-html-span"" html:fontcolor=""#333399"">Indigo
face</g>.</source>
      <target xml:lang=""ru"">Вопросы появятся в <g id=""genid-2"" ctype=""x-html-span"" html:fontcolor=""#339966"">зеленом
Лицо</g>, в то время как ответы появятся в <g id=""genid-3"" ctype=""x-html-span"" html:fontcolor=""#333399"">индиго
лицо</g>.</target>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual("#document", htmlDoc.DocumentNode.Name);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			Assert.AreEqual("ru", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("ru", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("body", n0.Name);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("p", n1.Name);
					Assert.AreEqual(0, n1.Attributes.Count);
					int count2 = 0;
					foreach (var n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("Вопросы появятся в ", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("span", n2.Name);
							Assert.AreEqual(1, n2.Attributes.Count);
							Assert.AreEqual("#339966", n2.Attributes["fontcolor"].Value);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual(@"зеленом
Лицо", n2.FirstChild.InnerText);
							break;
						case 3:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(", в то время как ответы появятся в ", n2.InnerText);
							break;
						case 4:
							Assert.AreEqual("span", n2.Name);
							Assert.AreEqual(1, n2.Attributes.Count);
							Assert.AreEqual("#333399", n2.Attributes["fontcolor"].Value);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual(@"индиго
лицо", n2.FirstChild.InnerText);
							break;
						case 5:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual(".", n2.InnerText);
							break;
						}
					}
					Assert.AreEqual(5, count2);
				}
				Assert.AreEqual(1, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestSpanWithLang_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""se"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-p"">
      <source xml:lang=""en"">She added that ""<g id=""genid-2"" ctype=""x-html-span"" xml:lang=""fr"">je ne sais quoi</g>"" that made her casserole absolutely delicious.</source>
      <target xml:lang=""se"">Hon lade till att ""<g id=""genid-2"" ctype=""x-html-span"" xml:lang=""fr"">je ne sais quoi</g>"" som gjorde hennes gryta absolut läckra.</target>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual("#document", htmlDoc.DocumentNode.Name);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			Assert.AreEqual("se", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("se", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("body", n0.Name);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("p", n1.Name);
					Assert.AreEqual(0, n1.Attributes.Count);
					int count2 = 0;
					foreach (var n2 in n1.ChildNodes)
					{
						++count2;
						switch (count2)
						{
						case 1:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("Hon lade till att \"", n2.InnerText);
							break;
						case 2:
							Assert.AreEqual("span", n2.Name);
							Assert.AreEqual(1, n2.Attributes.Count);
							Assert.AreEqual("fr", n2.Attributes["lang"].Value);
							Assert.AreEqual(1, n2.ChildNodes.Count);
							Assert.AreEqual("#text", n2.FirstChild.Name);
							Assert.AreEqual("je ne sais quoi", n2.FirstChild.InnerText);
							break;
						case 3:
							Assert.AreEqual("#text", n2.Name);
							Assert.AreEqual("\" som gjorde hennes gryta absolut läckra.", n2.InnerText);
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
		public void TestTableRepresentation_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""gr"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-h1"" html:class=""title"">
      <source xml:lang=""en"">Report</source>
      <target xml:lang=""gr"">αναφορά</target>
     </trans-unit>
     <group restype=""table"" html:border=""1"" html:width=""100%"">
      <group restype=""row"">
       <trans-unit id=""genid-2"" restype=""cell"" html:valign=""top"">
        <source xml:lang=""en"">Κείμενο στο κελί r1-c1</source>
       </trans-unit>
       <trans-unit id=""genid-3"" restype=""cell"" html:valign=""top"">
        <source xml:lang=""en"">Κείμενο στο κελί r1-c2</source>
       </trans-unit>
      </group>
      <group restype=""row"">
       <trans-unit id=""genid-4"" restype=""cell"" html:bgcolor=""#C0C0C0"">
        <source xml:lang=""en"">Κείμενο στο κελί r2-c1</source>
       </trans-unit>
       <trans-unit id=""genid-5"" restype=""cell"">
        <source xml:lang=""en"">Κείμενο στο κελί r2-c2</source>
       </trans-unit>
      </group>
     </group>
     <trans-unit id=""genid-6"" restype=""x-html-p"">
      <source xml:lang=""en"">All rights reserved (c) Gandalf Inc.</source>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual("#document", htmlDoc.DocumentNode.Name);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			// both lang and xml:lang are needed for HTML5: https://www.w3.org/International/questions/qa-html-language-declarations
			Assert.AreEqual("gr", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("gr", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("body", n0.Name);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					switch (count1)
					{
					case 1:
						Assert.AreEqual("h1", n1.Name);
						Assert.AreEqual(1, n1.Attributes.Count);
						Assert.AreEqual("title", n1.Attributes["class"].Value);
						Assert.AreEqual(1, n1.ChildNodes.Count);
						Assert.AreEqual("#text", n1.FirstChild.Name);
						Assert.AreEqual("αναφορά", n1.FirstChild.InnerText);
						break;
					case 2:
						Assert.AreEqual("table", n1.Name);
						Assert.AreEqual("table", n1.Name);
						Assert.AreEqual(2, n1.Attributes.Count);
						Assert.AreEqual("1", n1.Attributes["border"].Value);
						Assert.AreEqual("100%", n1.Attributes["width"].Value);
						int count2 = 0;
						foreach (var n2 in n1.ChildNodes)
						{
							++count2;
							Assert.AreEqual("tr", n2.Name);
							Assert.AreEqual(0, n2.Attributes.Count);
							int count3 = 0;
							foreach (var n3 in n2.ChildNodes)
							{
								++count3;
								Assert.AreEqual("td", n3.Name);
								switch (count2)
								{
								case 1:
									Assert.AreEqual(1, n3.Attributes.Count);
									Assert.AreEqual("top", n3.Attributes["valign"].Value);
									switch (count3)
									{
									case 1:
										Assert.AreEqual(1, n3.ChildNodes.Count);
										Assert.AreEqual("#text", n3.FirstChild.Name);
										Assert.AreEqual("Κείμενο στο κελί r1-c1", n3.FirstChild.InnerText);
										break;
									case 2:
										Assert.AreEqual(1, n3.ChildNodes.Count);
										Assert.AreEqual("#text", n3.FirstChild.Name);
										Assert.AreEqual("Κείμενο στο κελί r1-c2", n3.FirstChild.InnerText);
										break;
									}
									break;
								case 2:
									switch (count3)
									{
									case 1:
										Assert.AreEqual(1, n3.Attributes.Count);
										Assert.AreEqual("#C0C0C0", n3.Attributes["bgcolor"].Value);
										Assert.AreEqual(1, n3.ChildNodes.Count);
										Assert.AreEqual("#text", n3.FirstChild.Name);
										Assert.AreEqual("Κείμενο στο κελί r2-c1", n3.FirstChild.InnerText);
										break;
									case 2:
										Assert.AreEqual(0, n3.Attributes.Count);
										Assert.AreEqual(1, n3.ChildNodes.Count);
										Assert.AreEqual("#text", n3.FirstChild.Name);
										Assert.AreEqual("Κείμενο στο κελί r2-c2", n3.FirstChild.InnerText);
										break;
									}
									break;
								}
							}
							Assert.AreEqual(2, count3);
						}
						Assert.AreEqual(2, count2);
						break;
					case 3:
						Assert.AreEqual("p", n1.Name);
						Assert.AreEqual(0, n1.Attributes.Count);
						Assert.AreEqual(1, n1.ChildNodes.Count);
						Assert.AreEqual("#text", n1.FirstChild.Name);
						Assert.AreEqual("All rights reserved (c) Gandalf Inc.", n1.FirstChild.InnerText);
						break;
					}
				}
				Assert.AreEqual(3, count1);
			}
			Assert.AreEqual(1, count0);
		}

		[Test]
		public void TestHtmlFragment_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""eu"">
  <body>
   <group restype=""x-html-div"" html:class=""bloom-ui bloomDialogContainer"" html:id=""text-properties-dialog"" html:style=""visibility: hidden;"">
    <trans-unit id=""EditTab.TextBoxProperties.Title"" restype=""x-html-div"" html:class=""bloomDialogTitleBar"" html:data-i18n=""EditTab.TextBoxProperties.Title"">
     <source xml:lang=""en"">Text Box Properties</source>
     <target xml:lang=""eu"">Testu-koadroko propietateak</target>
    </trans-unit>
    <group restype=""x-html-div"" html:class=""hideWhenFormattingEnabled bloomDialogMainPage"">
     <trans-unit id=""BookEditor.FormattingDisabled"" restype=""x-html-p"" html:data-i18n=""BookEditor.FormattingDisabled"">
      <source xml:lang=""en"">Sorry, Reader Templates do not allow changes to formatting.</source>
      <target xml:lang=""eu"">Barkatu, Reader Templates-ek ez du formatua aldatzeko baimenik.</target>
     </trans-unit>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual("#document", htmlDoc.DocumentNode.Name);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("div", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(3, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			Assert.AreEqual("bloom-ui bloomDialogContainer", htmlDoc.DocumentNode.FirstChild.Attributes["class"].Value);
			Assert.AreEqual("text-properties-dialog", htmlDoc.DocumentNode.FirstChild.Attributes["id"].Value);
			Assert.AreEqual("visibility: hidden;", htmlDoc.DocumentNode.FirstChild.Attributes["style"].Value);
			int count0 = 0;
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.ChildNodes.Count);
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("div", n0.Name);
				switch (count0)
				{
				case 1:
					Assert.AreEqual(2, n0.Attributes.Count);
					Assert.AreEqual("bloomDialogTitleBar", n0.Attributes["class"].Value);
					Assert.AreEqual("EditTab.TextBoxProperties.Title", n0.Attributes["data-i18n"].Value);
					Assert.AreEqual(1, n0.ChildNodes.Count);
					Assert.AreEqual(HtmlNodeType.Text, n0.FirstChild.NodeType);
					Assert.AreEqual("Testu-koadroko propietateak", n0.FirstChild.InnerText);
					break;
				case 2:
					Assert.AreEqual(1, n0.Attributes.Count);
					Assert.AreEqual("hideWhenFormattingEnabled bloomDialogMainPage", n0.Attributes["class"].Value);
					Assert.AreEqual(1, n0.ChildNodes.Count);
					var n1 = n0.FirstChild;
					Assert.AreEqual("p", n1.Name);
					Assert.AreEqual(1, n1.Attributes.Count);
					Assert.AreEqual("BookEditor.FormattingDisabled", n1.Attributes["data-i18n"].Value);
					Assert.AreEqual(1, n1.ChildNodes.Count);
					Assert.AreEqual(HtmlNodeType.Text, n1.FirstChild.NodeType);
					Assert.AreEqual("Barkatu, Reader Templates-ek ez du formatua aldatzeko baimenik.", n1.FirstChild.InnerText);
					break;
				}
			}
		}

		[Test]
		public void TestFormInDiv_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""cy"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""header"">
     <group restype=""x-html-script"" html:src=""/bloom/bookEdit/toolbox/bookSettings/bookSettings.js"" />
     <group restype=""x-html-link"" html:rel=""stylesheet"" html:href=""/bloom/bookEdit/toolbox/bookSettings/bookSettings.css"" />
    </group>
    <group restype=""x-html-body"">
     <trans-unit id=""genid-1"" restype=""x-html-h3"" html:data-panelid=""bookSettingsTool"" html:data-order=""100"">
      <source xml:lang=""en""><x id=""genid-2"" ctype=""image"" html:src=""/bloom/bookEdit/toolbox/bookSettings/icon.svg"" /></source>
      <target xml:lang=""cy""><x id=""genid-2"" ctype=""image"" html:src=""/bloom/bookEdit/toolbox/bookSettings/icon.svg"" /></target>
     </trans-unit>
     <group restype=""x-html-div"" html:data-panelid=""bookSettingsTool"">
      <group restype=""dialog"" html:id=""bookSettings"">
       <group restype=""x-html-div"" html:class=""showOnlyWhenBookWouldNormallyBeLocked"">
        <trans-unit id=""EditTab.Toolbox.Settings.UnlockShellBookIntroductionText"" restype=""x-html-p"" html:data-i18n=""EditTab.Toolbox.Settings.UnlockShellBookIntroductionText"">
         <source xml:lang=""en"">Bloom normally prevents most changes to shellbooks. If you need to add pages, change images, etc., tick the box below.</source>
         <target xml:lang=""cy"">Fel arfer, Blodau yn atal y rhan fwyaf o newidiadau i'r shellbooks. Os oes angen i ychwanegu tudalennau, newid delweddau, ac ati, ticiwch y blwch isod.</target>
        </trans-unit>
        <group restype=""x-html-input"" html:type=""checkbox"" html:name=""unlockShellBook"" html:onclick=""FrameExports.handleBookSettingCheckboxClick(this);"" />
        <trans-unit id=""EditTab.Toolbox.Settings.Unlock"" restype=""x-html-label"" html:data-i18n=""EditTab.Toolbox.Settings.Unlock"">
         <source xml:lang=""en"">Allow changes to this shellbook</source>
         <target xml:lang=""cy"">Caniatáu newidiadau i'r shellbook hwn</target>
        </trans-unit>
       </group>
      </group>
     </group>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			Assert.AreEqual("cy", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("cy", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			int count0 = 0;
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.ChildNodes.Count);
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				int count1 = 0;
				switch (count0)
				{
				case 1:
					Assert.AreEqual("head", n0.Name);
					Assert.AreEqual(0, n0.Attributes.Count);
					Assert.AreEqual(2, n0.ChildNodes.Count);
					foreach (var n1 in n0.ChildNodes)
					{
						++count1;
						switch (count1)
						{
						case 1:
							Assert.AreEqual("script", n1.Name);
							Assert.AreEqual(1, n1.Attributes.Count);
							Assert.AreEqual("/bloom/bookEdit/toolbox/bookSettings/bookSettings.js", n1.Attributes["src"].Value);
							Assert.AreEqual(0, n1.ChildNodes.Count);
							break;
						case 2:
							Assert.AreEqual("link", n1.Name);
							Assert.AreEqual(2, n1.Attributes.Count);
							Assert.AreEqual("stylesheet", n1.Attributes["rel"].Value);
							Assert.AreEqual("/bloom/bookEdit/toolbox/bookSettings/bookSettings.css", n1.Attributes["href"].Value);
							Assert.AreEqual(0, n1.ChildNodes.Count);
							break;
						}
					}
					break;
				case 2:
					Assert.AreEqual("body", n0.Name);
					Assert.AreEqual(0, n0.Attributes.Count);
					Assert.AreEqual(2, n0.ChildNodes.Count);
					foreach (var n1 in n0.ChildNodes)
					{
						++count1;
						switch (count1)
						{
						case 1:
							Assert.AreEqual("h3", n1.Name);
							Assert.AreEqual(2, n1.Attributes.Count);
							Assert.AreEqual("bookSettingsTool", n1.Attributes["data-panelid"].Value);
							Assert.AreEqual("100", n1.Attributes["data-order"].Value);
							Assert.AreEqual(1, n1.ChildNodes.Count);
							Assert.AreEqual("img", n1.FirstChild.Name);
							Assert.AreEqual(1, n1.FirstChild.Attributes.Count);
							Assert.AreEqual("/bloom/bookEdit/toolbox/bookSettings/icon.svg", n1.FirstChild.Attributes["src"].Value);
							Assert.AreEqual(0, n1.FirstChild.ChildNodes.Count);
							break;
						case 2:
							Assert.AreEqual("div", n1.Name);
							Assert.AreEqual(1, n1.Attributes.Count);
							Assert.AreEqual("bookSettingsTool", n1.Attributes["data-panelid"].Value);
							Assert.AreEqual(1, n1.ChildNodes.Count);
							Assert.AreEqual("form", n1.FirstChild.Name);
							Assert.AreEqual(1, n1.FirstChild.Attributes.Count);
							Assert.AreEqual("bookSettings", n1.FirstChild.Attributes["id"].Value);
							Assert.AreEqual(1, n1.FirstChild.ChildNodes.Count);
							Assert.AreEqual("div", n1.FirstChild.FirstChild.Name);
							Assert.AreEqual(1, n1.FirstChild.FirstChild.Attributes.Count);
							Assert.AreEqual("showOnlyWhenBookWouldNormallyBeLocked", n1.FirstChild.FirstChild.Attributes["class"].Value);
							Assert.AreEqual(3, n1.FirstChild.FirstChild.ChildNodes.Count);
							int count2 = 0;
							foreach (var n2 in n1.FirstChild.FirstChild.ChildNodes)
							{
								++count2;
								switch (count2)
								{
								case 1:
									Assert.AreEqual("p", n2.Name);
									Assert.AreEqual(1, n2.Attributes.Count);
									Assert.AreEqual("EditTab.Toolbox.Settings.UnlockShellBookIntroductionText", n2.Attributes["data-i18n"].Value);
									Assert.AreEqual(1, n2.ChildNodes.Count);
									Assert.AreEqual(HtmlNodeType.Text, n2.FirstChild.NodeType);
									Assert.AreEqual("Fel arfer, Blodau yn atal y rhan fwyaf o newidiadau i'r shellbooks. Os oes angen i ychwanegu tudalennau, newid delweddau, ac ati, ticiwch y blwch isod.",
										n2.FirstChild.InnerText);
									break;
								case 2:
									Assert.AreEqual("input", n2.Name);
									Assert.AreEqual(3, n2.Attributes.Count);
									Assert.AreEqual("checkbox", n2.Attributes["type"].Value);
									Assert.AreEqual("unlockShellBook", n2.Attributes["name"].Value);
									Assert.AreEqual("FrameExports.handleBookSettingCheckboxClick(this);", n2.Attributes["onclick"].Value);
									Assert.AreEqual(0, n2.ChildNodes.Count);
									break;
								case 3:
									Assert.AreEqual("label", n2.Name);
									Assert.AreEqual(1, n2.Attributes.Count);
									Assert.AreEqual("EditTab.Toolbox.Settings.Unlock", n2.Attributes["data-i18n"].Value);
									Assert.AreEqual(1, n2.ChildNodes.Count);
									Assert.AreEqual(HtmlNodeType.Text, n2.FirstChild.NodeType);
									Assert.AreEqual("Caniatáu newidiadau i'r shellbook hwn", n2.FirstChild.InnerText);
									break;
								}
							}
							break;
						}
					}
					break;
				}
			}
		}

		[Test]
		public void TestDuplicateI18nStrings_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""ga"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""x-html-body"">
     <group restype=""table"" html:class=""statistics clear"" html:style=""margin-left: 6px"">
      <group restype=""row"">
       <trans-unit id=""EditTab.Toolbox.LeveledReaderTool.ThisPage"" restype=""cell"" html:class=""tableTitle thisPageSection"" html:data-i18n=""EditTab.Toolbox.LeveledReaderTool.ThisPage"">
        <source xml:lang=""en"">This Page</source>
        <target xml:lang=""ga"">seo Leathanach</target>
       </trans-unit>
      </group>
      <group restype=""row"">
       <trans-unit id=""EditTab.Toolbox.LeveledReaderTool.Max"" restype=""cell"" html:class=""statistics-max"" html:data-i18n=""EditTab.Toolbox.LeveledReaderTool.Max"">
        <source xml:lang=""en"">Maximum</source>
        <target xml:lang=""ga"">uasmhéid</target>
       </trans-unit>
      </group>
     </group>
     <group restype=""table"" html:class=""statistics clear"" html:style=""margin-left: 6px"">
      <group restype=""row"">
       <trans-unit id=""EditTab.Toolbox.LeveledReaderTool.ThisBook"" restype=""cell"" html:class=""tableTitle"" html:data-i18n=""EditTab.Toolbox.LeveledReaderTool.ThisBook"">
        <source xml:lang=""en"">This Book</source>
        <target xml:lang=""ga"">seo Leabhar</target>
       </trans-unit>
      </group>
      <group restype=""row"">
       <trans-unit id=""EditTab.Toolbox.LeveledReaderTool.Max-1"" restype=""cell"" html:class=""statistics-max"" html:data-i18n=""EditTab.Toolbox.LeveledReaderTool.Max"">
        <source xml:lang=""en"">Maximum</source>
        <target xml:lang=""ga"">uasmhéid</target>
       </trans-unit>
      </group>
     </group>
    </group>
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			Assert.AreEqual("ga", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("ga", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			Assert.AreEqual(1, htmlDoc.DocumentNode.FirstChild.ChildNodes.Count);
			Assert.AreEqual("body", htmlDoc.DocumentNode.FirstChild.FirstChild.Name);
			Assert.AreEqual(0, htmlDoc.DocumentNode.FirstChild.FirstChild.Attributes.Count);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.FirstChild.ChildNodes.Count);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual("table", n0.Name);
				Assert.AreEqual(2, n0.Attributes.Count);
				Assert.AreEqual("statistics clear", n0.Attributes["class"].Value);
				Assert.AreEqual("margin-left: 6px", n0.Attributes["style"].Value);
				Assert.AreEqual(2, n0.ChildNodes.Count);
				int count1 = 0;
				foreach (var n1 in n0.ChildNodes)
				{
					++count1;
					Assert.AreEqual("tr", n1.Name);
					Assert.AreEqual(0, n1.Attributes.Count);
					Assert.AreEqual(1, n1.ChildNodes.Count);
					Assert.AreEqual("td", n1.FirstChild.Name);
					Assert.AreEqual(2, n1.FirstChild.Attributes.Count);
					Assert.AreEqual(1, n1.FirstChild.ChildNodes.Count);
					Assert.AreEqual(HtmlNodeType.Text, n1.FirstChild.FirstChild.NodeType);
					switch (count0)
					{
					case 1:
						switch (count1)
						{
						case 1:
							Assert.AreEqual("tableTitle thisPageSection", n1.FirstChild.Attributes["class"].Value);
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.ThisPage", n1.FirstChild.Attributes["data-i18n"].Value);
							Assert.AreEqual("seo Leathanach", n1.FirstChild.FirstChild.InnerText);
							break;
						case 2:
							Assert.AreEqual("statistics-max", n1.FirstChild.Attributes["class"].Value);
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.Max", n1.FirstChild.Attributes["data-i18n"].Value);
							Assert.AreEqual("uasmhéid", n1.FirstChild.FirstChild.InnerText);
							break;
						}
						break;
					case 2:
						switch (count1)
						{
						case 1:
							Assert.AreEqual("tableTitle", n1.FirstChild.Attributes["class"].Value);
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.ThisBook", n1.FirstChild.Attributes["data-i18n"].Value);
							Assert.AreEqual("seo Leabhar", n1.FirstChild.FirstChild.InnerText);
							break;
						case 2:
							Assert.AreEqual("statistics-max", n1.FirstChild.Attributes["class"].Value);
							Assert.AreEqual("EditTab.Toolbox.LeveledReaderTool.Max", n1.FirstChild.Attributes["data-i18n"].Value);
							Assert.AreEqual("uasmhéid", n1.FirstChild.FirstChild.InnerText);
							break;
						}
						break;
					}
				}
			}
		}

		[Test]
		public void TestBareAmpersand_X2H()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:html=""http://www.w3.org/TR/html"" xmlns:sil=""http://sil.org/software/XLiff"">
 <file original=""sample.htm"" datatype=""html"" source-language=""en"" target-language=""he"">
  <body>
   <group restype=""x-html-html"">
    <group restype=""header"">
     <group restype=""x-html-meta"" html:charset=""UTF-8"" />
     <trans-unit id=""genid-1"" restype=""x-html-title"">
      <source xml:lang=""en"">Null Front &amp; Back Matter for books where you don't want any</source>
      <target xml:lang=""he"">Null מול &amp; חזרה החומר עבור ספרים שבהם אתה לא רוצה שום</target>
     </trans-unit>
    </group>
    <group restype=""x-html-body"" />
   </group>
  </body>
 </file>
</xliff>");
			var converter = new XliffToHtmlConverter(xmlDoc);
			var htmlDoc = converter.Convert();
			Assert.IsNotNull(htmlDoc);
			Assert.IsNotNull(htmlDoc.DocumentNode);
			Assert.AreEqual(1, htmlDoc.DocumentNode.ChildNodes.Count);
			Assert.AreEqual("html", htmlDoc.DocumentNode.FirstChild.Name);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.Attributes.Count);
			Assert.AreEqual("he", htmlDoc.DocumentNode.FirstChild.Attributes["lang"].Value);
			Assert.AreEqual("he", htmlDoc.DocumentNode.FirstChild.Attributes["xml:lang"].Value);
			Assert.AreEqual(2, htmlDoc.DocumentNode.FirstChild.ChildNodes.Count);
			int count0 = 0;
			foreach (var n0 in htmlDoc.DocumentNode.FirstChild.ChildNodes)
			{
				++count0;
				Assert.AreEqual(0, n0.Attributes.Count);
				switch (count0)
				{
				case 1:
					Assert.AreEqual("head", n0.Name);
					Assert.AreEqual(2, n0.ChildNodes.Count);
					int count1 = 0;
					foreach (var n1 in n0.ChildNodes)
					{
						++count1;
						switch (count1)
						{
						case 1:
							Assert.AreEqual("meta", n1.Name);
							Assert.AreEqual(1, n1.Attributes.Count);
							Assert.AreEqual("UTF-8", n1.Attributes["charset"].Value);
							Assert.AreEqual(0, n1.ChildNodes.Count);
							break;
						case 2:
							Assert.AreEqual("title", n1.Name);
							Assert.AreEqual(0, n1.Attributes.Count);
							Assert.AreEqual(1, n1.ChildNodes.Count);
							Assert.AreEqual(HtmlNodeType.Text, n1.FirstChild.NodeType);
							Assert.AreEqual("Null מול & חזרה החומר עבור ספרים שבהם אתה לא רוצה שום", n1.FirstChild.InnerText);
							break;
						}
					}
					break;
				case 2:
					Assert.AreEqual("body", n0.Name);
					Assert.AreEqual(0, n0.ChildNodes.Count);
					break;
				}
			}
		}
	}
}

