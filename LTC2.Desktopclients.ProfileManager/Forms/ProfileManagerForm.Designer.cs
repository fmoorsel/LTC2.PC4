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
            tabControl = new TabControl();
            tabPageStrava = new TabPage();
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
            tabPageRideWithGps = new TabPage();
            grpProfielenRwg = new GroupBox();
            btnDeleteRwg = new Button();
            btnNewRwg = new Button();
            btnEditRwg = new Button();
            lstProfielenRwg = new ListBox();
            grpDetailsRwg = new GroupBox();
            lblErrorRwGpsId = new Label();
            btnShowSecretRwg = new Button();
            btnTestProfileRwg = new Button();
            txtRwGpsSecret = new TextBox();
            lblRwGpsSecret = new Label();
            txtRwGpsId = new TextBox();
            lblRwGpsId = new Label();
            txtProfileNameRwg = new TextBox();
            lblProfileNameRwg = new Label();
            tabControl.SuspendLayout();
            tabPageStrava.SuspendLayout();
            grpProfielen.SuspendLayout();
            grpDetails.SuspendLayout();
            tabPageRideWithGps.SuspendLayout();
            grpProfielenRwg.SuspendLayout();
            grpDetailsRwg.SuspendLayout();
            SuspendLayout();
            // 
            // tmrKeepAlive
            // 
            tmrKeepAlive.Enabled = true;
            tmrKeepAlive.Tick += tmrKeepAlive_Tick;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageStrava);
            tabControl.Controls.Add(tabPageRideWithGps);
            tabControl.Location = new Point(12, 12);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(791, 470);
            tabControl.TabIndex = 2;
            // 
            // tabPageStrava
            // 
            tabPageStrava.Controls.Add(grpProfielen);
            tabPageStrava.Controls.Add(grpDetails);
            tabPageStrava.Location = new Point(4, 24);
            tabPageStrava.Name = "tabPageStrava";
            tabPageStrava.Padding = new Padding(3);
            tabPageStrava.Size = new Size(783, 442);
            tabPageStrava.TabIndex = 0;
            tabPageStrava.Text = "Strava";
            tabPageStrava.UseVisualStyleBackColor = true;
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
            // tabPageRideWithGps
            // 
            tabPageRideWithGps.Controls.Add(grpProfielenRwg);
            tabPageRideWithGps.Controls.Add(grpDetailsRwg);
            tabPageRideWithGps.Location = new Point(4, 24);
            tabPageRideWithGps.Name = "tabPageRideWithGps";
            tabPageRideWithGps.Padding = new Padding(3);
            tabPageRideWithGps.Size = new Size(783, 442);
            tabPageRideWithGps.TabIndex = 1;
            tabPageRideWithGps.Text = "Ride with Gps";
            tabPageRideWithGps.UseVisualStyleBackColor = true;
            // 
            // grpProfielenRwg
            // 
            grpProfielenRwg.Controls.Add(btnDeleteRwg);
            grpProfielenRwg.Controls.Add(btnNewRwg);
            grpProfielenRwg.Controls.Add(btnEditRwg);
            grpProfielenRwg.Controls.Add(lstProfielenRwg);
            grpProfielenRwg.Location = new Point(23, 23);
            grpProfielenRwg.Name = "grpProfielenRwg";
            grpProfielenRwg.Size = new Size(745, 181);
            grpProfielenRwg.TabIndex = 0;
            grpProfielenRwg.TabStop = false;
            grpProfielenRwg.Text = "#groupbox.profiles";
            // 
            // btnDeleteRwg
            // 
            btnDeleteRwg.Enabled = false;
            btnDeleteRwg.Location = new Point(578, 114);
            btnDeleteRwg.Name = "btnDeleteRwg";
            btnDeleteRwg.Size = new Size(138, 35);
            btnDeleteRwg.TabIndex = 3;
            btnDeleteRwg.Text = "#button.delete.profile";
            btnDeleteRwg.UseVisualStyleBackColor = true;
            btnDeleteRwg.Click += btnDeleteRwg_Click;
            // 
            // btnNewRwg
            // 
            btnNewRwg.Location = new Point(578, 73);
            btnNewRwg.Name = "btnNewRwg";
            btnNewRwg.Size = new Size(138, 35);
            btnNewRwg.TabIndex = 2;
            btnNewRwg.Text = "#button.new.profile";
            btnNewRwg.UseVisualStyleBackColor = true;
            btnNewRwg.Click += btnNewRwg_Click;
            // 
            // btnEditRwg
            // 
            btnEditRwg.Enabled = false;
            btnEditRwg.Location = new Point(578, 32);
            btnEditRwg.Name = "btnEditRwg";
            btnEditRwg.Size = new Size(138, 35);
            btnEditRwg.TabIndex = 1;
            btnEditRwg.Text = "#button.edit.profile";
            btnEditRwg.UseVisualStyleBackColor = true;
            btnEditRwg.Click += btnEditRwg_Click;
            // 
            // lstProfielenRwg
            // 
            lstProfielenRwg.Enabled = false;
            lstProfielenRwg.FormattingEnabled = true;
            lstProfielenRwg.Location = new Point(24, 32);
            lstProfielenRwg.Name = "lstProfielenRwg";
            lstProfielenRwg.Size = new Size(526, 124);
            lstProfielenRwg.TabIndex = 0;
            lstProfielenRwg.DoubleClick += lstProfielenRwg_DoubleClick;
            // 
            // grpDetailsRwg
            // 
            grpDetailsRwg.Controls.Add(lblErrorRwGpsId);
            grpDetailsRwg.Controls.Add(btnShowSecretRwg);
            grpDetailsRwg.Controls.Add(btnTestProfileRwg);
            grpDetailsRwg.Controls.Add(txtRwGpsSecret);
            grpDetailsRwg.Controls.Add(lblRwGpsSecret);
            grpDetailsRwg.Controls.Add(txtRwGpsId);
            grpDetailsRwg.Controls.Add(lblRwGpsId);
            grpDetailsRwg.Controls.Add(txtProfileNameRwg);
            grpDetailsRwg.Controls.Add(lblProfileNameRwg);
            grpDetailsRwg.Enabled = false;
            grpDetailsRwg.Location = new Point(23, 221);
            grpDetailsRwg.Name = "grpDetailsRwg";
            grpDetailsRwg.Size = new Size(745, 199);
            grpDetailsRwg.TabIndex = 1;
            grpDetailsRwg.TabStop = false;
            grpDetailsRwg.Text = "#groupbox.profile.details";
            // 
            // lblErrorRwGpsId
            // 
            lblErrorRwGpsId.AutoSize = true;
            lblErrorRwGpsId.ForeColor = Color.Crimson;
            lblErrorRwGpsId.Location = new Point(154, 113);
            lblErrorRwGpsId.Name = "lblErrorRwGpsId";
            lblErrorRwGpsId.Size = new Size(115, 15);
            lblErrorRwGpsId.TabIndex = 8;
            lblErrorRwGpsId.Text = "#label.error.rwgps.id";
            lblErrorRwGpsId.Visible = false;
            // 
            // btnShowSecretRwg
            // 
            btnShowSecretRwg.Location = new Point(578, 133);
            btnShowSecretRwg.Name = "btnShowSecretRwg";
            btnShowSecretRwg.Size = new Size(138, 35);
            btnShowSecretRwg.TabIndex = 7;
            btnShowSecretRwg.Text = "#button.show.secret";
            btnShowSecretRwg.UseVisualStyleBackColor = true;
            btnShowSecretRwg.Click += btnShowSecretRwg_Click;
            // 
            // btnTestProfileRwg
            // 
            btnTestProfileRwg.Location = new Point(578, 33);
            btnTestProfileRwg.Name = "btnTestProfileRwg";
            btnTestProfileRwg.Size = new Size(138, 35);
            btnTestProfileRwg.TabIndex = 6;
            btnTestProfileRwg.Text = "#button.test.profiel";
            btnTestProfileRwg.UseVisualStyleBackColor = true;
            btnTestProfileRwg.Click += btnTestProfileRwg_Click;
            // 
            // txtRwGpsSecret
            // 
            txtRwGpsSecret.Location = new Point(154, 140);
            txtRwGpsSecret.Name = "txtRwGpsSecret";
            txtRwGpsSecret.PasswordChar = '*';
            txtRwGpsSecret.Size = new Size(396, 23);
            txtRwGpsSecret.TabIndex = 5;
            txtRwGpsSecret.TextChanged += txtRwGpsSecret_TextChanged;
            // 
            // lblRwGpsSecret
            // 
            lblRwGpsSecret.AutoSize = true;
            lblRwGpsSecret.Location = new Point(24, 143);
            lblRwGpsSecret.Name = "lblRwGpsSecret";
            lblRwGpsSecret.Size = new Size(140, 15);
            lblRwGpsSecret.TabIndex = 4;
            lblRwGpsSecret.Text = "#label.rwgps.client.secret";
            // 
            // txtRwGpsId
            // 
            txtRwGpsId.Location = new Point(154, 87);
            txtRwGpsId.Name = "txtRwGpsId";
            txtRwGpsId.Size = new Size(396, 23);
            txtRwGpsId.TabIndex = 3;
            txtRwGpsId.TextChanged += txtRwGpsId_TextChanged;
            // 
            // lblRwGpsId
            // 
            lblRwGpsId.AutoSize = true;
            lblRwGpsId.Location = new Point(24, 90);
            lblRwGpsId.Name = "lblRwGpsId";
            lblRwGpsId.Size = new Size(119, 15);
            lblRwGpsId.TabIndex = 2;
            lblRwGpsId.Text = "#label.rwgps.client.id";
            // 
            // txtProfileNameRwg
            // 
            txtProfileNameRwg.AcceptsTab = true;
            txtProfileNameRwg.Location = new Point(154, 33);
            txtProfileNameRwg.Name = "txtProfileNameRwg";
            txtProfileNameRwg.Size = new Size(396, 23);
            txtProfileNameRwg.TabIndex = 1;
            txtProfileNameRwg.TextChanged += txtProfileNameRwg_TextChanged;
            // 
            // lblProfileNameRwg
            // 
            lblProfileNameRwg.AutoSize = true;
            lblProfileNameRwg.Location = new Point(24, 36);
            lblProfileNameRwg.Name = "lblProfileNameRwg";
            lblProfileNameRwg.Size = new Size(109, 15);
            lblProfileNameRwg.TabIndex = 0;
            lblProfileNameRwg.Text = "#label.profile.name";
            // 
            // ProfileManagerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(815, 494);
            Controls.Add(tabControl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ProfileManagerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "#form.manage.profiles";
            Load += ProfileManagerForm_Load;
            tabControl.ResumeLayout(false);
            tabPageStrava.ResumeLayout(false);
            grpProfielen.ResumeLayout(false);
            grpDetails.ResumeLayout(false);
            grpDetails.PerformLayout();
            tabPageRideWithGps.ResumeLayout(false);
            grpProfielenRwg.ResumeLayout(false);
            grpDetailsRwg.ResumeLayout(false);
            grpDetailsRwg.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer tmrKeepAlive;
        private TabControl tabControl;
        private TabPage tabPageStrava;
        private TabPage tabPageRideWithGps;
        // Strava
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
        // RwGPS
        private GroupBox grpProfielenRwg;
        private ListBox lstProfielenRwg;
        private Button btnNewRwg;
        private Button btnEditRwg;
        private GroupBox grpDetailsRwg;
        private Label lblProfileNameRwg;
        private TextBox txtProfileNameRwg;
        private TextBox txtRwGpsId;
        private Label lblRwGpsId;
        private Button btnDeleteRwg;
        private Label lblRwGpsSecret;
        private TextBox txtRwGpsSecret;
        private Button btnTestProfileRwg;
        private Button btnShowSecretRwg;
        private Label lblErrorRwGpsId;
    }
}
