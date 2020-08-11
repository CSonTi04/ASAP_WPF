using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Util;

namespace ASAP_WPF
{
    static class ExtensionMethods
    {
        public static Mat createNewHardCopyFromMat(this Mat matToHardCopy)
        {
            var returnMat = new Mat(matToHardCopy.Rows, matToHardCopy.Cols, matToHardCopy.Depth, matToHardCopy.NumberOfChannels);
            matToHardCopy.CopyTo(returnMat);
            return returnMat;
        }

        public static Mat createNewMatLikeThis(this Mat matToHardCopy)
        {
            var returnMat = new Mat(matToHardCopy.Rows, matToHardCopy.Cols, matToHardCopy.Depth, matToHardCopy.NumberOfChannels);
            return returnMat;
        }

        public static VectorOfVectorOfPoint convertToVectorOfPoint(this VectorOfVectorOfPointF vectorToTransform)
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
    }
}
