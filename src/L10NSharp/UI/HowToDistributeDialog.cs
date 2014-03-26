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
			if (!File.Exists(_targetTmxFilePath))
			{
				_targetTmxFilePath = Path.GetDirectoryName(_targetTmxFilePath);
				Debug.Assert(_targetTmxFilePath != null);
				if (!Directory.Exists(_targetTmxFilePath))
					Directory.CreateDirectory(_targetTmxFilePath);
			}
			InitializeComponent();

#if MONO
			//Steve M set these all to false in the Designer.cs, but that makes them all disappear on Windows
			label1.AutoSize = label2.AutoSize = label3.AutoSize = label4.AutoSize = false;
#endif
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
