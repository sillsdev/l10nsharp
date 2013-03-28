using System;

namespace L10NSharp.TMXUtils
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public static class TMXTags
	{
		#region TMX element tags: http://www.lisa.org/fileadmin/standards/tmx1.4/tmx.htm
		// Structural elements
		/// ------------------------------------------------------------------------------------
		public const string kTagRoot = "tmx";
		/// ------------------------------------------------------------------------------------
		public const string kTagHdr = "header";
		/// ------------------------------------------------------------------------------------
		public const string kTagBody = "body";
		/// ------------------------------------------------------------------------------------
		public const string kTagNote = "note";
		/// ------------------------------------------------------------------------------------
		public const string kTagUserDefEnc = "ude";
		/// ------------------------------------------------------------------------------------
		public const string kTagProp = "prop";
		/// ------------------------------------------------------------------------------------
		public const string kTagMap = "map";
		/// ------------------------------------------------------------------------------------
		public const string kTagSegment = "seg";
		/// ------------------------------------------------------------------------------------
		public const string kTagTransUnit = "tu";
		/// ------------------------------------------------------------------------------------
		public const string kTagTransUnitVariant = "tuv";

		// Inline elements (Currently not supported in this library. My understanding is
		// that these are probably not useful for our purposes in SIL. However, to fully
		// conform to the TMX standard, we may want to consider supporting them, even if
		// we never use them for anything we do.)
		/// ------------------------------------------------------------------------------------
		public const string kTagBegPaired = "bpt";
		/// ------------------------------------------------------------------------------------
		public const string kTagEndPaired = "ept";
		/// ------------------------------------------------------------------------------------
		public const string kTagHighlight = "hi";
		/// ------------------------------------------------------------------------------------
		public const string kTagIsolated = "it";
		/// ------------------------------------------------------------------------------------
		public const string kTagPlaceholder = "ph";
		/// ------------------------------------------------------------------------------------
		public const string kTagSubflow = "sub";
		/// ------------------------------------------------------------------------------------
		public const string kTagUnknown = "ut";

		#endregion

		#region TMX attribute tags
		/// ------------------------------------------------------------------------------------
		public const string kTagTransUnitId = "tuid";
		/// ------------------------------------------------------------------------------------
		public const string kTagCreationDate = "creationdate";
		/// ------------------------------------------------------------------------------------
		public const string kTagCreationId = "creationid";
		/// ------------------------------------------------------------------------------------
		public const string kTagCreationTool = "creationtool";
		/// ------------------------------------------------------------------------------------
		public const string kTagCreationToolVer = "creationtoolversion";
		/// ------------------------------------------------------------------------------------
		public const string kTagLastModDate = "changedate";
		/// ------------------------------------------------------------------------------------
		public const string kTagLastModId = "changeid";
		/// ------------------------------------------------------------------------------------
		public const string kTagType = "type";
		/// ------------------------------------------------------------------------------------
		public const string kTagDataType = "datatype";
		/// ------------------------------------------------------------------------------------
		public const string kTagSegType = "segtype";
		/// ------------------------------------------------------------------------------------
		public const string kTagCode = "code";
		/// ------------------------------------------------------------------------------------
		public const string kTagSubstitutionText = "subst";
		/// ------------------------------------------------------------------------------------
		public const string kTagEntity = "ent";
		/// ------------------------------------------------------------------------------------
		public const string kTagBaseEnc = "base";
		/// ------------------------------------------------------------------------------------
		public const string kTagUsageCount = "usagecount";
		/// ------------------------------------------------------------------------------------
		public const string kTagLastUsageDate = "lastusagedate";
		/// ------------------------------------------------------------------------------------
		public const string kTagName = "name";
		/// ------------------------------------------------------------------------------------
		public const string kTagIntMatching = "i";
		/// ------------------------------------------------------------------------------------
		public const string kTagExtMatching = "x";
		/// ------------------------------------------------------------------------------------
		public const string kTagPosition = "pos";
		/// ------------------------------------------------------------------------------------
		public const string kTagAssociation = "assoc";
		/// ------------------------------------------------------------------------------------
		public const string kTagUnicode = "unicode";
		/// ------------------------------------------------------------------------------------
		public const string kTagVersion = "version";
		/// ------------------------------------------------------------------------------------
		public const string kTagOrigTransMemFmt = "o-tmf";
		/// ------------------------------------------------------------------------------------
		public const string kTagAdminLang = "adminlang";
		/// ------------------------------------------------------------------------------------
		public const string kTagLang = "xml:lang";
		/// ------------------------------------------------------------------------------------
		public const string kTagOrigEnc = "o-encoding";
		/// ------------------------------------------------------------------------------------
		public const string kTagSrcLang = "srclang";

		#endregion

		#region datatype values
		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public enum TMXDataType
		{
			///<summary>undefined</summary>
			unknown,
			///<summary>WinJoust data</summary>
			alptext,
			///<summary>Channel Definition Format</summary>
			cdf,
			///<summary>Corel CMX Format</summary>
			cmx,
			///<summary>C and C++ style text</summary>
			cpp,
			///<summary>HP-Tag</summary>
			hptag,
			///<summary>HTML, DHTML, etc</summary>
			html,
			///<summary>Interleaf documents</summary>
			interleaf,
			///<summary>IPF/BookMaster</summary>
			ipf,
			///<summary>Java, source and property files</summary>
			java,
			///<summary>JavaScript, ECMAScript scripts</summary>
			javascript,
			///<summary>Lisp</summary>
			lisp,
			///<summary>Framemaker MIF, MML, etc</summary>
			mif,
			///<summary>OpenTag data</summary>
			opentag,
			///<summary>Pascal, Delphi style text</summary>
			pascal,
			///<summary>Plain text</summary>
			plaintext,
			///<summary>PageMaker</summary>
			pm,
			///<summary>Rich Text Format</summary>
			rtf,
			///<summary>SGML</summary>
			sgml,
			///<summary>S-Tagger for FrameMaker</summary>
			stf_f,
			///<summary>S-Tagger for Interleaf</summary>
			stf_i,
			///<summary>Transit data</summary>
			transit,
			///<summary>Visual Basic scripts</summary>
			vbscript,
			///<summary>Windows resources from RC, DLL, EXE</summary>
			winres,
			///<summary>XML</summary>
			xml,
			///<summary>Quark XPressTag</summary>
			xptag,
		}

		#endregion

		#region type values
		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public enum TMXType
		{
			///<summary>Undefined type</summary>
			undefined,
			///<summary>Bold</summary>
			bold,
			///<summary>Color change</summary>
			color,
			///<summary>Doubled-underlined</summary>
			dulined,
			///<summary>Font change</summary>
			font,
			///<summary>Italic</summary>
			italic,
			///<summary>Linked text</summary>
			link,
			///<summary>Small caps</summary>
			scap,
			///<summary>XML/SGML structure</summary>
			strct,
			///<summary>Underlined</summary>
			ulined,
			///<summary>Index marker</summary>
			index,
			///<summary>Date</summary>
			date,
			///<summary>Time</summary>
			time,
			///<summary>Footnote</summary>
			fnote,
			///<summary>End-note</summary>
			enote,
			///<summary>Alternate text</summary>
			alt,
			///<summary>Image</summary>
			image,
			///<summary>Page break</summary>
			pb,
			///<summary>Line break</summary>
			lb,
			///<summary>Column break</summary>
			cb,
			///<summary>Inset</summary>
			inset,
		}

		#endregion

		#region segment types
		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public enum TMXSegType
		{
			/// <summary></summary>
			undefined,
			/// <summary></summary>
			block,
			/// <summary></summary>
			paragraph,
			/// <summary></summary>
			sentence,
			/// <summary></summary>
			phrase
		}

		#endregion

		//private static Dictionary<TMXDataType, string> s_dataTypes;
		//private static Dictionary<TMXType, string> s_types;

		#region Construction
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes the <see cref="TMXTags"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		static TMXTags()
		{
			//s_dataTypes = new Dictionary<TMXDataType, string>();
			//s_types = new Dictionary<TMXType, string>();

			//s_dataTypes[TMXDataType.unknown] = "undefined";
			//s_dataTypes[TMXDataType.alptext] = "WinJoust data";
			//s_dataTypes[TMXDataType.cdf] = "Channel Definition Format";
			//s_dataTypes[TMXDataType.cmx] = "Corel CMX Format";
			//s_dataTypes[TMXDataType.cpp] = "C and C++ style text";
			//s_dataTypes[TMXDataType.hptag] = "HP-Tag";
			//s_dataTypes[TMXDataType.html] = "HTML, DHTML, etc";
			//s_dataTypes[TMXDataType.interleaf] = "Interleaf documents";
			//s_dataTypes[TMXDataType.ipf] = "IPF/BookMaster";
			//s_dataTypes[TMXDataType.java] = "Java, source and property files";
			//s_dataTypes[TMXDataType.javascript] = "JavaScript, ECMAScript scripts";
			//s_dataTypes[TMXDataType.lisp] = "Lisp";
			//s_dataTypes[TMXDataType.mif] = "Framemaker MIF, MML, etc";
			//s_dataTypes[TMXDataType.opentag] = "OpenTag data";
			//s_dataTypes[TMXDataType.pascal] = "Pascal, Delphi style text";
			//s_dataTypes[TMXDataType.plaintext] = "Plain text";
			//s_dataTypes[TMXDataType.pm] = "PageMaker";
			//s_dataTypes[TMXDataType.rtf] = "Rich Text Format";
			//s_dataTypes[TMXDataType.sgml] = "SGML";
			//s_dataTypes[TMXDataType.stf_f] = "S-Tagger for FrameMaker";
			//s_dataTypes[TMXDataType.stf_i] = "S-Tagger for Interleaf";
			//s_dataTypes[TMXDataType.transit] = "Transit data";
			//s_dataTypes[TMXDataType.vbscript] = "Visual Basic scripts";
			//s_dataTypes[TMXDataType.winres] = "Windows resources from RC, DLL, EXE";
			//s_dataTypes[TMXDataType.xml] = "XML";
			//s_dataTypes[TMXDataType.xptag] = "Quark XPressTag";

			//s_types[TMXType.bold] = "Bold";
			//s_types[TMXType.color] = "Color change";
			//s_types[TMXType.dulined] = "Doubled-underlined";
			//s_types[TMXType.font] = "Font change";
			//s_types[TMXType.italic] = "Italic";
			//s_types[TMXType.link] = "Linked text";
			//s_types[TMXType.scap] = "Small caps";
			//s_types[TMXType.strct] = "XML/SGML structure";
			//s_types[TMXType.ulined] = "Underlined";
			//s_types[TMXType.index] = "Index marker";
			//s_types[TMXType.date] = "Date";
			//s_types[TMXType.time] = "Time";
			//s_types[TMXType.fnote] = "Footnote";
			//s_types[TMXType.enote] = "End-note";
			//s_types[TMXType.alt] = "Alternate text";
			//s_types[TMXType.image] = "Image";
			//s_types[TMXType.pb] = "Page break";
			//s_types[TMXType.lb] = "Line break";
			//s_types[TMXType.cb] = "Column break";
			//s_types[TMXType.inset] = "Inset";
		}

		#endregion

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets the data type description.
		///// </summary>
		///// <param name="datatype">The datatype.</param>
		///// <returns></returns>
		///// ------------------------------------------------------------------------------------
		//public static string GetDataTypeDescription(TMXDataType datatype)
		//{
		//    return s_dataTypes[datatype];
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets the data type description.
		///// </summary>
		///// <param name="datatype">The datatype.</param>
		///// <returns></returns>
		///// ------------------------------------------------------------------------------------
		//public static string GetDataTypeDescription(string datatype)
		//{
		//    return s_dataTypes[GetDataType(datatype)];
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets the type of the data.
		///// </summary>
		///// <param name="datatype">The datatype name as a string.</param>
		///// ------------------------------------------------------------------------------------
		//public static TMXDataType GetDataType(string datatype)
		//{
		//    datatype = datatype.Replace('_', '-');
		//    return (string.IsNullOrEmpty(datatype) || !Enum.IsDefined(typeof(TMXDataType), datatype) ?
		//        TMXDataType.unknown : (TMXDataType)Enum.Parse(typeof(TMXDataType), datatype));
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets the type description.
		///// </summary>
		///// <param name="type">The type.</param>
		///// ------------------------------------------------------------------------------------
		//public static string GetTypeDescription(TMXType type)
		//{
		//    return s_types[type];
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets the type description.
		///// </summary>
		///// <param name="type">The type.</param>
		///// ------------------------------------------------------------------------------------
		//public static string GetTypeDescription(string type)
		//{
		//    return s_types[GetType(type)];
		//}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets the type of the data.
		///// </summary>
		///// <param name="type">The datatype name as a string.</param>
		///// ------------------------------------------------------------------------------------
		//public static TMXType GetType(string type)
		//{
		//    if (type == "struct")
		//        return TMXType.strct;

		//    return (string.IsNullOrEmpty(type) || !Enum.IsDefined(typeof(TMXType), type) ?
		//        TMXType.undefined : (TMXType)Enum.Parse(typeof(TMXType), type));
		//}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the segment type.
		/// </summary>
		/// <param name="segtype">The segment type as a string.</param>
		/// ------------------------------------------------------------------------------------
		public static TMXSegType GetSegmentType(string segtype)
		{
			return (string.IsNullOrEmpty(segtype) || !Enum.IsDefined(typeof(TMXSegType), segtype) ?
				TMXSegType.undefined : (TMXSegType)Enum.Parse(typeof(TMXSegType), segtype));
		}
	}
}
