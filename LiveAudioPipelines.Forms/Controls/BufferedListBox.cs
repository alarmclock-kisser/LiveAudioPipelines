using System.Windows.Forms;

namespace LiveAudioPipelines.Forms.Controls
{
    internal class BufferedListBox : ListBox
    {
        public BufferedListBox()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();
        }

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
    }
}
