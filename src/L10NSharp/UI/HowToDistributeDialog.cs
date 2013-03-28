using System.Diagnostics;
using System.Windows.Forms;

namespace Localization.UI
{
	public partial class HowToDistributeDialog : Form
	{
		private readonly string _targetTmxFilePath;

		public HowToDistributeDialog(string emailForSubmissions, string targetTmxFilePath)
		{
			_targetTmxFilePath = targetTmxFilePath;
			InitializeComponent();
			_emailLabel.Text=emailForSubmissions;
		}

		private void OnShowTMXFile(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("explorer.exe", "/select, \"" + _targetTmxFilePath + "\"");
		}

		private void _emailLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("mailto:" + _emailLabel.Text);
		}

	}
}
