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
        public event RoutedEventHandler MediaStoppedEventHandler;
        public delegate void ProgressUpdatedEventHandler(object sender, object[] arguments);
        Timer checkingStatusTimer = null;
        public event ProgressUpdatedEventHandler progressUpdateEventHandler;
        string currentVideo = String.Empty;
        bool _isPlaying = false;
        public bool IsPlaying
        {
            get { return _isPlaying; }
        }
        public MediaPlayer()
        {
            this.WindowState = WindowState.Normal;
            InitializeComponent();
            if (Properties.Settings.Default.ShowPlayerInPrimaryScreen)
            {
                this.Left = Screen.PrimaryScreen.Bounds.Left;
                this.Top = Screen.PrimaryScreen.Bounds.Top;
                this.Width = Screen.PrimaryScreen.Bounds.Width;
                this.Height = Screen.PrimaryScreen.Bounds.Height;
            }
            else
            {
                Screen secondaryScreen = getSecondaryScreen();
                this.Left = Screen.PrimaryScreen.Bounds.Right;
                this.Top = secondaryScreen.Bounds.Top;
                this.Width = secondaryScreen.Bounds.Width;
                this.Height = secondaryScreen.Bounds.Height;
            }
            this.Loaded += MediaPlayer_Loaded;
            videoPlayer.MediaEnded += VideoPlayer_MediaEnded;
        }
        private void MediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as Window).WindowState = WindowState.Maximized;
        }
        Screen getSecondaryScreen()
        {
            for(int i=0; i< Screen.AllScreens.Length; i++)
            {
                if(Screen.AllScreens[i].DeviceName.CompareTo(Screen.PrimaryScreen.DeviceName) != 0)
                {
                    return Screen.AllScreens[i];
                }
            }
            return null;
        }

        

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            /**/
            if (Properties.Settings.Default.PlayerAutoRepeat)
            {
                videoPlayer.Position = TimeSpan.FromMilliseconds(1);
                videoPlayer.Play();
            }
            else
            {
                checkingStatusTimer.Stop();
                checkingStatusTimer = null;
                _isPlaying = false;
                if (MediaStoppedEventHandler != null)
                {
                    MediaStoppedEventHandler(videoPlayer, null);
                }
            }
        }

        public void Play(string mediaPath)
        {
            videoPlayer.Source = null;
            videoPlayer.Source = new Uri(mediaPath,UriKind.Absolute);
            videoPlayer.Play();
            checkingStatusTimer = new Timer();
            checkingStatusTimer.Interval = 200;
            checkingStatusTimer.Enabled = true;
            checkingStatusTimer.Tick += CheckingStatusTimer_Tick;
            checkingStatusTimer.Start();
            currentVideo = mediaPath;
            _isPlaying = true;
        }
        public void Stop()
        {
            /*Action stopAction = delegate
            {
                checkingStatusTimer?.Stop();
                checkingStatusTimer = null;
                videoPlayer?.Stop();
            };
            videoPlayer?.Dispatcher.Invoke(stopAction);
            if (MediaStoppedEventHandler != null)
            {
                MediaStoppedEventHandler(videoPlayer, null);
            }*/
            _isPlaying = false;
            videoPlayer.Source = null;
        }
        private void CheckingStatusTimer_Tick(object sender, EventArgs e)
        {
            object[] progressUpdateParams = new object[2];
            progressUpdateParams[0] =  Utilities.DownScaleBitmap((System.Drawing.Bitmap)Utilities.takeScreenshot(this, (int)this.Width, (int)this.Height),8);
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
        public void FlipVideo(bool flip)
        {
            videoPlayer.Width = this.ActualWidth;
            videoPlayer.Height = this.ActualHeight;
            videoPlayer.Margin = new Thickness(0);
            if (flip)
            {
                videoPlayer.RenderTransform = new ScaleTransform(-1, 1, videoPlayer.ActualWidth / 2, videoPlayer.ActualHeight / 2);
            }
            else
            {
                videoPlayer.RenderTransform = new ScaleTransform(1, 1);
            }
        }
        
    }
}
