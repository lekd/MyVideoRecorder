using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyRecordingApp
{
    public class ScreenCapturer : GenericCaptureDevice
    {
        Rectangle screenBound = new Rectangle();
        Timer frameAquireScheduler;
        int deviceIndex = -1;
        public ScreenCapturer(int deviceID)
        {
            deviceIndex = deviceID;
            screenBound = Screen.AllScreens[deviceID].Bounds;
            
        }
        public override void Start()
        {
            frameAquireScheduler = new Timer();
            frameAquireScheduler.Interval = 40;
            frameAquireScheduler.Enabled = true;
            frameAquireScheduler.Tick += FrameAquireScheduler_Tick;
            frameAquireScheduler.Start();
        }

        private void FrameAquireScheduler_Tick(object sender, EventArgs e)
        {
            Bitmap curScreenshot = new Bitmap(screenBound.Width, screenBound.Height);
            using (var g = Graphics.FromImage(curScreenshot))
            {
                g.CopyFromScreen(new Point(screenBound.Left, screenBound.Top), Point.Empty, screenBound.Size);
            }
            OnNewFrameAvailable("Screen", deviceIndex, curScreenshot);
        }

        public override void Close()
        {
            frameAquireScheduler?.Stop();
            frameAquireScheduler = null;
        }

    }
}
