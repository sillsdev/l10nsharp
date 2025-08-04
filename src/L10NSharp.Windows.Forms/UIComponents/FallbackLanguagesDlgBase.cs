using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Windows.Forms;
using L10NSharp;

namespace L10NSharp.Windows.Forms.UIComponents
{
	/// ------------------------------------------------------------------------------------
	public partial class FallbackLanguagesDlgBase : Form
	{
		protected CultureInfo _uiCulture;

		/// ------------------------------------------------------------------------------------
		public FallbackLanguagesDlgBase()
		{
			InitializeComponent();
		}

		/// ------------------------------------------------------------------------------------
		public IEnumerable<string> FallbackLanguageIds
		{
			get { return _listBoxFallbackLanguages.Items.Cast<CultureInfo>().Select(x => x.Name); }
		}

		/// ------------------------------------------------------------------------------------
		private CultureInfo SelectedAvailableLanguage
		{
			get { return _listBoxAvailableLanguages.SelectedItem as CultureInfo; }
		}

		/// ------------------------------------------------------------------------------------
		private CultureInfo SelectedFallbackLanguage
		{
			get { return _listBoxFallbackLanguages.SelectedItem as CultureInfo; }
		}

		/// ------------------------------------------------------------------------------------
		protected void UpdateDisplay()
		{
			if (SelectedAvailableLanguage == null)
			{
				int i = Math.Max(0, _listBoxAvailableLanguages.SelectedIndex);
				_listBoxAvailableLanguages.SelectedItem = _listBoxAvailableLanguages.Items[i];
			}

			if (SelectedFallbackLanguage == null)
			{
				int i = Math.Max(0, _listBoxFallbackLanguages.SelectedIndex);
				_listBoxFallbackLanguages.SelectedItem = _listBoxFallbackLanguages.Items[i];
			}

			_buttonAdd.Enabled = (_uiCulture.Name != SelectedAvailableLanguage.Name &&
				!_listBoxFallbackLanguages.Items.Contains(SelectedAvailableLanguage));

			_buttonRemove.Enabled =
				(SelectedFallbackLanguage.Name != LocalizationManager.kDefaultLang);

			_buttonMoveUp.Enabled = (_listBoxFallbackLanguages.SelectedIndex > 0 &&
				SelectedFallbackLanguage.Name != LocalizationManager.kDefaultLang);

			_buttonMoveDown.Enabled =
				(_listBoxFallbackLanguages.SelectedIndex < _listBoxFallbackLanguages.Items.Count - 2);
		}

		/// ------------------------------------------------------------------------------------
		private void _buttonAdd_Click(object sender, EventArgs e)
		{
			int i = _listBoxFallbackLanguages.Items.Count - 1;
			_listBoxFallbackLanguages.Items.Insert(i, SelectedAvailableLanguage);
			_listBoxFallbackLanguages.SelectedItem = SelectedAvailableLanguage;
			UpdateDisplay();
		}

		/// ------------------------------------------------------------------------------------
		private void _buttonRemove_Click(object sender, EventArgs e)
		{
			_listBoxFallbackLanguages.Items.Remove(SelectedFallbackLanguage);
			UpdateDisplay();
		}

		/// ------------------------------------------------------------------------------------
		private void _buttonMoveUp_Click(object sender, EventArgs e)
		{
			MoveSelectedFallbackLanguage(false);
		}

		/// ------------------------------------------------------------------------------------
		private void _buttonMoveDown_Click(object sender, EventArgs e)
		{
			MoveSelectedFallbackLanguage(true);
		}

		/// ------------------------------------------------------------------------------------
		private void MoveSelectedFallbackLanguage(bool down)
		{
			int dy = (down ? 1 : -1);

			int i = _listBoxFallbackLanguages.SelectedIndex + dy;
			var ci = _listBoxFallbackLanguages.SelectedItem as CultureInfo;
			_listBoxFallbackLanguages.Items.Remove(ci);
			_listBoxFallbackLanguages.Items.Insert(i, ci);
			_listBoxFallbackLanguages.SelectedItem = ci;
		}

		/// ------------------------------------------------------------------------------------
		private void _listBoxAvailableLanguages_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (_buttonAdd.Enabled)
				_buttonAdd_Click(null, null);
		}

		/// ------------------------------------------------------------------------------------
		private void _listBoxFallbackLanguages_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (_buttonRemove.Enabled)
				_buttonRemove_Click(null, null);
		}

		/// ------------------------------------------------------------------------------------
		private void _listBoxFallbackLanguages_SelectedValueChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		/// ------------------------------------------------------------------------------------
		private void _listBoxAvailableLanguages_SelectedValueChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
	}
}
