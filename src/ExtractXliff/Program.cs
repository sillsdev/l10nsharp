using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Resources;
using System.Globalization;
using System.Collections;
using L10NSharp;
using L10NSharp.CodeReader;
using L10NSharp.XLiffUtils;

namespace ExtractXliff
{
	class Program
	{
		static List<string> _namespaces = new List<string>();
		static string _xliffFilename;
		static string _datatype;
		static string _original;
		static string _productVersion;
		static string _baseXliffFilename;
		static bool _verbose;
		static bool _doneWithOptions;
		static List<string> _assemblyFiles = new List<string>();

		const string kDefaultLangId = "en";
		const string kDefaultNewlineReplacement = "\\n";
		const string kDefaultAmpersandReplacement = "|amp|";

		static void Main(string[] args)
		{
			if (!ParseOptions(args))
			{
				Usage();
				return;
			}
			List<Assembly> assemblies = new List<Assembly>();
			foreach (var file in _assemblyFiles)
			{
				var asm = Assembly.LoadFile(file);
				if (asm != null)
					assemblies.Add(asm);
			}
			if (_namespaces.Count > 0 && assemblies.Count > 0)
			{
				var extractor = new StringExtractor();
				extractor.ExternalAssembliesToScan = assemblies.ToArray();
				IEnumerable<LocalizingInfo> localizedStrings = extractor.DoExtractingWork(_namespaces.ToArray(), null);
				var lm = new LocalizationManager(_original, _original, _productVersion);
				var stringCache = new LocalizedStringCache(lm, false);
				foreach (var locInfo in localizedStrings)
					stringCache.UpdateLocalizedInfo(locInfo);
				var newDoc = stringCache.XliffDocuments[kDefaultLangId];
				XLiffDocument baseDoc = null;
				if (_baseXliffFilename != null)
				{
					baseDoc = XLiffDocument.Read(_baseXliffFilename);
					if (baseDoc.File.SourceLang != newDoc.File.SourceLang && baseDoc.File.SourceLang != kDefaultLangId)
					{
						Console.WriteLine("ERROR: old source-language ({0}) is not the same as the new source-language ({1})",
							baseDoc.File.SourceLang, newDoc.File.SourceLang);
						return;
					}
					if (baseDoc.File.Original != newDoc.File.Original && baseDoc.File.Original != _original)
					{
						Console.WriteLine("WARNING: old original ({0}) is not the same as the new original ({1})",
							baseDoc.File.Original, newDoc.File.Original);
					}
					if (baseDoc.File.DataType != newDoc.File.DataType && baseDoc.File.DataType != _datatype)
					{
						Console.WriteLine("WARNING: old datatype ({0}) is not the same as the new datatype ({1})",
							baseDoc.File.DataType, newDoc.File.DataType);
					}
					if (baseDoc.File.ProductVersion != newDoc.File.ProductVersion && baseDoc.File.ProductVersion != _productVersion)
					{
						Console.WriteLine("WARNING: old product-version ({0}) is not the same as the new product-version ({1})",
							baseDoc.File.ProductVersion, newDoc.File.ProductVersion);
					}
				}
				SaveFileForLangId(kDefaultLangId, newDoc, baseDoc);
			}
		}

		private static void SaveFileForLangId(string langId, XLiffDocument xliffNew, XLiffDocument xliffOld)
		{
			var xliffOutput = new XLiffDocument();
			xliffOutput.File.SourceLang = kDefaultLangId;
			if (!String.IsNullOrEmpty(_productVersion))
				xliffOutput.File.ProductVersion = _productVersion;
			xliffOutput.File.HardLineBreakReplacement = kDefaultNewlineReplacement;
			xliffOutput.File.AmpersandReplacement = kDefaultAmpersandReplacement;
			xliffOutput.File.Original = _original;
			if (!String.IsNullOrEmpty(_datatype))
				xliffOutput.File.DataType = _datatype;

			var newStringCount = 0;
			var changedStringCount = 0;
			var wrongDynamicFlagCount = 0;
			var missingDynamicStringCount = 0;
			var missingStringCount = 0;

			var newStringIds = new List<string>();
			var changedStringIds = new List<string>();
			var wrongDynamicStringIds = new List<string>();
			var missingDynamicStringIds = new List<string>();
			var missingStringIds = new List<string>();

			foreach (var tu in xliffNew.File.Body.TransUnits)
			{
				xliffOutput.File.Body.TransUnits.Add(tu);
				if (xliffOld != null)
				{
					var tuOld = xliffOld.File.Body.GetTransUnitForId(tu.Id);
					if (tuOld == null)
					{
						++newStringCount;
						newStringIds.Add(tu.Id);
					}
					else
					{
						if (tu.Source.Value != tuOld.Source.Value)
						{
							++changedStringCount;
							changedStringIds.Add(tu.Id);
						}
						if (tuOld.Dynamic)
						{
							++wrongDynamicFlagCount;
							wrongDynamicStringIds.Add(tu.Id);
						}
					}
				}
			}
			if (xliffOld != null)
			{
				foreach (var tu in xliffOld.File.Body.TransUnits)
				{
					var tuNew = xliffNew.File.Body.GetTransUnitForId(tu.Id);
					if (tuNew == null)
					{
						xliffOutput.File.Body.TransUnits.Add(tu);
						if (tu.Dynamic)
						{
							++missingDynamicStringCount;
							missingDynamicStringIds.Add(tu.Id);
						}
						else
						{
							++missingStringCount;
							missingStringIds.Add(tu.Id);
						}
					}
				}
			}
			if (newStringCount > 0)
			{
				Console.WriteLine("Added {0} new strings to the xliff file", newStringCount);
				if (_verbose)
				{
					newStringIds.Sort();
					foreach (var id in newStringIds)
						Console.WriteLine("    {0}", id);
				}
			}
			if (changedStringCount > 0)
			{
				Console.WriteLine("{0} strings were updated in the xliff file.", changedStringCount);
				if (_verbose)
				{
					changedStringIds.Sort();
					foreach (var id in changedStringIds)
						Console.WriteLine("    {0}", id);
				}
			}
			if (wrongDynamicFlagCount > 0)
			{
				Console.WriteLine("{0} strings were marked dynamic incorrectly.", wrongDynamicFlagCount);
				if (_verbose)
				{
					wrongDynamicStringIds.Sort();
					foreach (var id in wrongDynamicStringIds)
						Console.WriteLine("    {0}", id);
				}
			}
			if (missingDynamicStringCount > 0)
			{
				Console.WriteLine("{0} dynamic strings were added back from the old xliff file", missingDynamicStringCount);
				if (_verbose)
				{
					missingDynamicStringIds.Sort();
					foreach (var id in missingDynamicStringIds)
						Console.WriteLine("    {0}", id);
				}
			}
			if (missingStringCount > 0)
			{
				Console.WriteLine("{0} possibly obsolete (maybe dynamic?) strings were added back from the old xliff file", missingStringCount);
				if (_verbose)
				{
					missingStringIds.Sort();
					foreach (var id in missingStringIds)
						Console.WriteLine("    {0}", id);
				}
			}
			xliffOutput.File.Body.TransUnits.Sort(LocalizedStringCache.TuComparer);
			xliffOutput.Save(_xliffFilename);
		}

		static bool ParseOptions(string[] args)
		{
			bool error = false;
			for (int i = 0; i < args.Length; ++i)
			{
				if (!_doneWithOptions)
				{
					if (AddNamespace(args, ref i, ref error))
						continue;
					if (SetXliff(args, ref i, ref error))
						continue;
					if (SetOriginal(args, ref i, ref error))
						continue;
					if (SetProductVersion(args, ref i, ref error))
						continue;
					if (SetDatatype(args, ref i, ref error))
						continue;
					if (SetBaseXliff(args, ref i, ref error))
						continue;
					if (SetVerbose(args, ref i, ref error))
						continue;
					if (args[i] == "--")
					{
						_doneWithOptions = true;
						continue;
					}
					if (error || args[i].StartsWith("-"))
						return false;
				}
				_doneWithOptions = true;
				_assemblyFiles.Add(args[i]);
			}
			return _namespaces.Count > 0 &&
				!String.IsNullOrEmpty(_xliffFilename) &&
				!String.IsNullOrEmpty(_original) &&
				_assemblyFiles.Count > 0;
		}

		static bool AddNamespace(string[] args, ref int i, ref bool error)
		{
			if (error)
				return false;
			if (args[i] == "-n" || "--namespace".StartsWith(args[i]))
			{
				++i;
				if (i < args.Length)
				{
					_namespaces.Add(args[i]);
					return true;
				}
				else
				{
					error = true;
				}
			}
			return false;
		}

		static bool SetXliff(string[] args, ref int i, ref bool error)
		{
			if (error)
				return false;
			if (args[i] == "-x" || "--xliff-file".StartsWith(args[i]))
			{
				++i;
				if (i < args.Length && _xliffFilename == null)
				{
					_xliffFilename = args[i];
					return true;
				}
				else
				{
					error = true;
				}
			}
			return false;
		}

		static bool SetOriginal(string[] args, ref int i, ref bool error)
		{
			if (error)
				return false;
			if (args[i] == "-o" || "--original".StartsWith(args[i]))
			{
				++i;
				if (i < args.Length && _original == null)
				{
					_original = args[i];
					return true;
				}
				else
				{
					error = true;
				}
			}
			return false;
		}

		static bool SetProductVersion(string[] args, ref int i, ref bool error)
		{
			if (error)
				return false;
			if (args[i] == "-p" || "--product-version".StartsWith(args[i]))
			{
				++i;
				if (i < args.Length && _productVersion == null)
				{
					_productVersion = args[i];
					return true;
				}
				else
				{
					error = true;
				}
			}
			return false;
		}

		static bool SetDatatype(string[] args, ref int i, ref bool error)
		{
			if (error)
				return false;
			if (args[i] == "-d" || "--datatype".StartsWith(args[i]))
			{
				++i;
				if (i < args.Length && _datatype == null)
				{
					_datatype = args[i];
					return true;
				}
				else
				{
					error = true;
				}
			}
			return false;
		}

		static bool SetBaseXliff(string[] args, ref int i, ref bool error)
		{
			if (error)
				return false;
			if (args[i] == "-b" || "--base-xliff".StartsWith(args[i]))
			{
				++i;
				if (i < args.Length && _datatype == null)
				{
					_baseXliffFilename = args[i];
					return true;
				}
				else
				{
					error = true;
				}
			}
			return false;
		}

		static bool SetVerbose(string[] args, ref int i, ref bool error)
		{
			if (error)
				return false;
			if (args[i] == "-v" || "--verbose".StartsWith(args[i]))
			{
				_verbose = true;
				return true;
			}
			return false;
		}

		static void Usage()
		{
			Console.WriteLine("usage: ExtractXliff [options] assembly-file(s)");
			Console.WriteLine("-n  --namespace = namespace beginning [one or more required]");
			Console.WriteLine("-x  --xliff-file = output .xlf file [one required]");
			Console.WriteLine("-o  --original = file element attribute value [one required]");
			Console.WriteLine("-d  --datatype = file element attribute value [one optional]");
			Console.WriteLine("-p  --product-version = file element attribute value [one optional]");
			Console.WriteLine("-b  --base-xliff = existing xliff file to serve as base for output [one optional]");
			Console.WriteLine("-v  --verbose = produce verbose output on differences from base file [optional]");
			Console.WriteLine("Every option except -v (--verbose) consumes a following argument as its value.");
			Console.WriteLine("The option list can be terminated by \"--\" in case an assembly filename starts");
			Console.WriteLine("with a dash (\"-\").  One or more assembly files (either .dll or .exe) are");
			Console.WriteLine("required following all of the options.  If a base xliff file is given, then its");
			Console.WriteLine("content serves as the base for the output, with the extracted strings merged");
			Console.WriteLine("into, and updating, the existing strings.  Statistics are then written to the");
			Console.WriteLine("console for the number of new strings, changed strings, identical strings, and");
			Console.WriteLine("number of strings in the base that were not extracted.");
		}
	}
}
