using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;

namespace L10NSharp.CodeReader
{
	/// ------------------------------------------------------------------------------------
	public class StringExtractor
	{
		private MethodInfo[] _getStringMethodOverloads;
		private List<LocalizingInfo> _getStringCallsInfo;
		private Dictionary<string, LocalizingInfo> _extenderInfo;
		private List<ILInstruction> _instructions;
		private readonly HashSet<string> _scannedTypes = new HashSet<string>();

		/// ------------------------------------------------------------------------------------
		public void ExtractFromNamespaces()
		{
		}

		/// ------------------------------------------------------------------------------------
		public IEnumerable<LocalizingInfo> DoExtractingWork(string[] namespaceBeginings, BackgroundWorker worker)
		{
			_getStringMethodOverloads = typeof(LocalizationManager)
				.GetMethods(BindingFlags.Static | BindingFlags.Public)
				.Where(m => m.Name == "GetString" || m.Name=="Localize")
				.Union(typeof(L10NStringExtensions)
				.GetMethods(BindingFlags.Static | BindingFlags.Public)
				.Where(m => m.Name == "Localize"))
				.ToArray();


			_getStringCallsInfo = new List<LocalizingInfo>();
			_extenderInfo = new Dictionary<string, LocalizingInfo>();

			int i = 1;
			var typesToScan = GetTypesToScan(namespaceBeginings).ToArray();
			foreach (var type in typesToScan)
			{
				FindLocalizedStringsInType(type);
				var pct = (int)Math.Round(((i++) / (double)typesToScan.Length) * 100d, 0, MidpointRounding.AwayFromZero);
				if (worker != null)
					worker.ReportProgress(pct);
				_scannedTypes.Add(type.FullName);

				// The above code finds calls in sets like
				// this._L10NSharpExtender.SetLocalizingId(this.myControl, "MyClass.SomeId");
				// this.myControl.Text = "some string to localize";
				// However, if the text is more than 200 characters, Visual studio perversely replaces the second line with
				// this.myControl.Text = resources.GetString("myControl.Text");
				// The code scanner does not find calls like that, since it is looking for set_Text with a string literal arg.
				// The following block retrieves the control's resources (if any) and fills in the text field
				// of any controls which have localizaztion info (typically at least a call to SetLocalizingId
				// has been successfully scanned with a literal string argument).
				try
				{
					var resources = new ResourceManager(type);
					using (var set = resources.GetResourceSet(CultureInfo.InvariantCulture, true, false))
					{
						if (set != null)
						{
							foreach (DictionaryEntry res in set)
							{
								var key = res.Key as string;
								var val = res.Value as string;
								if (key == null || val == null)
									continue;
								if (!key.EndsWith(".Text"))
									continue;
								key = key.Substring(0, key.Length - ".Text".Length);
								key = GetLocalizingKey(type.Name, key);
								LocalizingInfo info;
								if (_extenderInfo.TryGetValue(key, out info) && String.IsNullOrEmpty(info.Text))
								{
									info.Text = val;
								}
							}
						}
					}
					//Debug.WriteLine(String.Format("DEBUG: StringExtractor.DoExtractingWork() loaded resources for {0}", type.FullName));
				}
				catch (MissingManifestResourceException /*e*/)
				{
					// If it doesn't find any resources, no reason to die, we're just making a best attempt.
					Debug.WriteLine(String.Format("DEBUG: StringExtractor.DoExtractingWork() could not load resources for {0}", type.FullName));
				}
			}

			var extenderInfo = _extenderInfo
				.Where(kvp => !kvp.Key.EndsWith(".throwaway"))
				.Select(kvp => kvp.Value)
				.Where(l => l.Id != null && l.Priority != LocalizationPriority.NotLocalizable &&
					(l.Text != null || l.ToolTipText != null || l.ShortcutKeys != null));

			_getStringCallsInfo.AddRange(extenderInfo);
			_getStringCallsInfo = _getStringCallsInfo.Distinct(new LocInfoDistinctComparer()).OrderBy(l => l.Id).ToList();

			if (worker != null)
				worker.ReportProgress(100);

			foreach (var locInfo in _getStringCallsInfo)
			{
				locInfo.LangId = LocalizationManager.kDefaultLang;
				locInfo.UpdateFields = UpdateFields.None;

				if (locInfo.Text != null)
					locInfo.UpdateFields |= UpdateFields.Text;
				if (locInfo.Comment != null)
					locInfo.UpdateFields |= UpdateFields.Comment;
				if (locInfo.ToolTipText != null)
					locInfo.UpdateFields |= UpdateFields.ToolTip;
				if (locInfo.ShortcutKeys != null)
					locInfo.UpdateFields |= UpdateFields.ShortcutKeys;
			}

			return _getStringCallsInfo;
		}

		/// ------------------------------------------------------------------------------------
		private IEnumerable<Type> GetTypesToScan(ICollection<string> namespaceBeginnings)
		{
			var typesToScan = new HashSet<Type>();

			foreach (var assembly in GetAllAssemblies())
			{
				if (/* fails: assembly.Location.ToLower().Contains("framework") ||*/
					assembly.FullName.Contains("mscorlib") || assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Microsoft"))
					continue;

				try
				{
					foreach (var type in assembly.GetTypes()
						.Where(t => !typesToScan.Contains(t))
						.Where(type => namespaceBeginnings.Count == 0 || namespaceBeginnings.Any(nsb => type.FullName.StartsWith(nsb))))
					{
						typesToScan.Add(type);
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					Debug.Print("Unable to load assembly {0}:{1}", assembly.FullName, ex.Message);
				}
				catch (TypeLoadException ex)
				{
					Debug.Print("Unable to load type {0}:{1}", assembly.FullName, ex.Message);
				}
			}

			return typesToScan;
		}

		/// ------------------------------------------------------------------------------------
		private class LocInfoDistinctComparer : IEqualityComparer<LocalizingInfo>
		{
			/// ------------------------------------------------------------------------------------
			public bool Equals(LocalizingInfo x, LocalizingInfo y)
			{
				return x.Id.Equals(y.Id, StringComparison.Ordinal);
			}

			/// ------------------------------------------------------------------------------------
			public int GetHashCode(LocalizingInfo obj)
			{
				return obj.Id.GetHashCode();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Optional list of assemblies to scan that aren't part of the currently executing code.
		/// If this is set, no other loaded assemblies are scanned.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Assembly[] ExternalAssembliesToScan;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// If ExternalAssembliesToScan has data, return its value. Otherwise, return a list of
		/// all assemblies referenced by the entry assembly if it exists, or a list of assemblies
		/// loaded in the AppDomain as a last resort.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private IEnumerable<Assembly> GetAllAssemblies()
		{
			if (ExternalAssembliesToScan != null && ExternalAssembliesToScan.Length > 0)
				return ExternalAssembliesToScan;

			// If no entry assembly, just get assemblies loaded in AppDomain
			if (Assembly.GetEntryAssembly() != null)
			{
				foreach (AssemblyName assemblyName in Assembly.GetEntryAssembly().GetReferencedAssemblies())
				{
					try
					{
						Assembly.Load(assemblyName);
					}
					catch (FileNotFoundException)
					{
						// Ignore assemblies that aren't distributed with the released version
					}
				}
			}

			return AppDomain.CurrentDomain.GetAssemblies().ToList();
		}

		/// ------------------------------------------------------------------------------------
		private void FindLocalizedStringsInType(Type type)
		{
			if(!TypeNeedsLocalization(type))
				return;

			var methodsInType = new List<MethodBase>();

			methodsInType.AddRange(type.GetConstructors(BindingFlags.Static |
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

			try
			{
				// The DeclaredOnly flag ensures we do not include public methods from superclasses of this subtype.
				// If the superclass is in a requested namespace, we will collect it from the superclass directly.
				// But this ensures we don't collect it if the superclass is in a namespace which wasn't requested.
				methodsInType.AddRange(type.GetMethods(BindingFlags.Static |
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
			}
			catch (TypeLoadException)
			{
				// Caused by assemblies that have odd runtime loading problems (e.g. SIL.Windows.Forms.Keyboarding). Ignore.
				return;
			}
			foreach (var method in methodsInType)
			{
				try
				{
#if DEBUG
					// Set the environment variable L10NSHARPDEBUGGING to true to find out what types are being
					// searched for string calls. This is helpful for tracking down linux sigsev problems.
					if((Environment.GetEnvironmentVariable("L10NSHARPDEBUGGING") ?? "false").ToLower() == "true")
						Console.WriteLine(@"Looking for strings in {0}.{1}", type.Name, method.Name);
#endif
					_instructions = new List<ILInstruction>(new ILReader(method));

					foreach (var getStringOverload in _getStringMethodOverloads)
						FindGetStringCalls(method, getStringOverload);

					FindExtenderCalls(method);
				}
				catch (FileNotFoundException)
				{
					// Caused by assemblies that cannot be loaded at runtime (e.g. nunit). Ignore.
				}
				catch (TypeLoadException)
				{
					// Caused by assemblies that have odd runtime loading problems (e.g. Chorus). Ignore.
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		public void FindGetStringCalls(MethodBase caller, MethodInfo callee)
		{
			var module = caller.Module;
			var calleeParamCount = callee.GetParameters().Count();

			for (int i = 1; i < _instructions.Count; i++)
			{
				if (_instructions[i].opCode != OpCodes.Call)
					continue;

				Type[] genericMethodArguments = null;
				var genericTypeArguments = caller.DeclaringType.GetGenericArguments();

				if ((!caller.IsConstructor) && (!caller.Name.Equals(".cctor")))
					genericMethodArguments = caller.GetGenericArguments();

				if (callee.Equals(module.ResolveMethod((int)_instructions[i].operand,
					genericTypeArguments, genericMethodArguments)))
				{
					LocalizingInfo locInfo;
					if (callee.Name == "Localize")
					{
						locInfo = GetInfoForCallToLocalizeExtension(module, i, calleeParamCount);
					}
					else
					{
						locInfo = GetInfoForCallToGetStringMethod(module, i, calleeParamCount);
					}
					if (locInfo != null)
						_getStringCallsInfo.Add(locInfo);
					else
						Debug.Print("Call to {0} in {1} ({2}) could not be parsed", callee, caller.Name, caller.DeclaringType.Name);
				}
			}
		}

		/// <summary>
		/// This is special because, as an extension method the 1st parameter in "hello".Localize("myapp.greeting") will be the string ("hello") itself,
		/// which is backwards from the convention used in the GetString(id, theString, etc.)
		/// </summary>
		private LocalizingInfo GetInfoForCallToLocalizeExtension(Module module,int instrIndex, int paramsInMethodCall)
		{
			var parameters = GetParameters(module, instrIndex, paramsInMethodCall);

			//begin part that differes from GetInfoForCallToLocalizationMethod (for now)

			if (parameters[0] == null)
				return null;

			string id;
			if(String.IsNullOrEmpty(parameters[1]))
			{
				id = parameters[0];
			}
			else
			{
				id = parameters[1];
			}

			var locInfo = new LocalizingInfo(id);
			locInfo.Text = parameters[0];

			//end part that differes from GetInfoForCallToLocalizationMethod

			if (paramsInMethodCall >= 3 && parameters[2] != null)
				locInfo.Comment = parameters[2];

			if (paramsInMethodCall == 6)
			{
				if (parameters[3] != null)
					locInfo.ToolTipText = parameters[3];
				if (parameters[4] != null)
					locInfo.ShortcutKeys = parameters[4];
			}

			return locInfo;
		}

		private string[] GetParameters(Module module, int instrIndex, int paramsInMethodCall)
		{
			int parameterIndex = paramsInMethodCall - 1;
			var parameters = new string[paramsInMethodCall];
			for (int i = 1; ; i++)
			{
				if (_instructions[instrIndex - i].opCode == OpCodes.Ldstr)
					parameters[parameterIndex--] = module.ResolveString((int) _instructions[instrIndex - i].operand);
				else if (_instructions[instrIndex - i].opCode != OpCodes.Call)
				{
					parameterIndex--;
					while (_instructions[instrIndex - i].opCode == OpCodes.Ldfld)
						i++;
				}

				if (parameterIndex < 0)
					break;
			}
			return parameters;
		}

		/// ------------------------------------------------------------------------------------
		private LocalizingInfo GetInfoForCallToGetStringMethod(Module module,
			int instrIndex, int paramsInMethodCall)
		{
			var parameters = GetParameters(module, instrIndex, paramsInMethodCall);

			if (parameters[0] == null || parameters[1] == null)
				return null;

			var locInfo = new LocalizingInfo(parameters[0]);
			locInfo.Text = parameters[1];

			if (paramsInMethodCall >= 3 && parameters[2] != null)
				locInfo.Comment = parameters[2];

			if (paramsInMethodCall == 6)
			{
				if (parameters[3] != null)
					locInfo.ToolTipText = parameters[3];
				if (parameters[4] != null)
					locInfo.ShortcutKeys = parameters[4];
			}

			return locInfo;
		}

		/// ------------------------------------------------------------------------------------
		public void FindExtenderCalls(MethodBase caller)
		{
			var module = caller.Module;

			for (int i = 1; i < _instructions.Count; i++)
			{
				string text = null;

				if (_instructions[i].opCode == OpCodes.Ldstr)
				{
					text = module.ResolveString((int)_instructions[i].operand);
					if (text.StartsWith(LocalizationManager.kL10NPrefix))
					{
						var locInfo = GetLocInfoForField(caller.ReflectedType.Name, text);
						locInfo.Id = LocalizingInfo.GetIdFromText(text);
						locInfo.Text = LocalizationManager.StripOffLocalizationInfoFromText(text);
						continue;
					}
				}

				if (_instructions[i].opCode != OpCodes.Callvirt &&
					_instructions[i].opCode != OpCodes.Calli &&
					_instructions[i].opCode != OpCodes.Call)
				{
					continue;
				}

				if (_instructions[i - 1].opCode == OpCodes.Ldnull)
					continue;

				Type[] genericMethodArguments = null;
				var genericTypeArguments = caller.DeclaringType.GetGenericArguments();

				if ((!caller.IsConstructor) && (!caller.Name.Equals(".cctor")))
					genericMethodArguments = caller.GetGenericArguments();

				string fldName = null;

				MethodBase mi = null;
				try
				{
					mi = module.ResolveMethod((int)_instructions[i].operand,
						genericTypeArguments, genericMethodArguments);

				}
				catch (Exception)
				{
					//We started getting this with Palaso.ClearShare.LicenseInfo.Token, which is abstract. Could not determine just what causes it, could not reproduce in a test.
					//So it's not worth stopping the train over....
					continue;
				}

				if (mi.Name.Equals("SetLocalizationPriority", StringComparison.Ordinal))
				{
					var priority = (LocalizationPriority)(_instructions[i - 1].opCode.Value - 22);
					fldName = GetFieldName(module, _instructions[i - 2]);
					GetLocInfoForField(caller.ReflectedType.Name, fldName).Priority = priority;
					continue;
				}

				 text = (i > 1 && _instructions[i - 1].opCode == OpCodes.Ldstr ?
					module.ResolveString((int)_instructions[i - 1].operand) : null);

				if (text == null)
					continue;

				if (mi.Name.Equals("SetLocalizingId", StringComparison.Ordinal))
				{
					fldName = GetFieldName(module, _instructions[i - 2]);
					GetLocInfoForField(caller.ReflectedType.Name, fldName).Id = text;
				}
				else if (mi.Name.Equals("SetLocalizationComment", StringComparison.Ordinal))
				{
					fldName = GetFieldName(module, _instructions[i - 2]);
					GetLocInfoForField(caller.ReflectedType.Name, fldName).Comment = text;
				}
				else if (mi.Name.Equals("SetLocalizableToolTip", StringComparison.Ordinal))
				{
					fldName = GetFieldName(module, _instructions[i - 2]);
					GetLocInfoForField(caller.ReflectedType.Name, fldName).ToolTipText = text;
				}
				else if (mi.Name.Equals("set_Text", StringComparison.Ordinal))
				{
					fldName = GetFieldName(module, _instructions[i - 2]);
					GetLocInfoForField(caller.ReflectedType.Name, fldName).Text = text;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		private string GetFieldName(Module module, ILInstruction instruction)
		{
			if (instruction.opCode == OpCodes.Ldfld)
				return module.ResolveField((int)instruction.operand).Name;

			return (instruction.opCode == OpCodes.Ldarg_0 ? "Form" : "throwaway");
		}

		/// ------------------------------------------------------------------------------------
		private LocalizingInfo GetLocInfoForField(string className, string fieldName)
		{
			var key = GetLocalizingKey(className, fieldName);

			LocalizingInfo locInfo;
			if (_extenderInfo.TryGetValue(key, out locInfo))
				return locInfo;

			locInfo = new LocalizingInfo(null) { Priority = LocalizationPriority.Medium };
			_extenderInfo[key] = locInfo;
			return locInfo;
		}

		private static string GetLocalizingKey(string className, string fieldName)
		{
			return className + "." + fieldName;
		}

		/// <summary>
		/// Check if this type has a custom attribute which tells us to ignore it
		/// </summary>
		private static bool TypeNeedsLocalization(Type type)
		{
			var customAttributes = type.GetCustomAttributes(typeof(NoLocalizableStringsPresent), false);
			return ElementNeedsLocalization(customAttributes);
		}

		/// <summary>
		/// Check if this method has a custom attribute which tells us to ignore it.
		/// </summary>
		internal static bool MethodNeedsLocalization(MethodBase method)
		{
			var customAttributes = method.GetCustomAttributes(typeof(NoLocalizableStringsPresent), false);
			return ElementNeedsLocalization(customAttributes);
		}

		/// <summary>
		/// Test the custom attribute to determine if we should ignore localization for this element on this OS.
		/// </summary>
		private static bool ElementNeedsLocalization(object[] customAttributes)
		{
			if(customAttributes.Length == 0)
				return true;

			var attribute = (NoLocalizableStringsPresent)customAttributes[0];
			switch(Environment.OSVersion.Platform)
			{
				case PlatformID.Win32Windows:
				case PlatformID.Win32NT:
					return (attribute.DoNotLocalizeOn & NoLocalizableStringsPresent.OS.Windows) == 0;
				case PlatformID.MacOSX:
					return (attribute.DoNotLocalizeOn & NoLocalizableStringsPresent.OS.Mac) == 0;
				case PlatformID.Unix:
					return (attribute.DoNotLocalizeOn & NoLocalizableStringsPresent.OS.Linux) == 0;
				default:
					return true;
			}
		}
	}
}
