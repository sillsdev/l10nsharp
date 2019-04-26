using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace L10NSharp.UI
{
	public partial class HowToDistributeDialog : Form
	{
		private readonly string _targetTranslationFilePath;

		public HowToDistributeDialog(string emailForSubmissions, string targetTranslationFilePath)
		{
			_targetTranslationFilePath = targetTranslationFilePath;
			InitializeComponent();

			if (Utils.IsMono)
			{
				// In Mono, Label.AutoSize=true sets Size to PreferredSize (which is always
				// one line high) even if the Size has already been explicitly set.  In Windows,
				// Label.AutoSize=false makes the labels disappear.  So we need to turn off
				// AutoSize here and set the multiline labels explicitly to their largest
				// possible sizes for this fixed-size dialog.  (That allows all the available
				// space for localizations that may need more space.)
				label1.AutoSize = label2.AutoSize = label4.AutoSize = false;
				label4.Size = new System.Drawing.Size(300, 142); // top message
				label2.Size = new System.Drawing.Size(300, 56);  // middle message
				label1.Size = new System.Drawing.Size(300, 112); // bottom message
			}

			_emailLabel.Text=emailForSubmissions;
		}

		private void OnShowTranslationFile(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var path = _targetTranslationFilePath;
			if (Path.DirectorySeparatorChar != '/')
				path = path.Replace('/', Path.DirectorySeparatorChar); //forward slashes kill the selection attempt and it opens in My Documents.
			if (!File.Exists(path))
			{
				MessageBox.Show("Sorry, the translation memory file hasn't been saved yet, so we can't show it to you yet.");
				return;
			}
			if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
			{
				if (File.Exists("/usr/bin/nemo"))
					Process.Start("/usr/bin/nemo", path);		// default file manager for Cinnamon (Wasta)
				else if (File.Exists("/usr/bin/nautilus"))
					Process.Start("/usr/bin/nautilus", path);	// default file manager for Gnome / Unity? (Ubuntu)
				else
					MessageBox.Show("Sorry, we cannot find a suitable file manager for Linux. The file you want is at " + path);
			}
			else
			{
				Process.Start("explorer.exe", "/select, \"" + path + "\"");
			}
		}

		private void _emailLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("mailto:" + _emailLabel.Text);
		}

	}
}
