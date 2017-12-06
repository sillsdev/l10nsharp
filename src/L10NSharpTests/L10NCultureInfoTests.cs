using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using L10NSharp.XLiffUtils;
using L10NSharp.UI;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class L10NCultureInfoTests
	{
		[Test]
		public void L10NCultureInfo_TestEn()
		{
			var enci = L10NCultureInfo.GetCultureInfo("en");
			Assert.AreEqual("English", enci.EnglishName);
			Assert.IsNotNull(enci.RawCultureInfo);
		}

		[Test]
		public void L10NCultureInfo_TestPbu()
		{
			var pbuci = L10NCultureInfo.GetCultureInfo("pbu");
			Assert.AreEqual("Northern Pashto", pbuci.EnglishName);
			// Linux/Mono4 and Windows7 will have null for pbuci.RawCultureInfo.
			// Windows10 will produce a dummy object with no read information.
#if __MonoCS__
			Assert.IsNull(pbuci.RawCultureInfo);
#else
			if (pbuci.RawCultureInfo != null)
			{
				Assert.AreEqual("pbu", pbuci.RawCultureInfo.Name);
				Assert.AreEqual("Unknown Language (pbu)", pbuci.RawCultureInfo.EnglishName);
			}
#endif
		}

		[Test]
		public void L10NCultureInfo_TestList()
		{
			var list = L10NCultureInfo.GetCultures(CultureTypes.AllCultures).ToList();
			Assert.IsTrue(list.Contains(L10NCultureInfo.GetCultureInfo("en")));
			Assert.IsTrue(list.Contains(L10NCultureInfo.GetCultureInfo("pbu")));
			Assert.IsTrue(list.Contains(L10NCultureInfo.GetCultureInfo("prs")));
		}
	}
}
