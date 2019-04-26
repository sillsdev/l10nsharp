using System.Collections.Generic;
using System.Linq;

namespace L10NSharp.TMXUtils
{
	#region TMXBaseWithNotesAndProps class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class serves as the base class for TMX elements that contain notes and props
	/// (i.e. the header, translation unit and translation unit variant elements).
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public abstract class TMXBaseWithNotesAndProps
	{
		/// <summary></summary>
		protected List<TMXNote> _notes = new List<TMXNote>();
		/// <summary></summary>
		protected List<TMXProp> _props = new List<TMXProp>();

		/// ------------------------------------------------------------------------------------
		public List<TMXNote> CopyNotes()
		{
			return _notes.ToList();
		}

		/// ------------------------------------------------------------------------------------
		public List<TMXProp> CopyProps()
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
			return TMXNote.AddNote(text, _notes);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a note to the notes list.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddNote(string lang, string text)
		{
			return TMXNote.AddNote(lang, text, _notes);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a note to the notes list.
		/// </summary>
		/// <param name="note">The note.</param>
		/// <returns>true if the note was added successfully. Otherwise, false.</returns>
		/// ------------------------------------------------------------------------------------
		public bool AddNote(TMXNote note)
		{
			return TMXNote.AddNote(note, _notes);
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
			return TMXProp.AddProp(type, value, _props);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddProp(string lang, string type, string value)
		{
			return TMXProp.AddProp(lang, type, value, _props);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddProp(TMXProp prop)
		{
			return TMXProp.AddProp(prop, _props);
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
	}

	#endregion
}
