using System;
using System.Web;

namespace Localization.Translators
{
	/// ----------------------------------------------------------------------------------------
	public enum TranslatorTypes
	{
		/// ------------------------------------------------------------------------------------
		Google,
		/// ------------------------------------------------------------------------------------
		Bing
	}

	/// ----------------------------------------------------------------------------------------
	public interface ITranslator
	{
		/// ------------------------------------------------------------------------------------
		string TranslateText(string srcText);
	}

	/// ----------------------------------------------------------------------------------------
	public abstract class TranslatorBase : ITranslator
	{
		/// ------------------------------------------------------------------------------------
		protected string m_srcCultureId;
		/// ------------------------------------------------------------------------------------
		protected string m_tgtCultureId;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Translate the specified text.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public virtual string TranslateText(string srcText)
		{
			if (string.IsNullOrEmpty(srcText))
				return null;

			try
			{
				int paramCount = PreTranslateProcess(ref srcText);
				string result = InternalTranslate(srcText);
				return (result == null ? null : PostTranslateProcess(paramCount, result));
			}
			catch
			{
				return null;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Internal method for translating the specified text.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual string InternalTranslate(string srcText)
		{
			throw new NotImplementedException();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Prepares the text for google translate by removing accelerator ampersands and
		/// counting the number of parameters (i.e. "{n}" where n is a number from 0 - n).
		/// The number of parameters found is returned.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual int PreTranslateProcess(ref string text)
		{
			text = RemoveAcceleratorAmpersands(text);

			int paramCount = 0;

			for (int i = 0; i <= 9; i++)
			{
				if (text.Contains("{" + i + "}"))
					paramCount++;
			}

			text = text.Replace(Environment.NewLine, "(-999) ");
			text = text.Replace("\\n", "(-999) ");
			return paramCount;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Cleans up a string after it has returned from being translated by Google. The
		/// paramCount provides the number of parameters that were found in the pretranslated
		/// string.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual string PostTranslateProcess(int paramCount, string text)
		{
			if (text == null)
				return null;

			text = HttpUtility.HtmlDecode(text);

			if (paramCount > 0)
			{
				// Because Google translate replaces the braces in string parameters,
				// with parentheses, we need to restore the braces. This process will
				// be a problem only when the original string contains numbers surrounded
				// by parentheses. However, I think those cases are far less likely.
				for (int i = 0; i < paramCount; i++)
					text = text.Replace("(" + i + ")", "{" + i + "}");
			}

			text = text.Replace("(-999) ", Environment.NewLine);
			return text.Trim();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Removes accelerator ampersands from the specified text.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string RemoveAcceleratorAmpersands(string text)
		{
			if (text == null)
				return null;

			text = text.Replace(" & ", "!@!");
			text = text.Replace("&&", "~~");
			text = text.Replace("& ", "~~");
			text = text.Replace("&", string.Empty);
			text = text.Replace("~~", "&");
			return text.Replace("!@!", " & ");
		}
	}
}