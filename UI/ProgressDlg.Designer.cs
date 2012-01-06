namespace Localization.UI
{
	partial class ProgressDlg
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this._labelMessage = new System.Windows.Forms.Label();
			this._progressBar = new System.Windows.Forms.ProgressBar();
			this._labelDetails = new System.Windows.Forms.Label();
			this._tableLayout.SuspendLayout();
			this.SuspendLayout();
			// 
			// _tableLayout
			// 
			this._tableLayout.ColumnCount = 1;
			this._tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayout.Controls.Add(this._labelMessage, 0, 0);
			this._tableLayout.Controls.Add(this._progressBar, 0, 1);
			this._tableLayout.Controls.Add(this._labelDetails, 0, 2);
			this._tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tableLayout.Location = new System.Drawing.Point(15, 20);
			this._tableLayout.Name = "_tableLayout";
			this._tableLayout.RowCount = 3;
			this._tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayout.Size = new System.Drawing.Size(232, 91);
			this._tableLayout.TabIndex = 1;
			// 
			// _labelMessage
			// 
			this._labelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._labelMessage.AutoSize = true;
			this._labelMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._labelMessage.Location = new System.Drawing.Point(0, 0);
			this._labelMessage.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this._labelMessage.Name = "_labelMessage";
			this._labelMessage.Size = new System.Drawing.Size(232, 15);
			this._labelMessage.TabIndex = 1;
			this._labelMessage.Text = "First Time Initialization...";
			// 
			// _progressBar
			// 
			this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._progressBar.Location = new System.Drawing.Point(0, 21);
			this._progressBar.Margin = new System.Windows.Forms.Padding(0);
			this._progressBar.Name = "_progressBar";
			this._progressBar.Size = new System.Drawing.Size(232, 18);
			this._progressBar.TabIndex = 2;
			// 
			// _labelDetails
			// 
			this._labelDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._labelDetails.AutoEllipsis = true;
			this._labelDetails.AutoSize = true;
			this._labelDetails.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._labelDetails.Location = new System.Drawing.Point(0, 49);
			this._labelDetails.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this._labelDetails.Name = "_labelDetails";
			this._labelDetails.Size = new System.Drawing.Size(232, 30);
			this._labelDetails.TabIndex = 3;
			this._labelDetails.Text = "This process takes place only once after the application is installed or updated." +
    "";
			// 
			// ProgressDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(262, 126);
			this.ControlBox = false;
			this.Controls.Add(this._tableLayout);
			this.Name = "ProgressDlg";
			this.Padding = new System.Windows.Forms.Padding(15, 20, 15, 15);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "#";
			this._tableLayout.ResumeLayout(false);
			this._tableLayout.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel _tableLayout;
		private System.Windows.Forms.Label _labelMessage;
		private System.Windows.Forms.ProgressBar _progressBar;
		private System.Windows.Forms.Label _labelDetails;
	}
}