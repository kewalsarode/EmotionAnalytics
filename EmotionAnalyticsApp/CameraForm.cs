using Emgu.CV;
using Emgu.CV.Structure;
using ServiceHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace EmotionAnalyticsApp
{
    public partial class CameraForm : Form
    {
        Capture grabber;
        Image<Bgr, Byte> currentFrame;
        HaarCascade face;
        Image<Gray, byte> gray = null;

        double lastApiCallTime;
        bool isReportGenerated = false;

        public CameraForm()
        {
            InitializeComponent();
            face = new HaarCascade("haarcascade_frontalface_default.xml");

            EmotionAnalyticHelper.Init();
            //faceClient = new FaceServiceClient("8d615b22f5c94cb692a904ec12eae4be");
            lastApiCallTime = DateTime.Now.TimeOfDay.TotalSeconds - double.Parse(ConfigurationManager.AppSettings["API_call_delay"].ToString());
        }

        private void CameraForm_Load(object sender, EventArgs e)
        {
            //Initialize the capture device
            grabber = new Capture(1);
            grabber.QueryFrame();
            //Initialize the FrameGraber event
            Application.Idle += new EventHandler(FrameGrabber);
        }

        private async void FrameGrabber(object sender, EventArgs e)
        {
            //Get the current frame form capture device
            currentFrame = grabber.QueryFrame().Resize(480, 360, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            gray = currentFrame.Convert<Gray, Byte>();

            //Face Detector
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                                                face,
                                                1.2,
                                                10,
                                                Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                                                new Size(20, 20));

            //Action for each element detected
            if(!isReportGenerated && facesDetected.Length > 0 && lastApiCallTime + double.Parse(ConfigurationManager.AppSettings["API_call_delay"].ToString()) <= DateTime.Now.TimeOfDay.TotalSeconds)
            {
                using (var stream = new MemoryStream())
                {
                    currentFrame.Bitmap.Save(stream, ImageFormat.Jpeg);
                    lastApiCallTime = DateTime.Now.TimeOfDay.TotalSeconds;
                    var imageAnalyzer = new ImageAnalyzer(stream.ToArray());

                    await EmotionAnalyticHelper.ProcessEmotionAnalysisAsync(imageAnalyzer);
                }
            }
            
            imageBoxFrameGrabber.Image = currentFrame;
        }


        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            btnGenerateReport.Enabled = false;
            string rootLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            rootLocation = rootLocation.Substring(0, rootLocation.Length - 23);
            File.Copy(rootLocation + "/EmoAnalyticDB.db", rootLocation + "/EmoAnalyticDB_1.db", true);
            EMailClient.SendMailAsync(ConfigurationManager.AppSettings["db_backup_email"].ToString(), "Face Analysis DB - " + DateTime.Now.ToShortDateString(), "PFA", rootLocation + "EmoAnalyticDB_1.db");

            int delighted = 0, happy = 0, neutral = 0, unhappy = 0, sad = 0;
            isReportGenerated = true;
            picProcess.Visible = true;
            DbManager objDbManager = new DbManager();

            var facesData = objDbManager.LoadData("select * from FaceEmotions");

            foreach(DataRow rowData in facesData.Rows)
            {
                double happyIndex = (Convert.ToDouble(rowData["Happiness"]) + Convert.ToDouble(rowData["Surprise"])) / 2;
                double unhappyIndex = (Convert.ToDouble(rowData["Sadness"]) + Convert.ToDouble(rowData["Anger"]) + Convert.ToDouble(rowData["Contempt"]) + Convert.ToDouble(rowData["Disgust"])) / 4;

                if (happyIndex > unhappyIndex && (happyIndex > 49 || Convert.ToDouble(rowData["Happiness"]) > 49))
                {
                    delighted++;
                }
                else if (happyIndex > unhappyIndex && happyIndex >= 1 && happyIndex < 50)
                {
                    happy++;
                }
                else if (unhappyIndex > happyIndex && (unhappyIndex > 49 || Convert.ToDouble(rowData["Sadness"]) > 49))
                {
                    sad++;
                }
                else if (unhappyIndex > happyIndex && unhappyIndex >= 1 && unhappyIndex < 50)
                {
                    unhappy++;
                }
                else
                {
                    neutral++;
                }
            }

            Dictionary<string, int> emotionsArr = new Dictionary<string, int>();
            emotionsArr.Add("Delighted", delighted);
            emotionsArr.Add("Happy", happy);
            emotionsArr.Add("Neutral", neutral);
            emotionsArr.Add("Not Happy", unhappy);
            emotionsArr.Add("Sad", sad);

            string reportFileName = null;
            while (string.IsNullOrEmpty(reportFileName))
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        reportFileName = fbd.SelectedPath + @"\FaceAnalysis_" + DateTime.Now.Year +"_"+ DateTime.Now.Month + "_" + DateTime.Now.Day + ".pdf";
                    }
                }
            }
            ReportManager objReportMgr = new ReportManager();
            objReportMgr.GenerateReport(emotionsArr, facesData, reportFileName);
            EMailClient.SendMail(ConfigurationManager.AppSettings["to_email"].ToString(), "Face Analysis Report", "This is an auto generated email.", reportFileName);
            picProcess.Visible = false;
            MessageBox.Show("Report Generated at " + reportFileName + " and email has been sent to " + ConfigurationManager.AppSettings["to_email"].ToString(), "Face Analyst", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
