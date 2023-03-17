// ---------------------------------------------------------------------------------------------
#region
// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#endregion
//
// File: XLiffFile.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;
using System.Linq;

namespace L10NSharp.XLiffUtils
{
	#region XLiffFile class

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// XLiff file
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[XmlType("file", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
	public class XLiffFile : XLiffBaseWithNotesAndProps
	{
		/// ------------------------------------------------------------------------------------
		public XLiffFile()
		{
			SourceLang = LocalizationManager.kDefaultLang;
			ProductVersion = "0.0.0";
			DataType = "plaintext";
		}

		protected XLiffHeader _header /* = new XLiffHeader()*/;
		protected XLiffBody   _body = new XLiffBody();

		#region Properties

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of translation notes in the document header.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("header")]
		public XLiffHeader Header
		{
			get { return _header; }
			set { _header = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the body.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("body")]
		public XLiffBody Body
		{
			get { return _body; }
			set { _body = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the source language found in the XLiff file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("source-language")]
		public string SourceLang { get; set; }


		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the target language found in the XLiff file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("target-language")]
		public string TargetLang { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the product version found in the XLiff file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("product-version")]
		public string ProductVersion { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the dll or executable file name.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("original")]
		public string Original { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the type of data.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("datatype")]
		public string DataType { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the hard linebreak replacement string.  This is the literal value displayed
		/// to the translator (in the L10nSharp GUI) to indicate a hard linebreak in the source text.  It
		/// defaults to \n.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("hard-linebreak-replacement", Namespace =
			XliffXmlSerializationHelper.kSilNamespace),
		DefaultValue(LocalizedStringCache.kDefaultNewlineReplacement)]
		public string HardLineBreakReplacement { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the ampersand (&amp;) replacement string.  This is the literal value displayed
		/// to the translator (in the L10nSharp GUI) to indicate an ampersand in the source text.  It
		/// defaults to |amp|.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("ampersand-replacement", Namespace =
			XliffXmlSerializationHelper.kSilNamespace),
		DefaultValue(LocalizedStringCache.kDefaultAmpersandReplacement)]
		public string AmpersandReplacement { get; set; }

		#endregion
	}

	#endregion
}
