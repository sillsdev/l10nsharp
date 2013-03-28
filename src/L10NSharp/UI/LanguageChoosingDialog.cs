using System.Drawing;
using System.Windows.Forms;

namespace L10NSharp.UI
{
	public partial class LanguageChoosingDialog : Form
	{
		public LanguageChoosingDialog(string requestedLanguageDisplayName, Icon icon)
		{
			InitializeComponent();
			this.Icon = icon;
			_messageLabel.Text = string.Format(_messageLabel.Text, requestedLanguageDisplayName);
		}

		public string SelectedLanguage;

		private void _OKButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			SelectedLanguage = uiLanguageComboBox1.SelectedLanguage;
			base.OnClosing(e);
		}
	}
}
