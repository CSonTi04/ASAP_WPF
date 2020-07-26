using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASAP_WPF
{
    internal class LengthTriplet
    {
        public int ImageNumber { get; set; }
        public double HeightInPixel { get; set; }
        public double HeightInMicroMeter { get; set; }

        public LengthTriplet(int _imgNumber, double _heightInPixel, double _heightInMicroMeter)
        {
            this.ImageNumber = _imgNumber;
            this.HeightInPixel = _heightInPixel;
            this.HeightInMicroMeter = _heightInMicroMeter;
        }
    }
}
