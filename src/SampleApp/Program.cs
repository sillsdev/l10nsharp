using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using L10NSharp;
using SampleApp.Properties;

namespace SampleApp
{
	static class Program
	{
		private static ILocalizationManager _localizationManager;
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			SetUpLocalization(args.Any(a => a == "-m"), args.Any(a => a == "-tmx"));

			LocalizationManager.SetUILanguage(Settings.Default.UserInterfaceLanguage, false);

			Application.Run(new Form1());
			Settings.Default.Save();

			_localizationManager?.Dispose();
			_localizationManager = null;
		}

		public static void SetUpLocalization(bool useAdditionalMethodInfo, bool useTmx)
		{
			if (useTmx)
				throw new NotSupportedException("TMX-based localization is no longer supported.");

			//your installer should have a folder where you place the localization files you're shipping with the program
			var directoryOfInstalledLocFiles = "../../LocalizationFilesFromInstaller";
			Directory.CreateDirectory(directoryOfInstalledLocFiles);

			try
			{
				//if this is your first time running the app, the library will query the OS for the
				//the default language. If it doesn't have that, it puts up a dialog listing what
				//it does have to offer.

				var theLanguageYouRememberedFromLastTime = Settings.Default.UserInterfaceLanguage;

				if (useAdditionalMethodInfo)
				{
					MessageBox.Show(MyOwnGetString("SampleApp.InformationalMessageBox.Message",
							"The generated localization file should contain this string and the window title.", "This is a comment"),
						MyOwnGetString("SampleApp.InformationalMessageBox.Title", "Cool Title"));

					_localizationManager = LocalizationManager.Create(theLanguageYouRememberedFromLastTime,
						"SampleApp.exe", "SampleApp", Application.ProductVersion,
						directoryOfInstalledLocFiles,
						"MyCompany/L10NSharpSample",
						Resources.Icon, //replace with your icon
						"sampleappLocalizations@nowhere.com",
						typeof(Program)
							.GetMethods(BindingFlags.Static | BindingFlags.Public)
							.Where(m => m.Name == "MyOwnGetString"),
						"SampleApp");
				}
				else
				{
					_localizationManager = LocalizationManager.Create(theLanguageYouRememberedFromLastTime,
						"SampleApp.exe", "SampleApp", Application.ProductVersion,
						directoryOfInstalledLocFiles,
						"MyCompany/L10NSharpSample",
						Resources.Icon, //replace with your icon
						"sampleappLocalizations@nowhere.com", "SampleApp");
				}

				Settings.Default.UserInterfaceLanguage = LocalizationManager.UILanguageId;
			}
			catch (Exception error)
			{
				if (Process.GetProcesses().Count(p => p.ProcessName.ToLower().Contains("SampleApp")) > 1)
				{
					MessageBox.Show("There is another copy of SampleApp already running while SampleApp was trying to set up localization.");
					Environment.FailFast("SampleApp couldn't set up localization");
				}

				if (error.Message.Contains("SampleApp.en.xlf"))
				{
					MessageBox.Show("Sorry. SampleApp is trying to set up your machine to use this new version, but something went wrong getting at the file it needs. If you restart your computer, all will be well.");

					Environment.FailFast("SampleApp couldn't set up localization");
				}

				//otherwise, we don't know what caused it.
				throw;
			}
		}

		public static string MyOwnGetString(string id, string english)
		{
			return english;
		}

		public static string MyOwnGetString(string id, string english, string comment)
		{
			return english;
		}
	}
}
