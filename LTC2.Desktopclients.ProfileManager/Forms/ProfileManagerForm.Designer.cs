namespace LTC2.Desktopclients.ProfileManager.Forms
{
    partial class ProfileManagerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfileManagerForm));
            tmrKeepAlive = new System.Windows.Forms.Timer(components);
            grpProfielen = new GroupBox();
            btnDelete = new Button();
            btnNew = new Button();
            btnEdit = new Button();
            lstProfielen = new ListBox();
            grpDetails = new GroupBox();
            lblErrorClientId = new Label();
            btnShowSecret = new Button();
            btnTestProfile = new Button();
            txtClientSecret = new TextBox();
            lblClientSecret = new Label();
            txtClientID = new TextBox();
            lblClientId = new Label();
            txtProfileName = new TextBox();
            lblProfileName = new Label();
            grpProfielen.SuspendLayout();
            grpDetails.SuspendLayout();
            SuspendLayout();
            // 
            // tmrKeepAlive
            // 
            tmrKeepAlive.Enabled = true;
            tmrKeepAlive.Tick += tmrKeepAlive_Tick;
            // 
            // grpProfielen
            // 
            grpProfielen.Controls.Add(btnDelete);
            grpProfielen.Controls.Add(btnNew);
            grpProfielen.Controls.Add(btnEdit);
            grpProfielen.Controls.Add(lstProfielen);
            grpProfielen.Location = new Point(23, 23);
            grpProfielen.Name = "grpProfielen";
            grpProfielen.Size = new Size(745, 181);
            grpProfielen.TabIndex = 0;
            grpProfielen.TabStop = false;
            grpProfielen.Text = "#groupbox.profiles";
            // 
            // btnDelete
            // 
            btnDelete.Enabled = false;
            btnDelete.Location = new Point(578, 114);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(138, 35);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "#button.delete.profile";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(578, 73);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(138, 35);
            btnNew.TabIndex = 2;
            btnNew.Text = "#button.new.profile";
            btnNew.UseVisualStyleBackColor = true;
            btnNew.Click += btnNew_Click;
            // 
            // btnEdit
            // 
            btnEdit.Enabled = false;
            btnEdit.Location = new Point(578, 32);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(138, 35);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "#button.edit.profile";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // lstProfielen
            // 
            lstProfielen.Enabled = false;
            lstProfielen.FormattingEnabled = true;
            lstProfielen.ItemHeight = 15;
            lstProfielen.Location = new Point(24, 32);
            lstProfielen.Name = "lstProfielen";
            lstProfielen.Size = new Size(526, 124);
            lstProfielen.TabIndex = 0;
            lstProfielen.DoubleClick += lstProfielen_DoubleClick;
            // 
            // grpDetails
            // 
            grpDetails.Controls.Add(lblErrorClientId);
            grpDetails.Controls.Add(btnShowSecret);
            grpDetails.Controls.Add(btnTestProfile);
            grpDetails.Controls.Add(txtClientSecret);
            grpDetails.Controls.Add(lblClientSecret);
            grpDetails.Controls.Add(txtClientID);
            grpDetails.Controls.Add(lblClientId);
            grpDetails.Controls.Add(txtProfileName);
            grpDetails.Controls.Add(lblProfileName);
            grpDetails.Enabled = false;
            grpDetails.Location = new Point(23, 221);
            grpDetails.Name = "grpDetails";
            grpDetails.Size = new Size(745, 199);
            grpDetails.TabIndex = 1;
            grpDetails.TabStop = false;
            grpDetails.Text = "#groupbox.profile.details";
            // 
            // lblErrorClientId
            // 
            lblErrorClientId.AutoSize = true;
            lblErrorClientId.ForeColor = Color.Crimson;
            lblErrorClientId.Location = new Point(154, 113);
            lblErrorClientId.Name = "lblErrorClientId";
            lblErrorClientId.Size = new Size(112, 15);
            lblErrorClientId.TabIndex = 8;
            lblErrorClientId.Text = "#label.error.client.id";
            // 
            // btnShowSecret
            // 
            btnShowSecret.Location = new Point(578, 133);
            btnShowSecret.Name = "btnShowSecret";
            btnShowSecret.Size = new Size(138, 35);
            btnShowSecret.TabIndex = 7;
            btnShowSecret.Text = "#button.show.secret";
            btnShowSecret.UseVisualStyleBackColor = true;
            btnShowSecret.Click += btnShowSecret_Click;
            // 
            // btnTestProfile
            // 
            btnTestProfile.Location = new Point(578, 33);
            btnTestProfile.Name = "btnTestProfile";
            btnTestProfile.Size = new Size(138, 35);
            btnTestProfile.TabIndex = 6;
            btnTestProfile.Text = "#button.test.profiel";
            btnTestProfile.UseVisualStyleBackColor = true;
            btnTestProfile.Click += btnTestProfile_Click;
            // 
            // txtClientSecret
            // 
            txtClientSecret.Location = new Point(154, 140);
            txtClientSecret.Name = "txtClientSecret";
            txtClientSecret.PasswordChar = '*';
            txtClientSecret.Size = new Size(396, 23);
            txtClientSecret.TabIndex = 5;
            txtClientSecret.TextChanged += txtClientSecret_TextChanged;
            // 
            // lblClientSecret
            // 
            lblClientSecret.AutoSize = true;
            lblClientSecret.Location = new Point(24, 143);
            lblClientSecret.Name = "lblClientSecret";
            lblClientSecret.Size = new Size(139, 15);
            lblClientSecret.TabIndex = 4;
            lblClientSecret.Text = "#label.strava.client.secret";
            // 
            // txtClientID
            // 
            txtClientID.Location = new Point(154, 87);
            txtClientID.Name = "txtClientID";
            txtClientID.Size = new Size(396, 23);
            txtClientID.TabIndex = 3;
            txtClientID.TextChanged += txtClientID_TextChanged;
            // 
            // lblClientId
            // 
            lblClientId.AutoSize = true;
            lblClientId.Location = new Point(24, 90);
            lblClientId.Name = "lblClientId";
            lblClientId.Size = new Size(118, 15);
            lblClientId.TabIndex = 2;
            lblClientId.Text = "#label.strava.client.id";
            // 
            // txtProfileName
            // 
            txtProfileName.AcceptsTab = true;
            txtProfileName.Location = new Point(154, 33);
            txtProfileName.Name = "txtProfileName";
            txtProfileName.Size = new Size(396, 23);
            txtProfileName.TabIndex = 1;
            txtProfileName.TextChanged += txtProfileName_TextChanged;
            // 
            // lblProfileName
            // 
            lblProfileName.AutoSize = true;
            lblProfileName.Location = new Point(24, 36);
            lblProfileName.Name = "lblProfileName";
            lblProfileName.Size = new Size(109, 15);
            lblProfileName.TabIndex = 0;
            lblProfileName.Text = "#label.profile.name";
            // 
            // ProfileManagerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 441);
            Controls.Add(grpDetails);
            Controls.Add(grpProfielen);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ProfileManagerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "#form.manage.profiles";
            Load += ProfileManagerForm_Load;
            grpProfielen.ResumeLayout(false);
            grpDetails.ResumeLayout(false);
            grpDetails.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer tmrKeepAlive;
        private GroupBox grpProfielen;
        private ListBox lstProfielen;
        private Button btnNew;
        private Button btnEdit;
        private GroupBox grpDetails;
        private Label lblProfileName;
        private TextBox txtProfileName;
        private TextBox txtClientID;
        private Label lblClientId;
        private Button btnDelete;
        private Label lblClientSecret;
        private TextBox txtClientSecret;
        private Button btnTestProfile;
        private Button btnShowSecret;
        private Label lblErrorClientId;
    }
}