using System.Collections.Generic;
using L10NSharp.UI;

namespace L10NSharp
{
	/// <summary>
	/// This interface allows a control to be localized by L10NSharp. It can be used to enable
	/// localization of controls which allow strings to be set in the designer.
	/// </summary>
	public interface ILocalizableComponent
	{
		/// <summary>
		/// Allows the container to give L10NSharp the information it needs to put strings
		/// into the localization UI to be localized.
		/// </summary>
		/// <returns>A list of LocalizingInfo objects</returns>
		IEnumerable<LocalizingInfo> GetAllLocalizingInfoObjects(L10NSharpExtender extender);

		/// <summary>
		/// L10NSharp sends the localized string back to the ILocalizableComponent to be
		/// applied, since L10NSharp doesn't know the internal workings of the container.
		/// </summary>
		/// <param name="obj">if non-null this object contains a string to be localized</param>
		/// <param name="id">a key into the ILocalizableComponent allowing it to know what
		///  string to localize</param>
		/// <param name="localization">the actual localized string</param>
		void ApplyLocalizationToString(object obj, string id, string localization);
	}
}
