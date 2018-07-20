using System;
using System.IO;
using System.Threading;
using pdfforge.PDFCreator.UI.ComWrapper;

namespace PrintDemo
{
    class Program
    {
        public static void Main(string[] args)
        {
           Thread PrintHandle = new Thread(SetConfig2);
            PrintHandle.Start();
        }

        private static bool _isTypeInitialized;
        public static Queue CreateQueue()
        {
            if (!_isTypeInitialized)
            {
                Type queueType = Type.GetTypeFromProgID("PDFCreator.JobQueue");
                Activator.CreateInstance(queueType);
                _isTypeInitialized = true;
            }
            return new Queue();
        }


        public static void SetConfig2()
        {
            Random rd = new Random();
            var jobQueue = CreateQueue();
            var convertedFilePath = Path.Combine(@"C:\x", rd.Next() + "Demo.jpg");

            while (true)
            {
                jobQueue.Initialize();
                Console.WriteLine("Start Printing and ReceieFile......");
                if (!jobQueue.WaitForJob(2000))
                {
                    Console.WriteLine("Currently there are " + jobQueue.Count + " job(s) in the queue");
                }
                else
                {
                    var printJob = jobQueue.NextJob;
                    printJob.SetProfileByGuid("JpegGuid");

                    Console.WriteLine("Applying jpeg settings");
                    printJob.SetProfileSetting("JpegSettings.Color", "Color24Bit");
                    printJob.SetProfileSetting("JpegSettings.Quality", "100");
                    printJob.SetProfileSetting("OpenViewer", "false");
                    //printJob.SetProfileSetting("ShowAllNotifications", "false");
                    printJob.ConvertTo(convertedFilePath);

                    if (!printJob.IsFinished || !printJob.IsSuccessful)
                    {
                        Console.WriteLine("Could not convert: " + convertedFilePath);
                    }
                    else
                    {
                        Console.WriteLine("The conversion was succesful!");
                        Console.WriteLine("JobQueue Number is " + jobQueue.Count.ToString());
                        jobQueue.ReleaseCom();
                        Console.WriteLine("Cycle Again..");
                    }
                }
            }
        }
    }
}
