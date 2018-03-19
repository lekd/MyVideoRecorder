using Accord.Video.FFMPEG;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebSocketSharp.Server;

namespace MyRecordingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] DeviceTypes = {"Screen", "Camera" };
        string selectedDeviceType = "";
        int selectedDeviceIndex = -1;
        GenericCaptureDevice captureDevice = null;

        VideoFileWriter videoWriter = new VideoFileWriter();
        MediaPlayer playerWindow = null;

        TestLogging.Logger testLogger = null;
        WebSocketSharp.WebSocket networkListener;
        public MainWindow()
        {
            InitializeComponent();
            PositionWindowInStartupScreen();
            initDeviceTypes();
            if(Properties.Settings.Default.IsListeningToNetwork)
            {
                string serverURL = string.Format("ws://localhost:{0}/{1}", Properties.Settings.Default.WebSocketPort,Properties.Settings.Default.AnswerPage);
                networkListener = new WebSocketSharp.WebSocket(serverURL);
                networkListener.OnMessage += NetworkListener_OnMessage;
                networkListener.Connect();
            }
            EnableLogPanel(false);
        }
        #region Network events
        private void NetworkListener_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            if(e.IsText)
            {
                string msg = e.Data;
                try
                { 
                    var msgParams = JObject.Parse(msg);
                    string eventName = msgParams.Value<string>("event");
                    if(eventName.CompareTo(TestLogging.Logger.LOG_STOP)==0)
                    {
                        testLogger.setRecordStopTime(DateTime.Now.Subtract(TestLogging.Logger.BeginningOfToday()).TotalMilliseconds);
                        Action stopVideo = delegate
                        {
                            playerWindow?.Stop();
                        };
                        playerWindow?.Dispatcher.Invoke(stopVideo);
                    }
                    else if(eventName.CompareTo(TestLogging.Logger.LOG_TARGET_CHOOSE)==0)
                    {
                        testLogger.setRecordAnswer(msgParams.Value<string>("data"));
                        testLogger.setRecordAnswerTime(DateTime.Now.Subtract(TestLogging.Logger.BeginningOfToday()).TotalMilliseconds);
                    }
                    else if(eventName.CompareTo(TestLogging.Logger.LOG_CONFIDENCE_CHOOSE) == 0)
                    {
                        testLogger.setRecordConfidence(Convert.ToInt32(msgParams.Value<string>("data")));
                        testLogger.setConfidenceAnswerTime(DateTime.Now.Subtract(TestLogging.Logger.BeginningOfToday()).TotalMilliseconds);
                        testLogger.saveCurrentRecord();
                    }
                }
                catch(Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }


            }
        }
        #endregion
        #region UI control initialization
        void PositionWindowInStartupScreen()
        {
            Screen startupScreen = Screen.AllScreens[Properties.Settings.Default.StartupScreen];
            this.Left = startupScreen.Bounds.Left;
            this.Top = startupScreen.Bounds.Top;
            this.Width = startupScreen.WorkingArea.Width*2/3;
            this.Height = startupScreen.WorkingArea.Height*3/5;
            this.WindowState = WindowState.Maximized;
        }
        void initCapturePanelControls()
        {
            capturerControlPanelContainer.Width = grid_GeneralContainer.Width * grid_GeneralContainer.ColumnDefinitions[0].Width.Value;
            capturerControlPanelContainer.Height = grid_GeneralContainer.Height;
            img_Preview.Width = capturerControlPanelContainer.Width;
            img_Preview.Height = capturerControlPanelContainer.Height * capturerControlPanelContainer.RowDefinitions[1].Height.Value;
        }
        #endregion
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            captureDevice?.Close();
            captureDevice = null;
            playerWindow?.Close();
            testLogger?.Close();
            networkListener?.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grid_GeneralContainer.Width = this.Width;
            grid_GeneralContainer.Height = this.Height;
            initCapturePanelControls();
        }
        #region capture device init
        void initDeviceTypes()
        {
            cb_CaptureDevice.ItemsSource = DeviceTypes;
        }
        void initCapturingDevicesByType(string deviceType)
        {
            string[] capturingDevices = null;
            if(deviceType.CompareTo(DeviceTypes[0])==0)
            {
                int screenNums = Screen.AllScreens.Length;
                capturingDevices = new string[screenNums];
                for(int i=0; i< screenNums; i++)
                {
                    capturingDevices[i] = Screen.AllScreens[i].DeviceName;
                }
            }
            else if(deviceType.CompareTo(DeviceTypes[1]) == 0)
            {
                capturingDevices = CamRetriever.getCameraList();
            }
            else
            {
                capturingDevices = new string[0];
            }
            cb_DeviceID.ItemsSource = capturingDevices;
        }
        #endregion
        #region capture panel
        private void cb_CaptureDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedDeviceType = DeviceTypes[cb_CaptureDevice.SelectedIndex];
            initCapturingDevicesByType(DeviceTypes[cb_CaptureDevice.SelectedIndex]);
            
        }

        private void cb_DeviceID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedDeviceIndex = cb_DeviceID.SelectedIndex;
            captureDevice?.Close();
            if(selectedDeviceType.CompareTo(DeviceTypes[0]) == 0)
            {
                if (selectedDeviceIndex >= 0)
                {
                    captureDevice = new ScreenCapturer(selectedDeviceIndex);
                    captureDevice.NewFrameAvailableEvent += CaptureDevice_NewFrameAvailableEvent;
                    captureDevice.Start();
                }
            }
            else if(selectedDeviceType.CompareTo(DeviceTypes[1])==0)
            {
                if (selectedDeviceIndex >= 0)
                {
                    captureDevice = new CamRetriever(selectedDeviceIndex,1280,720);
                    captureDevice.NewFrameAvailableEvent += CaptureDevice_NewFrameAvailableEvent;
                    captureDevice.Start();
                }
            }
        }

        Bitmap currentFrame;
        private void CaptureDevice_NewFrameAvailableEvent(string deviceType, int deviceIndex, System.Drawing.Bitmap bmp)
        {
            Action displayFrame = delegate
            {
                if (chb_FlipFrame.IsChecked == true)
                {
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
                img_Preview.Source = Utilities.ToBitmapImage(bmp, ImageFormat.Jpeg);
                Bitmap buf = new Bitmap(bmp);
                
                if(isVideoRecording)
                {
                    videoWriter.WriteVideoFrame(buf);
                }
            };
            img_Preview.Dispatcher.Invoke(displayFrame);
            currentFrame = bmp;
        }
        #endregion
        #region player remote control panel
        string selectedVideoName = "";
        void loadMyVideoList()
        {
            DirectoryInfo d = new DirectoryInfo(Utilities.GetMyVideosDirectoryPath());
            FileInfo[] files = d.GetFiles("*.mp4");
            string[] fileNames = new string[files.Length];
            for(int i=0; i<files.Length; i++)
            {
                fileNames[i] = String.Format("{0}>>{1}",i+1, files[i].Name);
            }
            lb_VideosList.ItemsSource = fileNames;
        }
        void loadVideoPlaylist(string playlistFileName)
        {
            lb_VideosList.SelectionChanged -= lb_VideosList_SelectionChanged;
            FileStream fs = new FileStream(playlistFileName, FileMode.Open);
            StreamReader streamReader = new StreamReader(fs, Encoding.Default);
            string str = null;
            List<string> fileNames = new List<string>();
            while((str=streamReader.ReadLine())!=null)
            {
                fileNames.Add(str);
            }
            for(int i=0; i< fileNames.Count; i++)
            {
                fileNames[i] = String.Format("{0}>>{1}", i + 1, fileNames[i]);
            }
            lb_VideosList.ItemsSource = fileNames.ToArray();
            lb_VideosList.SelectionChanged += lb_VideosList_SelectionChanged;
        }
        private void lb_VideosList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] strSeparators = { ">>" };
            string selectedStr = (string)(lb_VideosList.SelectedItem);
            string[] strComponents = selectedStr.Split(strSeparators, StringSplitOptions.RemoveEmptyEntries);
            selectedVideoName = strComponents[1];
            if(!System.IO.Path.IsPathRooted(selectedVideoName))
            {
                selectedVideoName = Utilities.GetMyVideosDirectoryPath() + "//" + selectedVideoName;
            }
            tb_RecordFileName.Text = System.IO.Path.GetFileName(selectedVideoName);
            if(playerWindow != null)
            {
                btn_PlayVideo.IsEnabled = true;
            }
        }
        private void chb_FlipVideo_Checked(object sender, RoutedEventArgs e)
        {
            playerWindow?.FlipVideo(true);
        }

        private void chb_FlipVideo_Unchecked(object sender, RoutedEventArgs e)
        {
            playerWindow?.FlipVideo(false);
        }
        #region Log panel
        void EnableLogPanel(bool isEnabled)
        {
            if(!isEnabled)
            {
                chb_Log.IsChecked = false;
            }
            chb_Log.IsEnabled = isEnabled;
        }
        private void chb_Log_Checked(object sender, RoutedEventArgs e)
        {
            btn_LogConfig.IsEnabled = true;
            btn_SaveLog.IsEnabled = true;
        }

        private void chb_Log_Unchecked(object sender, RoutedEventArgs e)
        {
            btn_LogConfig.IsEnabled = false;
            btn_SaveLog.IsEnabled = false;
            testLogger = null;
        }
        private void btn_LogConfig_Click(object sender, RoutedEventArgs e)
        {
            LogConfigDialog logConfigDlg = new LogConfigDialog();
            logConfigDlg.logConfigSelectedHandler += LogConfigDlg_logConfigSelectedHandler;
            logConfigDlg.ShowDialog();
        }

        private void LogConfigDlg_logConfigSelectedHandler(object sender, string[] logConfigGlobalParams)
        {
            testLogger = new TestLogging.Logger();
            testLogger.setGlobalLogParams(logConfigGlobalParams);
            testLogger.recordSavedEventHandler += TestLogger_recordSavedEventHandler;
        }

        private void TestLogger_recordSavedEventHandler()
        {
            Action updateUIAfterRecordSaved = delegate
            {
                if (lb_VideosList.SelectedIndex < lb_VideosList.Items.Count - 1)
                {
                    lb_VideosList.SelectedIndex++;
                }
                else
                {
                    btn_PlayVideo.IsEnabled = true;
                    System.Windows.MessageBox.Show("The list is finished. Please save the log file");
                }
            };
            this.Dispatcher.Invoke(updateUIAfterRecordSaved);
        }

        private void btn_SaveLog_Click(object sender, RoutedEventArgs e)
        {
            testLogger?.Close();
        }
        #endregion
        #endregion
        #region Player Window Event Handler
        private void PlayerWindow_MediaStoppedEventHandler(object sender, RoutedEventArgs e)
        {
            setButtonTextColor(btn_PlayVideo, true);
            btn_PlayVideo.Content = "Play";
        }
        private void PlayerWindow_progressUpdateEventHandler(object sender, object[] arguments)
        {
            Bitmap playerScreenshot = (Bitmap)arguments[0];
            Action showPlayerScreen = delegate
            {
                //Bitmap scaledScreenshot = Utilities.DownScaleBitmap(playerScreenshot, 2);
                ImageSource temp = img_PlayerScreen.Source;
                img_PlayerScreen.Source = Utilities.ToBitmapImage(playerScreenshot, ImageFormat.Jpeg);
            };
            img_PlayerScreen.Dispatcher.Invoke(showPlayerScreen);
            double progressPercent = (Double)arguments[1];
            Bitmap progressBarBmp = Utilities.DrawProgressBar(progressPercent);
            Action showProgressBar = delegate
            {
                img_ProgressBar.Source = Utilities.ToBitmapImage(progressBarBmp, ImageFormat.Png);
            };
            img_ProgressBar.Dispatcher.Invoke(showProgressBar);
        }
        #endregion
        #region buttons events
        bool isVideoRecording = false;
        string getRecordingSubfolder()
        {
            if(!String.IsNullOrEmpty(txb_CaptureSubFolder.Text) && txb_CaptureSubFolder.Text.CompareTo("(Recording's sub-folder)")!=0)
            {
                return "//" + txb_CaptureSubFolder.Text;
            }
            return "";
        }
        private void btn_Record_Click(object sender, RoutedEventArgs e)
        {
            isVideoRecording = !isVideoRecording;
            if(isVideoRecording)
            {
                btn_Record.Content = "Stop";
                setButtonTextColor(btn_Record, false);
                string fileName = "";
                if(!String.IsNullOrEmpty(tb_RecordFileName.Text) && tb_RecordFileName.Text.CompareTo("(Recording's file name)")!= 0)
                {
                    fileName = tb_RecordFileName.Text;
                    if(!fileName.Contains(".mp4"))
                    {
                        fileName += ".mp4";
                    }
                }
                else
                {
                    fileName = Utilities.CreateFileNameFromCurrentMoment(".mp4");
                }
                if(!System.IO.Path.IsPathRooted(fileName))
                {
                    fileName = Utilities.GetCapturedVideoDirectoryPath() + getRecordingSubfolder() + "//" + fileName;
                    if(!Directory.Exists(Utilities.GetCapturedVideoDirectoryPath() + getRecordingSubfolder()))
                    {
                        Directory.CreateDirectory(Utilities.GetCapturedVideoDirectoryPath() + getRecordingSubfolder());
                    }
                }
                if (selectedDeviceType.CompareTo(DeviceTypes[0]) == 0)
                {
                    videoWriter.Open(fileName, currentFrame.Width, currentFrame.Height, 8, VideoCodec.MPEG4);
                }
                else if (selectedDeviceType.CompareTo(DeviceTypes[1]) == 0)
                {
                    videoWriter.Open(fileName, currentFrame.Width, currentFrame.Height, 10, VideoCodec.MPEG4);
                }
            }
            else
            {
                btn_Record.Content = "Record";
                videoWriter.Close();
                setButtonTextColor(btn_Record, true);
            }
        }
        private void btn_ShowPlayer_Click(object sender, RoutedEventArgs e)
        {
            if(playerWindow == null)
            {
                playerWindow = new MediaPlayer();
                //playerWindow.MediaEndedEventHandler += PlayerWindow_MediaEndedEventHandler;
                playerWindow.MediaStoppedEventHandler += PlayerWindow_MediaStoppedEventHandler;
                playerWindow.progressUpdateEventHandler += PlayerWindow_progressUpdateEventHandler;
                playerWindow.Show();
                btn_ShowPlayer.Content = "Close Player";
                btn_PlayVideo.IsEnabled = true;
                setButtonTextColor(btn_ShowPlayer, false);
                chb_FlipVideo.IsEnabled = true;
                EnableLogPanel(true);
            }
            else
            {
                playerWindow.MediaStoppedEventHandler -= PlayerWindow_MediaStoppedEventHandler;
                playerWindow.progressUpdateEventHandler -= PlayerWindow_progressUpdateEventHandler;
                playerWindow.Close();
                playerWindow = null;
                btn_ShowPlayer.Content = "Open Player";
                btn_PlayVideo.IsEnabled = false;
                setButtonTextColor(btn_ShowPlayer, true);
                chb_FlipVideo.IsChecked = false;
                chb_FlipVideo.IsEnabled = false;
                EnableLogPanel(false);
            }
        }

        private void btn_PlayVideo_Click(object sender, RoutedEventArgs e)
        {
            if (!playerWindow.IsPlaying)
            {
                if(chb_Log.IsChecked == true && testLogger == null)
                {
                    System.Windows.MessageBox.Show("Logger is not initialized yet");
                    return;
                }
                if (!String.IsNullOrEmpty(selectedVideoName))
                {
                    playerWindow?.Play(selectedVideoName);
                    btn_PlayVideo.IsEnabled = false;
                    //setButtonTextColor(btn_PlayVideo, false);
                    //btn_PlayVideo.Content = "Stop";
                    if (testLogger != null)
                    {
                        string selectedVidNameOnly = System.IO.Path.GetFileName(selectedVideoName);
                        string[] separators = { "_" };
                        string[] nameComponents = selectedVidNameOnly.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                        testLogger.startNewRecord(nameComponents[0]);
                        testLogger.setRecordStartTime(DateTime.Now.Subtract(TestLogging.Logger.BeginningOfToday()).TotalMilliseconds);
                    }
                }
            }
            else
            {
                
            }
        }
        private void btn_PlayListBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.pl|*.pl";
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txb_playList.Text = ofd.FileName;
                loadVideoPlaylist(ofd.FileName);
                testLogger = null;
            }
        }
        #endregion

        void setButtonTextColor(System.Windows.Controls.Button btn, bool isIdle)
        {
            if(isIdle)
            {
                btn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 255));
            }
            else
            {
                btn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
            }
        }

        
    }
}
