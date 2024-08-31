namespace LTC2.Desktopclients.ProfileManager.Forms
{
    partial class TesterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TesterForm));
            pnlBar = new Panel();
            pnlButtons = new Panel();
            btnLinkProfile = new Button();
            pbxBrowsing = new PictureBox();
            stsStrip = new StatusStrip();
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            tmrCheckTest = new System.Windows.Forms.Timer(components);
            pnlBar.SuspendLayout();
            pnlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbxBrowsing).BeginInit();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            SuspendLayout();
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
            pnlBar.TabIndex = 0;
            pnlBar.Resize += pnlBar_Resize;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnLinkProfile);
            pnlButtons.Location = new Point(683, 0);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(257, 58);
            pnlButtons.TabIndex = 1;
            // 
            // btnLinkProfile
            // 
            btnLinkProfile.Enabled = false;
            btnLinkProfile.Location = new Point(76, 6);
            btnLinkProfile.Name = "btnLinkProfile";
            btnLinkProfile.Size = new Size(108, 49);
            btnLinkProfile.TabIndex = 0;
            btnLinkProfile.Text = "Profiel koppelen";
            btnLinkProfile.UseVisualStyleBackColor = true;
            btnLinkProfile.Click += btnLinkProfile_Click;
            // 
            // pbxBrowsing
            // 
            pbxBrowsing.Dock = DockStyle.Left;
            pbxBrowsing.Image = LTC2.Desktopclients.ProfileManager.Properties.Resources.progress;
            pbxBrowsing.Location = new Point(0, 0);
            pbxBrowsing.Name = "pbxBrowsing";
            pbxBrowsing.Size = new Size(108, 62);
            pbxBrowsing.SizeMode = PictureBoxSizeMode.Zoom;
            pbxBrowsing.TabIndex = 0;
            pbxBrowsing.TabStop = false;
            pbxBrowsing.Visible = false;
            // 
            // stsStrip
            // 
            stsStrip.AutoSize = false;
            stsStrip.Location = new Point(0, 881);
            stsStrip.Name = "stsStrip";
            stsStrip.Size = new Size(1884, 30);
            stsStrip.TabIndex = 1;
            stsStrip.Text = "statusStrip1";
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
            webView.TabIndex = 2;
            webView.ZoomFactor = 1D;
            webView.NavigationStarting += webView_NavigationStarting;
            webView.NavigationCompleted += webView_NavigationCompleted;
            // 
            // tmrCheckTest
            // 
            tmrCheckTest.Enabled = true;
            tmrCheckTest.Tick += tmrCheckTest_Tick;
            // 
            // TesterForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1884, 911);
            Controls.Add(webView);
            Controls.Add(stsStrip);
            Controls.Add(pnlBar);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1200, 380);
            Name = "TesterForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Test je Strava API...";
            FormClosing += TesterForm_FormClosing;
            Load += TesterForm_Load;
            VisibleChanged += TesterForm_VisibleChanged;
            pnlBar.ResumeLayout(false);
            pnlButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbxBrowsing).EndInit();
            ((System.ComponentModel.ISupportInitialize)webView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlBar;
        private StatusStrip stsStrip;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private PictureBox pbxBrowsing;
        private Panel pnlButtons;
        private Button btnLinkProfile;
        private System.Windows.Forms.Timer tmrCheckTest;
    }
}