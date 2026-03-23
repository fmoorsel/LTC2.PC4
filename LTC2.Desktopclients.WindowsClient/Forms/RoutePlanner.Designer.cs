namespace LTC2.Desktopclients.WindowsClient.Forms
{
    partial class RoutePlanner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RoutePlanner));
            pnlBar = new Panel();
            btnUnCheckRoute = new Button();
            btnCheckRoute = new Button();
            chkToggleVisibility = new CheckBox();
            pnlPlace = new Panel();
            lblCurrentPlace = new Label();
            pbxBrowsing = new PictureBox();
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            timer1 = new System.Windows.Forms.Timer(components);
            pnlBar.SuspendLayout();
            pnlPlace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbxBrowsing).BeginInit();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            SuspendLayout();
            // 
            // pnlBar
            // 
            pnlBar.BorderStyle = BorderStyle.Fixed3D;
            pnlBar.Controls.Add(btnUnCheckRoute);
            pnlBar.Controls.Add(btnCheckRoute);
            pnlBar.Controls.Add(chkToggleVisibility);
            pnlBar.Controls.Add(pnlPlace);
            pnlBar.Controls.Add(pbxBrowsing);
            pnlBar.Dock = DockStyle.Top;
            pnlBar.Location = new Point(0, 0);
            pnlBar.Name = "pnlBar";
            pnlBar.Size = new Size(1884, 66);
            pnlBar.TabIndex = 0;
            pnlBar.Resize += pnlBar_Resize;
            // 
            // btnUnCheckRoute
            // 
            btnUnCheckRoute.Enabled = false;
            btnUnCheckRoute.Location = new Point(1109, 10);
            btnUnCheckRoute.Name = "btnUnCheckRoute";
            btnUnCheckRoute.Size = new Size(95, 40);
            btnUnCheckRoute.TabIndex = 5;
            btnUnCheckRoute.Text = "#button.uncheckroute";
            btnUnCheckRoute.UseVisualStyleBackColor = true;
            btnUnCheckRoute.Visible = false;
            btnUnCheckRoute.Click += btnUnCheckRoute_Click;
            // 
            // btnCheckRoute
            // 
            btnCheckRoute.Location = new Point(971, 10);
            btnCheckRoute.Name = "btnCheckRoute";
            btnCheckRoute.Size = new Size(95, 40);
            btnCheckRoute.TabIndex = 4;
            btnCheckRoute.Text = "#button.checkroute";
            btnCheckRoute.UseVisualStyleBackColor = true;
            btnCheckRoute.Visible = false;
            btnCheckRoute.Click += btnCheckRoute_Click;
            // 
            // chkToggleVisibility
            // 
            chkToggleVisibility.AutoSize = true;
            chkToggleVisibility.Checked = true;
            chkToggleVisibility.CheckState = CheckState.Checked;
            chkToggleVisibility.Location = new Point(114, 21);
            chkToggleVisibility.Name = "chkToggleVisibility";
            chkToggleVisibility.Size = new Size(147, 19);
            chkToggleVisibility.TabIndex = 3;
            chkToggleVisibility.Text = "#check.toggle.visibility";
            chkToggleVisibility.UseVisualStyleBackColor = true;
            chkToggleVisibility.Visible = false;
            chkToggleVisibility.CheckedChanged += chkToggleVisibility_CheckedChanged;
            // 
            // pnlPlace
            // 
            pnlPlace.Controls.Add(lblCurrentPlace);
            pnlPlace.Location = new Point(424, 0);
            pnlPlace.Name = "pnlPlace";
            pnlPlace.Size = new Size(531, 62);
            pnlPlace.TabIndex = 2;
            // 
            // lblCurrentPlace
            // 
            lblCurrentPlace.AutoSize = true;
            lblCurrentPlace.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblCurrentPlace.Location = new Point(97, 16);
            lblCurrentPlace.Name = "lblCurrentPlace";
            lblCurrentPlace.Size = new Size(0, 30);
            lblCurrentPlace.TabIndex = 1;
            lblCurrentPlace.TextAlign = ContentAlignment.TopCenter;
            // 
            // pbxBrowsing
            // 
            pbxBrowsing.BackgroundImageLayout = ImageLayout.Zoom;
            pbxBrowsing.Dock = DockStyle.Left;
            pbxBrowsing.Image = (Image)resources.GetObject("pbxBrowsing.Image");
            pbxBrowsing.InitialImage = Properties.Resources.refresh2;
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
            webView.Size = new Size(1884, 845);
            webView.TabIndex = 1;
            webView.ZoomFactor = 1D;
            webView.NavigationStarting += webView_NavigationStarting;
            webView.NavigationCompleted += webView_NavigationCompleted;
            webView.WebMessageReceived += webView_WebMessageReceived;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 500;
            timer1.Tick += timer1_Tick;
            // 
            // RoutePlanner
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1884, 911);
            Controls.Add(webView);
            Controls.Add(pnlBar);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1200, 380);
            Name = "RoutePlanner";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Route Planner";
            FormClosing += RoutePlanner_FormClosing;
            Load += RoutePlanner_Load;
            Resize += RoutePlanner_Resize;
            pnlBar.ResumeLayout(false);
            pnlBar.PerformLayout();
            pnlPlace.ResumeLayout(false);
            pnlPlace.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbxBrowsing).EndInit();
            ((System.ComponentModel.ISupportInitialize)webView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlBar;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private PictureBox pbxBrowsing;
        private Label lblCurrentPlace;
        private Panel pnlPlace;
        private CheckBox chkToggleVisibility;
        private Button btnCheckRoute;
        private Button btnUnCheckRoute;
        private System.Windows.Forms.Timer timer1;
    }
}
