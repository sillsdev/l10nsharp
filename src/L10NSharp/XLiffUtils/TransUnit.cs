// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2009, SIL International. All Rights Reserved.
// <copyright from='2009' to='2017' company='SIL International'>
//		Copyright (c) 2009, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: TransUnit.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace L10NSharp.XLiffUtils
{
	#region TransUnit class
	/// ----------------------------------------------------------------------------------------
	[XmlType("trans-unit", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
	public class TransUnit : XLiffBaseWithNotesAndProps
	{
		internal const string kDefaultLangId = "en";

		/// ------------------------------------------------------------------------------------
		public TransUnit()
		{
			Sources = new TransUnitVariant();
			Targets = null;
			Notes = new List<XLiffNote>();
		}

		#region Properties

		/// ------------------------------------------------------------------------------------
		[XmlAttribute("id")]
		public string Id { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the type of translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("extype")]
		public string Type { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("source")]
		public TransUnitVariant Sources { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("target")]
		public TransUnitVariant Targets { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("note")]
		public List<XLiffNote> Notes { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public bool IsEmpty
		{
			get
			{
				return (string.IsNullOrEmpty(Id) && Notes.Count == 0 && Sources == null && Targets == null);
			}
		}

		#endregion

		#region Other Methods


		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a translation unit variant having the specified language id and value.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddOrReplaceVariant(string langId, string value)
		{
			var tuv = new TransUnitVariant();
			tuv.Lang = langId;
			tuv.Value = value;
			return AddOrReplaceVariant(tuv);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the specified variant.
		/// </summary>
		/// <param name="tuv">The variant.</param>
		/// <returns>true if the variant was successfully added. Otherwise, false.</returns>
		/// ------------------------------------------------------------------------------------
		public bool AddOrReplaceVariant(TransUnitVariant tuv)
		{
			if (tuv == null)
				return false;

			// If a variant exists for the specified language, then remove it first.
			RemoveVariant(tuv.Lang);
			if (tuv.Lang == kDefaultLangId)
				Sources = tuv;
			else
				Targets = tuv;
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Removes the specified translation unit variant.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void RemoveVariant(TransUnitVariant tuv)
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
			TransUnitVariant tuv = GetVariantForLang(langId);
			if (tuv != null)
			{
				if (langId == kDefaultLangId)
					Sources = new TransUnitVariant();
				else
					Targets = new TransUnitVariant();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the translation unit variant for the specified language id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TransUnitVariant GetVariantForLang(string langId)
		{
			if (langId == kDefaultLangId)
				return Sources;
			else
				return Targets != null && langId == Targets.Lang ? Targets : null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			return (IsEmpty ? "Empty" : Id);
		}

		#endregion
	}

	#endregion
}
