using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using L10NSharp.XLiffUtils;

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

		internal XliffLocalizationManagerWinforms(string appId, string appName, string appVersion) : base(appId, appName, appVersion)
		{
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
					var lm = LocalizationManagerInternalWinforms<XLiffDocument>.GetLocalizationManagerForStringWinforms(id);
					if (lm != null && lm != this)
					{
						lm.RegisterComponentForLocalizing(info, successAction);
						return;
					}
					if (component is ILocalizableComponent)
						ComponentCache.Add(component, id);
					else
					{
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
		public void ReapplyLocalizationsToAllComponents()
		{
			foreach (var component in ComponentCache.Keys)
				ApplyLocalization(component);
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
		public void ApplyLocalization(IComponent component)
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

		public Icon ApplicationIcon
		{
			get => _applicationIcon;
			set => _applicationIcon = value;
		}
	}
}
