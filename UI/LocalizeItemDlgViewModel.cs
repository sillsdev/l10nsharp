using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using Localization.Translators;

namespace Localization.UI
{
	internal class LocalizeItemDlgViewModel
	{
		private bool _runInReadonlyMode;
		private readonly Color _untranslatedNodeColor = Color.Peru;
		private string _tgtLangId;
		private string _srcLangId;
		private LocTreeNode _currentNode;
		private TreeNodeCollection _allNodes;
		private BackgroundWorker _translationWorker;

		public List<LocTreeNode> AllLeafNodesShowingInGrid { get; private set; }
		public List<LocTreeNode> AllLeafNodes { get; private set; }
		public List<LocalizationManager> EnabledManagers;
		public ITranslator BingTranslator { get; private set; }
		public NodeComparer.SortField GridSortField { get; set; }
		public SortOrder GridSortOrder { get; set; }

		/// ------------------------------------------------------------------------------------
		public LocalizeItemDlgViewModel(bool runInReadonlyMode)
		{
			_runInReadonlyMode = runInReadonlyMode;
			AllLeafNodesShowingInGrid = new List<LocTreeNode>();
			SetLanguageIds(null, null);
		}

		/// ------------------------------------------------------------------------------------
		public void SetLanguageIds(string srcLangId, string tgtLangId)
		{
			_srcLangId = srcLangId ?? LocalizationManager.FallbackLanguageIds.ToArray()[0];
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
		public LocTreeNode CurrentNode
		{
			get { return _currentNode; }
			set
			{
				_currentNode = value;

				AllLeafNodesShowingInGrid = (_currentNode == null || _currentNode.FirstNode == null ?
					new List<LocTreeNode>() : GetLeafNodesOfNode(_currentNode).ToList());

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
			AllLeafNodes = new List<LocTreeNode>();

			EnabledManagers = LocalizationManager.LoadedManagers.Values
				.OrderBy(lm => lm.Name).ToList();

			foreach (var lm in EnabledManagers)
			{
				_runInReadonlyMode |= !lm.CanCustomizeLocalizations;
				var node = new LocTreeNode(lm, lm.Name, null, lm.Name);
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
		public LocTreeNode FindNode(string id, TreeNodeCollection nodeCollection)
		{
			if (id == null)
				return null;

			foreach (LocTreeNode node in nodeCollection)
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
				foreach (var obj in GetObjectsForId(node.Manager, node.Id).Where(o => o != null))
					node.Manager.ApplyLocalization(obj);
			}

			foreach (var lm in LocalizationManager.LoadedManagers.Values)
			{
				lm.SaveIfDirty();
				// If saving fails, the LocalizationManager will record the problem .
				_runInReadonlyMode |= !lm.CanCustomizeLocalizations;
			}

			return stringsLocalized;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves localization changes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SaveChangesInMemory(LocalizingInfo locInfo)
		{
			if (locInfo == null || locInfo.Id == null || locInfo.UpdateFields == UpdateFields.None)
				return;

			SaveChangesInMemory(CurrentNode, locInfo);
		}

		/// ------------------------------------------------------------------------------------
		public void SaveChangesInMemory(LocTreeNode node, LocalizingInfo locInfo)
		{
			if (locInfo == null || locInfo.Id == null ||
				locInfo.UpdateFields == UpdateFields.None || _tgtLangId == _srcLangId)
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
		private IEnumerable<object> GetObjectsForId(LocalizationManager lm, string id)
		{
			return lm.ObjectCache.Where(kvp => kvp.Value == id).Select(kvp => kvp.Key);
		}

		/// ------------------------------------------------------------------------------------
		public string GetNumberOfTranslatedItemsString()
		{
			int numStringsTranslated = EnabledManagers.Sum(lm => AllLeafNodes.Count(n =>
				(n.GetHasModifications(false) || lm.StringCache.DoTranslationsExist(TgtLangId, n.Id))));

			return string.Format("{0} of {1} Items Translated",
				numStringsTranslated, AllLeafNodes.Count);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Goes through each loaded LocalizationManager, looking for the one whose object
		/// cache contains the specified object. When it's found, that object's id is returned.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetObjIdFromAnyCache(object obj)
		{
			if (obj != null)
			{
				foreach (var manager in LocalizationManager.LoadedManagers.Values)
				{
					string id;
					if (manager.ObjectCache.TryGetValue(obj, out id))
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
				Utils.GetProperty(obj, "Font") as Font) ?? LocalizeItemDlg.DefaultDisplayFont;
		}

		/// ------------------------------------------------------------------------------------
		public object GetFirstObjectForId()
		{
			return CurrentNode.Manager.ObjectCache.FirstOrDefault(k => k.Value == CurrentNode.Id).Key;
		}

		/// ------------------------------------------------------------------------------------
		public void Translate(IDictionary<int, LocTreeNode> nodesToTranslate,
			Action<int, int> progressAction)
		{
			_translationWorker = new BackgroundWorker();
			_translationWorker.WorkerReportsProgress = true;
			_translationWorker.WorkerSupportsCancellation = true;
			_translationWorker.DoWork += HandleTanslateItems;

			if (progressAction != null)
			{
				_translationWorker.ProgressChanged += ((sender, args) =>
					progressAction(args.ProgressPercentage, (int)args.UserState));
			}

			_translationWorker.RunWorkerAsync(nodesToTranslate);
		}

		/// ------------------------------------------------------------------------------------
		private void HandleTanslateItems(object sender, DoWorkEventArgs e)
		{
			var worker = sender as BackgroundWorker;
			var nodesToTranslate = e.Argument as IDictionary<int, LocTreeNode>;
			int i = 0;

			foreach (var kvp in nodesToTranslate)
			{
				var node = kvp.Value;
				var locInfo = new LocalizingInfo(node.Id);
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
		private IEnumerable<LocTreeNode> GetLeafNodesOfNode(LocTreeNode node)
		{
			foreach (var childNode in node.Nodes.Cast<LocTreeNode>())
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
				new NodeComparer(_srcLangId, _tgtLangId, GridSortOrder, GridSortField));
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

			foreach (LocTreeNode node in nodeCollection)
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

	#region NodeComparer class
	/// ----------------------------------------------------------------------------------------
	internal class NodeComparer : IComparer<LocTreeNode>
	{
		internal enum SortField
		{
			Id = 0,
			SourceText = 1,
			TargetText = 2,
			SourceToolTip = 3,
			TargetToolTip = 4
		}

		private readonly string _srcLangId;
		private readonly string _tgtLangId;
		private readonly SortOrder _sortOrder;
		private readonly SortField _sortField;

		/// ------------------------------------------------------------------------------------
		internal NodeComparer(string srcLangId, string tgtLangId, SortOrder sortOrder, SortField sortField)
		{
			_srcLangId = srcLangId;
			_tgtLangId = tgtLangId;
			_sortOrder = sortOrder;
			_sortField = sortField;
		}

		/// ------------------------------------------------------------------------------------
		public int  Compare(LocTreeNode x, LocTreeNode y)
		{
			string xText = string.Empty;
			string yText = string.Empty;

			var prefixToRemove = (x.TreeView != null && x.TreeView.SelectedNode != null ?
				x.TreeView.SelectedNode.Name : string.Empty);

			var ci = CultureInfo.GetCultureInfo("en");

			switch ((int)_sortField)
			{
				case 0:
					xText = x.Id.Replace(prefixToRemove, string.Empty).Trim('.');
					yText = y.Id.Replace(prefixToRemove, string.Empty).Trim('.');
					break;

				case 1:
					xText = x.GetText(_srcLangId) ?? string.Empty;
					yText = y.GetText(_srcLangId) ?? string.Empty;
					ci = CultureInfo.GetCultureInfo(_srcLangId);
					break;

				case 2:
					xText = (x.GetTranslatedText(_tgtLangId) ?? x.GetText(_tgtLangId)) ?? string.Empty;
					yText = (y.GetTranslatedText(_tgtLangId) ?? y.GetText(_tgtLangId)) ?? string.Empty;
					ci = CultureInfo.GetCultureInfo(_tgtLangId);
					break;

				case 3:
					xText = x.GetToolTip(_srcLangId) ?? string.Empty;
					yText = y.GetToolTip(_srcLangId) ?? string.Empty;
					ci = CultureInfo.GetCultureInfo(_srcLangId);
					break;

				case 4:
					xText = (x.GetTranslatedToolTip(_tgtLangId) ?? x.GetToolTip(_tgtLangId)) ?? string.Empty;
					yText = (y.GetTranslatedToolTip(_tgtLangId) ?? y.GetToolTip(_tgtLangId)) ?? string.Empty;
					ci = CultureInfo.GetCultureInfo(_tgtLangId);
					break;
			}

			return (_sortOrder == SortOrder.Ascending ?
				 string.Compare(xText, yText, false, ci) :
				 string.Compare(yText, xText, false, ci));
		}
	}

	#endregion
}
