namespace LiveAudioPipelines.Forms
{
    partial class WindowMain
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
            button_newBag = new Button();
            button_import = new Button();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            preferencesToolStripMenuItem = new ToolStripMenuItem();
            exportFormatToolStripMenuItem = new ToolStripMenuItem();
            toolStripComboBox_exportFormat = new ToolStripComboBox();
            bitrateToolStripMenuItem = new ToolStripMenuItem();
            toolStripComboBox_bitrate = new ToolStripComboBox();
            exportPathToolStripMenuItem = new ToolStripMenuItem();
            toolStripTextBox_exportPath = new ToolStripTextBox();
            helpToolStripMenuItem = new ToolStripMenuItem();
            listBox_log = new ListBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // button_newBag
            // 
            button_newBag.Location = new Point(12, 106);
            button_newBag.Name = "button_newBag";
            button_newBag.Size = new Size(75, 23);
            button_newBag.TabIndex = 0;
            button_newBag.Text = "New Bag";
            button_newBag.UseVisualStyleBackColor = true;
            button_newBag.Click += button_newBag_Click;
            // 
            // button_import
            // 
            button_import.Location = new Point(12, 77);
            button_import.Name = "button_import";
            button_import.Size = new Size(75, 23);
            button_import.TabIndex = 1;
            button_import.Text = "Import";
            button_import.UseVisualStyleBackColor = true;
            button_import.Click += button_import_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, viewToolStripMenuItem, preferencesToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(464, 24);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(39, 20);
            editToolStripMenuItem.Text = "Edit";
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            // 
            // preferencesToolStripMenuItem
            // 
            preferencesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exportFormatToolStripMenuItem, exportPathToolStripMenuItem });
            preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            preferencesToolStripMenuItem.Size = new Size(80, 20);
            preferencesToolStripMenuItem.Text = "Preferences";
            // 
            // exportFormatToolStripMenuItem
            // 
            exportFormatToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripComboBox_exportFormat, bitrateToolStripMenuItem });
            exportFormatToolStripMenuItem.Name = "exportFormatToolStripMenuItem";
            exportFormatToolStripMenuItem.Size = new Size(157, 22);
            exportFormatToolStripMenuItem.Text = "Export Format...";
            // 
            // toolStripComboBox_exportFormat
            // 
            toolStripComboBox_exportFormat.Items.AddRange(new object[] { ".wav", ".mp3" });
            toolStripComboBox_exportFormat.Name = "toolStripComboBox_exportFormat";
            toolStripComboBox_exportFormat.Size = new Size(121, 23);
            toolStripComboBox_exportFormat.Text = ".wav";
            toolStripComboBox_exportFormat.SelectedIndexChanged += toolStripComboBox_exportFormat_SelectedIndexChanged;
            // 
            // bitrateToolStripMenuItem
            // 
            bitrateToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripComboBox_bitrate });
            bitrateToolStripMenuItem.Name = "bitrateToolStripMenuItem";
            bitrateToolStripMenuItem.Size = new Size(181, 22);
            bitrateToolStripMenuItem.Text = "Bitrate...";
            // 
            // toolStripComboBox_bitrate
            // 
            toolStripComboBox_bitrate.Name = "toolStripComboBox_bitrate";
            toolStripComboBox_bitrate.Size = new Size(121, 23);
            toolStripComboBox_bitrate.SelectedIndexChanged += toolStripComboBox_bitrate_SelectedIndexChanged;
            // 
            // exportPathToolStripMenuItem
            // 
            exportPathToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBox_exportPath });
            exportPathToolStripMenuItem.Name = "exportPathToolStripMenuItem";
            exportPathToolStripMenuItem.Size = new Size(157, 22);
            exportPathToolStripMenuItem.Text = "Export Path...";
            // 
            // toolStripTextBox_exportPath
            // 
            toolStripTextBox_exportPath.Name = "toolStripTextBox_exportPath";
            toolStripTextBox_exportPath.Size = new Size(160, 23);
            toolStripTextBox_exportPath.Text = "%MyMusic%\\LAPs\\";
            toolStripTextBox_exportPath.Leave += toolStripTextBox_exportPath_Leave;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // listBox_log
            // 
            listBox_log.Dock = DockStyle.Bottom;
            listBox_log.FormattingEnabled = true;
            listBox_log.HorizontalScrollbar = true;
            listBox_log.Location = new Point(0, 212);
            listBox_log.Name = "listBox_log";
            listBox_log.Size = new Size(464, 109);
            listBox_log.TabIndex = 3;
            // 
            // WindowMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(464, 321);
            Controls.Add(listBox_log);
            Controls.Add(button_import);
            Controls.Add(button_newBag);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "WindowMain";
            Text = "WindowMain";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button_newBag;
        private Button button_import;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem preferencesToolStripMenuItem;
        private ToolStripMenuItem exportPathToolStripMenuItem;
        private ToolStripTextBox toolStripTextBox_exportPath;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem exportFormatToolStripMenuItem;
        private ToolStripComboBox toolStripComboBox_exportFormat;
        private ToolStripMenuItem bitrateToolStripMenuItem;
        private ToolStripComboBox toolStripComboBox_bitrate;
        private ListBox listBox_log;
    }
}