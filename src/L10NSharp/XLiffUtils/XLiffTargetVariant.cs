// ---------------------------------------------------------------------------------------------
#region
// Copyright © 2017-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#endregion
//
// File: XLiffTargetVariant.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Xml.Serialization;

namespace L10NSharp.XLiffUtils
{
	#region XLiffTargetVariant class

	/// ----------------------------------------------------------------------------------------
	[XmlType("variant", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
	public class XLiffTargetVariant : XLiffBaseWithNotesAndProps
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
