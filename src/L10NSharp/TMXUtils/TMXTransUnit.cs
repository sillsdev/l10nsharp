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
// File: TMXTransUnit.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace L10NSharp.TMXUtils
{
	#region TMXTransUnit class
	/// ----------------------------------------------------------------------------------------
	[XmlType("tu")]
	public class TMXTransUnit : TMXBaseWithNotesAndProps, ITransUnit
	{
		/// ------------------------------------------------------------------------------------
		public TMXTransUnit()
		{
			Variants = new List<TMXTransUnitVariant>();
		}

		#region Properties

		/// ------------------------------------------------------------------------------------
		[XmlAttribute("tuid")]
		public string Id { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("note")]
		public List<TMXNote> Notes
		{
			get => _notes;
			set => _notes = value;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of props in the translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("prop")]
		public List<TMXProp> Props
		{
			get => _props;
			set => _props = value;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("tuv")]
		public List<TMXTransUnitVariant> Variants { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public bool IsEmpty =>
			(string.IsNullOrEmpty(Id) && Notes.Count == 0 &&
			Props.Count == 0 && (Variants == null || Variants.Count == 0));

		#endregion

		#region Other Methods


		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a translation unit variant having the specified language id and value.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AddOrReplaceVariant(string langId, string value)
		{
			var tuv = new TMXTransUnitVariant();
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
		public bool AddOrReplaceVariant(TMXTransUnitVariant tuv)
		{
			if (tuv == null || tuv.IsEmpty)
				return false;

			// If a variant exists for the specified language, then remove it first.
			RemoveVariant(tuv.Lang);

			Variants.Add(tuv);
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Removes the specified translation unit variant.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void RemoveVariant(TMXTransUnitVariant tuv)
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
			TMXTransUnitVariant tuv = GetVariantForLang(langId);
			if (tuv != null)
				Variants.Remove(tuv);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the translation unit variant for the specified language id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TMXTransUnitVariant GetVariantForLang(string langId)
		{
			return Variants.FirstOrDefault(x => x.Lang == langId);
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
