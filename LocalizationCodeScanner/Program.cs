using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Localization;

namespace LocalizationCodeScanner
{
	class Program
	{
		private const string kReplacementChar1 = "\uFFFC";
		private const string kReplacementChar2 = "\uFFFD";
		private static LocalizationManager _l10NMngr;
		private static string _text;
		private static ControlInfo _controlInfo;

		/// ------------------------------------------------------------------------------------
		static void Main(string[] args)
		{
			_controlInfo = ControlInfo.Load(args[0]);

			var outputFilePath = Path.Combine(_controlInfo.OutputPath, _controlInfo.ProjectId + ".tmx");
			if (File.Exists(outputFilePath))
				File.Delete(outputFilePath);

			_l10NMngr = LocalizationManager.Create(_controlInfo.UILang ?? "en",
				_controlInfo.ProjectId, _controlInfo.ProjectName, "1.1.1.1",
				null, _controlInfo.OutputPath);

			var fileList = from file in Directory.GetFiles(_controlInfo.TopLevelSrcFolder, "*.cs", SearchOption.AllDirectories)
						   where !file.ToLower().EndsWith(".designer.cs")
						   select file;

			foreach (var file in fileList)
			{
				Console.WriteLine("\r\nFile: " + file);
				ParseFile(file);
			}

			fileList = Directory.GetFiles(_controlInfo.TopLevelSrcFolder,
				"*.designer.cs", SearchOption.AllDirectories);

			foreach (var file in fileList)
			{
				Console.WriteLine("\r\nFile: " + file);
				ParseDesignerFiles(file);
			}

			_l10NMngr.StringCache.SaveIfDirty();
		}

		/// ------------------------------------------------------------------------------------
		private static void ParseFile(string file)
		{
			_text = File.ReadAllText(file).Replace("\\\"", kReplacementChar1);
			const string methodName = "GetString(";
			int index = _text.IndexOf(methodName);

			while (index >= 0)
			{
				index += methodName.Length;
				if (IsMatch(index))
					ParseMethodCall(index);

				index = _text.IndexOf(methodName, index);
			}

			ParseL10NLines();
		}

		/// ------------------------------------------------------------------------------------
		private static void ParseMethodCall(int index)
		{
			var args = ExtractMethodArguments(index);

			if (args.Length < 2 || args.Length > 6 || args.Length == 5)
				return;

			var id = args[0].Replace(kReplacementChar1, "\"");
			var text = args[1].Replace(kReplacementChar1, "\"");
			var comment = (args.Length >= 3 ? args[2] : null);
			var toolTipText = (args.Length == 5 ? args[3] : null);
			var shortCutKey = (args.Length == 5 ? args[4] : null);

			if (comment != null)
				comment = comment.Replace(kReplacementChar1, "\"");

			_l10NMngr.AddString(id, text, toolTipText, shortCutKey, comment);

			Console.WriteLine("       Id: " + args[0]);
		}

		/// ------------------------------------------------------------------------------------
		private static bool IsMatch(int index)
		{
			int i = index;

			while (i >= 0 && _text[i] != '\t' && _text[i] != '\r')
				i--;

			var precedingText = _text.Substring(i, index - i);
			return (!precedingText.Contains("void ") && !precedingText.Contains("private ") &&
				!precedingText.Contains("public ") && !precedingText.Contains("internal "));
		}

		/// ------------------------------------------------------------------------------------
		private static string[] ExtractMethodArguments(int i)
		{
			var bldr = new StringBuilder();
			bool insideQuote = false;

			while (_text[i] != ')' || insideQuote)
			{
				if (_text[i] == ';' && !insideQuote)
					break;

				if (_text[i] == '\"')
					insideQuote = !insideQuote;

				if (_text[i] == ',' && !insideQuote)
					bldr.Append('\x1');
				else if (!GetThrowAwayChars(insideQuote).Contains(_text[i]))
					bldr.Append(_text[i]);

				i++;
			}

			var args = bldr.ToString().Replace("\"+\"", string.Empty);
			args = args.Replace("\"", string.Empty);
			return args.Split('\x1');
		}

		/// ------------------------------------------------------------------------------------
		private static IEnumerable<char> GetThrowAwayChars(bool insideQuote)
		{
			yield return '\r';

			if (!insideQuote)
			{
				yield return '\t';
				yield return '\n';
				yield return ' ';
			}
		}

		/// ------------------------------------------------------------------------------------
		private static void ParseDesignerFiles(string file)
		{
			_text = File.ReadAllText(file).Replace("\\\"", kReplacementChar1).Replace("\\'", kReplacementChar2);

			foreach (Match match in Regex.Matches(_text, @"\.SetLocalizingId\(.+,"))
			{
				var ctrlName = match.Value.Substring(17).TrimEnd(',');

				if (!GetShouldBeLocalized(ctrlName))
					continue;

				var id = GetSecondArg(match.Index + match.Length);
				var text = GetCtrlText(ctrlName);
				var tooltip = GetLocalizationValue(ctrlName, "SetLocalizableToolTip");
				var comment = GetLocalizationValue(ctrlName, "SetLocalizationComment");

				if (text != null)
					text = text.Replace(kReplacementChar1, "\"").Replace(kReplacementChar2, "'");

				if (tooltip != null)
					tooltip = tooltip.Replace(kReplacementChar1, "\"").Replace(kReplacementChar2, "'");

				if (comment != null)
					comment = comment.Replace(kReplacementChar1, "\"").Replace(kReplacementChar2, "'");

				_l10NMngr.AddString(id, text, tooltip, null, comment);

				Console.WriteLine("\t  Id: " + id);
			}

			ParseL10NLines();
		}

		/// ------------------------------------------------------------------------------------
		private static void ParseL10NLines()
		{
			foreach (Match match in Regex.Matches(_text, "=.\"_L10N:_"))
			{
				var idAndText = GetSecondArg(match.Index + match.Length - 6);
				if (idAndText == null)
					continue;

				var id = LocalizingInfo.GetIdFromText(idAndText);
				var text = LocalizationManager.StripOffLocalizationInfoFromText(idAndText);
				_l10NMngr.AddString(id, text, null, null, null);
			}
		}

		/// ------------------------------------------------------------------------------------
		private static string GetSecondArg(int index)
		{
			var args = ExtractMethodArguments(index);
			return (args.Length == 0 ? null : args[0]);
		}

		/// ------------------------------------------------------------------------------------
		private static bool GetShouldBeLocalized(string ctrlName)
		{
			var srchString = string.Format(".SetLocalizationPriority({0},", ctrlName);
			int i = _text.IndexOf(srchString);
			if (i < 0)
				return true;

			var priority = GetSecondArg(i + srchString.Length);
			return (!priority.EndsWith(LocalizationPriority.NotLocalizable.ToString()));
		}

		/// ------------------------------------------------------------------------------------
		private static string GetCtrlText(string ctrlName)
		{
			var pattern = string.Format("{0}.Text = \".+\"", ctrlName);
			var match = Regex.Match(_text, pattern);

			if (!match.Success)
				return null;

			var text = match.Value.Substring(ctrlName.Length + 5);
			text = text.TrimStart(' ', '=', '\"');
			return text .TrimEnd('\"');
		}

		/// ------------------------------------------------------------------------------------
		private static string GetLocalizationValue(string ctrlName, string extenderMethod)
		{
			var srchString = string.Format(".{0}({1},", extenderMethod, ctrlName);
			int i = _text.IndexOf(srchString);
			if (i < 0)
				return null;

			var value = GetSecondArg(i + srchString.Length);
			return (string.IsNullOrEmpty(value) || value == "null" ? null : value);
		}
	}
}