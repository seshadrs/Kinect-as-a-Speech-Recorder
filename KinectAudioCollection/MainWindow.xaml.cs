/* 
Author: Seshadri Sridharan, LTI, Carnegie Mellon Univ
<first 5 letters of first name> +'s' AT andrew.cmu.edu
Feb 2013
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        
        /// <summary>
        /// Execute uninitialization tasks.
        /// </summary>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            KinectAudioRecorder.UnInitializeKinectAudio();
        }

       


        public MainWindow()
        {
            InitializeComponent();

        }

        
        private void BTN_Click_BTN_StartOrStopRecording(object sender, RoutedEventArgs e)
        {
            if (KinectAudioRecorder.recording == false)
            {
                if (KinectAudioRecorder.recordingThread!=null)
                    while (KinectAudioRecorder.recordingThread.IsAlive)
                    { } //wait till the recording has actually stopped. will hang UI. But, should be matter of milliseconds at most
                BTN_StartOrStopRecording.Content = "STOP RECORDING";
                String OutputWAVFilePath = TB_OutputDirectory.Text + "\\" + Utterances.CurrentFileName();
                KinectAudioRecorder.StartRecordingAudio(@OutputWAVFilePath);
                BTN_StartOrStopRecording.Background = Brushes.OrangeRed;
            }
            else 
            {
                KinectAudioRecorder.StopRecordingAudio(); 
                BTN_StartOrStopRecording.Content = "START RECORDING";
                BTN_StartOrStopRecording.Background = Brushes.LightGreen;
                
            }
        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TB_Utterance.Text = "STEPS:\n1) Point to Utterances CSV file and click 'Load Utterances'.\n2)Point to Output Directory for the .wav recording files.";
            ///Initialize Kinect for audio, and then enable the START_RECORDING button
            if (KinectAudioRecorder.InitializeKinectAudio())
            {
                statusBarText.Content = "STATUS : Connected to Kinect.";

            }
            else
            {
                statusBarText.Content = "STATUS : Failed to connect to Kinect! Check whether Kinect sensor is powered and plugged in!";
            }
        }

        private void BTN_NEXT_UTT_Click(object sender, RoutedEventArgs e)
        {
            TB_Utterance.Text = Utterances.NextUtterance();
            if (!Utterances.hasNext())
                BTN_NEXT_UTT.IsEnabled = false;
            if (Utterances.hasPrevious())
                BTN_PREVIOUS_UTT.IsEnabled = true;

        }

        private void BTN_PREVIOUS_UTT_Click(object sender, RoutedEventArgs e)
        {
            TB_Utterance.Text = Utterances.PreviousUtterance();
            if (!Utterances.hasPrevious())
                BTN_PREVIOUS_UTT.IsEnabled = false;
            if (Utterances.hasNext())
                BTN_NEXT_UTT.IsEnabled = true;
        }

        private void BTN_Click_LOAD_UTTERANCES(object sender, RoutedEventArgs e)
        {
            if (Utterances.Read(TB_UtterancesFilePath.Text))
            {
                Console.WriteLine("Inside!");
                if (Utterances.IsNonEmpty())
                {

                    BTN_StartOrStopRecording.IsEnabled = true;
                    BTN_StartOrStopRecording.Background = Brushes.LightGreen;
                    TB_Utterance.Text = Utterances.CurrentUtterance();
                    if (Utterances.hasNext())
                    { BTN_NEXT_UTT.IsEnabled = true; }
                }
                else
                {
                    statusBarText.Content += "\nUtterances file is empty!";
                }
            }
            else
            {
                statusBarText.Content += "\nUtterances file could not be read! Check path";
            }
        }


    }
}
