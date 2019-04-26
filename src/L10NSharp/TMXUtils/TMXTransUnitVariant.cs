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
// File: TMXTransUnitVariant.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml.Serialization;

namespace L10NSharp.TMXUtils
{
	#region TMXTransUnitVariant class
	/// ----------------------------------------------------------------------------------------
	[XmlType("tuv")]
	public class TMXTransUnitVariant : TMXBaseWithNotesAndProps, ITransUnitVariant
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
		/// Gets or sets the type of the data.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("datatype")]
		public string DataType { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of translation notes in the variant.
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
		/// Gets the list of props in the variant.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("prop")]
		public List<TMXProp> Props
		{
			get { return _props; }
			set { _props = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the seg. (this is really just for serializing/deserializing).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("seg")]
		public TMXSegment Seg { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the value of the translation unit variant.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public string Value
		{
			get { return (Seg == null ? null : Seg.Value); }
			set
			{
				if (Seg == null)
					Seg = new TMXSegment();

				Seg.Value = value;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public bool IsEmpty
		{
			get { return (string.IsNullOrEmpty(Value) && Notes.Count == 0 && Props.Count == 0); }
		}

		#endregion

		#region Other Methods
		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Copies to this instance the information from the specified translation unit.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public void Copy(TMXTransUnitVariant tuv)
		//{
		//    Value = tuv.Value;
		//    Lang = tuv.Lang;
		//    DataType = tuv.DataType;

		//    m_notes = (from note in tuv.Notes
		//               select new TMXNote { Text = note.Text, Lang = note.Lang }).ToList();

		//    m_props = (from prop in tuv.Props
		//               select new TMXProp { Type = prop.Type, Value = prop.Value, Lang = prop.Lang }).ToList();
		//}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Clears all the value. Using the setter to assign the Value property will append
		/// to the existing values. That is because a translation unit may have more than
		/// one 'seg' element.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void ClearValue()
		{
			Seg.ClearValue();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			return (IsEmpty ? "Empty" : (string.IsNullOrEmpty(Lang) ?
				string.Empty : Lang + ": ") + Value);
		}

		#endregion
	}

	#endregion
}
