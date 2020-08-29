using System;
using System.Collections.Generic;
using System.Drawing;
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
    internal static class ExtensionMethods
    {
        public static Mat CreateNewHardCopyFromMat(this Mat matToHardCopy)
        {
            var returnMat = new Mat(matToHardCopy.Rows, matToHardCopy.Cols, matToHardCopy.Depth, matToHardCopy.NumberOfChannels);
            matToHardCopy.CopyTo(returnMat);
            return returnMat;
        }

        public static Mat CreateNewMatLikeThis(this Mat matToHardCopy)
        {
            var returnMat = new Mat(matToHardCopy.Rows, matToHardCopy.Cols, matToHardCopy.Depth, matToHardCopy.NumberOfChannels);
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

        public static Mat RotateMatWithoutCutoff(this Mat matToRotate, RotatedRect boundingRectangle)
        {
            //https://stackoverflow.com/questions/22041699/rotate-an-image-without-cropping-in-opencv-in-c
            //var tempRectangle = boundingRectangle.MinAreaRect();
            var height = matToRotate.Height;
            var width = matToRotate.Width;
            var center = new Point(width / 2, height / 2);
            //var rotatingMat = matToRotate.CreateNewMatLikeThis();
            //CvInvoke.GetRotationMatrix2D(center, boundingRectangle.Angle, 1.0, rotatingMat);
            //var roiBoundingRectangle = new RotatedRect(center,new SizeF(width,height),boundingRectangle.Angle).MinAreaRect();


            //rotatingMat.GetData().

            var matToReturn = matToRotate.CreateNewHardCopyFromMat();
            //TODO befelyezni :|
            //https://www.pyimagesearch.com/2017/01/02/rotate-images-correctly-with-opencv-and-python/
            return matToReturn;
        }

        public static (Point, Point) GetPointsOfWidestSliceOfCell(this Mat matToMeasure, Point cornerA, Point cornerB)
        {
            //Kell majd hozzá a bounding box
            //https://stackoverflow.com/questions/15043152/rotate-opencv-matrix-by-90-180-270-degrees
            //https://jayrambhia.wordpress.com/2012/09/20/roi-bounding-box-selection-of-mat-images-in-opencv/
            //https://www.pyimagesearch.com/2017/01/02/rotate-images-correctly-with-opencv-and-python/
            //var roiMat = matToMeasure.CreateNewHardCopyFromMat();
            var tempA = new Point(0);
            var tempB = new Point(0);
            var tempSize = new Size(cornerB.X - cornerA.X, cornerB.Y - cornerA.Y);
            var roiRectangle = new Rectangle(cornerA, tempSize);
            var roiMat = new Mat(matToMeasure,roiRectangle);
            //MVCRect tempRect = new MVSRect();

            //roiMat = roiMat(roiRectangle);

            var biggestAreaWindow = GetBiggestAreaOfCellWithSlidingWindow(roiMat, 5);
            var biggestAreaRow = GetBiggestAreaOfCellWithSlidingWindow(biggestAreaWindow, 1);
            var (firstOffset, secondOffset) = biggestAreaRow.GetCenterIdxOfDiffractionLineSlice();

            return (tempA, tempB);
        }

        /*public static Mat GetWidestSliceOfCell(this Mat matToMeasure)
        {
            var returnMat = new Mat();



            return returnMat;
        }*/

        public static (int, int) GetCenterIdxOfDiffractionLineSlice(this Mat matToMeasure)
        {
            var firstSlice = new List<int>();
            var secondSlice = new List<int>();
            var firstWhiteFound = false;
            var firstBlackAfterFirstSlice = false;

            if(matToMeasure.Rows > 1) throw new Exception("Given Mat contains more than on row");
            var tempRow = matToMeasure.Row(0);
            for (var colIdx = 0; colIdx < matToMeasure.Cols; colIdx++)
            {
                var tempValue = (int)tempRow.GetData().GetValue(0, colIdx, 0);
                if (0 != tempValue) continue;
                if (!firstWhiteFound) firstWhiteFound = true;
                firstSlice.Add(colIdx);
                if (!firstWhiteFound) continue;
                if (!firstBlackAfterFirstSlice && 255 == tempValue) firstBlackAfterFirstSlice = true;
                if(firstBlackAfterFirstSlice && 0 == tempValue) secondSlice.Add(colIdx);
            }
            return (firstSlice[firstSlice.Count / 2], secondSlice[secondSlice.Count / 2]);
        }

        public static (Mat, Mat) SliceMatInHalfHorizontally(this Mat matToSlice)
        {//https://answers.opencv.org/question/82641/dividing-image-horizontally-into-equal-parts/

            if(matToSlice.Rows < 2) throw new Exception("Given matrix has 1 or no rows!");
            var upperHalfRectFirstPoint = new Point(0,0);
            var upperHalfRectSecondPoint = new Point(matToSlice.Cols, matToSlice.Rows / 2);
            var firstTempSize = new Size(upperHalfRectSecondPoint.X - upperHalfRectFirstPoint.X, upperHalfRectSecondPoint.Y - upperHalfRectFirstPoint.Y);
            var upperHalfRect = new Rectangle(upperHalfRectFirstPoint, firstTempSize);

            var lowerHalfRectFirstPoint = new Point(0, upperHalfRect.Height);
            var lowerHalfRectSecondPoint = new Point(matToSlice.Cols, matToSlice.Rows - upperHalfRect.Height);
            var secondTempSize = new Size(lowerHalfRectSecondPoint.X - lowerHalfRectFirstPoint.X, lowerHalfRectSecondPoint.Y - lowerHalfRectFirstPoint.Y);
            var lowerHalfRect = new Rectangle(lowerHalfRectFirstPoint, secondTempSize);

            var upperHalf = new Mat(matToSlice, upperHalfRect);
            var lowerHalf = new Mat(matToSlice, lowerHalfRect);

            //Talán kellene a RowRange, mert az o1

            if(upperHalf.Height + upperHalf.Height != matToSlice.Height) throw new Exception("Separated halves summed height does not equal the original matrix height!");

            return (upperHalf, lowerHalf);
        }

        public static (Mat, Mat) SliceMatInHalfHorizontallyUsingRange(this Mat matToSlice)
        {
            var upperHalf = GetRowsFromRange(matToSlice,0, matToSlice.Rows / 2);
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
        {//https://answers.opencv.org/question/82641/dividing-image-horizontally-into-equal-parts/
            //leget ezt is range-gel kellene?
            if (matToSlice.Cols < 2) throw new Exception("Given matrix has 1 or no rows!");
            var leftHalfRectFirstPoint = new Point(0, 0);
            var leftHalfRectSecondPoint = new Point(matToSlice.Cols / 2, matToSlice.Height );
            var firstTempSize = new Size(leftHalfRectSecondPoint.X - leftHalfRectFirstPoint.X, leftHalfRectSecondPoint.Y - leftHalfRectFirstPoint.Y);
            var leftHalfRect = new Rectangle(leftHalfRectFirstPoint, firstTempSize);

            var rightHalfRectFirstPoint = new Point(0, leftHalfRect.Width);
            var rightHalfRectSecondPoint = new Point(matToSlice.Cols, matToSlice.Cols - leftHalfRect.Width);
            var secondTempSize = new Size(rightHalfRectSecondPoint.X - rightHalfRectFirstPoint.X, rightHalfRectSecondPoint.Y - rightHalfRectFirstPoint.Y);
            var rightHalfRect = new Rectangle(rightHalfRectFirstPoint, secondTempSize);

            var leftHalf = new Mat(matToSlice, leftHalfRect);
            var rightHalf = new Mat(matToSlice, rightHalfRect);

            //Talán kellene a RowRange, mert az o1

            if (leftHalf.Height + leftHalf.Height != matToSlice.Height) throw new Exception("Separated halves summed height does not equal the original matrix height!");

            return (leftHalf, rightHalf);
        }

        //Majd a forgatáshoz
        //https://stackoverflow.com/questions/38250597/rotate-roi-in-a-picture
        //https://stackoverflow.com/questions/26279853/how-to-store-all-the-pixels-within-a-rotatedrect-to-another-matrix/26284491#26284491
        public static int GetAreaOfCellSlice(this Mat matToMeasure)
        {
            var foundFirstWhitePixelInFrontOfCell = false;
            var foundLastWhitePixelInFrontOfCell = false;
            var foundLastBlackPixelOfCell = false;
            var counter = 0;

            var prevPixelVal = -1;

            if(matToMeasure.Dims > 1) throw new Exception("Given matrix does not contain a greyscale picture!");
            //var tempArray = matToMeasure.Data;
            /*
            for (var colIdx = 0; colIdx < matToMeasure.Cols; colIdx++)

            {
                for (var rowIdx = 0; rowIdx < matToMeasure.Rows; rowIdx++)
                {
                    var tempValue = (int)matToMeasure.GetData().GetValue(colIdx, rowIdx, 0);
                    if(0 < tempValue || tempValue < 255 ) throw new Exception("Given image in the matrix has no proper threshold applied!");
                    //if (tempValue == 255) counter++;

                }
            }*/

            for (var rowIdx = 0; rowIdx < matToMeasure.Rows; rowIdx++)
            {
                var tempRow = matToMeasure.Row(rowIdx);
                for (var colIdx = 0; colIdx < matToMeasure.Cols; colIdx++)
                {
                    var tempValue = (int)tempRow.GetData().GetValue(0, colIdx, 0);
                    if ( colIdx > 0) prevPixelVal = (int)tempRow.GetData().GetValue(0, colIdx-1, 0);

                    //Helyette inkább majd http://www.emgu.com/wiki/files/3.1.0/document/html/1293f167-1f50-82a2-14f0-5cbd3fff67a4.htm
                    if (0 != tempValue || tempValue != 255) throw new Exception("Given image in the matrix has no proper threshold applied!");
                    if (!foundFirstWhitePixelInFrontOfCell && 0 == tempValue) foundFirstWhitePixelInFrontOfCell = true;
                    if (foundFirstWhitePixelInFrontOfCell && 255 == tempValue && 0 == prevPixelVal) foundLastWhitePixelInFrontOfCell = true;
                    if (255 == prevPixelVal && 0 == tempValue) foundLastBlackPixelOfCell = true;
                    if (!foundLastBlackPixelOfCell && foundLastWhitePixelInFrontOfCell && 255 == tempValue) counter++;

                }
            }

            return counter;
        }

        //https://stackoverflow.com/questions/4122527/sliding-window-minimum-algorithm
        //Az nem derült ki, hogy mekkora legyen a sliding window mérete, de lehet azt paraméterezni kellene, és kezdjük öttel

        public static Mat GetBiggestAreaOfCellWithSlidingWindow(this Mat matToMeasure, int slidingWindowSize)
        {
            var biggestAreSoFar = -1;
            var returnMat = new Mat();
            //var slidingWindowMat = new Mat();


            for (var rowIdx = 0; rowIdx + slidingWindowSize < matToMeasure.Rows; rowIdx++)
            {
                var slidingWindowMat = matToMeasure.GetRowsFromRange(rowIdx, rowIdx + slidingWindowSize);
                var slidingWindowArea = slidingWindowMat.GetAreaOfCellSlice();
                if (biggestAreSoFar >= slidingWindowArea) continue;// itt jó kérdés, hogy az egyenlősgéet megengedjüke
                biggestAreSoFar = slidingWindowArea;
                returnMat = slidingWindowMat;
            }

            return returnMat;
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
            var rowRange = new Range(rowStartIdx,rowEndIdx);
            var colRange = new Range(0,matToSlice.Cols);
            var returnMat = new Mat(matToSlice, rowRange, colRange);
            return returnMat;
        }

        public static Mat GetColsFromRange(this Mat matToSlice, int colStartIDx, int colEndIdx)
        {
            var rowRange = new Range(0, matToSlice.Rows);
            var colRange = new Range(colStartIDx, colEndIdx);
            var returnMat = new Mat(matToSlice, rowRange, colRange);
            return returnMat;
        }

        public static int GetPrevValueOfMatrix(this Mat matToMeasure, int colIdx, int rowIdx)
        {
            var returnVal = int.MinValue;

            //if (colIdx == 0                 && rowIdx == 0)                 return returnVal;
            //if (colIdx == 0                 && rowIdx == matToMeasure.Rows) return (int)matToMeasure.GetData().GetValue(matToMeasure.Cols, matToMeasure.Rows -1, 0);
            //if (colIdx == matToMeasure.Cols && rowIdx == 0)                 return (int)matToMeasure.GetData().GetValue(matToMeasure.Cols, matToMeasure.Rows - 1, 0);
            //if (colIdx == matToMeasure.Cols && rowIdx == matToMeasure.Rows) return returnVal;
            /*
            if (colIdx == 0 && rowIdx == 0) return returnVal;
            if (colIdx == matToMeasure.Cols && rowIdx == matToMeasure.Rows) return (int)matToMeasure.GetData().GetValue(matToMeasure.Cols - 1, matToMeasure.Rows , 0);
            if (colIdx <= matToMeasure.Cols && rowIdx <= matToMeasure.Rows) return (int)matToMeasure.GetData().GetValue(matToMeasure.Cols - 1, matToMeasure.Rows, 0);
            if (colIdx <= matToMeasure.Cols && rowIdx >= matToMeasure.Rows) return (int)matToMeasure.GetData().GetValue(matToMeasure.Cols - 1, matToMeasure.Rows, 0);
            if (colIdx >= matToMeasure.Cols && rowIdx <= matToMeasure.Rows) return returnVal;
            if (colIdx >= matToMeasure.Cols && rowIdx >= matToMeasure.Rows) return returnVal;
            */


            return returnVal;
        }

    }
}
