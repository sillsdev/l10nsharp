// ---------------------------------------------------------------------------------------------
#region // Copyright © 2009-2025 SIL Global
// <copyright from='2009' to='2025' company='SIL Global'>
//		Copyright © 2009-2025 SIL Global
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: XLiffTransUnitVariant.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml.Serialization;

namespace L10NSharp.XLiffUtils
{
	#region XLiffTransUnitVariant class

	/// ----------------------------------------------------------------------------------------
	[XmlType("source", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
	public class XLiffTransUnitVariant : XLiffBaseWithNotesAndProps, ITransUnitVariant
	{
		public XLiffTransUnitVariant()
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
			[XmlEnumAttribute("undefined")] Undefined,

			[XmlEnumAttribute("translated")] // Indicates that the item has been translated.
			Translated,

			[XmlEnumAttribute(
				"needs-translation")]
			// Indicates that the item needs to be translated.
			NeedsTranslation,

			[XmlEnumAttribute("final")] // Indicates the terminating state.
			Final,

			[XmlEnumAttribute(
				"needs-adaptation")]
			//Indicates only non-textual information needs adaptation.
			NeedsAdaptation,

			[XmlEnumAttribute(
				"needs-l10n")]
			// Indicates both text and non-textual information needs adaptation.
			NeedsLocalization,

			[XmlEnumAttribute(
				"needs-review-adaptation")]
			// Indicates only non-textual information needs review.
			AdaptationNeedsReview,

			[XmlEnumAttribute(
				"needs-review-l10n")]
			// Indicates both text and non-textual information needs review.
			LocalizationNeedsReview,

			[XmlEnumAttribute(
				"needs-review-translation")]
			// Indicates that only the text of the item needs to be reviewed.
			TranslationNeedsReview,

			[XmlEnumAttribute(
				"new")]
			// Indicates that the item is new. For example, translation units that were not in a previous version of the document.
			New,

			[XmlEnumAttribute("signed-off")] // Indicates that changes are reviewed and approved.
			SignedOff
		};

		/// <summary>
		/// The state of a target element.
		/// </summary>
		[XmlAttribute("state"), System.ComponentModel.DefaultValue(TranslationState.Undefined)]
		public TranslationState TargetState;

		private string _value;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the value of the translation unit variant.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlText]
		public string Value
		{
			get
			{
				if (string.IsNullOrEmpty(_value) &&
					string.IsNullOrEmpty(_deserializedFromElement))
					return string.Empty;

				if (!string.IsNullOrEmpty(_deserializedFromElement))
				{
					// See the extended comment in  XliffXmlSerializationHelper.deserializer_UnknownElement()
					// to explain better why this code is needed.
					if (_value == null)
						_value =
							_deserializedFromElement; // last thing deserialized was an element
					else
						_value = _deserializedFromElement +
								_value; // last thing deserialized was a text node following an element.
					_deserializedFromElement = null;
				}

				return _value;
			}
			set { _value = value; }
		}

		/// <summary>
		/// This is a temp value that allows complex input strings (with xliff-style html markup)
		/// to be deserialized properly.
		/// </summary>
		private string _deserializedFromElement;

		/// <summary>
		/// Save the value deserialized from an element (and anything preceding it), and clear
		/// the Value.  The getter will use what we store here the next time it is accessed.
		/// This is needed because the XmlSerializer for XmlText just stores the content of
		/// each text node it encounters, ignoring whatever may already be there.
		/// The deserialization stored here should look like it has HTML markup since that's
		/// all that we have represented this way in the xliff files.
		/// </summary>
		/// <remarks>
		/// We are shifting toward using markdown if possible since it is easier for translators
		/// to deal with.  But at the moment, only a small subset of markdown in handled, and
		/// then only in the context of translating content for TSX.  But this may still be
		/// needed/wanted for some items.
		/// </remarks>
		internal void SaveDeserializationFromElement(string value)
		{
			_value = null;
			_deserializedFromElement = value;
		}

		#endregion
	}

	#endregion
}
