namespace ASAP_WPF
{
    internal class LengthTriplet
    {
        public int ImageNumber { get; set; }
        public double HeightInPixel { get; set; }
        public double HeightInMicroMeter { get; set; }

        public LengthTriplet(int imgNumber, double heightInPixel, double heightInMicroMeter)
        {
            this.ImageNumber = imgNumber;
            this.HeightInPixel = heightInPixel;
            this.HeightInMicroMeter = heightInMicroMeter;
        }
    }
}
