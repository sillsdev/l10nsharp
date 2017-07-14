﻿// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2009, SIL International. All Rights Reserved.
// <copyright from='2009' to='2009' company='SIL International'>
//		Copyright (c) 2009, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: XLiffDocument.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace L10NSharp.XLiffUtils
{
	#region XLiffDocment class
	/// ----------------------------------------------------------------------------------------

	[XmlRoot("xliff", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
	public class XLiffDocument
	{
        /// ------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="XLiffDocument"/> class.
        /// </summary>
        /// ------------------------------------------------------------------------------------
        public XLiffDocument()
		{
			File = new XLiffFile();
			Version = "1.2";
		}

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the XLIFF version.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("version")]
		public string Version { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlElement("file", Namespace = null)]
		public XLiffFile File { get; set; }

		#endregion

		#region Methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the translation unit for the specified id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TransUnit GetTransUnitForId(string id)
		{
			return File.Body.GetTransUnitForId(id);
		}

		/// ------------------------------------------------------------------------------------
		public IEnumerable<TransUnit> GetTransUnitsForTextInLang(string langId, string text)
		{
			return from tu in File.Body.TransUnits
				   let variant = tu.GetVariantForLang(langId)
				   where variant != null && variant.Value == text
				   select tu;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets all the unique language ids found in the XLIFF file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public IEnumerable<string> GetAllVariantLanguagesFound(bool isTargetNeeded)
		{
			if(isTargetNeeded)
				return File.Body.TransUnits.Select(tu => tu.Source).Where(s => (s != null && s.Lang != null)).Select(s => s.Lang)
					.Union(File.Body.TransUnits.Select(tu => tu.Target).Where(t => (t != null && t.Lang != null)).Select(t => t.Lang)).Distinct();
			else
				return File.Body.TransUnits.Select(tu => tu.Source).Where(s => (s != null && s.Lang != null)).Select(s => s.Lang).Distinct();
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
			return File.Body.AddTransUnit(tu);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Remove the specified translation unit.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void RemoveTransUnit(TransUnit tu)
		{
			File.Body.RemoveTransUnit(tu);
		}

        /// ------------------------------------------------------------------------------------
        /// <summary>
        /// Saves the XLIFFDocument to the specified XLIFF file.
        /// </summary>
        /// ------------------------------------------------------------------------------------
        public void Save(string xliffFile)
		{
            XLiffXmlSerializationHelper.SerializeToFile(xliffFile, this);
		}

        /// ------------------------------------------------------------------------------------
        /// <summary>
        /// Reads the specified XLIFF file and returns a XLIFFDocument containing the information
        /// in the file.
        /// </summary>
        /// <param name= "xLiffFile">The XLiff file to read.</param>
        /// ------------------------------------------------------------------------------------
        public static XLiffDocument Read(string xLiffFile)
		{
			if (!System.IO.File.Exists(xLiffFile))
				throw new FileNotFoundException("XLiff file not found.", xLiffFile);

			Exception e;
			var xLiffDoc = XLiffXmlSerializationHelper.DeserializeFromFile<XLiffDocument>(xLiffFile, out e);

			if (e != null)
				throw e;

			return xLiffDoc;
		}

		#endregion

		/// <summary>
		/// When we change ids after people have already been localizing, we have a BIG PROBLEM.
		/// This helps with the common case were we just changed the hierarchical organizaiton of the id,
		/// that is, the parts of the id before th final '.'.
		/// </summary>
		 public TransUnit GetTransUnitForOrphan(TransUnit orphan)
		{
			 return File.Body.GetTransUnitForOrphan(orphan);
		}
	}


	#endregion
}
