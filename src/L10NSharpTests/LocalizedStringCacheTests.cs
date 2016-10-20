using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using L10NSharp.TMXUtils;
using L10NSharp.UI;
using NUnit.Framework;

namespace L10NSharp.Tests
{
	[TestFixture]
	public class LocalizedStringCacheTests
	{
		[Test]
		public void LoadGroupNodes_CopiesInstalledIfAvailable()
		{
			using (var folder = new TempFolder("CreateOrUpdate_CopiesInstalledIfAvailable"))
			{
				var manager = LocalizationManagerTests.SetupManager(folder);
				var cache = new LocalizedStringCache(manager);
				var node = new LocTreeNode(manager, manager.Name, null, manager.Name);
				cache.LoadGroupNodes(node.Nodes);
				Assert.AreEqual(2, node.Nodes.Count);
				Assert.AreEqual("blahId", node.Nodes[0].Text);
				Assert.AreEqual("theId", node.Nodes[1].Text);
			}
		}
	}
}