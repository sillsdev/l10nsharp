using System;
using System.Diagnostics;
using L10NSharp.Translators;

namespace L10NSharp.WindowsForms.UI
{
	internal class LanguageChoosingDialogViewModel
	{
		private readonly L10NCultureInfo _requestedCulture;
		private readonly string _messageLabelFormat;
		private readonly string _acceptButtonText;
		private readonly string _windowTitle;

		/// <summary>
		/// Creates a new LanguageChoosingDialogViewModel object to handle (asynchronous) translation of message and other UI strings
		/// displayed in the LanguageChoosingDialog.
		/// </summary>
		/// <param name="messageLabelFormat">Format string where param {0} is the native name of the requested UI language/culture and
		/// param {1} is the English name of the requested UI language/culture</param>
		/// <param name="acceptButtonText">The "OK" button text (not a format string)</param>
		/// <param name="windowTitle">The dialog's title (not a format string)</param>
		/// <param name="requestedCulture">The requested UI language/culture. Typically, this will not be English (though in some of
		/// the tests it is)</param>
		/// <param name="nonEnglishUiAction">An action that should be performed only if the requested culture is not English. In
		/// production, this action will set up an action to be performed (once) on idle so that TranslateStrings can be called and
		/// the UI can be updated to reflect the newly translated strings, if appropriate.</param>
		internal LanguageChoosingDialogViewModel(string messageLabelFormat, string acceptButtonText, string windowTitle,
			L10NCultureInfo requestedCulture, Action nonEnglishUiAction)
		{
			_messageLabelFormat = messageLabelFormat;
			AcceptButtonText = _acceptButtonText = acceptButtonText;
			WindowTitle = _windowTitle = windowTitle;
			_requestedCulture = requestedCulture;
			if (requestedCulture.EnglishName == requestedCulture.NativeName)
			{
				// It looks weird and stupid to display "English (English)" or any other such pair where the two strings are the same.
				_messageLabelFormat = _messageLabelFormat.Replace(" ({1})", "");
			}
			Message = string.Format(_messageLabelFormat, requestedCulture.EnglishName, requestedCulture.NativeName);
			if (requestedCulture.TwoLetterISOLanguageName != "en")
				nonEnglishUiAction?.Invoke();
		}

		internal void TranslateStrings(TranslatorBase translator)
		{
			try
			{
				var sourceString = string.Format(_messageLabelFormat, _requestedCulture.EnglishName, "{0}");
				var s = translator.TranslateText(sourceString);
				if (s == sourceString)
					return;
				if (s.Contains("{0}") && s.Length > 5) // If we just get back "{0} or "({0})", we won't consider that useful.
				{
					// Bing will presumably have translated the English string into the native language, so now we want
					// to display the English name in parentheses. (As a sanity check, we could look to see whether the
					// native name is in the string, but there could be situations where it may not be an exact match.)
					s = string.Format(s, _requestedCulture.EnglishName);
				}
				else if (_messageLabelFormat.Contains("{1}"))
				{
					// If we already weeded out the param (because the language names are the same), there's no need to re-try (in case it's slow).
					// This is just a fall-back in case there is some rare situation where the translator chokes on the presence of a formatting param in the string.
					s = translator.TranslateText(string.Format(_messageLabelFormat, _requestedCulture.EnglishName, _requestedCulture.NativeName));
				}

				if (!string.IsNullOrEmpty(s))
				{
					Message = s;
					// In general, we will be able to translate OK and the title bar text iff we were able to translate
					// the message.  This assumption saves a few processor cycles and prevents disappearing text when
					// a language has not been localized (as is likely the case when we display this dialog).
					AcceptButtonText = translator.TranslateText(_acceptButtonText);
					WindowTitle = translator.TranslateText(_windowTitle);
				}
			}
			catch (Exception)
			{
				//swallow
			}
		}

		internal string RequestedCultureTwoLetterISOLanguageName => _requestedCulture.TwoLetterISOLanguageName;
		internal string Message { get; private set; }
		internal string AcceptButtonText { get; private set; }
		internal string WindowTitle { get; private set; }
	}
}
