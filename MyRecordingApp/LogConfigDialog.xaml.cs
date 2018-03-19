using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyRecordingApp
{
    /// <summary>
    /// Interaction logic for LogConfigDialog.xaml
    /// </summary>
    public partial class LogConfigDialog : Window
    {
        public delegate void LogConfigSelectedEventHandler(object sender,string[] logConfigGlobalParams);
        public event LogConfigSelectedEventHandler logConfigSelectedHandler;
        public LogConfigDialog()
        {
            InitializeComponent();
            loadSeatPositions();
            loadLayouts();
            loadInterfaces();
        }
        void loadSeatPositions()
        {
            string[] seatPositions = { "Front", "Side" };
            cb_seatPos.ItemsSource = seatPositions;
            cb_seatPos.SelectedIndex = 0;
        }
        void loadLayouts()
        {
            string[] layouts = { "3x3", "5x5" };
            cb_Layout.ItemsSource = layouts;
            cb_Layout.SelectedIndex = 0;
        }
        void loadInterfaces()
        {
            string[] interfaces = { "GazeNoGap", "GazeGap", "WideAngle" };
            cb_Interface.ItemsSource = interfaces;
            cb_Interface.SelectedIndex = 0;
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(txb_participantID.Text))
            {
                MessageBox.Show("Please enter participant ID");
            }
            else
            {
                string participantID = txb_participantID.Text;
                string selectedSeat = (string)cb_seatPos.SelectedItem;
                string selectedLayout = (string)cb_Layout.SelectedItem;
                string selectedInterface = (string)cb_Interface.SelectedItem;
                string[] globalLogParams = { participantID, selectedSeat, selectedLayout, selectedInterface };
                if(logConfigSelectedHandler != null)
                {
                    logConfigSelectedHandler(this, globalLogParams);
                }
                this.Close();
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
