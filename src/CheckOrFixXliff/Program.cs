using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using L10NSharp.XLiffUtils;

namespace CheckOrFixXliff
{
	/// <summary>
	/// This program optionally validates XLiff files in three ways:
	/// 1) Check for being well-formed XML.  This is the most basic check, and must pass for anything else to work.
	/// 2) Check against the XLiff 1.2 schema.  This is optional ("--validate" flag), and not really needed for Bloom
	///    and other L10NSharp clients who ignore the exact order of the child elements in trans-unit elements.  Note
	///    that crowdin doesn't always produce valid Xliff, and they don't seem all that concerned about fixing it.
	/// 3) Check all translated format strings (those that contain markers like '{0}' for validity: markers match
	///    between the source and target strings, and the markers in the target strings are not mangled in translation.
	///    If malformed markers are detected that would cause the program to crash, the 3 characters preceding and 6
	///    characters following each open brace ('{') are displayed following the warning messages for the string.
	/// If the third check reveals problems involving mangled substitution markers (most often in RTL scripts), the
	/// program can optionally try to repair the strings using some common patterns that have been observed ("--fix"
	/// flag).
	/// </summary>
	class Program
	{
		/// <summary>
		/// List of trans-unit id values whose target value contains a malformed substitution marker.
		/// </summary>
		private static List<string> _mangledTargets = new List<string>();
		private static bool _quiet = false;

		static void Main(string[] args)
		{
			bool validate = false;
			bool fix = false;
			for (int i = 0; i < args.Length; ++i)
			{
				if (args[i] == "--validate")
				{
					validate = true;
				}
				else if (args[i] == "--fix")
				{
					fix = true;
				}
				else if (args[i] == "--quiet")
				{
					_quiet = true;
				}
				else
				{
					string filename = args[i];
					if (!File.Exists(filename))
					{
						Console.WriteLine("{0} does not exist!", filename);
					}
					if (!CheckForWellFormedXml(filename))
						return;	// all other checks depend on loading XML
					if (validate)
						ValidateXliffAgainstSchema(filename);
					CheckFormatStringMarkers(filename);
					if (fix && _mangledTargets.Count > 0)
						FixMangledFormatMarkers(filename);
				}
			}
		}

		private static void FixMangledFormatMarkers(string filename)
		{
			var reader = XmlReader.Create(new StreamReader(filename));
			var document = XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
			var namespaceManager = new XmlNamespaceManager(reader.NameTable);
			namespaceManager.AddNamespace("x", "urn:oasis:names:tc:xliff:document:1.2");
			foreach (var id in _mangledTargets)
			{
				var xpath = string.Format("//x:trans-unit[@id='{0}']", id);
				var tu = document.XPathSelectElement(xpath, namespaceManager);
				if (tu != null)
					FixBrokenTransUnit(tu, namespaceManager);
			}
			document.Save(filename + "-fixed");
		}

		private static void FixBrokenTransUnit(XElement tu, XmlNamespaceManager namespaceManager)
		{
			var tuid = tu.Attribute("id").Value;
			var source = tu.XPathSelectElement("x:source", namespaceManager);
			var target = tu.XPathSelectElement("x:target", namespaceManager);
			if (source.HasElements || target.HasElements)
			{
				Console.WriteLine("Cannot fix {0} because the translated material contains XML elements itself", tuid);
				return;
			}
			// Most if not all the problems I've seen are due to confusion in RTL scripts.  The following regular expression
			// operations fix the patterns of mistakes that I've seen.
			// Note that \u200E is 'LEFT-TO-RIGHT MARK' and \u200F is 'RIGHT-TO-LEFT MARK'.
			var target0 = target.Value;
			var target1 = Regex.Replace(target0, "'{\u200E'{([0-9]+)\u200F*", "\u200E'{$1}'\u200F", RegexOptions.CultureInvariant);
			var target2 = Regex.Replace(target1, "\"{\u200E\"{([0-9]+)\u200F*", "\u200E\"{$1}\"\u200F", RegexOptions.CultureInvariant);
			var target3 = Regex.Replace(target2, "{\u200E{([0-9]+)\u200F*", "\u200E{$1}\u200F", RegexOptions.CultureInvariant);
			var target4 = Regex.Replace(target3, "'{\u200E([0-9]+)}'\u200F*", "\u200E'{$1}'\u200F", RegexOptions.CultureInvariant);
			var target5 = Regex.Replace(target4, "{\u200E([0-9]+)}\u200F*", "\u200E{$1}\u200F", RegexOptions.CultureInvariant);
			var target6 = Regex.Replace(target5, " {\u200E {([0-9]+)\u200F*", " \u200E{$1}\u200F ", RegexOptions.CultureInvariant);
			if (target6 != target.Value)
				target.SetValue(target6);
		}

		/// <summary>
		/// Check whether the given file is even valid ("well formed") XML.
		/// </summary>
		/// <returns>
		/// true if okay, false if the file cannot load as an XmlDocument
		/// </returns>
		private static bool CheckForWellFormedXml(string filename)
		{
			try
			{
				var xdoc = new XmlDocument();
				xdoc.Load(filename);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Check whether the given file validates against the XLIFF 1.2 schema.
		/// </summary>
		/// <returns>
		/// true if okay, false if the file fails to validate
		/// </returns>
		private static bool ValidateXliffAgainstSchema(string filename)
		{
			bool valid = true;
			var installedXliffDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var schemaLocation = Path.Combine(installedXliffDir, "xliff-core-1.2-transitional.xsd");
			var schemas = new XmlSchemaSet();
			using (var reader = XmlReader.Create(schemaLocation))
			{
				schemas.Add("urn:oasis:names:tc:xliff:document:1.2", reader);
				var document = XDocument.Load(filename, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
				document.Validate(schemas, (sender, args) => {
					if (!_quiet)
						Console.WriteLine("{0} did not validate against schema on line {1}: {2}", filename, args.Exception.LineNumber, args.Message);
					valid = false;
				});
			}
			return valid;
		}

		/// <summary>
		/// Check that all the format substitution markers are present and okay.
		/// </summary>
		/// <returns>
		/// true if the translation substitution markers are all valid and match the source markers, false otherwise
		/// </returns>
		private static bool CheckFormatStringMarkers(string filename)
		{
			bool allOkay = true;
			var doc = XLiffDocument.Read(filename);
			var dictSourceMarkers = new Dictionary<string, int>();
			var dictTargetMarkers = new Dictionary<string, int>();
			foreach (var tu in doc.File.Body.TransUnits)
			{
				var matchesSource = Regex.Matches(tu.Source.Value, "{[0-9]+}");
				var matchesTarget = Regex.Matches(tu.Target.Value, "{[0-9]+}");
				if (matchesSource.Count == 0 && matchesTarget.Count == 0)
					continue;
				TabulateMarkers(matchesSource, dictSourceMarkers);
				TabulateMarkers(matchesTarget, dictTargetMarkers);
				var okay = CheckForMissingOrExtraMarkers(dictSourceMarkers, dictTargetMarkers, tu);
				okay &= CheckForMalformedMarkers(dictSourceMarkers.Count, tu);
				if (!okay && !_quiet)
					Console.WriteLine();
				allOkay &= okay;
			}
			return allOkay;
		}

		/// <summary>
		/// Check for malformed substitution markers in the target string.
		/// </summary>
		/// <returns>
		/// true if any markers are okay, false if any are malformed.
		/// </returns>
		private static bool CheckForMalformedMarkers(int markerCount, TransUnit tu)
		{
			try
			{
				string s;
				switch (markerCount)
				{
					case 1:
						s = String.Format(tu.Target.Value, "FIRST");
						break;
					case 2:
						s = String.Format(tu.Target.Value, "FIRST", "SECOND");
						break;
					case 3:
						s = String.Format(tu.Target.Value, "FIRST", "SECOND", "THIRD");
						break;
					case 4:
						s = String.Format(tu.Target.Value, "FIRST", "SECOND", "THIRD", "FOURTH");
						break;
					case 5:
						s = String.Format(tu.Target.Value, "FIRST", "SECOND", "THIRD", "FOURTH", "FIFTH");
						break;
					case 6:
						s = String.Format(tu.Target.Value, "FIRST", "SECOND", "THIRD", "FOURTH", "FIFTH", "SIXTH");
						break;
					case 7:
						s = String.Format(tu.Target.Value, "FIRST", "SECOND", "THIRD", "FOURTH", "FIFTH", "SIXTH", "SEVENTH");
						break;
					case 8:
						s = String.Format(tu.Target.Value, "FIRST", "SECOND", "THIRD", "FOURTH", "FIFTH", "SIXTH", "SEVENTH", "EIGHTH");
						break;
					case 9:
						s = String.Format(tu.Target.Value, "FIRST", "SECOND", "THIRD", "FOURTH", "FIFTH", "SIXTH", "SEVENTH", "EIGHTH", "NINTH");
						break;
					case 10:
						s = String.Format(tu.Target.Value, "FIRST", "SECOND", "THIRD", "FOURTH", "FIFTH", "SIXTH", "SEVENTH", "EIGHTH", "NINTH", "TENTH");
						break;
					default:
						Console.WriteLine("trans-unit {0} has more than ten distinct substition markers!", tu.Id);
						break;
				}
				return true;
			}
			catch (Exception e)
			{
				if (!_quiet)
					Console.WriteLine(@"Translation of " + tu.Id + @" will cause crash");
				_mangledTargets.Add(tu.Id);
				DumpPossiblyBadData(tu.Id, tu.Target.Value);
				return false;
			}
		}

		/// <summary>
		/// Dump the 3 characters before and 6 characters following each { character in the string.  This
		/// should be enough to show how a substitution marker has been mangled, or that it is okay.
		/// </summary>
		private static void DumpPossiblyBadData(string tuid, string target)
		{
			var data = target.ToCharArray();
			for (int i = 0; i < data.Length; ++i)
			{
				if (data[i] == '{')
				{
					Console.Write("{0}({1}):", tuid, i);
					var first = Math.Max(0, i - 3);
					var last = Math.Min(i + 6, data.Length - 1);
					for (int j = first; j <= last; ++j)
					{
						if (data[j] > 32 && data[j] < 127)
							Console.Write(" " + data[j]);
						else
							Console.Write(" {0:X4}", (ushort)data[j]);
					}
					Console.WriteLine();
					i = last;
				}
			}
		}

		/// <summary>
		/// Check that the target substitution markers exactly match those in the source.
		/// </summary>
		/// <returns>
		/// true if everything matches, false if there's any disparity
		/// </returns>
		private static bool CheckForMissingOrExtraMarkers(Dictionary<string, int> dictSourceMarkers, Dictionary<string, int> dictTargetMarkers, TransUnit tu)
		{
			bool retval = true;
			foreach (var key in dictSourceMarkers.Keys)
			{
				if (dictTargetMarkers.ContainsKey(key))
				{
					if (dictTargetMarkers[key] < dictSourceMarkers[key])
					{
						// warning: missing x instances, might possibly be valid
						if (!_quiet)
							Console.WriteLine(@"Translation of {0} is missing {1} copies of the marker: {2}",
								tu.Id, dictSourceMarkers[key] - dictTargetMarkers[key], key);
						retval = false;
					}
					else if (dictTargetMarkers[key] > dictSourceMarkers[key])
					{
						// info: more instances, but may be valid
						if (!_quiet)
							Console.WriteLine(@"Translation of {0} has {1} extra copies of the marker: {2}",
								tu.Id, dictTargetMarkers[key] - dictSourceMarkers[key], key);
						retval = false;
					}
				}
				else
				{
					// missing altogether, probably invalid
					if (!_quiet)
						Console.WriteLine(@"Translation of {0} is missing the marker: {1}", tu.Id, key);
					retval = false;
				}
			}
			foreach (var key in dictTargetMarkers.Keys)
			{
				if (!dictSourceMarkers.ContainsKey(key))
				{
					if (!_quiet)
						Console.WriteLine(@"Translation of {0} has an unexpected marker: {1}", tu.Id, key);
					retval = false;
				}
			}
			return retval;
		}

		/// <summary>
		/// Store the different markers found by the regex search and how often each occurs in the provided dictionary.
		/// </summary>
		private static void TabulateMarkers(MatchCollection matches, Dictionary<string, int> dictMarkers)
		{
			dictMarkers.Clear();
			for (int i = 0; i < matches.Count; ++i)
			{
				var key = matches[i].Value;
				if (dictMarkers.ContainsKey(key))
				{
					var val = dictMarkers[key];
					dictMarkers[key] = val + 1;
				}
				else
				{
					dictMarkers.Add(key, 1);
				}
			}
		}
	}
}
