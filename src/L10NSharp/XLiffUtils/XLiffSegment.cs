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
// File: XLiffSegment.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Xml.Serialization;

namespace L10NSharp.XLiffUtils
{
	#region XLiffSegment class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This handles XLiff segments and just combines all the values from in-line elements. If
	/// support for in-line elements is ever needed, then this class will need to be modified.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[XmlType("seg")]
	public class XLiffSegment
	{
		private string m_seg;

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlText]
		public string Value
		{
			get { return m_seg; }
			set { Append(value); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the BPT.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string bpt
		{
			get { return null; }
			set { Append(value); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the ept.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string ept
		{
			get { return null; }
			set { Append(value); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the hi.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string hi
		{
			get { return null; }
			set { Append(value); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets it.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string it
		{
			get { return null; }
			set { Append(value); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the ph.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string ph
		{
			get { return null; }
			set { Append(value); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the sub.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string sub
		{
			get { return null; }
			set { Append(value); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the ut.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string ut
		{
			get { return null; }
			set { Append(value); }
		}

		#endregion

		#region Methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Appends the specified value to the segment.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void Append(string val)
		{
			if (m_seg == null)
				m_seg = val;
			else
				m_seg += val;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Clears the value.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void ClearValue()
		{
			m_seg = null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			return m_seg;
		}

		#endregion
	}

	#endregion
}
