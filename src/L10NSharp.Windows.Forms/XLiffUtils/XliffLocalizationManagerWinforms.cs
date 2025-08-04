using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using L10NSharp.Windows.Forms.UIComponents;
using L10NSharp.XLiffUtils;
using L10NSharp;

namespace L10NSharp.Windows.Forms.XLiffUtils
{
	/// ----------------------------------------------------------------------------------------
	internal class XliffLocalizationManagerWinforms : XliffLocalizationManager, ILocalizationManagerInternalWinforms<XLiffDocument>
	{
		/// ------------------------------------------------------------------------------------
		private static Icon _applicationIcon;
		public Dictionary<Control, ToolTip> ToolTipCtrls { get; }
		public Dictionary<ILocalizableComponent, Dictionary<string, LocalizingInfoWinforms>> LocalizableComponents { get; }

		#region XliffLocalizationManager construction/disposal
		/// ------------------------------------------------------------------------------------
		internal XliffLocalizationManagerWinforms(string appId, string origExtension, string appName,
			string appVersion, string directoryOfInstalledXliffFiles,
			string directoryForGeneratedDefaultXliffFile, string directoryOfUserModifiedXliffFiles,
			IEnumerable<MethodInfo> additionalLocalizationMethods,
			params string[] namespaceBeginnings) :
			base(appId, origExtension, appName, appVersion, directoryOfInstalledXliffFiles,
				directoryForGeneratedDefaultXliffFile, directoryOfUserModifiedXliffFiles,
				additionalLocalizationMethods, namespaceBeginnings)
		{
			ToolTipCtrls = new Dictionary<Control, ToolTip>();
			StringCache = new XliffLocalizedStringCacheWinforms(this);
			LocalizableComponents = new Dictionary<ILocalizableComponent,
				Dictionary<string, LocalizingInfoWinforms>>();
		}

		/// <summary> Sometimes, on Linux, there is an empty DefaultStringFile.  This causes problems. </summary>
		private bool DefaultStringFileExistsAndHasContents()
		{
			return File.Exists(DefaultStringFilePath) && !string.IsNullOrWhiteSpace(File.ReadAllText(DefaultStringFilePath));
		}

		#endregion

		#region Public Properties
		/// ------------------------------------------------------------------------------------
		public new ILocalizedStringCacheWinforms<XLiffDocument> StringCache { get; }

		#endregion

		#region Methods for caching and localizing objects.
		/// <summary>
		/// Adds the specified component to the localization manager's cache of objects to be
		/// localized and then applies localizations for the current UI language.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void RegisterComponentForLocalizing(IComponent component, string id, string
		defaultText, string defaultTooltip, string defaultShortcutKeys, string comment)
		{
			RegisterComponentForLocalizing(new LocalizingInfoWinforms(component, id)
			{
				Text = defaultText,
				ToolTipText = defaultTooltip,
				ShortcutKeys = defaultShortcutKeys,
				Comment = comment
			}, null);
		}

		public void RegisterComponentForLocalizing(LocalizingInfoWinforms info,
			Action<ILocalizationManagerInternalWinforms, LocalizingInfoWinforms> successAction)
		{
			var component = info.Component;
			var id = info.Id;
			if (component == null || string.IsNullOrWhiteSpace(id))
				return;

			try
			{

				// This if/else used to be more concise but sometimes there were occasions
				// adding an item the first time using ComponentCache[component] = id would throw an
				// index outside the bounds of the array exception. I have no clue why nor
				// can I reliably reproduce the error nor do I know if this change will solve
				// the problem. Hopefully it will, but my guess is the same underlying code
				// will be called.
				if (ComponentCache.ContainsKey(component))
					ComponentCache[component] = id;  //somehow, we sometimes see "Msg: Index was outside the bounds of the array."
				else
				{
					var lm = LocalizationManagerInternalWinforms<XLiffDocument>.GetLocalizationManagerForString(id);
					if (lm != null && lm != this)
					{
						lm.RegisterComponentForLocalizing(info, successAction);
						return;
					}
					if (component is ILocalizableComponent)
						ComponentCache.Add(component, id);
					else
					{
						// If this is the first time this object has passed this way, then
						// prepare it to be available for end-user localization.
						PrepareComponentForRuntimeLocalization(component);
						ComponentCache.Add(component, id);
						// Make it available for the config dialog to localize.
						StringCache.UpdateLocalizedInfo(info);
					}
				}

				successAction?.Invoke(this, info);
			}
			catch (Exception)
			{
#if DEBUG
				throw; // if you hit this ( Index was outside the bounds of the array) try to figure out why. What is the hash (?) value for the component?
#endif
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Prepares the specified component for runtime localization by subscribing to a
		/// mouse down event that will monitor whether or not to show the localization
		/// dialog box.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void PrepareComponentForRuntimeLocalization(IComponent component)
		{
			var toolStripItem = component as ToolStripItem;
			if (toolStripItem != null)
			{
				toolStripItem.MouseDown += HandleToolStripItemMouseDown;
				toolStripItem.Disposed += HandleToolStripItemDisposed;
				return;
			}

			// For component that are part of an owning parent control that needs to
			// do some special handling when the user wants to localize, we need
			// the parent to subscribe to the mouse event, but we don't want to
			// subscribe once per column/page, so we first unsubscribe and then
			// subscribe. It's a little ugly, but there doesn't seem to be a better way:
			// http://stackoverflow.com/questions/399648/preventing-same-event-handler-assignment-multiple-times

			var ctrl = component as Control;
			if (ctrl != null)
			{
				ctrl.Disposed += HandleControlDisposed;

				TabPage tpg = ctrl as TabPage;
				if (tpg != null && tpg.Parent is TabControl)
				{
					tpg.Parent.MouseDown -= HandleControlMouseDown;
					tpg.Parent.MouseDown += HandleControlMouseDown;
					tpg.Parent.Disposed -= HandleControlDisposed;
					tpg.Parent.Disposed += HandleControlDisposed;
					tpg.Disposed += HandleTabPageDisposed;
					return;
				}

				ctrl.MouseDown += HandleControlMouseDown;
				return;
			}

			var columnHeader = component as ColumnHeader;
			if (columnHeader != null && columnHeader.ListView != null)
			{
				columnHeader.ListView.Disposed -= HandleListViewDisposed;
				columnHeader.ListView.Disposed += HandleListViewDisposed;
				columnHeader.ListView.ColumnClick -= HandleListViewColumnHeaderClicked;
				columnHeader.ListView.ColumnClick += HandleListViewColumnHeaderClicked;
				columnHeader.Disposed += HandleListViewColumnDisposed;
			}

			var dataGridViewColumn = component as DataGridViewColumn;
			if (dataGridViewColumn != null && dataGridViewColumn.DataGridView != null)
			{
				dataGridViewColumn.DataGridView.CellMouseDown -= HandleDataGridViewCellMouseDown;
				dataGridViewColumn.DataGridView.CellMouseDown += HandleDataGridViewCellMouseDown;
				dataGridViewColumn.DataGridView.Disposed -= HandleDataGridViewDisposed;
				dataGridViewColumn.DataGridView.Disposed += HandleDataGridViewDisposed;
				dataGridViewColumn.Disposed += HandleColumnDisposed;
			}
		}
		#endregion

		#region Methods for showing localization dialog box
		/// ------------------------------------------------------------------------------------
		public void ShowLocalizationDialogBox(bool runInReadonlyMode, IWin32Window owner = null)
		{
			LocalizeItemDlg<XLiffDocument>.ShowDialog(this, "", runInReadonlyMode, owner);
		}

		/// ------------------------------------------------------------------------------------
		public static void ShowLocalizationDialogBox(IComponent component,
			IWin32Window owner = null)
		{
			if (owner == null)
				owner = (component as Control)?.FindForm();
			TipDialog.ShowAltShiftClickTip(owner);
			LocalizeItemDlg<XLiffDocument>.ShowDialog(LocalizationManagerInternal<XLiffDocument>.GetLocalizationManagerForComponent(component),
				component, false, owner);
		}

		/// ------------------------------------------------------------------------------------
		public static void ShowLocalizationDialogBox(string id, IWin32Window owner = null)
		{
			TipDialog.ShowAltShiftClickTip(owner);
			LocalizeItemDlg<XLiffDocument>.ShowDialog(LocalizationManagerInternal<XLiffDocument>.GetLocalizationManagerForString(id), id, false, owner);
		}

		#endregion

		#region Non static methods for getting localized strings
		/// ------------------------------------------------------------------------------------
		private Keys GetShortCutKeyFromStringCache(string uiLangId, string id)
		{
			var realLangId = LocalizationManagerInternal<XLiffDocument>.MapToExistingLanguageIfPossible(uiLangId);
			return StringCache.GetShortcutKeys(realLangId, id);
		}

		#endregion

		#region Methods that apply localizations to an object.
		public void ApplyLocalizationsToILocalizableComponent(LocalizingInfoWinforms locInfo)
		{
			if (locInfo.Component is ILocalizableComponent locComponent &&
			    LocalizableComponents.TryGetValue(locComponent, out var idToLocInfo))
			{
				ApplyLocalizationsToLocalizableComponent(locComponent, idToLocInfo);
				return;
			}
#if DEBUG
			var msg =
				"Either locInfo.component is not an ILocalizableComponent or LocalizableComponents hasn't been updated with id={0}.";
			throw new ApplicationException(string.Format(msg, locInfo.Id));
#endif
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Reapplies the localizations to all components in the localization manager's cache of
		/// localized components.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public new void ReapplyLocalizationsToAllComponents()
		{
			foreach (var component in ComponentCache.Keys)
				ApplyLocalization(component);

			LocalizeItemDlg<XLiffDocument>.FireStringsLocalizedEvent(this);
		}
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Recreates the tooltip control and updates the tooltip text for each object having
		/// a tooltip. This is necessary sometimes when controls get moved from form to form
		/// during runtime.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public new void RefreshToolTips()
		{
			foreach (var toolTipCtrl in ToolTipCtrls.Values)
				toolTipCtrl.Dispose();

			ToolTipCtrls.Clear();

			// This used to be a for-each, but on rare occasions, a "Collection was
			// modified; enumeration operation may not execute" exception would be
			// thrown. This should solve the problem.
			var controls = ComponentCache.Where(x => x.Key is Control).ToArray();
			foreach (var ctrl in controls)
			{
				var toolTipText = GetTooltipFromStringCache(UILanguageId, ctrl.Value);
				if (!string.IsNullOrEmpty(toolTipText)) //JH: hoping to speed this up a bit
					ApplyLocalizedToolTipToControl((Control)ctrl.Key, toolTipText);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public new void ApplyLocalization(IComponent component)
		{
			if (component == null)
				return;

			if (!ComponentCache.TryGetValue(component, out var id))
				return;

			if (component is ILocalizableComponent locComponent)
			{
				if (LocalizableComponents.TryGetValue(locComponent, out var idToLocInfo))
				{
					ApplyLocalizationsToLocalizableComponent(locComponent, idToLocInfo);
					return;
				}
			}

			if (ApplyLocalizationsToControl(component as Control, id))
				return;

			if (ApplyLocalizationsToToolStripItem(component as ToolStripItem, id))
				return;

			if (ApplyLocalizationsToListViewColumnHeader(component as ColumnHeader, id))
				return;

			ApplyLocalizationsToDataGridViewColumn(component as DataGridViewColumn, id);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes the specified ILocalizableComponent.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void ApplyLocalizationsToLocalizableComponent(
			ILocalizableComponent locComponent, Dictionary<string, LocalizingInfoWinforms> idToLocInfo)
		{
			if (locComponent == null)
				return;

			foreach (var kvp in idToLocInfo)
			{
				var id = kvp.Key;
				var locInfo = kvp.Value;
				locComponent.ApplyLocalizationToString(locInfo.Component, id, GetLocalizedString(id, locInfo.Text));
			}
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
			var topCtrl = LocalizationManagerInternalWinforms<XLiffDocument>.GetRealTopLevelControl(ctrl);
			if (topCtrl == null)
				return;

			// Check if the control's top level control has a reference to a tooltip. If
			// it does, then use that tooltip for assigning tooltip text to the control.
			// Otherwise, create a new tooltip and reference it using the control's top
			// level control.
			if (!ToolTipCtrls.TryGetValue(topCtrl, out var ttCtrl))
			{
				if (string.IsNullOrEmpty(toolTipText))
					return;

				ttCtrl = new ToolTip();
				ToolTipCtrls[topCtrl] = ttCtrl;
				topCtrl.ParentChanged += HandleToolTipRefChanged;
				topCtrl.HandleDestroyed += HandleToolTipRefDestroyed;
			}

			ttCtrl.SetToolTip(ctrl, toolTipText);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the case when a tooltip instance was created and assigned to a top level
		/// control that has now been added to another control, thus making the other control
		/// top level instead. Therefore, we need to make sure the tooltip is reassigned to
		/// the new top level control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleToolTipRefChanged(object sender, EventArgs e)
		{
			var oldTopCtrl = sender as Control;
			var newTopCtrl = LocalizationManagerInternalWinforms<XLiffDocument>.GetRealTopLevelControl(oldTopCtrl);
			if (oldTopCtrl == null || newTopCtrl == null)
				return;

			oldTopCtrl.ParentChanged -= HandleToolTipRefChanged;
			newTopCtrl.ParentChanged += HandleToolTipRefChanged;
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
			if (!(sender is Control topCtrl))
				return;

			topCtrl.ParentChanged -= HandleToolTipRefChanged;
			topCtrl.HandleDestroyed -= HandleToolTipRefDestroyed;

			if (ToolTipCtrls.TryGetValue(topCtrl, out var ttCtrl))
				ttCtrl.Dispose();

			ToolTipCtrls.Remove(topCtrl);
		}

		/// ------------------------------------------------------------------------------------
		private bool ApplyLocalizationsToToolStripItem(ToolStripItem item, string id)
		{
			if (item == null)
				return false;

			var text = GetStringFromStringCache(UILanguageId, id);
			var toolTipText = GetTooltipFromStringCache(UILanguageId, id);
			item.Text = text ?? LocalizationManager.StripOffLocalizationInfoFromText(item.Text);
			item.ToolTipText = toolTipText ?? LocalizationManager.StripOffLocalizationInfoFromText(item.ToolTipText);

			var shortcutKeys = GetShortCutKeyFromStringCache(UILanguageId, id);
			if (item is ToolStripMenuItem menuItem && shortcutKeys != Keys.None)
				menuItem.ShortcutKeys = shortcutKeys;

			return true;
		}

		/// ------------------------------------------------------------------------------------
		private bool ApplyLocalizationsToListViewColumnHeader(ColumnHeader hdr, string id)
		{
			if (hdr == null)
				return false;

			var text = GetStringFromStringCache(UILanguageId, id);
			hdr.Text = text ?? LocalizationManager.StripOffLocalizationInfoFromText(hdr.Text);
			return true;
		}

		/// ------------------------------------------------------------------------------------
		private bool ApplyLocalizationsToDataGridViewColumn(DataGridViewColumn col, string id)
		{
			if (col == null)
				return false;

			var text = GetStringFromStringCache(UILanguageId, id);
			col.HeaderText = text ?? LocalizationManager.StripOffLocalizationInfoFromText(col.HeaderText);
			col.ToolTipText = GetTooltipFromStringCache(UILanguageId, id);
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
			Form owningForm = null;
			if (sender is ToolStripDropDownItem tsddi)
			{
				owningForm = tsddi.Owner?.FindForm();
				while (tsddi != null)
				{
					tsddi.DropDown.Close();

					if (tsddi.Owner is ContextMenuStrip menuStrip)
						menuStrip.Close();

					tsddi = tsddi.OwnerItem as ToolStripDropDownItem;
				}
			}

			LocalizeItemDlg<XLiffDocument>.ShowDialog(this, (IComponent)sender, false,
				owningForm);
		}

		private static bool DoHandleMouseDown =>
			LocalizationManager.EnableClickingOnControlToBringUpLocalizationDialog &&
			Control.ModifierKeys == (Keys.Alt | Keys.Shift);

		public Icon ApplicationIcon
		{
			get => _applicationIcon;
			set => _applicationIcon = value;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the tool strip item disposed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleToolStripItemDisposed(object sender, EventArgs e)
		{
			if (sender is ToolStripItem item)
			{
				item.MouseDown -= HandleToolStripItemMouseDown;
				item.Disposed -= HandleToolStripItemDisposed;

				ComponentCache.Remove(item);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles Alt-Shift-Click on controls.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleControlMouseDown(object sender, MouseEventArgs e)
		{
			if (!DoHandleMouseDown)
				return;

			var ctrl = sender as Control;

			if (ctrl is TabControl tabControl)
			{
				for (int i = 0; i < tabControl.TabPages.Count; i++)
				{
					if (tabControl.GetTabRect(i).Contains(e.Location))
					{
						ctrl = tabControl.TabPages[i];
						break;
					}
				}
			}

			var lm = LocalizationManagerInternal<XLiffDocument>.GetLocalizationManagerForComponent(ctrl);

			LocalizationManager.OnLaunchingLocalizationDialog(lm);
			LocalizeItemDlg<XLiffDocument>.ShowDialog(lm, ctrl, false, ctrl?.FindForm());
			LocalizationManager.OnClosingLocalizationDialog(lm);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// When controls get destroyed, do a little clean-up.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleControlDisposed(object sender, EventArgs e)
		{
			var ctrl = sender as Control;
			if (ctrl == null)
				return;

			ctrl.Disposed -= HandleControlDisposed;
			ctrl.MouseDown -= HandleControlMouseDown;

			ComponentCache.Remove(ctrl);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// When a TabPage gets disposed, remove reference to it from the object cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleTabPageDisposed(object sender, EventArgs e)
		{
			if (!(sender is TabPage tabPage))
				return;

			tabPage.Disposed -= HandleTabPageDisposed;
			ComponentCache.Remove(tabPage);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// When DataGridView controls get disposed, do a little clean-up.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleDataGridViewDisposed(object sender, EventArgs e)
		{
			if (!(sender is DataGridView grid))
				return;

			grid.Disposed -= HandleControlDisposed;
			grid.CellMouseDown -= HandleDataGridViewCellMouseDown;

			ComponentCache.Remove(grid);
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
			if (!DoHandleMouseDown)
				return;

			if (sender is ListView lv && ComponentCache.ContainsKey(lv.Columns[e.Column]))
				LocalizeItemDlg<XLiffDocument>.ShowDialog(this, lv.Columns[e.Column], false,
					lv.FindForm());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// When ListView controls get disposed, do a little clean-up.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleListViewDisposed(object sender, EventArgs e)
		{
			if (!(sender is ListView lv))
				return;

			lv.Disposed -= HandleListViewDisposed;
			lv.ColumnClick -= HandleListViewColumnHeaderClicked;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// When ListView ColumnHeader controls get disposed, remove the reference to it from the
		/// object cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleListViewColumnDisposed(object sender, EventArgs e)
		{
			if (!(sender is ColumnHeader column))
				return;

			column.Disposed -= HandleListViewColumnDisposed;
			ComponentCache.Remove(column);
		}

		/// ------------------------------------------------------------------------------------
		internal void HandleDataGridViewCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (!DoHandleMouseDown)
				return;

			if (sender is DataGridView grid && e.RowIndex < 0 &&
			    ComponentCache.ContainsKey(grid.Columns[e.ColumnIndex]))
			{
				LocalizeItemDlg<XLiffDocument>.ShowDialog(this, grid.Columns[e.ColumnIndex], false,
					grid.FindForm());
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// When DataGridViewColumn controls get disposed, remove the reference to it from the
		/// object cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void HandleColumnDisposed(object sender, EventArgs e)
		{
			if (!(sender is DataGridViewColumn column))
				return;

			column.Disposed -= HandleColumnDisposed;
			ComponentCache.Remove(column);
		}
		#endregion
	}
}
