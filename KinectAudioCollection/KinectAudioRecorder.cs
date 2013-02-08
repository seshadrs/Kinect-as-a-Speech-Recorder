/* 
Author: Seshadri Sridharan, LTI, Carnegie Mellon Univ
<first 5 letters of first name> +'s' AT andrew.cmu.edu
Feb 2013
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace WpfApplication1
{
    static class KinectAudioRecorder
    {

        private static Boolean SUCCESS = true;
        private static Boolean FAILURE = false;

        /// <summary>
        /// Number of milliseconds between each read of audio data from the stream.
        /// </summary>
        private const int AudioPollingInterval = 50;

        /// <summary>
        /// Number of samples captured from Kinect audio stream each millisecond.
        /// </summary>
        private const int SamplesPerMillisecond = 16;

        /// <summary>
        /// Number of bytes in each Kinect audio stream sample.
        /// </summary>
        private const int BytesPerSample = 2;

        /// <summary>
        /// Buffer used to hold audio data read from audio stream.
        /// </summary>
        private static readonly byte[] audioBuffer = new byte[AudioPollingInterval * SamplesPerMillisecond * BytesPerSample];

        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        private static KinectSensor sensor;

        /// <summary>
        /// Stream of audio being captured by Kinect sensor.
        /// </summary>
        private static System.IO.Stream audioStream;

        /// <summary>
        /// <code>true</code> if audio is currently being read from Kinect stream, <code>false</code> otherwise.
        /// Since the application needs to be aware of the recording status, it is public
        /// </summary>
        public static bool recording;

        /// <summary>
        /// Thread that is reading audio from Kinect stream.
        /// Application may need to know if thread is still alive, if its gone rogue etc. Thus public
        /// </summary>
        public static System.Threading.Thread recordingThread = null;

        /// <summary>
        /// recording file stream object
        /// </summary>
        private static System.IO.FileStream recordingFileStream = null;

        /// <summary>
        /// total number of samples recorded
        /// </summary>
        private static int recordedSamplesCount = 0;

        /// <summary>
        /// File name for the recording
        /// </summary>
        private static string recordingFileFullName;

        struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }


        private static void WriteString(System.IO.Stream stream, string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }


        /// <summary>
        /// A bare bones WAV file header writer
        /// </summary>        
        private static void WriteWavHeader(System.IO.Stream stream, int dataLength)
        {
            //We need to use a memory stream because the BinaryWriter will close the underlying stream when it is closed
            using (var memStream = new System.IO.MemoryStream(64))
            {
                int cbFormat = 18; //sizeof(WAVEFORMATEX)
                WAVEFORMATEX format = new WAVEFORMATEX()
                {
                    wFormatTag = 1,
                    nChannels = 1,
                    nSamplesPerSec = 16000,
                    nAvgBytesPerSec = 32000,
                    nBlockAlign = 2,
                    wBitsPerSample = 16,
                    cbSize = 0
                };

                using (var bw = new System.IO.BinaryWriter(memStream))
                {
                    //RIFF header
                    WriteString(memStream, "RIFF");
                    bw.Write(dataLength + cbFormat + 4); //File size - 8
                    WriteString(memStream, "WAVE");
                    WriteString(memStream, "fmt ");
                    bw.Write(cbFormat);

                    //WAVEFORMATEX
                    bw.Write(format.wFormatTag);
                    bw.Write(format.nChannels);
                    bw.Write(format.nSamplesPerSec);
                    bw.Write(format.nAvgBytesPerSec);
                    bw.Write(format.nBlockAlign);
                    bw.Write(format.wBitsPerSample);
                    bw.Write(format.cbSize);

                    //data header
                    WriteString(memStream, "data");
                    bw.Write(dataLength);
                    memStream.WriteTo(stream);
                }
            }
        }


        /// <summary>
        /// Execute initialization tasks.
        /// </summary>
        public static Boolean InitializeKinectAudio()
        {

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    sensor = potentialSensor;
                    break;
                }
            }

            if (null != sensor)
            {
                try
                {
                    // Start the sensor!
                    sensor.Start();
                }
                catch (System.IO.IOException)
                {
                    // Some other application is streaming from the same Kinect sensor
                    sensor = null;
                }
            }

            if (null == sensor)
            {
                return FAILURE;
            }
            else
            {
                // Start streaming audio!
                audioStream = sensor.AudioSource.Start();
                return SUCCESS;
            }


        }


        ///
        /// UnInitialize Kinect Audio and recording thread
        ///
        public static void UnInitializeKinectAudio()
        {
            // Tell audio reading thread to stop and wait for it to finish.
            recording = false;
            if (null != recordingThread)
            {
                recordingThread.Join();
            }

            if (null != sensor)
            {
                sensor.AudioSource.Stop();
                sensor.Stop();
                sensor = null;
            }
        }



        ///<summary>
        ///Star tcapturing audio from Kinect
        ///Called by Application. Hence Public
        ///</summary>
        public static void StartRecordingAudio(String fileNameInFull)
        {
            recording = true;
            recordingFileFullName = fileNameInFull;

            // Use a separate thread for capturing audio because audio stream read operations
            // will block, and we don't want to block main UI thread.

            recordingFileStream = new System.IO.FileStream(recordingFileFullName, System.IO.FileMode.Create);
            recordingThread = new System.Threading.Thread(AudioRecordingThread);
            recordingThread.Start();
        }


        /// <summary>
        /// Handles polling audio stream and updating visualization every tick.
        /// </summary>
        private static void AudioRecordingThread()
        {
            //Prepare a wave header with a dummy recording length whcih you'll go back and rewrite once you know the actual number at teh end of the recording
            //Possibly not the best way to do  But, should do for now
            //TODO: find a work-around for this hack
            int dummyrecordingLength = 6 * 2 * 16000;
            WriteWavHeader(recordingFileStream, dummyrecordingLength);

            while (recording)
            {
                int readCount = audioStream.Read(audioBuffer, 0, audioBuffer.Length);
                recordingFileStream.Write(audioBuffer, 0, readCount);
                recordedSamplesCount += readCount;

            }

            recordingFileStream.Close();

            System.Console.WriteLine(recordedSamplesCount);

            //Rewrite the wavheader with the right sample count
            System.IO.FileStream newrecordingFileStream = new System.IO.FileStream(recordingFileFullName, System.IO.FileMode.Open);
            WriteWavHeader(newrecordingFileStream, recordedSamplesCount);
            newrecordingFileStream.Close();
            recordedSamplesCount = 0;

        }

        /// <summary>
        /// Called By application. Hence public
        /// </summary>
        public static void StopRecordingAudio()
        {
            recording = false;

        }






    }
}
