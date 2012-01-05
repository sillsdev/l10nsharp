// ---------------------------------------------------------------------------------------------
// File: TMXProp.cs
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Localization.TMXUtils
{
	#region TMXProp class
	/// ----------------------------------------------------------------------------------------
	[XmlType("prop")]
	public class TMXProp
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
		public TMXProp Copy()
		{
			return new TMXProp
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
		internal static bool AddProp(string type, string value, List<TMXProp> propList)
		{
			return AddProp(null, type, value, propList);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static bool AddProp(string lang, string type, string value, List<TMXProp> propList)
		{
			var prop = new TMXProp();
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
		internal static bool AddProp(TMXProp prop, List<TMXProp> propList)
		{
			if (prop == null || prop.IsEmpty || propList == null)
				return false;

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
