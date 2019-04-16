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
using System.Collections.Generic;

namespace L10NSharp.XLiffUtils
{
	#region XliffHeader class

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Xliff header
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[XmlType("header", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
	public class XLiffHeader
	{
		/// ------------------------------------------------------------------------------------
		public XLiffHeader()
		{
			Notes = new List<XLiffNote>();
		}

		#region Properties

		[XmlElement("note")]
		public List<XLiffNote> Notes { get; private set; }

		#endregion
	}

	#endregion
}
