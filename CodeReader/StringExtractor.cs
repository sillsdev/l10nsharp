using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;

namespace Localization.CodeReader
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
				.Where(m => m.Name == "GetString").ToArray();

			_getStringCallsInfo = new List<LocalizingInfo>();
			_extenderInfo = new Dictionary<string, LocalizingInfo>();

			int i = 1;
			var typesToScan = GetTypesToScan(namespaceBeginings).ToArray();
			foreach (var type in typesToScan)
			{
				FindLocalizedStringsInType(type);
				var pct = (int)Math.Round(((i++) / (double)typesToScan.Length) * 100d, 0, MidpointRounding.AwayFromZero);
				worker.ReportProgress(pct);
				_scannedTypes.Add(type.FullName);
			}

			var extenderInfo = _extenderInfo
				.Where(kvp => !kvp.Key.EndsWith(".throwaway"))
				.Select(kvp => kvp.Value)
				.Where(l => l.Id != null && l.Priority != LocalizationPriority.NotLocalizable &&
					(l.Text != null || l.ToolTipText != null || l.ShortcutKeys != null));

			_getStringCallsInfo.AddRange(extenderInfo);
			_getStringCallsInfo = _getStringCallsInfo.Distinct(new LocInfoDistinctComparer()).OrderBy(l => l.Id).ToList();

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
		/// Gets a list of all assemblies referenced by the entry assembly
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private IEnumerable<Assembly> GetAllAssemblies()
		{
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
			var methodsInType = new List<MethodBase>();

			methodsInType.AddRange(type.GetConstructors(BindingFlags.Static |
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

			methodsInType.AddRange(type.GetMethods(BindingFlags.Static |
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

			foreach (var method in methodsInType)
			{
				try
				{
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
					var locInfo = GetInfoForCallToLocalizationMethod(module, i, calleeParamCount);
					if (locInfo != null)
						_getStringCallsInfo.Add(locInfo);
					else
						Debug.Print("Call to {0} in {1} ({2}) could not be parsed", callee, caller.Name, caller.DeclaringType.Name);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		private LocalizingInfo GetInfoForCallToLocalizationMethod(Module module,
			int instrIndex, int paramsInMethodCall)
		{
			var parameters = new string[paramsInMethodCall];
			int parameterIndex = paramsInMethodCall - 1;

			for (int i = 1; ; i++)
			{
				if (_instructions[instrIndex - i].opCode == OpCodes.Ldstr)
					parameters[parameterIndex--] = module.ResolveString((int)_instructions[instrIndex - i].operand);
				else if (_instructions[instrIndex - i].opCode != OpCodes.Call)
				{
					parameterIndex--;
					if (_instructions[instrIndex - i].opCode == OpCodes.Ldfld)
						i++;
				}

				if (parameterIndex < 0)
					break;
			}

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

				var mi = module.ResolveMethod((int)_instructions[i].operand,
					genericTypeArguments, genericMethodArguments);

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
			var key = className + "." + fieldName;

			LocalizingInfo locInfo;
			if (_extenderInfo.TryGetValue(key, out locInfo))
				return locInfo;

			locInfo = new LocalizingInfo(null) { Priority = LocalizationPriority.High };
			_extenderInfo[key] = locInfo;
			return locInfo;
		}
	}
}
