using System;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using HtmlAgilityPack;
using System.Web;
using System.Collections.Generic;

namespace HtmlToXliff
{
	/// <summary>
	/// This html to xliff converter follows the "maximalist" approach described in
	/// http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html.
	/// </summary>
	public class HtmlToXliffConverter
	{
		public const string kXliffNamespace = "urn:oasis:names:tc:xliff:document:1.2";
		public const string kHtmlNamespace = "http://www.w3.org/TR/html";	// probably bogus but good enough?
		public const string kSilNamespace = "http://sil.org/software/XLiff";

		private readonly HtmlDocument _htmlDoc;
		private readonly XmlDocument _xliffDoc;
		private readonly XmlElement _file;

		/// <summary>
		/// Fix an HtmlAgility parser bug: form isn't always empty. We can't use a newer version of HtmlAgility
		/// which appears to fix this bug (and maybe others) because it won't work with either Mono 4 or even
		/// Mono 5.  But the code leaves a gaping visibility hole that lets us fix it at runtime...
		/// Call this method before creating/loading any HtmlDocument objects.
		/// </summary>
		/// <remarks>
		/// We could avoid this hack by including the HtmlAgility code with the fix in it in the l10nsharp
		/// solution.  But I'd rather use a NuGet package than clutter up our repository with borrowed code.
		/// </remarks>
		public static void FixHtmlParserBug()
		{
			// replaces "HtmlElementFlag.CanOverlap | HtmlElementFlag.Empty"
			HtmlNode.ElementsFlags["form"] = HtmlElementFlag.CanOverlap;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HtmlToXliff.HtmlToXliffConverter"/> class.
		/// </summary>
		/// <param name="filename">name of the input html file</param>
		public HtmlToXliffConverter(HtmlDocument htmlDoc, string filename)
		{
			_htmlDoc = htmlDoc;

			_xliffDoc = new XmlDocument();
			var decl = _xliffDoc.CreateXmlDeclaration("1.0", "utf-8", null);
			_xliffDoc.AppendChild(decl);
			var schema = new XmlSchema();
			schema.Namespaces.Add("xmlns", kXliffNamespace);
			schema.Namespaces.Add("html", kHtmlNamespace);
			schema.Namespaces.Add("sil", kSilNamespace);
			_xliffDoc.Schemas.Add(schema);

			var xliff = _xliffDoc.CreateElement("xliff");
			_xliffDoc.AppendChild(xliff);
			xliff.SetAttribute("version", "1.2");
			xliff.SetAttribute("xmlns", kXliffNamespace);
			xliff.SetAttribute("xmlns:html", kHtmlNamespace);	// only attribute allowed a colon...
			xliff.SetAttribute("xmlns:sil", kSilNamespace);		// only attribute allowed a colon...
			_file = _xliffDoc.CreateElement("file");
			xliff.AppendChild(_file);
			_file.SetAttribute("original", filename);
			_file.SetAttribute("datatype", "html");
			_file.SetAttribute("source-language", "en");
		}

		/// <summary>
		/// Convert the loaded HTML document into an Xliff XML document.
		/// </summary>
		public XmlDocument Convert()
		{
			var xliffBody = _xliffDoc.CreateElement("body");
			_file.AppendChild(xliffBody);
			var html = _htmlDoc.DocumentNode.Element("html");
			if (html == null)
			{
				// Process whatever fragments we have at the top level.
				ProcessHtmlElement(xliffBody, _htmlDoc.DocumentNode);
				return _xliffDoc;
			}
			var group1 = _xliffDoc.CreateElement("group");
			xliffBody.AppendChild(group1);
			group1.SetAttribute("restype", GetXliffTypeForElement(html.Name));
			var head = html.Element("head");
			if (head != null)
			{
				var group2 = _xliffDoc.CreateElement("group");
				group1.AppendChild(group2);
				group2.SetAttribute("restype", GetXliffTypeForElement(head.Name));
				ProcessHtmlElement(group2, head);
			}
			var body = html.Element("body");
			if (body != null)
			{
				var group3 = _xliffDoc.CreateElement("group");
				group1.AppendChild(group3);
				group3.SetAttribute("restype", GetXliffTypeForElement(body.Name));
				ProcessHtmlElement(group3, body);
			}
			return _xliffDoc;
		}

		private void ProcessHtmlElement(XmlElement parentXliffElement, HtmlNode htmlElement)
		{
			foreach (var node in htmlElement.ChildNodes)
			{
				if (!IsEmptyElement(node.Name) && ContainsTranslatableText(node))
				{
					var transUnit = _xliffDoc.CreateElement("trans-unit");
					parentXliffElement.AppendChild(transUnit);
					ProcessTransUnit(transUnit, node);
				}
				else if (node.NodeType == HtmlNodeType.Element)
				{
					var groupX = _xliffDoc.CreateElement("group");
					parentXliffElement.AppendChild(groupX);
					var restype = GetXliffTypeForElement(node.Name);
					// okay for ctype, but not for restype
					if (restype == "lb" || restype == "image")
						restype = String.Format("x-html-{0}", node.Name);
					groupX.SetAttribute("restype", restype);
					//if (bnode.Name == "br")
					//	groupX.SetAttribute("equiv-text", "\n");
					CopyHtmlAttributes(groupX, node);
					ProcessHtmlElement(groupX, node);
				}
			}
		}

		private void CopyHtmlAttributes(XmlElement xml, HtmlNode html, bool checkForTranslatableAttribute = false)
		{
			foreach (var attr in html.Attributes)
			{
				if (attr.Name == "lang")
					xml.SetAttribute("xml:" + attr.Name, attr.Value);	// colon allowed for xml:lang attribute
				else if (!checkForTranslatableAttribute || !IsTranslatableAttribute(attr.Name, html.Name))
					xml.SetAttribute(attr.Name, kHtmlNamespace, attr.Value);
			}
		}

		private bool ContainsTranslatableText(HtmlNode bnode)
		{
			if (bnode.NodeType != HtmlNodeType.Element)
				return false;	// ignor comments, text handled already in recursion
			if (bnode.ChildNodes.Count == 0 || bnode.InnerHtml == "")
				return false;	// empty node shouldn't produce a trans-unit

			// Check whether any #text children have nonwhitespace characters.
			// If so, then we're at a node with translatable text.
			foreach (var tnode in bnode.Elements("#text"))
			{
				if (!String.IsNullOrWhiteSpace(tnode.InnerText))
					return true;
			}
			// Check whether all of the children that aren't #text or #comment
			// are inline elements.  If so, we can create a trans-unit.
			foreach (var tnode in bnode.ChildNodes)
			{
				if (tnode.NodeType != HtmlNodeType.Element)
					continue;
				if (!IsInlineElement(tnode.Name))
					return false;
				if (IsWrapperElement(tnode.Name))
					return false;
			}
			return true;
		}

		public static string GetAttributesInString(HtmlNode node)
		{
			var bldr = new StringBuilder();
			foreach (var a in node.Attributes)
			{
				if (bldr.Length > 0)
					bldr.Append(", ");
				bldr.AppendFormat("{0}='{1}'", a.Name, a.Value);
			}
			return bldr.ToString();
		}

		private void ProcessTransUnit(XmlElement transUnit, HtmlNode translatableNode)
		{
			transUnit.SetAttribute("id", ExtractOrCreateIdValue(translatableNode));
			transUnit.SetAttribute("restype", GetXliffTypeForElement(translatableNode.Name));
			CopyHtmlAttributes(transUnit, translatableNode, true);
			var source = _xliffDoc.CreateElement("source");
			transUnit.AppendChild(source);
			source.SetAttribute("xml:lang", "en");
			if (HasTranslatableAttribute(translatableNode))
				AddTranslatableAttributeNodes(source, translatableNode);
			ProcessSourceNode(source, translatableNode);
		}

		private void AddTranslatableAttributeNodes(XmlNode xml, HtmlNode html)
		{
			foreach (var attr in html.Attributes)
			{
				if (IsTranslatableAttribute(attr.Name, html.Name))
				{
					var phNode = _xliffDoc.CreateElement("ph");
					xml.AppendChild(phNode);
					phNode.SetAttribute("id", CreateNextIdValue());	// id value from html node already used for trans-unit
					var subNode = _xliffDoc.CreateElement("sub");
					phNode.AppendChild(subNode);
					subNode.SetAttribute("ctype", GetXliffTypeForAttribute(attr.Name, html.Name));
					var textNode = _xliffDoc.CreateTextNode(attr.Value);
					subNode.AppendChild(textNode);
				}
			}
		}

		private void ProcessSourceNode(XmlElement source, HtmlNode translatableNode)
		{
			foreach (var node in translatableNode.ChildNodes)
			{
				if (node.NodeType == HtmlNodeType.Text)
				{
					var text = HttpUtility.HtmlDecode(node.InnerText);
					var tn = _xliffDoc.CreateTextNode(text);
					source.AppendChild(tn);
				}
				else if (node.NodeType != HtmlNodeType.Element)
				{
					continue;
				}
				else if (IsEmptyElement(node.Name))
				{
					if (HasTranslatableAttribute(node))
						ProcessEmptyElementWithTranslatableAttribute(source, node);
					else
						ProcessEmptyElement(source, node);
				}
				else
				{
					var gNode = _xliffDoc.CreateElement("g");
					source.AppendChild(gNode);
					gNode.SetAttribute("id", ExtractOrCreateIdValue(node));
					gNode.SetAttribute("ctype", GetXliffTypeForElement(node.Name));
					CopyHtmlAttributes(gNode, node, true);
					ProcessSourceNode(gNode, node);
				}
			}
		}

		private void ProcessEmptyElement(XmlElement source, HtmlNode node)
		{
			var xNode = _xliffDoc.CreateElement("x");
			source.AppendChild(xNode);
			xNode.SetAttribute("id", ExtractOrCreateIdValue(node));
			xNode.SetAttribute("ctype", GetXliffTypeForElement(node.Name));
			CopyHtmlAttributes(xNode, node);
			if (node.Name == "br")
				xNode.SetAttribute("equiv-text", "\n");
		}

		private void ProcessEmptyElementWithTranslatableAttribute(XmlElement source, HtmlNode node)
		{
			var phNode = _xliffDoc.CreateElement("ph");
			source.AppendChild(phNode);
			phNode.SetAttribute("id", ExtractOrCreateIdValue(node));
			phNode.SetAttribute("ctype", GetXliffTypeForElement(node.Name));
			CopyHtmlAttributes(phNode, node, true);
			foreach (var attr in node.Attributes)
			{
				if (IsTranslatableAttribute(attr.Name, node.Name))
				{
					var subNode = _xliffDoc.CreateElement("sub");
					phNode.AppendChild(subNode);
					subNode.SetAttribute("ctype", GetXliffTypeForAttribute(attr.Name, node.Name));
					var textNode = _xliffDoc.CreateTextNode(attr.Value);
					subNode.AppendChild(textNode);
				}
			}
		}

		private bool HasTranslatableAttribute(HtmlNode node)
		{
			foreach (var attr in node.Attributes)
			{
				if (IsTranslatableAttribute(attr.Name, node.Name))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Check whether the named attribute of the  named HTML element contains translatable content.
		/// These values come from the XLIFF 1.2 Representation Guide for HTML.
		/// (http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html#SectionDetailsElements)
		/// </summary>
		private bool IsTranslatableAttribute(string attrName, string nodeName)
		{
			switch (attrName)
			{
			case "abbr":		return nodeName == "td" || nodeName == "th";
			case "accesskey":	return nodeName == "a" || nodeName == "area" || nodeName == "button" || nodeName == "input" || nodeName == "label" || nodeName == "legend" || nodeName == "textarea";
			case "alt":			return nodeName == "applet" || nodeName == "area" || nodeName == "img" || nodeName == "input";
			case "content":		return nodeName == "meta";
			case "label":		return nodeName == "option" || nodeName == "optgroup";
			case "prompt":		return nodeName == "isindex";
			case "standby":		return nodeName == "object";
			case "summary":		return nodeName == "table";
			case "title":		return nodeName != "base" && nodeName != "basefont" && nodeName != "head" && nodeName != "html" && nodeName != "meta" && nodeName != "param" && nodeName != "script" && nodeName != "title";
			case "value":		return nodeName == "input" || nodeName == "button";	// possibly if nodeName == "param", but not always so don't worry about it...
			default:			return false;
			}
		}

		/// <summary>
		/// Check whether the named HTML element can be an inline element.
		/// These values come from the XLIFF 1.2 Representation Guide for HTML.
		/// (http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html#SectionDetailsElements)
		/// </summary>
		private bool IsInlineElement(string name)
		{
			switch (name)
			{
			case "a":
			case "abbr":
			case "acronym":
			case "applet":
			case "b":
			case "bdo":
			case "big":
			case "blink":
			case "br":
			case "button":
			case "cite":
			case "code":
			case "del":
			case "dfn":
			case "em":
			case "embed":
			case "face":
			case "font":
			case "i":
			case "iframe":
			case "img":
			case "input":
			case "ins":
			case "kbd":
			case "label":
			case "map":
			case "nobr":
			case "object":
			case "param":
			case "q":
			case "rb":
			case "rbc":
			case "rp":
			case "rt":
			case "rtc":
			case "ruby":
			case "s":
			case "samp":
			case "select":
			case "small":
			case "spacer":
			case "span":
			case "strike":
			case "strong":
			case "sub":
			case "sup":
			case "symbol":
			case "textarea":
			case "tt":
			case "u":
			case "var":
			case "wbr":
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Check whether the named HTML element is always an empty element.
		/// These values come from the XLIFF 1.2 Representation Guide for HTML.
		/// (http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html#SectionDetailsElements)
		/// </summary>
		private bool IsEmptyElement(string name)
		{
			switch (name)
			{
			case "area":
			case "base":
			case "basefont":
			case "bgsound":
			case "br":
			case "col":
			case "frame":
			case "hr":
			case "img":
			case "input":
			case "isindex":
			case "link":
			case "meta":
			case "nobr":
			case "param":
			case "spacer":
			case "wbr":
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Check whether the named HTML element contains PCDATA (uninterpreted by the browser) data.
		/// These values come from the XLIFF 1.2 Representation Guide for HTML.
		/// (http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html#SectionDetailsElements)
		/// </summary>
		private bool IsPcdataElement(string name)
		{
			switch (name)
			{
			case "option":
			case "plaintext":
			case "script":
			case "style":
			case "textarea":
			case "title":
			case "xmp":
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Check whether the named HTML element can contain both text and another element.
		/// These values come from the XLIFF 1.2 Representation Guide for HTML.
		/// (http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html#SectionDetailsElements)
		/// </summary>
		private bool IsMixedElement(string name)
		{
			switch (name)
			{
			case "abbr":
			case "acronym":
			case "address":
			case "applet":
			case "b":
			case "bdo":
			case "big":
			case "blink":
			case "button":
			case "caption":
			case "cite":
			case "code":
			case "dd":
			case "del":
			case "dfn":
			case "div":
			case "dt":
			case "em":
			case "face":
			case "fieldset":
			case "font":
			case "h1":
			case "h2":
			case "h3":
			case "h4":
			case "h5":
			case "h6":
			case "i":
			case "iframe":
			case "ins":
			case "kbd":
			case "label":
			case "legend":
			case "li":
			case "listing":
			case "marquee":
			case "object":
			case "p":
			case "pre":
			case "q":
			case "rb":
			case "rbc":
			case "rp":
			case "rt":
			case "rtc":
			case "ruby":
			case "s":
			case "samp":
			case "small":
			case "span":
			case "strike":
			case "strong":
			case "sub":
			case "sup":
			case "symbol":
			case "td":
			case "th":
			case "tt":
			case "u":
			case "var":
			case "xml":
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Check whether the named HTML element can "wrap a group".
		/// These values come from the XLIFF 1.2 Representation Guide for HTML.
		/// (http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html#SectionDetailsElements)
		/// </summary>
		private bool IsWrapperElement(string name)
		{
			switch (name)
			{
			case "a":
			case "applet":
			case "blockquote":
			case "body":
			case "colgroup":
			case "dir":
			case "dl":
			case "fieldset":
			case "form":
			case "head":
			case "html":
			case "menu":
			case "noembed":
			case "noframes":
			case "noscript":
			case "object":
			case "ol":
			case "optgroup":
			case "select":
			case "table":
			case "tbody":
			case "tfoot":
			case "thead":
			case "tr":
			case "ul":
			case "xml":
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Get the ctype (or restype if not inline) value for the given HTML element name.
		/// These values come from the XLIFF 1.2 Representation Guide for HTML.
		/// (http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html#SectionDetailsElements)
		/// </summary>
		private string GetXliffTypeForElement(string name)
		{
			switch (name)
			{
			case "b":			return "bold";
			case "br":			return "lb";	// schema doesn't like this for restype, but does for ctype
			case "caption":		return "caption";
			case "fieldset":	return "groupbox";
			case "form":		return "dialog";
			case "frame":		return "frame";
			case "head":		return "header";
			case "i":			return "italic";
			case "img":			return "image";	// schema doesn't like this for restype, but does for ctype
			case "li":			return "listitem";
			case "menu":		return "menu";
			case "table":		return "table";
			case "td":			return "cell";
			case "tfoot":		return "footer";
			case "tr":			return "row";
			case "u":			return "underlined";
			default:			return "x-html-" + name;
			}
		}

		/// <summary>
		/// Get the ctype value for the given HTML attribute and element name.
		/// These values come from the XLIFF 1.2 Representation Guide for HTML.
		/// (http://docs.oasis-open.org/xliff/v1.2/xliff-profile-html/xliff-profile-html-1.2-cd02.html#SectionDetailsElements)
		/// </summary>
		private string GetXliffTypeForAttribute(string attrName, string nodeName)
		{
			if (attrName == "label")
				return "label";
			else
				return String.Format("x-html-{0}-{1}", nodeName, attrName);
		}

		/// <summary>
		/// Counter used to generate id values when no appropriate attribute exists (id / i18n / data-i18n).
		/// </summary>
		private int _idCounter;
		/// <summary>
		/// HashSet to keep track of used id strings derived from html attributes (needed to prevent reuse of same id).
		/// </summary>
		private HashSet<string> _idsUsed = new HashSet<string>();

		private string ExtractOrCreateIdValue(HtmlNode node)
		{
			string id = null;
			bool foundBest = false;
			foreach (var attr in node.Attributes)
			{
				switch (attr.Name)
				{
				case "id":
					if (id == null)
						id = attr.Value;
					break;
				case "i18n":
					id = attr.Value;
					break;
				case "data-i18n":
					id = attr.Value;
					foundBest = true;
					break;
				}
				if (foundBest)
					break;
			}
			if (String.IsNullOrEmpty(id))
			{
				id = CreateNextIdValue();
			}
			else
			{
				if (_idsUsed.Contains(id))
					id = AdjustUsedId(id);
				_idsUsed.Add(id);
			}
			return id;
		}

		private string CreateNextIdValue()
		{
			return String.Format("genid-{0}", ++_idCounter);
		}

		private string AdjustUsedId(string oldId)
		{
			for (int i = 1; ; ++i)
			{
				var newId = String.Format("{0}-{1}", oldId, i);
				if (!_idsUsed.Contains(newId))
					return newId;
			}
		}
	}
}

