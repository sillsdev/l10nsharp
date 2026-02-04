using System;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Xml;
using System.Reflection;

namespace L10NSharp.XLiffUtils
{
	internal static class XliffXmlSerializationHelper
	{
		#region XLiffXmlReader class

		public const string kSilNamespace = "http://sil.org/software/XLiff";

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Custom XmlTextReader that can preserve whitespace characters (spaces, tabs, etc.)
		/// that are in XML elements. This allows us to properly handle deserialization of
		/// paragraph runs that contain runs that contain only whitespace characters.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private class XLiffXmlReader : XmlTextReader
		{
			private readonly bool m_fKeepWhitespaceInElements;

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Initializes a new instance of the <see cref="XLiffXmlReader"/> class.
			/// </summary>
			/// <param name="filename">The filename.</param>
			/// <param name="fKeepWhitespaceInElements">if set to <c>true</c>, the reader
			/// will preserve and return elements that contain only whitespace, otherwise
			/// these elements will be ignored during a deserialization.</param>
			/// --------------------------------------------------------------------------------
			public XLiffXmlReader(string filename, bool fKeepWhitespaceInElements) :
				base(new StreamReader(filename))
			{
				m_fKeepWhitespaceInElements = fKeepWhitespaceInElements;
			}

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Reads the next node from the stream.
			/// </summary>
			/// --------------------------------------------------------------------------------
			public override bool Read()
			{
				// Since we use this class only for deserialization, catch file not found
				// exceptions for the case when the XML file contains a !DOCTYPE declaration
				// and the specified DTD file is not found. (This is because the base class
				// attempts to open the DTD by merely reading the !DOCTYPE node from the
				// current directory instead of relative to the XML document location.)
				try
				{
					return base.Read();
				}
				catch (FileNotFoundException)
				{
					return true;
				}
				catch (Exception e)
				{
					Console.WriteLine(e); // Allows for setting a breakpoint when debugging
					throw;
				}
			}

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// Gets the type of the current node.
			/// </summary>
			/// --------------------------------------------------------------------------------
			public override XmlNodeType NodeType
			{
				get
				{
					if (m_fKeepWhitespaceInElements &&
						(base.NodeType == XmlNodeType.Whitespace ||
						base.NodeType == XmlNodeType.SignificantWhitespace) &&
						Value.IndexOf('\n') < 0 && Value.Trim().Length == 0)
					{
						// We found some whitespace that was most
						// likely whitespace we want to keep.
						return XmlNodeType.Text;
					}

					return base.NodeType;
				}
			}
		}

		#endregion

		#region Methods for XML serializing and deserializing data

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Serializes an object to the specified file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool SerializeToFile<T>(string filename, T data)
		{
			// Ensure that the file can be written, even to a new language tag subfolder.
			var folder = Path.GetDirectoryName(filename);
			if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			using (TextWriter writer = new StreamWriter(filename))
			{
				XmlSerializerNamespaces nameSpaces = new XmlSerializerNamespaces();
				nameSpaces.Add(string.Empty, "urn:oasis:names:tc:xliff:document:1.2");
				nameSpaces.Add("sil", kSilNamespace);
				XmlSerializer serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(writer, data, nameSpaces);
				writer.Close();
				return true;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified file to an object of the specified type.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="filename">The filename from which to load</param>
		/// <param name="e">The exception generated during the deserialization.</param>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromFile<T>(string filename, out Exception e) where T : class
		{
			return DeserializeFromFile<T>(filename, false, out e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes XML from the specified file to an object of the specified type.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="filename">The filename from which to load</param>
		/// <param name="fKeepWhitespaceInElements">if set to <c>true</c>, the reader
		/// will preserve and return elements that contain only whitespace, otherwise
		/// these elements will be ignored during a deserialization.</param>
		/// <param name="e">The exception generated during the deserialization.</param>
		/// ------------------------------------------------------------------------------------
		public static T DeserializeFromFile<T>(string filename, bool fKeepWhitespaceInElements,
			out Exception                             e) where T : class
		{
			T data = null;
			e = null;

			try
			{
				if (!File.Exists(filename))
					return null;

				using (XLiffXmlReader reader = new XLiffXmlReader(
					filename, fKeepWhitespaceInElements))
				{
					data = DeserializeInternal<T>(reader);
				}
			}
			catch (Exception outEx)
			{
				e = outEx;
			}

			return data;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes an object using the specified reader.
		/// </summary>
		/// <typeparam name="T">The type of object to deserialize</typeparam>
		/// <param name="reader">The reader.</param>
		/// <returns>The deserialized object</returns>
		/// ------------------------------------------------------------------------------------
		private static T DeserializeInternal<T>(XmlReader reader)
		{
			XmlSerializer deserializer = new XmlSerializer(typeof(T));
			deserializer.UnknownAttribute += deserializer_UnknownAttribute;
			deserializer.UnknownElement += deserializer_UnknownElement;
			return (T) deserializer.Deserialize(reader);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the UnknownAttribute event of the deserializer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Xml.Serialization.XmlAttributeEventArgs"/>
		/// instance containing the event data.</param>
		/// ------------------------------------------------------------------------------------
		static void deserializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			if (e.Attr.LocalName == "lang")
			{
				// This is special handling for the xml:lang attribute that is used to specify
				// the WS for the current paragraph, run in a paragraph, etc. The XmlTextReader
				// treats xml:lang as a special case and basically skips over it (but it does
				// set the current XmlLang to the specified value). This keeps the deserializer
				// from getting xml:lang as an attribute which keeps us from getting these values.
				// The fix is to inspect the object currently being deserialized and, using reflection,
				// look for a field or property marked with an XmlAttribute for "xml:lang", then set it
				// explicitly to the value provided here. (TE-8328)
				object obj = e.ObjectBeingDeserialized;
				if (obj == null)
					return; // Probably impossible here, but just in case.
				Type type = obj.GetType();
				foreach (FieldInfo field in type.GetFields())
				{
					object[] attributes =
						field.GetCustomAttributes(typeof(XmlAttributeAttribute), false);
					if (attributes.Length == 1 &&
						((XmlAttributeAttribute) attributes[0]).AttributeName == "xml:lang")
					{
						field.SetValue(obj, e.Attr.Value);
						return;
					}
				}

				foreach (PropertyInfo prop in type.GetProperties())
				{
					object[] attributes = prop.GetCustomAttributes(typeof(XmlAttributeAttribute), false);
					if (attributes.Length == 1 &&
						((XmlAttributeAttribute) attributes[0]).AttributeName == "xml:lang")
					{
						prop.SetValue(obj, e.Attr.Value, null);
						return;
					}
				}
			}
		}

		/// <summary>
		/// Handle complex encoded HTML markup inside the translated strings. This is detected by unknown
		/// elements encountered while deserializing XLiffTransUnitVariant objects.
		/// </summary>
		private static void deserializer_UnknownElement(object sender, XmlElementEventArgs e)
		{
			/* The XLIFF standard allows for internal markup inside <source> and <target> elements.
			 * This markup takes the form of <g> and <x> elements. As far I can tell, the only
			 * difference between <g> and <x> elements is that <g> elements have content and <x>
			 * elements do not. Consider an input like this:
			 *
			 * <target xml:lang='en'>This is a <g id='gen1' ctype='x-html-strong' html:style='color:red;'>test</g>.</target>
			 *
			 * The default XmlSerializer stores "This is a " in the XLiffTransUnitVariant Value property, skips
			 * over the "<g id='gen1' ctype='x-html-strong' html:style='color:red;'>test</g>", and
			 * stores "." in the XLiffTransUnitVariant Value property, wiping out what it had stored earlier.
			 * This behavior motivates this method, which is called whenever an element is encountered
			 * reading the content of <source> or <target> elements.
			 *
			 * When the content of the <target> element in the example above is deserialized, the following steps
			 * occur:
			 * 1) The XmlSerializer encounters the text node containing "This is a " and stores it in the Value
			 *    property of the XLiffTransUnitVariant object (which places it in the private _value variable).
			 *
			 *** tuv._value = "This is a "
			 *** tuv._deserializedFromElement = null
			 *
			 * 2) The XmlSerializer encounters the <g> element and calls this method.
			 * 3) A string builder is created and initialized with "This is a " from the existing Value.
			 * 4) The ctype attribute of the <g> element is decoded to "strong", and the string builder is
			 *    updated to contain "This is a <strong".
			 * 5) Stepping through the attributes, the html:style attribute is found and decoded, and the
			 *    string builder is updated to contain "This is a <strong style=\"color:red;\"".
			 * 6) Reaching the end of the attributes, the code notices that it is processing a <g> element
			 *    so it updates the string builder to contain "This is a <strong style=\"color:red;\">test</strong>".
			 * 7) The content of the string builder is stored in the private _deserializedFromElement variable
			 *    of the XLiffTransUnitVariant object.
			 *
			 *** tuv._value = null
			 *** tuv._deserializedFromElement = "This is a <strong style=\"color:red;\">test</strong>"
			 *
			 * 8) The XmlSerializer encounters the text node containing "." and stores it in the Value property
			 *    of the XLiffTransUnitVariant object.
			 *
			 *** tuv._value = "."
			 *** tuv._deserializedFromElement = "This is a <strong style=\"color:red;\">test</strong>"
			 *
			 * At this point, the desired value of the XLiffTransUnitVariant object's Value property is split between
			 * two private string variables: _value and _deserializedFromElement. The next call from anywhere
			 * to the getter of the Value property will put the pieces together. This could even be from the
			 * XmlSerializer code if the input had another <g> or <x> element in it.
			 */
			var tuv = e.ObjectBeingDeserialized as XLiffTransUnitVariant;
			if (tuv == null)
			{
				if (e.ObjectBeingDeserialized is XLiffTransUnit &&
					e.Element.LocalName == "alt-trans")
					return; // legal xliff that we totally don't care about.

				Debug.WriteLine(
					$"{e.ObjectBeingDeserialized.GetType()} being deserialized: UnknownElement OuterXml={e.Element.OuterXml}");
				return;
			}

			// Only <g></g> and <x/> elements can be encountered since that's all the xliff standard allows
			// inside <source> and <target> elements (other than text of course). <g> elements have internal
			// content while <x> elements do not. The expected attributes are the same for both <g> and <x>
			// elements.
			var bldr = new StringBuilder();
			bldr.Append(tuv.Value);
			// Most common HTML elements are marked by ctype="x-html-NAME". img elements are an exception,
			// marked by ctype="image".
			// ENHANCE: more possibilities exist for ctype, but this covers by far the most common ones.
			var ctype = e.Element.GetAttribute("ctype");
			if (ctype.StartsWith("x-html-"))
			{
				ctype = ctype.Substring(7);
			}
			else if (ctype == "image")
			{
				ctype = "img";
			}
			else
			{
				Debug.WriteLine(
					$"XLiffTransUnitVariant being deserialized: UnknownElement OuterXml={e.Element.OuterXml}");
				return;
			}

			bldr.AppendFormat("<{0}", ctype);
			for (int i = 0; i < e.Element.Attributes.Count; ++i)
			{
				// HTML element attributes are reproduced with a leading html: namespace tag.
				// All other attributes can be ignored at this point.
				var attr = e.Element.Attributes[i];
				if (attr.Name.StartsWith("html:"))
					bldr.AppendFormat(" {0}=\"{1}\"", attr.LocalName, attr.Value);
			}

			// ENHANCE: handle nested <g> (and <x> inside <g>) elements. Perhaps a recursive method?
			if (e.Element.Name == "g")
				bldr.AppendFormat(">{0}</{1}>",
					e.Element.InnerText.Replace("\\n", Environment.NewLine), ctype);
			else
				bldr.Append("/>");
			// We can't just set tuv.Value from here because it would get wiped out by a following text node.
			// The string would end up equal to just the content of the last text node if the overall content
			// ends in a text node. So we let the Value getter code in the XLiffTransUnitVariant figure things
			// out properly. (Note that what we store here is the entire string so far, not just what we
			// obtained from the current element.)
			tuv.SaveDeserializationFromElement(bldr.ToString());
		}

		#endregion
	}
}
