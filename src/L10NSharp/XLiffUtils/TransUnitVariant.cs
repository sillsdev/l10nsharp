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
// File: TransUnitVariant.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml.Serialization;

namespace L10NSharp.XLiffUtils
{
	#region TransUnitVariant class
	/// ----------------------------------------------------------------------------------------
	[XmlType("source")]
	public class TransUnitVariant : XLiffBaseWithNotesAndProps
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
		public List<XLiffNote> Notes
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
		public List<XLiffProp> Props
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
		public XLiffSegment Seg { get; set; }

	    private string _value;
        /// ------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the value of the translation unit variant.
        /// </summary>
        /// ------------------------------------------------------------------------------------
        [XmlText]
        public string Value
        {
            get { return _value; }
            set { _value = value; }
            //get { return (Seg == null ? null : Seg.Value); }
            //set
            //{
            //	if (Seg == null)
            //		Seg = new XLiffSegment();

            //	Seg.Value = value;
            //}
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
		//public void Copy(TransUnitVariant tuv)
		//{
		//    Value = tuv.Value;
		//    Lang = tuv.Lang;
		//    DataType = tuv.DataType;

		//    m_notes = (from note in tuv.Notes
		//               select new XLiffNote { Text = note.Text, Lang = note.Lang }).ToList();

		//    m_props = (from prop in tuv.Props
		//               select new XLiffProp { Type = prop.Type, Value = prop.Value, Lang = prop.Lang }).ToList();
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
