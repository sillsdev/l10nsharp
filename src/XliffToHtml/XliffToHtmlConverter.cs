using System;
using HtmlAgilityPack;
using System.Xml;
using System.Diagnostics;

namespace XliffToHtml
{
	/// <summary>
	/// This xliff to html converter assumes the "maximalist" approach described in
	/// http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html.
	/// </summary>
	public class XliffToHtmlConverter
	{
		public const string kXliffNamespace = "urn:oasis:names:tc:xliff:document:1.2";
		public const string kHtmlNamespace = "http://www.w3.org/TR/html";	// probably bogus but good enough?
		public const string kSilNamespace = "http://sil.org/software/XLiff";

		private XmlDocument _xliffDoc;
		private XmlNamespaceManager _nsmgr;
		private HtmlDocument _htmlDoc;

		public XliffToHtmlConverter(XmlDocument xliffDoc)
		{
			_xliffDoc = xliffDoc;
			_nsmgr = new XmlNamespaceManager(_xliffDoc.NameTable);
			_nsmgr.AddNamespace("x", kXliffNamespace);
			_nsmgr.AddNamespace("html", kHtmlNamespace);
			_nsmgr.AddNamespace("sil", kSilNamespace);
		}

		public HtmlDocument Convert()
		{
			_htmlDoc = new HtmlDocument();
			foreach (XmlNode xmlBody in _xliffDoc.SelectNodes("/x:xliff/x:file/x:body", _nsmgr))
			{
				foreach (XmlNode node in xmlBody.ChildNodes)
				{
					if (node.Name == "group")
					{
						var elementName = GetElementNameFromAttribute(node, "restype");
						if (elementName != null)
						{
							ProcessGroupElement(_htmlDoc.DocumentNode, node, elementName);
						}
					}
					else
					{
					}
				}
			}
			return _htmlDoc;
		}

		private void ProcessGroupElement(HtmlNode htmlParent, XmlNode groupX, string elementName)
		{
			var htmlElement = CreateElementAndCopyAttributes(htmlParent, groupX, elementName);
			foreach (XmlNode node in groupX.ChildNodes)
			{
				if (node.Name == "group")
				{
					var name = GetElementNameFromAttribute(node, "restype");
					if (name != null)
					{
						ProcessGroupElement(htmlElement, node, name);
					}
				}
				else if (node.Name == "trans-unit")
				{
					var name = GetElementNameFromAttribute(node, "restype");
					if (name != null)
					{
						ProcessTransUnit(htmlElement, node, name);
					}
				}
				else if (node.NodeType == XmlNodeType.Element)
				{
					Debug.WriteLine("DEBUG: ProcessGroupElement() found something other than group or trans-unit: " + node.Name);
				}
			}
		}

		private void ProcessTransUnit(HtmlNode htmlParent, XmlNode transUnit, string elementName)
		{
			var htmlElement = CreateElementAndCopyAttributes(htmlParent, transUnit, elementName);

			var target = transUnit.SelectSingleNode("x:target", _nsmgr);
			if (target == null || String.IsNullOrEmpty(target.InnerText))
				target = transUnit.SelectSingleNode("x:source", _nsmgr);;	// use the source, Luke, if there's no translation.
			Debug.Assert(target != null);

			ProcessTargetContent(htmlElement, target);
		}

		private void ProcessSubTransUnit(HtmlNode htmlParent, XmlNode xmlNode, string elementName)
		{
			var htmlElement = CreateElementAndCopyAttributes(htmlParent, xmlNode, elementName);
			ProcessTargetContent(htmlElement, xmlNode);
		}

		private void ProcessTargetContent(HtmlNode htmlElement, XmlNode target)
		{
			foreach (XmlNode node in target.ChildNodes)
			{
				if (node.NodeType == XmlNodeType.Text)
				{
					var tNode = _htmlDoc.CreateTextNode(node.InnerText);
					htmlElement.AppendChild(tNode);
				}
				else if (node.Name == "g" || node.Name == "x" || node.Name == "ph")
				{
					var name = GetElementNameFromAttribute(node, "ctype");
					if (name != null)
					{
						ProcessSubTransUnit(htmlElement, node, name);
					}
					else if (node.Name == "ph" && node.FirstChild.Name == "sub" && node.ChildNodes.Count == 1)
					{
						name = GetAttributeNameFromAttribute(node.FirstChild, "ctype", htmlElement.Name);
						if (name != null)
							htmlElement.SetAttributeValue(name, node.FirstChild.InnerText);
					}
				}
				else if (node.Name == "sub" && target.Name == "ph")
				{
					var attrName = GetAttributeNameFromAttribute(node, "ctype", htmlElement.Name);
					if (attrName != null)
						htmlElement.SetAttributeValue(attrName, node.InnerText);
				}
				else if (node.NodeType == XmlNodeType.Element)
				{
					Debug.WriteLine("ProcessTargetContent found something other than #text, x, g, or ph: " + node.Name);
				}
			}
		}

		HtmlNode CreateElementAndCopyAttributes(HtmlNode htmlParent, XmlNode xmlNode, string elementName)
		{
			var htmlElement = _htmlDoc.CreateElement(elementName);
			htmlParent.AppendChild(htmlElement);
			CopyAttributesToElement(xmlNode, htmlElement);
			if (htmlElement.Name == "html")
			{
				// Set lang and xml:lang to the target language if possible.
				var targAttr = _xliffDoc.SelectSingleNode("/x:xliff/x:file/@target-language", _nsmgr);
				if (targAttr == null)
					targAttr = _xliffDoc.SelectSingleNode("/x:xliff/x:file/x:body//x:trans-unit/x:target/@xml:lang", _nsmgr);
				if (targAttr == null)
					targAttr = _xliffDoc.SelectSingleNode("/x:xliff/x:file/@source-language", _nsmgr);
				if (targAttr != null)
				{
					htmlElement.SetAttributeValue("lang", targAttr.Value);
					htmlElement.SetAttributeValue("xml:lang", targAttr.Value);
				}
			}
			return htmlElement;
		}

		private string GetElementNameFromAttribute(XmlNode node, string name)
		{
			var xtype = node.Attributes[name]?.Value;
			if (xtype != null)
			{
				switch (xtype)
				{
				case "bold":		return "b";
				case "lb":			return "br";
				case "caption":		return "caption";
				case "groupbox":	return "fieldset";
				case "dialog":		return "form";
				case "frame":		return "frame";
				case "header":		return "head";
				case "italic":		return "i";
				case "image":		return "img";
				case "listitem":	return "li";
				case "menu":		return "menu";
				case "table":		return "table";
				case "cell":		return "td";
				case "row":			return "tr";
				case "footer":		return "tfoot";
				case "underlined":	return "u";
				default:
					if (xtype.StartsWith("x-html-"))
						return xtype.Substring(7);	// 7 == "x-html-".Length
					else
						return null;
				}
			}
			return null;
		}

		private string GetAttributeNameFromAttribute(XmlNode node, string attrName, string htmlElementName)
		{
			var xtype = node.Attributes[attrName]?.Value;
			if (xtype != null)
			{
				if (xtype == "label")
					return "label";
				var tagHead = String.Format("x-html-{0}-", htmlElementName);
				if (xtype.StartsWith(tagHead))
					return xtype.Substring(tagHead.Length);
			}
			return null;
		}

		private void CopyAttributesToElement(XmlNode xmlNode, HtmlNode htmlNode)
		{
			foreach (XmlAttribute attr in xmlNode.Attributes)
			{
				if (attr.Name.StartsWith("html:"))
					htmlNode.SetAttributeValue(attr.Name.Substring(5), attr.Value);	// 5 == "html:".Length
				else if (attr.Name == "xml:lang")
					htmlNode.SetAttributeValue("lang", attr.Value);
			}
		}
	}
}

