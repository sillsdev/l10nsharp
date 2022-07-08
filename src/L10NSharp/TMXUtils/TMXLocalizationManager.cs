using System.IO;

namespace L10NSharp.TMXUtils
{
	/// ----------------------------------------------------------------------------------------
	internal static class TMXLocalizationManager
	{
		private const string kFileExtension = ".tmx";

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// A long time back, L10NSharp used to create writable TMX files under the
		/// common/shared AppData folder. TMX files are no longer supported, but applications can
		/// use this method to purge old TMX files.</summary>
		/// <param name="appId">ID of the application used for creating the TMX files (typically
		/// the same ID passed as the 2nd parameter to LocalizationManagerInternal.Create).</param>
		/// <param name="directoryOfWritableTmxFiles">Folder from which to delete TMX files.
		/// </param>
		/// <param name="directoryOfInstalledTmxFiles">Used to limit file deletion to only
		/// include copies of the installed TMX files (plus the generated default file). If this
		/// is <c>null</c>, then all TMX files for the given appID will be deleted from
		/// <paramref name="directoryOfWritableTmxFiles"/></param>
		/// ------------------------------------------------------------------------------------
		public static void DeleteOldTmxFiles(string appId, string directoryOfWritableTmxFiles,
			string directoryOfInstalledTmxFiles)
		{
			//if (Assembly.GetEntryAssembly() == null)
			//    return; // Probably being called in a unit test.
			if (!Directory.Exists(directoryOfWritableTmxFiles))
				return; // Nothing to do.

			var oldDefaultTmxFilePath = Path.Combine(directoryOfWritableTmxFiles, GetTmxFileNameForLanguage(appId, LocalizationManager.kDefaultLang));
			if (!File.Exists(oldDefaultTmxFilePath))
				return; // Cleanup was apparently done previously

			File.Delete(oldDefaultTmxFilePath);

			foreach (var oldTmxFile in Directory.GetFiles(directoryOfWritableTmxFiles,
				         GetTmxFileNameForLanguage(appId, "*")))
			{
				var filename = Path.GetFileName(oldTmxFile);
				if (string.IsNullOrEmpty(directoryOfInstalledTmxFiles) || File.Exists(Path.Combine(directoryOfInstalledTmxFiles, filename)))
				{
					try
					{
						File.Delete(oldTmxFile);
					}
					catch
					{
						// Oh, well, we tried.
					}
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		private static string GetTmxFileNameForLanguage(string appId, string langId) =>
			LocalizationManager.GetTranslationFileNameForLanguage(appId, langId, kFileExtension);
	}
}
