Kinect-as-a-Speech-Recorder
===========================

A C# Application for recording utterances using the MS Kinect as a microphone.
Intended use is to record utterances for specific sentences, Build speech recogntion models/Test models.

AUTHOR: Seshadri Sridharan

DEPENDENCIES:
MS Kinect SDK 1.6
Visual C# 2008 or later


APP DETAILS:
Kinect Audio recordings are echo cancelled and noise supressed by default.

Steps to record Audio:
1) Point to an utterances file 
2) Press load utterances file
3) Enter output directory for recordings

Uttereances file must be in CSV format, with no headers.
It needs to have the .wav file name and the utterance text tab separated. Eg: "filename.wav	This is my utterance."
Look at example_utetrances.csv in this folder for a template.