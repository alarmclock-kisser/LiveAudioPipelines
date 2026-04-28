namespace LiveAudioPipelines.Forms.Modules
{
    partial class AudioView
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
            panel_controls = new Panel();
            panel_controls2 = new Panel();
            button_apply = new Button();
            label_volume = new Label();
            textBox_time = new TextBox();
            checkBox_mute = new CheckBox();
            vScrollBar_volume = new VScrollBar();
            button_loop = new Button();
            button_pause = new Button();
            button_play = new Button();
            hScrollBar_offset = new HScrollBar();
            pictureBox_view = new LiveAudioPipelines.Forms.Controls.BufferedPictureBox();
            contextMenuSelection = new ContextMenuStrip(components);
            toolStripMenuItem_copy = new ToolStripMenuItem();
            toolStripMenuItem_cut = new ToolStripMenuItem();
            toolStripMenuItem_silence = new ToolStripMenuItem();
            toolStripMenuItem_remove = new ToolStripMenuItem();
            toolStripMenuItem_normalize = new ToolStripMenuItem();
            toolStripMenuItem_fadeIn = new ToolStripMenuItem();
            toolStripMenuItem_fadeOut = new ToolStripMenuItem();
            panel_controls.SuspendLayout();
            panel_controls2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_view).BeginInit();
            contextMenuSelection.SuspendLayout();
            SuspendLayout();
            // 
            // panel_controls
            // 
            panel_controls.Controls.Add(panel_controls2);
            panel_controls.Controls.Add(vScrollBar_volume);
            panel_controls.Controls.Add(button_loop);
            panel_controls.Controls.Add(button_pause);
            panel_controls.Controls.Add(button_play);
            panel_controls.Dock = DockStyle.Left;
            panel_controls.Location = new Point(0, 0);
            panel_controls.Name = "panel_controls";
            panel_controls.Size = new Size(92, 141);
            panel_controls.TabIndex = 0;
            // 
            // panel_controls2
            // 
            panel_controls2.Controls.Add(button_apply);
            panel_controls2.Controls.Add(label_volume);
            panel_controls2.Controls.Add(textBox_time);
            panel_controls2.Controls.Add(checkBox_mute);
            panel_controls2.Dock = DockStyle.Bottom;
            panel_controls2.Location = new Point(0, 73);
            panel_controls2.Name = "panel_controls2";
            panel_controls2.Size = new Size(77, 68);
            panel_controls2.TabIndex = 5;
            // 
            // button_apply
            // 
            button_apply.Font = new Font("Segoe UI Semilight", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button_apply.Location = new Point(3, 44);
            button_apply.Margin = new Padding(3, 3, 0, 3);
            button_apply.Name = "button_apply";
            button_apply.Size = new Size(48, 21);
            button_apply.TabIndex = 11;
            button_apply.Text = "Apply";
            button_apply.UseVisualStyleBackColor = true;
            button_apply.Click += button_apply_Click;
            // 
            // label_volume
            // 
            label_volume.AutoSize = true;
            label_volume.Font = new Font("Segoe UI Semilight", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_volume.Location = new Point(51, 48);
            label_volume.Margin = new Padding(0);
            label_volume.Name = "label_volume";
            label_volume.Size = new Size(28, 13);
            label_volume.TabIndex = 10;
            label_volume.Text = "80%";
            // 
            // textBox_time
            // 
            textBox_time.Font = new Font("Bahnschrift SemiLight SemiConde", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_time.Location = new Point(3, 20);
            textBox_time.Margin = new Padding(3, 3, 3, 0);
            textBox_time.Name = "textBox_time";
            textBox_time.PlaceholderText = "0:00:00.000";
            textBox_time.Size = new Size(71, 21);
            textBox_time.TabIndex = 9;
            textBox_time.TextChanged += textBox_time_TextChanged;
            textBox_time.KeyDown += textBox_time_KeyDown;
            // 
            // checkBox_mute
            // 
            checkBox_mute.AutoSize = true;
            checkBox_mute.Font = new Font("Segoe UI Semilight", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBox_mute.Location = new Point(3, 0);
            checkBox_mute.Margin = new Padding(3, 0, 3, 0);
            checkBox_mute.Name = "checkBox_mute";
            checkBox_mute.Size = new Size(51, 17);
            checkBox_mute.TabIndex = 8;
            checkBox_mute.Text = "Mute";
            checkBox_mute.UseVisualStyleBackColor = true;
            checkBox_mute.CheckedChanged += checkBox_mute_CheckedChanged;
            // 
            // vScrollBar_volume
            // 
            vScrollBar_volume.Dock = DockStyle.Right;
            vScrollBar_volume.Location = new Point(77, 0);
            vScrollBar_volume.Maximum = 1000;
            vScrollBar_volume.Name = "vScrollBar_volume";
            vScrollBar_volume.ScaleScrollBarForDpiChange = false;
            vScrollBar_volume.Size = new Size(15, 141);
            vScrollBar_volume.TabIndex = 4;
            vScrollBar_volume.Value = 200;
            vScrollBar_volume.Scroll += vScrollBar_volume_Scroll;
            // 
            // button_loop
            // 
            button_loop.Font = new Font("Segoe UI Symbol", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button_loop.Location = new Point(53, 3);
            button_loop.Margin = new Padding(1, 3, 1, 3);
            button_loop.Name = "button_loop";
            button_loop.Size = new Size(23, 23);
            button_loop.TabIndex = 2;
            button_loop.Tag = "■";
            button_loop.Text = "↺";
            button_loop.UseVisualStyleBackColor = true;
            button_loop.Click += button_loop_Click;
            // 
            // button_pause
            // 
            button_pause.Font = new Font("Bahnschrift", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button_pause.Location = new Point(28, 3);
            button_pause.Margin = new Padding(1, 3, 1, 3);
            button_pause.Name = "button_pause";
            button_pause.Size = new Size(23, 23);
            button_pause.TabIndex = 1;
            button_pause.Tag = "■";
            button_pause.Text = "||";
            button_pause.UseVisualStyleBackColor = true;
            button_pause.Click += button_pause_Click;
            // 
            // button_play
            // 
            button_play.Location = new Point(3, 3);
            button_play.Margin = new Padding(3, 3, 1, 3);
            button_play.Name = "button_play";
            button_play.Size = new Size(23, 23);
            button_play.TabIndex = 0;
            button_play.Tag = "■";
            button_play.Text = "▶";
            button_play.UseVisualStyleBackColor = true;
            button_play.Click += button_play_Click;
            // 
            // hScrollBar_offset
            // 
            hScrollBar_offset.Dock = DockStyle.Bottom;
            hScrollBar_offset.Location = new Point(92, 129);
            hScrollBar_offset.Name = "hScrollBar_offset";
            hScrollBar_offset.Size = new Size(492, 12);
            hScrollBar_offset.TabIndex = 1;
            hScrollBar_offset.Scroll += hScrollBar_offset_Scroll;
            // 
            // pictureBox_view
            // 
            pictureBox_view.BackColor = Color.White;
            pictureBox_view.BorderStyle = BorderStyle.Fixed3D;
            pictureBox_view.Dock = DockStyle.Fill;
            pictureBox_view.Location = new Point(92, 0);
            pictureBox_view.Margin = new Padding(3, 3, 3, 0);
            pictureBox_view.Name = "pictureBox_view";
            pictureBox_view.Size = new Size(492, 129);
            pictureBox_view.TabIndex = 2;
            pictureBox_view.TabStop = false;
            pictureBox_view.Paint += pictureBox_view_Paint;
            pictureBox_view.MouseDown += pictureBox_view_MouseDown;
            pictureBox_view.MouseMove += pictureBox_view_MouseMove;
            pictureBox_view.MouseUp += pictureBox_view_MouseUp;
            // 
            // contextMenuSelection
            // 
            contextMenuSelection.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_copy, toolStripMenuItem_cut, toolStripMenuItem_silence, toolStripMenuItem_remove, toolStripMenuItem_normalize, toolStripMenuItem_fadeIn, toolStripMenuItem_fadeOut });
            contextMenuSelection.Name = "contextMenuSelection";
            contextMenuSelection.Size = new Size(129, 158);
            contextMenuSelection.Opening += contextMenuSelection_Opening;
            // 
            // toolStripMenuItem_copy
            // 
            toolStripMenuItem_copy.Name = "toolStripMenuItem_copy";
            toolStripMenuItem_copy.Size = new Size(128, 22);
            toolStripMenuItem_copy.Text = "Copy";
            toolStripMenuItem_copy.Click += toolStripMenuItem_copy_Click;
            // 
            // toolStripMenuItem_cut
            // 
            toolStripMenuItem_cut.Name = "toolStripMenuItem_cut";
            toolStripMenuItem_cut.Size = new Size(128, 22);
            toolStripMenuItem_cut.Text = "Cut";
            toolStripMenuItem_cut.Click += toolStripMenuItem_cut_Click;
            // 
            // toolStripMenuItem_silence
            // 
            toolStripMenuItem_silence.Name = "toolStripMenuItem_silence";
            toolStripMenuItem_silence.Size = new Size(128, 22);
            toolStripMenuItem_silence.Text = "Silence";
            toolStripMenuItem_silence.Click += toolStripMenuItem_silence_Click;
            // 
            // toolStripMenuItem_remove
            // 
            toolStripMenuItem_remove.Name = "toolStripMenuItem_remove";
            toolStripMenuItem_remove.Size = new Size(128, 22);
            toolStripMenuItem_remove.Text = "Remove";
            toolStripMenuItem_remove.Click += toolStripMenuItem_remove_Click;
            // 
            // toolStripMenuItem_normalize
            // 
            toolStripMenuItem_normalize.Name = "toolStripMenuItem_normalize";
            toolStripMenuItem_normalize.Size = new Size(128, 22);
            toolStripMenuItem_normalize.Text = "Normalize";
            toolStripMenuItem_normalize.Click += toolStripMenuItem_normalize_Click;
            // 
            // toolStripMenuItem_fadeIn
            // 
            toolStripMenuItem_fadeIn.Name = "toolStripMenuItem_fadeIn";
            toolStripMenuItem_fadeIn.Size = new Size(128, 22);
            toolStripMenuItem_fadeIn.Text = "Fade In";
            toolStripMenuItem_fadeIn.Click += toolStripMenuItem_fadeIn_Click;
            // 
            // toolStripMenuItem_fadeOut
            // 
            toolStripMenuItem_fadeOut.Name = "toolStripMenuItem_fadeOut";
            toolStripMenuItem_fadeOut.Size = new Size(128, 22);
            toolStripMenuItem_fadeOut.Text = "Fade Out";
            toolStripMenuItem_fadeOut.Click += toolStripMenuItem_fadeOut_Click;
            // 
            // AudioView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 141);
            Controls.Add(pictureBox_view);
            Controls.Add(hScrollBar_offset);
            Controls.Add(panel_controls);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AudioView";
            Text = "AudioView";
            panel_controls.ResumeLayout(false);
            panel_controls2.ResumeLayout(false);
            panel_controls2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_view).EndInit();
            contextMenuSelection.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel_controls;
        private HScrollBar hScrollBar_offset;
        private LiveAudioPipelines.Forms.Controls.BufferedPictureBox pictureBox_view;
        private Button button_loop;
        private Button button_pause;
        private Button button_play;
        private VScrollBar vScrollBar_volume;
        private Panel panel_controls2;
        private Button button_apply;
        private Label label_volume;
        private TextBox textBox_time;
        private CheckBox checkBox_mute;
        private ContextMenuStrip contextMenuSelection;
        private ToolStripMenuItem toolStripMenuItem_copy;
        private ToolStripMenuItem toolStripMenuItem_cut;
        private ToolStripMenuItem toolStripMenuItem_silence;
        private ToolStripMenuItem toolStripMenuItem_remove;
        private ToolStripMenuItem toolStripMenuItem_normalize;
        private ToolStripMenuItem toolStripMenuItem_fadeIn;
        private ToolStripMenuItem toolStripMenuItem_fadeOut;
    }
}