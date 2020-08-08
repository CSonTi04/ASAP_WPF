using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Point = System.Drawing.Point;

namespace ASAP_WPF
{
    public class ImageHandler
    {
        public string FolderName { get; set; }
        public string ImgName { get; set; }
        public List<string> Files { get; set; } //paths for the time being
        public int OpenedImgNumber { get; set; }
        public Mat Image { get; set; }
        public Mat ProcessedImage { get; set; }
        public VectorOfVectorOfPoint Contours { get; set; }
        public Mat ContourImage { get; set; }

        public Mat ProcessedImgWithContourOverlay { get; set; }
        public Mat OgImgWithContourOverlay { get; set; }
        /*
        public enum ModTypeEnum
        {
            Clahe,
            EqualizeHist
        }
        */
        private int AdaptiveThresholdConstant { get; set; }
        //sginals? https://softwareengineering.stackexchange.com/questions/142458/any-practical-alternative-to-the-signals-slots-model-for-gui-programming
        //threadpool System.Threading.ThreadPool;

        public ImageProcessor ImgProcessor { get; set; } //auto delete?? signalos  itt is

        public override string ToString()
        {
            return "ImageHandler, opened img:" + this.ImgName;
        }

        public ImageHandler()
        {
            FolderName = null;
            ImgName = null;
            Files = new List<string>();
            OpenedImgNumber = 0;
            Image = null;
            ProcessedImage = null;
            Contours = new VectorOfVectorOfPoint();
            ContourImage = null;
            AdaptiveThresholdConstant = 5;
            ImgProcessor = new ImageProcessor();
        }

        public ImageHandler(string path)
        {
            FolderName = null;
            ImgName = null;
            Files = new List<string>();
            OpenedImgNumber = 0;
            Image = null;
            ProcessedImage = null;
            Contours = new VectorOfVectorOfPoint();
            ContourImage = null;
            AdaptiveThresholdConstant = 5;
            ImgProcessor = new ImageProcessor();
            this.UpdateFolder(path);
        }



        public void UpdateImage()
        {
            if (this.Files.Count <= 0 || this.OpenedImgNumber < 0) return;
            this.ImgName = this.Files[OpenedImgNumber];
            //itt lehetne kísérletezni, hogy egyből két árnyalatos szürkével menjen, vagy a hisztogram generáláshoz több árnyalatos legyen
            //tényleg meg kellene nézni, hogy ennek van -e effektje a kimenetelre
            this.Image = CvInvoke.Imread(this.ImgName, Emgu.CV.CvEnum.ImreadModes.ReducedGrayscale8);
            this.ImgProcessor.SetValues(this.Image, this.AdaptiveThresholdConstant);
            //this.isloading == ture???? ez valami signalos lehet
            //this.signals.image_provcessing_change.emit(True)
            Process();
        }
        /*
        public void UpdateImage(string modType)
        {
            if (this.Files.Count <= 0 || this.OpenedImgNumber < 0) return;
            if (modType.Equals("EqualizeHist"))
            {
                this.ImgName = this.Files[OpenedImgNumber];
                this.Image = CvInvoke.Imread(this.ImgName, Emgu.CV.CvEnum.ImreadModes.ReducedGrayscale8);
                CvInvoke.EqualizeHist(this.Image, this.Image);
                this.ImgProcessor.SetValues(this.Image, this.AdaptiveThresholdConstant);
            }
            else if (modType.Equals("Clahe"))
            {
                this.ImgName = this.Files[OpenedImgNumber];
                this.Image = CvInvoke.Imread(this.ImgName, Emgu.CV.CvEnum.ImreadModes.ReducedGrayscale8);
                CvInvoke.CLAHE(this.Image,20.0,new Size(8,8),this.Image);
                this.ImgProcessor.SetValues(this.Image, this.AdaptiveThresholdConstant);
            }
        }
        */


        //saját
        public void Process()
        {
            //TODO ezeknek a refeknek az elhagyása
            this.ProcessedImage = null;
            this.ContourImage = null;
            this.Contours.Clear();
            ImgProcessor.Process();
            this.ProcessedImage = ImgProcessor.ImageMat;
            this.ContourImage = ImgProcessor.ContourImageMat;
            this.Contours.Push(ImgProcessor.ContoursToReturn);
            MainWindow.ImageProcessorExaminer.AddImage(Image, "ImageHandler_Image");
            MainWindow.ImageProcessorExaminer.AddImage(ProcessedImage, "ImageHandler_ProcessedImage");
            MainWindow.ImageProcessorExaminer.AddImage(ContourImage, "ImageHandler_ContourImage");
            //new PopupImage(Image, "ImageHandler_Image").Show();
            //new PopupImage(ProcessedImage, "ImageHandler_ProcessedImage").Show();
            //new PopupImage(ContourImage, "ImageHandler_ContourImage").Show();
        }

        public void ProcessOverlays(System.Drawing.Point lastClickedPoint)
        {
            //Ide kell majd a single cell contourt átdobni
            var processedImgWithSingleCellContour = DrawCellContourBoxToNewMat(lastClickedPoint, this.ProcessedImage);
            MainWindow.ImageProcessorExaminer.AddImage(processedImgWithSingleCellContour.createNewHardCopyFromMat(), "processedImgWithSingleCellContour");
            var ogImgWithSingleCellContour = DrawCellContourBoxToNewMat(lastClickedPoint, this.Image);
            MainWindow.ImageProcessorExaminer.AddImage(ogImgWithSingleCellContour.createNewHardCopyFromMat(), "ogImgWithSingleCellContour");
            this.ProcessedImgWithContourOverlay = OverlayImageMats(processedImgWithSingleCellContour, this.ContourImage);
            MainWindow.ImageProcessorExaminer.AddImage(ProcessedImgWithContourOverlay.createNewHardCopyFromMat(), "ProcessedImgWithContourOverlay");
            this.OgImgWithContourOverlay  = OverlayImageMats(ogImgWithSingleCellContour, this.ContourImage);
            MainWindow.ImageProcessorExaminer.AddImage(OgImgWithContourOverlay.createNewHardCopyFromMat(), "OgImgWithContourOverlay");

        }

        public Mat OverlayImageMats(Mat obscuredImgMat, Mat overlayedImgMat)
        {
            var matToReturn = new Mat();
            try
            {
                //Valami azt súgja, hogy
                //https://answers.opencv.org/question/24463/how-to-remove-black-background-from-grabcut-output-image-in-opencv-android/?comment=24786#comment-24786
                //Ez kell majd
                var tempImgMat = new Mat();
                var alpha = new Mat();
                //Amúgy is fekete fehér nem? - most már biztos, mert már csak 1 csatornája van

                CvInvoke.CvtColor(overlayedImgMat, tempImgMat, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                //new PopupImage(tempImgMat, "tempImgMat").Show();
                //new PopupImage(alpha, "alpha").Show();
                CvInvoke.Threshold(tempImgMat, alpha, 100, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                matToReturn = Mat.Zeros(overlayedImgMat.Size.Width, overlayedImgMat.Size.Height, overlayedImgMat.Depth, overlayedImgMat.NumberOfChannels);
                // Multiply the foreground with the alpha matte
                CvInvoke.Multiply(alpha, tempImgMat, tempImgMat);
                //new PopupImage(tempImgMat, "tempImgMat_2").Show();
                // Multiply the background with ( 1 - alpha )
                //http://www.emgu.com/wiki/files/3.0.0/document/html/e3f8abb7-3706-0ccd-46f0-8c57c2232585.htm
                var tempScalarAlphaMat = new MCvScalar(1.0, 1.0, 1.0, 1.0) - alpha;
                //var tempScalarAlphaMat = new MCvScalar(1.0) - alpha;
                var tempScalarAlphaScalar = tempScalarAlphaMat;

                //CvInvoke.Multiply(tempScalarAlphaScalar, obscuredImgMat, obscuredImgMat);
                CvInvoke.Multiply(obscuredImgMat, tempScalarAlphaScalar, obscuredImgMat);
                CvInvoke.Add(tempImgMat, obscuredImgMat, matToReturn);
                //https://www.learnopencv.com/alpha-blending-using-opencv-cpp-python/
                //https://stackoverflow.com/questions/11958473/opencv-emgu-cv-compositing-images-with-alpha
                //https://github.com/karlphillip/GraphicsProgramming/blob/master/cvDisplacementMapFilter/main.cpp
                //https://stackoverflow.com/questions/45660427/emgu-c-sharp-opencv-make-color-black-transparent
                //https://www.learnopencv.com/alpha-blending-using-opencv-cpp-python/
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return matToReturn;
        }


        public void UpdateFolder(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            var attr = File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
            {
                return;
            }

            this.FolderName = path;
            this.Files = new List<string>(Directory.GetFiles(path, "*.jpg")).OrderBy(q => q).ToList();
            this.OpenedImgNumber = 0;
            this.UpdateImage();
        }

        public void NextImage()
        {
            if (this.OpenedImgNumber + 1 < this.Files.Count)
            {
                this.OpenedImgNumber++;
            }
            this.UpdateImage();
        }

        public void PreviousImage()
        {
            if (this.OpenedImgNumber - 1 >= 0)
            {
                this.OpenedImgNumber--;
            }
            this.UpdateImage();
        }

        public void SetAdaptiveThresholdConstant(int value) //http://www.learncsharptutorial.com/threadpooling-csharp-example.php
        {
            if (this.AdaptiveThresholdConstant == value)
            {
                return;
            }

            this.AdaptiveThresholdConstant = value;
            //this.loading = True
            //this.signals.image_processing_change.emit(True)
            //ThreadPool.QueueUserWorkItem(this.ImgProcessor);
            this.ImgProcessor.RunThread();
        }

        public void JumpToImage(int idx)
        {
            //If this is loading return????
            if (idx < 0 || idx >= this.Files.Count) return;
            this.OpenedImgNumber = idx;
            UpdateImage();
        }

        public void DrawCellContour(IInputOutputArray imgToMod,PointF point)
        {
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                InnerDrawMethod(imgToMod,tempVector);
                break;
            }
        }

        private void InnerDrawMethod(IInputOutputArray imgToMod, IInputArray tempVector)
        {
            var tempRect = CvInvoke.MinAreaRect(tempVector);
            var box = CvInvoke.BoxPoints(tempRect);
            var boxVec = new VectorOfPointF(box);
            CvInvoke.DrawContours(imgToMod, boxVec, 0, new MCvScalar(0,0,255),2);
            CvInvoke.PutText(imgToMod, tempRect.Size.Height.ToString(CultureInfo.InvariantCulture),
                new Point((int) (10 + box[0].X), (int) (10 + box[0].Y)),
                Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
        }

        public Mat DrawCellContourBoxToNewMat(PointF point, Mat imgToMod)
        {
            if (imgToMod == null) throw new ArgumentNullException(nameof(imgToMod));
            var returnMat = imgToMod.createNewHardCopyFromMat();
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                InnerDrawMethod(returnMat,tempVector);
                break;
            }
            return returnMat;
        }

        public void DrawAllCellContours()
        {
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                var box = CvInvoke.BoxPoints(tempRect);
                var boxVec = new VectorOfPointF(box);
                CvInvoke.PutText(ContourImage, tempRect.Size.Height.ToString(CultureInfo.InvariantCulture),
                    new Point((int)(10 + boxVec[0].X), (int)(10 + boxVec[0].Y)),
                    Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
            }
        }

        //Returns the length of a cell around a point
        public double GetCellLength(Point point)
        {
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                CvInvoke.BoxPoints(tempRect);
                return tempRect.Size.Height;
            }
            return -1.0;
        }

        public List<double> GetAllCellLength()
        {
            return (from contour in Contours.ToArrayOfArray() select new VectorOfPoint(contour) into tempVector select CvInvoke.MinAreaRect(tempVector) into tempRect select tempRect.Size.Height).Select(dummy => (double) dummy).ToList();
        }

        public List<(PointF, double)> GetAllCellLengthWithCenterPoint()
        {
            //return (from contour in Contours.ToArrayOfArray() select new VectorOfPoint(contour) into tempVector let moment = CvInvoke.Moments(tempVector) let cx = moment.M10 / moment.M00 let cy = moment.M01 / moment.M00 let tempPoint = new PointF((int) cx, (int) cy) let tempRect = CvInvoke.MinAreaRect(tempVector) select (tempRect.Size.Height, tempPoint)).Select(dummy => ((double, PointF)) dummy).ToList();
            return (from contour in Contours.ToArrayOfArray() select new VectorOfPoint(contour) into tempVector let moment = CvInvoke.Moments(tempVector) let cx = moment.M10 / moment.M00 let cy = moment.M01 / moment.M00 let tempPoint = new PointF((int)cx, (int)cy) let tempRect = CvInvoke.MinAreaRect(tempVector) select (tempPoint, tempRect.Size.Height)).Select(dummy => ((PointF,double))dummy).ToList();
        }


        public void SaveImgWithCountours(PointF point)
        {
            var imgToSave = this.Image;
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                CvInvoke.DrawContours(imgToSave, tempVector, 0, new MCvScalar(0, 255, 0), 2);
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                var box = CvInvoke.BoxPoints(tempRect);
                var boxVec = new VectorOfPointF(box);
                CvInvoke.DrawContours(imgToSave, boxVec, 0, new MCvScalar(0, 0, 255), 2);
                if (CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)
                {
                    CvInvoke.PutText(ContourImage, tempRect.Size.Height.ToString(CultureInfo.InvariantCulture),
                        new Point((int) (10 + boxVec[0].X), (int) (10 + boxVec[0].Y)),
                        Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
                }
            }

            var processedDir = this.FolderName + Path.DirectorySeparatorChar + "processed";
            if (!Directory.Exists(processedDir))
            {
                Directory.CreateDirectory(processedDir);
            }

            CvInvoke.Imwrite(processedDir + Path.DirectorySeparatorChar + this.OpenedImgNumber + ".jpg", imgToSave);
        }

        public Point ContourCenter(Point point)
        {
            var temp = new Point();
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                var moment = CvInvoke.Moments(tempVector);
                var cx = moment.M10 / moment.M00;
                var cy = moment.M01 / moment.M00;
                temp = new Point((int) cx, (int) cy);
                break;
            }

            return temp;
        }

        public string GetCurrentImgPath()
        {
            return  Path.Combine(this.FolderName, this.ImgName);
        }
    }
}
