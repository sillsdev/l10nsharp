// ---------------------------------------------------------------------------------------------
#region
// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#endregion
//
// File: XliffHeader.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Xml.Serialization;

namespace L10NSharp.XLiffUtils
{
	#region XliffHeader class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Xliff header
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[XmlType("header", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
	public class XLiffHeader : XLiffBaseWithNotesAndProps
	{
		/// ------------------------------------------------------------------------------------
		public XLiffHeader()
		{

        }

		#region Properties
		protected XLiffNote _note = new XLiffNote();

		[XmlElement("note")]
		public XLiffNote Note
		{
			get { return _note; }
			set { _note = value; }
		}
		#endregion
	}

	#endregion
}
