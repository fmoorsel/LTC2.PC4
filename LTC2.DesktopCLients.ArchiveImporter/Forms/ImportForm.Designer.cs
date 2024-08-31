namespace LTC2.DesktopClients.ArchiveImporter.Forms
{
    partial class ImportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportForm));
            btnStartImport = new Button();
            lblStatusImport = new Label();
            lblExplanation1 = new Label();
            lblExplanation2 = new Label();
            grpExplanation = new GroupBox();
            groupBox1 = new GroupBox();
            btnChooseFile = new Button();
            txtFile = new TextBox();
            grpImport = new GroupBox();
            grpFoundImports = new GroupBox();
            btnRemoveArchive = new Button();
            lstArchives = new ListBox();
            grpExplanation.SuspendLayout();
            groupBox1.SuspendLayout();
            grpImport.SuspendLayout();
            grpFoundImports.SuspendLayout();
            SuspendLayout();
            // 
            // btnStartImport
            // 
            btnStartImport.Enabled = false;
            btnStartImport.Location = new Point(18, 30);
            btnStartImport.Name = "btnStartImport";
            btnStartImport.Size = new Size(164, 47);
            btnStartImport.TabIndex = 1;
            btnStartImport.Text = "#button.import";
            btnStartImport.UseVisualStyleBackColor = true;
            btnStartImport.Click += btnImport_Click;
            // 
            // lblStatusImport
            // 
            lblStatusImport.AutoSize = true;
            lblStatusImport.Location = new Point(204, 46);
            lblStatusImport.Name = "lblStatusImport";
            lblStatusImport.Size = new Size(102, 15);
            lblStatusImport.TabIndex = 2;
            lblStatusImport.Text = "#status.choosefile";
            // 
            // lblExplanation1
            // 
            lblExplanation1.AutoSize = true;
            lblExplanation1.Location = new Point(18, 30);
            lblExplanation1.Name = "lblExplanation1";
            lblExplanation1.Size = new Size(113, 15);
            lblExplanation1.TabIndex = 3;
            lblExplanation1.Text = "#label.explanation.1";
            // 
            // lblExplanation2
            // 
            lblExplanation2.AutoSize = true;
            lblExplanation2.Location = new Point(18, 55);
            lblExplanation2.Name = "lblExplanation2";
            lblExplanation2.Size = new Size(113, 15);
            lblExplanation2.TabIndex = 4;
            lblExplanation2.Text = "#label.explanation.2";
            // 
            // grpExplanation
            // 
            grpExplanation.AutoSize = true;
            grpExplanation.Controls.Add(lblExplanation1);
            grpExplanation.Controls.Add(lblExplanation2);
            grpExplanation.Location = new Point(21, 12);
            grpExplanation.Name = "grpExplanation";
            grpExplanation.Size = new Size(754, 94);
            grpExplanation.TabIndex = 5;
            grpExplanation.TabStop = false;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnChooseFile);
            groupBox1.Controls.Add(txtFile);
            groupBox1.Location = new Point(21, 112);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(754, 79);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "#group.selectfile";
            // 
            // btnChooseFile
            // 
            btnChooseFile.Location = new Point(596, 29);
            btnChooseFile.Name = "btnChooseFile";
            btnChooseFile.Size = new Size(141, 30);
            btnChooseFile.TabIndex = 1;
            btnChooseFile.Text = "#button.chooseafile";
            btnChooseFile.UseVisualStyleBackColor = true;
            btnChooseFile.Click += btnChooseFile_Click;
            // 
            // txtFile
            // 
            txtFile.Enabled = false;
            txtFile.Location = new Point(18, 32);
            txtFile.Name = "txtFile";
            txtFile.Size = new Size(560, 23);
            txtFile.TabIndex = 0;
            txtFile.Text = "#text.chooseafile";
            // 
            // grpImport
            // 
            grpImport.Controls.Add(lblStatusImport);
            grpImport.Controls.Add(btnStartImport);
            grpImport.Location = new Point(21, 206);
            grpImport.Name = "grpImport";
            grpImport.Size = new Size(754, 97);
            grpImport.TabIndex = 7;
            grpImport.TabStop = false;
            grpImport.Text = "#group.import";
            // 
            // grpFoundImports
            // 
            grpFoundImports.Controls.Add(btnRemoveArchive);
            grpFoundImports.Controls.Add(lstArchives);
            grpFoundImports.Location = new Point(21, 319);
            grpFoundImports.Name = "grpFoundImports";
            grpFoundImports.Size = new Size(754, 149);
            grpFoundImports.TabIndex = 8;
            grpFoundImports.TabStop = false;
            grpFoundImports.Text = "#group.found.archives";
            // 
            // btnRemoveArchive
            // 
            btnRemoveArchive.Enabled = false;
            btnRemoveArchive.Location = new Point(596, 34);
            btnRemoveArchive.Name = "btnRemoveArchive";
            btnRemoveArchive.Size = new Size(141, 30);
            btnRemoveArchive.TabIndex = 1;
            btnRemoveArchive.Text = "#button.remove";
            btnRemoveArchive.UseVisualStyleBackColor = true;
            btnRemoveArchive.Click += btnRemoveArchive_Click;
            // 
            // lstArchives
            // 
            lstArchives.FormattingEnabled = true;
            lstArchives.ItemHeight = 15;
            lstArchives.Location = new Point(18, 34);
            lstArchives.Name = "lstArchives";
            lstArchives.Size = new Size(560, 94);
            lstArchives.TabIndex = 0;
            lstArchives.DoubleClick += btnRemoveArchive_Click;
            // 
            // ImportForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 488);
            Controls.Add(grpFoundImports);
            Controls.Add(grpImport);
            Controls.Add(groupBox1);
            Controls.Add(grpExplanation);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2, 1, 2, 1);
            MaximizeBox = false;
            Name = "ImportForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "#form.importer";
            FormClosing += ImportForm_FormClosing;
            Load += ImportForm_Load;
            grpExplanation.ResumeLayout(false);
            grpExplanation.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            grpImport.ResumeLayout(false);
            grpImport.PerformLayout();
            grpFoundImports.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnStartImport;
        private Label lblStatusImport;
        private Label lblExplanation1;
        private Label lblExplanation2;
        private GroupBox grpExplanation;
        private GroupBox groupBox1;
        private TextBox txtFile;
        private Button btnChooseFile;
        private GroupBox grpImport;
        private GroupBox grpFoundImports;
        private Button btnRemoveArchive;
        private ListBox lstArchives;
    }
}