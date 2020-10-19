using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace ASAP_WPF
{
    internal class MicroMeterCalibrator
    {
        private VectorOfVectorOfPoint Contours { get; set; }
        private VectorOfVectorOfPoint ContoursToMeasure { get; set; }

        private List<RotatedRect> BoundingBoxes { get; set; }

        private Mat ImageMat { get; set; }
        private Mat ContourImageMat { get; set; }

        private int AdaptiveThresholdConstant { get; set; }

        private int MicrometerOnImg { get; set; }
        public float CalibratedMicroMeterPerPixel { get; set; }

        public MicroMeterCalibrator(Mat microMeterImg, int micrometerOnImg = 10 ,int thresholdConst = 5)
        {
            this.ImageMat = microMeterImg;
            this.AdaptiveThresholdConstant = thresholdConst;
            this.MicrometerOnImg = micrometerOnImg;
            this.BoundingBoxes = new List<RotatedRect>();
            this.ProcessImage();
            this.CalculateMicroMeterPerPixel();

        }

        private void CalculateMicroMeterPerPixel()
        {
            var temp = (from box in BoundingBoxes let tempW = box.Size.Width let tempH = box.Size.Height select tempW + tempH).Sum();

            temp /= (BoundingBoxes.Count * 2);
            //??
            //temp /= MicrometerOnImg;

            this.CalibratedMicroMeterPerPixel = temp;
        }

        private void ProcessImage()
        {
            if (null == this.ImageMat)
            {
                return;
            }

            try
            {
                CvInvoke.GaussianBlur(this.ImageMat, this.ImageMat, new System.Drawing.Size(5,5), 0);

                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "MicroMeterCalibrator_GaussianBlur");
                CvInvoke.AdaptiveThreshold(this.ImageMat, this.ImageMat, 255, Emgu.CV.CvEnum.AdaptiveThresholdType.MeanC, Emgu.CV.CvEnum.ThresholdType.BinaryInv, 59, this.AdaptiveThresholdConstant);
                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "MicroMeterCalibrator_AdaptiveThreshold");
                Contours = new VectorOfVectorOfPoint();
                var hierarchy = new Mat();
                CvInvoke.FindContours(this.ImageMat, Contours, hierarchy, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                var dims = Contours.Size;
                for (var idx = 0; idx < dims; idx++)
                {
                    var con = Contours[idx];
                    if (!(CvInvoke.ContourArea(con) < 800)) continue;
                    var color = new MCvScalar(0, 0, 0);
                    CvInvoke.FillConvexPoly(this.ImageMat, con, color);
                }

                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "MicroMeterCalibrator_FillConvexPoly");
                var kernelMat1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(3, 3), new System.Drawing.Point(-1, -1));
                CvInvoke.MorphologyEx(
                    this.ImageMat,
                    this.ImageMat,
                    Emgu.CV.CvEnum.MorphOp.Open,
                    kernelMat1,
                    new System.Drawing.Point(-1, -1),
                    1, Emgu.CV.CvEnum.BorderType.Default,
                    new MCvScalar());
                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "MicroMeterCalibrator_MorphologyEx_Open");
                var kernelMat2 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(7, 7), new System.Drawing.Point(-1, -1));
                CvInvoke.MorphologyEx(
                    this.ImageMat,
                    this.ImageMat,
                    Emgu.CV.CvEnum.MorphOp.Close,
                    kernelMat2,
                    new Point(-1, -1),
                    2,
                    Emgu.CV.CvEnum.BorderType.Default,
                    new MCvScalar());
                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "MicroMeterCalibrator_MorphologyEx_Close");
                CvInvoke.FindContours(this.ImageMat, Contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                ContoursToMeasure = new VectorOfVectorOfPoint();
                ContourImageMat = new Mat(this.ImageMat.Rows, this.ImageMat.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
                MainWindow.ImageProcessorExaminer.AddImage(ContourImageMat.CreateNewHardCopyFromMat(), "MicroMeterCalibrator_ContourImageMat");
                dims = Contours.Size;

                for (var idx = 0; idx < dims; idx++)
                {
                    var con = Contours[idx];
                    if (CvInvoke.ContourArea(con) < 300)
                    {
                        continue;
                    }
                    if ((int)hierarchy.GetData().GetValue(0, idx, 3) < 0)
                    {
                        continue;
                    }
                    ContoursToMeasure.Push(con);
                    CvInvoke.DrawContours(ContourImageMat, Contours, 0, new MCvScalar(0, 255, 0, 255), 1);

                    var rect = CvInvoke.MinAreaRect(con);
                    this.BoundingBoxes.Add(rect);
                }

                MainWindow.ImageProcessorExaminer.AddImage(ContourImageMat.CreateNewHardCopyFromMat(), "MicroMeterCalibratorContourImageMat_2");
                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "MicroMeterCalibrator_PutText");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
