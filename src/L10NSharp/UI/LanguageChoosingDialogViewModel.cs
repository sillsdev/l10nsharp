using System;
using L10NSharp.Translators;

namespace L10NSharp.UI
{
	public class LanguageChoosingDialogViewModel
	{
		private readonly L10NCultureInfo _requestedCulture;
		private readonly string _messageLabelFormat;
		private readonly string _acceptButtonText;
		private readonly string _windowTitle;

		public LanguageChoosingDialogViewModel(string messageLabelFormat, string acceptButtonText, string windowTitle,
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

		public void SetTranslator(TranslatorBase translator)
		{
			try
			{
				var s = translator.TranslateText(string.Format(_messageLabelFormat, _requestedCulture.EnglishName, "{0}"));
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

		public string RequestedCultureTwoLetterISOLanguageName => _requestedCulture.TwoLetterISOLanguageName;
		public string Message { get; private set; }
		public string AcceptButtonText { get; private set; }
		public string WindowTitle { get; private set; }
	}
}
