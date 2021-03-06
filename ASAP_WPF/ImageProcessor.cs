﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Documents;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;


namespace ASAP_WPF
{
    public class ImageProcessor :BaseThread
    {
        public ImageProcessor()
            : base()
        {
        }
        public override void RunThread()
        {
            Process();
        }
        public int AdaptiveThresholdConstant { get; set; }

        public Mat ImageMat { get; set; }
        public Mat OgImageMat { get; set; }
        public Mat ContourImageMat { get; set; }
        public VectorOfVectorOfPoint Contours { get; set; }
        public VectorOfVectorOfPoint ContoursToReturn { get; set; }
        public VectorOfVectorOfPoint AngledBoundingBoxesToReturn{ get; set; }
        //public List<Rectangle> UprightBoundingRectangles { get; set; }




        public void SetValues(Mat imgMat, int @const)
        {
            this.ImageMat = new Mat();
            imgMat.CopyTo(this.ImageMat);
            this.AdaptiveThresholdConstant = @const;
        }

        public void SetValues(string imgPath, int @const)
        {
            this.ImageMat = CvInvoke.Imread(imgPath);
            this.AdaptiveThresholdConstant = @const;
        }

        public void Process()
        {
            if(null == this.ImageMat)
            {
                return;
            }

            try
            {
                this.OgImageMat = this.ImageMat.CreateNewHardCopyFromMat();

                //Itt megkellene nézni, hogy mik vannak minden enum mögött, hogy van-e jobb alternatíva
                //Blurring image
                var tempSize = new Size(5, 5);
                //Using input | output arrays instead of temp objects
                CvInvoke.GaussianBlur(this.ImageMat, this.ImageMat, tempSize, 0);

                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "ImageProcessor_GaussianBlur");
                //Adaptive threshold
                var tempAdaptiveThreshold = this.AdaptiveThresholdConstant;
                //Azt az 59-et majd meg kell lesni, hogy miért az van


                CvInvoke.AdaptiveThreshold(this.ImageMat, this.ImageMat, 255, Emgu.CV.CvEnum.AdaptiveThresholdType.MeanC, Emgu.CV.CvEnum.ThresholdType.BinaryInv, 59, this.AdaptiveThresholdConstant);
                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "ImageProcessor_AdaptiveThreshold");
                //Get contours and remove small patches
                Contours = new VectorOfVectorOfPoint();
                Mat hierarchy = new Mat();
                // meg kellene nézni miez ez a mat, vactorofvectorpoint és input, output és inoutputarrayek
                CvInvoke.FindContours(this.ImageMat, Contours, hierarchy, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                var dims = Contours.Size;
                for (var idx = 0; idx < dims; idx++)
                {
                    var con = Contours[idx];
                    if (!(CvInvoke.ContourArea(con) < 800)) continue;
                    var color = new MCvScalar(0, 0, 0);
                    //CvInvoke.FillPoly(this.ImageMat, con, color);
                    //TODO ezt lehet meg kellene még vizsgálni
                    CvInvoke.FillConvexPoly(this.ImageMat, con, color);
                }

                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "ImageProcessor_FillConvexPoly");
                //Open then close to close gaps
                var kernelMat1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
                CvInvoke.MorphologyEx(
                    this.ImageMat,
                    this.ImageMat,
                    Emgu.CV.CvEnum.MorphOp.Open,
                    kernelMat1,
                    new Point(-1, -1),
                    1, Emgu.CV.CvEnum.BorderType.Default,
                    new MCvScalar());
                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "ImageProcessor_MorphologyEx_Open");
                var kernelMat2 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(7, 7), new Point(-1, -1));
                CvInvoke.MorphologyEx(
                    this.ImageMat,
                    this.ImageMat,
                    Emgu.CV.CvEnum.MorphOp.Close,
                    kernelMat2,
                    new Point(-1, -1),
                    2,
                    Emgu.CV.CvEnum.BorderType.Default,
                    new MCvScalar());
                MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "ImageProcessor_MorphologyEx_Close");
                //Get new contours find the correct ones by size and hierarchy draw them onto the img, draw bounding box, and length
                CvInvoke.FindContours(this.ImageMat, Contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                //Empty out objects contours
                ContoursToReturn = new VectorOfVectorOfPoint();
                //Image to be overlayed, with alpha values
                //ContourImageMat = new Mat(this.ImageMat.Rows, this.ImageMat.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
                ContourImageMat = OgImageMat.CreateNewHardCopyFromMat();
                CvInvoke.CvtColor(ContourImageMat, ContourImageMat, ColorConversion.Gray2Bgr);
                //ContourImageMat = new Mat(this.ImageMat.Rows, this.ImageMat.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                //MainWindow.ImageProcessorExaminer.AddImage(ContourImageMat.CreateNewHardCopyFromMat(), "ImageProcessor_ContourImageMat");
                dims = Contours.Size;

                var tempVoVoPointF = new VectorOfVectorOfPointF();

                for (var idx = 0; idx < dims; idx++)
                {
                    var con = Contours[idx];
                    //Skip small patches
                    if (CvInvoke.ContourArea(con) < 300)
                    {
                        continue;
                    }
                    //Only use ones that have parents, so the inner ones
                    //https://stackoverflow.com/questions/41560048/c-sharp-emgu-cv-findcontours-hierarchy-data-always-null
                    //https://stackoverflow.com/questions/37408481/navigate-through-hierarchy-of-contours-found-by-findcontours-method/37470968#37470968
                    //http://www.emgu.com/forum/viewtopic.php?t=6333
                    //http://www.emgu.com/forum/viewtopic.php?f=7&t=5263
                    //https://stackoverrun.com/fr/q/11437249
                    if ((int)hierarchy.GetData().GetValue(0, idx, 3) < 0)
                    {
                        continue;
                    }
                    ContoursToReturn.Push(con);
                    //UprightBoundingRectangles.Add(CvInvoke.BoundingRectangle(con));
                    //CvInvoke.DrawContours(ContourImageMat, Contours, 0, new MCvScalar(0, 255, 0, 255), 2);
                    CvInvoke.DrawContours(ContourImageMat, Contours, -1, new MCvScalar(0, 255, 0, 255), 1);

                    var rect = CvInvoke.MinAreaRect(con);
                    var box = CvInvoke.BoxPoints(rect);
                    var boxVec = new VectorOfPointF(box);
                    tempVoVoPointF.Push(boxVec);
                    /*
                    var rect = CvInvoke.MinAreaRect(con);
                    //var box = CvInvoke.BoxPoints(rect);
                    //VectorOfPointF boxVec = new VectorOfPointF(box);
                    VectorOfVectorOfPoint boxVec = new VectorOfVectorOfPoint(con);
                    //az jókérdés, hogy ez most a megfelelő alak-e vagy sem
                    CvInvoke.DrawContours(ContourImageMat, boxVec, 0, new MCvScalar(0, 255, 0, 255), 2);
                    */
                }
                MainWindow.ImageProcessorExaminer.AddImage(ContourImageMat.CreateNewHardCopyFromMat(), "ImageProcessor_ContourImageMat");
                this.AngledBoundingBoxesToReturn = tempVoVoPointF.ConvertToVectorOfPoint();

                MainWindow.ImageProcessorExaminer.AddImage(ContourImageMat.CreateNewHardCopyFromMat(), "ImageProcessor_ContourImageMat_2");
                //CvInvoke.PutText(this.ImageMat, "{" + ContoursToReturn.Size + "}", new Point(100, 300), Emgu.CV.CvEnum.FontFace.HersheySimplex, 8.0, new MCvScalar(255), 5);
                //this.DetectedCellCount = ContoursToReturn.Size;
                //MainWindow.ImageProcessorExaminer.AddImage(ImageMat.CreateNewHardCopyFromMat(), "ImageProcessor_PutText");
                //CvInvoke.EqualizeHist(this.ImageMat,this.ImageMat);

                //new PopupImage(ImageMat, "ImgProcessor_ImageMat").Show();
                //new PopupImage(ContourImageMat, "ImgProcessor_ContourImageMat").Show();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void ExportImg(string exportPath)
        {
            CvInvoke.Imwrite(exportPath, this.ImageMat);
        }


    }
}
