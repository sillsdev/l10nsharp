// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2009-2022, SIL International. All Rights Reserved.
// <copyright from='2009' to='2022' company='SIL International'>
//		Copyright (c) 2009-2022, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: XLiffTransUnit.cs
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml.Serialization;
using static System.String;

namespace L10NSharp.XLiffUtils
{
	#region XLiffTransUnit class

	/// ----------------------------------------------------------------------------------------
	[XmlType("trans-unit", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
	public class XLiffTransUnit : XLiffBaseWithNotesAndProps, ITransUnit
	{
		internal const string kDefaultLangId = "en";

		/// ------------------------------------------------------------------------------------
		public XLiffTransUnit()
		{
			Source = new XLiffTransUnitVariant();
			Target = null;
			TranslationStatus = TranslationStatus.Unapproved;
		}

		#region Properties

		/// ------------------------------------------------------------------------------------
		[XmlAttribute("id")]
		public string Id { get; set; }

		//  approved="yes"

		/// <summary>
		/// The state of a target element.
		/// </summary>
		[XmlAttribute("approved"),
		System.ComponentModel.DefaultValue(TranslationStatus.Unapproved)]
		public TranslationStatus TranslationStatus;


		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets whether the unit is "dynamic" (discovered dynamically while the program is running).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("dynamic", Namespace = XliffXmlSerializationHelper.kSilNamespace),
		System.ComponentModel.DefaultValue(false)]
		public bool Dynamic { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the source (original) text (and language) of the translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("source")]
		public XLiffTransUnitVariant Source { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the target (translated) text (and language) of the translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("target")]
		public XLiffTransUnitVariant Target { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of translation notes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("note")]
		public List<XLiffNote> Notes
		{
			get => _notes;
			set => _notes = value;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the priority value. This is an enumeration which defaults to "High" (of
		/// course): everything is HIGH PRIORITY!!!! We treat it as a string to more gracefully
		/// handle "creative" users.
		/// See LocalizationPriority in LocalizingInfo.cs for the full list of values.
		/// </summary>
		/// <remarks>This appears to not be used in the Bloom files.</remarks>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("priority", Namespace = XliffXmlSerializationHelper.kSilNamespace)]
		public string Priority { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the group value. This is an arbitrary string value.  The default is
		/// null or the empty string.
		/// </summary>
		/// <remarks>This appears to not be used in the Bloom files.</remarks>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("group", Namespace = XliffXmlSerializationHelper.kSilNamespace)]
		public string Group { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the category value. This is an enumeration which defaults to
		/// <see cref="LocalizationCategory.DontCare"/>.
		/// We treat it as a string to more gracefully handle "creative" users.
		/// See <see cref="LocalizationCategory"/> for the full list of standard values.
		/// </summary>
		/// <remarks>This appears to not be used in the Bloom files.</remarks>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("category", Namespace = XliffXmlSerializationHelper.kSilNamespace)]
		public string Category { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public bool IsEmpty =>
			IsNullOrEmpty(Id) && Notes.Count == 0 && Source == null && Target == null;

		#endregion

		#region Other Methods


		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a translation unit variant having the specified language id and value.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddOrReplaceVariant(string langId, string value)
		{
			var tuv = new XLiffTransUnitVariant
			{
				Lang = langId,
				Value = value
			};
			return AddOrReplaceVariant(tuv);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the specified variant.
		/// </summary>
		/// <param name="tuv">The variant.</param>
		/// <returns>true if the variant was successfully added. Otherwise, false.</returns>
		/// ------------------------------------------------------------------------------------
		public bool AddOrReplaceVariant(XLiffTransUnitVariant tuv)
		{
			if (tuv == null)
				return false;

			// If a variant exists for the specified language, then remove it first.
			RemoveVariant(tuv.Lang);
			if (tuv.Lang == kDefaultLangId)
				Source = tuv;
			else
				Target = tuv;
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Removes the specified translation unit variant.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void RemoveVariant(XLiffTransUnitVariant tuv)
		{
			if (tuv != null)
				RemoveVariant(tuv.Lang);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Removes the variant for the specified language.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void RemoveVariant(string langId)
		{
			XLiffTransUnitVariant tuv = GetVariantForLang(langId);
			if (tuv != null)
			{
				if (langId == kDefaultLangId)
					Source = new XLiffTransUnitVariant();
				else
					Target = new XLiffTransUnitVariant();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the translation unit variant for the specified language id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public XLiffTransUnitVariant GetVariantForLang(string langId)
		{
			if (langId == kDefaultLangId)
				return Source;
			return Target != null && langId == Target.Lang ? Target : null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			return IsEmpty ? "Empty" : Id;
		}

		#endregion
	}

	#endregion
}
