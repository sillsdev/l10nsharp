using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace L10NSharp.UI
{
	/// ----------------------------------------------------------------------------------------
	public class UILanguageComboBox : ComboBox
	{
		private bool _showOnlyLanguagesHavingLocalizations;

		/// ------------------------------------------------------------------------------------
		public UILanguageComboBox()
		{
			DropDownHeight = 200;
			DropDownStyle = ComboBoxStyle.DropDownList;
			FormattingEnabled = true;
			Font = SystemFonts.IconTitleFont;
			DisplayMember = "NativeName";
			RefreshList();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not the extender is currently in design mode.
		/// I have had some problems with the base class' DesignMode property being true
		/// when in design mode. I'm not sure why, but adding a couple more checks fixes the
		/// problem.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		private new bool DesignMode
		{
			get
			{
				return (base.DesignMode || GetService(typeof(IDesignerHost)) != null) ||
					(LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			}
		}

		/// ----------------------------------------------------------------------------------------
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string SelectedLanguage
		{
			get { return ((CultureInfo)SelectedItem).Name; }
			set { SelectedItem = CultureInfo.GetCultureInfo(value); }
		}

		/// ----------------------------------------------------------------------------------------
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDirty
		{
			get
			{
				if (SelectedItem == null)
					return false;

				return (LocalizationManager.UILanguageId != ((CultureInfo)SelectedItem).Name);
			}
		}

		/// ----------------------------------------------------------------------------------------
		[Browsable(true)]
		[DefaultValue(false)]
		public bool ShowOnlyLanguagesHavingLocalizations
		{
			get { return _showOnlyLanguagesHavingLocalizations; }
			set
			{
				if (_showOnlyLanguagesHavingLocalizations == value)
					return;

				_showOnlyLanguagesHavingLocalizations = value;
				RefreshList();
			}
		}

		/// ----------------------------------------------------------------------------------------
		public void RefreshList()
		{
			if (DesignMode)
				return;

			var cultureList = LocalizationManager.GetUILanguages(_showOnlyLanguagesHavingLocalizations).ToList();
			cultureList.Add(CultureInfo.GetCultureInfo("en"));

			Items.Clear();
			Items.AddRange(cultureList.Distinct().OrderBy(ci => ci.NativeName).ToArray());
			var currCulture = CultureInfo.GetCultureInfo(LocalizationManager.UILanguageId);
			if (Items.Contains(currCulture))
			{
				SelectedItem = currCulture;
			}
			else
			{
				SelectedItem = "en";
			}
		}
	}
}
