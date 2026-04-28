using System.Windows.Forms;

namespace LiveAudioPipelines.Forms.Controls
{
    public class BufferedPictureBox : PictureBox
    {
        public BufferedPictureBox()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (this.Image == null)
            {
                base.OnPaintBackground(pevent);
            }
        }
    }
}
