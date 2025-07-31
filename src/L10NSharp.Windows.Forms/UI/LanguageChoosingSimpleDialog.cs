using System;
using System.Drawing;
using System.Windows.Forms;

namespace L10NSharp.Windows.Forms.UI
{
	/// <summary>
	/// The current thinking for use of this dialog is when an application first starts up,
	/// it allows the user to select which UI language to begin in.
	/// It could, of course, have  other uses as well.
	/// </summary>
	public partial class LanguageChoosingSimpleDialog : Form
	{
		public LanguageChoosingSimpleDialog(Icon icon)
		{
			InitializeComponent();
			Icon = icon;
		}

		public string SelectedLanguage { get; set; }

		private void btnOk_Click(object sender, EventArgs e)
		{
			SelectedLanguage = _uiLanguageListBox.SelectedLanguage;
		}

		private void m_uiLanguageListBox_DoubleClick(object sender, EventArgs e)
		{
			_btnOk.PerformClick();
		}
	}
}
