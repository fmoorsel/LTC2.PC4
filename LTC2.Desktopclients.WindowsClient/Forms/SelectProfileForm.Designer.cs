namespace LTC2.Desktopclients.WindowsClient.Forms
{
    partial class SelectProfileForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectProfileForm));
            grpInstruction = new GroupBox();
            lblInstruction2 = new Label();
            btnManage = new Button();
            lblInstruction1 = new Label();
            grpSelectProfile = new GroupBox();
            btnSelectProfiles = new Button();
            lstProfiles = new ListBox();
            grpInstruction.SuspendLayout();
            grpSelectProfile.SuspendLayout();
            SuspendLayout();
            // 
            // grpInstruction
            // 
            grpInstruction.Controls.Add(lblInstruction2);
            grpInstruction.Controls.Add(btnManage);
            grpInstruction.Controls.Add(lblInstruction1);
            grpInstruction.Location = new Point(30, 31);
            grpInstruction.Name = "grpInstruction";
            grpInstruction.Size = new Size(745, 114);
            grpInstruction.TabIndex = 0;
            grpInstruction.TabStop = false;
            grpInstruction.Text = "#groupbox.profile.info";
            // 
            // lblInstruction2
            // 
            lblInstruction2.AutoSize = true;
            lblInstruction2.Location = new Point(38, 75);
            lblInstruction2.Name = "lblInstruction2";
            lblInstruction2.Size = new Size(38, 15);
            lblInstruction2.TabIndex = 3;
            lblInstruction2.Text = "label1";
            // 
            // btnManage
            // 
            btnManage.Location = new Point(586, 40);
            btnManage.Name = "btnManage";
            btnManage.Size = new Size(135, 36);
            btnManage.TabIndex = 2;
            btnManage.Text = "#button.profile.manager";
            btnManage.UseVisualStyleBackColor = true;
            btnManage.Visible = false;
            btnManage.Click += btnManage_Click;
            // 
            // lblInstruction1
            // 
            lblInstruction1.AutoSize = true;
            lblInstruction1.Location = new Point(38, 40);
            lblInstruction1.Name = "lblInstruction1";
            lblInstruction1.Size = new Size(38, 15);
            lblInstruction1.TabIndex = 1;
            lblInstruction1.Text = "label1";
            // 
            // grpSelectProfile
            // 
            grpSelectProfile.Controls.Add(btnSelectProfiles);
            grpSelectProfile.Controls.Add(lstProfiles);
            grpSelectProfile.Location = new Point(30, 177);
            grpSelectProfile.Name = "grpSelectProfile";
            grpSelectProfile.Size = new Size(745, 231);
            grpSelectProfile.TabIndex = 1;
            grpSelectProfile.TabStop = false;
            grpSelectProfile.Text = "Selecteer profiel";
            // 
            // btnSelectProfiles
            // 
            btnSelectProfiles.Location = new Point(586, 37);
            btnSelectProfiles.Name = "btnSelectProfiles";
            btnSelectProfiles.Size = new Size(135, 36);
            btnSelectProfiles.TabIndex = 1;
            btnSelectProfiles.Text = "#button.select.profile";
            btnSelectProfiles.UseVisualStyleBackColor = true;
            btnSelectProfiles.Click += btnSelectProfiles_Click;
            // 
            // lstProfiles
            // 
            lstProfiles.FormattingEnabled = true;
            lstProfiles.ItemHeight = 15;
            lstProfiles.Location = new Point(38, 37);
            lstProfiles.Name = "lstProfiles";
            lstProfiles.Size = new Size(517, 169);
            lstProfiles.TabIndex = 0;
            lstProfiles.DoubleClick += lstProfiles_DoubleClick;
            // 
            // SelectProfileForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(grpSelectProfile);
            Controls.Add(grpInstruction);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SelectProfileForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "#form.select.profile";
            TopMost = true;
            Load += SelectProfileForm_Load;
            grpInstruction.ResumeLayout(false);
            grpInstruction.PerformLayout();
            grpSelectProfile.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpInstruction;
        private GroupBox grpSelectProfile;
        private Label lblInstruction1;
        private Button btnSelectProfiles;
        private ListBox lstProfiles;
        private Button btnManage;
        private Label lblInstruction2;
    }
}