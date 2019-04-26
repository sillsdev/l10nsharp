// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2009, SIL International. All Rights Reserved.
// <copyright from='2009' to='2009' company='SIL International'>
//		Copyright (c) 2009, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: TMXHeader.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;

namespace L10NSharp.TMXUtils
{
	#region TMXHeader class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// TMX header
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[XmlType("header")]
	public class TMXHeader : TMXBaseWithNotesAndProps
	{
		/// ------------------------------------------------------------------------------------
		public TMXHeader()
		{
			OrigTransMemFmt = "PalasoTMXUtils";
			DataType = TMXTags.TMXDataType.unknown.ToString();
			SegmentType = TMXTags.TMXSegType.block.ToString();
			CreationToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			CreationTool = "PalasoTMXUtils";
			SourceLang = "en";
			AdminLang = "en";
		}

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the source language found in the TMX file's header.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("srclang")]
		public string SourceLang { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the default language for the administrative and informative
		/// elements 'note' and 'prop'.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("adminlang")]
		public string AdminLang { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the tool that created the translation memory file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("creationtool")]
		public string CreationTool { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the version of the tool that created the translation memory file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("creationtoolversion")]
		public string CreationToolVersion { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the segment type used for translation units (tu) when
		/// translation units don't specify a segment type.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("segtype")]
		public string SegmentType { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the type of data.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("datatype")]
		public string DataType { get; set; }


		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the original translation memory format which specifies the
		/// format of the translation memory file from which the TMX document or segment thereof
		/// have been generated.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("o-tmf")]
		public string OrigTransMemFmt { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of translation notes in the document header.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("note")]
		public List<TMXNote> Notes
		{
			get { return _notes; }
			set { _notes = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of props in the translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("prop")]
		public List<TMXProp> Props
		{
			get { return _props; }
			set { _props = value; }
		}

		#endregion
	}

	#endregion
}
