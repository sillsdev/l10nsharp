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
// File: TMXNote.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Localization.TMXUtils
{
	#region TMXNote class
	/// ----------------------------------------------------------------------------------------
	[XmlType("note")]
	public class TMXNote
	{
		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the lang.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("xml:lang")]
		public string Lang { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the note's content.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlText]
		public string Text { get; set; }

		#endregion

		#region Methods
		/// ------------------------------------------------------------------------------------
		public TMXNote Copy()
		{
			return new TMXNote
			{
				Lang = Lang,
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
			get { return (string.IsNullOrEmpty(Lang) && string.IsNullOrEmpty(Text)); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			return (IsEmpty ? "Empty" : (string.IsNullOrEmpty(Lang) ?
				string.Empty : Lang + ": ") + Text);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static bool AddNote(string text, List<TMXNote> noteList)
		{
			return AddNote(null, text, noteList);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static bool AddNote(string lang, string text, List<TMXNote> noteList)
		{
			var note = new TMXNote();
			note.Text = text;
			note.Lang = lang;
			return AddNote(note, noteList);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static bool AddNote(TMXNote note, List<TMXNote> noteList)
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
