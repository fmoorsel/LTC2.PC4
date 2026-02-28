namespace LTC2.Desktopclients.WindowsClient.Forms
{
    partial class SelectRwGpsActivitiesForm
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
            grpSelectActivities = new GroupBox();
            chlSelectActivities = new CheckedListBox();
            btnOK = new Button();
            btnCancel = new Button();
            grpSelectActivities.SuspendLayout();
            SuspendLayout();
            //
            // grpSelectActivities
            //
            grpSelectActivities.Controls.Add(chlSelectActivities);
            grpSelectActivities.Location = new Point(30, 25);
            grpSelectActivities.Name = "grpSelectActivities";
            grpSelectActivities.Size = new Size(490, 262);
            grpSelectActivities.TabIndex = 0;
            grpSelectActivities.TabStop = false;
            grpSelectActivities.Text = "#groupbox.selectactivies";
            //
            // chlSelectActivities
            //
            chlSelectActivities.CheckOnClick = true;
            chlSelectActivities.FormattingEnabled = true;
            chlSelectActivities.Location = new Point(26, 34);
            chlSelectActivities.Name = "chlSelectActivities";
            chlSelectActivities.Size = new Size(434, 202);
            chlSelectActivities.TabIndex = 0;
            //
            // btnOK
            //
            btnOK.Location = new Point(132, 304);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(135, 36);
            btnOK.TabIndex = 1;
            btnOK.Text = "#button.selectactvities.ok";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            //
            // btnCancel
            //
            btnCancel.Location = new Point(273, 304);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(135, 36);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "#button.selectactvities.cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            //
            // SelectRwGpsActivitiesForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(544, 361);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(grpSelectActivities);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SelectRwGpsActivitiesForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "#form.selectactivities";
            Load += SelectRwGpsActivitiesForm_Load;
            grpSelectActivities.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpSelectActivities;
        private CheckedListBox chlSelectActivities;
        private Button btnOK;
        private Button btnCancel;
    }
}
