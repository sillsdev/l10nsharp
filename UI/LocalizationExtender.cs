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
using System.Linq;
using System.Windows.Forms;

namespace Localization.UI
{
	/// ----------------------------------------------------------------------------------------
	[ProvideProperty("LocalizingId", typeof(object))]
	[ProvideProperty("LocalizableToolTip", typeof(object))]
	[ProvideProperty("LocalizationPriority", typeof(object))]
	[ProvideProperty("LocalizationComment", typeof(object))]
	public class LocalizationExtender : Component, IExtenderProvider, ISupportInitialize
	{
		private static HashSet<Type> s_doNotExtend;

		private Container components = null;
		private Dictionary<object, LocalizingInfo> m_extendedCtrls;
		private LocalizationManager _lm;
		private string _locManagerId;

		#region Constructors
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Constructor for instance that supports Class Composition designer.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public LocalizationExtender(IContainer container) : this()
		{
			container.Add(this);
		}

		/// ------------------------------------------------------------------------------------
		public LocalizationExtender()
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
				_locManagerId = value;
				if (!DesignMode && LocalizationManager.LoadedManagers != null &&
					LocalizationManager.LoadedManagers.ContainsKey(_locManagerId))
				{
					_lm = LocalizationManager.LoadedManagers[_locManagerId];
				}
			}
		}

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
					s_doNotExtend.Add(typeof(LocalizationExtender));
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
			if (ControlsNotExtended.Contains(extendee.GetType()) || (!DesignMode && !_lm.Enabled))
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
		/// Signals the object that initialization is complete. This method will go through
		/// the collection of the controls that have been extended and adds to or udates the
		/// default values in the string files. Then each extended control is localized.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void EndInit()
		{
			if (DesignMode || m_extendedCtrls == null || _lm == null || !_lm.Enabled)
				return;

			FinalizationForListViewColumnHeaders();
			FinalizationForDataGridViewColumns();

			// Now make sure each extended control is localized.
			foreach (var locInfo in m_extendedCtrls.Values
				.Where(li => li.Priority != LocalizationPriority.NotLocalizable))
			{
				_lm.RegisterObjectForLocalizing(locInfo.Obj, locInfo.Id, null, null, null, null);
			}

			m_extendedCtrls = null;
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
					var loi = GetLocalizedObjectInfo(hdr);
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
					var loi = GetLocalizedObjectInfo(col);
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
			var locInfo = new LocalizingInfo(e.Column);
			_lm.RegisterObjectForLocalizing(locInfo.Obj, locInfo.Id, null, null, null, null);
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
			return GetLocalizedObjectInfo(obj).Id;
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
			var loi = GetLocalizedObjectInfo(obj);
			loi.Id = (string.IsNullOrEmpty(id) ? null : id);

			if (m_extendedCtrls != null && !DesignMode)
				m_extendedCtrls[obj] = loi;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the level of importance for localizing the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("Localizing Properties")]
		[DefaultValue(LocalizationPriority.High)]
		public LocalizationPriority GetLocalizationPriority(object obj)
		{
			return GetLocalizedObjectInfo(obj).Priority;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the level of importance for localizing the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizationPriority(object obj, LocalizationPriority priority)
		{
			GetLocalizedObjectInfo(obj).Priority = priority;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the tooltip for the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("Localizing Properties")]
		public string GetLocalizationComment(object obj)
		{
			return GetLocalizedObjectInfo(obj).Comment;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the tooltip for the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizationComment(object obj, string cmnt)
		{
			GetLocalizedObjectInfo(obj).Comment = cmnt;
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
			return GetLocalizedObjectInfo(obj).ToolTipText;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the tooltip for the specified control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLocalizableToolTip(object obj, string tip)
		{
			GetLocalizedObjectInfo(obj).ToolTipText = tip;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the localized object info. for the specified object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private LocalizingInfo GetLocalizedObjectInfo(object obj)
		{
			LocalizingInfo loi;
			if (m_extendedCtrls.TryGetValue(obj, out loi))
				return loi;

			loi = new LocalizingInfo(obj);
			m_extendedCtrls[obj] = loi;
			return loi;
		}

		#endregion
	}
}