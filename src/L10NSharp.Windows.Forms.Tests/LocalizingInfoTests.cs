using System.Windows.Forms;
using NUnit.Framework;
using L10NSharp;
using L10NSharp.Windows.Forms;

namespace L10NSharp.Windows.Forms.Tests
{
	[TestFixture]
	[Category("RequiresDisplay")]
	public class LocalizingInfoTests
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Makes the proper making of an id for a form object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeIdTest_ForForm()
		{
			Form frm = new Form { Name = "hamster" };
			var loi = new LocalizingInfoWinforms(frm, true);
			Assert.AreEqual("hamster.WindowTitle", loi.Id);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test making the proper id for a control object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeIdTest_ForControl()
		{
			var frm = new Form { Name = "racoon" };

			var btn = new Button { Name = "fox" };
			frm.Controls.Add(btn);

			var loi = new LocalizingInfoWinforms(btn, true);
			Assert.AreEqual("racoon.fox", loi.Id);

			var lbl = new Label { Name = "opossum" };
			var pnl1 = new Panel();
			var pnl2 = new Panel();
			pnl1.Controls.Add(pnl2);
			pnl2.Controls.Add(lbl);
			frm.Controls.Add(pnl1);
			loi = new LocalizingInfoWinforms(lbl, true);
			Assert.AreEqual("racoon.opossum", loi.Id);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test making the proper id for a list view's ColumnHeader object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		[Ignore("I don't see code around that would make this work(e.g. insert a 'Col'), maybe David left it as a todo?")]
		public void MakeIdTest_ForColumnHeader()
		{
			var lv = new ListView { Name = "fish" };

			var hdr = new ColumnHeader { Name = "monkey" };
			lv.Columns.Add(hdr);

			var loi = new LocalizingInfoWinforms(hdr, true);
			Assert.AreEqual("fish.Colmonkey", loi.Id);

			var frm = new Form { Name = "wolf" };
			frm.Controls.Add(lv);
			loi = new LocalizingInfoWinforms(hdr, true);
			Assert.AreEqual("wolf.fishColmonkey", loi.Id);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test making the proper id for a list view's ColumnHeader object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		[Ignore("I don't see code around that would make this work(e.g. insert a 'Col'), maybe David left it as a todo?")]
		public void MakeIdTest_ForDataGridViewColumn()
		{
			var grid = new DataGridView { Name = "hippo" };

			var col = new DataGridViewTextBoxColumn { Name = "cheetah" };
			grid.Columns.Add(col);

			var loi = new LocalizingInfoWinforms(col, true);
			Assert.AreEqual("hippo.Colcheetah", loi.Id);

			var frm = new Form { Name = "jackal" };
			frm.Controls.Add(grid);

			loi = new LocalizingInfoWinforms(col, true);
			Assert.AreEqual("jackal.hippoColcheetah", loi.Id);
		}
	}
}
