using System.Collections.Generic;
using L10NSharp.WindowsForms.UI;

namespace L10NSharp.WindowsForms
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
		/// L10NSharp will call this for each localized string so that the component can set
		/// the correct value in the control.
		/// </summary>
		/// <param name="control">The control that was returned via the LocalizingInfo in
		/// GetAllLocalizingInfoObjects(). Will be null if that value was null.</param>
		/// <param name="id">a key into the ILocalizableComponent allowing it to know what
		/// string to localize</param>
		/// <param name="localization">the actual localized string</param>
		void ApplyLocalizationToString(object control, string id, string localization);
	}
}
