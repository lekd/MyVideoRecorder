using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRecordingApp
{
    public delegate void NewFrameAvailableEventHandler(string deviceType, int deviceIndex, Bitmap bmp);
    public abstract class GenericCaptureDevice
    {
        protected bool isRunning = false;
        public event NewFrameAvailableEventHandler NewFrameAvailableEvent;
        public bool IsStarted
        {
            get
            {
                return isRunning;
            }
            set
            {
                isRunning = value;
            }
        }
        abstract public void Start();
        abstract public void Close();
        public void OnNewFrameAvailable(string deviceType,int deviceIndex,Bitmap frame)
        {
            if(NewFrameAvailableEvent != null)
            {
                NewFrameAvailableEvent(deviceType, deviceIndex, frame);
            }
        }
    }
}
