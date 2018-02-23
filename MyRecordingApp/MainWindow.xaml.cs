

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

        //VideoFileWriter videoWriter = null;
        public MainWindow()
        {
            InitializeComponent();
            PositionWindowInStartupScreen();
            initDeviceTypes();
        }
        
        #region UI control initialization
        void PositionWindowInStartupScreen()
        {
            Screen startupScreen = Screen.AllScreens[Properties.Settings.Default.StartupScreen];
            this.Left = startupScreen.WorkingArea.Left;
            this.Top = startupScreen.WorkingArea.Top;
            this.Width = startupScreen.WorkingArea.Width;
            this.Height = startupScreen.WorkingArea.Height;
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
                    captureDevice = new CamRetriever(selectedDeviceIndex);
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
                img_Preview.Source = Utilities.ToBitmapImage(bmp, ImageFormat.Jpeg);
                if(isVideoRecording)
                {
                    //videoWriter.WriteVideoFrame(bmp);
                }
            };
            img_Preview.Dispatcher.Invoke(displayFrame);
            currentFrame = bmp;
        }
        #region buttons events
        bool isVideoRecording = false;
        private void btn_Record_Click(object sender, RoutedEventArgs e)
        {
            /*Action saveBitmap = delegate
            {
                currentFrame?.Save("E://test.jpg", ImageFormat.Jpeg);
            };
            btn_Record.Dispatcher.Invoke(saveBitmap);*/
            isVideoRecording = !isVideoRecording;
            if(isVideoRecording)
            {
                btn_Record.Content = "Stop";
                //videoWriter.Open("E://test.mp4", currentFrame.Width, currentFrame.Height, 25, VideoCodec.MPEG4);
            }
            else
            {
                btn_Record.Content = "Record";
                //videoWriter.Close();
            }
        }
        #endregion
    }
}
