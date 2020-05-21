using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Util;
using Microsoft.VisualBasic.CompilerServices;
using System.Threading;
using System.Windows;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;

namespace ASAP_WPF
{
    class Models
    {
        private string FolderName { get; set; }
        private string ImgName { get; set; }
        private List<String> Files { get; set; } //paths for the time being
        private int OpenedImgNumber { get; set; }
        private Mat Image { get; set; }
        private Mat ProcessedImage { get; set; }
        private List<VectorOfVectorOfPoint> Contours { get; set; }
        private Mat CountourImage { get; set; }

        private int AdaptiveThresholdConstant { get; set; }
        //sginals? https://softwareengineering.stackexchange.com/questions/142458/any-practical-alternative-to-the-signals-slots-model-for-gui-programming
        //threadpool System.Threading.ThreadPool;

        private ImageProcessor ImgProcessor { get; set; } //auto delete?? signalos baszás itt is

        public override string ToString()
        {
            return "ImageHandler, opened img:" + this.ImgName;
        }

        public Models()
        {
            FolderName = null;
            ImgName = null;
            Files = new List<string>();
            OpenedImgNumber = 0;
            Image = null;
            ProcessedImage = null;
            Contours = new List<VectorOfVectorOfPoint>();
            CountourImage = null;
            AdaptiveThresholdConstant = 5;
            ImgProcessor = new ImageProcessor();
        }

        public void UpdateImage()
        {
            if (this.Files.Count <= 0 || this.OpenedImgNumber < 0) return;
            this.ImgName = this.Files[OpenedImgNumber];
            //itt lehetne kísérletezni, hogy egyből két árnyalatos szürkével menjen, vagy a hisztogram generáláshoz több árnyalatos legyen
            this.Image = CvInvoke.Imread(this.ImgName, Emgu.CV.CvEnum.ImreadModes.ReducedGrayscale8);
            this.ImgProcessor.SetValues(this.Image, this.AdaptiveThresholdConstant);
            //this.isloading == ture???? ez valami signalos lehet
            //this.signals.image_provcessing_change.emit(True)
        }

        public void ProcessProcessedThread(List<object> _returnedList)
        {
            this.ProcessedImage = (Mat) _returnedList[0];
            this.Contours = (List<VectorOfVectorOfPoint>) _returnedList[1];
            this.CountourImage = (Mat) _returnedList[2];

            //this.loading = False
            //this.signals.image_processing_change.emit(False)
        }

        public void UpdateFolder(String _path)
        {
            var attr = File.GetAttributes(_path);
            if (null == _path && (attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return;
            }

            this.FolderName = _path;
            this.Files = new List<string>(Directory.GetFiles(_path)).OrderBy(q => q).ToList();
            this.OpenedImgNumber = 0;
            this.UpdateImage();
        }

        public void NextImage()
        {
            //if is loading return??? ezekre tényleg meg kell nézni a dolgokat
            if (this.OpenedImgNumber + 1 > this.Files.Count)
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
            foreach (var tempRect in from contour in Contours
                where CvInvoke.PointPolygonTest(contour, _point, true) >= 0
                select CvInvoke.MinAreaRect(contour))
            {
                PointF[] box = CvInvoke.BoxPoints(tempRect);
                VectorOfPointF boxVec = new VectorOfPointF(box);
                CvInvoke.PutText(CountourImage, tempRect.Size.Height.ToString(),
                    new System.Drawing.Point((int) (10 + box[0].X), (int) (10 + box[0].Y)),
                    Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
            }
        }

        //Returns the length of a cell around a point
        public double GetCellLength(PointF _point)
        {
            foreach (var tempRect in from contour in Contours
                where CvInvoke.PointPolygonTest(contour, _point, true) >= 0
                select CvInvoke.MinAreaRect(contour))
            {
                //ezek valszeg nem kellenek ide
                //PointF[] box = CvInvoke.BoxPoints(tempRect);
                //VectorOfPointF boxVec = new VectorOfPointF(box);
                return tempRect.Size.Height;
            }

            return -1.0;
        }

        public void SaveImgWithCountours(PointF _point)
        {
            Mat ImgToSave = this.Image;
            foreach (var contour in Contours)
            {
                CvInvoke.DrawContours(ImgToSave, contour, 0, new MCvScalar(0, 255, 0), 2);
                RotatedRect tempRect = CvInvoke.MinAreaRect(contour);
                PointF[] box = CvInvoke.BoxPoints(tempRect);
                VectorOfPointF boxVec = new VectorOfPointF(box);
                CvInvoke.DrawContours(ImgToSave, boxVec, 0, new MCvScalar(0, 0, 255), 2);
                if (CvInvoke.PointPolygonTest(contour, _point, true) >= 0)
                {
                    CvInvoke.PutText(CountourImage, tempRect.Size.Height.ToString(),
                        new System.Drawing.Point((int) (10 + box[0].X), (int) (10 + box[0].Y)),
                        Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.65, new MCvScalar(255, 100, 100, 255), 2);
                }
            }

            String processedDir = this.FolderName + Path.DirectorySeparatorChar + "processed";
            if (!Directory.Exists(processedDir))
            {
                Directory.CreateDirectory(processedDir);
            }

            CvInvoke.Imwrite(processedDir + Path.DirectorySeparatorChar + this.OpenedImgNumber + ".jpg", ImgToSave);
        }

        public Point ContourCenter(PointF _point)
        {
            Point temp = new Point();
            foreach (var contour in Contours)
            {
                if (!(CvInvoke.PointPolygonTest(contour, _point, true) >= 0)) continue;
                var moment = CvInvoke.Moments(contour);
                var cx = moment.M10 / moment.M00;
                var cy = moment.M01 / moment.M00;
                temp = new Point((int) cx, (int) cy);
                break;
            }

            return temp;
        }
    }
}
