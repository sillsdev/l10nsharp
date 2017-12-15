using System;
using System.Collections.Generic;
using System.Diagnostics;
using L10NSharp.XLiffUtils;

namespace L10NSharp
{
	internal class TransUnitUpdater
	{
		internal const string kToolTipSuffix = "_ToolTip_";
		internal const string kShortcutSuffix = "_ShortcutKeys_";

		// Cannot use Environment.NewLine because that also includes a carriage return
		// character which, when included, messes up the display of text in controls.
		internal const string kOSRealNewline = "\n";

		// This is the symbol for a newline that users put in their localized text when
		// they want a real newline inserted. The program will replace literal newlines
		// with the value of kOSNewline.
		internal string _literalNewline = "\\n";

		private readonly LocalizedStringCache _stringCache;
		private readonly string _defaultLang;
		private bool _updated;


		/// ------------------------------------------------------------------------------------
		internal TransUnitUpdater(LocalizedStringCache cache)
		{
			_stringCache = cache;
			_defaultLang = LocalizationManager.kDefaultLang;
			var replacement = _stringCache.XliffDocuments[_defaultLang].File.HardLineBreakReplacement;
			if (replacement != null)
				_literalNewline = replacement;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Updates the localized info. in the cache with the info. from the specified
		/// LocalizedObjectInfo.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal bool Update(LocalizingInfo locInfo)
		{
			_updated = false;

			// Can't do anything without a language id.
			if (string.IsNullOrEmpty(locInfo.LangId))
				return _updated;

			var xliffSource = _stringCache.XliffDocuments[_defaultLang];
			Debug.Assert(xliffSource != null);

			XLiffDocument xliffTarget;
			if (!_stringCache.XliffDocuments.TryGetValue(locInfo.LangId, out xliffTarget))
			{
				xliffTarget = new XLiffDocument();
				xliffTarget.File.AmpersandReplacement = xliffSource.File.AmpersandReplacement;
				xliffTarget.File.DataType = xliffSource.File.DataType;
				xliffTarget.File.HardLineBreakReplacement = xliffSource.File.HardLineBreakReplacement;
				xliffTarget.File.Original = xliffSource.File.Original;
				xliffTarget.File.ProductVersion = xliffSource.File.ProductVersion;
				xliffTarget.File.SourceLang = xliffSource.File.SourceLang;
				xliffTarget.File.TargetLang = locInfo.LangId;
				xliffTarget.IsDirty = true;
				_updated = true;
				_stringCache.XliffDocuments.Add(locInfo.LangId, xliffTarget);
			}

			var tuSourceText = xliffSource.GetTransUnitForId(locInfo.Id);
			var tuSourceToolTip = xliffSource.GetTransUnitForId(locInfo.Id + kToolTipSuffix);
			var tuSourceShortcutKeys = xliffSource.GetTransUnitForId(locInfo.Id + kShortcutSuffix);
			if (locInfo.Priority == LocalizationPriority.NotLocalizable)
			{
				_updated = (tuSourceText != null || tuSourceToolTip != null || tuSourceShortcutKeys != null);
				xliffSource.RemoveTransUnit(tuSourceText);
				xliffSource.RemoveTransUnit(tuSourceToolTip);
				xliffSource.RemoveTransUnit(tuSourceShortcutKeys);
				if (_defaultLang != locInfo.LangId)
				{
					xliffTarget.RemoveTransUnit(tuSourceText);
					xliffTarget.RemoveTransUnit(tuSourceToolTip);
					xliffTarget.RemoveTransUnit(tuSourceShortcutKeys);
				}
				return _updated;
			}

			// Save the shortcut keys
			var shortcutId = locInfo.Id + kShortcutSuffix;
			if ((locInfo.UpdateFields & UpdateFields.ShortcutKeys) == UpdateFields.ShortcutKeys)
			{
				UpdateValueAndComment(xliffTarget, tuSourceShortcutKeys, locInfo.ShortcutKeys, locInfo, shortcutId);
			}

			// Save the tooltips
			var tooltipId = locInfo.Id + kToolTipSuffix;
			if ((locInfo.UpdateFields & UpdateFields.ToolTip) == UpdateFields.ToolTip)
			{
				UpdateValueAndComment(xliffTarget, tuSourceToolTip, locInfo.ToolTipText, locInfo, tooltipId);
			}

			// Save the text
			if ((locInfo.UpdateFields & UpdateFields.Text) == UpdateFields.Text)
			{
				var text = locInfo.Text ?? string.Empty;
				text = text.Replace(Environment.NewLine, _literalNewline);
				text = text.Replace(_literalNewline, "@#$");
				text = text.Replace(kOSRealNewline, _literalNewline);
				text = text.Replace("@#$", _literalNewline);
				UpdateValueAndComment(xliffTarget, tuSourceText, text, locInfo, locInfo.Id);
			}

			if (_updated)
				xliffTarget.IsDirty = true;
			return _updated;
		}

		void UpdateValueAndComment(XLiffDocument xliffTarget, TransUnit tuSource, string newText, LocalizingInfo locInfo, string tuId)
		{
			var tuTarget = UpdateValue(xliffTarget, tuSource, newText, locInfo, tuId);
			UpdateTransUnitComment(xliffTarget, tuSource, locInfo);
			UpdateTransUnitComment(xliffTarget, tuTarget, locInfo);
		}

		private void UpdateTransUnitComment(XLiffDocument xliffTarget, TransUnit tu, LocalizingInfo locInfo)
		{
			if (tu == null)
				return;
			if (locInfo.DiscoveredDynamically && !tu.Dynamic)
			{
				tu.Dynamic = true;
				_updated = true;
				Console.WriteLine("DEBUG: mark {0} trans-unit dynamic id=\"{1}\"; text=\"{2}\"", xliffTarget.File.Original, tu.Id, tu.Source.Value);
			}
			if ((locInfo.UpdateFields & UpdateFields.Comment) != UpdateFields.Comment)
				return;
			if (tu.Notes.Count == 0 && string.IsNullOrEmpty(locInfo.Comment))
				return;		// empty comment and already no comment in TransUnit
			if (tu.NotesContain(locInfo.Comment))
				return;		// exactly the same comment already exists in TransUnit

			_updated = true;
			tu.Notes.Clear();
			tu.AddNote("ID: " + tu.Id);
			if (!string.IsNullOrEmpty(locInfo.Comment))
				tu.AddNote(locInfo.Comment);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Updates the value for the specified translation unit with the specified new value.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private TransUnit UpdateValue(XLiffDocument xliffTarget, TransUnit tuSource, string newValue, LocalizingInfo locInfo, string tuId)
		{
			// One would think there would be a source TransUnit, but that isn't necessarily true
			// with users editing interactively and adding tooltips or shortcuts.
			Debug.Assert(tuSource == null || tuId == tuSource.Id);
			Debug.Assert(tuId.StartsWith(locInfo.Id));
			var tuTarget = xliffTarget.GetTransUnitForId(tuId);
			// If the TransUnit exists in the target language, check whether we're removing the translation
			// instead of adding or changing it.
			if (tuTarget != null)
			{
				var tuvTarg = tuTarget.GetVariantForLang(locInfo.LangId);
				if (tuvTarg != null)
				{
					// don't need to update if the value hasn't changed
					if (tuvTarg.Value == newValue)
						return tuTarget;

					if (String.IsNullOrEmpty(newValue))
					{
						_updated = true;
						tuTarget.RemoveVariant(tuvTarg);
						if ((tuTarget.Source == null || String.IsNullOrEmpty(tuTarget.Source.Value)) &&
							(tuTarget.Target == null || String.IsNullOrEmpty(tuTarget.Target.Value)))
						{
							xliffTarget.RemoveTransUnit(tuTarget);
							tuTarget = null;
						}
					}
				}
			}
			// If we're removing an existing translation, we can quit now.
			if (String.IsNullOrEmpty(newValue))
			{
				xliffTarget.File.Body.TranslationsById.Remove(tuId);
				return tuTarget;
			}
			// If the TransUnit does not exist in the target language yet, create it and fill in the
			// source language value (if any).
			if (tuTarget == null)
			{
				tuTarget = new TransUnit();
				tuTarget.Id = tuId;
				tuTarget.Dynamic = locInfo.DiscoveredDynamically;
				if (locInfo.DiscoveredDynamically)
					Console.WriteLine("DEBUG: {0} dynamic trans-unit id=\"{1}\"; text=\"{2}\"", xliffTarget.File.Original, tuId, newValue);
				xliffTarget.AddTransUnit(tuTarget);
				if (tuSource != null && locInfo.LangId != _defaultLang)
				{
					var tuvSrc = tuSource.GetVariantForLang(_defaultLang);
					if (tuvSrc != null && !String.IsNullOrEmpty(tuvSrc.Value))
						tuTarget.AddOrReplaceVariant(_defaultLang, tuvSrc.Value);
				}
				tuTarget.AddNote("ID: " + tuId);
			}
			tuTarget.AddOrReplaceVariant(locInfo.LangId, newValue);
			xliffTarget.File.Body.TranslationsById[tuId] = newValue;
			_updated = true;
			return tuTarget;
		}
	}
}
