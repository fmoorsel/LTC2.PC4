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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RoutePlanner));
            pnlBar = new Panel();
            pnlPlace = new Panel();
            lblCurrentPlace = new Label();
            pbxBrowsing = new PictureBox();
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            pnlBar.SuspendLayout();
            pnlPlace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbxBrowsing).BeginInit();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            SuspendLayout();
            // 
            // pnlBar
            // 
            pnlBar.BorderStyle = BorderStyle.Fixed3D;
            pnlBar.Controls.Add(pnlPlace);
            pnlBar.Controls.Add(pbxBrowsing);
            pnlBar.Dock = DockStyle.Top;
            pnlBar.Location = new Point(0, 0);
            pnlBar.Name = "pnlBar";
            pnlBar.Size = new Size(1884, 66);
            pnlBar.TabIndex = 0;
            pnlBar.Resize += pnlBar_Resize;
            // 
            // pnlPlace
            // 
            pnlPlace.Controls.Add(lblCurrentPlace);
            pnlPlace.Location = new Point(545, 0);
            pnlPlace.Name = "pnlPlace";
            pnlPlace.Size = new Size(200, 62);
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
    }
}
