using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceHelpers;

namespace EmotionAnalyticsApp
{
    public class EmotionAnalyticHelper
    {
        static string dirPath;
        static long index = 1;
        static DbManager objDbManager;

        public static void Init(Action throttled = null)
        {
            objDbManager = new DbManager();
            try
            {
                index = (long)objDbManager.ExecuteFunctions("select COALESCE(MAX(Id),0) +1 AS NextIndex from FaceEmotions");
            }
            catch(Exception ex)
            {
                index = 1;
                System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
                System.Windows.Forms.Application.Exit();
            }
            //FaceServiceHelper.ApiKey = "462df473088b4f7dbab6df3333632325"; 
            FaceServiceHelper.ApiKey = "7b49110c760d41fdb2d76a5a38c55fa4";
            if (throttled != null)
                FaceServiceHelper.Throttled += throttled;
            dirPath = ConfigurationManager.AppSettings["res_dir_location"].ToString();
        }

        public static async Task ProcessEmotionAnalysisAsync(ImageAnalyzer e)
        {
            await e.DetectFacesAsync(true);

            if (e.DetectedFaces.Any())
            {
                Microsoft.ProjectOxford.Face.Contract.Face face = e.DetectedFaces.FirstOrDefault();
                var stream = await e.GetImageStreamCallback();
                Bitmap bmp = new Bitmap(600, 360);
                Graphics g = Graphics.FromImage(bmp);
                g.DrawImage(new Bitmap(stream), 0, 0);
                
                g.DrawString(face.FaceAttributes.Gender.ToUpper() + ", " + face.FaceAttributes.Age, new Font("Arial", 11), Brushes.Black, 490, 10);
                string emotions = "";
                string dbCols = null, dbVals=null;
                foreach(var prop in face.FaceAttributes.Emotion.GetType().GetProperties())
                {
                    dbCols += prop.Name + ",";
                    dbVals += (Convert.ToDouble(prop.GetValue(face.FaceAttributes.Emotion)) * 100) + ",";
                    emotions += prop.Name + ": " + (Convert.ToDouble(prop.GetValue(face.FaceAttributes.Emotion)) * 100) + " %\n";
                }

                g.DrawString(emotions, new Font("Arial", 10), Brushes.Black, 490, 40);
                string emojiPath = dirPath + "/res/" + GetEmoji(face.FaceAttributes.Emotion);
                g.DrawImage(new Bitmap(emojiPath), 490, 250, 100, 100);
                g.Dispose();

                string imgName = "face" + index + ".jpg";
                bmp.Save(dirPath + "/images/" + imgName);
                objDbManager.ExecuteQuery("INSERT INTO FaceEmotions (" + dbCols + " Description" + ") VALUES (" + dbVals +"'"+ imgName + "')");
                index++;
                //your code
            }
            else
            {
                Console.WriteLine("No Face Detected.");
            }
        }

        static string GetEmoji(Microsoft.ProjectOxford.Face.Contract.Emotion emotion)
        {
            double happyIndex = ((emotion.Happiness + emotion.Surprise) / 2) * 100;
            double unhappyIndex = ((emotion.Sadness + emotion.Anger + emotion.Contempt + emotion.Disgust) / 4) * 100;

            if (happyIndex > unhappyIndex && (happyIndex > 49 || emotion.Happiness > 0.49))
            {
                return "full_happy.png";
            }
            else if(happyIndex > unhappyIndex && happyIndex >=1 && happyIndex < 50)
            {
                return "normal_happy.png";
            }
            else if (unhappyIndex > happyIndex && (unhappyIndex > 49 || emotion.Sadness > 0.49))
            {
                return "sad.png";
            }
            else if(unhappyIndex > happyIndex && unhappyIndex >=1 && unhappyIndex < 50)
            {
                return "Unhappy.png";
            }
            else
            {
                return "neutral.png";
            }
        }
    }
}
