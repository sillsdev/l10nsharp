using System.Linq;
using Localization.CodeReader;
using NUnit.Framework;

namespace Localization.Tests
{
	[TestFixture]
	[LocalizationPackage("Foo")]
	public class LocalizationPackageAttributeTests
	{
		[Test]
		public void DisplayNameDefaultsToId()
		{
			var packageInfo = typeof(LocalizationPackageAttributeTests).GetCustomAttributes(typeof(LocalizationPackageAttribute), false).FirstOrDefault() as LocalizationPackageAttribute;
			Assert.NotNull(packageInfo);
			Assert.AreEqual("Foo", packageInfo.ID);
			Assert.AreEqual("Foo", packageInfo.GetDisplayName());
		}
		[Test]
		public void CanDetermineRootNamespace()
		{
			var packageInfo = typeof(LocalizationPackageAttributeTests).GetCustomAttributes(typeof(LocalizationPackageAttribute), false).FirstOrDefault() as LocalizationPackageAttribute;
			Assert.AreEqual("Localization", packageInfo.GetNameSpace(typeof(LocalizationPackageAttributeTests)));
		}
		[Test]
		public void CanDetermineVersion()
		{
			var packageInfo = typeof(LocalizationPackageAttributeTests).GetCustomAttributes(typeof(LocalizationPackageAttribute), false).FirstOrDefault() as LocalizationPackageAttribute;
			Assert.AreEqual("123", packageInfo.GetVersionName(typeof(LocalizationPackageAttributeTests)));
		}
	}
}
