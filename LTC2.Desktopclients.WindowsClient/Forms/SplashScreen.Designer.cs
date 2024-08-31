namespace LTC2.Desktopclients.WindowsClient
{
    partial class SplashScreen
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreen));
            btnStart = new Button();
            lblStatusCalculator = new Label();
            lblLabelRouteChecker = new Label();
            lblLabelWebApp = new Label();
            lblStatusWebApp = new Label();
            btnProfileManager = new Button();
            grpStatus = new GroupBox();
            grpAction = new GroupBox();
            grpStatus.SuspendLayout();
            grpAction.SuspendLayout();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Enabled = false;
            btnStart.Location = new Point(177, 77);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(252, 76);
            btnStart.TabIndex = 1;
            btnStart.Text = "#button.map.viewer";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // lblStatusCalculator
            // 
            lblStatusCalculator.AutoSize = true;
            lblStatusCalculator.Location = new Point(204, 37);
            lblStatusCalculator.Name = "lblStatusCalculator";
            lblStatusCalculator.Size = new Size(214, 15);
            lblStatusCalculator.TabIndex = 3;
            lblStatusCalculator.Text = "#label.status.route.checker.actualstatus";
            // 
            // lblLabelRouteChecker
            // 
            lblLabelRouteChecker.AutoSize = true;
            lblLabelRouteChecker.Location = new Point(24, 37);
            lblLabelRouteChecker.Name = "lblLabelRouteChecker";
            lblLabelRouteChecker.Size = new Size(148, 15);
            lblLabelRouteChecker.TabIndex = 4;
            lblLabelRouteChecker.Text = "#label.status.route.checker";
            // 
            // lblLabelWebApp
            // 
            lblLabelWebApp.AutoSize = true;
            lblLabelWebApp.Location = new Point(24, 73);
            lblLabelWebApp.Name = "lblLabelWebApp";
            lblLabelWebApp.Size = new Size(150, 15);
            lblLabelWebApp.TabIndex = 5;
            lblLabelWebApp.Text = "#label.status.map.manager";
            // 
            // lblStatusWebApp
            // 
            lblStatusWebApp.AutoSize = true;
            lblStatusWebApp.Location = new Point(204, 73);
            lblStatusWebApp.Name = "lblStatusWebApp";
            lblStatusWebApp.Size = new Size(212, 15);
            lblStatusWebApp.TabIndex = 6;
            lblStatusWebApp.Text = "#label.status.map.manage.actualstatus";
            // 
            // btnProfileManager
            // 
            btnProfileManager.Location = new Point(247, 31);
            btnProfileManager.Name = "btnProfileManager";
            btnProfileManager.Size = new Size(121, 29);
            btnProfileManager.TabIndex = 7;
            btnProfileManager.Text = "#button.profile.manager";
            btnProfileManager.UseVisualStyleBackColor = true;
            btnProfileManager.Click += btnProfileManager_Click;
            // 
            // grpStatus
            // 
            grpStatus.Controls.Add(lblLabelRouteChecker);
            grpStatus.Controls.Add(lblStatusCalculator);
            grpStatus.Controls.Add(lblStatusWebApp);
            grpStatus.Controls.Add(lblLabelWebApp);
            grpStatus.Location = new Point(24, 12);
            grpStatus.Name = "grpStatus";
            grpStatus.Size = new Size(611, 111);
            grpStatus.TabIndex = 8;
            grpStatus.TabStop = false;
            grpStatus.Text = "#groupbox.start";
            // 
            // grpAction
            // 
            grpAction.Controls.Add(btnProfileManager);
            grpAction.Controls.Add(btnStart);
            grpAction.Location = new Point(24, 144);
            grpAction.Name = "grpAction";
            grpAction.Size = new Size(611, 179);
            grpAction.TabIndex = 9;
            grpAction.TabStop = false;
            // 
            // SplashScreen
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(656, 346);
            Controls.Add(grpAction);
            Controls.Add(grpStatus);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2, 1, 2, 1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SplashScreen";
            StartPosition = FormStartPosition.CenterScreen;
            FormClosed += SplashScreen_FormClosed;
            Load += SplashScreen_Load;
            grpStatus.ResumeLayout(false);
            grpStatus.PerformLayout();
            grpAction.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button btnStart;
        private Label lblStatusCalculator;
        private Label lblLabelRouteChecker;
        private Label lblLabelWebApp;
        private Label lblStatusWebApp;
        private Button btnProfileManager;
        private GroupBox grpStatus;
        private GroupBox grpAction;
    }
}
