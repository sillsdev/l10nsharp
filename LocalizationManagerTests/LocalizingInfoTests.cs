using System.Windows.Forms;
using NUnit.Framework;

namespace Localization.Tests
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
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
			Form frm = new Form();
			frm.Name = "hamster";
			var loi = new LocalizingInfo(frm);
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
			Form frm = new Form();
			frm.Name = "racoon";

			var btn = new Button();
			btn.Name = "fox";
			frm.Controls.Add(btn);

			var loi = new LocalizingInfo(btn);
			Assert.AreEqual("racoon.fox", loi.Id);

			var lbl = new Label();
			lbl.Name = "opossum";
			var pnl1 = new Panel();
			var pnl2 = new Panel();
			pnl1.Controls.Add(pnl2);
			pnl2.Controls.Add(lbl);
			frm.Controls.Add(pnl1);
			loi = new LocalizingInfo(lbl);
			Assert.AreEqual("racoon.opossum", loi.Id);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test making the proper id for a list view's ColumnHeader object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeIdTest_ForColumnHeader()
		{
			var lv = new ListView();
			lv.Name = "fish";

			var hdr = new ColumnHeader();
			hdr.Name = "monkey";
			lv.Columns.Add(hdr);

			var loi = new LocalizingInfo(hdr);
			Assert.AreEqual("fish.Colmonkey", loi.Id);

			Form frm = new Form();
			frm.Name = "wolf";
			frm.Controls.Add(lv);
			loi = new LocalizingInfo(hdr);
			Assert.AreEqual("wolf.fishColmonkey", loi.Id);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Test making the proper id for a list view's ColumnHeader object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void MakeIdTest_ForDataGridViewColumn()
		{
			var grid = new DataGridView();
			grid.Name = "hippo";

			var col = new DataGridViewTextBoxColumn();
			col.Name = "cheetah";
			grid.Columns.Add(col);

			var loi = new LocalizingInfo(col);
			Assert.AreEqual("hippo.Colcheetah", loi.Id);

			Form frm = new Form();
			frm.Name = "jackal";
			frm.Controls.Add(grid);

			loi = new LocalizingInfo(col);
			Assert.AreEqual("jackal.hippoColcheetah", loi.Id);
		}
	}
}
