using System;
using System.IO;
using System.Windows.Forms;
using Localization;

namespace LocalizationManagerRunner
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			string filename;

			var args = Environment.GetCommandLineArgs();
			if (args.Length >= 2)
				filename = args[1];
			else
			{
				using (var dlg = new OpenFileDialog())
				{
					dlg.Filter = "Translation Memory File (*.tmx)|*.tmx|All Files (*.*)|*.*";
					dlg.Title = "Open Translation Memory File";
					dlg.CheckPathExists = true;
					dlg.CheckFileExists = true;
					dlg.Multiselect = false;
					if (dlg.ShowDialog() == DialogResult.Cancel)
						return;

					filename = dlg.FileName;
				}
			}

			var folder = Path.GetDirectoryName(filename);
			var file = Path.GetFileNameWithoutExtension(filename);
			var manager = LocalizationManager.Create(file, file, folder);
			manager.ShowLocalizationDialogBox(true);
		}
	}
}
