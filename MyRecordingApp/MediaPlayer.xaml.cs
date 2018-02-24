using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyRecordingApp
{
    /// <summary>
    /// Interaction logic for MediaPlayer.xaml
    /// </summary>
    public partial class MediaPlayer : Window
    {
        public event RoutedEventHandler MediaEndedEventHandler;
        public delegate void ProgressUpdatedEventHandler(object sender, object[] arguments);
        Timer checkingStatusTimer = null;
        public event ProgressUpdatedEventHandler progressUpdateEventHandler;
        public MediaPlayer()
        {
            InitializeComponent();
            this.Left =  Screen.PrimaryScreen.Bounds.Left;
            this.Top = Screen.PrimaryScreen.Bounds.Top;
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.Height = Screen.PrimaryScreen.Bounds.Height;

            videoPlayer.MediaEnded += VideoPlayer_MediaEnded;
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            checkingStatusTimer.Stop();
            checkingStatusTimer = null;
            if (MediaEndedEventHandler != null)
            {
                MediaEndedEventHandler(sender, e);
            }
            
        }

        public void Play(string mediaPath)
        {
            videoPlayer.Source = new Uri(mediaPath,UriKind.Absolute);
            videoPlayer.Play();
            checkingStatusTimer = new Timer();
            checkingStatusTimer.Interval = 20;
            checkingStatusTimer.Enabled = true;
            checkingStatusTimer.Tick += CheckingStatusTimer_Tick;
            checkingStatusTimer.Start();
        }

        private void CheckingStatusTimer_Tick(object sender, EventArgs e)
        {
            object[] progressUpdateParams = new object[2];
            progressUpdateParams[0] = Utilities.takeScreenshot(this, (int)this.Width, (int)this.Height);
            if (videoPlayer.NaturalDuration.HasTimeSpan)
            {
                progressUpdateParams[1] = videoPlayer.Position.TotalMilliseconds * 1.0 / videoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            }
            else
            {
                progressUpdateParams[1] = (double)0;
            }
            if(progressUpdateEventHandler != null)
            {
                progressUpdateEventHandler(this, progressUpdateParams);
            }
        }
        
    }
}
