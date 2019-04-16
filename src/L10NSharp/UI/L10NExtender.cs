// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2010, SIL International. All Rights Reserved.
// <copyright from='2009' to='2010' company='SIL International'>
//		Copyright (c) 2010, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: LocalizingExtender.cs
// Responsibility: D. Olson
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace L10NSharp.UI
{
	/// ----------------------------------------------------------------------------------------
	[ProvideProperty("LocalizingId", typeof(IComponent))]
	[ProvideProperty("LocalizableToolTip", typeof(IComponent))]
	[ProvideProperty("LocalizationPriority", typeof(IComponent))]
	[ProvideProperty("LocalizationComment", typeof(IComponent))]
	public class L10NSharpExtender : Component, IExtenderProvider, ISupportInitialize
	{
		private static HashSet<Type> s_doNotExtend;

		#pragma warning disable CS0414	// field is assigned a value but its value is never used
		// Required for Windows.Forms Class Composition Designer support
		private Container components = null;
		#pragma warning restore CS0414
		private Dictionary<object, LocalizingInfo> m_extendedCtrls;
		private ILocalizationManagerInternal _manager;
		private string _locManagerId;
		//private string _idPrefixForThisForm="";
		private bool _okayToLocalizeControls = false;

		#region Constructors
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Constructor for instance that supports Class Composition designer.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public L10NSharpExtender(IContainer container) : this()
		{
			container.Add(this);
		}

		/// ------------------------------------------------------------------------------------
		public L10NSharpExtender()
		{
			// Required for Windows.Forms Class Composition Designer support
			components = new Container();

			m_extendedCtrls = new Dictionary<object, LocalizingInfo>();
		}

		#endregion

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the id of the localization manager associated with this extender.
		/// This is only used at runtime so the extender can create a reference to one of the
		/// loaded localization managers stored in LocalizationManagerInternal{T}.LoadedManagers.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string LocalizationManagerId
		{
			get { return _locManagerId; }
			set
			{
				if (LocalizationManager.LoadedManagers == null)
				{
					return;
				}

				Debug.Assert(ReallyDesignMode || value != null, "You need to enter the manager/package id for this L10NExtender");

				_locManagerId = value;
				if (value != null && !DesignMode && LocalizationManager.LoadedManagers.ContainsKey(_locManagerId))
				{
					_manager = LocalizationManager.LoadedManagers[_locManagerId];
					LocalizeControls();
				}
			}
		}

		protected bool ReallyDesignMode
		{
			get
			{
				return (base.DesignMode || GetService(typeof(IDesignerHost)) != null) ||
					(LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			}
		}

		/// <summary>
		/// This id will be prepended to all new items added to this page.
		/// </summary>
		[Localizable(false)]
		[Category("Localizing Properties")]
		public string PrefixForNewItems{get;set;}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets or sets the localization group.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//public string LocalizationGroup { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the collection of controls that are not extended.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static HashSet<Type> ControlsNotExtended
		{
			get
			{
				if (s_doNotExtend == null)
				{
					s_doNotExtend = new HashSet<Type>();
					//s_doNotExtend.Add(typeof(TextBox));
					//s_doNotExtend.Add(typeof(MaskedTextBox));
					//s_doNotExtend.Add(typeof(NumericUpDown));
					s_doNotExtend.Add(typeof(ListBox));
					s_doNotExtend.Add(typeof(ListView));
					s_doNotExtend.Add(typeof(TreeView));
					s_doNotExtend.Add(typeof(TabControl));
					s_doNotExtend.Add(typeof(ProgressBar));
					s_doNotExtend.Add(typeof(RichTextBox));
					s_doNotExtend.Add(typeof(ToolStripSeparator));
					s_doNotExtend.Add(typeof(ToolStripProgressBar));
					s_doNotExtend.Add(typeof(StatusStrip));
					s_doNotExtend.Add(typeof(ProgressBar));
					s_doNotExtend.Add(typeof(SplitterPanel));
					s_doNotExtend.Add(typeof(Splitter));
					s_doNotExtend.Add(typeof(SplitContainer));
					s_doNotExtend.Add(typeof(Panel));
					s_doNotExtend.Add(typeof(TableLayoutPanel));
					s_doNotExtend.Add(typeof(FlowLayoutPanel));
					s_doNotExtend.Add(typeof(L10NSharpExtender));
				}

				return s_doNotExtend;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not the extender is currently in design mode.
		/// I have had some problems with the base class' DesignMode property being true
		/// when in design mode. I'm not sure why, but adding a couple more checks fixes the
		/// problem.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private new bool DesignMode
		{
			get
			{
				return (base.DesignMode || GetService(typeof(IDesignerHost)) != null) ||
					(LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			}
		}

		#endregion

		#region IExtenderProvider Members
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Used to determine if a specific form component instance supports this attribute
		/// extension. Allows custom attributes to target specific control types.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool CanExtend(object extendee)
		{
			if (ControlsNotExtended.Contains(extendee.GetType()))
				return false;

			return (extendee is Control || extendee is ToolStripItem ||
				extendee is ColumnHeader || extendee is ILocalizableComponent);
		}

		#endregion

		#region ISupportInitialize Members
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Signals the object that initialization is starting.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void BeginInit()
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Signals the object that initialization is complete. If the manager has been set
		/// (i.e., a valid LocalizationManagerId was supplied), then the controls will be
		/// localized.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void EndInit()
		{
			try
			{
				if (DesignMode)
					return;

				_okayToLocalizeControls = true;

				LocalizeControls();
			}
			catch (Exception)
			{
#if DEBUG
				throw;
#endif
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This method goes through the collection of the controls that have been extended
		/// and adds to or udates the default values in the string files. Then each extended
		/// control is localized.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void LocalizeControls()
		{
			if (!_okayToLocalizeControls || m_extendedCtrls == null || _manager == null)
				return;

			FinalizationForListViewColumnHeaders();
			FinalizationForDataGridViewColumns();
			FinalizationForILocalizableComponents();

			// Now make sure each extended control is localized.
			foreach (var locInfo in m_extendedCtrls.Values
				.Where(li => li.Priority != LocalizationPriority.NotLocalizable))
			{
				if (string.IsNullOrEmpty(locInfo.LangId))
					locInfo.LangId = LocalizationManager.kDefaultLang;
				// Depending on the order in which VS Designer decides to initialize fields, locInfo may be originally created before the Text of the
				// control is set. If so, obtain it again, unless this is an ILocalizableComponent. In that case, the above Finalization
				// method should have taken care of this.
				if (locInfo.Category != LocalizationCategory.LocalizableComponent && string.IsNullOrWhiteSpace(locInfo.Text))
					locInfo.UpdateTextFromObject();
				// Special case: the Text of a column header is "ColumnHeader" before it is ever set.
				// This means that if we first processed the CH before we set its text, we have noted
				// "ColumnHeader" as its default English name. Get the real one if it has since been updated.
				var ch = locInfo.Component as ColumnHeader;
				if (ch != null && ch.Text != "ColumnHeader" && locInfo.Text == "ColumnHeader")
					locInfo.UpdateTextFromObject();
				_manager.RegisterComponentForLocalizing(locInfo, (lm, info) =>
				{
					if (info.Category == LocalizationCategory.LocalizableComponent)
					{
						lm.ApplyLocalizationsToILocalizableComponent(info);
					}
					else
					{
						lm.ApplyLocalization(info.Component);
					}
				});
			}

			m_extendedCtrls = null;
			_okayToLocalizeControls = false;
		}

		private void FinalizationForILocalizableComponents()
		{
			if (m_extendedCtrls == null || DesignMode)
				return;

			var locCompArray = m_extendedCtrls.Where(x => x.Key is ILocalizableComponent).ToArray();

			foreach (var kvp in locCompArray)
			{
				var locComponent = kvp.Key as ILocalizableComponent;
				AddMultipleStrings(locComponent);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Goes through all the extended controls to find those that are ListViews.
		/// For those list views, each column header is added to the list of extended controls
		/// and the list view is removed from the extended controls. That is because the only
		/// thing we support being extended on a list view are the column headings. The way
		/// this is done means that ids for column headings must be inferred from the column
		/// name and cannot be specified in the designer.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void FinalizationForListViewColumnHeaders()
		{
			if (m_extendedCtrls == null || DesignMode)
				return;

			var lviews = m_extendedCtrls.Where(x => x.Key is ListView).ToArray();

			foreach (var kvp in lviews)
			{
				// Add each grid column to the list of extended controls.
				var lv = kvp.Key as ListView;
				foreach (ColumnHeader hdr in lv.Columns)
				{
					var loi = GetLocalizedComponentInfo(hdr, true);
					loi.Comment = kvp.Value.Comment;
					m_extendedCtrls[hdr] = loi;
				}

				// Remove the grid from the extended controls.
				m_extendedCtrls.Remove(lv);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Goes through all the extended controls to find those that are DataGridViews.
		/// For those grids, each column is added to the list of extended controls and the
		/// grid is removed from the extended controls. That is because the only thing we
		/// support being extended on a DataGridView are the column headings. The way this
		/// is done means that ids for column headings must be inferred from the column
		/// name and cannot be specified in the designer.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void FinalizationForDataGridViewColumns()
		{
			if (m_extendedCtrls == null || DesignMode)
				return;

			var grids = m_extendedCtrls.Where(x => x.Key is DataGridView).ToArray();

			foreach (var kvp in grids)
			{
				// Add each grid column to the list of extended controls.
				var grid = kvp.Key as DataGridView;
				foreach (DataGridViewColumn col in grid.Columns)
				{
					var loi = GetLocalizedComponentInfo(col, true);
					loi.Comment = kvp.Value.Comment;
					m_extendedCtrls[col] = loi;
				}

				// Remove the grid from the extended controls.
				m_extendedCtrls.Remove(grid);

				grid.ColumnAdded += HandleGridColumnAdded;
				grid.Disposed += HandleGridDisposed;
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleGridColumnAdded(object sender, DataGridViewColumnEventArgs e)
		{
			var locInfo = new LocalizingInfo(e.Column, true);
			_manager.RegisterComponentForLocalizing(locInfo,
				(lm, info) => lm.ApplyLocalization(info.Component));
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridDisposed(object sender, EventArgs e)
		{
			var grid = sender as DataGridView;
			grid.ColumnAdded -= HandleGridColumnAdded;
			grid.Disposed -= HandleGridDisposed;
		}

		#endregion

		#region Properties provided by this extender
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the string id for the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("Localizing Properties")]
		public string GetLocalizingId(IComponent component)
		{
			var l = GetLocalizedComponentInfo(component, true);
			l.CreateIdIfMissing(PrefixForNewItems);
			return l.Id;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the string id for the specified component. Use this method to keep track of all
		/// the components being extended. This information will be used in the EndInit (i.e.
		/// after all the designer code has finished executing in InitializeComponents()).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizingId(IComponent component, string id)
		{
			var loi = GetLocalizedComponentInfo(component, false);
			loi.Id = (string.IsNullOrEmpty(id) ? null : id);

			if (m_extendedCtrls != null && !DesignMode)
				m_extendedCtrls[component] = loi;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds multiple strings from a ILocalizableComponent control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void AddMultipleStrings(ILocalizableComponent locComponent)
		{
			if (m_extendedCtrls == null) // no can do! (can happen during view setup)
				return;
			var lios = locComponent.GetAllLocalizingInfoObjects(this);
			var idToLocInfo = new Dictionary<string, LocalizingInfo>();
			foreach (var localizingInfo in lios)
			{
				if (string.IsNullOrEmpty(localizingInfo.Id))
					continue;
				_manager.AddString(localizingInfo.Id, localizingInfo.Text, null, null, null);
				idToLocInfo.Add(localizingInfo.Id, localizingInfo);
			}
			_manager.LocalizableComponents.Add(locComponent, idToLocInfo);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the level of importance for localizing the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("Localizing Properties")]
		[DefaultValue(LocalizationPriority.Medium)]
		public LocalizationPriority GetLocalizationPriority(IComponent component)
		{
			return GetLocalizedComponentInfo(component, true).Priority;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the level of importance for localizing the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizationPriority(IComponent component, LocalizationPriority priority)
		{
			GetLocalizedComponentInfo(component, false).Priority = priority;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localization comment for the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("Localizing Properties")]
		public string GetLocalizationComment(IComponent component)
		{
			return GetLocalizedComponentInfo(component, true).Comment;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the localization comment for the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizationComment(IComponent component, string cmnt)
		{
			GetLocalizedComponentInfo(component, false).Comment = cmnt;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the tooltip for the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("Localizing Properties")]
		public string GetLocalizableToolTip(IComponent component)
		{
			return GetLocalizedComponentInfo(component, true).ToolTipText;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the tooltip for the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizableToolTip(IComponent component, string tip)
		{
			GetLocalizedComponentInfo(component, false).ToolTipText = tip;
		}


			/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized object info. for the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private LocalizingInfo GetLocalizedComponentInfo(IComponent component, bool initTextFromCompIfNewlyCreated)
		{
			LocalizingInfo loi;
			if (m_extendedCtrls.TryGetValue(component, out loi)) // && !string.IsNullOrEmpty(loi.Id) && loi.Priority != LocalizationPriority.NotLocalizable)
				return loi;

			loi = new LocalizingInfo(component, initTextFromCompIfNewlyCreated);
			m_extendedCtrls[component] = loi;
			return loi;
		}

		#endregion

		private void InitializeComponent()
		{

		}
	}
}
