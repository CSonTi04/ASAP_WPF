using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ASAP_WPF
{
    internal class ImageHandler
    {
        public string FolderName { get; set; }
        public string ImgName { get; set; }
        public List<string> Files { get; set; } //paths for the time being
        public int OpenedImgNumber { get; set; }
        public Mat Image { get; set; }
        public Mat ProcessedImage { get; set; }
        public VectorOfVectorOfPoint Contours { get; set; }
        public Mat CountourImage { get; set; }

        public Mat ProcessedImgWithCountourOverlay { get; set; }
        public Mat OgImgWithCountourOverlay { get; set; }

        public enum ModTypeEnum
        {
            Clahe,
            EqualizeHist
        }

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
            CountourImage = null;
            AdaptiveThresholdConstant = 5;
            ImgProcessor = new ImageProcessor();
        }

        public ImageHandler(String _path)
        {
            FolderName = null;
            ImgName = null;
            Files = new List<string>();
            OpenedImgNumber = 0;
            Image = null;
            ProcessedImage = null;
            Contours = new VectorOfVectorOfPoint();
            CountourImage = null;
            AdaptiveThresholdConstant = 5;
            ImgProcessor = new ImageProcessor();
            this.UpdateFolder(_path);
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
        }

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

        public void ProcessProcessedThread(List<object> _returnedList)
        {
            this.ProcessedImage = (Mat) _returnedList[0];
            this.Contours = (VectorOfVectorOfPoint) _returnedList[1];
            this.CountourImage = (Mat) _returnedList[2];

            //this.loading = False
            //this.signals.image_processing_change.emit(False)
        }

        //saját
        public void Process()
        {
            this.ProcessedImage = null;
            this.CountourImage = null;
            this.Contours.Clear();
            ImgProcessor.Process();
            this.ProcessedImage = ImgProcessor.ImageMat;
            this.CountourImage = ImgProcessor.ContourImageMat;
            this.Contours.Push(ImgProcessor.ContoursToReturn);
            //this.ProcessedImgWithCountourOverlay = OverlayImageMats(this.ProcessedImage, this.CountourImage);
            //this.OgImgWithCountourOverlay  = OverlayImageMats(this.Image, this.CountourImage);
        }

        public Mat OverlayImageMats(Mat obscuredImgMat, Mat overlayedImgMat)
        {
            var overlay_mask = overlayedImgMat;

            Emgu.CV.CvInvoke.CvtColor(overlay_mask, overlay_mask, Emgu.CV.CvEnum.ColorConversion.Gray2Bgr);
            //https://stackoverflow.com/questions/11958473/opencv-emgu-cv-compositing-images-with-alpha
            //https://github.com/karlphillip/GraphicsProgramming/blob/master/cvDisplacementMapFilter/main.cpp

            /*
            import numpy as np
            import cv2

            # ==============================================================================

            def blend_transparent(face_img, overlay_t_img):
            # Split out the transparency mask from the colour info
            overlay_img = overlay_t_img[:,:,:3] # Grab the BRG planes
            overlay_mask = overlay_t_img[:,:,3:]  # And the alpha plane

            # Again calculate the inverse mask
            background_mask = 255 - overlay_mask

            # Turn the masks into three channel, so we can use them as weights
            overlay_mask = cv2.cvtColor(overlay_mask, cv2.COLOR_GRAY2BGR)
            background_mask = cv2.cvtColor(background_mask, cv2.COLOR_GRAY2BGR)

            # Create a masked out face image, and masked out overlay
            # We convert the images to floating point in range 0.0 - 1.0
            face_part = (face_img * (1 / 255.0)) * (background_mask * (1 / 255.0))
            overlay_part = (overlay_img * (1 / 255.0)) * (overlay_mask * (1 / 255.0))

            # And finally just add them together, and rescale it back to an 8bit integer image
            return np.uint8(cv2.addWeighted(face_part, 255.0, overlay_part, 255.0, 0.0))

            # ==============================================================================

            # We load the images
            face_img = cv2.imread("lena.png", -1)
            overlay_t_img = cv2.imread("overlay_transparent.png", -1) # Load with transparency

            result_2 = blend_transparent(face_img, overlay_t_img)
            cv2.imwrite("merged_transparent.png", result_2)
             */
            Mat overlayedMat = null;

            return overlayedMat;
        }

        public void UpdateFolder(String _path)
        {
            var attr = File.GetAttributes(_path);
            if (null == _path && (attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return;
            }

            this.FolderName = _path;
            this.Files = new List<string>(Directory.GetFiles(_path, "*.jpg")).OrderBy(q => q).ToList();
            this.OpenedImgNumber = 0;
            this.UpdateImage("");
        }

        public void NextImage()
        {
            //if is loading return??? ezekre tényleg meg kell nézni a dolgokat
            if (this.OpenedImgNumber + 1 < this.Files.Count)
            {
                this.OpenedImgNumber++;
            }

            this.UpdateImage();
        }

        public void PreviousImage()
        {
            //if is loading return??? ezekre tényleg meg kell nézni a dolgokat
            if (this.OpenedImgNumber - 1 >= 0)
            {
                this.OpenedImgNumber--;
            }

            this.UpdateImage();
        }

        public void
            SetAdaptiveThresholdConstant(
                int _value) //http://www.learncsharptutorial.com/threadpooling-csharp-example.php
        {
            if (this.AdaptiveThresholdConstant == _value)
            {
                return;
            }

            this.AdaptiveThresholdConstant = _value;
            //this.loading = True
            //this.signals.image_processing_change.emit(True)
            //ThreadPool.QueueUserWorkItem(this.ImgProcessor);
            this.ImgProcessor.RunThread();
        }

        public void JumpToImage(int _idx)
        {
            //If this is loading return????
            if (_idx < 0 || _idx >= this.Files.Count) return;
            this.OpenedImgNumber = _idx;
            UpdateImage();
        }

        public void DrawCellCountours(PointF _point)
        {
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, _point, true) >= 0)) continue;
                InnerDrawMethod(tempVector);
                break;
            }
        }

        private void InnerDrawMethod(VectorOfPoint tempVector)
        {
            var tempRect = CvInvoke.MinAreaRect(tempVector);
            var box = CvInvoke.BoxPoints(tempRect);
            //var boxVec = new VectorOfPointF(box);
            CvInvoke.PutText(CountourImage, tempRect.Size.Height.ToString(),
                new System.Drawing.Point((int) (10 + box[0].X), (int) (10 + box[0].Y)),
                Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
        }

        public void DrawAllCellCountours()
        {
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                var box = CvInvoke.BoxPoints(tempRect);
                var boxVec = new VectorOfPointF(box);
                CvInvoke.PutText(CountourImage, tempRect.Size.Height.ToString(),
                    new System.Drawing.Point((int)(10 + boxVec[0].X), (int)(10 + boxVec[0].Y)),
                    Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
            }
        }

        //Returns the length of a cell around a point
        public double GetCellLength(Point _point)
        {
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, _point, true) >= 0)) continue;
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                var box = CvInvoke.BoxPoints(tempRect);
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


        public void SaveImgWithCountours(PointF _point)
        {
            var ImgToSave = this.Image;
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                CvInvoke.DrawContours(ImgToSave, tempVector, 0, new MCvScalar(0, 255, 0), 2);
                var tempRect = CvInvoke.MinAreaRect(tempVector);
                var box = CvInvoke.BoxPoints(tempRect);
                var boxVec = new VectorOfPointF(box);
                CvInvoke.DrawContours(ImgToSave, boxVec, 0, new MCvScalar(0, 0, 255), 2);
                if (CvInvoke.PointPolygonTest(tempVector, _point, true) >= 0)
                {
                    CvInvoke.PutText(CountourImage, tempRect.Size.Height.ToString(),
                        new System.Drawing.Point((int) (10 + boxVec[0].X), (int) (10 + boxVec[0].Y)),
                        Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
                }
            }

            var processedDir = this.FolderName + Path.DirectorySeparatorChar + "processed";
            if (!Directory.Exists(processedDir))
            {
                Directory.CreateDirectory(processedDir);
            }

            CvInvoke.Imwrite(processedDir + Path.DirectorySeparatorChar + this.OpenedImgNumber + ".jpg", ImgToSave);
        }

        public PointF ContourCenter(PointF _point)
        {
            PointF temp = new PointF();
            foreach (var contour in Contours.ToArrayOfArray())
            {
                var tempVector = new VectorOfPoint(contour);
                if (!(CvInvoke.PointPolygonTest(tempVector, _point, true) >= 0)) continue;
                var moment = CvInvoke.Moments(tempVector);
                var cx = moment.M10 / moment.M00;
                var cy = moment.M01 / moment.M00;
                temp = new PointF((int) cx, (int) cy);
                break;
            }

            return temp;
        }

        public string getCurrentImgPath()
        {
            return  Path.Combine(this.FolderName, this.ImgName);
        }
    }
}
