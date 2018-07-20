using pdfforge.PDFCreator.UI.ComWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace TestPDFCreator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private delegate void BarMessageSet(string barcode);
        private readonly List<string> _lstMessage = new List<string>();

        private void SetBarMessage(string msg)
        {
            if (InvokeRequired)
            {
                BarMessageSet bms = SetBarMessage;
                BeginInvoke(bms, msg);
            }
            else
            {
                string dispMsg = $"{DateTime.Now.ToString("HH:mm:ss")}：{msg}";
                lblMsg.Text = dispMsg;
                timerMsg.Stop();
                timerMsg.Start();

                //加入消息队列
                if (_lstMessage.Count > 99)
                    _lstMessage.RemoveRange(99, _lstMessage.Count - 99);
                _lstMessage.Insert(0, dispMsg);

                StringBuilder sb = new StringBuilder();
                foreach (string msg1 in _lstMessage)
                {
                    sb.AppendLine(msg1);
                }
                tbMessage.Text = sb.ToString();
                tbMessage.SelectionStart = 0;
                tbMessage.SelectionLength = 0;
            }
        }

        Thread PrintHandle;

        private void Button1_Click(object sender, EventArgs e)
        {
            PrintHandle = new Thread(SetConfig2);
            PrintHandle.Start();
        }

        private bool _isTypeInitialized;
        public Queue CreateQueue()
        {
            if (!_isTypeInitialized)
            {
                Type queueType = Type.GetTypeFromProgID("PDFCreator.JobQueue");
                Activator.CreateInstance(queueType);
                _isTypeInitialized = true;
            }
            return new Queue();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            start = true;
        }


        private void SetConfig()
        {
            var jobQueue = CreateQueue();
            var convertedFilePath = Path.Combine(@"C:\x", "TestPage_2Jpeg.jpg");

            jobQueue.Initialize();
            if (!jobQueue.WaitForJob(20))
            {
                SetBarMessage("Currently there are " + jobQueue.Count + " job(s) in the queue");
            }
            else
            {
                var printJob = jobQueue.NextJob;
                printJob.SetProfileByGuid("JpegGuid");

                SetBarMessage("Applying jpeg settings");
                printJob.SetProfileSetting("JpegSettings.Color", "Color24Bit");
                printJob.SetProfileSetting("JpegSettings.Quality", "100");
                printJob.SetProfileSetting("OpenViewer", "false");
                printJob.SetProfileSetting("ShowAllNotifications", "false");

                printJob.ConvertTo(convertedFilePath);

                if (!printJob.IsFinished || !printJob.IsSuccessful)
                {
                    SetBarMessage("Could not convert: " + convertedFilePath);
                }
                else
                {
                    SetBarMessage("The conversion was succesful!");
                    SetBarMessage(jobQueue.Count.ToString());
                    jobQueue.ReleaseCom();
                }
            }
            SetBarMessage(jobQueue.Count.ToString());
        }

        bool start;
       
        private void SetConfig2()
        {
            Random rd = new Random();
            var jobQueue = CreateQueue();
            var convertedFilePath = Path.Combine(@"C:\x", rd.Next().ToString()+ "Demo.jpg");

            while (start)
            {
                jobQueue.Initialize();
                SetBarMessage("开始打印任务");
                if (!jobQueue.WaitForJob(2000))
                {
                    SetBarMessage("Currently there are " + jobQueue.Count + " job(s) in the queue");
                }
                else
                {
                    var printJob = jobQueue.NextJob;
                    printJob.SetProfileByGuid("JpegGuid");

                    SetBarMessage("Applying jpeg settings");
                    printJob.SetProfileSetting("JpegSettings.Color", "Color24Bit");
                    printJob.SetProfileSetting("JpegSettings.Quality", "100");
                    printJob.SetProfileSetting("OpenViewer", "false");
                    //printJob.SetProfileSetting("ShowAllNotifications", "false");

                    printJob.ConvertTo(convertedFilePath);

                    if (!printJob.IsFinished || !printJob.IsSuccessful)
                    {
                        SetBarMessage("Could not convert: " + convertedFilePath);
                    }
                    else
                    {
                        SetBarMessage("The conversion was succesful!");
                        SetBarMessage(jobQueue.Count.ToString());
                        jobQueue.ReleaseCom();
                        SetBarMessage(jobQueue.Count.ToString());
                    }
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (PrintHandle == null)
            {
                
            }
            else
            {
                //start = false;
                PrintHandle.Abort();
            }
            Application.ExitThread();

        }
    }
}
