/* 
Author: Seshadri Sridharan, LTI, Carnegie Mellon Univ
<first 5 letters of first name> +'s' AT andrew.cmu.edu
Feb 2013
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WpfApplication1
{
    class Utterances
    {
        private static List<string> fileNames = new List<String>();
        private static List<string> utterances = new List<String>();
        private static int count;
        private static int curIndex;


        public static String CurrentFileName()
        {
            return fileNames[curIndex];
        }


        public static bool Read(string filePath)
        {
            try
            {
                var reader = new StreamReader(File.OpenRead(@filePath));
                fileNames = new List<string>();
                utterances = new List<string>();
                count = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split('\t');

                    fileNames.Add(values[0]);
                    utterances.Add(values[1]);
                    count += 1;
                }

                curIndex = 0;
                Console.WriteLine(fileNames.ToString());
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public static String NextUtterance()
        {
            curIndex=(curIndex+1)%count;
            return utterances[curIndex];
        }


        public static bool hasNext()
        {
            if (curIndex == count - 1)
                return false;
            else return true;

        }

        public static bool hasPrevious()
        {
            if (curIndex == 0)
                return false;
            else return true;

        }


        public static String PreviousUtterance()
        {
            curIndex = (curIndex - 1) % count;
            return utterances[curIndex];
        }

        public static String CurrentUtterance()
        {
            return utterances[curIndex];
        }


        public static bool IsNonEmpty()
        {
            if (count > 0)
                return true;
            else
                return false;
        }

        public static void main(String[] args)
        {
            Read("test.csv");

        }


    }
}
