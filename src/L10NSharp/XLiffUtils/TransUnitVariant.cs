// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2009-2017, SIL International. All Rights Reserved.
// <copyright from='2009' to='2017' company='SIL International'>
//		Copyright (c) 2009-2017, SIL International. All Rights Reserved.
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
	[XmlType("source", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
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
        /// Gets or sets the value of the translation unit variant.
        /// </summary>
        /// ------------------------------------------------------------------------------------
        [XmlText]
		public string Value { get; set; }

		#endregion
	}

	#endregion
}
