using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Localization;
using SampleApp.Properties;

namespace SampleApp
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

			SetUpLocalization();

			Localization.LocalizationManager.SetUILanguage(Settings.Default.UserInterfaceLanguage, false);

			Application.Run(new Form1());
			Settings.Default.Save();
		}

		public static void SetUpLocalization()
		{
			var installedStringFileFolder = "../../";

			try
			{
				//You sure won't want to use "temp" in your real app. Consider AppData/Product.
				//Downside of that one is that the user needs write access to that folder.
				var pathForStoringLocalizations = Path.GetTempPath();
				LocalizationManager.Create(Settings.Default.UserInterfaceLanguage,
										   "SampleApp", "SampleApp", Application.ProductVersion,
										   installedStringFileFolder,
										   Path.Combine(pathForStoringLocalizations, "Localizations"),
										   SystemIcons.Application, //replace with your icon
										   "sampleappLocalizations@nowhere.com", "SampleApp");

				Settings.Default.UserInterfaceLanguage = LocalizationManager.UILanguageId;
			}
			catch (Exception error)
			{
				if (Process.GetProcesses().Count(p => p.ProcessName.ToLower().Contains("SampleApp")) > 1)
				{
					MessageBox.Show("There is another copy of SampleApp already running while SampleApp was trying to set up localization.");
					Environment.FailFast("SampleApp couldn't set up localization");
				}

				if (error.Message.Contains("SampleApp.en.tmx"))
				{
					MessageBox.Show("Sorry. SampleApp is trying to set up your machine to use this new version, but something went wrong getting at the file it needs. If you restart your computer, all will be well.");

					Environment.FailFast("SampleApp couldn't set up localization");
				}

				//otherwise, we don't know what caused it.
				throw;
			}
		}

	}
}
