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
// File: TMXBody.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace L10NSharp.TMXUtils
{
	#region TMXBody class
	/// ----------------------------------------------------------------------------------------
	[XmlType("body")]
	public class TMXBody
	{
		// This is used when translation unit IDs are not found in the file (which seems to be
		// the case with Lingobit TMX files).
		private int _transUnitId;
		private bool _idsVerified;
		private List<TransUnit> _transUnits = new List<TransUnit>();

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the list of translation units in the header.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("tu")]
		public List<TransUnit> TransUnits
		{
			get
			{
				if (!_idsVerified && _transUnits != null && _transUnits.Count > 0)
				{
					foreach (var tu in _transUnits.Where(tu => string.IsNullOrEmpty(tu.Id)))
						tu.Id = (++_transUnitId).ToString();

					_idsVerified = true;
				}

				return _transUnits;
			}
			set { _transUnits = value; }
		}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets the list of translation units in the document.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//[XmlIgnore]
		//public Dictionary<string, TransUnit> TransUnitsById
		//{
		//    get
		//    {
		//        if (m_transUnitsById.Count == 0 && m_transUnits != null && m_transUnits.Count > 0)
		//        {
		//            foreach (TransUnit tu in m_transUnits)
		//                m_transUnitsById[tu.Id] = tu;
		//        }

		//        return m_transUnitsById;
		//    }
		//}

		#endregion

		#region Methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the translation unit for the specified id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal TransUnit GetTransUnitForId(string id)
		{
			return _transUnits.FirstOrDefault(tu => tu.Id == id);
		}

		/// <summary>
		/// When all but the last part of the id changed, this can help reunite thigns
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		internal TransUnit GetTransUnitForOrphanWithId(string id)
		{
			var terminalIdToMatch = LocalizedStringCache.GetTerminalIdPart(id);
			return _transUnits.FirstOrDefault(tu => LocalizedStringCache.GetTerminalIdPart(tu.Id) == terminalIdToMatch);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the specified translation unit.
		/// </summary>
		/// <param name="tu">The translation unit.</param>
		/// <returns>true if the translation unit was successfully added. Otherwise, false.</returns>
		/// ------------------------------------------------------------------------------------
		internal bool AddTransUnit(TransUnit tu)
		{
			if (tu == null || tu.IsEmpty)
				return false;

			if (tu.Id == null)
				tu.Id = (++_transUnitId).ToString();

			// If a translation unit with the specified id already exists, then quit here.
			if (GetTransUnitForId(tu.Id) != null)
				return false;

			_transUnits.Add(tu);
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// If a translation unit does not already exist for the id in the specified
		/// translation unit, then the translation unit is added. Otherwise, if the variant
		/// for the specified language does not exist in the translation unit, it is added.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void AddTransUnitOrVariantFromExisting(TransUnit tu, string langId)
		{
			var variantToAdd = tu.GetVariantForLang(langId);

			if (variantToAdd == null || AddTransUnit(tu))
				return;

			var existingTu = GetTransUnitForId(tu.Id);

			//notice, we don't care if there is already a string in there for this language
			//(that was the source of a previous bug), because the tmx of language X should
			//surely take precedence, as source of the translation, over other language's
			//tms files which, by virtue of their alphabetica order (e.g. arabic), came
			//first. This probably only effects English, as it has variants in all the other
			//languages. Previously, Arabic would be processed first, so when English came
			//along, it was too late.
			existingTu.AddOrReplaceVariant(variantToAdd);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Removes the specified translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void RemoveTransUnit(TransUnit tu)
		{
			if (tu == null)
				return;

			if (_transUnits.Contains(tu))
				_transUnits.Remove(tu);
			else if (tu.Id != null)
			{
				var tmptu = GetTransUnitForId(tu.Id);
				if (tmptu != null)
					_transUnits.Remove(tmptu);
			}
		}

		#endregion
	}

	#endregion
}
