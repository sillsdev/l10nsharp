using System;

namespace L10NSharp.TMXUtils
{
	#region TMXWriter class
	/// ----------------------------------------------------------------------------------------
	public class TMXWriter
	{
		//private XmlTextWriter m_xmlWriter;
		/// ------------------------------------------------------------------------------------
		public string TMXFile { get; private set; }

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Asks the user to specify a TMX file for saving TMX information.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public static string AskForTMXFileAndWrite(TMXDocument tmxDoc)
		//{
		//    using (SaveFileDialog dlg = new SaveFileDialog())
		//    {
		//        dlg.OverwritePrompt = true;
		//        dlg.Title = Properties.Resources.kstidTMXSFDCaption;
		//        dlg.Filter = Properties.Resources.kstidTMXOpenAndSaveDlgFilter;
		//        DialogResult result = dlg.ShowDialog();
		//        if (result == DialogResult.Cancel)
		//            return null;

		//        Write(tmxDoc, dlg.FileName);
		//        return dlg.FileName;
		//    }
		//}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Writes the specified TMXDocument information to the specified TMX file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void Write(TMXDocument tmxDoc, string tmxFile)
		{
			if (tmxDoc == null)
				throw new ArgumentNullException("tmxDoc");

			TMXXmlSerializationHelper.SerializeToFile(tmxFile, tmxDoc);
		}

	//    /// ------------------------------------------------------------------------------------
	//    /// <summary>
	//    /// Writes the specified TMXDocument information to the specified TMX file.
	//    /// </summary>
	//    /// ------------------------------------------------------------------------------------
	//    public void Write(TMXDocument tmxDoc, string tmxFile)
	//    {
	//        if (tmxDoc == null)
	//            throw new ArgumentNullException("doc");

	//        TMXFile = tmxFile;

	//        using (m_xmlWriter = new XmlTextWriter(TMXFile, Encoding.UTF8))
	//        {
	//            m_xmlWriter.Formatting = Formatting.Indented;
	//            m_xmlWriter.IndentChar = '\t';
	//            m_xmlWriter.Indentation = 1;
	//            m_xmlWriter.WriteStartDocument();
	//            m_xmlWriter.WriteStartElement(TMXTags.kTagRoot);
	//            m_xmlWriter.WriteStartAttribute(TMXTags.kTagVersion);
	//            m_xmlWriter.WriteString("1.4");
	//            m_xmlWriter.WriteEndAttribute();
	//            WriteHeader(tmxDoc);
	//            WriteBody(tmxDoc);
	//            m_xmlWriter.WriteEndElement();
	//            m_xmlWriter.Close();
	//        }
	//    }

	//    /// ------------------------------------------------------------------------------------
	//    /// <summary>
	//    /// Writes the header.
	//    /// </summary>
	//    /// ------------------------------------------------------------------------------------
	//    private void WriteHeader(TMXDocument tmxDoc)
	//    {
	//        m_xmlWriter.WriteStartElement(TMXTags.kTagHdr);

	//        m_xmlWriter.WriteStartAttribute(TMXTags.kTagCreationTool);
	//        m_xmlWriter.WriteString(tmxDoc.CreationTool);
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(TMXTags.kTagCreationToolVer);
	//        m_xmlWriter.WriteString(tmxDoc.CreationToolVersion);
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(TMXTags.kTagSegType);
	//        m_xmlWriter.WriteString(tmxDoc.SegmentType.ToString());
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(TMXTags.kTagOrigTransMemFmt);
	//        m_xmlWriter.WriteString(tmxDoc.OrigTransMemFmt);
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(TMXTags.kTagAdminLang);
	//        m_xmlWriter.WriteString(tmxDoc.AdminLang);
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(TMXTags.kTagSrcLang);
	//        m_xmlWriter.WriteString(tmxDoc.SourceLang);
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(TMXTags.kTagDataType);
	//        m_xmlWriter.WriteString(tmxDoc.DataType);
	//        m_xmlWriter.WriteEndAttribute();

	//        WriteNotes(tmxDoc.Notes);
	//        m_xmlWriter.WriteEndElement();
	//    }

	//    /// ------------------------------------------------------------------------------------
	//    /// <summary>
	//    /// Writes the body.
	//    /// </summary>
	//    /// ------------------------------------------------------------------------------------
	//    private void WriteBody(TMXDocument tmxDoc)
	//    {
	//        m_xmlWriter.WriteStartElement(TMXTags.kTagBody);

	//        foreach (TMXTransUnit tu in tmxDoc.TransUnits.Values)
	//        {
	//            m_xmlWriter.WriteStartElement(TMXTags.kTagTransUnit);

	//            if (!string.IsNullOrEmpty(tu.Id))
	//            {
	//                m_xmlWriter.WriteStartAttribute(TMXTags.kTagTransUnitId);
	//                m_xmlWriter.WriteString(tu.Id);
	//                m_xmlWriter.WriteEndAttribute();
	//            }

	//            WriteNotes(tu.Notes);
	//            WriteVariants(tu);
	//            m_xmlWriter.WriteEndElement();
	//        }

	//        m_xmlWriter.WriteEndElement();
	//    }

	//    /// ------------------------------------------------------------------------------------
	//    /// <summary>
	//    /// Writes the variants from the specified translation unit.
	//    /// </summary>
	//    /// ------------------------------------------------------------------------------------
	//    private void WriteVariants(TMXTransUnit tu)
	//    {
	//        foreach (TMXTransUnitVariant tuv in tu.Variants.Values)
	//        {
	//            if (tuv.IsEmpty)
	//                continue;

	//            m_xmlWriter.WriteStartElement(TMXTags.kTagTransUnitVariant);

	//            m_xmlWriter.WriteStartAttribute(TMXTags.kTagLang);
	//            m_xmlWriter.WriteString(tuv.Lang);
	//            m_xmlWriter.WriteEndAttribute();

	//            WriteNotes(tuv.Notes);

	//            m_xmlWriter.WriteStartElement(TMXTags.kTagSegment);
	//            m_xmlWriter.WriteString(tuv.Value);
	//            m_xmlWriter.WriteEndElement();

	//            m_xmlWriter.WriteEndElement();
	//        }
	//    }

	//    /// ------------------------------------------------------------------------------------
	//    /// <summary>
	//    /// Writes the specified list of notes.
	//    /// </summary>
	//    /// ------------------------------------------------------------------------------------
	//    private void WriteNotes(List<TMXNote> notes)
	//    {
	//        if (notes == null)
	//            return;

	//        foreach (TMXNote note in notes)
	//        {
	//            if (note.IsEmpty)
	//                continue;

	//            m_xmlWriter.WriteStartElement(TMXTags.kTagNote);

	//            if (!string.IsNullOrEmpty(note.Lang))
	//            {
	//                m_xmlWriter.WriteStartAttribute(TMXTags.kTagLang);
	//                m_xmlWriter.WriteString(note.Lang);
	//                m_xmlWriter.WriteEndAttribute();
	//            }

	//            m_xmlWriter.WriteString(note.Text);
	//            m_xmlWriter.WriteEndElement();
	//        }
	//    }
	}

	#endregion
}
