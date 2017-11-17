// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
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
	/// <summary>
	/// This program uses the L10NSharp code for extracting static strings from one or
	/// more C# assemblies (either .dll or .exe).  It requires command line arguments
	/// to set the internal XLIFF file element "original" attribute, to set the
	/// namespace beginning(s), and to set the output XLIFF filename.  It also requires
	/// one or more assembly files to be specified on the command line.  There are
	/// also optional command line arguments for specifying the XLIFF file element
	/// "datatype" attribute, the XLIFF file element "product-version" attribute, and
	/// an existing XLIFF file to merge from after reading everything from the input
	/// assemblies.
	/// </summary>
	class Program
	{
		delegate bool ProcessOption(string[] args, ref int i);

		static List<string> _namespaces = new List<string>();	// namespace beginning(s) (-n)
		static string _xliffOutputFilename;			// new XLIFF output file (-x)
		static string _fileDatatype;				// file element attribute value (-d)
		static string _fileOriginal;				// file element attribute value (-o)
		static string _fileProductVersion;			// file element attribute value (-p)
		static string _baseXliffFilename;			// input file that provides existing data to merge (-b)
		static bool _verbose;						// verbose console output (-v)
		static bool _doneWithOptions;				// flag that either "--" or first assembly filename seen already
		static List<string> _assemblyFiles = new List<string>();	// input assembly file(s)

		const string kDefaultLangId = "en";
		const string kDefaultNewlineReplacement = "\\n";
		const string kDefaultAmpersandReplacement = "|amp|";

		static void Main(string[] args)
		{
			// Parse and check the command line arguments.
			if (!ParseOptions(args))
			{
				Usage();
				return;
			}

			// Load the input assemblies so that they can be scanned.
			List<Assembly> assemblies = new List<Assembly>();
			foreach (var file in _assemblyFiles)
			{
				var asm = Assembly.LoadFile(file);
				if (asm != null)
					assemblies.Add(asm);
			}

			// Scan the input assemblies for localizable strings.
			var extractor = new StringExtractor();
			extractor.ExternalAssembliesToScan = assemblies.ToArray();
			var localizedStrings = extractor.DoExtractingWork(_namespaces.ToArray(), null);

			// The arguments to this constructor don't really matter much as they're used internally by
			// L10NSharp for reasons that may not percolate out to xliff.  We just need a LocalizationManager
			// to feed into the constructor the LocalizedStringCache that does some heavy lifting for us in
			// creating the XliffDocument from the newly extracted localized strings.
			var lm = new LocalizationManager(_fileOriginal, _fileOriginal, _fileProductVersion);
			var stringCache = new LocalizedStringCache(lm, false);
			foreach (var locInfo in localizedStrings)
				stringCache.UpdateLocalizedInfo(locInfo);

			// Get the newly loaded static strings (in newDoc) and the baseline XLIFF (in baseDoc).
			var newDoc = stringCache.XliffDocuments[kDefaultLangId];
			var baseDoc = LoadBaselineAndCompare(newDoc);

			// Save the results to the output file, merging in data from the baseline XLIFF if one was specified.
			MergeAndSaveXliffDataToFile(newDoc, baseDoc);
		}

		/// <summary>
		/// Saves the merged XLIFF data to the output file.
		/// </summary>
		private static void MergeAndSaveXliffDataToFile(XLiffDocument xliffNew, XLiffDocument xliffOld)
		{
			// xliffNew has the data found in the current scan.
			// xliffOld is that data from the (optional) input baseline XLIFF file.
			// xliffOutput is the data that is actually written to the designated output XLIFF file.  It combines
			//   data from both xliffNew and xliffOld.

			// write the header elements of the new XLIFF file.
			var xliffOutput = new XLiffDocument();
			xliffOutput.File.SourceLang = kDefaultLangId;
			if (!String.IsNullOrEmpty(_fileProductVersion))
				xliffOutput.File.ProductVersion = _fileProductVersion;
			else
				xliffOutput.File.ProductVersion = xliffNew.File.ProductVersion;
			xliffOutput.File.HardLineBreakReplacement = kDefaultNewlineReplacement;
			xliffOutput.File.AmpersandReplacement = kDefaultAmpersandReplacement;
			xliffOutput.File.Original = _fileOriginal;
			if (!String.IsNullOrEmpty(_fileDatatype))
				xliffOutput.File.DataType = _fileDatatype;
			else
				xliffOutput.File.DataType = xliffNew.File.DataType;

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

			// write out the newly-found units, comparing against units with the same ids
			// found in the old XLIFF file.
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

			// write out any units found in the old XLIFF file that were not found
			// in the new scan.
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

			// report on the differences between the new scan and the old XLIFF file.
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
			xliffOutput.Save(_xliffOutputFilename);
		}

		/// <summary>
		/// Parse the command line arguments, storing the associated information for the program.
		/// </summary>
		/// <returns><c>true</c>, if the command line parse successfully, <c>false</c> otherwise.</returns>
		static bool ParseOptions(string[] args)
		{
			bool error = false;
			for (int i = 0; i < args.Length; ++i)
			{
				if (!_doneWithOptions)
				{
					if (ProcessArgument(args, "-n", "--namespace", AddNamespace, ref i, ref error))
						continue;
					if (ProcessArgument(args, "-x", "--xliff-file", SetXliff, ref i, ref error))
						continue;
					if (ProcessArgument(args, "-o", "--original", SetOriginal, ref i, ref error))
						continue;
					if (ProcessArgument(args, "-p", "--product-version", SetProductVersion, ref i, ref error))
						continue;
					if (ProcessArgument(args, "-d", "--datatype", SetDatatype, ref i, ref error))
						continue;
					if (ProcessArgument(args, "-b", "--base-xliff", SetBaseXliff, ref i, ref error))
						continue;
					if (ProcessArgument(args, "-v", "--verbose", SetVerbose, ref i, ref error))
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
				!String.IsNullOrEmpty(_xliffOutputFilename) &&
				!String.IsNullOrEmpty(_fileOriginal) &&
				_assemblyFiles.Count > 0;
		}

		/// <summary>
		/// Try to process one command line option.
		/// The ref input "i" may be incremented if the option was matched.  The ref input "error" is set if the option
		/// was matched but could not be fully processed.
		/// </summary>
		/// <returns><c>true</c>, if the option matches and was processed successfully, <c>false</c> otherwise.</returns>
		/// <param name="args">command line argument array</param>
		/// <param name="shortForm">short form of the option</param>
		/// <param name="longForm">long form of the option</param>
		/// <param name="action">method to process the option</param>
		/// <param name="i">index into args</param>
		/// <param name="error">flag an error in processing a matched option</param>
		static bool ProcessArgument(string[] args, string shortForm, string longForm, ProcessOption action, ref int i, ref bool error)
		{
			if (error)
				return false;
			if (args[i] == shortForm || longForm.StartsWith(args[i]))
			{
				var okay = action(args, ref i);
				if (okay)
					return true;
				error = true;
			}
			return false;
		}

		static bool AddNamespace(string[] args, ref int i)
		{
			// -n  --namespace = namespace beginning [one or more required]
			++i;
			if (i < args.Length)
			{
				if (args[i] == "--")
					return false;
				_namespaces.Add(args[i]);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Process an option that can occur only once and that has a string value.  The ref input "i" is always
		/// incremented.  An error occurs if that places it past the end of the input "args" array as an index.
		/// </summary>
		/// <returns><c>true</c>, if the option was processed successfully, <c>false</c> otherwise.</returns>
		static bool SetSingleValueOption(string[] args, ref int i, ref string optionValue)
		{
			++i;
			if (i < args.Length && optionValue == null)
			{
				if (args[i] == "--")
					return false;
				optionValue = args[i];
				return true;
			}
			return false;
		}

		static bool SetXliff(string[] args, ref int i)
		{
			// -x  --xliff-file = output .xlf file [one required]
			return SetSingleValueOption(args, ref i, ref _xliffOutputFilename);
		}

		static bool SetOriginal(string[] args, ref int i)
		{
			// -o  --original = file element attribute value [one required]
			return SetSingleValueOption(args, ref i, ref _fileOriginal);
		}

		static bool SetProductVersion(string[] args, ref int i)
		{
			// -p  --product-version = file element attribute value [one optional]
			return SetSingleValueOption(args, ref i, ref _fileProductVersion);
		}

		static bool SetDatatype(string[] args, ref int i)
		{
			// -d  --datatype = file element attribute value [one optional]
			return SetSingleValueOption(args, ref i, ref _fileDatatype);
		}

		static bool SetBaseXliff(string[] args, ref int i)
		{
			// -b  --base-xliff = existing xliff file to serve as base for output [one optional]
			return SetSingleValueOption(args, ref i, ref _baseXliffFilename);
		}

		static bool SetVerbose(string[] args, ref int i)
		{
			// -v  --verbose = produce verbose output on differences from base file [optional]
			_verbose = true;
			return true;
		}

		/// <summary>
		/// Display a usage message on the console.
		/// </summary>
		static void Usage()
		{
			Console.WriteLine("usage: ExtractXliff [options] assembly-file(s)");
			Console.WriteLine("-n  --namespace = namespace beginning [one or more required]");
			Console.WriteLine("-x  --xliff-file = output .xlf file [one required]");
			Console.WriteLine("-o  --original = file element attribute value [one required]");
			Console.WriteLine();
			Console.WriteLine("-d  --datatype = file element attribute value [one optional]");
			Console.WriteLine("-p  --product-version = file element attribute value [one optional]");
			Console.WriteLine("-b  --base-xliff = existing xliff file to serve as base for output [one optional]");
			Console.WriteLine("-v  --verbose = produce verbose output on differences from base file [optional]");
			Console.WriteLine();
			Console.WriteLine("Every option except -v (--verbose) consumes a following argument as its value.");
			Console.WriteLine("The option list can be terminated by \"--\" in case an assembly filename starts");
			Console.WriteLine("with a dash (\"-\").  One or more assembly files (either .dll or .exe) are");
			Console.WriteLine("required following all of the options.  If a base xliff file is given, then its");
			Console.WriteLine("content serves as the base for the output, with the extracted strings merged");
			Console.WriteLine("into, and updating, the existing strings.  Statistics are then written to the");
			Console.WriteLine("console for the number of new strings, changed strings, identical strings, and");
			Console.WriteLine("number of strings in the base that were not extracted.");
		}

		/// <summary>
		/// If one was provided, load the baseline XLIFF file and compare its file element attributes
		/// against the new data.
		/// </summary>
		/// <returns>The baseline XliffDocument, or null</returns>
		static XLiffDocument LoadBaselineAndCompare(XLiffDocument newDoc)
		{
			XLiffDocument baseDoc = null;
			if (_baseXliffFilename != null)
			{
				// Compare the new file element attributes against the baseline XLIFF file element attributes.
				// Complain if something has changed.
				baseDoc = XLiffDocument.Read(_baseXliffFilename);
				if (baseDoc.File.SourceLang != kDefaultLangId)
				{
					Console.WriteLine("ERROR: old source-language ({0}) is not the same as the new source-language ({1})",
						baseDoc.File.SourceLang, kDefaultLangId);
					throw new ApplicationException("Only " + kDefaultLangId + " is allowed as the source-language attribute");
				}
				if (baseDoc.File.Original != _fileOriginal)
				{
					Console.WriteLine("WARNING: old original ({0}) is not the same as the new original ({1})",
						baseDoc.File.Original, _fileOriginal);
				}
				if (String.IsNullOrEmpty(_fileDatatype))
				{
					if (baseDoc.File.DataType != newDoc.File.DataType)
					{
						Console.WriteLine("WARNING: old datatype ({0}) is not the same as the new datatype ({1})",
							baseDoc.File.DataType, newDoc.File.DataType);
					}
				}
				else
				{
					if (baseDoc.File.DataType != _fileDatatype)
					{
						Console.WriteLine("WARNING: old datatype ({0}) is not the same as the new datatype ({1})",
							baseDoc.File.DataType, _fileDatatype);
					}
				}
				if (String.IsNullOrEmpty(_fileProductVersion))
				{
					if (baseDoc.File.ProductVersion != newDoc.File.ProductVersion)
					{
						Console.WriteLine("WARNING: old product-version ({0}) is not the same as the new product-version ({1})",
							baseDoc.File.ProductVersion, newDoc.File.ProductVersion);
					}
				}
				else
				{
					if (baseDoc.File.ProductVersion != _fileProductVersion)
					{
						Console.WriteLine("WARNING: old product-version ({0}) is not the same as the new product-version ({1})",
							baseDoc.File.ProductVersion, _fileProductVersion);
					}
				}
			}
			return baseDoc;
		}
	}
}
