namespace LiveAudioPipelines.Forms.Modules
{
    partial class AudioInfoPreview
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
            pictureBox_preview = new PictureBox();
            textBox_info = new TextBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox_preview).BeginInit();
            SuspendLayout();
            // 
            // pictureBox_preview
            // 
            pictureBox_preview.Dock = DockStyle.Left;
            pictureBox_preview.Location = new Point(0, 0);
            pictureBox_preview.Name = "pictureBox_preview";
            pictureBox_preview.Size = new Size(150, 150);
            pictureBox_preview.TabIndex = 0;
            pictureBox_preview.TabStop = false;
            pictureBox_preview.SizeMode = PictureBoxSizeMode.Zoom;
            // 
            // textBox_info
            // 
            textBox_info.Dock = DockStyle.Right;
            textBox_info.Font = new Font("Segoe UI Semilight", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_info.Location = new Point(156, 0);
            textBox_info.Multiline = true;
            textBox_info.Name = "textBox_info";
            textBox_info.Size = new Size(144, 150);
            textBox_info.TabIndex = 1;
            textBox_info.ReadOnly = true;
            textBox_info.WordWrap = false;
            // 
            // AudioInfoPreview
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(300, 150);
            Controls.Add(textBox_info);
            Controls.Add(pictureBox_preview);
            FormBorderStyle = FormBorderStyle.None;
            Name = "AudioInfoPreview";
            Text = "AudioInfoPreview";
            TopMost = true;
            ((System.ComponentModel.ISupportInitialize)pictureBox_preview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox_preview;
        private TextBox textBox_info;
    }
}