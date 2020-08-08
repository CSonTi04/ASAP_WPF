using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

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
    }
}
