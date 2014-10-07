using System.Collections.Generic;

namespace L10NSharp
{
	/// <summary>
	/// This interface allows localization of a control that collects multiple strings
	/// and doesn't expose them in a way that is easy for L10NSharp to localize itself.
	/// This interface was initially motivated by Bloom's BetterToolTip.
	/// </summary>
	public interface IMultiStringContainer
	{
		/// <summary>
		/// Allows the container to give L10NSharp the information it needs to put strings
		/// into the localization UI to be localized.
		/// </summary>
		/// <returns>A list of LocalizingInfo objects</returns>
		IEnumerable<LocalizingInfo> GetAllLocalizingInfoObjects();

		/// <summary>
		/// L10NSharp sends the localized string back to the IMultiStringContainer to be
		/// applied, since L10NSharp doesn't know the internal workings of the container.
		/// We assume that the container is a collection of subcontrols that have string
		/// ids that need localizing.
		/// </summary>
		/// <param name="obj">somewhere in this object is a string to be localized</param>
		/// <param name="id">a key into the subControl allowing it to know what string to localize</param>
		/// <param name="localization">the actual localized string</param>
		void ApplyLocalizationToString(object obj, string id, string localization);
	}
}
