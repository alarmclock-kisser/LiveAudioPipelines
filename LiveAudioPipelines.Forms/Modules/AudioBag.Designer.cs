namespace LiveAudioPipelines.Forms.Modules
{
    partial class AudioBag
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
            listBox_audios = new LiveAudioPipelines.Forms.Controls.BufferedListBox();
            checkBox_preview = new CheckBox();
            checkBox_autoPlay = new CheckBox();
            checkBox_lock = new CheckBox();
            panel_controls = new Panel();
            button_export = new Button();
            panel_controls.SuspendLayout();
            SuspendLayout();
            // 
            // listBox_audios
            // 
            listBox_audios.Dock = DockStyle.Fill;
            listBox_audios.FormattingEnabled = true;
            listBox_audios.IntegralHeight = false;
            listBox_audios.Location = new Point(0, 32);
            listBox_audios.Name = "listBox_audios";
            listBox_audios.Size = new Size(204, 124);
            listBox_audios.TabIndex = 0;
            listBox_audios.DoubleClick += listBox_audios_DoubleClick;
            // 
            // checkBox_preview
            // 
            checkBox_preview.AutoSize = true;
            checkBox_preview.Dock = DockStyle.Left;
            checkBox_preview.Font = new Font("Segoe UI Semilight", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBox_preview.Location = new Point(45, 0);
            checkBox_preview.Margin = new Padding(3, 0, 3, 0);
            checkBox_preview.Name = "checkBox_preview";
            checkBox_preview.Size = new Size(49, 32);
            checkBox_preview.TabIndex = 1;
            checkBox_preview.Text = "View";
            checkBox_preview.UseVisualStyleBackColor = true;
            // 
            // checkBox_autoPlay
            // 
            checkBox_autoPlay.AutoSize = true;
            checkBox_autoPlay.Dock = DockStyle.Left;
            checkBox_autoPlay.Font = new Font("Segoe UI Semilight", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBox_autoPlay.Location = new Point(0, 0);
            checkBox_autoPlay.Margin = new Padding(3, 0, 3, 0);
            checkBox_autoPlay.Name = "checkBox_autoPlay";
            checkBox_autoPlay.Size = new Size(45, 32);
            checkBox_autoPlay.TabIndex = 2;
            checkBox_autoPlay.Text = "Play";
            checkBox_autoPlay.UseVisualStyleBackColor = true;
            // 
            // checkBox_lock
            // 
            checkBox_lock.AutoSize = true;
            checkBox_lock.Dock = DockStyle.Right;
            checkBox_lock.Font = new Font("Segoe UI Semilight", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBox_lock.Location = new Point(166, 0);
            checkBox_lock.Margin = new Padding(3, 0, 3, 0);
            checkBox_lock.Name = "checkBox_lock";
            checkBox_lock.RightToLeft = RightToLeft.Yes;
            checkBox_lock.Size = new Size(38, 32);
            checkBox_lock.TabIndex = 3;
            checkBox_lock.Text = "🔒";
            checkBox_lock.UseVisualStyleBackColor = true;
            checkBox_lock.CheckedChanged += checkBox_lock_CheckedChanged;
            // 
            // panel_controls
            // 
            panel_controls.Controls.Add(button_export);
            panel_controls.Controls.Add(checkBox_preview);
            panel_controls.Controls.Add(checkBox_lock);
            panel_controls.Controls.Add(checkBox_autoPlay);
            panel_controls.Dock = DockStyle.Top;
            panel_controls.Location = new Point(0, 0);
            panel_controls.Name = "panel_controls";
            panel_controls.Size = new Size(204, 32);
            panel_controls.TabIndex = 4;
            // 
            // button_export
            // 
            button_export.Font = new Font("Segoe UI Semilight", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button_export.Location = new Point(100, 6);
            button_export.Name = "button_export";
            button_export.Size = new Size(60, 22);
            button_export.TabIndex = 4;
            button_export.Text = "Export";
            button_export.UseVisualStyleBackColor = true;
            button_export.Click += button_export_Click;
            // 
            // AudioBag
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(204, 156);
            Controls.Add(listBox_audios);
            Controls.Add(panel_controls);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "AudioBag";
            Text = "AudioBag";
            panel_controls.ResumeLayout(false);
            panel_controls.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private LiveAudioPipelines.Forms.Controls.BufferedListBox listBox_audios;
        private CheckBox checkBox_preview;
        private CheckBox checkBox_autoPlay;
        private CheckBox checkBox_lock;
        private Panel panel_controls;
        private Button button_export;
    }
}