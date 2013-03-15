using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Localization.UI;

namespace Localization
{
	/// ----------------------------------------------------------------------------------------
	public class LocalizationManager : IDisposable
	{
		/// ------------------------------------------------------------------------------------
		public const string kDefaultLang = "en";
		internal const string kAppVersionPropTag = "x-appversion";
		internal const string kL10NPrefix = "_L10N_:";

		private static string s_uiLangId;
		private static List<string> s_fallbackLanguageIds = new List<string>(new[] { kDefaultLang });

		private static readonly Dictionary<string, LocalizationManager> s_loadedManagers =
			new Dictionary<string, LocalizationManager>();

		private static Icon _iconForProgressDialogInTaskBar;
		private string m_tmxFileFolder;

		internal Dictionary<object, string> ObjectCache { get; private set; }
		internal Dictionary<Control, ToolTip> ToolTipCtrls { get; private set; }

		#region Static methods for creating a LocalizationManager

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a new instance of a localization manager for the specifed application id.
		/// If a localization manager has already been created for the specified id, then
		/// that is returned.
		/// </summary>
		/// <param name="desiredUiLangId">The language code of the desired UI language. If
		/// there are no translations for that ID, a message is displayed and the UI language
		/// falls back to the default.</param>
		/// <param name="appId">The application Id (e.g. 'Pa' for Phonology Assistant).
		/// This should be a unique name that identifies the manager for an assembly or
		/// application.</param>
		/// <param name="appName">The application's name. This will appear to the user
		/// in the localization dialog box as a parent item in the tree.</param>
		/// <param name="appVersion"></param>
		/// <param name="installedTmxFilePath">The full file path of the original TMX files
		/// installed with the application.</param>
		/// <param name="targetTmxFilePath">The full file path where to copy the TMX files
		/// found in 'installedTmxFilePath' so they can be edited by the user. If the
		/// value is null, the default location is used (which is appName combined with
		/// Environment.SpecialFolder.CommonApplicationData)</param>
		/// <param name="iconForProgressDialogInTaskBar"> </param>
		/// <param name="emailForSubmissions">This will be used in UI that helps the translator
		/// know what to do with their work</param>
		/// <param name="namespaceBeginnings">A list of namespace beginnings indicating
		/// what types to scan for localized string calls. For example, to only scan
		/// types found in Pa.exe and assuming all types in that assembly begin with
		/// 'Pa', then this value would only contain the string 'Pa'.</param>
		/// ------------------------------------------------------------------------------------
		public static LocalizationManager Create(string desiredUiLangId, string appId,
			string appName, string appVersion, string installedTmxFilePath, string targetTmxFilePath, Icon iconForProgressDialogInTaskBar,
			string emailForSubmissions,
			params string[] namespaceBeginnings)
		{
			EmailForSubmissions = emailForSubmissions;
			_iconForProgressDialogInTaskBar = iconForProgressDialogInTaskBar;
			if (targetTmxFilePath == null)
			{
				targetTmxFilePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				targetTmxFilePath = Path.Combine(targetTmxFilePath, appName);
			}

			SetUILanguage(desiredUiLangId ?? kDefaultLang, false);

			LocalizationManager lm;
			if (!LoadedManagers.TryGetValue(appId, out lm))
			{
				lm = new LocalizationManager(appId, appName, appVersion,
					installedTmxFilePath, targetTmxFilePath, namespaceBeginnings);

				LoadedManagers[appId] = lm;
			}

			EnableClickingOnControlToBringUpLocalizationDialog = true;
			VerifyThatUILangHasTranslations();
			return lm;
		}

		/// ------------------------------------------------------------------------------------
		internal static Dictionary<string, LocalizationManager> LoadedManagers
		{
			get { return s_loadedManagers; }
		}

		#endregion

		#region LocalizationManager construction/disposal
		/// ------------------------------------------------------------------------------------
		private LocalizationManager(string appId, string appName, string appVersion,
			string installedTmxFilePath, string tmxFolder, params string[] namespaceBeginnings)
		{
			Id = appId;
			Name = appName;
			AppVersion = appVersion;
			TmxFileFolder = tmxFolder;

			try
			{
				new FileIOPermission(FileIOPermissionAccess.Write, TmxFileFolder).Demand();
				CanCustomizeLocalizations = true;
				// Make sure the folder exists.
				if (!Directory.Exists(TmxFileFolder))
					Directory.CreateDirectory(TmxFileFolder);

				CreateOrUpdateDefaultTmxFileIfNecessary(namespaceBeginnings);
				CopyInstalledTmxFilesToWritableLocation(installedTmxFilePath);
			}
			catch (Exception e)
			{
				if (e is SecurityException || e is UnauthorizedAccessException || e is IOException)
				{
					CanCustomizeLocalizations = false;
					// If a user with access to the target folder has never run the application,
					// fall back to the install location.
					if (!File.Exists(DefaultStringFilePath))
						TmxFileFolder = installedTmxFilePath;
				}
				else
					throw;
			}

			ObjectCache = new Dictionary<object, string>();
			ToolTipCtrls = new Dictionary<Control, ToolTip>();
			StringCache = new LocalizedStringCache(this);
		}

		/// ------------------------------------------------------------------------------------
		private void CreateOrUpdateDefaultTmxFileIfNecessary(params string[] namespaceBeginnings)
		{
			if (File.Exists(DefaultStringFilePath))
			{
				var xmlDoc = XElement.Load(DefaultStringFilePath);
				var verElement = xmlDoc.Element("header").Elements("prop")
					.FirstOrDefault(e => (string)e.Attribute("type") == kAppVersionPropTag);

				if (verElement != null && new Version(verElement.Value) >= new Version(AppVersion ?? "0.0.1"))
					return;
			}

			// Before wasting a bunch of time, make sure we can open the file for writing.
			var fileStream = File.Open(DefaultStringFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
			fileStream.Close();

			var tmxDoc = LocalizedStringCache.CreateEmptyStringFile();
			tmxDoc.Header.SetPropValue(kAppVersionPropTag, AppVersion);
			var tuUpdater = new TransUnitUpdater(tmxDoc);

			using (var dlg = new InitializationProgressDlg(Name, namespaceBeginnings))
			{
				dlg.Icon = _iconForProgressDialogInTaskBar;
				dlg.ShowDialog();
				foreach (var locInfo in dlg.ExtractedInfo)
					tuUpdater.Update(locInfo);
			}

			tmxDoc.Save(DefaultStringFilePath);
		}

		/// ------------------------------------------------------------------------------------
		private void CopyInstalledTmxFilesToWritableLocation(string installedTmxFilePath)
		{
			if (installedTmxFilePath == null)
				return;

			foreach (var installedFile in Directory.GetFiles(installedTmxFilePath, Id + "*.tmx"))
			{
				var targetFile = Path.Combine(TmxFileFolder, Path.GetFileName(installedFile));

				if (!File.Exists(targetFile))
					File.Copy(installedFile, targetFile);
			}
		}

		///// ------------------------------------------------------------------------------------
		//private string GetLangIdFromTmxFileName(string fileName)
		//{
		//    fileName = fileName.Substring(0, fileName.Length - 4);
		//    int i = fileName.LastIndexOf('.');
		//    return (i < 0 ? null : fileName.Substring(i + 1));
		//}

		/// ------------------------------------------------------------------------------------
		public void Dispose()
		{
			if (LoadedManagers.ContainsKey(Id))
				LoadedManagers.Remove(Id);
		}

		#endregion

		#region Methods for getting, setting and verifying UI language id
		/// ------------------------------------------------------------------------------------
		public static void SetUILanguage(string langId,
			bool reapplyLocalizationsToAllObjectsInAllManagers)
		{
			if (UILanguageId == langId || string.IsNullOrEmpty(langId))
				return;

			var ci = CultureInfo.GetCultureInfo(langId);
			Thread.CurrentThread.CurrentUICulture = ci;
			s_uiLangId = langId;

			if (reapplyLocalizationsToAllObjectsInAllManagers)
				ReapplyLocalizationsToAllObjectsInAllManagers();
		}

		/// ------------------------------------------------------------------------------------
		public static IEnumerable<CultureInfo> GetUILanguages(bool returnOnlyLanguagesHavingLocalizations)
		{
			var allLangs = from ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures)
						   where ci.TwoLetterISOLanguageName != "iv"
						   orderby ci.DisplayName
						   select ci;

			if (!returnOnlyLanguagesHavingLocalizations)
				return allLangs;

			var langsHavinglocalizations = (LoadedManagers == null ? new List<string>() :
				LoadedManagers.Values.SelectMany(lm => lm.StringCache.TmxDocument.GetAllVariantLanguagesFound())
				.Distinct().ToList());

			return from ci in allLangs
				   where langsHavinglocalizations.Contains(ci.Name)
				   orderby ci.DisplayName
				   select ci;
		}

		/// ------------------------------------------------------------------------------------
		public static bool VerifyThatUILangHasTranslations()
		{
			// REVIEW: Consider combining with SetUILanguage method

			var ci = CultureInfo.GetCultureInfo(UILanguageId);
			if (GetUILanguages(true).Contains(ci) || UILanguageId == kDefaultLang)
				return true;

			var defaultCultureInfo = CultureInfo.GetCultureInfo(kDefaultLang);
			var msg = string.Format("Your user interface language was previously set to {0} " +
				"but there are no localziations found for that language. Therefore, your user " +
				"interface language will revert to {1}. It's possible the file that contains " +
				"your localized strings is corrupt or missing.", ci.DisplayName,
				defaultCultureInfo.DisplayName);

			MessageBox.Show(msg, Application.ProductName);
			SetUILanguage(kDefaultLang, false);
			return false;
		}

		///// ------------------------------------------------------------------------------------
		//public static string SetUILanguageFromCommandLineArgs(IEnumerable<string> commandLineArgs)
		//{
		//    string langId = null;

		//    if (commandLineArgs != null)
		//    {
		//        // Specifying the UI language on the command-line trumps the one in
		//        // the settings file (i.e. the one set in the options dialog box).
		//        foreach (var arg in commandLineArgs
		//            .Where(arg => arg.ToLower().StartsWith("/uilang:") || arg.ToLower().StartsWith("-uilang:")))
		//        {
		//            langId = arg.Substring(8);
		//            break;
		//        }
		//    }

		//    langId = (string.IsNullOrEmpty(langId) ? kDefaultLang : langId);
		//    SetUILanguage(langId, false);
		//    return langId;
		//}

		#endregion

		#region Methods for showing localization dialog box
		/// ------------------------------------------------------------------------------------
		public void ShowLocalizationDialogBox(bool runInReadonlyMode)
		{
			LocalizeItemDlg.ShowDialog(this, null, runInReadonlyMode);
		}

		/// ------------------------------------------------------------------------------------
//		public static void ShowLocalizationDialogBox()
//		{
//            TipDialog.Show("If you click on an item while you hold ctrl and shift keys down, this tool will open up with that item already selected.");
//            LocalizeItemDlg.ShowDialog(null, null, false);
//		}

		/// ------------------------------------------------------------------------------------
		public static void ShowLocalizationDialogBox(object ctrl)
		{
			TipDialog.Show("If you click on an item while you hold ctrl and shift keys down, this tool will open up with that item already selected.");
			LocalizeItemDlg.ShowDialog(GetLocalizationManagerForObject(ctrl), ctrl, false);
		}

		/// ------------------------------------------------------------------------------------
		public static void ShowLocalizationDialogBox(string id)
		{
			TipDialog.Show("If you click on an item while you hold ctrl and shift keys down, this tool will open up with that item already selected.");
			LocalizeItemDlg.ShowDialog(GetLocalizationManagerForString(id), id, false);
		}

		#endregion

		#region Public Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the list of langauges (by id) used to fallback to when looking for a
		/// string in the current UI language fails. The fallback order goes from the first
		/// item in this list to the last.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static IEnumerable<string> FallbackLanguageIds
		{
			get { return s_fallbackLanguageIds; }
			set
			{
				if (s_fallbackLanguageIds != null && s_fallbackLanguageIds.Count > 0)
					s_fallbackLanguageIds = value.ToList();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the current UI language Id (i.e. the target language).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string UILanguageId
		{
			get
			{
				if (s_uiLangId == null)
				{
					s_uiLangId = Thread.CurrentThread.CurrentUICulture.Name;
					int i = s_uiLangId.IndexOf('-');
					if (i >= 0)
						s_uiLangId = s_uiLangId.Substring(0, i);
				}

				return s_uiLangId;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This is what identifies a localization manager for a particular set of
		/// localized strings. This would likely be a DLL or EXE name like 'PA' or 'SayMore'.
		/// This will be the file name of the portion of the TMX file in which localized
		/// strings are stored. This would usually be the name of the assembly that owns a
		/// set of localized strings.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Id { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This is the presentable name for the set of localized strings. For example, the
		/// Id might be 'PA' but the LocalizationSetName might be 'Phonology Assistant'.
		/// This should be a name presentable to the user.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Name { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This is sent from the application that's creating the localization manager. It's
		/// written to the TMX file and used to determine whether or not the application needs
		/// to be rescanned for localized strings.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string AppVersion { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Full file name and path to the default string file (i.e. English strings).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal string DefaultStringFilePath { get; private set; }

		/// ------------------------------------------------------------------------------------
		internal LocalizedStringCache StringCache { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not user has authority to change localized strings.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool CanCustomizeLocalizations { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the full path (without file nanme) to the TMX file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string TmxFileFolder
		{
			get { return m_tmxFileFolder; }
			private set
			{
				m_tmxFileFolder = value;
				DefaultStringFilePath = GetTmxPathForLanguage(kDefaultLang);
			}
		}
		#endregion

		#region Methods for caching and localizing objects.
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the specified object to the localization manager's cache of objects to be
		/// localized and then applies localizations for the current UI language.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal bool RegisterObjectForLocalizing(object obj, string id, string defaultText,
			string defaultTooltip, string defaultShortcutKeys, string comment)
		{
			if (obj == null || id == null || id.Trim() == string.Empty)
				return false;

			try
			{

				// This if/else used to be more concise but sometimes there were occassions
				// adding an item the first time using ObjectCache[obj] = id would throw an
				// index outside the bounds of the array exception. I have no clue why nor
				// can I reliably reproduce the error nor do I know if this change will solve
				// the problem. Hopefully it will, but my guess is the same underlying code
				// will be called.
				if (ObjectCache.ContainsKey(obj))
					ObjectCache[obj] = id;  //somehow, we sometimes see "Msg: Index was outside the bounds of the array."
				else
				{
					// If this is the first time this object has passed this way, then
					// prepare it to be available for end-user localization.
					PrepareObjectForRuntimeLocalization(obj);
					ObjectCache.Add(obj, id);
				}

				return true;
			}
			catch (Exception)
			{
				#if DEBUG
				throw; // if you hit this ( Index was outside the bounds of the array) try to figure out why. What is the hash (?) value for the obj?
				#endif
			}
			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Prepares the specified object for runtime localization by subscribing to a
		/// mouse down event that will monitor whether or not to show the localization
		/// dialog box.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void PrepareObjectForRuntimeLocalization(object obj)
		{
			if (obj is ToolStripItem)
			{
				((ToolStripItem)obj).MouseDown += HandleToolStripItemMouseDown;
				((ToolStripItem)obj).Disposed += HandleToolStripItemDisposed;
			}
			else if (obj is Control)
			{
				((Control)obj).HandleDestroyed += HandleControlHandleDestroyed;

				TabPage tpg = obj as TabPage;
				if (tpg != null && tpg.Parent is TabControl)
					tpg.Parent.MouseDown += HandleControlMouseDown;
				else
					((Control)obj).MouseDown += HandleControlMouseDown;
			}
			else if (obj is ColumnHeader && ((ColumnHeader)obj).ListView != null)
			{
				((ColumnHeader)obj).ListView.HandleDestroyed += HandleListViewHandleDestroyed;
				((ColumnHeader)obj).ListView.ColumnClick += HandleListViewColumnHeaderClicked;
			}
			else if (obj is DataGridViewColumn && ((DataGridViewColumn)obj).DataGridView != null)
			{
				((DataGridViewColumn)obj).DataGridView.HandleDestroyed += HandleDataGridViewHandleDestroyed;
				((DataGridViewColumn)obj).DataGridView.CellMouseDown += HandleDataGridViewCellMouseDown;
			}
		}

		/// ------------------------------------------------------------------------------------
		internal void SaveIfDirty()
		{
			try
			{
				StringCache.SaveIfDirty();
			}
			catch (IOException e)
			{
				CanCustomizeLocalizations = false;
				MessageBox.Show(e.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

		}

		/// ------------------------------------------------------------------------------------
		internal string GetTmxPathForLanguage(string langId)
		{
			return Path.Combine(TmxFileFolder, string.Format("{0}.{1}.tmx", Id, langId));
		}
		#endregion

		#region Methods for adding localized strings to cache.
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Add a localized string to the string cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void AddString(string id, string defaultText, string defaultTooltip,
			string defaultShortcutKeys, string comment)
		{
			var locInfo = new LocalizingInfo(id)
			{
				Text = defaultText,
				ToolTipText = defaultTooltip,
				ShortcutKeys = defaultShortcutKeys,
				Comment = comment,
				LangId = kDefaultLang
			};

			StringCache.UpdateLocalizedInfo(locInfo);
		}

		#endregion

		#region Non static methods for getting localized strings
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized text for the specified object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetLocalizedString(object obj, string id, string defaultText,
			string defaultTooltip, string defaultShortcutKeys, string comment)
		{
			return GetLocalizedString(id, defaultText);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized text for the specified id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetLocalizedString(string id, string defaultText)
		{
			var text = (UILanguageId != kDefaultLang ? GetStringFromStringCache(UILanguageId, id) : null);

			return (text ?? StripOffLocalizationInfoFromText(defaultText));
		}

		/// ------------------------------------------------------------------------------------
		private string GetStringFromStringCache(string uiLangId, string id)
		{
			return StringCache.GetString(uiLangId, id);
		}

		/// ------------------------------------------------------------------------------------
		private string GetTooltipFromStringCache(string uiLangId, string id)
		{
			return StringCache.GetToolTipText(uiLangId, id);
		}

		/// ------------------------------------------------------------------------------------
		private Keys GetShortCutKeyFromStringCache(string uiLangId, string id)
		{
			return StringCache.GetShortcutKeys(uiLangId, id);
		}

		#endregion

		#region GetString static methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the text for the specified object. The englishText is returned when the text
		/// for the specified object cannot be found for the current UI language.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetStringForObject(object obj, string englishText)
		{
			var lm = GetLocalizationManagerForObject(obj);

			if (lm != null)
			{
				string id;
				if (lm.ObjectCache.TryGetValue(obj, out id))
					return lm.GetLocalizedString(id, englishText);
			}

			return (StripOffLocalizationInfoFromText(englishText ??
				Utils.GetProperty(obj, "Text") as string));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a string for the specified string id. The englishText is returned when
		/// a string cannot be found for the specified id and the current UI language.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetString(string id, string englishText)
		{
			return GetString(id, englishText, null, null, null, null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a string for the specified string id. The englishText is returned when
		/// a string cannot be found for the specified id and the current UI language.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetString(string id, string englishText, string comment)
		{
			return GetString(id, englishText, comment, null, null, null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a string for the specified string id. The englishText is returned when
		/// a string cannot be found for the specified id and the current UI language.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetString(string id, string englishText, string comment, object obj)
		{
			return GetString(id, englishText, comment, null, null, obj);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a string for the specified string id. The englishText is returned when
		/// a string cannot be found for the specified id and the current UI language.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetString(string id, string englishText, string comment,
			string englishToolTipText, string englishShortcutKey, object obj)
		{
			if (obj != null)
			{
				var lm = GetLocalizationManagerForObject(obj) ??
					GetLocalizationManagerForString(id);

				if (lm != null)
				{
					lm.RegisterObjectForLocalizing(obj, id, englishText,
						englishToolTipText, englishShortcutKey, comment);

					return lm.GetLocalizedString(id, englishText);
				}
			}

			return GetStringFromAnyLocalizationManager(id) ??
				StripOffLocalizationInfoFromText(englishText);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a string for the specified application id and string id. When a string for the
		/// specified id cannot be found, then one is added  using the specified englishText is
		/// returned when a string cannot be found for the specified id and the current UI
		/// language.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetDynamicString(string appId, string id, string englishText)
		{
			return GetDynamicString(appId, id, englishText, null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a string for the specified application id and string id. When a string for the
		/// specified id cannot be found, then one is added  using the specified englishText is
		/// returned when a string cannot be found for the specified id and the current UI
		/// language.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetDynamicString(string appId, string id, string englishText, string comment)
		{
			LocalizationManager lm;
			if (!LoadedManagers.TryGetValue(appId, out lm))
			{
				throw new ArgumentException(
					string.Format("The application id '{0}' does not have an associated localization manager.",
					appId));
			}

			var text = lm.GetStringFromStringCache(UILanguageId, id);
			if (text != null)
				return text;

			var locInfo = new LocalizingInfo(id) { LangId = kDefaultLang, Text = englishText };
			locInfo.UpdateFields = UpdateFields.Text;

			if (!string.IsNullOrEmpty(comment))
			{
				locInfo.Comment = comment;
				locInfo.UpdateFields |= UpdateFields.Comment;
			}

			lm.StringCache.UpdateLocalizedInfo(locInfo);
			lm.SaveIfDirty();
			return englishText;
		}

		/// ------------------------------------------------------------------------------------
		public static bool GetIsStringAvailableForLangId(string id, string langId)
		{
			return LoadedManagers.Values.Select(lm => lm.StringCache.GetString(langId, id))
				.FirstOrDefault(txt => txt != null) != null;
		}

		/// ------------------------------------------------------------------------------------
		public static string StripOffLocalizationInfoFromText(string text)
		{
			if (text == null || !text.StartsWith(kL10NPrefix))
				return text;

			text = text.Substring(kL10NPrefix.Length);
			int i = text.IndexOf("!", StringComparison.Ordinal);
			return (i < 0 ? text : text.Substring(i + 1));
		}

		/// ------------------------------------------------------------------------------------
		private static string GetStringFromAnyLocalizationManager(string id)
		{
			// This will enforce that the text to localize is just returned to the caller
			// when the default language id is the same as the current UI langauge id.
			if (UILanguageId == kDefaultLang)
				return null;

			return LoadedManagers.Values.Select(lm => lm.StringCache.GetString(UILanguageId, id))
				.FirstOrDefault(text => text != null);
		}

		/// ------------------------------------------------------------------------------------
		private static LocalizationManager GetLocalizationManagerForObject(object obj)
		{
			return LoadedManagers.Values.FirstOrDefault(lm => lm.ObjectCache.ContainsKey(obj));
		}

		/// ------------------------------------------------------------------------------------
		private static LocalizationManager GetLocalizationManagerForString(string id)
		{
			return LoadedManagers.Values.FirstOrDefault(
				lm => lm.StringCache.GetString(UILanguageId, id) != null);
		}

		#endregion

		#region Methods that apply localizations to an object.
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Reapplies the localizations to all objects in the localization manager's cache of
		/// localized objects.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void ReapplyLocalizationsToAllObjectsInAllManagers()
		{
			if (LoadedManagers == null)
				return;

			foreach (var lm in LoadedManagers.Values)
				lm.ReapplyLocalizationsToAllObjects();
		}
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Reapplies the localizations to all objects in the localization manager's cache of
		/// localized objects.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void ReapplyLocalizationsToAllObjects(string localizationManagerId)
		{
			if (LoadedManagers == null)
				return;

			LocalizationManager lm;
			if (LoadedManagers.TryGetValue(localizationManagerId, out lm))
				lm.ReapplyLocalizationsToAllObjects();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Reapplies the localizations to all objects in the localization manager's cache of
		/// localized objects.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void ReapplyLocalizationsToAllObjects()
		{
			foreach (object obj in ObjectCache.Keys)
				ApplyLocalization(obj);

			LocalizeItemDlg.FireStringsLocalizedEvent();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Recreates the tooltip control and updates the tooltip text for each object having
		/// a tooltip. This is necessary sometimes when controls get moved from form to form
		/// during runtime.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void RefreshToolTips()
		{
			foreach (var toolTipCtrl in ToolTipCtrls.Values)
				toolTipCtrl.Dispose();

			ToolTipCtrls.Clear();

			// This used to be a for-each, but on rare occassions, a "Collection was
			// modified; enumeration operation may not execute" exception would be
			// thrown. This should solve the problem.
			var controls = ObjectCache.Where(x => x.Key is Control).ToArray();
			for (int i = 0; i < controls.Length; i++)
			{
				var toolTipText = GetTooltipFromStringCache(UILanguageId, controls[i].Value);
				if(!string.IsNullOrEmpty(toolTipText)) //JH: hoping to speed this up a bit
					ApplyLocalizedToolTipToControl((Control)controls[i].Key, toolTipText);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void ApplyLocalization(object obj)
		{
			if (obj == null)
				return;

			string id;
			if (!ObjectCache.TryGetValue(obj, out id))
				return;

			if (ApplyLocalizationsToControl(obj as Control, id))
				return;

			if (ApplyLocalizationsToToolStripItem(obj as ToolStripItem, id))
				return;

			if (ApplyLocalizationsToListViewColumnHeader(obj as ColumnHeader, id))
				return;

			ApplyLocalizationsToDataGridViewColumn(obj as DataGridViewColumn, id);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private bool ApplyLocalizationsToControl(Control ctrl, string id)
		{
			if (ctrl == null)
				return false;

			var text = GetStringFromStringCache(UILanguageId, id);
			var toolTipText = GetTooltipFromStringCache(UILanguageId, id);

			if (text != null && string.CompareOrdinal(ctrl.Text, text) != 0)
				ctrl.Text = text;

			ApplyLocalizedToolTipToControl(ctrl, toolTipText);
			return true;
		}

		/// ------------------------------------------------------------------------------------
		private void ApplyLocalizedToolTipToControl(Control ctrl, string toolTipText)
		{
			var topctrl = GetRealTopLevelControl(ctrl);
			if (topctrl == null)
				return;

			// Check if the control's top level control has a reference to a tooltip. If
			// it does, then use that tooltip for assigning tooltip text to the control.
			// Otherwise, create a new tooltip and reference it using the control's top
			// level control.
			ToolTip ttctrl;
			if (!ToolTipCtrls.TryGetValue(topctrl, out ttctrl))
			{
				if (string.IsNullOrEmpty(toolTipText))
					return;

				ttctrl = new ToolTip();
				ToolTipCtrls[topctrl] = ttctrl;
				topctrl.ParentChanged += HandleToolTipRefChanged;
				topctrl.HandleDestroyed += HandleToolTipRefDestroyed;
			}

			ttctrl.SetToolTip(ctrl, toolTipText);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the real top level control (using the control's TopLevelControl property
		/// seems to return null until the control is on a form).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static Control GetRealTopLevelControl(Control ctrl)
		{
			var pctrl = ctrl;
			while (pctrl.Parent != null)
				pctrl = pctrl.Parent;

			return pctrl;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the case when a tooltip instance was created and assinged to a top level
		/// control that has now been added to another control, thus making the other control
		/// top level instead. Therefore, we need to make sure the tooltip is reassigned to
		/// the new top level control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleToolTipRefChanged(object sender, EventArgs e)
		{
			var oldtopctrl = sender as Control;
			var newtopctrl = GetRealTopLevelControl(oldtopctrl);
			if (oldtopctrl == null || newtopctrl == null)
				return;

			oldtopctrl.ParentChanged -= HandleToolTipRefChanged;
			newtopctrl.ParentChanged += HandleToolTipRefChanged;
			RefreshToolTips();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles removing tooltip controls from the global tool tip collection for top level
		/// controls that are destroyed and have controls on them using tool tip controls from
		/// that collection.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleToolTipRefDestroyed(object sender, EventArgs e)
		{
			var topctrl = sender as Control;
			if (topctrl == null)
				return;

			topctrl.ParentChanged -= HandleToolTipRefChanged;
			topctrl.HandleDestroyed -= HandleToolTipRefDestroyed;

			ToolTip ttctrl;
			if (ToolTipCtrls.TryGetValue(topctrl, out ttctrl))
				ttctrl.Dispose();

			ToolTipCtrls.Remove(topctrl);
		}

		/// ------------------------------------------------------------------------------------
		private bool ApplyLocalizationsToToolStripItem(ToolStripItem item, string id)
		{
			if (item == null)
				return false;

			var text = GetStringFromStringCache(UILanguageId, id);
			var toolTipText = GetTooltipFromStringCache(UILanguageId, id);
			item.Text = (text ?? StripOffLocalizationInfoFromText(item.Text));
			item.ToolTipText = (toolTipText ?? StripOffLocalizationInfoFromText(item.ToolTipText));

			var shortcutKeys = GetShortCutKeyFromStringCache(UILanguageId, id);
			if (item is ToolStripMenuItem && shortcutKeys != Keys.None)
				((ToolStripMenuItem)item).ShortcutKeys = shortcutKeys;

			return true;
		}

		/// ------------------------------------------------------------------------------------
		private bool ApplyLocalizationsToListViewColumnHeader(ColumnHeader hdr, string id)
		{
			if (hdr == null)
				return false;

			var text = GetStringFromStringCache(UILanguageId, id);
			hdr.Text = (text ?? StripOffLocalizationInfoFromText(hdr.Text));
			return true;
		}

		/// ------------------------------------------------------------------------------------
		private bool ApplyLocalizationsToDataGridViewColumn(DataGridViewColumn col, string id)
		{
			if (col == null)
				return false;

			var text = GetStringFromStringCache(UILanguageId, id);
			col.HeaderText = (text ?? StripOffLocalizationInfoFromText(col.HeaderText));
			return true;
		}

		#endregion

		#region Mouse down, handle destroyed, and dispose handlers
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles Ctrl-Shift-Click on ToolStripItems;
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleToolStripItemMouseDown(object sender, MouseEventArgs e)
		{
			if (!DoHandleMouseDown)
				return;

			// Make sure all drop-downs are closed that are in the
			// chain of menu items for this item.
			var tsddi = sender as ToolStripDropDownItem;
			while (tsddi != null)
			{
				tsddi.DropDown.Close();

				if (tsddi.Owner is ContextMenuStrip)
					((ContextMenuStrip)tsddi.Owner).Close();

				tsddi = tsddi.OwnerItem as ToolStripDropDownItem;
			}

			LocalizeItemDlg.ShowDialog(this, sender, false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Set this to false to make Localization Manager ignore clicks on the UI
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static bool EnableClickingOnControlToBringUpLocalizationDialog { get; set; }

		private static bool DoHandleMouseDown
		{
			get
			{
				return EnableClickingOnControlToBringUpLocalizationDialog &&
					Control.ModifierKeys == (Keys.Shift | Keys.Control);
			}
		}

		public static string EmailForSubmissions;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the tool strip item disposed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleToolStripItemDisposed(object sender, EventArgs e)
		{
			var item = sender as ToolStripItem;
			if (item != null)
			{
				item.MouseDown -= HandleToolStripItemMouseDown;
				item.Disposed -= HandleToolStripItemDisposed;

				if (ObjectCache.ContainsKey(item))
					ObjectCache.Remove(item);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles Ctrl-Shift-Click on controls.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleControlMouseDown(object sender, MouseEventArgs e)
		{
			if (!DoHandleMouseDown)
				return;

			var ctrl = sender as Control;

			if (ctrl is TabControl)
			{
				var tabctrl = ctrl as TabControl;
				for (int i = 0; i < tabctrl.TabPages.Count; i++)
				{
					if (tabctrl.GetTabRect(i).Contains(e.Location))
					{
						ctrl = tabctrl.TabPages[i];
						break;
					}
				}
			}

			LocalizeItemDlg.ShowDialog(this, ctrl, false);
		}

		/// ------------------------------------------------------------------------------------
		internal void HandleControlHandleDestroyed(object sender, EventArgs e)
		{
			var ctrl = sender as Control;
			if (ctrl == null)
				return;

			ctrl.HandleDestroyed -= HandleControlHandleDestroyed;
			ctrl.MouseDown -= HandleControlMouseDown;

			if (ObjectCache.ContainsKey(ctrl))
				ObjectCache.Remove(ctrl);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles ListView column header clicks. Unfortunately, even if the localization
		/// dialog box is shown, this click on the header will not get eaten (like it does
		/// for other controls). Therefore, if clicking on the column header sorts the column,
		/// that sorting will take place after the dialog box is closed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleListViewColumnHeaderClicked(object sender, ColumnClickEventArgs e)
		{
			if (DoHandleMouseDown)
				return;

			var lv = sender as ListView;
			if (lv != null && ObjectCache.ContainsKey(lv.Columns[e.Column]))
				LocalizeItemDlg.ShowDialog(this, lv.Columns[e.Column], false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// When controls get destroyed, do a little clean up.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleListViewHandleDestroyed(object sender, EventArgs e)
		{
			var lv = sender as ListView;
			if (lv == null)
				return;

			lv.HandleDestroyed -= HandleListViewHandleDestroyed;
			lv.ColumnClick -= HandleListViewColumnHeaderClicked;

			foreach (var hdr in lv.Columns.Cast<ColumnHeader>().Where(h => ObjectCache.ContainsKey(h)))
				ObjectCache.Remove(hdr);
		}

		/// ------------------------------------------------------------------------------------
		internal void HandleDataGridViewCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (!DoHandleMouseDown)
				return;

			var grid = sender as DataGridView;
			if (grid != null && e.RowIndex < 0 && ObjectCache.ContainsKey(grid.Columns[e.ColumnIndex]))
				LocalizeItemDlg.ShowDialog(this, grid.Columns[e.ColumnIndex], false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// When controls get destroyed, do a little clean up.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleDataGridViewHandleDestroyed(object sender, EventArgs e)
		{
			var grid = sender as DataGridView;
			if (grid == null)
				return;

			grid.HandleDestroyed -= HandleDataGridViewHandleDestroyed;
			grid.CellMouseDown -= HandleDataGridViewCellMouseDown;

			foreach (DataGridViewColumn col in grid.Columns.Cast<DataGridViewColumn>()
				.Where(col => ObjectCache.ContainsKey(col)))
			{
				ObjectCache.Remove(col);
			}
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			return Id + ", " + Name;
		}
	}
}
