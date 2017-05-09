using System;

namespace L10NSharp.XLiffUtils
{
	#region XLiffWriter class
	/// ----------------------------------------------------------------------------------------
	public class XLiffWriter
	{
		//private XmlTextWriter m_xmlWriter;
		/// ------------------------------------------------------------------------------------
		public string XLiffFile { get; private set; }

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Asks the user to specify a XLiff file for saving XLiff information.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public static string AskForXLiffFileAndWrite(XLiffDocument XLiffDoc)
		//{
		//    using (SaveFileDialog dlg = new SaveFileDialog())
		//    {
		//        dlg.OverwritePrompt = true;
		//        dlg.Title = Properties.Resources.kstidXLiffSFDCaption;
		//        dlg.Filter = Properties.Resources.kstidXLiffOpenAndSaveDlgFilter;
		//        DialogResult result = dlg.ShowDialog();
		//        if (result == DialogResult.Cancel)
		//            return null;

		//        Write(XLiffDoc, dlg.FileName);
		//        return dlg.FileName;
		//    }
		//}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Writes the specified XLiffDocument information to the specified XLiff file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void Write(XLiffDocument XLiffDoc, string XLiffFile)
		{
			if (XLiffDoc == null)
				throw new ArgumentNullException("XLiffDoc");

            XLiffXmlSerializationHelper.SerializeToFile(XLiffFile, XLiffDoc);
		}

	//    /// ------------------------------------------------------------------------------------
	//    /// <summary>
	//    /// Writes the specified XLiffDocument information to the specified XLiff file.
	//    /// </summary>
	//    /// ------------------------------------------------------------------------------------
	//    public void Write(XLiffDocument XLiffDoc, string XLiffFile)
	//    {
	//        if (XLiffDoc == null)
	//            throw new ArgumentNullException("doc");

	//        XLiffFile = XLiffFile;

	//        using (m_xmlWriter = new XmlTextWriter(XLiffFile, Encoding.UTF8))
	//        {
	//            m_xmlWriter.Formatting = Formatting.Indented;
	//            m_xmlWriter.IndentChar = '\t';
	//            m_xmlWriter.Indentation = 1;
	//            m_xmlWriter.WriteStartDocument();
	//            m_xmlWriter.WriteStartElement(XLiffTags.kTagRoot);
	//            m_xmlWriter.WriteStartAttribute(XLiffTags.kTagVersion);
	//            m_xmlWriter.WriteString("1.4");
	//            m_xmlWriter.WriteEndAttribute();
	//            WriteHeader(XLiffDoc);
	//            WriteBody(XLiffDoc);
	//            m_xmlWriter.WriteEndElement();
	//            m_xmlWriter.Close();
	//        }
	//    }

	//    /// ------------------------------------------------------------------------------------
	//    /// <summary>
	//    /// Writes the header.
	//    /// </summary>
	//    /// ------------------------------------------------------------------------------------
	//    private void WriteHeader(XLiffDocument XLiffDoc)
	//    {
	//        m_xmlWriter.WriteStartElement(XLiffTags.kTagHdr);

	//        m_xmlWriter.WriteStartAttribute(XLiffTags.kTagCreationTool);
	//        m_xmlWriter.WriteString(XLiffDoc.CreationTool);
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(XLiffTags.kTagCreationToolVer);
	//        m_xmlWriter.WriteString(XLiffDoc.CreationToolVersion);
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(XLiffTags.kTagSegType);
	//        m_xmlWriter.WriteString(XLiffDoc.SegmentType.ToString());
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(XLiffTags.kTagOrigTransMemFmt);
	//        m_xmlWriter.WriteString(XLiffDoc.OrigTransMemFmt);
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(XLiffTags.kTagAdminLang);
	//        m_xmlWriter.WriteString(XLiffDoc.AdminLang);
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(XLiffTags.kTagSrcLang);
	//        m_xmlWriter.WriteString(XLiffDoc.SourceLang);
	//        m_xmlWriter.WriteEndAttribute();

	//        m_xmlWriter.WriteStartAttribute(XLiffTags.kTagDataType);
	//        m_xmlWriter.WriteString(XLiffDoc.DataType);
	//        m_xmlWriter.WriteEndAttribute();

	//        WriteNotes(XLiffDoc.Notes);
	//        m_xmlWriter.WriteEndElement();
	//    }

	//    /// ------------------------------------------------------------------------------------
	//    /// <summary>
	//    /// Writes the body.
	//    /// </summary>
	//    /// ------------------------------------------------------------------------------------
	//    private void WriteBody(XLiffDocument XLiffDoc)
	//    {
	//        m_xmlWriter.WriteStartElement(XLiffTags.kTagBody);

	//        foreach (TransUnit tu in XLiffDoc.TransUnits.Values)
	//        {
	//            m_xmlWriter.WriteStartElement(XLiffTags.kTagTransUnit);

	//            if (!string.IsNullOrEmpty(tu.Id))
	//            {
	//                m_xmlWriter.WriteStartAttribute(XLiffTags.kTagTransUnitId);
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
	//    private void WriteVariants(TransUnit tu)
	//    {
	//        foreach (TransUnitVariant tuv in tu.Variants.Values)
	//        {
	//            if (tuv.IsEmpty)
	//                continue;

	//            m_xmlWriter.WriteStartElement(XLiffTags.kTagTransUnitVariant);

	//            m_xmlWriter.WriteStartAttribute(XLiffTags.kTagLang);
	//            m_xmlWriter.WriteString(tuv.Lang);
	//            m_xmlWriter.WriteEndAttribute();

	//            WriteNotes(tuv.Notes);

	//            m_xmlWriter.WriteStartElement(XLiffTags.kTagSegment);
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
	//    private void WriteNotes(List<XLiffNote> notes)
	//    {
	//        if (notes == null)
	//            return;

	//        foreach (XLiffNote note in notes)
	//        {
	//            if (note.IsEmpty)
	//                continue;

	//            m_xmlWriter.WriteStartElement(XLiffTags.kTagNote);

	//            if (!string.IsNullOrEmpty(note.Lang))
	//            {
	//                m_xmlWriter.WriteStartAttribute(XLiffTags.kTagLang);
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
