using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using L10NSharp.Translators;
using L10NSharp;

namespace L10NSharpWinforms.UI
{
	internal class LocalizeItemDlgViewModel<T>
	{
		private bool _runInReadonlyMode;
		private readonly Color _untranslatedNodeColor = Color.Peru;
		private string _tgtLangId;
		private string _srcLangId;
		private LocTreeNode<T> _currentNode;
		private TreeNodeCollection _allNodes;
		private BackgroundWorker _translationWorker;

		public List<LocTreeNode<T>> AllLeafNodesShowingInGrid { get; private set; }
		public List<LocTreeNode<T>> AllLeafNodes { get; private set; }
		public List<ILocalizationManagerInternalWinforms<T>> EnabledManagers;
		private readonly Dictionary<ILocalizationManagerInternalWinforms<T>, HashSet<string>> _modifiedManagersAndLanguages =
			new Dictionary<ILocalizationManagerInternalWinforms<T>, HashSet<string>>();
		public ITranslator BingTranslator { get; private set; }
		public NodeComparer<T>.SortField GridSortField { get; set; }
		public SortOrder GridSortOrder { get; set; }

		/// ------------------------------------------------------------------------------------
		public LocalizeItemDlgViewModel(bool runInReadonlyMode)
		{
			_runInReadonlyMode = runInReadonlyMode;
			AllLeafNodesShowingInGrid = new List<LocTreeNode<T>>();
			SetLanguageIds(null, null);
		}

		/// ------------------------------------------------------------------------------------
		public void SetLanguageIds(string srcLangId, string tgtLangId)
		{
			_srcLangId = srcLangId ?? LocalizationManagerInternal<T>.FallbackLanguageIds.ToArray()[0];
			_tgtLangId = tgtLangId ?? LocalizationManager.UILanguageId;
			CreateTranslator();
		}

		#region Properties
		/// ------------------------------------------------------------------------------------
		public string SrcLangId
		{
			get { return _srcLangId; }
			set
			{
				_srcLangId = value;
				CreateTranslator();
			}
		}

		/// ------------------------------------------------------------------------------------
		public string TgtLangId
		{
			get { return _tgtLangId; }
			set
			{
				_tgtLangId = value;
				CreateTranslator();
			}
		}

		/// ------------------------------------------------------------------------------------
		public LocTreeNode<T> CurrentNode
		{
			get { return _currentNode; }
			set
			{
				_currentNode = value;

				AllLeafNodesShowingInGrid = (_currentNode == null || _currentNode.FirstNode == null ?
					new List<LocTreeNode<T>>() : GetLeafNodesOfNode(_currentNode).ToList());

				SortGridNodes();
			}
		}

		/// ------------------------------------------------------------------------------------
		public void SetCurrentNodeFromGridIndex(int index)
		{
			if (index >= 0 && index < AllLeafNodesShowingInGrid.Count)
				_currentNode = AllLeafNodesShowingInGrid[index];
		}

		/// ------------------------------------------------------------------------------------
		public string CurrentNodeSourceText
		{
			get { return (_currentNode != null ? _currentNode.GetText(_srcLangId) : null); }
		}

		/// ------------------------------------------------------------------------------------
		public string CurrentNodeTargetText
		{
			get
			{
				if (_currentNode == null)
					return null;

				return (_currentNode.GetTranslatedText(_tgtLangId) ?? _currentNode.GetText(_tgtLangId));
			}
		}

		/// ------------------------------------------------------------------------------------
		public string CurrentNodeSourceToolTip
		{
			get { return (_currentNode != null ? _currentNode.GetToolTip(_srcLangId) : null); }
		}

		/// ------------------------------------------------------------------------------------
		public string CurrentNodeTargetToolTip
		{
			get
			{
				if (_currentNode == null)
					return null;

				return (_currentNode.GetTranslatedToolTip(_tgtLangId) ?? _currentNode.GetToolTip(_tgtLangId));
			}
		}

		/// ------------------------------------------------------------------------------------
		public string CurrentNodeSourceShortcutKeys
		{
			get { return (_currentNode != null ? _currentNode.GetShortcutKeys(_srcLangId) : null); }
		}

		/// ------------------------------------------------------------------------------------
		public string CurrentNodeTargetShortcutKeys
		{
			get
			{
				if (_currentNode == null)
					return null;

				return (_currentNode.GetTranslatedShortcutKeys(_tgtLangId) ?? _currentNode.GetShortcutKeys(_tgtLangId));
			}
		}

		/// ------------------------------------------------------------------------------------
		public string CurrentNodeComment
		{
			get { return (_currentNode != null ? _currentNode.GetComment() : null); }
		}

		/// ------------------------------------------------------------------------------------
		public bool TranslatorBusy
		{
			get { return (_translationWorker != null && _translationWorker.IsBusy); }
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		private void CreateTranslator()
		{
			var worker = new BackgroundWorker();
			worker.DoWork += delegate { BingTranslator = new BingTranslator(_srcLangId, _tgtLangId); };
			worker.RunWorkerAsync();
		}

		/// ------------------------------------------------------------------------------------
		public void LoadTreeNodes(TreeView treeVw)
		{
			AllLeafNodes = new List<LocTreeNode<T>>();

			EnabledManagers = LocalizationManagerInternal<T>.LoadedManagers.Values
				.OrderBy(lm => lm.Name).Cast<ILocalizationManagerInternalWinforms<T>>().ToList();

			foreach (var lm in EnabledManagers)
			{
				_runInReadonlyMode |= !lm.CanCustomizeLocalizations;
				var node = new LocTreeNode<T>(lm, lm.Name, null, lm.Name);
				treeVw.Nodes.Add(node);
				var	childNodes = node.Nodes;

				lm.StringCache.LoadGroupNodes(childNodes);
				AllLeafNodes.AddRange(lm.StringCache.LeafNodeList);
				_allNodes = treeVw.Nodes;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Finds the node in the specified node collection having the specified id. If a
		/// node cannot be found, then each nodes node collection will be searched, and so on.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public LocTreeNode<T> FindNode(string id, TreeNodeCollection nodeCollection)
		{
			if (id == null)
				return null;

			foreach (LocTreeNode<T> node in nodeCollection)
			{
				if (node.Id == id)
					return node;

				var nd = FindNode(id, node.Nodes);
				if (nd != null)
					return nd;
			}

			return null;
		}

		/// ------------------------------------------------------------------------------------
		public bool GetIsDirty()
		{
			return (AllLeafNodes.Any(n => n.GetHasModifications(true)));
		}

		/// ------------------------------------------------------------------------------------
		public IEnumerable<ILocalizationManagerInternal<T>> GetModifiedManagers(
			string langId = null) =>
			_modifiedManagersAndLanguages.Where(m => m.Value.Contains(
				langId ?? LocalizationManager.UILanguageId)).Select(m => m.Key);

		/// ------------------------------------------------------------------------------------
		public bool Save()
		{
			if (_runInReadonlyMode)
				return false;

			var stringsLocalized = false;

			foreach (var node in AllLeafNodes.Where(n => n.SavedTranslationInfo.Count > 0))
			{
				stringsLocalized = true;

				foreach (var locInfo in node.SavedTranslationInfo.Values)
					node.Manager.StringCache.UpdateLocalizedInfo(locInfo);

				// Update each object with the specified id, with the localized string(s).
				foreach (var component in GetComponentsForId(node.Manager, node.Id).Where(o => o != null))
					node.Manager.ApplyLocalization(component);
			}

			foreach (var lm in _modifiedManagersAndLanguages.Keys)
			{
				lm.PrepareToCustomizeLocalizations();
				lm.SaveIfDirty(_modifiedManagersAndLanguages[lm]);

				// If saving fails, the LocalizationManagerInternal will record the problem .
				_runInReadonlyMode |= !lm.CanCustomizeLocalizations;
			}

			return stringsLocalized;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves localization changes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SaveChangesInMemory(LocalizingInfoWinforms locInfo)
		{
			if (locInfo?.Id == null || locInfo.UpdateFields == UpdateFields.None)
				return;

			SaveChangesInMemory(CurrentNode, locInfo);
		}

		/// ------------------------------------------------------------------------------------
		public void SaveChangesInMemory(LocTreeNode<T> node, LocalizingInfoWinforms locInfo)
		{
			if (locInfo?.Id == null || locInfo.UpdateFields == UpdateFields.None || _tgtLangId == _srcLangId)
			{
				return;
			}

			if (locInfo.Text == (node.GetText(_tgtLangId) ?? string.Empty) &&
				locInfo.ToolTipText == (node.GetToolTip(_tgtLangId) ?? string.Empty) &&
				locInfo.ShortcutKeys == (node.GetShortcutKeys(_tgtLangId) ?? string.Empty))
			{
				return;
			}

			locInfo.LangId = _tgtLangId;

			if (!_modifiedManagersAndLanguages.TryGetValue(node.Manager, out var modifiedLangIds))
			{
				_modifiedManagersAndLanguages[node.Manager] = modifiedLangIds = new HashSet<string>();
			}
			modifiedLangIds.Add(_tgtLangId);

			if (!node.SavedTranslationInfo.ContainsKey(_tgtLangId))
				node.SavedTranslationInfo[_tgtLangId] = locInfo;
			else
			{
				if ((locInfo.UpdateFields & UpdateFields.Text) == UpdateFields.Text)
					node.SavedTranslationInfo[_tgtLangId].Text = (locInfo.Text == string.Empty ? null : locInfo.Text);

				if ((locInfo.UpdateFields & UpdateFields.ToolTip) == UpdateFields.ToolTip)
					node.SavedTranslationInfo[_tgtLangId].ToolTipText = (locInfo.ToolTipText == string.Empty ? null : locInfo.ToolTipText);

				if ((locInfo.UpdateFields & UpdateFields.ShortcutKeys) == UpdateFields.ShortcutKeys)
					node.SavedTranslationInfo[_tgtLangId].ShortcutKeys = (locInfo.ShortcutKeys == string.Empty ? null : locInfo.ShortcutKeys);

				if ((locInfo.UpdateFields & UpdateFields.Comment) == UpdateFields.Comment)
					node.SavedComment = (locInfo.Comment == string.Empty ? null : locInfo.Comment);

				node.SavedTranslationInfo[_tgtLangId].UpdateFields |= locInfo.UpdateFields;
			}
		}

		/// ------------------------------------------------------------------------------------
		private IEnumerable<IComponent> GetComponentsForId(ILocalizationManagerInternal<T> lm, string id)
		{
			return lm.ComponentCache.Where(kvp => kvp.Value == id).Select(kvp => kvp.Key);
		}

		/// ------------------------------------------------------------------------------------
		public string GetNumberOfTranslatedItemsString()
		{
			int numStringsTranslated = EnabledManagers.Sum(lm => AllLeafNodes.Count(n =>
				(n.GetHasModifications(false) || lm.StringCache.DoTranslationsExist(TgtLangId, n.Id))));

			return $"{numStringsTranslated} of {AllLeafNodes.Count} Items Translated";
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Goes through each loaded LocalizationManagerInternal, looking for the one whose component
		/// cache contains the specified component. When it's found, that component's id is returned.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetObjIdFromAnyCache(IComponent component)
		{
			if (component != null)
			{
				foreach (var manager in LocalizationManagerInternal<T>.LoadedManagers.Values)
				{
					string id;
					if (manager.ComponentCache.TryGetValue(component, out id))
						return id;
				}
			}

			return null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Attempts to get the image for the specified object, if one exists.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Image GetObjectsImage(object obj)
		{
			var img = Utils.GetProperty(obj, "Image") as Image;
			if (img != null)
				return img;

			var imgList = Utils.GetProperty(obj, "ImageList") as ImageList;
			if (imgList != null)
			{
				var index = Utils.GetProperty(obj, "ImageIndex");
				if (index != null && index is int)
				{
					int i = (int)index;
					if (i >= 0 && i < imgList.Images.Count)
						return imgList.Images[i];
				}
			}

			return null;
		}

		/// ------------------------------------------------------------------------------------
		public Font GetFontForObject(object obj)
		{
			return (obj == null ? null :
				Utils.GetProperty(obj, "Font") as Font) ?? LocalizeItemDlg<T>.DefaultDisplayFont;
		}

		/// ------------------------------------------------------------------------------------
		public IComponent GetFirstObjectForId()
		{
			return CurrentNode.Manager.ComponentCache.FirstOrDefault(k => k.Value == CurrentNode.Id).Key;
		}

		/// ------------------------------------------------------------------------------------
		public void Translate(IDictionary<int, LocTreeNode<T>> nodesToTranslate,
			Action<int, int> progressAction)
		{
			_translationWorker = new BackgroundWorker();
			_translationWorker.WorkerReportsProgress = true;
			_translationWorker.WorkerSupportsCancellation = true;
			_translationWorker.DoWork += HandleTranslateItems;

			if (progressAction != null)
			{
				_translationWorker.ProgressChanged += ((sender, args) =>
					progressAction(args.ProgressPercentage, (int)args.UserState));
			}

			_translationWorker.RunWorkerAsync(nodesToTranslate);
		}

		/// ------------------------------------------------------------------------------------
		private void HandleTranslateItems(object sender, DoWorkEventArgs e)
		{
			var worker = sender as BackgroundWorker;
			var nodesToTranslate = e.Argument as IDictionary<int, LocTreeNode<T>>;
			int i = 0;

			foreach (var kvp in nodesToTranslate)
			{
				var node = kvp.Value;
				var locInfo = new LocalizingInfoWinforms(node.Id);
				locInfo.UpdateFields = UpdateFields.None;
				locInfo.LangId = TgtLangId;

				var text = node.GetText(SrcLangId);
				if (!string.IsNullOrEmpty(text))
				{
					text = (BingTranslator.TranslateText(text) ?? string.Empty).Trim();
					if (text != string.Empty)
					{
						locInfo.Text = text;
						locInfo.UpdateFields |= UpdateFields.Text;
					}
				}

				text = node.GetToolTip(SrcLangId);
				if (!string.IsNullOrEmpty(text))
				{
					text = (BingTranslator.TranslateText(text) ?? string.Empty).Trim();
					if (text != string.Empty)
					{
						locInfo.ToolTipText = text;
						locInfo.UpdateFields |= UpdateFields.ToolTip;
					}
				}

				if (locInfo.UpdateFields != UpdateFields.None)
				{
					SaveChangesInMemory(node, locInfo);
					worker.ReportProgress(++i, kvp.Key);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		private IEnumerable<LocTreeNode<T>> GetLeafNodesOfNode(LocTreeNode<T> node)
		{
			foreach (var childNode in node.Nodes.Cast<LocTreeNode<T>>())
			{
				if (AllLeafNodes.Contains(childNode))
					yield return childNode;
				else
				{
					foreach (var leafNode in GetLeafNodesOfNode(childNode))
						yield return leafNode;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		public string GetStringIdForGridIndex(int index, string prefixToRemove)
		{
			if (index >= AllLeafNodesShowingInGrid.Count || index < 0)
				return null;

			var node = AllLeafNodesShowingInGrid[index];
			var id = node.Id;
			if (!string.IsNullOrEmpty(prefixToRemove))
				id = id.Replace(prefixToRemove, string.Empty).Trim('.');
			return id;
		}

		/// ------------------------------------------------------------------------------------
		public void SortGridNodes()
		{
			AllLeafNodesShowingInGrid.Sort(
				new NodeComparer<T>(_srcLangId, _tgtLangId, GridSortOrder, GridSortField));
		}

		#region Methods for returning various grid fields for a specified index
		/// ------------------------------------------------------------------------------------
		public string GetSourceTextForGridIndex(int index)
		{
			return (index >= AllLeafNodesShowingInGrid.Count ? null :
				AllLeafNodesShowingInGrid[index].GetText(_srcLangId));
		}

		/// ------------------------------------------------------------------------------------
		public string GetTargetTextForGridIndex(int index)
		{
			if (index >= AllLeafNodesShowingInGrid.Count)
				return null;

			return (AllLeafNodesShowingInGrid[index].GetTranslatedText(_tgtLangId) ??
				AllLeafNodesShowingInGrid[index].GetText(_tgtLangId));
		}

		/// ------------------------------------------------------------------------------------
		public string GetSourceToolTipForGridIndex(int index)
		{
			return (index >= AllLeafNodesShowingInGrid.Count ? null :
				AllLeafNodesShowingInGrid[index].GetToolTip(_srcLangId));
		}

		/// ------------------------------------------------------------------------------------
		public string GetTargetToolTipForGridIndex(int index)
		{
			if (index >= AllLeafNodesShowingInGrid.Count)
				return null;

			return (AllLeafNodesShowingInGrid[index].GetTranslatedToolTip(_tgtLangId) ??
				AllLeafNodesShowingInGrid[index].GetToolTip(_tgtLangId));
		}

		/// ------------------------------------------------------------------------------------
		public string GetCommentForGridIndex(int index)
		{
			return (index >= AllLeafNodesShowingInGrid.Count ? null :
				AllLeafNodesShowingInGrid[index].GetComment());
		}

		#endregion

		#region Methods for setting node colors
		/// ------------------------------------------------------------------------------------
		public Color SetNodeColors()
		{
			return SetNodeColors(_allNodes);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the color of the specified node (and all its parents) based on whether or
		/// not the node represents a localization entry for which translations exist
		/// (other than for the default, that is).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private Color SetNodeColors(TreeNodeCollection nodeCollection)
		{
			if (nodeCollection == null)
				return Color.Empty;

			var clrParent = Color.Empty;

			foreach (LocTreeNode<T> node in nodeCollection)
			{
				if (node.Nodes.Count > 0)
				{
					clrParent = SetNodeColors(node.Nodes);
					node.ForeColor = clrParent;
				}
				else if (node.Id != null)
				{
					if (node.SavedTranslationInfo != null ||
						node.Manager.StringCache.DoTranslationsExist(_tgtLangId, node.Id))
					{
						node.ForeColor = SystemColors.WindowText;
					}
					else
					{
						node.ForeColor = _untranslatedNodeColor;
						if (clrParent == Color.Empty)
							clrParent = _untranslatedNodeColor;
					}
				}
			}

			return clrParent;
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		public void ShowEditSourceBeforeTranslatingDlg(IWin32Window parent)
		{
			if (BingTranslator == null)
				return;

			using (var dlg = new EditSourceBeforeTranslatingDlg(CurrentNodeSourceText,
				_srcLangId, _tgtLangId, "Bing", BingTranslator))
			{
				dlg.ShowDialog(parent);
			}
		}
	}

}
