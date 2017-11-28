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
			Assert.IsNull(pbuci.RawCultureInfo);
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
