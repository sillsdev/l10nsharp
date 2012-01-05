using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Localization.TMXUtils;
using Localization.UI;

namespace Localization
{
	/// ----------------------------------------------------------------------------------------
	public class LocalizedStringCache
	{
		internal const string kPriorityPropTag = "x-priority";
		internal const string kGroupPropTag = "x-group";
		internal const string kCategoryPropTag = "x-category";
		internal const string kNoLongerUsedPropTag = "x-nolongerused";
		internal const string kToolTipSuffix = "_ToolTip_";
		internal const string kShortcutSuffix = "_ShortcutKeys_";

		// Cannot use Environment.NewLine because that also includes a carriage return
		// character which, when included, messes up the display of text in controls.
		internal const string kOSRealNewline = "\n";

		private readonly string _ampersandReplacement = "|amp|";

		// This is the symbol for a newline that users put in their localized text when
		// they want a real newline inserted. The program will replace literal newlines
		// with the value of kOSNewline.
		internal static string s_literalNewline = "\\n";

		private readonly TransUnitUpdater _tuUpdater;

		internal List<LocTreeNode> LeafNodeList { get; private set; }
		internal LocalizationManager OwningManager { get; private set; }
		public TMXDocument TmxDocument { get; private set; }

		#region Loading methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the string cache from all the specified tmx files
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal LocalizedStringCache(LocalizationManager owningManager)
		{
			OwningManager = owningManager;

			TmxDocument = CreateEmptyStringFile();
			MergeTmxFilesIntoCache(Directory.GetFiles(OwningManager.TmxFileFolder, OwningManager.Id + ".*.tmx"));

			_tuUpdater = new TransUnitUpdater(TmxDocument);

			var replacement = TmxDocument.Header.GetPropValue("x-ampersandreplacement");
			if (replacement != null)
				_ampersandReplacement = replacement;

			replacement = TmxDocument.Header.GetPropValue("x-hardlinebreakreplacement");
			if (replacement != null)
				s_literalNewline = replacement;

			LeafNodeList = new List<LocTreeNode>();
			IsDirty = false;
		}

		/// ------------------------------------------------------------------------------------
		private void MergeTmxFilesIntoCache(IEnumerable<string> tmxFiles)
		{
			var defaultTmxDoc = TMXDocument.Read(OwningManager.DefaultStringFilePath);


			foreach (var file in tmxFiles.Where(f => Path.GetFileName(f) != OwningManager.DefaultStringFilePath))
			{
				var tmxDoc = TMXDocument.Read(file);
				var langId = tmxDoc.Header.SourceLang;
				foreach (var tu in tmxDoc.Body.TransUnits)
				{
					if (defaultTmxDoc.GetTransUnitForId(tu.Id) == null &&
						!tu.Id.EndsWith(kToolTipSuffix) && !tu.Id.EndsWith(kShortcutSuffix))
					{
						tu.AddProp(kNoLongerUsedPropTag, "true");
					}

					TmxDocument.Body.AddTransUnitOrVariantFromExisting(tu, langId);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates an empty string file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal static TMXDocument CreateEmptyStringFile()
		{
			var tmxDoc = new TMXDocument();
			tmxDoc.Header.CreationTool = "Palaso Localization Manager";
			tmxDoc.Header.CreationToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			tmxDoc.Header.SourceLang = LocalizationManager.kDefaultLang;
			tmxDoc.Header.AddProp(LocalizationManager.kAppVersionPropTag, "0.0.0");
			tmxDoc.Header.AddProp("x-hardlinebreakreplacement", s_literalNewline);
			tmxDoc.Header.AddProp("x-hardlinebreakreplacement", s_literalNewline);
			return tmxDoc;
		}

		#endregion

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether the cache has unsaved changes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool IsDirty { get; private set; }

		#endregion

		#region Methods for updating values in cache
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Updates the localized info. in the cache with the info. from the specified
		/// LocalizedObjectInfo.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void UpdateLocalizedInfo(LocalizingInfo locInfo)
		{
			if (_tuUpdater.Update(TmxDocument, locInfo) && !IsDirty)
				IsDirty = true;
		}

		#endregion

		#region Methods for saving cache to disk
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves the cache to the file from which the cache was originally loaded, but only
		/// if the cache is dirty. If the cache is dirty and saved, then true is returned.
		/// Otherwise, false is returned.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool SaveIfDirty()
		{
			if (!IsDirty)
				return false;

			foreach (var langId in TmxDocument.GetAllVariantLanguagesFound())
				SaveFileForLangId(langId);

			IsDirty = false;
			return true;
		}

		/// ------------------------------------------------------------------------------------
		private void SaveFileForLangId(string langId)
		{
			var tmxDoc = CreateEmptyStringFile();
			tmxDoc.Header.SourceLang = langId;
			tmxDoc.Header.SetPropValue(LocalizationManager.kAppVersionPropTag, OwningManager.AppVersion);

			foreach (var tu in TmxDocument.Body.TransUnits)
			{
				var tuv = tu.GetVariantForLang(langId);
				if (tuv == null)
					continue;

				var newTu = new TransUnit { Id = tu.Id };
				tmxDoc.AddTransUnit(newTu);
				newTu.AddVariant(tu.GetVariantForLang(LocalizationManager.kDefaultLang));
				newTu.AddVariant(tuv);
				newTu.Notes = tu.CopyNotes();
				newTu.Props = tu.CopyProps();
			}

			tmxDoc.Body.TransUnits.Sort(TuComparer);

			var tmxFilePath = Path.Combine(OwningManager.TmxFileFolder,
				string.Format("{0}.{1}.tmx", OwningManager.Id, langId));

			tmxDoc.Save(tmxFilePath);
		}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Saves the cache to the specified file, if the cache is dirty. If the cache is
		///// dirty and saved, then true is returned. Otherwise, false is returned.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//private bool SaveIfDirty(string tmxFile)
		//{
		//    if (!IsDirty || string.IsNullOrEmpty(tmxFile))
		//        return false;

		//    //_tmxFile = tmxFile;
		//    IsDirty = false;
		//    TmxDocument.Body.TransUnits.Sort(TuComparer);
		//    TmxDocument.Save(tmxFile);
		//    return true;
		//}

		#endregion

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Compares two translation units for equality.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static int TuComparer(TransUnit tu1, TransUnit tu2)
		{
			if (tu1 == null && tu2 == null)
				return 0;

			if (tu1 == null)
				return -1;

			if (tu2 == null)
				return 1;

			string x = tu1.GetPropValue(kGroupPropTag);
			string y = tu2.GetPropValue(kGroupPropTag);

			if (x == y)
				return tu1.Id.CompareTo(tu2.Id);

			if (x == null)
				return -1;

			if (y == null)
				return 1;

			return x.CompareTo(y);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not translations exist for the specified
		/// string id for the current language id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal bool DoTranslationsExist(string langId, string id)
		{
			var text = GetValueForLangAndId(langId, id, false);
			var toolTip = GetValueForLangAndId(langId, id + kToolTipSuffix, false);
			var shortcutKeys = GetValueForLangAndId(langId, id + kShortcutSuffix, false);

			return (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(toolTip) ||
				!string.IsNullOrEmpty(shortcutKeys));
		}

		#region Methods for getting localized strings (including shortcut key strings)
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized text for the specified id and suffix.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal string GetString(string langId, string id)
		{
			return GetValueForLangAndId(langId, id);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized text for the specified id and suffix. If formatForDisplay is
		/// true, then the string will have ampersands and newlines converted so the text
		/// is displayed nicely at runtime.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal string GetString(string langId, string id, bool formatForDisplay)
		{
			return GetValueForLangAndId(langId, id, formatForDisplay);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized tooltip text for the specified id and suffix.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal string GetToolTipText(string langId, string id)
		{
			return GetValueForLangAndId(langId, id + kToolTipSuffix);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized tooltip text for the specified id and suffix. If
		/// formatForDisplay is true, then the string will have ampersands and newlines
		/// converted so the text is displayed nicely at runtime.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal string GetToolTipText(string langId, string id, bool formatForDisplay)
		{
			return GetValueForLangAndId(langId, id + kToolTipSuffix, formatForDisplay);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized tooltip text for the specified id and suffix.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal Keys GetShortcutKeys(string langId, string id)
		{
			string keys = GetValueForLangAndId(langId, id + kShortcutSuffix);
			return ShortcutKeysEditor.KeysFromString(keys);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized tooltip text for the specified id and suffix.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal string GetShortcutKeysText(string langId, string id)
		{
			return GetValueForLangAndId(langId, id + kShortcutSuffix, false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tries to get a string value for the specified id, starting with the current
		/// language id. If that fails, then the fall back language id is used and if that
		/// fails, then the default (i.e. "en") is used.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private string GetValueForLangAndId(string langId, string id)
		{
			var value = GetValueForLangAndId(langId, id, true);
			if (value != null)
				return value;

			foreach (var fallbackLangId in LocalizationManager.FallbackLanguageIds)
			{
				value = GetValueForLangAndId(fallbackLangId, id, true);
				if (value != null)
					return value;
			}

			return GetValueForLangAndId(LocalizationManager.kDefaultLang, id, true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized value for the specified id. If formatForDisplay is true, then
		/// the string will have ampersands and newlines converted so the text is displayed
		/// nicely at runtime.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private string GetValueForLangAndId(string langId, string id, bool formatForDisplay)
		{
			var tu = TmxDocument.GetTransUnitForId(id);
			if (tu == null)
				return null;

			var tuv = tu.GetVariantForLang(langId);
			if (tuv == null)
				return null;

			var value = tuv.Value;
			if (value == null)
				return null;

			if (formatForDisplay && s_literalNewline != null)
				value = value.Replace(s_literalNewline, kOSRealNewline);

			if (formatForDisplay && _ampersandReplacement != null)
				value = value.Replace(_ampersandReplacement, "&");

			return value;
		}

		#endregion

		#region Methods for getting localized string metadata (e.g. comment)
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the comment for the specified id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal string GetComment(string id)
		{
			TransUnit tu = TmxDocument.GetTransUnitForId(id);
			return (tu == null || tu.Notes.Count == 0 ? null : tu.Notes[0].Text);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the group for the specified id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal string GetGroup(string id)
		{
			TransUnit tu = TmxDocument.GetTransUnitForId(id);
			return (tu == null ? null : tu.GetPropValue(kGroupPropTag));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the priority for the specified id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal LocalizationPriority GetPriority(string id)
		{
			TransUnit tu = TmxDocument.GetTransUnitForId(id);
			if (tu != null)
			{
				string priority = tu.GetPropValue(kPriorityPropTag);

				try
				{
					return (LocalizationPriority)Enum.Parse(typeof(LocalizationPriority), priority);
				}
				catch { }
			}

			return LocalizationPriority.NotLocalizable;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the category for the specified id.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal LocalizationCategory GetCategory(string id)
		{
			TransUnit tu = TmxDocument.GetTransUnitForId(id);
			if (tu != null)
			{
				string category = tu.GetPropValue(kCategoryPropTag);

				try
				{
					return (LocalizationCategory)Enum.Parse(typeof(LocalizationCategory), category);
				}
				catch { }
			}

			return LocalizationCategory.Other;
		}

		#endregion

		#region Methods for loading a tree node collection with all the localizable strings.
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the base id for the specified translation unit id. For each UI object, there
		/// may be up to three translation unit (all having their own id). One for the object's
		/// text, one for the object's tooltip and one for the object's shortcut keys. The
		/// translation unit id for the tooltip and shortcutkeys are just the text's id
		/// with a suffix added. This method will receive a translation unit id and strip off
		/// either of those suffixes, if they are present.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal string GetBaseId(string tuid)
		{
			if (tuid.EndsWith(kToolTipSuffix))
			{
				int i = tuid.LastIndexOf(kToolTipSuffix);
				return tuid.Substring(0, i);
			}

			if (tuid.EndsWith(kShortcutSuffix))
			{
				int i = tuid.LastIndexOf(kShortcutSuffix);
				return tuid.Substring(0, i);
			}

			return tuid;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the specified tree node collection with all the string groups and their
		/// localizable string ids.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void LoadGroupNodes(TreeNodeCollection topCollection)
		{
			TmxDocument.Body.TransUnits.Sort(TuComparer);
			LeafNodeList.Clear();

			foreach (var tu in GetTranslationUnitsForTree())
			{
				string id = GetBaseId(tu.Id);

				var groupChain = ParseGroupAndId(GetGroup(tu.Id), id);
				var nodeKey = string.Empty;
				var nodeCollection = topCollection;
				LocTreeNode newNode;

				for (int i = groupChain.Count - 1; i > 0; i--)
				{
					nodeKey = (nodeKey + "." + groupChain[i]).TrimStart('.');

					var nodes = nodeCollection.Find(nodeKey, true);
					if (nodes.Length > 0)
						nodeCollection = nodes[0].Nodes;
					else
					{
						newNode = new LocTreeNode(OwningManager, groupChain[i], null, nodeKey);
						nodeCollection.Add(newNode);
						nodeCollection = newNode.Nodes;
					}
				}

				nodeKey = nodeKey + ("." + groupChain[0]).TrimStart('.');
				newNode = new LocTreeNode(OwningManager, groupChain[0], id, nodeKey);
				nodeCollection.Add(newNode);
				LeafNodeList.Add(newNode);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a list of only those translation units that should show up in the localizing
		/// dialog's tree control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private IEnumerable<TransUnit> GetTranslationUnitsForTree()
		{
			foreach (var tu in TmxDocument.Body.TransUnits)
			{
				if (tu.GetPropValue(kNoLongerUsedPropTag) == "true")
					continue;

				// If the translation unit is not for a tooltip or shortcutkey, then return it.
				if (!tu.Id.EndsWith(kToolTipSuffix) && !tu.Id.EndsWith(kShortcutSuffix))
					yield return tu;

				// At this point, we know the translation unit is for a tooltip or shortcutkeys.
				// Therefore, we need to determine whether or not the tooltip or shortcutkeys
				// translation unit has an associated 'base' translation unit. If so, then
				// skip the current tooltip or shortcutkeys translation unit since the base
				// one is all that's needed in the tree.
				var tmpId = GetBaseId(tu.Id);
				if (TmxDocument.Body.TransUnits.Any(t => t.Id == tmpId))
					continue;

				// At this point, we know there is not a base translation unit so return the
				// translation unit if it's for a tooltip.
				if (tu.Id.EndsWith(kToolTipSuffix))
					yield return tu;

				// At this point, we know there is not a base translation unit and the current
				// translation unit is for a shortcutkeys. Therefore, only return the current
				// translation unit if there is not associated tooltip translation unit.
				tmpId = tu.Id.Replace(kShortcutSuffix, kToolTipSuffix);
				if (!TmxDocument.Body.TransUnits.Any(t => t.Id == tmpId))
					yield return tu;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Parses the group and id and returns all the pieces between periods.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static List<string> ParseGroupAndId(string group, string id)
		{
			var allPieces = new List<string>();
			string[] pieces;

			if (group != null)
			{
				pieces = group.Split('.');
				foreach (string piece in pieces)
					allPieces.Insert(0, piece);
			}

			pieces = id.Split('.');
			foreach (string piece in pieces)
				allPieces.Insert(0, piece);

			return allPieces;
		}

		#endregion
	}
}
