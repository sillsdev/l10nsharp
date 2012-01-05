using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace LocalizationCodeScanner
{
	public enum L10NTargetType
	{
		String,
		Object
	}

	[XmlType("lcsControlInfo")]
	public class ControlInfo
	{
		public string TopLevelSrcFolder { get; set; }
		public string OutputPath { get; set; }
		public string ProjectId { get; set; }
		public string ProjectName { get; set; }
		public string UILang { get; set; }
		public List<MethodCallInfo> MethodCalls { get; set; }
		public List<GroupOverride> GroupOverrides { get; set; }

		public static ControlInfo Load(string filePath)
		{
			var xml = XElement.Load(filePath);

			var ctrlInfo = new ControlInfo();

			foreach (var element in xml.Elements())
			{
				switch (element.Name.LocalName)
				{
					case "topLevelSourceCodeFolder": ctrlInfo.TopLevelSrcFolder = element.Value; break;
					case "outputPath": ctrlInfo.OutputPath = element.Value; break;
					case "projectId": ctrlInfo.ProjectId = element.Value; break;
					case "projectName": ctrlInfo.ProjectName = element.Value; break;
					case "uiLang": ctrlInfo.UILang = element.Value; break;
					case "methodCalls": ctrlInfo.MethodCalls = GetMethodCalls(element).ToList(); break;
					case "groupOverrides": ctrlInfo.GroupOverrides = GetGroupOverrides(element).ToList(); break;
				}
			}

			return ctrlInfo;
		}

		private static IEnumerable<MethodCallInfo> GetMethodCalls(XElement methodElement)
		{
			foreach (var element in methodElement.Elements())
			{
				var mci = new MethodCallInfo();
				mci.MethodCall = element.Value;

				switch (element.Attribute("localizationTarget").Value)
				{
					case "String": mci.TargetType = L10NTargetType.String; break;
					case "Object": mci.TargetType = L10NTargetType.Object; break;
				}

				yield return mci;
			}
		}

		private static IEnumerable<GroupOverride> GetGroupOverrides(XElement grpOverrideElement)
		{
			foreach (var element in grpOverrideElement.Elements())
			{
				var go = new GroupOverride();
				go.CalculatedGroup = element.Attribute("calculatedGroup").Value;
				go.DesiredGroup = element.Attribute("desiredGroup").Value;
				yield return go;
			}
		}
	}

	public class MethodCallInfo
	{
		public L10NTargetType TargetType { get; set; }
		public string MethodCall { get; set; }
	}

	public class GroupOverride
	{
		public string CalculatedGroup { get; set; }
		public string DesiredGroup { get; set; }
	}
}
