// ---------------------------------------------------------------------------------------------
#region
// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#endregion
//
// File: TargetVariant.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Xml.Serialization;

namespace L10NSharp.XLiffUtils
{
	#region TargetVariant class
	/// ----------------------------------------------------------------------------------------
	[XmlType("variant")]
	public class TargetVariant : XLiffBaseWithNotesAndProps
	{
		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the lang.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("xml:lang")]
		public string Lang { get; set; }

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
        }

		#endregion
	}

	#endregion
}
