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
		public TransUnitVariant()
		{
			TargetState = TranslationState.Undefined;
		}

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the lang.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("xml:lang")]
		public string Lang { get; set; }

		// Crowdin uses only "needs-translated" and "translated" as far as I can tell.  It appears to remove
		// this attribute ("undefined") and use the "approved" attribute on the the trans-unit element to
		// signal that it's been "reviewed and approved".
		public enum TranslationState
		{
			[XmlEnumAttribute("undefined")]
			Undefined,
			[XmlEnumAttribute("translated")]				// Indicates that the item has been translated.
			Translated,
			[XmlEnumAttribute("needs-translation")]			// Indicates that the item needs to be translated.
			NeedsTranslation,
			[XmlEnumAttribute("final")]						// Indicates the terminating state.
			Final,
			[XmlEnumAttribute("needs-adaptation")]			//Indicates only non-textual information needs adaptation.
			NeedsAdaptation,
			[XmlEnumAttribute("needs-l10n")]				// Indicates both text and non-textual information needs adaptation.
			NeedsLocalization,
			[XmlEnumAttribute("needs-review-adaptation")]	// Indicates only non-textual information needs review.
			AdaptationNeedsReview,
			[XmlEnumAttribute("needs-review-l10n")]			// Indicates both text and non-textual information needs review.
			LocalizationNeedsReview,
			[XmlEnumAttribute("needs-review-translation")]	// Indicates that only the text of the item needs to be reviewed.
			TranslationNeedsReview,
			[XmlEnumAttribute("new")]						// Indicates that the item is new. For example, translation units that were not in a previous version of the document.
			New,
			[XmlEnumAttribute("signed-off")]				// Indicates that changes are reviewed and approved.
			SignedOff
		};

		/// <summary>
		/// The state of a target element.
		/// </summary>
		[XmlAttribute("state"), System.ComponentModel.DefaultValue(TranslationState.Undefined)]
		public TranslationState TargetState;

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
