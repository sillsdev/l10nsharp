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
// File: TMXDocument.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Localization.TMXUtils
{
	#region TMXDocument class
	/// ----------------------------------------------------------------------------------------
	[XmlRoot("tmx")]
	public class TMXDocument
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="TMXDocument"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TMXDocument()
		{
			Body = new TMXBody();
			Header = new TMXHeader();
			Version = "1.4";
		}

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the TMX version.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("version")]
		public string Version { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the header.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("header")]
		public TMXHeader Header { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the body.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("body")]
		public TMXBody Body { get; set; }

		#endregion

		#region Methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the translation unit for the specified id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TransUnit GetTransUnitForId(string id)
		{
			return Body.GetTransUnitForId(id);
		}

		/// ------------------------------------------------------------------------------------
		public IEnumerable<TransUnit> GetTransUnitsForTextInLang(string langId, string text)
		{
			return from tu in Body.TransUnits
				   let variant = tu.GetVariantForLang(langId)
				   where variant != null && variant.Value == text
				   select tu;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets all the unique language ids found in the TMX file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public IEnumerable<string> GetAllVariantLanguagesFound()
		{
			return Body.TransUnits.SelectMany(tu => tu.Variants).Select(v => v.Lang).Distinct();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the specified translation unit.
		/// </summary>
		/// <param name="tu">The translation unit.</param>
		/// <returns>true if the translation unit was successfully added. Otherwise, false.</returns>
		/// ------------------------------------------------------------------------------------
		public bool AddTransUnit(TransUnit tu)
		{
			return Body.AddTransUnit(tu);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Remove the specified translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void RemoveTransUnit(TransUnit tu)
		{
			Body.RemoveTransUnit(tu);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves the TMXDocument to the specified TMX file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Save(string tmxFile)
		{
			XmlSerializationHelper.SerializeToFile(tmxFile, this);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Reads the specified TMX file and returns a TMXDocument containing the information
		/// in the file.
		/// </summary>
		/// <param name="tmxFile">The TMX file to read.</param>
		/// ------------------------------------------------------------------------------------
		public static TMXDocument Read(string tmxFile)
		{
			if (!File.Exists(tmxFile))
				throw new FileNotFoundException("TMX file not found.", tmxFile);

			Exception e;
			var tmxDoc = XmlSerializationHelper.DeserializeFromFile<TMXDocument>(tmxFile, out e);

			if (e != null)
				throw e;

			return tmxDoc;
		}

		#endregion
	}

	#endregion
}
