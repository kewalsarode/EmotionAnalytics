using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionAnalyticsApp
{
    public class ReportManager
    {
        private Bitmap GeneratePieChart(Dictionary<string, int> emotionsArr, int totalCount)
        {
            Color[] myPieColors = { Color.Green, Color.YellowGreen, Color.Blue, Color.Orange, Color.Red };
            int diameter = 500;
            int rectY = 0;
            Bitmap bmp = new Bitmap(diameter + 150, diameter);
            using (Graphics objGraphic = Graphics.FromImage(bmp))
            {
                int PiePercentTotal = 0;
                for (int i = 0; i < 5; i++)
                {
                    using (SolidBrush objBrush = new SolidBrush(myPieColors[i]))
                    {
                        objGraphic.FillPie(objBrush, new System.Drawing.Rectangle(0, 0, diameter, diameter), Convert.ToSingle(PiePercentTotal * 360 / totalCount), Convert.ToSingle(emotionsArr.Values.ToArray()[i] * 360 / totalCount));
                        objGraphic.FillRectangle(objBrush, diameter + 15, rectY += 20, 15, 15);
                        objGraphic.DrawString(emotionsArr.Keys.ToArray()[i], new System.Drawing.Font("Arial", 11), Brushes.Blue, diameter + 40, rectY);
                    }
                    PiePercentTotal += emotionsArr.Values.ToArray()[i];
                }
            }

            return bmp;
        }

        public void GenerateReport(Dictionary<string,int> emotionsArr, DataTable facesData, string reportFileName)
        {
            int totalCount = facesData.Rows.Count;
            var bmp = GeneratePieChart(emotionsArr, totalCount);
            
            //bmp.Save("D:/sample.jpg");

            using (System.IO.FileStream fileStream = new System.IO.FileStream(reportFileName, System.IO.FileMode.Create))
            {
                Document document = new Document(PageSize.A4, 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(document, fileStream);
                document.Open();

                Paragraph para = new Paragraph("Face Analysis Report");
                para.Alignment = Element.ALIGN_CENTER;
                para.Font = FontFactory.GetFont(FontFactory.HELVETICA, 13f, BaseColor.BLACK);
                document.Add(para);

                Phrase phrase = new Phrase("\nDate: " + DateTime.Now.ToShortDateString());
                document.Add(phrase);

                PdfPTable tab = new PdfPTable(2);
                tab.AddCell("Delighted");
                tab.AddCell(emotionsArr["Delighted"].ToString());

                tab.AddCell("Happy");
                tab.AddCell(emotionsArr["Happy"].ToString());

                tab.AddCell("Neutral");
                tab.AddCell(emotionsArr["Neutral"].ToString());

                tab.AddCell("Not Happy");
                tab.AddCell(emotionsArr["Not Happy"].ToString());

                tab.AddCell("Sad");
                tab.AddCell(emotionsArr["Sad"].ToString());

                tab.AddCell("Total Faces Analyzed");
                tab.AddCell(totalCount.ToString());
                document.Add(tab);

                document.Add(new Paragraph());
                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(bmp, BaseColor.WHITE);
                jpg.ScaleToFit(bmp.Width * 0.6f, bmp.Height * 0.6f);
                jpg.Alignment = Element.ALIGN_CENTER;
                jpg.SpacingBefore = 50;
                jpg.SpacingAfter = 20;
                document.Add(jpg);

                document.NewPage();
                para = new Paragraph("Face detail Report");
                para.Alignment = Element.ALIGN_CENTER;
                para.Font = FontFactory.GetFont(FontFactory.HELVETICA, 13f, BaseColor.BLACK);
                document.Add(para);

                string dirPath = ConfigurationManager.AppSettings["res_dir_location"].ToString();
                PdfPTable tabImg = new PdfPTable(2);
                tabImg.SpacingBefore = 20f;
                
                foreach (DataRow row in facesData.Rows)
                {
                    string fileName = dirPath + "/images/" + row["Description"];
                    iTextSharp.text.Image faceImg = iTextSharp.text.Image.GetInstance(fileName);
                    faceImg.ScaleAbsolute(280f, 160f);
                    tabImg.AddCell(faceImg);
                }
                document.Add(tabImg);
                document.Close();        
            }
        }
    }
}

