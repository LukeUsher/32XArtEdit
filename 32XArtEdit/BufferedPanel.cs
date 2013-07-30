using System;
using System.Windows.Forms;

namespace _32XArtEdit
{
    class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
        }
    }

}
