using System.Collections.Generic;
using System.Linq;

namespace L10NSharp.XLiffUtils
{
	#region XLiffBaseWithNotesAndProps class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class serves as the base class for Xliff elements that contain notes and props
	/// (i.e. the header, translation unit and translation unit variant elements).
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public abstract class XLiffBaseWithNotesAndProps
	{
		/// <summary></summary>
		protected List<XLiffNote> _notes = new List<XLiffNote>();
		/// <summary></summary>
		protected List<XLiffProp> _props = new List<XLiffProp>();

		/// ------------------------------------------------------------------------------------
		public List<XLiffNote> CopyNotes()
		{
			return _notes.ToList();
		}

		/// ------------------------------------------------------------------------------------
		public List<XLiffProp> CopyProps()
		{
			return _props.ToList();
		}

		#region Methods for adding a note.
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a note to the notes list.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddNote(string text)
		{
			return XLiffNote.AddNote(text, _notes);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a note to the notes list.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddNote(string lang, string text)
		{
			return XLiffNote.AddNote(lang, text, _notes);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a note to the notes list.
		/// </summary>
		/// <param name="note">The note.</param>
		/// <returns>true if the note was added successfully. Otherwise, false.</returns>
		/// ------------------------------------------------------------------------------------
		public bool AddNote(XLiffNote note)
		{
			return XLiffNote.AddNote(note, _notes);
		}

		#endregion

		#region Methods for adding a property
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddProp(string type, string value)
		{
			return XLiffProp.AddProp(type, value, _props);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddProp(string lang, string type, string value)
		{
			return XLiffProp.AddProp(lang, type, value, _props);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddProp(XLiffProp prop)
		{
			return XLiffProp.AddProp(prop, _props);
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the value of the property with the specified type. If a property for the
		/// specified type doesn't exist, then one is added.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetPropValue(string type, string value)
		{
			foreach (var prop in _props.Where(p => p.Type == type))
			{
				prop.Value = value;
				return;
			}

			AddProp(type, value);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the value of the property with the specified type. If a property for the
		/// specified type doesn't exist, then null is returned.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetPropValue(string type)
		{
			return _props.Where(p => p.Type == type).Select(p => p.Value).FirstOrDefault();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Check whether the give comment already exists as a note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool NotesContain(string comment)
		{
			return _notes.Any(n => n.Text == comment);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Return the "comment" string if it exists in the notes.  We use one to represent the
		/// id string (prefacing it with "ID: ").  Any other note is liable to be considered the
		/// "comment" if it exists.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetComment()
		{
			var commentNote = _notes.FirstOrDefault(n => !string.IsNullOrEmpty(n.Text) && !n.Text.StartsWith("ID: "));
			return commentNote?.Text;
		}
	}

	#endregion
}
