using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;

namespace L10NSharp.Windows.Forms.UIComponents
{
	public partial class HowToDistributeDialog : Form
	{
		private readonly string _emailForSubmissions;
		private readonly string _targetTranslationFilePath;

		public HowToDistributeDialog(string emailForSubmissions, string targetTranslationFilePath)
		{
			_emailForSubmissions = emailForSubmissions;
			_targetTranslationFilePath = targetTranslationFilePath;
			InitializeComponent();

			if (Utils.IsMono)
			{
				// In Mono, Label.AutoSize=true sets Size to PreferredSize (which is always
				// one line high) even if the Size has already been explicitly set. In Windows,
				// Label.AutoSize=false makes the labels disappear. So we need to turn off
				// AutoSize here and set the multiline labels (which all have a max height set
				// in Designer) to their largest possible heights. (That allows all the available
				// space for localizations that may need more space.) 
				foreach (var label in Controls.OfType<Label>().Where(l => l.MaximumSize.Height > 0))
				{
					int width = _table.Width - label.Margin.Horizontal;
					label.AutoSize = false;
					if (label.MaximumSize.Width > 0 && label.MaximumSize.Width < width)
						width = label.MaximumSize.Width;
					label.Size = new System.Drawing.Size(width, label.MaximumSize.Height);
				}
			}

			_lblHowToDistribute.Text = string.Format(_lblHowToDistribute.Text,
				Environment.NewLine + _targetTranslationFilePath + Environment.NewLine,
				Environment.NewLine + _emailForSubmissions + Environment.NewLine);
			_lblHowToDistribute.Links.Clear();
			AddLink(_lblHowToDistribute, _targetTranslationFilePath, OnShowTranslationFile);
			AddLink(_lblHowToDistribute, _emailForSubmissions, OpenEmail);
		}

		private static void AddLink(LinkLabel label, string s, Action action)
		{
			if (string.IsNullOrEmpty(s))
				throw new ArgumentException("Error: no link text specified", nameof(s));

			var linkStart = label.Text.IndexOf(s, StringComparison.Ordinal);
			if (linkStart < 0)
				throw new ArgumentException("Error: specified text not found in link label text:" + s, nameof(s));

			label.Links.Add(linkStart, s.Length, action);
		}

		private void HandleLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			e.Link.Visited = true;
			((Action)e.Link.LinkData).Invoke();
		}

		private void OnShowTranslationFile()
		{
			var path = _targetTranslationFilePath;
			if (Path.DirectorySeparatorChar != '/')
				path = path.Replace('/', Path.DirectorySeparatorChar); //forward slashes kill the selection attempt and it opens in My Documents.
			if (!File.Exists(path))
			{
				MessageBox.Show("Sorry, the translation memory file hasn't been saved yet, so we can't show it to you yet.");
				return;
			}
			if (Environment.OSVersion.Platform == PlatformID.Unix)
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

		private void OpenEmail()
		{
			Process.Start("mailto:" + _emailForSubmissions);
		}

	}
}
