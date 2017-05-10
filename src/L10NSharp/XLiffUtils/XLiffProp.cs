// ---------------------------------------------------------------------------------------------
// File: XLiffProp.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml.Serialization;

namespace L10NSharp.XLiffUtils
{
	#region XLiffProp class
	/// ----------------------------------------------------------------------------------------
	[XmlType("prop", Namespace = "urn:oasis:names:tc:xliff:document:1.2")]
	public class XLiffProp
	{
		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the property's type.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("type")]
		public string Type { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the property's language Id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("xml:lang")]
		public string Lang { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the property's value.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlText]
		public string Value { get; set; }

		#endregion

		#region Methods
		/// ------------------------------------------------------------------------------------
		public XLiffProp Copy()
		{
			return new XLiffProp
			{
				Lang = Lang,
				Type = Type,
				Value = Value
			};
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool IsEmpty
		{
			get
			{
				return (string.IsNullOrEmpty(Lang) &&
					string.IsNullOrEmpty(Value) && string.IsNullOrEmpty(Type));
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static bool AddProp(string type, string value, List<XLiffProp> propList)
		{
			return AddProp(null, type, value, propList);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static bool AddProp(string lang, string type, string value, List<XLiffProp> propList)
		{
			var prop = new XLiffProp();
			prop.Lang = lang;
			prop.Type = type;
			prop.Value = value;
			return AddProp(prop, propList);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static bool AddProp(XLiffProp prop, List<XLiffProp> propList)
		{
			if (prop == null || prop.IsEmpty || propList == null)
				return false;

			//review: I (jh) was surprised to see that each setting of a property merely added it to this list, so we'd get dozens of the same thing over and over.
			//From what I can tell looking around, it seems it is safe to treat these properites as atomic, so I've added this Remove before re-adding with the
			//current value.
			propList.RemoveAll(p => p.Type == prop.Type);

			propList.Add(prop);
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			return (IsEmpty ? "Empty" : Type + ": " + Value);
		}

		#endregion
	}

	#endregion
}
