using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using L10NSharp.UI;

namespace L10NSharp.Tests
{
	class MockLocalizableComponent: ILocalizableComponent, IComponent
	{

		public Dictionary<Tuple<Control, string>, string> StringContainer;
		public Button BirdButton;
		public Button ChickenButton;

		public MockLocalizableComponent()
		{
			StringContainer = new Dictionary<Tuple<Control, string>, string>();
			LoadTestData();
		}

		private void LoadTestData()
		{
			BirdButton = new Button() {Text = "Bird"};
			var crowId = "TestItem.Bird.Crow";
			var ravenId = "TestItem.Bird.Raven";
			var crowKey = new Tuple<Control, string>(BirdButton, crowId);
			var ravenKey = new Tuple<Control, string>(BirdButton, ravenId);
			StringContainer.Add(crowKey, "It's a crow");
			StringContainer.Add(ravenKey, "It's not a crow");

			ChickenButton = new Button() { Text = "Chicken" };
			var chickenKey = new Tuple<Control, string>(ChickenButton, "TestItem.Chicken.Rooster");
			StringContainer.Add(chickenKey, "It's a chicken");

			var eagleKey = new Tuple<Control, string>(BirdButton, "TestItem.Bird.Eagle");
			StringContainer.Add(eagleKey, "Fish-eating bird");
		}

		// For verifying tests
		public string GetLocalizedStringFromMock(Control control, string subId)
		{
			var key = new Tuple<Control, string>(control, subId);
			return StringContainer[key];
		}

		/// <summary>
		/// Allows the MockLocalizableComponent to give L10NSharp the information it needs to put strings
		/// into the localization UI to be localized.
		/// </summary>
		/// <returns>A list of LocalizingInfo objects</returns>
		public IEnumerable<LocalizingInfo> GetAllLocalizingInfoObjects(L10NSharpExtender extender)
		{
			var result = new List<LocalizingInfo>();
			foreach (var kvp in StringContainer)
			{
				var control = kvp.Key.Item1;
				var id = extender.GetLocalizingId(control) + kvp.Key.Item2;
				result.Add(new LocalizingInfo(control, id) { Text = kvp.Value, Category = LocalizationCategory.LocalizableComponent});
			}
			return result;
		}

		/// <summary>
		/// L10NSharp will call this for each localized string so that the component can set
		/// the correct value in the control.
		/// </summary>
		/// <param name="control">The control that was returned via the LocalizingInfo in
		/// GetAllLocalizingInfoObjects(). Will be null if that value was null.</param>
		/// <param name="id">a key into the ILocalizableComponent allowing it to know what
		/// string to localize</param>
		/// <param name="localization">the actual localized string</param>
		public void ApplyLocalizationToString(object control, string id, string localization)
		{
			var control1 = control as Control;
			var key = new Tuple<Control, string>(control1, id);
			string currentLocalization;
			if (StringContainer.TryGetValue(key, out currentLocalization))
			{
				StringContainer.Remove(key);
				StringContainer.Add(key, localization);
			}
			else
			{
				StringContainer.Add(key, localization);
			}
		}

#region unused IComponent support

		public event EventHandler Disposed;

		public ISite Site
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

#endregion

	}
}
