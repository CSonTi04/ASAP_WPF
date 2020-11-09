using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace ASAP_WPF
{
    public class ImageHandler
    {
        public string FolderName { get; set; }
        public string ImgName { get; set; }
        public List<string> Files { get; set; } //paths for the time being
        public int OpenedImgNumber { get; set; }
        public VectorOfVectorOfPoint Contours { get; set; }
        public VectorOfVectorOfPoint Boxes { get; set; }

        public int DetectedCellCount { get; set; }
        public Mat OgImage { get; set; }
        public Mat Image { get; set; }
        public Mat ProcessedImage { get; set; }
        public Mat ContourImage { get; set; }
        //public Mat BoxedImage { get; set; }
        public Mat ProcessedImgWithContourOverlay { get; set; }
        public Mat OgImgWithContourOverlay { get; set; }
        public Mat ImageToDisplayModifiedByMouseClick { get; set; }
        public Mat ImageToDisplay { get; set; }

        public Dictionary<VectorOfPoint, double> CellLengths { get; set; }

        public string[] AllowedFileExtensions = new[] {".jpg", ".tif"};
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
            return "ImageHandler, opened img:" + ImgName;
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
            Boxes = new VectorOfVectorOfPoint();
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
            Boxes = new VectorOfVectorOfPoint();
            ContourImage = null;
            AdaptiveThresholdConstant = 5;
            ImgProcessor = new ImageProcessor();
            UpdateFolder(path);
        }



        public void UpdateImage()
        {
            if (Files.Count <= 0 || OpenedImgNumber < 0) return;
            ImgName = Files[OpenedImgNumber];
            //itt lehetne kísérletezni, hogy egyből két árnyalatos szürkével menjen, vagy a hisztogram generáláshoz több árnyalatos legyen
            //tényleg meg kellene nézni, hogy ennek van -e effektje a kimenetelre
            Image = CvInvoke.Imread(ImgName, ImreadModes.ReducedGrayscale8);
            ImgProcessor.SetValues(Image, AdaptiveThresholdConstant);
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

        public Mat GetImageToDisplay(MainWindow.ImageToDisplay ImgToDisplayEnum)
        {
            ImageToDisplay = ImgToDisplayEnum switch
            {
                MainWindow.ImageToDisplay.None => null,
                MainWindow.ImageToDisplay.Original => Image,
                MainWindow.ImageToDisplay.Processed => ProcessedImage,
                MainWindow.ImageToDisplay.OriginalWithOverlay => OgImgWithContourOverlay,
                MainWindow.ImageToDisplay.ProcessedWithOverlay => ProcessedImgWithContourOverlay,
                MainWindow.ImageToDisplay.PictureModifiedByClick => ImageToDisplayModifiedByMouseClick,
                _ => throw new ArgumentOutOfRangeException(nameof(ImgToDisplayEnum), ImgToDisplayEnum, null)
            };
            return ImageToDisplay;
        }
        //saját
        public void Process()
        {
            //TODO ezeknek a refeknek az elhagyása
            ProcessedImage = null;
            ContourImage = null;
            Contours.Clear();
            ImgProcessor.Process();
            OgImage = ImgProcessor.OgImageMat;
            ProcessedImage = ImgProcessor.ImageMat;
            ContourImage = ImgProcessor.ContourImageMat;
            Contours.Push(ImgProcessor.ContoursToReturn);
            Boxes.Push(ImgProcessor.AngledBoundingBoxesToReturn);
            DetectedCellCount = Contours.Size;
            //BoxedImage = DrawAllCellContourBoundingBoxes();
            MainWindow.ImageProcessorExaminer.AddImage(OgImage.CreateNewHardCopyFromMat(), "ImageHandler_OgImage");
            MainWindow.ImageProcessorExaminer.AddImage(Image.CreateNewHardCopyFromMat(), "ImageHandler_Image");
            MainWindow.ImageProcessorExaminer.AddImage(ProcessedImage.CreateNewHardCopyFromMat(), "ImageHandler_ProcessedImage");
            MainWindow.ImageProcessorExaminer.AddImage(ContourImage.CreateNewHardCopyFromMat(), "ImageHandler_ContourImage");
            //MainWindow.ImageProcessorExaminer.AddImage(BoxedImage.createNewHardCopyFromMat(), "ImageHandler_BoxedImage");
            ProcessOverlays();
        }

        public void ProcessOverlays()
        {
            //Ide kell majd a single cell contourt átdobni
            //var processedImgWithSingleCellContourBox = DrawCellContourBoxToNewMat(lastClickedPoint, this.ProcessedImage);
            //MainWindow.ImageProcessorExaminer.AddImage(processedImgWithSingleCellContourBox.createNewHardCopyFromMat(), "processedImgWithSingleCellContourBox");
            //var ogImgWithSingleCellContourBox = DrawCellContourBoxToNewMat(lastClickedPoint, this.Image);
            ///MainWindow.ImageProcessorExaminer.AddImage(ogImgWithSingleCellContourBox.createNewHardCopyFromMat(), "ogImgWithSingleCellContourBox");
            //this.ProcessedImgWithContourOverlay = OverlayImageMats(processedImgWithSingleCellContourBox, processedImgWithSingleCellContourBox);
            //MainWindow.ImageProcessorExaminer.AddImage(ProcessedImgWithContourOverlay.createNewHardCopyFromMat(), "ProcessedImgWithContourOverlay");
            //this.OgImgWithContourOverlay  = OverlayImageMats(ogImgWithSingleCellContourBox, ogImgWithSingleCellContourBox);
            //MainWindow.ImageProcessorExaminer.AddImage(OgImgWithContourOverlay.createNewHardCopyFromMat(), "OgImgWithContourOverlay");

            //this.ProcessedImgWithContourOverlay = OverlayImageMats(this.ProcessedImage, this.BoxedImage);
            //MainWindow.ImageProcessorExaminer.AddImage(ProcessedImgWithContourOverlay.createNewHardCopyFromMat(), "ProcessedImgWithContourOverlay");
            //this.OgImgWithContourOverlay  = OverlayImageMats(this.Image, this.BoxedImage);
            //MainWindow.ImageProcessorExaminer.AddImage(OgImgWithContourOverlay.createNewHardCopyFromMat(), "OgImgWithContourOverlay");

            ProcessedImgWithContourOverlay = DrawAllCellContourBoundingBoxes(ProcessedImage);
            MainWindow.ImageProcessorExaminer.AddImage(ProcessedImgWithContourOverlay.CreateNewHardCopyFromMat(), "ProcessedImgWithContourOverlay");
            OgImgWithContourOverlay = DrawAllCellContourBoundingBoxes(Image);
            MainWindow.ImageProcessorExaminer.AddImage(OgImgWithContourOverlay.CreateNewHardCopyFromMat(), "OgImgWithContourOverlay");
        }

        public Mat OverlayImageMats(Mat paramObscuredImgMat, Mat paramOverlayedImgMat)
        {

            var obscuredImgMat = paramObscuredImgMat.CreateNewHardCopyFromMat();
            var overlayImgMat = paramOverlayedImgMat.CreateNewHardCopyFromMat();
            var matToReturn = new Mat();
            try
            {
                //Valami azt súgja, hogy
                //https://answers.opencv.org/question/24463/how-to-remove-black-background-from-grabcut-output-image-in-opencv-android/?comment=24786#comment-24786
                //Ez kell majd
                var tempImgMat = new Mat();
                var alpha = new Mat();
                //Amúgy is fekete fehér nem? - most már biztos, mert már csak 1 csatornája van

                if (overlayImgMat.NumberOfChannels > 1)
                {
                    CvInvoke.CvtColor(overlayImgMat, tempImgMat, ColorConversion.Bgr2Gray);
                }
                else
                {
                    tempImgMat = overlayImgMat.CreateNewHardCopyFromMat();
                }
                //new PopupImage(tempImgMat, "tempImgMat").Show();
                //new PopupImage(alpha, "alpha").Show();
                CvInvoke.Threshold(tempImgMat, alpha, 100, 255, ThresholdType.Binary);
                matToReturn = Mat.Zeros(overlayImgMat.Size.Width, overlayImgMat.Size.Height, overlayImgMat.Depth, overlayImgMat.NumberOfChannels);
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

            FolderName = path;
            //Files = new List<string>(Directory.GetFiles(path, "*.jpg")).OrderBy(q => q).ToList();
            Files = new List<string>(Directory.GetFiles(path).Where(file => AllowedFileExtensions.Any(file.ToLower().EndsWith))).OrderBy(q => q).ToList();
            OpenedImgNumber = 0;
            UpdateImage();
        }

        public void NextImage()
        {
            if (OpenedImgNumber + 1 < Files.Count)
            {
                OpenedImgNumber++;
            }
            UpdateImage();
        }

        public void PreviousImage()
        {
            if (OpenedImgNumber - 1 >= 0)
            {
                OpenedImgNumber--;
            }
            UpdateImage();
        }

        public void SetAdaptiveThresholdConstant(int value) //http://www.learncsharptutorial.com/threadpooling-csharp-example.php
        {
            if (AdaptiveThresholdConstant == value)
            {
                return;
            }

            AdaptiveThresholdConstant = value;
            //this.loading = True
            //this.signals.image_processing_change.emit(True)
            //ThreadPool.QueueUserWorkItem(this.ImgProcessor);
            ImgProcessor.RunThread();
        }

        public void JumpToImage(int idx)
        {
            //If this is loading return????
            if (idx < 0 || idx >= Files.Count) return;
            OpenedImgNumber = idx;
            UpdateImage();
        }

        public void DrawCellContour(Mat imgToMod,PointF point)
        {
            /*
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                //InnerDrawMethod(imgToMod,tempVector);
                break;
            }*/
        }

        public void DrawSelectedCellContourBoxToImageToDisplay(Point point)
        {
            this.ImageToDisplayModifiedByMouseClick = DrawSelectedCellContourBoxToNewMat(point,this.ImageToDisplay);
        }

        private void DrawSelectedCellContourBoxToMat(Mat imgToMod, VectorOfPoint tempVector)
        {
            var matToReturn = imgToMod.CreateNewHardCopyFromMat();
            if (matToReturn.NumberOfChannels < 3)
            {
                CvInvoke.CvtColor(matToReturn, matToReturn, ColorConversion.Gray2Bgr);
            }


            var boxVecOfVectorPoint = new VectorOfVectorOfPointF();
            var tempRect = CvInvoke.MinAreaRect(tempVector);
            var box = CvInvoke.BoxPoints(tempRect);
            var boxVec = new VectorOfPointF(box);
            boxVecOfVectorPoint.Push(boxVec);
            var convertedVectorOfVectorPoint = boxVecOfVectorPoint.ConvertToVectorOfPoint();

            CvInvoke.DrawContours(matToReturn, convertedVectorOfVectorPoint, -1, new MCvScalar(0, 114, 251, 0), 3);
            //CvInvoke.DrawContours(matToReturn, tempVector, -1, new MCvScalar(0, 114, 251, 255), 3);
            //return matToReturn;
            /*
            var tempRect = CvInvoke.MinAreaRect(tempVector);
            var box = CvInvoke.BoxPoints(tempRect);
            var boxVec = new VectorOfPointF(box);
            CvInvoke.DrawContours(imgToMod, boxVec, 0, new MCvScalar(0,0,255),2);
            CvInvoke.PutText(imgToMod, tempRect.Size.Height.ToString(CultureInfo.InvariantCulture),
                new Point((int) (10 + box[0].X), (int) (10 + box[0].Y)),
                FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
            MainWindow.ImageProcessorExaminer.AddImage(imgToMod.createNewHardCopyFromMat(), "DrawSelectedCellContourBoxToMat");
            */

        }

        public Mat DrawSelectedCellContourBoxToNewEmptyMat(Point point, Mat imgToMimic)
        {
            if (imgToMimic == null) throw new ArgumentNullException(nameof(imgToMimic));
            var tempVectorOfPoint = GetContourForGivenPoint(point);
            var returnMat = imgToMimic.CreateNewMatLikeThis();
            DrawSelectedCellContourBoxToMat(returnMat, tempVectorOfPoint);
            /*
            var returnMat = imgToMod.createNewMatLikeThis();
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                DrawSelectedCellContourBoxToMat(returnMat,tempVector);
                break;
            }
            return returnMat;
            */
            return returnMat;
        }

        public Mat DrawSelectedCellContourBoxToNewMat(Point point, Mat imgToMod)
        {
            if (imgToMod == null) throw new ArgumentNullException(nameof(imgToMod));
            //var tempVectorOfPoint = GetContourForGivenPoint(point);
            var tempVectorOfPoint = GetBoundingBox(point);
            var returnMat = imgToMod.CreateNewHardCopyFromMat();
            DrawSelectedCellContourBoxToMat(returnMat, tempVectorOfPoint);
            MainWindow.ImageProcessorExaminer.AddImage(returnMat.CreateNewHardCopyFromMat(), "DrawSelectedCellContourBoxToNewMat");
            /*
            var returnMat = imgToMod.createNewMatLikeThis();
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                DrawSelectedCellContourBoxToMat(returnMat,tempVector);
                break;
            }
            return returnMat;
            */
            return returnMat;
        }

        private VectorOfPoint GetContourForGivenPoint(Point point)
        {
            var tempVectorList = new List<VectorOfPoint>();
            VectorOfPoint tempVectorOfPoint = null;
            var contArray = Contours.ToArrayOfArray();
            foreach (var contour in contArray)
            {
                tempVectorOfPoint = new VectorOfPoint(contour);
                if ((CvInvoke.PointPolygonTest(tempVectorOfPoint, point, true) >= 0))
                {
                    tempVectorList.Add(tempVectorOfPoint);
                }
            }

            //if (tempVectorList.Count > 1) throw new Exception("Point is inside of more than one contour!");
            //if (tempVectorList.Count == 0) throw new Exception("Point is not inside registered contours!");
            return tempVectorOfPoint;
        }

        public void DrawAllCellContourSizes()
        {
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                var box = CvInvoke.BoxPoints(tempRect);
                var boxVec = new VectorOfPointF(box);
                CvInvoke.PutText(ContourImage, tempRect.Size.Height.ToString(CultureInfo.InvariantCulture),
                    new Point((int)(10 + boxVec[0].X), (int)(10 + boxVec[0].Y)),
                    FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
            }
        }

        public Mat DrawAllCellContourBoundingBoxes(Mat imgToMod)
        {
            var matToReturn = imgToMod.CreateNewHardCopyFromMat();
            CvInvoke.CvtColor(matToReturn, matToReturn, ColorConversion.Gray2Bgr);
            var boxVecOfVectorPoint = new VectorOfVectorOfPointF();
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                var box = CvInvoke.BoxPoints(tempRect);
                var boxVec = new VectorOfPointF(box);
                boxVecOfVectorPoint.Push(boxVec);
            }

            var convertedVectorOfVectorPoint = boxVecOfVectorPoint.ConvertToVectorOfPoint();
            CvInvoke.DrawContours(matToReturn, convertedVectorOfVectorPoint, -1, new MCvScalar(0, 255, 0, 255), 2);
            return matToReturn;
        }

        public Mat DrawAllCellContourBoundingBoxes()
        {
            var matToReturn = ContourImage.CreateNewMatLikeThis();
            var boxVecOfVectorPoint = new VectorOfVectorOfPointF();
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                var box = CvInvoke.BoxPoints(tempRect);
                var boxVec = new VectorOfPointF(box);
                boxVecOfVectorPoint.Push(boxVec);

            }
            //CvInvoke.DrawContours(matToReturn, boxVecOfVectorPoint, 0, new MCvScalar(0, 255, 0, 255), 2);
            //contourIdx	Parameter indicating a contour to draw. If it is negative, all the contours are drawn.
            //
            //var tempMat = boxVecOfVectorPoint.GetInputOutputArray().GetMat();

            var convertedVectorOfVectorPoint = boxVecOfVectorPoint.ConvertToVectorOfPoint();
            CvInvoke.DrawContours(matToReturn, convertedVectorOfVectorPoint, -1, new MCvScalar(0, 255, 0, 255), 2);
            return matToReturn;
        }

        public void PrintAllTypeOfCellLengthToDebug(Point point)
        {
            //var msg = "";
            //msg.Concat("Contour: ").Concat(GetCellLengthWithContour(point).ToString(CultureInfo.InvariantCulture)).Concat(Environment.NewLine);
            //msg.Concat("BoundingBox: ").Concat(GetCellLengthWithBoundingBox(point).ToString(CultureInfo.InvariantCulture)).Concat(Environment.NewLine);
            //msg.Concat("BoundingBoxPoint: ").Concat(GetCellLengthWithBoundingBoxPoint(point).ToString(CultureInfo.InvariantCulture)).Concat(Environment.NewLine);
            //msg.Concat("EnclosingCircle: ").Concat(GetCellLengthWithEnclosingCircle(point).ToString(CultureInfo.InvariantCulture)).Concat(Environment.NewLine);
            //Debug.WriteLine(msg);
            var msg = new StringBuilder("");
            msg.Append("Contour: ").Append(GetCellLengthWithContour(point).ToString(CultureInfo.InvariantCulture)).Append(Environment.NewLine);
            msg.Append("BoundingBox: ").Append(GetCellLengthWithBoundingBox(point).ToString(CultureInfo.InvariantCulture)).Append(Environment.NewLine);
            msg.Append("BoundingBoxPoint: ").Append(GetCellLengthWithBoundingBoxPoint(point).ToString(CultureInfo.InvariantCulture)).Append(Environment.NewLine);
            msg.Append("EnclosingCircle: ").Append(GetCellLengthWithEnclosingCircle(point).ToString(CultureInfo.InvariantCulture)).Append(Environment.NewLine);
            Debug.WriteLine(msg.ToString());
        }


        //Returns the length of a cell around a point
        public double GetCellLengthWithContour(Point point)
        {
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                //CvInvoke.BoxPoints(tempRect);
                return tempRect.Size.Height;
            }
            return -1.0;
        }

        public double GetCellLengthWithBoundingBox(Point point)
        {
            foreach (var contour in Boxes.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                //var boxPoints = CvInvoke.BoxPoints(tempRect);
                return tempRect.Size.Height;
            }
            return -1.0;
        }

        public double GetCellLengthWithBoundingBoxPoint(Point point)
        {
            foreach (var contour in Boxes.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                var boxPoints = CvInvoke.BoxPoints(tempRect);
                return GetCellLengthFromBoxPoints(boxPoints);
            }
            return -1.0;
        }

        public double GetCellLengthWithBoundingBoxPoint(VectorOfPoint contour)
        {
            var tempRect = CvInvoke.MinAreaRect(contour);
            var boxPoints = CvInvoke.BoxPoints(tempRect);
            return GetCellLengthFromBoxPoints(boxPoints);
        }

        public double GetCellLengthWithEnclosingCircle(Point point)
        {
            var tempVectorOfVector = Contours.ConvertToVectorOfPoint();
            foreach (var contour in tempVectorOfVector.ToArrayOfArray())
            {
                var tempVector = new VectorOfPointF(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                var tempCircle = CvInvoke.MinEnclosingCircle(contour);
                return tempCircle.Radius * 2.0;
            }
            return -1.0;
        }

        public double GetCellLengthFromBoxPoints(PointF[] boxPoints)
        {
            var length = -1.0;
            var lengthSet = new SortedSet<double>();
            var listOfPoint = new List<PointF>(boxPoints);
            //var cartesianListOfPointPairs = new List<(PointF,PointF)>();
            //var crossJoinedListOfPointPairs = from x in listOfPoint from y in listOfPoint select new  { x, y };

            //foreach (var currentDistance in crossJoinedListOfPointPairs.ToList().Select(pair => CalculateDistance(pair.x,pair.y)))
            //{
                //lengthSet.Add(currentDistance);
            //}

            foreach (var currentDistance in listOfPoint.Select(currentPoint => CalculateDistance(listOfPoint[0], currentPoint)))
            {
                lengthSet.Add(currentDistance);
            }

            length = lengthSet.ToList()[2];
            return length;
        }

        private static double CalculateDistance(PointF a, PointF b)
        {
            var distance = -1.0;

            distance = Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));

            return distance;
        }

        public VectorOfPoint GetBoundingBox(Point point)
        {
            VectorOfPoint returnVector = null;

            foreach (var contour in Boxes.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                returnVector = tempVector;
            }

            return returnVector;
        }

        public VectorOfPoint GetContour(Point point)
        {
            VectorOfPoint returnVector = null;

            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, point, true) >= 0)) continue;
                returnVector = tempVector;
            }

            return returnVector;
        }

        public List<double> GetAllCellLength()
        {
            return (from contour in Contours.ToArrayOfArray() select new VectorOfPoint(contour) into tempVector select CvInvoke.MinAreaRect(tempVector) into tempRect select tempRect.Size.Height).Select(dummy => (double) dummy).ToList();
        }

        public List<(Point, double)> GetAllCellLengthWithCenterPoint()
        {
            //return (from contour in Contours.ToArrayOfArray() select new VectorOfPoint(contour) into tempVector let moment = CvInvoke.Moments(tempVector) let cx = moment.M10 / moment.M00 let cy = moment.M01 / moment.M00 let tempPoint = new PointF((int) cx, (int) cy) let tempRect = CvInvoke.MinAreaRect(tempVector) select (tempRect.Size.Height, tempPoint)).Select(dummy => ((double, PointF)) dummy).ToList();
            return (from contour in Contours.ToArrayOfArray() select new VectorOfPoint(contour) into tempVector let moment = CvInvoke.Moments(tempVector) let cx = moment.M10 / moment.M00 let cy = moment.M01 / moment.M00 let tempPoint = new Point((int)cx, (int)cy) let tempRect = CvInvoke.MinAreaRect(tempVector) select (tempPoint, tempRect.Size.Height)).Select(dummy => ((Point,double))dummy).ToList();
        }


        public void SaveImgWithContours(Point point)
        {
            var imgToSave = Image;
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
                        FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
                }
            }

            var processedDir = FolderName + Path.DirectorySeparatorChar + "processed";
            if (!Directory.Exists(processedDir))
            {
                Directory.CreateDirectory(processedDir);
            }

            CvInvoke.Imwrite(processedDir + Path.DirectorySeparatorChar + OpenedImgNumber + ".jpg", imgToSave);
        }

        public Point GetContourCenterPoint(Point point)
        {
            /*
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
            */

            var tempVector = GetContourForGivenPoint(point);
            var moment = CvInvoke.Moments(tempVector);
            var cx = moment.M10 / moment.M00;
            var cy = moment.M01 / moment.M00;
            var tempPoint = new Point((int)cx, (int)cy);
            return tempPoint;
        }



        public string GetCurrentImgPath()
        {
            return  Path.Combine(FolderName, ImgName);
        }


        private void CalculateCellLengths(VectorOfPoint contour)
        {
            //TODO ez miért is van itt?

            var roiRectangle = CvInvoke.BoundingRectangle(contour);
            //var roiMat = new Mat(this.ContourImageMat, roiRectangle);//var leftHalf = new Mat(matToSlice, leftHalfRect);
            var roiMat = new Mat(this.ContourImage, roiRectangle);
            CvInvoke.CvtColor(roiMat, roiMat, ColorConversion.Bgra2Gray);
            var tempRect = CvInvoke.MinAreaRect(contour);
            var rotatedRoiMat = roiMat.RotateMatWithoutCutoff(tempRect);
            //var pointPair = roiMat.GetPointsOfWidestSliceOfCell();
            //var sizeInPx = Math.Sqrt(Math.Pow(pointPair.Item2.X - pointPair.Item1.X, 2) + Math.Pow(pointPair.Item2.Y - pointPair.Item2.Y, 2));
            var sizeInPx = rotatedRoiMat.GetWidestSliceOfCellLengthInPx();
            if (!CellLengths.ContainsKey(contour))
            {
                CellLengths.Add(contour, sizeInPx);
            }
            else
            {
                CellLengths.Remove(contour);
                CellLengths.Add(contour, sizeInPx);
            }
        }

        public double CalculateCellLength(VectorOfPoint contour)
        {
            if (null == contour) return Double.NaN;

            //TODO hierarchia kihasználása, hogy a nagyobb kontúr meg legyen, ne a doboz növelésével
            var roiRectangle = CvInvoke.BoundingRectangle(contour);
            var ogSize = roiRectangle.Size;
            var ogPoint = roiRectangle.Location;

            //ogSize.Width > ogSize.Height ? ogSize.Width * Math.Sqrt(2) : ogSize.Height * Math.Sqrt(2);
            var roiUpScale = Math.Sqrt(2);

            var newSize = new Size((int) (ogSize.Width * roiUpScale) , (int)(ogSize.Height * roiUpScale));

            var diffWidth = newSize.Width - ogSize.Width;
            var diffHeight = newSize.Height - ogSize.Height;
            var newPoint = new Point(ogPoint.X - diffWidth / 2, ogPoint.Y - diffHeight / 2);
            var newRoiRectangle = new Rectangle(newPoint,newSize);
            //var roiMat = new Mat(this.ContourImageMat, roiRectangle);//var leftHalf = new Mat(matToSlice, leftHalfRect);
            var roiMat = new Mat(this.ProcessedImage, roiRectangle);
            var newRoiMat = new Mat(this.ProcessedImage,newRoiRectangle);
            var roiMatOgPic = new Mat(this.OgImage, roiRectangle);
            var newRoiMatOgPic = new Mat(this.OgImage, newRoiRectangle);
            if (roiMat.NumberOfChannels > 1)
            {
                CvInvoke.CvtColor(roiMat, roiMat, ColorConversion.Bgra2Gray);
            }
            MainWindow.ImageProcessorExaminer.AddImage(roiMat.CreateNewHardCopyFromMat(), "CalculateCellLength_roiMat");
            MainWindow.ImageProcessorExaminer.AddImage(newRoiMat.CreateNewHardCopyFromMat(), "CalculateCellLength_newRoiMat");
            MainWindow.ImageProcessorExaminer.AddImage(roiMatOgPic.CreateNewHardCopyFromMat(), "CalculateCellLength_roiMatOgPic");
            MainWindow.ImageProcessorExaminer.AddImage(newRoiMatOgPic.CreateNewHardCopyFromMat(), "CalculateCellLength_newRoiMatOgPic");
            var pixelNum = CvInvoke.CountNonZero(roiMat);
            if (pixelNum < 1) throw new Exception("Selected ROI is blank!");
            //var tempRect = CvInvoke.MinAreaRect(contour);
            //var rotatedRoiMat = newRoiMat.RotateMat(contour);
            //var rotatedRoiMatAlt = newRoiMat.RotateMat(contour, 90.0);

            var rotatedRoiMat = newRoiMat.RotMat(contour);
            var rotatedRoiMatOgPic = newRoiMatOgPic.RotMatGreyscale(contour,0.0);

            //var newGrayScaleContour = rotatedRoiMatOgPic.DetectBiggestCellInMat();
            var newGrayScaleContour = rotatedRoiMatOgPic.DetectSmallestCellInMat();
            rotatedRoiMatOgPic.GetContourIntensitySet(newGrayScaleContour);

            //var rotatedRoiMatAlt = newRoiMat.RotMat(contour, 90.0);

            MainWindow.ImageProcessorExaminer.AddImage(rotatedRoiMat.CreateNewHardCopyFromMat(), "CalculateCellLength_rotatedRoiMat");
            MainWindow.ImageProcessorExaminer.AddImage(rotatedRoiMatOgPic.CreateNewHardCopyFromMat(), "CalculateCellLength_rotatedRoiMatOgPic");
            //MainWindow.ImageProcessorExaminer.AddImage(rotatedRoiMatAlt.CreateNewHardCopyFromMat(), "CalculateCellLength_rotatedRoiMatAlt");
            //var pointPair = roiMat.GetPointsOfWidestSliceOfCell();
            //var sizeInPx = Math.Sqrt(Math.Pow(pointPair.Item2.X - pointPair.Item1.X, 2) + Math.Pow(pointPair.Item2.Y - pointPair.Item2.Y, 2));
            var sizeInPx = rotatedRoiMat.GetWidestSliceOfCellLengthInPx();
            var sizeEndPoint = rotatedRoiMat.GetPointsOfWidestSliceOfCellSegmented();

            var newContours = rotatedRoiMat.DetectCellContoursInMat();


            var sizeInPxWithContours = rotatedRoiMat.GetWidestSliceOfCellLengthInPxWithContours(newContours[1],newContours[0]);
            var sizeEndPointWithContours = rotatedRoiMat.GetPointsOfWidestSliceOfCellSegmentedWithContours(newContours[1], newContours[0]);


            //var newContour = rotatedRoiMat.DetectBiggestCellInMat();
            //var newContour = rotatedRoiMat.DetectBiggestCellInMat();
            var newContour = rotatedRoiMat.DetectSmallestCellInMat();


            //TEST
            //GetBiggestAreaOfCellWithSlidingWindowAndContourSegmented
            //var test = new Mat(5, 5, rotatedRoiMat.Depth, rotatedRoiMat.NumberOfChannels);
            //test.SetValue(0, 0, (byte)1);
            //test.SetValue(1, 0, (byte)1);
            //test.SetValue(2, 0, (byte)1);
            //test.SetValue(3, 0, (byte)1);
            //test.SetValue(4, 0, (byte)1);
            //test.GetBiggestAreaOfCellWithSlidingWindowAndContourSegmented(1, newContour);

            var sizeInPxWithContour = rotatedRoiMat.GetWidestSliceOfCellLengthInPxWithContour(newContour);
            var sizeEndPointWithContour = rotatedRoiMat.GetPointsOfWidestSliceOfCellSegmentedWithContour(newContour);
            //var sizeInPxOgPic = rotatedRoiMatOgPic.GetWidestSliceOfCellLengthInPx();
            //var sizeEndPointOgPic = rotatedRoiMatOgPic.GetPointsOfWidestSliceOfCell();
            var angledBoundingRectangle = CvInvoke.MinAreaRect(contour);

            //The order is bottomLeft, topLeft, topRight, bottomRight.
            //Remélhetőleg ez tényleg így van




            // ez mindig a legalsó pontot fogja elöszőpr visszaadni aztán onnan kezd majd el az óramutat járásáával ellentétesen végig menni rajtuk


            //var ogBBoxPoints = angledBoundingRectangle.GetVertices().ReorderBoxPoints();
            //var ogBBoxPoints = angledBoundingRectangle.GetVertices();
            //var ogBBoxSize = angledBoundingRectangle.Size;

            //var bBoxPoints = CvInvoke.MinAreaRect(newContour).GetVertices().ReorderBoxPoints();
            //var refBBoxPoints = CvInvoke.MinAreaRect(newContour).GetVertices();
            //var refBBoxSize = CvInvoke.MinAreaRect(newContour).Size;

            //ogBBoxPoints = ogBBoxPoints.RotatePointsUntilLengthsAreSame(refBBoxPoints);

            //var rotationMat = newRoiMat.RotMatOnly(contour,0);

            //var locTriangleOne = new LocatorTriangle(refBBoxPoints[0], refBBoxPoints[1],sizeEndPoint.Item1);
            //locTriangleOne.CalculateNewPPosition(ogBBoxPoints[0], ogBBoxPoints[1]);
            //locTriangleOne.CalculateNewPPositionReverseAffine(rotationMat);
            //var locTriangleTwo = new LocatorTriangle(refBBoxPoints[2], refBBoxPoints[3], sizeEndPoint.Item2);
            //locTriangleTwo.CalculateNewPPosition(ogBBoxPoints[2], ogBBoxPoints[3]);
            //locTriangleTwo.CalculateNewPPositionReverseAffine(rotationMat);

            //var points = new VectorOfPointF();
            //points.Push(new PointF[]{ locTriangleOne.TransformedPointPMinus, locTriangleOne.TransformedPointPPlus , locTriangleTwo.TransformedPointPMinus, locTriangleTwo.TransformedPointPPlus });
            //points.Push(new PointF[] {  locTriangleOne.TransformedPointPPlus, locTriangleTwo.TransformedPointPPlus });
            Debug.Print(ImgName);
            Debug.Print(contour.GetContourCenterPoint().ToString());
            //First
            var firstPoints = ExtensionMethods.GetDiffractionBandHalvingPointsToDrawBack(sizeEndPoint,contour,newContour);
            this.ImageToDisplay = this.ImageToDisplay.DrawColorPixelsToMat(firstPoints.ConvertToVectorOfPoint(), new MCvScalar(0,0,255));
            Debug.Print("FirstMethod");
            Debug.Print(firstPoints.ToPrintableString());
            //Second
            var secondPoints = ExtensionMethods.GetDiffractionBandHalvingPointsToDrawBack(sizeEndPointWithContour, contour, newContour);
            this.ImageToDisplay = this.ImageToDisplay.DrawColorPixelsToMat(secondPoints.ConvertToVectorOfPoint(), new MCvScalar(0, 255, 0));
            Debug.Print("SecondMethod");
            Debug.Print(secondPoints.ToPrintableString());
            //Third
            var thirdPoints = ExtensionMethods.GetDiffractionBandHalvingPointsToDrawBack(sizeEndPointWithContours, contour, newContour);
            this.ImageToDisplay = this.ImageToDisplay.DrawColorPixelsToMat(thirdPoints.ConvertToVectorOfPoint(), new MCvScalar(255, 0, 0));
            Debug.Print("ThirdMethod");
            Debug.Print(thirdPoints.ToPrintableString());
            //var sizeInPxAlt = rotatedRoiMatAlt.GetWidestSliceOfCellLengthInPX();

            //var sizeInPx = -1;
            //var sizeInPxAlt = -1;
            //var valueToReturn = sizeInPx > sizeInPxAlt ? sizeInPx : sizeInPxAlt;
            //var sizeInPx = - 1;
            return sizeInPx;
        }
    }
}
