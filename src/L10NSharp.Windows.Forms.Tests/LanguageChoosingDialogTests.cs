using System.Threading;
using L10NSharp.Windows.Forms.UIComponents;
using NUnit.Framework;

namespace L10NSharp.Windows.Forms.Tests
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	class LanguageChoosingDialogTests
	{
		[Test]
		public void Close_SetsSelectedLanguageFromComboBox()
		{
			using (var dlg = new LanguageChoosingDialog(L10NCultureInfo.GetCultureInfo("en"), null))
			{
				var comboBox = (UILanguageComboBox)ReflectionHelper.GetField(dlg, "uiLanguageComboBox1");
				dlg.Show();
				Assert.IsNull(dlg.SelectedLanguage);

				dlg.Close();

				Assert.IsNotNull(dlg.SelectedLanguage);
				Assert.AreEqual(comboBox.SelectedLanguage, dlg.SelectedLanguage);
			}
		}
	}
}
