namespace LTC2.Desktopclients.WindowsClient.Forms
{
    partial class UpdateActivitiesForm
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
            grpStatus = new GroupBox();
            lblProgress = new Label();
            lblProgressRunningUpdate = new Label();
            lblStatusRunningUpdates = new Label();
            lblRunningUpdates = new Label();
            grpStartUpdate = new GroupBox();
            chkByPassCache = new CheckBox();
            rdoRefresh = new RadioButton();
            rdoUpdate = new RadioButton();
            btnStartUpdate = new Button();
            grpStatus.SuspendLayout();
            grpStartUpdate.SuspendLayout();
            SuspendLayout();
            // 
            // grpStatus
            // 
            grpStatus.BackColor = SystemColors.Control;
            grpStatus.Controls.Add(lblProgress);
            grpStatus.Controls.Add(lblProgressRunningUpdate);
            grpStatus.Controls.Add(lblStatusRunningUpdates);
            grpStatus.Controls.Add(lblRunningUpdates);
            grpStatus.ForeColor = SystemColors.ControlText;
            grpStatus.Location = new Point(28, 22);
            grpStatus.Name = "grpStatus";
            grpStatus.Size = new Size(743, 95);
            grpStatus.TabIndex = 1;
            grpStatus.TabStop = false;
            grpStatus.Text = "#groupbox.status";
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(318, 59);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(22, 15);
            lblProgress.TabIndex = 3;
            lblProgress.Text = "---";
            // 
            // lblProgressRunningUpdate
            // 
            lblProgressRunningUpdate.AutoSize = true;
            lblProgressRunningUpdate.Location = new Point(129, 59);
            lblProgressRunningUpdate.Name = "lblProgressRunningUpdate";
            lblProgressRunningUpdate.Size = new Size(127, 15);
            lblProgressRunningUpdate.TabIndex = 2;
            lblProgressRunningUpdate.Text = "#label.progress.update";
            // 
            // lblStatusRunningUpdates
            // 
            lblStatusRunningUpdates.AutoSize = true;
            lblStatusRunningUpdates.Location = new Point(318, 30);
            lblStatusRunningUpdates.Name = "lblStatusRunningUpdates";
            lblStatusRunningUpdates.Size = new Size(96, 15);
            lblStatusRunningUpdates.TabIndex = 1;
            lblStatusRunningUpdates.Text = "#label.no.update";
            // 
            // lblRunningUpdates
            // 
            lblRunningUpdates.AutoSize = true;
            lblRunningUpdates.Location = new Point(129, 30);
            lblRunningUpdates.Name = "lblRunningUpdates";
            lblRunningUpdates.Size = new Size(124, 15);
            lblRunningUpdates.TabIndex = 0;
            lblRunningUpdates.Text = "#label.running.update";
            // 
            // grpStartUpdate
            // 
            grpStartUpdate.Controls.Add(chkByPassCache);
            grpStartUpdate.Controls.Add(rdoRefresh);
            grpStartUpdate.Controls.Add(rdoUpdate);
            grpStartUpdate.Controls.Add(btnStartUpdate);
            grpStartUpdate.Location = new Point(28, 142);
            grpStartUpdate.Name = "grpStartUpdate";
            grpStartUpdate.Size = new Size(743, 257);
            grpStartUpdate.TabIndex = 2;
            grpStartUpdate.TabStop = false;
            grpStartUpdate.Text = "#groupbox.new.update";
            // 
            // chkByPassCache
            // 
            chkByPassCache.AutoSize = true;
            chkByPassCache.Enabled = false;
            chkByPassCache.Location = new Point(92, 122);
            chkByPassCache.Name = "chkByPassCache";
            chkByPassCache.Size = new Size(156, 19);
            chkByPassCache.TabIndex = 3;
            chkByPassCache.Text = "#checkbox.renew.details";
            chkByPassCache.UseVisualStyleBackColor = true;
            // 
            // rdoRefresh
            // 
            rdoRefresh.AutoSize = true;
            rdoRefresh.Location = new Point(65, 87);
            rdoRefresh.Name = "rdoRefresh";
            rdoRefresh.Size = new Size(138, 19);
            rdoRefresh.TabIndex = 2;
            rdoRefresh.Text = "#radio.calc.all.update";
            rdoRefresh.UseVisualStyleBackColor = true;
            rdoRefresh.CheckedChanged += rdoRefresh_CheckedChanged;
            // 
            // rdoUpdate
            // 
            rdoUpdate.AutoSize = true;
            rdoUpdate.Checked = true;
            rdoUpdate.Location = new Point(65, 48);
            rdoUpdate.Name = "rdoUpdate";
            rdoUpdate.Size = new Size(140, 19);
            rdoUpdate.TabIndex = 1;
            rdoUpdate.TabStop = true;
            rdoUpdate.Text = "#radio.normal.update";
            rdoUpdate.UseVisualStyleBackColor = true;
            // 
            // btnStartUpdate
            // 
            btnStartUpdate.Location = new Point(65, 166);
            btnStartUpdate.Name = "btnStartUpdate";
            btnStartUpdate.Size = new Size(615, 54);
            btnStartUpdate.TabIndex = 0;
            btnStartUpdate.Text = "#button.start.update";
            btnStartUpdate.UseVisualStyleBackColor = true;
            btnStartUpdate.Click += btnStartUpdate_Click;
            // 
            // UpdateActivitiesForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 432);
            Controls.Add(grpStartUpdate);
            Controls.Add(grpStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UpdateActivitiesForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "#form.update";
            FormClosed += UpdateActivitiesForm_FormClosed;
            Load += UpdateActivitiesForm_Load;
            VisibleChanged += UpdateActivitiesForm_VisibleChanged;
            grpStatus.ResumeLayout(false);
            grpStatus.PerformLayout();
            grpStartUpdate.ResumeLayout(false);
            grpStartUpdate.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private GroupBox grpStatus;
        private Label lblRunningUpdates;
        private Label lblProgress;
        private Label lblProgressRunningUpdate;
        private Label lblStatusRunningUpdates;
        private GroupBox grpStartUpdate;
        private Button btnStartUpdate;
        private RadioButton rdoRefresh;
        private RadioButton rdoUpdate;
        private CheckBox chkByPassCache;
    }
}