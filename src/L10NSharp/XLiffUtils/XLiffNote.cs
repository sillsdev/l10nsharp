// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2009, SIL International. All Rights Reserved.
// <copyright from='2009' to='2017' company='SIL International'>
//		Copyright (c) 2009, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: XLiffNote.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml.Serialization;

namespace L10NSharp.XLiffUtils
{
	#region XLiffNote class
	/// ----------------------------------------------------------------------------------------
	[XmlType("note", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
	public class XLiffNote : XLiffBaseWithNotesAndProps
    {
		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("type")]
		public string Type { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the note's content.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlText]
		public string Text { get; set; }

        /// ------------------------------------------------------------------------------------
        /// <summary>
		/// Gets the list of notes in the document header.
        /// </summary>
        /// ------------------------------------------------------------------------------------
		[XmlElement("note")]
		public List<XLiffNote> Notes
        {
            get { return _notes; }
            set { _notes = value; }
        }
        #endregion

        #region Methods
        // ------------------------------------------------------------------------------------
        public XLiffNote Copy()
        {
            return new XLiffNote
            {
                Type = Type,
                Text = Text
            };
        }

        /// ------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// ------------------------------------------------------------------------------------
        public bool IsEmpty
        {
            get { return (string.IsNullOrEmpty(Type) && string.IsNullOrEmpty(Text)); }
        }

        /// ------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// ------------------------------------------------------------------------------------
        public override string ToString()
        {
            return (IsEmpty ? "Empty" : (string.IsNullOrEmpty(Type) ?
                string.Empty : Type + ": ") + Text);
        }

        /// ------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the note.
        /// </summary>
        /// ------------------------------------------------------------------------------------
        public static bool AddNote(string text, List<XLiffNote> noteList)
        {
            return AddNote(null, text, noteList);
        }

        /// ------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the note.
        /// </summary>
        /// ------------------------------------------------------------------------------------
		public static bool AddNote(string type, string text, List<XLiffNote> noteList)
        {
            var note = new XLiffNote();
            note.Text = text;
			note.Type = type;
            return AddNote(note, noteList);
        }

        /// ------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the note.
        /// </summary>
        /// ------------------------------------------------------------------------------------
        public static bool AddNote(XLiffNote note, List<XLiffNote> noteList)
        {
            if (note == null || note.IsEmpty || noteList == null)
                return false;

            noteList.Add(note);
            return true;
        }

        #endregion
    }

    #endregion
}
