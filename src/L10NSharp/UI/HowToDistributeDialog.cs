using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace L10NSharp.UI
{
	public partial class HowToDistributeDialog : Form
	{
		private readonly string _targetTmxFilePath;

		public HowToDistributeDialog(string emailForSubmissions, string targetTmxFilePath)
		{
			_targetTmxFilePath = targetTmxFilePath;
			InitializeComponent();

#if MONO
			//Steve M set these all to false in the Designer.cs, but that makes them all disappear on Windows
			label1.AutoSize = label2.AutoSize = label3.AutoSize = label4.AutoSize = false;
#endif
			_emailLabel.Text=emailForSubmissions;
		}

		private void OnShowTMXFile(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var path = _targetTmxFilePath;
#if MONO
			MessageBox.Show(
				"Sorry, this function isn't implemented for the Linux version yet. The file you want is at " +
				_targetTmxFilePath);
#else
			path = path.Replace("/", "\\"); //forward slashes kill the selection attempt and it opens in My Documents.

			if (!File.Exists(path))
			{
				MessageBox.Show("Sorry, the TMX file hasn't been saved yet, so we can't show it to you yet.");
				return;
			}
			Process.Start("explorer.exe", "/select, \"" + path + "\"");
  #endif

		}

		private void _emailLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("mailto:" + _emailLabel.Text);
		}

	}
}
