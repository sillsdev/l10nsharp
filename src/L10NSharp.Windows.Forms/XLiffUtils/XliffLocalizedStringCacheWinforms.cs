using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;
using L10NSharp.Windows.Forms.UIComponents;
using L10NSharp.XLiffUtils;
using L10NSharp;

namespace L10NSharp.Windows.Forms.XLiffUtils
{
	/// ----------------------------------------------------------------------------------------
	internal class XliffLocalizedStringCacheWinforms : XliffLocalizedStringCache, ILocalizedStringCacheWinforms<XLiffDocument>
	{

		public List<LocTreeNode<XLiffDocument>> LeafNodeList { get; private set; }
		//internal XliffLocalizationManagerWinforms OwningManager { get; private set; }
		private XLiffDocument DefaultXliffDocument { get; set; } // matches LanguageManager.kDefaultLanguage

		/// <summary>
		/// Record the xliff document loaded for each language. Use this with care...XLiff documents are only
		/// loaded as needed, so unless _unloadedXliffDocuments is empty, XliffDocuments won't necessarily
		/// contain the one you want or have a complete list of keys. This class has its own GetDocument,
		/// TryGetDocument, and AvailableLangKeys which should usually be used instead. To help enforce this,
		/// XliffDocuments should be kept private, and any access to it should go through methods that
		/// take lazy loading of Xliff documents into account.
		/// </summary>
		private readonly ConcurrentDictionary<string, XLiffDocument> XliffDocuments = new ConcurrentDictionary<string, XLiffDocument>();

		private readonly ConcurrentDictionary<string, string> _unloadedXliffDocuments = new ConcurrentDictionary<string, string>();

		#region Loading methods

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the string cache from all the specified Xliff files
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal XliffLocalizedStringCacheWinforms(ILocalizationManager owningManager, bool loadAvailableXliffFiles = true) : base(owningManager, loadAvailableXliffFiles)
		{
			LeafNodeList = new List<LocTreeNode<XLiffDocument>>();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized tooltip text for the specified id and suffix.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Keys GetShortcutKeys(string langId, string id)
		{
			string keys = GetValueForLangAndIdWithFallback(langId, id + kShortcutSuffix);
			return ShortcutKeysEditor.KeysFromString(keys);
		}

		/*/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized tooltip text for the specified id and suffix.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetShortcutKeysText(string langId, string id)
		{
			return GetValueForExactLangAndId(langId, id + kShortcutSuffix, false);
		}*/

		#endregion

		#region Methods for loading a tree node collection with all the localizable strings.

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the specified tree node collection with all the string groups and their
		/// localizable string ids.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void LoadGroupNodes(TreeNodeCollection topCollection)
		{
			LeafNodeList.Clear();

			foreach (var tu in GetTranslationUnitsForTree())
			{
				string id = GetBaseId(tu.Id);
				var groupChain = ParseGroupAndId(GetGroup(tu.Id), id);
				var nodeKey = string.Empty;
				var nodeCollection = topCollection;
				LocTreeNode<XLiffDocument> newNode;

				for (int i = groupChain.Count - 1; i > 0; i--)
				{
					nodeKey = (nodeKey + "." + groupChain[i]).TrimStart('.');

					var nodes = nodeCollection.Find(nodeKey, true);
					if (nodes.Length > 0)
						nodeCollection = nodes[0].Nodes;
					else
					{
						newNode = new LocTreeNode<XLiffDocument>((XliffLocalizationManagerWinforms)OwningManager, groupChain[i], null,
						nodeKey);
						nodeCollection.Add(newNode);
						nodeCollection = newNode.Nodes;
					}
				}

				nodeKey = nodeKey + ("." + groupChain[0]).TrimStart('.');
				newNode = new LocTreeNode<XLiffDocument>((XliffLocalizationManagerWinforms)OwningManager, groupChain[0], id, nodeKey);
				nodeCollection.Add(newNode);
				LeafNodeList.Add(newNode);
			}
		}
		#endregion
	}
}
