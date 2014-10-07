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
	[ProvideProperty("LocalizingId", typeof(object))]
	[ProvideProperty("LocalizableToolTip", typeof(object))]
	[ProvideProperty("LocalizationPriority", typeof(object))]
	[ProvideProperty("LocalizationComment", typeof(object))]
	public class L10NSharpExtender : Component, IExtenderProvider, ISupportInitialize
	{
		private static HashSet<Type> s_doNotExtend;

		private Container components = null;
		private Dictionary<object, LocalizingInfo> m_extendedCtrls;
		private LocalizationManager _manager;
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
		/// loaded localization managers stored in LocalizationManager.LoadedManagers.
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

			return (extendee is Control || extendee is ToolStripItem || extendee is ColumnHeader);
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

			// Now make sure each extended control is localized.
			foreach (var locInfo in m_extendedCtrls.Values
				.Where(li => li.Priority != LocalizationPriority.NotLocalizable))
			{
				if (string.IsNullOrEmpty(locInfo.LangId))
					locInfo.LangId = LocalizationManager.kDefaultLang;
				// Depending on the order in which VS Designer decides to initialize fields, locInfo may be originally created before the Text of the
				// control is set. If so, obtain it again.
				if (string.IsNullOrWhiteSpace(locInfo.Text))
					locInfo.UpdateTextFromObject();
				// Special case: the Text of a column header is "ColumnHeader" before it is ever set.
				// This means that if we first processed the CH before we set its text, we have noted
				// "ColumnHeader" as its default English name. Get the real one if it has since been updated.
				var ch = locInfo.Obj as ColumnHeader;
				if (ch != null && ch.Text != "ColumnHeader" && locInfo.Text == "ColumnHeader")
					locInfo.UpdateTextFromObject();
				if (_manager.RegisterObjectForLocalizing(locInfo))
					_manager.ApplyLocalization(locInfo.Obj);
			}

			// Now make sure each IMultiStringContainer is localized.
			foreach (var kvp in _manager.MultiStringContainers)
			{
				var msc = kvp.Key;
				var idToLocInfo = kvp.Value;
				foreach (var locInfo in idToLocInfo.Values
					.Where(li => li.Priority != LocalizationPriority.NotLocalizable))
				{
					if (string.IsNullOrEmpty(locInfo.LangId))
						locInfo.LangId = LocalizationManager.kDefaultLang;
				}
				if(_manager.RegisterObjectForLocalizing(new LocalizingInfo(msc, "dummy")))
					_manager.ApplyLocalizationsToMultiStringContainer(msc, idToLocInfo);
			}

			m_extendedCtrls = null;
			_okayToLocalizeControls = false;
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
					var loi = GetLocalizedObjectInfo(hdr, true);
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
					var loi = GetLocalizedObjectInfo(col, true);
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
			if (_manager.RegisterObjectForLocalizing(locInfo))
				_manager.ApplyLocalization(locInfo.Obj);
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
		/// Gets the string id for the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("Localizing Properties")]
		public string GetLocalizingId(object obj)
		{
			var l = GetLocalizedObjectInfo(obj, true);
			l.CreateIdIfMissing(PrefixForNewItems);
			return l.Id;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the string id for the specified control. Use this method to keep track of all
		/// the controls being extended. This information will be used in the EndInit (i.e.
		/// after all the designer code has finished executing in InitializeComponents()).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizingId(object obj, string id)
		{
			var loi = GetLocalizedObjectInfo(obj, false);
			loi.Id = (string.IsNullOrEmpty(id) ? null : id);

			if (m_extendedCtrls != null && !DesignMode)
				m_extendedCtrls[obj] = loi;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds multiple strings from a IMultiStringContainer control. This interface was initially
		/// motivated by Bloom's BetterToolTip. This method should be run before EndInit(), but
		/// after all the designer code has finished executing in InitializeComponents().
		/// </summary>
		/// <remarks>The method needs to be called just before designer InitializeComponent() calls
		/// L10NSharpExtender.EndInit(), otherwise m_extendedCtrls will be null.</remarks>
		/// ------------------------------------------------------------------------------------
		public void AddMultipleStrings(IMultiStringContainer msc)
		{
			if (m_extendedCtrls == null) // no can do! (can happen during view setup)
				return;
			var lios = msc.GetAllLocalizingInfoObjects();
			var idToLocInfo = new Dictionary<string, LocalizingInfo>();
			foreach (var localizingInfo in lios)
			{
				if (string.IsNullOrEmpty(localizingInfo.Id))
					continue;
				_manager.AddString(localizingInfo.Id, localizingInfo.Text, null, null, null);
				idToLocInfo.Add(localizingInfo.Id, localizingInfo);
			}
			_manager.MultiStringContainers.Add(msc, idToLocInfo);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the level of importance for localizing the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("Localizing Properties")]
		[DefaultValue(LocalizationPriority.Medium)]
		public LocalizationPriority GetLocalizationPriority(object obj)
		{
			return GetLocalizedObjectInfo(obj, true).Priority;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the level of importance for localizing the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizationPriority(object obj, LocalizationPriority priority)
		{
			GetLocalizedObjectInfo(obj, false).Priority = priority;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localization comment for the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("Localizing Properties")]
		public string GetLocalizationComment(object obj)
		{
			return GetLocalizedObjectInfo(obj, true).Comment;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the localization comment for the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizationComment(object obj, string cmnt)
		{
			GetLocalizedObjectInfo(obj, false).Comment = cmnt;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the tooltip for the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("Localizing Properties")]
		public string GetLocalizableToolTip(object obj)
		{
			return GetLocalizedObjectInfo(obj, true).ToolTipText;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the tooltip for the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizableToolTip(object obj, string tip)
		{
			GetLocalizedObjectInfo(obj, false).ToolTipText = tip;
		}


			/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized object info. for the specified object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private LocalizingInfo GetLocalizedObjectInfo(object obj, bool initTextFromObjIfNewlyCreated)
		{
			LocalizingInfo loi;
			if (m_extendedCtrls.TryGetValue(obj, out loi)) // && !string.IsNullOrEmpty(loi.Id) && loi.Priority != LocalizationPriority.NotLocalizable)
				return loi;

			loi = new LocalizingInfo(obj, initTextFromObjIfNewlyCreated);
			m_extendedCtrls[obj] = loi;
			return loi;
		}

		#endregion

		private void InitializeComponent()
		{

		}
	}
}