namespace LTC2.Desktopclients.WindowsClient.Forms
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            stsStrip = new StatusStrip();
            lblCalculatorStatus = new ToolStripStatusLabel();
            lblWebappStatus = new ToolStripStatusLabel();
            lblUpdateProgress = new ToolStripStatusLabel();
            pnlBar = new Panel();
            pnlButtons = new Panel();
            btnRefresh = new Button();
            pbxBrowsing = new PictureBox();
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            tmrKeepAlive = new System.Windows.Forms.Timer(components);
            tltButtons = new ToolTip(components);
            stsStrip.SuspendLayout();
            pnlBar.SuspendLayout();
            pnlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbxBrowsing).BeginInit();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            SuspendLayout();
            // 
            // stsStrip
            // 
            stsStrip.AutoSize = false;
            stsStrip.Items.AddRange(new ToolStripItem[] { lblCalculatorStatus, lblWebappStatus, lblUpdateProgress });
            stsStrip.Location = new Point(0, 881);
            stsStrip.Name = "stsStrip";
            stsStrip.Size = new Size(1884, 30);
            stsStrip.TabIndex = 1;
            // 
            // lblCalculatorStatus
            // 
            lblCalculatorStatus.Margin = new Padding(0, 3, 5, 2);
            lblCalculatorStatus.Name = "lblCalculatorStatus";
            lblCalculatorStatus.Size = new Size(0, 25);
            lblCalculatorStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblWebappStatus
            // 
            lblWebappStatus.Margin = new Padding(0, 3, 5, 2);
            lblWebappStatus.Name = "lblWebappStatus";
            lblWebappStatus.Size = new Size(0, 25);
            lblWebappStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUpdateProgress
            // 
            lblUpdateProgress.Name = "lblUpdateProgress";
            lblUpdateProgress.Overflow = ToolStripItemOverflow.Never;
            lblUpdateProgress.Size = new Size(0, 25);
            lblUpdateProgress.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlBar
            // 
            pnlBar.BorderStyle = BorderStyle.Fixed3D;
            pnlBar.Controls.Add(pnlButtons);
            pnlBar.Controls.Add(pbxBrowsing);
            pnlBar.Dock = DockStyle.Top;
            pnlBar.Location = new Point(0, 0);
            pnlBar.Name = "pnlBar";
            pnlBar.Size = new Size(1884, 66);
            pnlBar.TabIndex = 2;
            pnlBar.Resize += pnlBar_Resize;
            // 
            // pnlButtons
            // 
            pnlButtons.BackColor = SystemColors.Control;
            pnlButtons.Controls.Add(btnRefresh);
            pnlButtons.Location = new Point(683, 0);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(257, 58);
            pnlButtons.TabIndex = 1;
            // 
            // btnRefresh
            // 
            btnRefresh.BackgroundImage = Properties.Resources.refresh3;
            btnRefresh.BackgroundImageLayout = ImageLayout.Zoom;
            btnRefresh.Enabled = false;
            btnRefresh.ImageAlign = ContentAlignment.MiddleLeft;
            btnRefresh.Location = new Point(76, 6);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(108, 49);
            btnRefresh.TabIndex = 0;
            btnRefresh.Text = "#button.update";
            btnRefresh.TextAlign = ContentAlignment.MiddleRight;
            tltButtons.SetToolTip(btnRefresh, "Update activiteiten");
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // pbxBrowsing
            // 
            pbxBrowsing.BackgroundImageLayout = ImageLayout.Zoom;
            pbxBrowsing.Dock = DockStyle.Left;
            pbxBrowsing.Image = (Image)resources.GetObject("pbxBrowsing.Image");
            pbxBrowsing.Location = new Point(0, 0);
            pbxBrowsing.Name = "pbxBrowsing";
            pbxBrowsing.Size = new Size(108, 62);
            pbxBrowsing.SizeMode = PictureBoxSizeMode.Zoom;
            pbxBrowsing.TabIndex = 0;
            pbxBrowsing.TabStop = false;
            pbxBrowsing.Visible = false;
            // 
            // webView
            // 
            webView.AllowExternalDrop = true;
            webView.CreationProperties = null;
            webView.DefaultBackgroundColor = Color.White;
            webView.Dock = DockStyle.Fill;
            webView.Location = new Point(0, 66);
            webView.Name = "webView";
            webView.Size = new Size(1884, 815);
            webView.TabIndex = 3;
            webView.ZoomFactor = 1D;
            webView.NavigationStarting += webView_NavigationStarting;
            webView.NavigationCompleted += webView_NavigationCompleted;
            // 
            // tmrKeepAlive
            // 
            tmrKeepAlive.Interval = 1000;
            tmrKeepAlive.Tick += tmrKeepAlive_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1884, 911);
            Controls.Add(webView);
            Controls.Add(pnlBar);
            Controls.Add(stsStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1200, 380);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Long Term NL Postcode Challenge";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            stsStrip.ResumeLayout(false);
            stsStrip.PerformLayout();
            pnlBar.ResumeLayout(false);
            pnlButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbxBrowsing).EndInit();
            ((System.ComponentModel.ISupportInitialize)webView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private StatusStrip stsStrip;
        private Panel pnlBar;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private ToolStripStatusLabel lblCalculatorStatus;
        private ToolStripStatusLabel lblWebappStatus;
        private System.Windows.Forms.Timer tmrKeepAlive;
        private PictureBox pbxBrowsing;
        private Panel pnlButtons;
        private ToolTip tltButtons;
        private Button btnRefresh;
        private ToolStripStatusLabel lblUpdateProgress;
    }
}