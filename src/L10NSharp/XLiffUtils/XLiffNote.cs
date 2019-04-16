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
		/// Gets or sets the language of the note.  Optional.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("xml:lang")]
		public string NoteLang { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets who wrote the note.  Optional
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("from")]
		public string From { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the priority of the note.  Optional.  If set, 1=high, 10=low.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("priority"), System.ComponentModel.DefaultValue(0)]
		public int Priority { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets which element the note applies to, the source or the target.  Optional.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("annotates")]
		public string Annotates { get; set; }



		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the note's content.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlText]
		public string Text { get; set; }

		#endregion

		#region Methods

		// ------------------------------------------------------------------------------------
		public XLiffNote Copy()
		{
			return new XLiffNote {
				NoteLang = NoteLang,
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
			get { return (string.IsNullOrEmpty(Text)); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			return (IsEmpty
				? "Empty"
				: (string.IsNullOrEmpty(NoteLang) ? string.Empty : NoteLang + ": ") + Text);
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
		public static bool AddNote(string lang, string text, List<XLiffNote> noteList)
		{
			var note = new XLiffNote();
			note.Text = text;
			note.NoteLang = lang;
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
