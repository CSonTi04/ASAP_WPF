using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace ASAP_WPF
{
    public class LengthCollector
    {
        Dictionary<int, double> lengthDictionary = new Dictionary<int, double>();
        Dictionary<int, Point> positionDictionary = new Dictionary<int, Point>();

        private bool ContainsKey(int imgNumber)
        {
            return lengthDictionary.ContainsKey(imgNumber) && positionDictionary.ContainsKey(imgNumber);
        }

        public void Add(int imgNumber, Point point, double length)
        {

            if (!ContainsKey(imgNumber))
            {
                this.lengthDictionary.Add(imgNumber, length);
                this.positionDictionary.Add(imgNumber, point);
            }
            else
            {
                this.lengthDictionary.Remove(imgNumber);
                this.positionDictionary.Remove(imgNumber);
                this.lengthDictionary.Add(imgNumber, length);
                this.positionDictionary.Add(imgNumber, point);
            }
        }

        public List<LengthTriplet> GetLengthTripletList(double pixelPerMicroMeter)
        {
            return lengthDictionary.Select(entry => new LengthTriplet(entry.Key, entry.Value, entry.Value / pixelPerMicroMeter)).ToList();
        }



        public Point GetPositionByNumber(int imgNumber)
        {
            return this.positionDictionary[imgNumber];
        }


        public void ExportToCsv(string path, double pixelPerMicroMeter)
        {
            var fileName = "ASAP_EXPORT_" + DateTime.Now.ToString(CultureInfo.InvariantCulture);
            fileName = fileName.Replace('.', '_');
            fileName = fileName.Replace(':', '_');
            fileName = fileName.Replace('/', '_');
            fileName = fileName.Replace(' ', '_');
            fileName += ".csv";
            using var writer = new StreamWriter( Path.Combine(path, fileName));
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            writer.WriteLine("img_number,length_in_px,length_in_um");
            csv.WriteRecords(GetLengthTripletList(pixelPerMicroMeter));
        }
    }
}
