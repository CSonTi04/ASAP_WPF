using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using System.Linq.Dynamic.Core;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Forms;

namespace ASAP_WPF
{
    internal static class ExtensionMethods
    {
        private static byte WHITE_PIXEL = 255;
        private static byte BLACK_PIXEL = 0;
        private static double WHITE_TO_BLACK_RATIO = 0.65;

        public static Mat CreateNewHardCopyFromMat(this Mat matToHardCopy)
        {
            var returnMat = new Mat(matToHardCopy.Rows, matToHardCopy.Cols, matToHardCopy.Depth,
                matToHardCopy.NumberOfChannels);
            matToHardCopy.CopyTo(returnMat);
            return returnMat;
        }

        public static Mat CreateNewMatLikeThis(this Mat matToHardCopy)
        {
            var returnMat = new Mat(matToHardCopy.Rows, matToHardCopy.Cols, matToHardCopy.Depth,
                matToHardCopy.NumberOfChannels);
            return returnMat;
        }

        public static VectorOfVectorOfPoint ConvertToVectorOfPoint(this VectorOfVectorOfPointF vectorToTransform)
        {
            var vectorToReturn = new VectorOfVectorOfPoint();
            var vectorArrayOfArray = vectorToTransform.ToArrayOfArray();
            foreach (var vector in vectorArrayOfArray)
            {
                var temp = new VectorOfPoint();
                var pointList = vector.Select(pointF => new Point((int) pointF.X, (int) pointF.Y)).ToList();
                temp.Push(pointList.ToArray());
                vectorToReturn.Push(temp);
            }

            return vectorToReturn;
        }

        public static VectorOfVectorOfPointF ConvertToVectorOfPoint(this VectorOfVectorOfPoint vectorToTransform)
        {
            var vectorToReturn = new VectorOfVectorOfPointF();
            var vectorArrayOfArray = vectorToTransform.ToArrayOfArray();
            foreach (var vector in vectorArrayOfArray)
            {
                var temp = new VectorOfPointF();
                var pointList = vector.Select(point => new PointF((int) point.X, (int) point.Y)).ToList();
                temp.Push(pointList.ToArray());
                vectorToReturn.Push(temp);
            }

            return vectorToReturn;
        }

        public static VectorOfPoint ConvertToVectorOfPoint(this VectorOfPointF vectorToTransform)
        {
            var vectorToReturn = new VectorOfPoint();
            var vectorArrayOfArray = vectorToTransform.ToArray();
            var tempList = vectorArrayOfArray.Select(point => new Point((int) point.X, (int) point.Y)).ToList();
            vectorToReturn.Push(tempList.ToArray());
            return vectorToReturn;
        }

        public static VectorOfPointF ConvertToVectorOfPoint(this VectorOfPoint vectorToTransform)
        {
            var vectorToReturn = new VectorOfPointF();
            var vectorArrayOfArray = vectorToTransform.ToArray();
            var tempList = vectorArrayOfArray.Select(point => new PointF(point.X, point.Y)).ToList();
            vectorToReturn.Push(tempList.ToArray());
            return vectorToReturn;
        }

        public static Point GetContourCenterPoint(this VectorOfPoint contour)
        {
            var tempPoint = Point.Empty;
            if (null == contour) return tempPoint;
            var moment = CvInvoke.Moments(contour);
            var cx = moment.M10 / moment.M00;
            var cy = moment.M01 / moment.M00;
            tempPoint = new Point((int)cx, (int)cy);

            return tempPoint;
        }

        public static PointF GetContourCenterPointF(this VectorOfPoint contour)
        {
            var moment = CvInvoke.Moments(contour);
            var cx = moment.M10 / moment.M00;
            var cy = moment.M01 / moment.M00;
            var tempPoint = new PointF((float) cx, (float) cy);
            return tempPoint;
        }

        public static VectorOfPoint DetectBiggestCellInMat(this Mat matToSample)
        {
            var contours = new VectorOfVectorOfPoint();
            var contoursToReturn = new VectorOfVectorOfPoint();
            var hierarchy = new Mat();
            CvInvoke.FindContours(matToSample, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree,
                Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            var maxArea = double.MinValue;
            var maxAreaContour = new VectorOfPoint();

            for (var idx = 0; idx < contours.Size; idx++)
            {
                var con = contours[idx];
                var tempArea = CvInvoke.ContourArea(con);
                if ((tempArea < 800)) continue;
                if (!(tempArea > maxArea)) continue;
                maxArea = tempArea;
                maxAreaContour = con;
            }

            contoursToReturn.Push(maxAreaContour);

            //if (contoursToReturn.Size > 1) throw new Exception("More than one complete contour in ROI!");

            return contoursToReturn[0];
        }

        public static VectorOfPoint DetectSmallestCellInMat(this Mat matToSample)
        {
            var contours = new VectorOfVectorOfPoint();
            var contoursToReturn = new VectorOfVectorOfPoint();
            var hierarchy = new Mat();
            CvInvoke.FindContours(matToSample, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree,
                Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            var minArea = double.MaxValue;
            var minAreaContour = new VectorOfPoint();

            for (var idx = 0; idx < contours.Size; idx++)
            {
                var con = contours[idx];
                var tempArea = CvInvoke.ContourArea(con);
                if ((tempArea < 800)) continue;
                if (!(tempArea < minArea)) continue;
                minArea = tempArea;
                minAreaContour = con;
            }

            contoursToReturn.Push(minAreaContour);

            //if (contoursToReturn.Size > 1) throw new Exception("More than one complete contour in ROI!");

            return contoursToReturn[0];
        }

        public static List<VectorOfPoint> DetectCellContoursInMat(this Mat matToSample)
        {
            var contours = new VectorOfVectorOfPoint();
            var contoursToReturn = new List<VectorOfPoint>();
            var hierarchy = new Mat();
            CvInvoke.FindContours(matToSample, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree,
                Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);


            for (var idx = 0; idx < contours.Size; idx++)
            {
                var con = contours[idx];
                var tempArea = CvInvoke.ContourArea(con);
                if ((tempArea < 800)) continue;
                contoursToReturn.Add(con);
            }


            //if (contoursToReturn.Count > 2) throw new Exception("More than two complete contour in ROI!");
            if (contoursToReturn.Count > 2) contoursToReturn.RemoveRange(2, contoursToReturn.Count - 2);

            contoursToReturn.OrderByDescending(x => CvInvoke.ContourArea(x));

            return contoursToReturn;
        }

        public static void DrawContourOnMat(this Mat matToDrawOn, VectorOfVectorOfPoint contours, MCvScalar color)
        {
            if (matToDrawOn.NumberOfChannels < 3)
            {
                CvInvoke.CvtColor(matToDrawOn, matToDrawOn, ColorConversion.Gray2Bgr);
            }

            CvInvoke.DrawContours(matToDrawOn, contours, -1, color);
        }

        public static Mat RotMatOnly(this Mat uprightBondingRectangleMat, VectorOfPoint contour, double angleOffset)
        {
            var matToReturn = new Mat();

            var angledBoundingRectangle = CvInvoke.MinAreaRect(contour);
            var ogRoiHeight = uprightBondingRectangleMat.Height;
            var ogRoiWidth = uprightBondingRectangleMat.Width;

            var newRoiSideLength = ogRoiWidth > ogRoiHeight ? ogRoiWidth * Math.Sqrt(2) : ogRoiHeight * Math.Sqrt(2);

            var ogBBoxHeight = angledBoundingRectangle.Size.Height;
            var ogBBoxWidth = angledBoundingRectangle.Size.Width;
            //var actualBBoxWidth = ogBBoxWidth > ogBBoxHeight ? ogBBoxWidth : ogBBoxHeight;
            //var actualBBoxHeight = ogBBoxWidth > ogBBoxHeight ? ogBBoxHeight : ogBBoxWidth;

            var oldRoi = uprightBondingRectangleMat;

            var newRoi = Mat.Zeros((int) newRoiSideLength, (int) newRoiSideLength, uprightBondingRectangleMat.Depth, 1);
            var newRoiCenter = new PointF((float) (newRoiSideLength / 2.0 - 0.5),
                (float) (newRoiSideLength / 2.0f - 0.5f));

            var rowOffset = newRoi.Rows / 2 - oldRoi.Rows / 2;
            var colOffset = newRoi.Cols / 2 - oldRoi.Cols / 2;

            var rowRange = new Range(rowOffset, rowOffset + oldRoi.Rows);
            var colRange = new Range(colOffset, colOffset + oldRoi.Cols);

            var newRoiViewPort = new Mat(newRoi, rowRange, colRange);

            oldRoi.CopyTo(newRoiViewPort);

            MainWindow.ImageProcessorExaminer.AddImage(oldRoi.CreateNewHardCopyFromMat(), "RotMat_oldRoi");
            MainWindow.ImageProcessorExaminer.AddImage(newRoi.CreateNewHardCopyFromMat(), "RotMat_newRoi");

            //var ogRoiCenter = new PointF((ogRoiWidth / 2f - 0.5f), (ogRoiHeight / 2.0f - 0.5f));

            /*var desiredAngleOfRotation = angledBoundingRectangle.Angle + angleOffset;
            if (ogBBoxWidth < ogBBoxHeight)
            {
                desiredAngleOfRotation = 90 + desiredAngleOfRotation;

            }*/

            var desiredAngleOfRotation = angledBoundingRectangle.Angle + angleOffset;
            if (ogBBoxWidth < ogBBoxHeight)
            {
                desiredAngleOfRotation = angledBoundingRectangle.Angle - 90.0;

            }

            var rotatingMat = new Mat();
            CvInvoke.GetRotationMatrix2D(newRoiCenter, desiredAngleOfRotation, 1.0, rotatingMat);

            return rotatingMat;
        }

        public static Mat RotMat(this Mat uprightBondingRectangleMat, VectorOfPoint contour)
        {
            return RotMat(uprightBondingRectangleMat, contour, 0.0);
        }

        public static Mat RotMat(this Mat uprightBondingRectangleMat, VectorOfPoint contour, double angleOffset)
        {
            var matToReturn = new Mat();

            var angledBoundingRectangle = CvInvoke.MinAreaRect(contour);
            var ogRoiHeight = uprightBondingRectangleMat.Height;
            var ogRoiWidth = uprightBondingRectangleMat.Width;

            var newRoiSideLength = ogRoiWidth > ogRoiHeight ? ogRoiWidth * Math.Sqrt(2) : ogRoiHeight * Math.Sqrt(2);

            var ogBBoxHeight = angledBoundingRectangle.Size.Height;
            var ogBBoxWidth = angledBoundingRectangle.Size.Width;
            //var actualBBoxWidth = ogBBoxWidth > ogBBoxHeight ? ogBBoxWidth : ogBBoxHeight;
            //var actualBBoxHeight = ogBBoxWidth > ogBBoxHeight ? ogBBoxHeight : ogBBoxWidth;

            var oldRoi = uprightBondingRectangleMat;

            var newRoi = Mat.Zeros((int) newRoiSideLength, (int) newRoiSideLength, uprightBondingRectangleMat.Depth, 1);
            var newRoiCenter = new PointF((float) (newRoiSideLength / 2.0 - 0.5),
                (float) (newRoiSideLength / 2.0f - 0.5f));

            var rowOffset = newRoi.Rows / 2 - oldRoi.Rows / 2;
            var colOffset = newRoi.Cols / 2 - oldRoi.Cols / 2;

            var rowRange = new Range(rowOffset, rowOffset + oldRoi.Rows);
            var colRange = new Range(colOffset, colOffset + oldRoi.Cols);

            var newRoiViewPort = new Mat(newRoi, rowRange, colRange);

            oldRoi.CopyTo(newRoiViewPort);

            MainWindow.ImageProcessorExaminer.AddImage(oldRoi.CreateNewHardCopyFromMat(), "RotMat_oldRoi");
            MainWindow.ImageProcessorExaminer.AddImage(newRoi.CreateNewHardCopyFromMat(), "RotMat_newRoi");

            //var ogRoiCenter = new PointF((ogRoiWidth / 2f - 0.5f), (ogRoiHeight / 2.0f - 0.5f));

            /*var desiredAngleOfRotation = angledBoundingRectangle.Angle + angleOffset;
            if (ogBBoxWidth < ogBBoxHeight)
            {
                desiredAngleOfRotation = 90 + desiredAngleOfRotation;

            }*/

            var desiredAngleOfRotation = angledBoundingRectangle.Angle + angleOffset;
            if (ogBBoxWidth < ogBBoxHeight)
            {
                desiredAngleOfRotation = angledBoundingRectangle.Angle - 90.0;

            }

            var rotatingMat = new Mat();
            CvInvoke.GetRotationMatrix2D(newRoiCenter, desiredAngleOfRotation, 1.0, rotatingMat);

            CvInvoke.WarpAffine(newRoi, matToReturn, rotatingMat, newRoi.Size);


            //CvInvoke.GaussianBlur(this.ImageMat, this.ImageMat, new Size(), 0);


            //Adaptive threshold
            //var tempAdaptiveThreshold = MainWindow.ImageHandler.ImgProcessor.AdaptiveThresholdConstant;
            //var tempAdaptiveThreshold = 5;

            var tresholdedMat = new Mat();
            //CvInvoke.AdaptiveThreshold(matToReturn, tresholdedMat, 255, Emgu.CV.CvEnum.AdaptiveThresholdType.MeanC, Emgu.CV.CvEnum.ThresholdType.Binary, 59, tempAdaptiveThreshold);
            CvInvoke.Threshold(matToReturn, tresholdedMat, 127, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
            //TODO kideríteni, hogy ide milyen thresholding kellene
            MainWindow.ImageProcessorExaminer.AddImage(matToReturn.CreateNewHardCopyFromMat(), "RotMat_RotatedRoi");
            MainWindow.ImageProcessorExaminer.AddImage(tresholdedMat.CreateNewHardCopyFromMat(),
                "RotMat_ThresholdedRotatedRoi");

            return tresholdedMat;
        }

        public static Mat RotMatGreyscale(this Mat uprightBondingRectangleMat, VectorOfPoint contour,
            double angleOffset)
        {
            var matToReturn = new Mat();

            var angledBoundingRectangle = CvInvoke.MinAreaRect(contour);
            var ogRoiHeight = uprightBondingRectangleMat.Height;
            var ogRoiWidth = uprightBondingRectangleMat.Width;

            var newRoiSideLength = ogRoiWidth > ogRoiHeight ? ogRoiWidth * Math.Sqrt(2) : ogRoiHeight * Math.Sqrt(2);

            var ogBBoxHeight = angledBoundingRectangle.Size.Height;
            var ogBBoxWidth = angledBoundingRectangle.Size.Width;

            var oldRoi = uprightBondingRectangleMat;

            var vm = new VectorOfMat();
            vm.Push(uprightBondingRectangleMat);

            var histogram = new Mat();
            CvInvoke.CalcHist(vm, new int[] {0}, new Mat(), histogram, new int[] {255}, new float[] {0f, 256f}, false);
            var tempRow = histogram.Col(0);
            var mostCommonIntensityCount = -1;
            var mostCommonIntensity = -1;

            for (var rowIdx = 0; rowIdx < tempRow.Rows; rowIdx++)
            {
                var tempValue = tempRow.GetData().GetValue(rowIdx, 0);
                var convertedTempValue = (int) (float) tempValue;
                if (convertedTempValue <= mostCommonIntensityCount) continue;
                mostCommonIntensityCount = convertedTempValue;
                mostCommonIntensity = rowIdx;
            }

            //var newRoi = Mat.Zeros((int)newRoiSideLength, (int)newRoiSideLength, uprightBondingRectangleMat.Depth, 1);
            var newRoi = new Mat((int) newRoiSideLength, (int) newRoiSideLength, uprightBondingRectangleMat.Depth, 1);
            newRoi.SetTo(new MCvScalar(mostCommonIntensity));

            var newRoiCenter = new PointF((float) (newRoiSideLength / 2.0 - 0.5),
                (float) (newRoiSideLength / 2.0f - 0.5f));

            var rowOffset = newRoi.Rows / 2 - oldRoi.Rows / 2;
            var colOffset = newRoi.Cols / 2 - oldRoi.Cols / 2;

            var rowRange = new Range(rowOffset, rowOffset + oldRoi.Rows);
            var colRange = new Range(colOffset, colOffset + oldRoi.Cols);

            var newRoiViewPort = new Mat(newRoi, rowRange, colRange);

            oldRoi.CopyTo(newRoiViewPort);

            MainWindow.ImageProcessorExaminer.AddImage(oldRoi.CreateNewHardCopyFromMat(), "RotMatGreyscale_oldRoi");
            MainWindow.ImageProcessorExaminer.AddImage(newRoi.CreateNewHardCopyFromMat(), "RotMatGreyscale_newRoi");

            var desiredAngleOfRotation = angledBoundingRectangle.Angle + angleOffset;
            if (ogBBoxWidth < ogBBoxHeight)
            {
                desiredAngleOfRotation = angledBoundingRectangle.Angle - 90.0;

            }

            var rotatingMat = new Mat();
            CvInvoke.GetRotationMatrix2D(newRoiCenter, desiredAngleOfRotation, 1.0, rotatingMat);

            CvInvoke.WarpAffine(newRoi, matToReturn, rotatingMat, newRoi.Size);
            //var thresholdedMat = new Mat();
            //CvInvoke.AdaptiveThreshold(matToReturn, tresholdedMat, 255, Emgu.CV.CvEnum.AdaptiveThresholdType.MeanC, Emgu.CV.CvEnum.ThresholdType.Binary, 59, tempAdaptiveThreshold);
            //CvInvoke.Threshold(matToReturn, thresholdedMat, 127, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
            //TODO kideríteni, hogy ide milyen thresholding kellene
            MainWindow.ImageProcessorExaminer.AddImage(matToReturn.CreateNewHardCopyFromMat(), "RotMat_RotatedRoi");
            //MainWindow.ImageProcessorExaminer.AddImage(thresholdedMat.CreateNewHardCopyFromMat(),"RotMat_ThresholdedRotatedRoi");

            return matToReturn;
        }

        /*public static Mat RotMat(this Mat uprightBondingRectangleMat, VectorOfPoint contour, double angleOffset)
         {
            var matToReturn = new Mat();

            var angledBoundingRectangle = CvInvoke.MinAreaRect(contour);
            var ogHeight = uprightBondingRectangleMat.Height;
            var ogWidth = uprightBondingRectangleMat.Width;

            var ogBBoxHeight = angledBoundingRectangle.Size.Height;
            var ogBBoxWidth = angledBoundingRectangle.Size.Width;


            var ogCenter = new PointF((float)(ogWidth / 2.0 - 0.5), (float)(ogHeight / 2.0 - 0.5));

            var ogSize = uprightBondingRectangleMat.Size;

            var desiredAngleOfRotation = angledBoundingRectangle.Angle + angleOffset;
            if (ogSize.Width < ogSize.Height)
            {
                desiredAngleOfRotation = 90 + desiredAngleOfRotation;
            }

            //var sin = Math.Sin(desiredAngleOfRotation);
            //var cos = Math.Sin(desiredAngleOfRotation);

            var rotatingMat = CreateRotationMatrix(desiredAngleOfRotation);
            var altRotatingMat = new Mat();
            //CvInvoke.GetRotationMatrix2D(new PointF(0f,0f), desiredAngleOfRotation, 1.0, altRotatingMat);

            //var cos = Math.Abs((double)rotatingMat.GetData().GetValue(0, 0));
            //var sin = Math.Abs((double)rotatingMat.GetData().GetValue(0, 1));

            //var cosAlt = Math.Abs((double)altRotatingMat.GetData().GetValue(0, 0));
            //var sinAlt = Math.Abs((double)altRotatingMat.GetData().GetValue(0, 1));



            var angleInRadian = desiredAngleOfRotation * Math.PI / 180;

            var cos = Math.Cos(angleInRadian);
            var sin = Math.Sin(angleInRadian);

            var offset = ogBBoxWidth > ogBBoxHeight ? ogBBoxWidth : ogBBoxHeight;

            var newWidth = (int)((ogHeight * sin) + (ogWidth * cos));
            var newHeight = (int)((ogHeight * cos) + (ogWidth * sin));
            var newCenterPoint = new PointF((float) (newWidth / 2.0 - 0.5) / 2f, (float)(newHeight / 2.0 - 0.5));

            //CvInvoke.GetRotationMatrix2D(newCenterPoint, angleInRadian, 1.0, altRotatingMat);

            var offsetMatrix = new Mat();
            var rotatingMatrix = new Mat();

            CvInvoke.GetRotationMatrix2D(newCenterPoint, desiredAngleOfRotation, 1.0, offsetMatrix);
            CvInvoke.GetRotationMatrix2D(new PointF(0.5f,0.5f), desiredAngleOfRotation, 1.0, rotatingMatrix);

            var tempMatOne = new Mat();
            var tempMatTwo = new Mat();
            var tempMatThree= new Mat();

            var rotatedShiftedMatAlt = CreateRotationAndTranslationMat(newCenterPoint, desiredAngleOfRotation);
            rotatedShiftedMatAlt = rotatedShiftedMatAlt.GetRowsFromRange(0, 1);

            CvInvoke.WarpAffine(uprightBondingRectangleMat, tempMatOne, offsetMatrix, new Size(newWidth, newHeight));
            CvInvoke.WarpAffine(uprightBondingRectangleMat, tempMatTwo, rotatingMatrix, new Size(newWidth, newHeight));
            CvInvoke.WarpAffine(uprightBondingRectangleMat, tempMatThree, rotatedShiftedMatAlt, new Size(newWidth, newHeight));



            MainWindow.ImageProcessorExaminer.AddImage(tempMatOne.CreateNewHardCopyFromMat(), "WarpAffine_offsetMatrix");
            MainWindow.ImageProcessorExaminer.AddImage(tempMatTwo.CreateNewHardCopyFromMat(), "WarpAffine_rotatingMatrix");
            MainWindow.ImageProcessorExaminer.AddImage(tempMatTwo.CreateNewHardCopyFromMat(), "WarpAffine_rotatedShiftedMatAlt");



            var shiftingMatOne = CreateTranslationMat(newWidth / 2.0 - 0.5,newHeight / 2.0 - 0.5);
            var shiftingMatTwo = CreateTranslationMat(-(newWidth / 2.0 - 0.5), -(newHeight / 2.0 - 0.5));

            //var rotatedShiftedMat = shiftingMatTwo.Cross(rotatingMat.Cross(shiftingMatOne));

            //var rotatedShiftedMat = new Mat();

            //var tempMatOne = new Mat();
            //CvInvoke.Multiply(   shiftingMatOne, rotatingMat, tempMatOne);
            //var tempMatTwo = new Mat();
            //CvInvoke.Multiply(  tempMatOne, shiftingMatTwo, tempMatTwo);

            //var rotatedShiftedMat = tempMatTwo.GetRowsFromRange(0, 1);





            //CvInvoke.WarpAffine(uprightBondingRectangleMat, tempMatOne, altRotatingMat, new Size(newWidth,newHeight));
            //CvInvoke.WarpAffine(uprightBondingRectangleMat, tempMatTwo, rotatedShiftedMatAlt, new Size(newWidth, newHeight));


            //MainWindow.ImageProcessorExaminer.AddImage(tempMatOne.CreateNewHardCopyFromMat(), "WarpAffine_rotatedShiftedMat");
            //MainWindow.ImageProcessorExaminer.AddImage(tempMatTwo.CreateNewHardCopyFromMat(), "WarpAffine_rotatedShiftedMatAlt");

            //CvInvoke.WarpPerspective(uprightBondingRectangleMat, matToReturn, rotatedShiftedMat, new Size(newWidth, newHeight));

            matToReturn = tempMatTwo;
            return matToReturn;
        }*/

        public static Mat DrawPixelsToMat(this Mat matToDrawOn, VectorOfPoint pixels)
        {
            //https://stackoverflow.com/questions/49799057/how-to-draw-a-point-in-an-image-using-given-co-ordinate-with-python-opencv
            var modifiedMat = matToDrawOn.CreateNewHardCopyFromMat();
            var color = new MCvScalar(127, 127, 127);
            foreach (var point in pixels.ToArray())
            {
                CvInvoke.Circle(modifiedMat, point, 0, color, -1);
                CvInvoke.Circle(modifiedMat, point, 3, color, 1);
            }

            return modifiedMat;
        }

        public static Mat DrawMarkerPixelsToMat(this Mat matToDrawOn, VectorOfPoint pixels, MCvScalar color)
        {
            //https://stackoverflow.com/questions/49799057/how-to-draw-a-point-in-an-image-using-given-co-ordinate-with-python-opencv
            var modifiedMat = matToDrawOn.CreateNewHardCopyFromMat();
            //var color = new MCvScalar(127, 127, 127);
            foreach (var point in pixels.ToArray())
            {
                CvInvoke.Circle(modifiedMat, point, 0, color, -1);
            }

            return modifiedMat;
        }

        public static Mat DrawColorPixelsToMat(this Mat matToDrawOn, VectorOfPoint pixels, MCvScalar color)
        {
            //https://stackoverflow.com/questions/49799057/how-to-draw-a-point-in-an-image-using-given-co-ordinate-with-python-opencv
            var modifiedMat = matToDrawOn.CreateNewHardCopyFromMat();
            foreach (var point in pixels.ToArray())
            {
                CvInvoke.Circle(modifiedMat, point, 0, color, -1);
                CvInvoke.Circle(modifiedMat, point, 3, color, 1);
            }

            return modifiedMat;
        }

        public static PointF[] ReorderBoxPoints(this PointF[] ogArray)
        {
            //https://stackoverflow.com/questions/298725/multiple-order-by-in-linq
            var points = ogArray.OrderByDescending(point => point.Y).ThenByDescending(point => point.X).ToList();
            //m => new { m.CategoryID, m.Name }

            return points.ToArray();
        }

        public static Mat RotateMat(this Mat uprightBondingRectangleMat, VectorOfPoint contour)
        {
            return RotateMat(uprightBondingRectangleMat, contour, 0.0);
        }

        public static Mat RotateMat(this Mat uprightBondingRectangleMat, VectorOfPoint contour, double angleOffset)
        {
            var angledBoundingRectangle = CvInvoke.MinAreaRect(contour);
            var ogHeight = uprightBondingRectangleMat.Height;
            var ogWidth = uprightBondingRectangleMat.Width;

            var ogBBoxHeight = angledBoundingRectangle.Size.Height;
            var ogBBoxWidth = angledBoundingRectangle.Size.Width;

            //var center = new PointF((float)(ogBBoxWidth / 2.0), (float)(ogBBoxHeight / 2.0));
            var center = new PointF((float) (ogWidth / 2.0 - 0.5), (float) (ogHeight / 2.0 - 0.5));

            //var center = contour.GetContourCenterPointF();

            var rotatingMat = new Mat();
            var ogSize = uprightBondingRectangleMat.Size;

            var desiredAngleOfRotation = angledBoundingRectangle.Angle + angleOffset;
            if (ogSize.Width < ogSize.Height)
            {
                desiredAngleOfRotation = 90 + desiredAngleOfRotation;
            }

            //https://math.stackexchange.com/questions/2093314/rotation-matrix-of-rotation-around-a-point-other-than-the-origin
            //https://stackoverflow.com/questions/11764575/python-2-7-3-opencv-2-4-after-rotation-window-doesnt-fit-image

            //var r = (Math.PI / 180) * desiredAngleOfRotation;

            //newX = abs(np.sin(r)*newY) + abs(np.cos(r)*newX)
            //newY = abs(np.sin(r)*newX) + abs(np.cos(r)*newY)

            //var newWidth = Math.Abs(Math.Sin(desiredAngleOfRotation) * ogWidth) + Math.Abs(Math.Cos(desiredAngleOfRotation) * ogWidth);
            //var newHeight = Math.Abs(Math.Sin(desiredAngleOfRotation) * ogHeight) + Math.Abs(Math.Cos(desiredAngleOfRotation) * ogHeight);

            CvInvoke.GetRotationMatrix2D(center, desiredAngleOfRotation, 1.0, rotatingMat);

            var cos = Math.Abs((double) rotatingMat.GetData().GetValue(0, 0));
            var sin = Math.Abs((double) rotatingMat.GetData().GetValue(0, 1));

            //var tempFirst = (double)rotatingMat.GetData().GetValue(0, 2) + uprightBondingRectangleMat.Width / 2.0 - angledBoundingRectangle.Size.Width / 2.0;
            //var tempSecond = (double)rotatingMat.GetData().GetValue(1, 2) + uprightBondingRectangleMat.Height / 2.0 - angledBoundingRectangle.Size.Height / 2.0;

            var newWidth = (int) ((ogHeight * sin) + (ogWidth * cos));
            var newHeight = (int) ((ogHeight * cos) + (ogWidth * sin));

            var tempFirst = (double) rotatingMat.GetData().GetValue(0, 2) + newWidth / 2.0 -
                            angledBoundingRectangle.Size.Width / 2.0;
            var tempSecond = (double) rotatingMat.GetData().GetValue(1, 2) + newHeight / 2.0 -
                             angledBoundingRectangle.Size.Height / 2.0;

            //var translationMat = CreateTranslationMat(rotatingMat.Depth,center.X,center.Y);

            var newCenter = new PointF((float) (newWidth / 2.0 - 0.5), (float) (newHeight / 2.0 - 0.5));

            //CvInvoke.GetRotationMatrix2D(center, desiredAngleOfRotation, 1.0, rotatingMat);

            CvInvoke.GetRotationMatrix2D(newCenter, desiredAngleOfRotation, 1.0, rotatingMat);

            /*var tempFirst = double.NaN;
            var tempSecond = double.NaN;

            if (ogSize.Width < ogSize.Height)
            {
                 tempFirst = (double)rotatingMat.GetData().GetValue(1, 2) + newWidth / 2.0 - angledBoundingRectangle.Size.Width / 2.0;
                 tempSecond = (double)rotatingMat.GetData().GetValue(0, 2) + newHeight / 2.0 - angledBoundingRectangle.Size.Height / 2.0;
            }
            else
            {
                 tempFirst = (double)rotatingMat.GetData().GetValue(0, 2) + newWidth / 2.0 - angledBoundingRectangle.Size.Width / 2.0;
                 tempSecond = (double)rotatingMat.GetData().GetValue(1, 2) + newHeight / 2.0 - angledBoundingRectangle.Size.Height / 2.0;
            }*/

            //var offsetMat = rotatingMat.CreateNewMatLikeThis();

            rotatingMat.SetValue(0, 2, tempFirst);
            rotatingMat.SetValue(1, 2, tempSecond);
            //offsetMat.SetValue(0, 2, tempFirst);
            //offsetMat.SetValue(1, 2, tempSecond);


            var matToReturn = new Mat();




            //https://stackoverflow.com/questions/4279008/specify-an-origin-to-warpperspective-function-in-opencv-2-x


            //var tempSize = ogSize.Width > ogSize.Height ? new Size( ogSize.Width, ogSize.Width) : new Size(ogSize.Height, ogSize.Height);

            //CvInvoke.WarpAffine(uprightBondingRectangleMat, matToReturn, offsetMat, new Size((int)newWidth, (int)newHeight));
            //MainWindow.ImageProcessorExaminer.AddImage(matToReturn.CreateNewHardCopyFromMat(), "RotateMat_offsetMat");

            CvInvoke.WarpAffine(uprightBondingRectangleMat, matToReturn, rotatingMat,
                new Size((int) newWidth, (int) newHeight));

            return matToReturn;
        }


        //https://stackoverflow.com/questions/4279008/specify-an-origin-to-warpperspective-function-in-opencv-2-x
        public static Mat CreateTranslationMat(double dx, double dy)
        {
            var matToReturn = Mat.Eye(3, 3, DepthType.Cv64F, 1);
            matToReturn.SetValue(0, 2, dx);
            matToReturn.SetValue(1, 2, dy);
            return matToReturn;
        }

        public static Mat CreateTranslationMat(PointF newCenterPoint)
        {
            var matToReturn = Mat.Eye(3, 3, DepthType.Cv64F, 1);
            matToReturn.SetValue(0, 2, newCenterPoint.X);
            matToReturn.SetValue(1, 2, newCenterPoint.Y);
            return matToReturn;
        }

        public static Mat CreateRotationMatrix(double angleOfRotation)
        {
            var matToReturn = Mat.Zeros(3, 3, DepthType.Cv64F, 1);
            //https://github.com/opencv/opencv/blob/master/modules/imgproc/src/imgwarp.cpp
            //[np.sin(np.pi/4), -np.cos(np.pi/4), 0,
            //np.cos(np.pi/4), np.sin(np.pi/4), 0,
            //0, 0, 1]).reshape((3,3))

            var angle = angleOfRotation * Math.PI / 180;

            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);
            matToReturn.SetValue(0, 0, cos);
            matToReturn.SetValue(0, 1, sin);
            matToReturn.SetValue(1, 0, -sin);
            matToReturn.SetValue(1, 1, cos);
            matToReturn.SetValue(2, 2, 1);

            return matToReturn;
        }

        public static Mat CreateRotationAndTranslationMat(PointF newCenterPoint, double angleOfRotation)
        {
            var matToReturn = Mat.Zeros(3, 3, DepthType.Cv64F, 1);

            //[np.sin(np.pi/4), -np.cos(np.pi/4), 0,
            //np.cos(np.pi/4), np.sin(np.pi/4), 0,
            //0, 0, 1]).reshape((3,3))

            var angleInRadian = angleOfRotation * Math.PI / 180;

            var beta = Math.Sin(angleInRadian);
            var alpha = Math.Cos(angleInRadian);
            var x = newCenterPoint.X;
            var y = newCenterPoint.Y;

            matToReturn.SetValue(0, 0, alpha);
            matToReturn.SetValue(0, 1, beta);
            matToReturn.SetValue(0, 2, (1 - alpha) * x - beta * y);
            matToReturn.SetValue(1, 0, -beta);
            matToReturn.SetValue(1, 1, alpha);
            matToReturn.SetValue(1, 2, beta * x + (1 - alpha) * y);
            matToReturn.SetValue(2, 2, 1);

            return matToReturn;
        }


        public static Mat RotateMatWithoutCutoff(this Mat uprightBondingRectangleMat,
            RotatedRect angledBoundingRectangle)
        {
            //https://docs.opencv.org/master/da/d0c/tutorial_bounding_rects_circles.html
            //az upright kell majd nekünk :D jeeeee



            //https://stackoverflow.com/questions/22041699/rotate-an-image-without-cropping-in-opencv-in-c
            //var tempRectangle = boundingRectangle.MinAreaRect();
            var height = uprightBondingRectangleMat.Height;
            var width = uprightBondingRectangleMat.Width;
            var center = new PointF((float) (width / 2.0), (float) (height / 2.0));
            //var center = angledBoundingRectangle.Center;
            //var temp = CvInvoke.BoundingRectangle(angledBoundingRectangle.MinAreaRect())
            //az angle -- The angle of the box in degrees. Possitive value means counter-clock wise rotation

            //Itt az angle az

            var rotatingMat = new Mat();


            //Angle between the horizontal axis and the first side (width) in degrees
            //Positive value means  counter-clockwise rotation

            //if (angledBoundingRectangle.Angle < 0)
            //{
            //    desiredAngleOfRotation = angledBoundingRectangle.Angle < 90 ?  -Math.Abs(0 - angledBoundingRectangle.Angle) :  -Math.Abs(180 - angledBoundingRectangle.Angle);
            //    desiredAngleOfRotation += 90;
            //}
            //else if (angledBoundingRectangle.Angle > 0)
            //{
            //    desiredAngleOfRotation = angledBoundingRectangle.Angle < -90 ? -Math.Abs(0 - angledBoundingRectangle.Angle) : -Math.Abs(180 - angledBoundingRectangle.Angle);
            //    desiredAngleOfRotation -= 90;
            //}

            //desiredAngleOfRotation = angledBoundingRectangle.Angle < 0 ? angledBoundingRectangle.Angle :  - angledBoundingRectangle.Angle;

            //desiredAngleOfRotation = angledBoundingRectangle.Angle + 90 < 180 ? angledBoundingRectangle.Angle + 90 : -angledBoundingRectangle.Angle;


            var ogSize = uprightBondingRectangleMat.Size;

            //desiredAngleOfRotation = angledBoundingRectangle.Angle < 0 ? angledBoundingRectangle.Angle : - angledBoundingRectangle.Angle;

            //desiredAngleOfRotation = ogSize.Width > ogSize.Height ? desiredAngleOfRotation : desiredAngleOfRotation + 90;

            //https://stackoverflow.com/questions/24073127/opencvs-rotatedrect-angle-does-not-provide-enough-information

            double desiredAngleOfRotation = angledBoundingRectangle.Angle;
            if (ogSize.Width < ogSize.Height)
            {
                desiredAngleOfRotation = 90 + desiredAngleOfRotation;
            }

            CvInvoke.GetRotationMatrix2D(center, desiredAngleOfRotation, 1.0, rotatingMat);

            //Ez fölösleges, ha tényleg upright az a rectangle !
            var tempFirst = (double) rotatingMat.GetData().GetValue(0, 2) + uprightBondingRectangleMat.Width / 2.0 -
                            angledBoundingRectangle.Size.Width / 2.0;
            var tempSecond = (double) rotatingMat.GetData().GetValue(1, 2) + uprightBondingRectangleMat.Height / 2.0 -
                             angledBoundingRectangle.Size.Height / 2.0;

            //rotatingMat.Data.SetValue(tempFirst,0, 2);
            //rotatingMat.Data.SetValue(tempSecond, 1, 2);

            //rotatingMat.GetRawData().SetValue(tempFirst, 0, 2);
            //rotatingMat.GetRawData().SetValue(tempSecond, 1, 2);

            //var tempMatAlpha = rotatingMat.Row(0).Col(2);
            //var tempMatBeta= rotatingMat.Row(1).Col(2);
            //tempMatAlpha.Data.SetValue(tempFirst,0);
            //tempMatBeta.Data.SetValue(tempSecond, 0);


            rotatingMat.SetValue(2, 0, tempFirst);
            rotatingMat.SetValue(2, 1, tempSecond);
            //var roiBoundingRectangle = new RotatedRect(center,new SizeF(width,height),boundingRectangle.Angle).MinAreaRect();


            //rotatingMat.GetData().

            var matToReturn = new Mat();
            //https://www.pyimagesearch.com/2017/01/02/rotate-images-correctly-with-opencv-and-python/
            //https://en.wikipedia.org/wiki/Affine_transformation


            //CvInvoke.WarpAffine(uprightBondingRectangleMat,matToReturn,rotatingMat, uprightBondingRectangleMat.Size);

            var tempSize = ogSize.Width > ogSize.Height
                ? new Size(ogSize.Width, ogSize.Height)
                : new Size(ogSize.Height, ogSize.Width);

            CvInvoke.WarpAffine(uprightBondingRectangleMat, matToReturn, rotatingMat, tempSize);

            return matToReturn;
        }

        public static (Point, Point) GetPointsOfWidestSliceOfCellSegmented(this Mat matToMeasure)
        {
            //Kell majd hozzá a bounding box
            //https://stackoverflow.com/questions/15043152/rotate-opencv-matrix-by-90-180-270-degrees
            //https://jayrambhia.wordpress.com/2012/09/20/roi-bounding-box-selection-of-mat-images-in-opencv/
            //https://www.pyimagesearch.com/2017/01/02/rotate-images-correctly-with-opencv-and-python/
            //var roiMat = matToMeasure.CreateNewHardCopyFromMat();
            //var tempA = new Point(0);
            //var tempB = new Point(0);
            //var tempSize = new Size(cornerB.X - cornerA.X, cornerB.Y - cornerA.Y);
            //var roiRectangle = new Rectangle(cornerA, tempSize);
            //var roiMat = new Mat(matToMeasure, roiRectangle);
            //MVCRect tempRect = new MVSRect();

            //roiMat = roiMat(roiRectangle);
            var biggestAreaWindowAndIdx = GetBiggestAreaOfCellWithSlidingWindowAndRowIndex(matToMeasure, 5);
            var biggestAreaRowAndId =
                GetBiggestAreaOfCellWithSlidingWindowAndRowIndex(biggestAreaWindowAndIdx.Item1, 1);
            var pointPair = biggestAreaRowAndId.Item1.GetCenterIdxOfDiffractionLineSlice();

            var rowIdx = biggestAreaWindowAndIdx.Item2 + biggestAreaRowAndId.Item2;

            var tempPointA = new Point(pointPair.Item1, rowIdx);
            var tempPointB = new Point(pointPair.Item2, rowIdx);

            var points = new VectorOfPoint();
            points.Push(new[] {tempPointA, tempPointB});

            var pointsVoVo = new VectorOfVectorOfPoint();
            var temp = new VectorOfPoint();
            temp.Push(new[] {tempPointA});
            pointsVoVo.Push(temp);
            temp = new VectorOfPoint();
            temp.Push(new[] {tempPointB});
            pointsVoVo.Push(temp);

            // CvInvoke.DrawContours(matToReturn, convertedVectorOfVectorPoint, -1, new MCvScalar(0, 255, 0, 255), 2);

            //var newContour = matToMeasure.DetectBiggestCellInMat();
            var newContour = matToMeasure.DetectSmallestCellInMat();
            var tempBBox = CvInvoke.MinAreaRect(newContour);
            var boxPoints = tempBBox.GetVertices();
            points.Push(boxPoints.ConvertToPointArray());
            //var VONewContour = new VectorOfVectorOfPoint();
            //VONewContour.Push(newContour);

            var markedMat = matToMeasure.CreateNewHardCopyFromMat();
            //CvInvoke.DrawContours(markedMat, VONewContour, 0, new MCvScalar(127, 127, 127), 2);

            markedMat = DrawPixelsToMat(matToMeasure, points);

            MainWindow.ImageProcessorExaminer.AddImage(markedMat.CreateNewHardCopyFromMat(),
                "GetPointsOfWidestSliceOfCell_PointsMarked");

            return (tempPointA, tempPointB);
        }

        public static (Point, Point) GetPointsOfWidestSliceOfCellSegmentedWithContours(this Mat matToMeasure,
            VectorOfPoint innerContour, VectorOfPoint outerContour)
        {
            //Kell majd hozzá a bounding box
            //https://stackoverflow.com/questions/15043152/rotate-opencv-matrix-by-90-180-270-degrees
            //https://jayrambhia.wordpress.com/2012/09/20/roi-bounding-box-selection-of-mat-images-in-opencv/
            //https://www.pyimagesearch.com/2017/01/02/rotate-images-correctly-with-opencv-and-python/
            //var roiMat = matToMeasure.CreateNewHardCopyFromMat();
            //var tempA = new Point(0);
            //var tempB = new Point(0);
            //var tempSize = new Size(cornerB.X - cornerA.X, cornerB.Y - cornerA.Y);
            //var roiRectangle = new Rectangle(cornerA, tempSize);
            //var roiMat = new Mat(matToMeasure, roiRectangle);
            //MVCRect tempRect = new MVSRect();

            //roiMat = roiMat(roiRectangle);
            var biggestAreaWindowAndIdx =
                GetBiggestAreaOfCellWithSlidingWindowAndRowIndexWithContours(matToMeasure, 5, 0, innerContour,
                    outerContour);
            var biggestAreaRowAndId = GetBiggestAreaOfCellWithSlidingWindowAndRowIndexWithContours(
                biggestAreaWindowAndIdx.Item1, 1, biggestAreaWindowAndIdx.Item2, innerContour, innerContour);
            var pointPair = biggestAreaRowAndId.Item1.GetCenterIdxOfDiffractionLineSlice();

            var rowIdx = biggestAreaWindowAndIdx.Item2 + biggestAreaRowAndId.Item2;

            var tempPointA = new Point(pointPair.Item1, rowIdx);
            var tempPointB = new Point(pointPair.Item2, rowIdx);

            var points = new VectorOfPoint();
            points.Push(new[] {tempPointA, tempPointB});

            var pointsVoVo = new VectorOfVectorOfPoint();
            var temp = new VectorOfPoint();
            temp.Push(new[] {tempPointA});
            pointsVoVo.Push(temp);
            temp = new VectorOfPoint();
            temp.Push(new[] {tempPointB});
            pointsVoVo.Push(temp);

            // CvInvoke.DrawContours(matToReturn, convertedVectorOfVectorPoint, -1, new MCvScalar(0, 255, 0, 255), 2);

            //var newContour = matToMeasure.DetectBiggestCellInMat();
            var newContour = matToMeasure.DetectSmallestCellInMat();
            var tempBBox = CvInvoke.MinAreaRect(newContour);
            var boxPoints = tempBBox.GetVertices();
            points.Push(boxPoints.ConvertToPointArray());
            //var VONewContour = new VectorOfVectorOfPoint();
            //VONewContour.Push(newContour);

            var markedMat = matToMeasure.CreateNewHardCopyFromMat();
            //CvInvoke.DrawContours(markedMat, VONewContour, 0, new MCvScalar(127, 127, 127), 2);

            markedMat = DrawPixelsToMat(matToMeasure, points);

            MainWindow.ImageProcessorExaminer.AddImage(markedMat.CreateNewHardCopyFromMat(),
                "GetPointsOfWidestSliceOfCellSegmentedWithContours_PointsMarked");

            return (tempPointA, tempPointB);
        }

        public static (Point, Point) GetPointsOfWidestSliceOfCellSegmentedWithContour(this Mat matToMeasure,
            VectorOfPoint contour)
        {
            //Kell majd hozzá a bounding box
            //https://stackoverflow.com/questions/15043152/rotate-opencv-matrix-by-90-180-270-degrees
            //https://jayrambhia.wordpress.com/2012/09/20/roi-bounding-box-selection-of-mat-images-in-opencv/
            //https://www.pyimagesearch.com/2017/01/02/rotate-images-correctly-with-opencv-and-python/
            //var roiMat = matToMeasure.CreateNewHardCopyFromMat();
            //var tempA = new Point(0);
            //var tempB = new Point(0);
            //var tempSize = new Size(cornerB.X - cornerA.X, cornerB.Y - cornerA.Y);
            //var roiRectangle = new Rectangle(cornerA, tempSize);
            //var roiMat = new Mat(matToMeasure, roiRectangle);
            //MVCRect tempRect = new MVSRect();

            //roiMat = roiMat(roiRectangle);
            var biggestAreaWindowAndIdx =
                GetBiggestAreaOfCellWithSlidingWindowAndRowIndexWithContour(matToMeasure, 5, 0, contour);
            var biggestAreaRowAndId =
                GetBiggestAreaOfCellWithSlidingWindowAndRowIndexWithContour(biggestAreaWindowAndIdx.Item1, 1,
                    biggestAreaWindowAndIdx.Item2, contour);
            var pointPair = biggestAreaRowAndId.Item1.GetCenterIdxOfDiffractionLineSlice();

            var rowIdx = biggestAreaWindowAndIdx.Item2 + biggestAreaRowAndId.Item2;

            var tempPointA = new Point(pointPair.Item1, rowIdx);
            var tempPointB = new Point(pointPair.Item2, rowIdx);

            var points = new VectorOfPoint();
            points.Push(new[] {tempPointA, tempPointB});

            var pointsVoVo = new VectorOfVectorOfPoint();
            var temp = new VectorOfPoint();
            temp.Push(new[] {tempPointA});
            pointsVoVo.Push(temp);
            temp = new VectorOfPoint();
            temp.Push(new[] {tempPointB});
            pointsVoVo.Push(temp);

            // CvInvoke.DrawContours(matToReturn, convertedVectorOfVectorPoint, -1, new MCvScalar(0, 255, 0, 255), 2);

            //var newContour = matToMeasure.DetectBiggestCellInMat();
            var newContour = matToMeasure.DetectSmallestCellInMat();
            var tempBBox = CvInvoke.MinAreaRect(newContour);
            var boxPoints = tempBBox.GetVertices();
            points.Push(boxPoints.ConvertToPointArray());
            //var VONewContour = new VectorOfVectorOfPoint();
            //VONewContour.Push(newContour);

            var markedMat = matToMeasure.CreateNewHardCopyFromMat();
            //CvInvoke.DrawContours(markedMat, VONewContour, 0, new MCvScalar(127, 127, 127), 2);

            markedMat = DrawPixelsToMat(matToMeasure, points);

            MainWindow.ImageProcessorExaminer.AddImage(markedMat.CreateNewHardCopyFromMat(),
                "GetPointsOfWidestSliceOfCellSegmentedWithContour_PointsMarked");

            return (tempPointA, tempPointB);
        }

        public static Point[] ConvertToPointArray(this PointF[] ogArray)
        {
            var tempPointArray = new Point[ogArray.Length];

            for (var i = 0; i < ogArray.Length; i++)
            {
                var ogItem = ogArray[i];
                tempPointArray[i] = new Point((int) ogItem.X, (int) ogItem.Y);
            }

            return tempPointArray;
        }

        public static PointF[] LeftShift(this PointF[] array)
        {
            // all elements except for the first one... and at the end, the first one. to array.
            return array.Skip(1).Concat(array.Take(1)).ToArray();
        }

        public static PointF[] RightShift(this PointF[] array)
        {
            // the last element (because we're skipping all but one)... then all but the last one.
            return array.Skip(array.Length - 1).Concat(array.Take(array.Length - 1)).ToArray();
        }

        public static (PointF, PointF)[] LeftShift(this (PointF, PointF)[] array)
        {
            return array.Skip(1).Concat(array.Take(1)).ToArray();
        }

        public static (PointF, PointF)[] RightShift(this (PointF, PointF)[] array)
        {
            return array.Skip(array.Length - 1).Concat(array.Take(array.Length - 1)).ToArray();
        }

        public static PointF[] RotatePointsUntilLengthsAreSame(this PointF[] arrayToRotate, PointF[] reference)
        {
            var arrayToReturn = new PointF[arrayToRotate.Length];

            for (var i = 0; i < arrayToRotate.Length; i++)
            {
                arrayToReturn[i] = arrayToRotate[i];
            }

            for (var i = 0; i < arrayToRotate.Length; i++)
            {
                if (!arrayToReturn.GetLengthTriplet().LengthTripletIsSameMoreOrLess(reference.GetLengthTriplet()))
                {
                    arrayToReturn = arrayToReturn.RightShift();
                }
                else
                {
                    break;
                }
            }

            return arrayToReturn;
        }

        public static (PointF, PointF)[] RotatePointsUntilLengthsAreSame(this (PointF, PointF)[] arrayToRotate, (PointF, PointF)[] reference)
        {
            var arrayToReturn = new (PointF, PointF)[arrayToRotate.Length];

            for (var i = 0; i < arrayToRotate.Length; i++)
            {
                arrayToReturn[i] = arrayToRotate[i];
            }

            for (var i = 0; i < arrayToRotate.Length; i++)
            {
                if (!arrayToReturn.GetLengthTriplet().LengthTripletIsSameMoreOrLess(reference.GetLengthTriplet()))
                {
                    arrayToReturn = arrayToReturn.RightShift();
                }
                else
                {
                    break;
                }
            }

            return arrayToReturn;
        }

        public static (double, double, double) GetLengthTriplet(this PointF[] reference)
        {
            var length01 = reference[0].CalculateLengthBetweenPoint(reference[1]);
            var length02 = reference[0].CalculateLengthBetweenPoint(reference[2]);
            var length03 = reference[0].CalculateLengthBetweenPoint(reference[3]);
            return (length01, length02, length03);
        }

        public static (double, double, double) GetLengthTriplet(this (PointF,PointF)[] reference)
        {
            var length01 = reference[0].Item1.CalculateLengthBetweenPoint(reference[1].Item1);
            var length02 = reference[0].Item1.CalculateLengthBetweenPoint(reference[2].Item1);
            var length03 = reference[0].Item1.CalculateLengthBetweenPoint(reference[3].Item1);
            return (length01, length02, length03);
        }

        public static bool LengthTripletIsSameMoreOrLess(this (double, double, double) reference,
            (double, double, double) eval)
        {
            var delta1 = Math.Abs(reference.Item1 - eval.Item1);
            var delta2 = Math.Abs(reference.Item2 - eval.Item2);
            var delta3 = Math.Abs(reference.Item2 - eval.Item3);

            var boolToReturn = (delta1 < 1.0 && delta2 < 1.0 && Math.Sqrt(delta3) < 4.0);

            return boolToReturn;
        }

        public static double CalculateLengthBetweenPoint(this PointF first, PointF second)
        {
            var length = double.NaN;
            var alpha = double.NaN;
            var beta = double.NaN;

            alpha = first.X - second.X;
            alpha = Math.Pow(alpha, 2);

            beta = first.Y - second.Y;
            beta = Math.Pow(beta, 2);

            length = alpha + beta;
            length = Math.Sqrt(length);
            return length;
        }

        public static int GetWidestSliceOfCellLengthInPx(this Mat uprightBondingRectangleMat)
        {

            var pixelNum = CvInvoke.CountNonZero(uprightBondingRectangleMat);
            if (pixelNum < 1) throw new Exception("Selected ROI is blank!");

            var biggestAreaWindow = GetBiggestAreaOfCellWithSlidingWindowSegmented(uprightBondingRectangleMat, 5);
            MainWindow.ImageProcessorExaminer.AddImage(biggestAreaWindow.CreateNewHardCopyFromMat(),
                "GetPointsOfWidestSliceOfCell_biggestAreaWindow");
            var biggestAreaRow = GetBiggestAreaOfCellWithSlidingWindowSegmented(biggestAreaWindow, 1);
            MainWindow.ImageProcessorExaminer.AddImage(biggestAreaRow.CreateNewHardCopyFromMat(),
                "GetPointsOfWidestSliceOfCell_biggestAreaRow");
            var (firstOffset, secondOffset) = biggestAreaRow.GetCenterIdxOfDiffractionLineSlice();

            return secondOffset - firstOffset;
        }

        public static int GetWidestSliceOfCellLengthInPxWithContour(this Mat uprightBondingRectangleMat,
            VectorOfPoint contour)
        {

            var pixelNum = CvInvoke.CountNonZero(uprightBondingRectangleMat);
            if (pixelNum < 1) throw new Exception("Selected ROI is blank!");

            var biggestAreaWindow =
                GetBiggestAreaOfCellWithSlidingWindowAndContourSegmented(uprightBondingRectangleMat, 5, contour);
            MainWindow.ImageProcessorExaminer.AddImage(biggestAreaWindow.CreateNewHardCopyFromMat(),
                "GetWidestSliceOfCellLengthInPxWithContour_biggestAreaWindow");
            var biggestAreaRow =
                GetBiggestAreaOfCellWithSlidingWindowAndContourSegmented(biggestAreaWindow, 1, contour);
            MainWindow.ImageProcessorExaminer.AddImage(biggestAreaRow.CreateNewHardCopyFromMat(),
                "GetWidestSliceOfCellLengthInPxWithContour_biggestAreaRow");
            var firstOffsetToReturn = 0;
            var secondOffsetToReturn = 0;

            var cellWithContour = uprightBondingRectangleMat.CreateNewHardCopyFromMat();
            var VoVoPoint = new VectorOfVectorOfPoint();
            VoVoPoint.Push(new VectorOfPoint[] {contour});
            cellWithContour.DrawContourOnMat(VoVoPoint, new MCvScalar(0, 255, 0));
            MainWindow.ImageProcessorExaminer.AddImage(cellWithContour.CreateNewHardCopyFromMat(),
                "GetWidestSliceOfCellLengthInPxWithContour_cellWithContour");

            try
            {
                var (firstOffset, secondOffset) = biggestAreaRow.GetCenterIdxOfDiffractionLineSlice();
                firstOffsetToReturn = firstOffset;
                secondOffsetToReturn = secondOffset;

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }


            return secondOffsetToReturn - firstOffsetToReturn;
        }

        public static int GetWidestSliceOfCellLengthInPxWithContours(this Mat uprightBondingRectangleMat,
            VectorOfPoint innerContour, VectorOfPoint outerContour)
        {

            var pixelNum = CvInvoke.CountNonZero(uprightBondingRectangleMat);
            if (pixelNum < 1) throw new Exception("Selected ROI is blank!");

            var biggestAreaWindow =
                GetBiggestAreaOfCellWithSlidingWindowAndContoursSegmented(uprightBondingRectangleMat, 5, innerContour,
                    outerContour);
            MainWindow.ImageProcessorExaminer.AddImage(biggestAreaWindow.CreateNewHardCopyFromMat(),
                "GetWidestSliceOfCellLengthInPxWithContours_biggestAreaWindow");
            var biggestAreaRow =
                GetBiggestAreaOfCellWithSlidingWindowAndContoursSegmented(biggestAreaWindow, 1, innerContour,
                    outerContour);
            MainWindow.ImageProcessorExaminer.AddImage(biggestAreaRow.CreateNewHardCopyFromMat(),
                "GetWidestSliceOfCellLengthInPxWithContours_biggestAreaRow");
            var firstOffsetToReturn = 0;
            var secondOffsetToReturn = 0;

            var cellWithContour = uprightBondingRectangleMat.CreateNewHardCopyFromMat();
            var VoVoPoint = new VectorOfVectorOfPoint();
            VoVoPoint.Push(new VectorOfPoint[] {innerContour, outerContour});
            cellWithContour.DrawContourOnMat(VoVoPoint, new MCvScalar(0, 255, 0));
            MainWindow.ImageProcessorExaminer.AddImage(cellWithContour.CreateNewHardCopyFromMat(),
                "GetWidestSliceOfCellLengthInPxWithContours_cellWithContour");




            try
            {
                var (firstOffset, secondOffset) = biggestAreaRow.GetCenterIdxOfDiffractionLineSlice();
                firstOffsetToReturn = firstOffset;
                secondOffsetToReturn = secondOffset;

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }


            return secondOffsetToReturn - firstOffsetToReturn;
        }

        public static int GetWidestSliceOfCellLengthWithContourInPx(this Mat uprightBondingRectangleMat,
            VectorOfPointF contour)
        {

            var pixelNum = CvInvoke.CountNonZero(uprightBondingRectangleMat);
            if (pixelNum < 1) throw new Exception("Selected ROI is blank!");

            var biggestAreaWindow = GetBiggestAreaOfCellWithSlidingWindowSegmented(uprightBondingRectangleMat, 5);
            MainWindow.ImageProcessorExaminer.AddImage(biggestAreaWindow.CreateNewHardCopyFromMat(),
                "GetPointsOfWidestSliceOfCell_biggestAreaWindow");
            var biggestAreaRow = GetBiggestAreaOfCellWithSlidingWindowSegmented(biggestAreaWindow, 1);
            MainWindow.ImageProcessorExaminer.AddImage(biggestAreaRow.CreateNewHardCopyFromMat(),
                "GetPointsOfWidestSliceOfCell_biggestAreaRow");
            var (firstOffset, secondOffset) = biggestAreaRow.GetCenterIdxOfDiffractionLineSlice();

            return secondOffset - firstOffset;
        }

        public static (int, int) GetCenterIdxOfDiffractionLineSlice(this Mat matToMeasure)
        {
            var firstSlice = new List<int>();
            var secondSlice = new List<int>();
            var listPointer = firstSlice;
            //var firstWhiteFound = false;
            var firstBlackAfterFirstSliceFound = false;
            var prevPixelVal = -1;

            if (matToMeasure.Rows > 1) throw new Exception("Given Mat contains more than on row");
            var tempRow = matToMeasure.Row(0);
            for (var colIdx = 0; colIdx < matToMeasure.Cols; colIdx++)
            {
                var tempValue = (byte) tempRow.GetData().GetValue(0, colIdx);
                if (colIdx > 0) prevPixelVal = (byte) tempRow.GetData().GetValue(0, colIdx - 1);
                //if (!firstBlackAfterFirstSliceFound && WHITE_PIXEL != tempValue) continue;
                //if (!firstWhiteFound && WHITE_PIXEL == tempValue) firstWhiteFound = true;
                if (!firstBlackAfterFirstSliceFound && WHITE_PIXEL == tempValue)
                {
                    listPointer.Add(colIdx);
                }

                //if (!firstWhiteFound) continue;
                if (!firstBlackAfterFirstSliceFound && BLACK_PIXEL == tempValue && WHITE_PIXEL == prevPixelVal)
                {
                    firstBlackAfterFirstSliceFound = true;
                    listPointer = secondSlice;
                }

                if (firstBlackAfterFirstSliceFound && WHITE_PIXEL == tempValue)
                {
                    listPointer.Add(colIdx);
                }
            }

            return (firstSlice[firstSlice.Count / 2], secondSlice[secondSlice.Count / 2]);
        }

        public static (Mat, Mat) SliceMatInHalfHorizontally(this Mat matToSlice)
        {
            //https://answers.opencv.org/question/82641/dividing-image-horizontally-into-equal-parts/

            if (matToSlice.Rows < 2) throw new Exception("Given matrix has 1 or no rows!");
            var upperHalfRectFirstPoint = new Point(0, 0);
            var upperHalfRectSecondPoint = new Point(matToSlice.Cols, matToSlice.Rows / 2);
            var firstTempSize = new Size(upperHalfRectSecondPoint.X - upperHalfRectFirstPoint.X,
                upperHalfRectSecondPoint.Y - upperHalfRectFirstPoint.Y);
            var upperHalfRect = new Rectangle(upperHalfRectFirstPoint, firstTempSize);

            var lowerHalfRectFirstPoint = new Point(0, upperHalfRect.Height);
            var lowerHalfRectSecondPoint = new Point(matToSlice.Cols, matToSlice.Rows - upperHalfRect.Height);
            var secondTempSize = new Size(lowerHalfRectSecondPoint.X - lowerHalfRectFirstPoint.X,
                lowerHalfRectSecondPoint.Y - lowerHalfRectFirstPoint.Y);
            var lowerHalfRect = new Rectangle(lowerHalfRectFirstPoint, secondTempSize);

            var upperHalf = new Mat(matToSlice, upperHalfRect);
            var lowerHalf = new Mat(matToSlice, lowerHalfRect);

            //Talán kellene a RowRange, mert az o1

            if (upperHalf.Height + upperHalf.Height != matToSlice.Height)
                throw new Exception("Separated halves summed height does not equal the original matrix height!");

            return (upperHalf, lowerHalf);
        }

        public static (Mat, Mat) SliceMatInHalfHorizontallyUsingRange(this Mat matToSlice)
        {
            var upperHalf = GetRowsFromRange(matToSlice, 0, matToSlice.Rows / 2);
            var lowerHalf = GetRowsFromRange(matToSlice, matToSlice.Rows - upperHalf.Height, matToSlice.Rows);
            return (upperHalf, lowerHalf);
        }

        public static (Mat, Mat) SliceMatInHalfVerticallyUsingRange(this Mat matToSlice)
        {
            var upperHalf = GetColsFromRange(matToSlice, 0, matToSlice.Cols / 2);
            var lowerHalf = GetColsFromRange(matToSlice, matToSlice.Cols - upperHalf.Width, matToSlice.Cols);
            return (upperHalf, lowerHalf);
        }

        public static (Mat, Mat) SliceMatInHalfVertically(this Mat matToSlice)
        {
            //https://answers.opencv.org/question/82641/dividing-image-horizontally-into-equal-parts/
            //lehet ezt is range-gel kellene?
            if (matToSlice.Cols < 2) throw new Exception("Given matrix has 1 or no rows!");
            var leftHalfRectFirstPoint = new Point(0, 0);
            var leftHalfRectSecondPoint = new Point(matToSlice.Cols / 2, matToSlice.Height);
            var firstTempSize = new Size(leftHalfRectSecondPoint.X - leftHalfRectFirstPoint.X,
                leftHalfRectSecondPoint.Y - leftHalfRectFirstPoint.Y);
            var leftHalfRect = new Rectangle(leftHalfRectFirstPoint, firstTempSize);

            var rightHalfRectFirstPoint = new Point(0, leftHalfRect.Width);
            var rightHalfRectSecondPoint = new Point(matToSlice.Cols, matToSlice.Cols - leftHalfRect.Width);
            var secondTempSize = new Size(rightHalfRectSecondPoint.X - rightHalfRectFirstPoint.X,
                rightHalfRectSecondPoint.Y - rightHalfRectFirstPoint.Y);
            var rightHalfRect = new Rectangle(rightHalfRectFirstPoint, secondTempSize);

            var leftHalf = new Mat(matToSlice, leftHalfRect);
            var rightHalf = new Mat(matToSlice, rightHalfRect);

            //Talán kellene a RowRange, mert az o1

            if (leftHalf.Height + leftHalf.Height != matToSlice.Height)
                throw new Exception("Separated halves summed height does not equal the original matrix height!");

            return (leftHalf, rightHalf);
        }

        //Majd a forgatáshoz
        //https://stackoverflow.com/questions/38250597/rotate-roi-in-a-picture
        //https://stackoverflow.com/questions/26279853/how-to-store-all-the-pixels-within-a-rotatedrect-to-another-matrix/26284491#26284491


        public static int GetAreaOfSegmentedCellSliceWithContourSegmented(this Mat matToMeasure, int startRowIdxOffset,
            VectorOfPoint contour)
        {
            var whiteArea = 0;
            var blackArea = 0;
            var listOfPointsMarkedAsWhite = new List<PointF>();
            for (var rowIdx = 0; rowIdx < matToMeasure.Rows; rowIdx++)
            {
                var tempRow = matToMeasure.Row(rowIdx);
                var nonZeroPixelNumForRow = CvInvoke.CountNonZero(tempRow);
                if (nonZeroPixelNumForRow == 0) continue;

                var whitePixelCountForRow = 0;

                for (var colIdx = 0; colIdx < matToMeasure.Cols; colIdx++)
                {
                    //TODO eval if ok or not
                    //var tempPointF = new PointF(startRowIdxOffset + rowIdx, colIdx);
                    var tempPointF = new PointF(colIdx, startRowIdxOffset + rowIdx);
                    var tempContourDistance = CvInvoke.PointPolygonTest(contour, tempPointF, false);
                    var tempValue = (byte) tempRow.GetData().GetValue(0, colIdx);

                    if (tempContourDistance > 0 && BLACK_PIXEL == tempValue)
                    {
                        blackArea++;
                    }

                    if (!(tempContourDistance < 0) || WHITE_PIXEL != tempValue) continue;
                    whitePixelCountForRow++;
                    listOfPointsMarkedAsWhite.Add(tempPointF);
                }

                whiteArea += whitePixelCountForRow;

            }

            var pointsMarkedAsWhite = new VectorOfPointF();
            pointsMarkedAsWhite.Push(listOfPointsMarkedAsWhite.ToArray());
            var feedbackMat = matToMeasure.CreateNewHardCopyFromMat();
            CvInvoke.CvtColor(feedbackMat, feedbackMat, ColorConversion.Gray2Bgr);
            feedbackMat.DrawMarkerPixelsToMat(pointsMarkedAsWhite.ConvertToVectorOfPoint(), new MCvScalar(0, 0, 255));
            MainWindow.ImageProcessorExaminer.AddImage(feedbackMat.CreateNewHardCopyFromMat(),
                "GetAreaOfSegmentedCellSliceWithContourSegmented_feedbackMat");

            var areaToReturn = -1;
            var currentWhiteToBLackRatio = -1.0;
            if (blackArea > 0)
            {
                currentWhiteToBLackRatio = (double) whiteArea / (double) blackArea;
            }

            if (currentWhiteToBLackRatio > 0 && WHITE_TO_BLACK_RATIO > currentWhiteToBLackRatio)
            {
                areaToReturn = blackArea + whiteArea;
            }
            else
            {
                areaToReturn = blackArea;
            }

            return areaToReturn;
        }

        public static int GetAreaOfCellSliceWithContours(this Mat matToMeasure, int startRowIdxOffset,
            VectorOfPoint innerContour, VectorOfPoint outerContour)
        {
            var innerArea = 0;
            var outerRimArea = 0;
            for (var rowIdx = 0; rowIdx < matToMeasure.Rows; rowIdx++)
            {
                var tempRow = matToMeasure.Row(rowIdx);
                var nonZeroPixelNumForRow = CvInvoke.CountNonZero(tempRow);
                if (nonZeroPixelNumForRow == 0) continue;

                for (var colIdx = 0; colIdx < matToMeasure.Cols; colIdx++)
                {
                    //TODO eval if ok or not
                    //var tempPointF = new PointF(startRowIdxOffset + rowIdx, colIdx);
                    var tempPointF = new PointF(colIdx, startRowIdxOffset + rowIdx);
                    var tempInnerContourDistance = CvInvoke.PointPolygonTest(innerContour, tempPointF, false);
                    var tempOuterContourDistance = CvInvoke.PointPolygonTest(outerContour, tempPointF, false);
                    if (tempInnerContourDistance > 0)
                    {
                        innerArea++;
                    }

                    if (tempOuterContourDistance > 0 && tempInnerContourDistance < 0)
                    {
                        outerRimArea++;
                    }
                }
            }


            return innerArea + outerRimArea;
        }

        public static int GetAreaOfCellSlice(this Mat matToMeasure)
        {
            var foundFirstWhitePixelInFrontOfCellInGivenRow = false;
            var foundLastWhitePixelInFrontOfCellInGivenRow = false;
            //var foundLastBlackPixelOfCellInGivenRow = false;
            var blackCounterOfSlice = 0;
            var whiteCounterOfSlice = 0;
            var firstWhitBeforeIDx = int.MinValue;
            var lasttWhitBeforeIDx = int.MinValue;

            var prevPixelVal = -1;

            if (matToMeasure.NumberOfChannels > 1)
                throw new Exception("Given matrix does not contain a greyscale picture!");

            var nonZeroPixelNum = CvInvoke.CountNonZero(matToMeasure);
            if (nonZeroPixelNum == 0)
                throw new Exception(
                    "Given image in the matrix has no proper threshold applied! - Only black pixels in Matrix!");
            var segmentationSet = new HashSet<byte>();
            foreach (var rowElement in matToMeasure.GetData())
            {
                var tempValue = (byte) rowElement;
                if (segmentationSet.Contains(tempValue)) continue;
                segmentationSet.Add(tempValue);
                if (segmentationSet.Count > 2)
                {
                    throw new Exception(
                        "Given image in the matrix has no proper threshold applied! - More than two values in segmented Matrix!");
                }
            }

            if (!(segmentationSet.Remove(BLACK_PIXEL) && segmentationSet.Remove(WHITE_PIXEL)))
            {
                throw new Exception(
                    "Given image in the matrix has no proper threshold applied! - The two segmentation values were not equal to BLACK_PIXEL and WHITE_PIXEL values");
            }

            for (var rowIdx = 0; rowIdx < matToMeasure.Rows; rowIdx++)
            {
                var tempRow = matToMeasure.Row(rowIdx);
                var nonZeroPixelNumForRow = CvInvoke.CountNonZero(tempRow);
                if (nonZeroPixelNumForRow == 0) continue;

                for (var colIdx = 0; colIdx < matToMeasure.Cols; colIdx++)
                {
                    //var tempObject = tempRow.GetData().GetValue(0, colIdx);
                    var tempValue = (byte) tempRow.GetData().GetValue(0, colIdx);
                    if (colIdx > 0) prevPixelVal = (byte) tempRow.GetData().GetValue(0, colIdx - 1);

                    if (!foundFirstWhitePixelInFrontOfCellInGivenRow && BLACK_PIXEL == tempValue)
                    {
                        continue;
                    }

                    if (!foundFirstWhitePixelInFrontOfCellInGivenRow && WHITE_PIXEL == tempValue &&
                        BLACK_PIXEL == prevPixelVal)
                    {
                        foundFirstWhitePixelInFrontOfCellInGivenRow = true;
                        firstWhitBeforeIDx = colIdx;
                    }

                    if (foundFirstWhitePixelInFrontOfCellInGivenRow && BLACK_PIXEL == tempValue &&
                        WHITE_PIXEL == prevPixelVal)
                    {
                        foundLastWhitePixelInFrontOfCellInGivenRow = true;
                        lasttWhitBeforeIDx = colIdx;
                    }

                    if (foundLastWhitePixelInFrontOfCellInGivenRow && BLACK_PIXEL == tempValue)
                    {
                        blackCounterOfSlice++;
                    }

                    if (!foundLastWhitePixelInFrontOfCellInGivenRow || BLACK_PIXEL != prevPixelVal ||
                        WHITE_PIXEL != tempValue) continue;
                    //foundLastBlackPixelOfCellInGivenRow = true;
                    foundFirstWhitePixelInFrontOfCellInGivenRow = false;
                    foundLastWhitePixelInFrontOfCellInGivenRow = false;
                    //foundLastBlackPixelOfCellInGivenRow = false;
                    prevPixelVal = -1;
                    break;
                }
            }

            return blackCounterOfSlice;
        }

        public static int GetWholeAreaOfCellSlice(this Mat matToMeasure)
        {
            var foundFirstWhitePixelInFrontOfCellInGivenRow = false;
            var foundLastWhitePixelInFrontOfCellInGivenRow = false;
            //var foundLastBlackPixelOfCellInGivenRow = false;
            var blackCounterOfSlice = 0;
            var whiteCounterOfSlice = 0;

            var prevPixelVal = -1;

            if (matToMeasure.NumberOfChannels > 1)
                throw new Exception("Given matrix does not contain a greyscale picture!");

            var nonZeroPixelNum = CvInvoke.CountNonZero(matToMeasure);
            if (nonZeroPixelNum == 0)
                throw new Exception(
                    "Given image in the matrix has no proper threshold applied! - Only black pixels in Matrix!");
            var segmentationSet = new HashSet<byte>();
            foreach (var rowElement in matToMeasure.GetData())
            {
                var tempValue = (byte) rowElement;
                if (segmentationSet.Contains(tempValue)) continue;
                segmentationSet.Add(tempValue);
                if (segmentationSet.Count > 2)
                {
                    throw new Exception(
                        "Given image in the matrix has no proper threshold applied! - More than two values in segmented Matrix!");
                }
            }

            if (!(segmentationSet.Remove(BLACK_PIXEL) && segmentationSet.Remove(WHITE_PIXEL)))
            {
                throw new Exception(
                    "Given image in the matrix has no proper threshold applied! - The two segmentation values were not equal to BLACK_PIXEL and WHITE_PIXEL values");
            }

            for (var rowIdx = 0; rowIdx < matToMeasure.Rows; rowIdx++)
            {
                var tempRow = matToMeasure.Row(rowIdx);
                var whitePixelNumForRow = CvInvoke.CountNonZero(tempRow);
                var blackPixelNumForRow = matToMeasure.Cols - whitePixelNumForRow;
                var selectedBlackPixelNumForRow = 0;
                if (whitePixelNumForRow == 0) continue;

                for (var colIdx = 0; colIdx < matToMeasure.Cols; colIdx++)
                {
                    //var tempObject = tempRow.GetData().GetValue(0, colIdx);
                    var tempValue = (byte) tempRow.GetData().GetValue(0, colIdx);
                    if (colIdx > 0) prevPixelVal = (byte) tempRow.GetData().GetValue(0, colIdx - 1);

                    if (!foundFirstWhitePixelInFrontOfCellInGivenRow && BLACK_PIXEL == tempValue)
                    {
                        continue;
                    }

                    if (!foundFirstWhitePixelInFrontOfCellInGivenRow && WHITE_PIXEL == tempValue &&
                        BLACK_PIXEL == prevPixelVal)
                    {
                        foundFirstWhitePixelInFrontOfCellInGivenRow = true;
                    }

                    if (foundFirstWhitePixelInFrontOfCellInGivenRow && BLACK_PIXEL == tempValue &&
                        WHITE_PIXEL == prevPixelVal)
                    {
                        foundLastWhitePixelInFrontOfCellInGivenRow = true;
                    }

                    if (foundLastWhitePixelInFrontOfCellInGivenRow && BLACK_PIXEL == tempValue)
                    {
                        blackCounterOfSlice++;
                        selectedBlackPixelNumForRow++;
                    }

                    if (!foundLastWhitePixelInFrontOfCellInGivenRow || BLACK_PIXEL != prevPixelVal ||
                        WHITE_PIXEL != tempValue) continue;
                    //foundLastBlackPixelOfCellInGivenRow = true;
                    //blackCounterOfSlice += selectedBlackPixelNumForRow;
                    if (selectedBlackPixelNumForRow > whitePixelNumForRow)
                    {
                        whiteCounterOfSlice += whitePixelNumForRow;
                    }

                    foundFirstWhitePixelInFrontOfCellInGivenRow = false;
                    foundLastWhitePixelInFrontOfCellInGivenRow = false;
                    //foundLastBlackPixelOfCellInGivenRow = false;
                    prevPixelVal = -1;
                    break;
                }
            }

            return blackCounterOfSlice + whiteCounterOfSlice;
        }

        public static (Mat, Mat) SliceMatInHalfVerticallyAlt(this Mat matToSlice)
        {
            //https://answers.opencv.org/question/82641/dividing-image-horizontally-into-equal-parts/
            //leget ezt is range-gel kellene?
            if (matToSlice.Cols < 2) throw new Exception("Given matrix has 1 or no rows!");
            var leftHalfRectFirstPoint = new Point(0, 0);
            var leftHalfRectSecondPoint = new Point(matToSlice.Cols / 2, matToSlice.Height);
            var firstTempSize = new Size(leftHalfRectSecondPoint.X - leftHalfRectFirstPoint.X,
                leftHalfRectSecondPoint.Y - leftHalfRectFirstPoint.Y);
            var leftHalfRect = new Rectangle(leftHalfRectFirstPoint, firstTempSize);

            var rightHalfRectFirstPoint = new Point(0, leftHalfRect.Width);
            var rightHalfRectSecondPoint = new Point(matToSlice.Cols, matToSlice.Cols - leftHalfRect.Width);
            var secondTempSize = new Size(rightHalfRectSecondPoint.X - rightHalfRectFirstPoint.X,
                rightHalfRectSecondPoint.Y - rightHalfRectFirstPoint.Y);
            var rightHalfRect = new Rectangle(rightHalfRectFirstPoint, secondTempSize);

            var leftHalf = new Mat(matToSlice, leftHalfRect);
            var rightHalf = new Mat(matToSlice, rightHalfRect);

            //Talán kellene a RowRange, mert az o1

            if (leftHalf.Height + leftHalf.Height != matToSlice.Height)
                throw new Exception("Separated halves summed height does not equal the original matrix height!");

            return (leftHalf, rightHalf);
        }

        //Majd a forgatáshoz
        //https://stackoverflow.com/questions/38250597/rotate-roi-in-a-picture
        //https://stackoverflow.com/questions/26279853/how-to-store-all-the-pixels-within-a-rotatedrect-to-another-matrix/26284491#26284491


        public static Mat TrimMat(this Mat matToTrim)
        {
            var matToReturn = matToTrim.CreateNewMatLikeThis();

            var nonZeroPixelNum = CvInvoke.CountNonZero(matToTrim);
            if (nonZeroPixelNum == 0) throw new Exception("Given image in the matrix is empty!");


            for (var rowIdx = 0; rowIdx < matToTrim.Rows; rowIdx++)
            {
                var tempRow = matToTrim.Row(rowIdx);
                var nonZeroPixelNumForRow = CvInvoke.CountNonZero(tempRow);
                if (nonZeroPixelNumForRow == 0)
                {
                    matToReturn.PushBack(tempRow);
                    continue;
                }


                var trimHeader = new Mat();
                var alphaIdx = int.MinValue;
                var omegaIdx = int.MaxValue;
                var prevPixelVal = int.MinValue;
                var foundFirstWhitePixelInFrontOfCellInGivenRow = false;
                var listOfWhitePixelIdxs = new List<int>();
                for (var colIdx = 0; colIdx < matToTrim.Cols; colIdx++)
                {
                    var tempValue = (byte) tempRow.GetData().GetValue(0, colIdx);
                    if (colIdx > 0) prevPixelVal = (byte) tempRow.GetData().GetValue(0, colIdx - 1);

                    if (!foundFirstWhitePixelInFrontOfCellInGivenRow && BLACK_PIXEL == tempValue)
                    {
                        continue;
                    }

                    if (!foundFirstWhitePixelInFrontOfCellInGivenRow && WHITE_PIXEL == tempValue &&
                        BLACK_PIXEL == prevPixelVal)
                    {
                        foundFirstWhitePixelInFrontOfCellInGivenRow = true;
                    }

                    if (foundFirstWhitePixelInFrontOfCellInGivenRow && WHITE_PIXEL == tempValue)
                    {
                        listOfWhitePixelIdxs.Add(colIdx);
                    }


                }
            }

            return matToReturn;
        }




        //https://stackoverflow.com/questions/4122527/sliding-window-minimum-algorithm
        //Az nem derült ki, hogy mekkora legyen a sliding window mérete, de lehet azt paraméterezni kellene, és kezdjük öttel

        public static Mat GetBiggestAreaOfCellWithSlidingWindowSegmented(this Mat matToMeasure, int slidingWindowSize)
        {
            var biggestAreSoFar = -1;
            var returnMat = new Mat();
            //var slidingWindowMat = new Mat();


            for (var rowIdx = 0; rowIdx + slidingWindowSize - 1 < matToMeasure.Rows; rowIdx++)
            {
                var slidingWindowMat = matToMeasure.GetRowsFromRange(rowIdx, rowIdx + slidingWindowSize - 1);
                var nonZeroPixelNum = CvInvoke.CountNonZero(slidingWindowMat);
                if (nonZeroPixelNum == 0) continue;
                var slidingWindowArea = slidingWindowMat.GetAreaOfCellSlice();
                if (biggestAreSoFar >= slidingWindowArea) continue; // itt jó kérdés, hogy az egyenlősgéet megengedjüke
                biggestAreSoFar = slidingWindowArea;
                returnMat = slidingWindowMat;
            }

            return returnMat;
        }

        public static Mat GetBiggestAreaOfCellWithSlidingWindowAndContourSegmented(this Mat matToMeasure,
            int slidingWindowSize, VectorOfPoint contour)
        {
            var biggestAreSoFar = -1;
            var returnMat = new Mat();
            //var slidingWindowMat = new Mat();


            for (var rowIdx = 0; rowIdx + slidingWindowSize - 1 < matToMeasure.Rows; rowIdx++)
            {
                var slidingWindowMat = matToMeasure.GetRowsFromRange(rowIdx, rowIdx + slidingWindowSize - 1);
                var nonZeroPixelNum = CvInvoke.CountNonZero(slidingWindowMat);
                if (nonZeroPixelNum == 0) continue;
                var slidingWindowArea =
                    slidingWindowMat.GetAreaOfSegmentedCellSliceWithContourSegmented(rowIdx, contour);
                if (biggestAreSoFar >= slidingWindowArea) continue; // itt jó kérdés, hogy az egyenlősgéet megengedjüke
                biggestAreSoFar = slidingWindowArea;
                returnMat = slidingWindowMat;
            }

            return returnMat;
        }

        public static Mat GetBiggestAreaOfCellWithSlidingWindowAndContoursSegmented(this Mat matToMeasure,
            int slidingWindowSize, VectorOfPoint innerContour, VectorOfPoint outerContour)
        {
            var biggestAreSoFar = -1;
            var returnMat = new Mat();
            //var slidingWindowMat = new Mat();


            for (var rowIdx = 0; rowIdx + slidingWindowSize - 1 < matToMeasure.Rows; rowIdx++)
            {
                var slidingWindowMat = matToMeasure.GetRowsFromRange(rowIdx, rowIdx + slidingWindowSize - 1);
                var nonZeroPixelNum = CvInvoke.CountNonZero(slidingWindowMat);
                if (nonZeroPixelNum == 0) continue;
                var slidingWindowArea =
                    slidingWindowMat.GetAreaOfCellSliceWithContours(rowIdx, innerContour, outerContour);
                if (biggestAreSoFar >= slidingWindowArea) continue; // itt jó kérdés, hogy az egyenlősgéet megengedjüke
                biggestAreSoFar = slidingWindowArea;
                returnMat = slidingWindowMat;
            }

            return returnMat;
        }

        public static Mat GetBiggestAreaOfCellWithSlidingWindow(this Mat matToMeasure, int slidingWindowSize)
        {
            var biggestAreSoFar = -1;
            var returnMat = new Mat();
            //var slidingWindowMat = new Mat();


            for (var rowIdx = 0; rowIdx + slidingWindowSize - 1 < matToMeasure.Rows; rowIdx++)
            {
                var slidingWindowMat = matToMeasure.GetRowsFromRange(rowIdx, rowIdx + slidingWindowSize - 1);
                var nonZeroPixelNum = CvInvoke.CountNonZero(slidingWindowMat);
                if (nonZeroPixelNum == 0) continue;
                var slidingWindowArea = slidingWindowMat.GetAreaOfCellSlice();
                if (biggestAreSoFar >= slidingWindowArea) continue; // itt jó kérdés, hogy az egyenlősgéet megengedjüke
                biggestAreSoFar = slidingWindowArea;
                returnMat = slidingWindowMat;
            }

            return returnMat;
        }

        public static (Mat, int) GetBiggestAreaOfCellWithSlidingWindowAndRowIndex(this Mat matToMeasure,
            int slidingWindowSize)
        {
            var biggestAreSoFar = -1;
            var rowIdxToReturn = -1;
            var returnMat = new Mat();
            //var slidingWindowMat = new Mat();


            for (var rowIdx = 0; rowIdx + slidingWindowSize - 1 < matToMeasure.Rows; rowIdx++)
            {
                var slidingWindowMat = matToMeasure.GetRowsFromRange(rowIdx, rowIdx + slidingWindowSize - 1);
                var nonZeroPixelNum = CvInvoke.CountNonZero(slidingWindowMat);
                if (nonZeroPixelNum == 0) continue;
                var slidingWindowArea = slidingWindowMat.GetAreaOfCellSlice();
                //var slidingWindowAreaAlt = slidingWindowMat.GetWholeAreaOfCellSlice();
                if (biggestAreSoFar >= slidingWindowArea) continue; // itt jó kérdés, hogy az egyenlősgéet megengedjüke
                biggestAreSoFar = slidingWindowArea;
                returnMat = slidingWindowMat;
                rowIdxToReturn = rowIdx;
            }

            return (returnMat, rowIdxToReturn);
        }

        public static (Mat, int) GetBiggestAreaOfCellWithSlidingWindowAndRowIndexWithContour(this Mat matToMeasure,
            int slidingWindowSize, int rowIdxOffset, VectorOfPoint vector)
        {
            var biggestAreSoFar = -1;
            var rowIdxToReturn = -1;
            var returnMat = new Mat();
            //var slidingWindowMat = new Mat();


            for (var rowIdx = 0; rowIdx + slidingWindowSize - 1 < matToMeasure.Rows; rowIdx++)
            {
                var slidingWindowMat = matToMeasure.GetRowsFromRange(rowIdx, rowIdx + slidingWindowSize - 1);
                var nonZeroPixelNum = CvInvoke.CountNonZero(slidingWindowMat);
                if (nonZeroPixelNum == 0) continue;
                var slidingWindowArea =
                    slidingWindowMat.GetAreaOfSegmentedCellSliceWithContourSegmented(rowIdx, vector);
                //var slidingWindowAreaAlt = slidingWindowMat.GetWholeAreaOfCellSlice();
                if (biggestAreSoFar >= slidingWindowArea) continue; // itt jó kérdés, hogy az egyenlősgéet megengedjüke
                biggestAreSoFar = slidingWindowArea;
                returnMat = slidingWindowMat;
                rowIdxToReturn = rowIdx;
            }

            return (returnMat, rowIdxToReturn);
        }

        public static (Mat, int) GetBiggestAreaOfCellWithSlidingWindowAndRowIndexWithContours(this Mat matToMeasure,
            int slidingWindowSize, int rowIdxOffset, VectorOfPoint innerContour, VectorOfPoint outerContour)
        {
            var biggestAreSoFar = -1;
            var rowIdxToReturn = -1;
            var returnMat = new Mat();
            //var slidingWindowMat = new Mat();


            for (var rowIdx = 0; rowIdx + slidingWindowSize - 1 < matToMeasure.Rows; rowIdx++)
            {
                var slidingWindowMat = matToMeasure.GetRowsFromRange(rowIdx, rowIdx + slidingWindowSize - 1);
                var nonZeroPixelNum = CvInvoke.CountNonZero(slidingWindowMat);
                if (nonZeroPixelNum == 0) continue;
                var slidingWindowArea =
                    slidingWindowMat.GetAreaOfCellSliceWithContours(rowIdx + rowIdxOffset, innerContour, outerContour);
                //var slidingWindowAreaAlt = slidingWindowMat.GetWholeAreaOfCellSlice();
                if (biggestAreSoFar >= slidingWindowArea) continue; // itt jó kérdés, hogy az egyenlősgéet megengedjüke
                biggestAreSoFar = slidingWindowArea;
                returnMat = slidingWindowMat;
                rowIdxToReturn = rowIdx;
            }

            return (returnMat, rowIdxToReturn);
        }

        //kell majd ilyen in and out dolog, hogy a koordináták ne vesszenek el
        public static Mat GetRowsFromRange(this Mat matToSlice, int rowStartIdx, int rowEndIdx)
        {
            /*
            var cornerA = new Point(0, rowStartIdx);
            var cornerB = new Point(matToSlice.Width, rowEndIdx);
            var tempSize = new Size(cornerB.X - cornerA.X, cornerB.Y - cornerA.Y);
            var slice = new Rectangle(cornerA, tempSize);

            var returnMat = new Mat(matToSlice, slice);
            */
            //ez remélhetőleg csak a headert állítja és jóóóó gyors lesz :)
            var rowRange = new Range(rowStartIdx, rowEndIdx + 1);
            var colRange = new Range(0, matToSlice.Cols);
            var returnMat = new Mat(matToSlice, rowRange, colRange);
            return returnMat;
        }

        public static Mat GetColsFromRange(this Mat matToSlice, int colStartIDx, int colEndIdx)
        {
            var rowRange = new Range(0, matToSlice.Rows);
            var colRange = new Range(colStartIDx, colEndIdx + 1);
            var returnMat = new Mat(matToSlice, rowRange, colRange);
            return returnMat;
        }

        //https://stackoverflow.com/questions/32255440/how-can-i-get-and-set-pixel-values-of-an-emgucv-mat-image

        public static dynamic GetValue(this Mat mat, int row, int col)
        {
            var value = CreateElement(mat.Depth);
            Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
            return value[0];
        }

        public static void SetValue(this Mat mat, int row, int col, dynamic value)
        {
            var target = CreateElement(mat.Depth, value);
            Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);
        }

        private static dynamic CreateElement(DepthType depthType, dynamic value)
        {
            var element = CreateElement(depthType);
            element[0] = value;
            return element;
        }

        private static dynamic CreateElement(DepthType depthType)
        {
            return depthType switch
            {
                DepthType.Cv8S => new sbyte[1],
                DepthType.Cv8U => new byte[1],
                DepthType.Cv16S => new short[1],
                DepthType.Cv16U => new ushort[1],
                DepthType.Cv32S => new int[1],
                DepthType.Cv32F => new float[1],
                DepthType.Cv64F => new double[1],
                _ => new float[1]
            };
        }


        public static PointF Multiply(this PointF basePointF, double amount)
        {
            var tempPointF = new PointF(basePointF.X * (float) amount, basePointF.Y * (float) amount);
            return tempPointF;
        }

        public static PointF Add(this PointF basePointA, PointF basePointB)
        {
            var tempPointF = new PointF(basePointA.X + basePointB.X, basePointA.Y + basePointB.Y);
            return tempPointF;
        }

        public static PointF Subtract(this PointF basePointA, PointF basePointB)
        {
            var tempPointF = new PointF(basePointA.X - basePointB.X, basePointA.Y - basePointB.Y);
            return tempPointF;
        }

        public static void GetContourIntensitySet(this Mat matToMeasure, VectorOfPoint contour)
        {
            //https://stackoverflow.com/questions/56905592/automatic-contrast-and-brightness-adjustment-of-a-color-photo-of-a-sheet-of-pape


            var vm = new VectorOfMat();
            vm.Push(matToMeasure);
            var histogram = new Mat();
            CvInvoke.CalcHist(vm, new int[] {0}, new Mat(), histogram, new int[] {255}, new float[] {0f, 256f}, false);
            //Calculate cumulative distribution from the histogram
            var accumulator = new List<float>();
            var tempRow = histogram.Col(0);
            var first = (float) tempRow.GetData().GetValue(0, 0);
            accumulator.Add(first);
            for (var rowIdx = 1; rowIdx < tempRow.Rows; rowIdx++)
            {
                var tempValue = (float) tempRow.GetData().GetValue(rowIdx, 0);
                accumulator.Add(accumulator[rowIdx - 1] + tempValue);
            }

            //Locate points to clip
            var maximum = accumulator[accumulator.Count - 1];
            var clipHistPrecent = 1.0;
            clipHistPrecent *= (maximum / 100.0);
            clipHistPrecent /= 2.0;
            //Locate left cut
            var minGray = 0;
            while (accumulator[minGray] < clipHistPrecent)
            {
                minGray++;
            }

            //Locate right cut
            var maxGray = histogram.Rows - 1;
            while (accumulator[maxGray] >= (maximum - clipHistPrecent))
            {
                maxGray--;
            }

            var alpha = 255.0 / (maxGray - minGray);
            var beta = -minGray * alpha;


            var normalizedMat = matToMeasure.CreateNewHardCopyFromMat();
            //var minIntensity = -1.0;
            //var minIntensityIdxs = new int[] { };
            //var maxIntensity = -1.0;
            //var maxIntensityIdxs = new int[] { };

            //CvInvoke.MinMaxIdx(normalizedMat,out minIntensity,out maxIntensity, minIntensityIdxs, maxIntensityIdxs, new Mat());

            //var alpha = 255 / (maxIntensity / minIntensity);
            //var beta = -minIntensity * alpha;


            //CvInvoke.Normalize(normalizedMat, normalizedMat, alpha, beta, NormType.MinMax,matToMeasure.Depth);

            CvInvoke.ConvertScaleAbs(normalizedMat, normalizedMat, alpha, beta);


            var contourIntensitySet = new HashSet<byte>();
            var innerIntensitySet = new HashSet<byte>();
            var outerIntensitySet = new HashSet<byte>();

            for (var rowIdx = 0; rowIdx < normalizedMat.Rows; rowIdx++)
            {
                var tempMatRow = normalizedMat.Row(rowIdx);

                for (var colIdx = 0; colIdx < normalizedMat.Cols; colIdx++)
                {
                    var tempValue = (byte) tempMatRow.GetData().GetValue(0, colIdx);
                    var tempPointF = new Point(rowIdx, colIdx);
                    //var tempPointF = new Point (colIdx,rowIdx);
                    var dist = CvInvoke.PointPolygonTest(contour, tempPointF, false);
                    if (dist < 0) outerIntensitySet.Add(tempValue);
                    else if (dist > 0) innerIntensitySet.Add(tempValue);
                    else if (dist == 0) contourIntensitySet.Add(tempValue);
                }
            }

            MainWindow.ImageProcessorExaminer.AddImage(normalizedMat.CreateNewHardCopyFromMat(),
                "GetContourIntensitySet_normalizedMat");
        }

        public static VectorOfPointF GetDiffractionBandHalvingPointsToDrawBack((Point,Point) sizeEndPoint, VectorOfPoint refContour, VectorOfPoint evalContour)
        {
            var angledBoundingRectangle = CvInvoke.MinAreaRect(refContour);
            var ogBBoxPoints = angledBoundingRectangle.GetVertices();
            var refBBoxPoints = CvInvoke.MinAreaRect(evalContour).GetVertices();
            ogBBoxPoints = ogBBoxPoints.RotatePointsUntilLengthsAreSame(refBBoxPoints);
            var locTriangleOne = new LocatorTriangle(refBBoxPoints[0], refBBoxPoints[1], sizeEndPoint.Item1);
            locTriangleOne.CalculateNewPPosition(ogBBoxPoints[0], ogBBoxPoints[1]);
            var locTriangleTwo = new LocatorTriangle(refBBoxPoints[2], refBBoxPoints[3], sizeEndPoint.Item2);
            locTriangleTwo.CalculateNewPPosition(ogBBoxPoints[2], ogBBoxPoints[3]);
            var points = new VectorOfPointF();
            points.Push(new PointF[] { locTriangleOne.TransformedPointPPlus, locTriangleTwo.TransformedPointPPlus });
            return points;
        }

        public static string ToPrintableString(this VectorOfPointF vector)
        {
            var temp = "";
            foreach (var v in vector.ToArray().Reverse())
            {
                temp += v.ToString();
                temp += "\r\n";
            }
            return temp;
        }

        public static (PointF,PointF) getSizeWithMomentAndContours(this Mat matToMeasure)
        {
            var firstPoint = PointF.Empty;
            var secondPoint = PointF.Empty;

            //csökkenőbe van rendezve a két kontúr
            var detectedContours = matToMeasure.DetectCellContoursInMat();
            var outerCircle = CvInvoke.MinEnclosingCircle(detectedContours[0].ConvertToVectorOfPoint().ToArray());
            var innerCircle = CvInvoke.MinEnclosingCircle(detectedContours[1].ConvertToVectorOfPoint().ToArray());
            //https://www.youtube.com/watch?v=Zd68AthoNIw
            //a két kontúrnak ugyanúgy kellene állnia
            var outerCircleCenterPoint = detectedContours[0].GetContourCenterPointF();
            var tempMinAreaRect = CvInvoke.MinAreaRect(detectedContours[0]);
            var isHeightLonger = tempMinAreaRect.Size.Height > tempMinAreaRect.Size.Width;

            //CvInvoke.FitLine();



            return (firstPoint, secondPoint);
        }
    }
}
