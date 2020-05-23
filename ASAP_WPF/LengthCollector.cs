using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace ASAP_WPF
{
    class LengthCollector
    {
        Dictionary<int, (double, PointF)> lengthDictionary = new Dictionary<int, (double, PointF)>();

        public void Add(int _imgNumber, double _length, PointF _point)
        {
            var tempTuple = (_length, _point);
            this.lengthDictionary.Add(_imgNumber, tempTuple);
        }

        public Dictionary<int, (double, PointF)> GetLengthTuples(double _pixelPerMicroMeter)
        {
            var tempLengthDictionary = new Dictionary<int, (double, PointF)>();
            foreach (var dictItem in lengthDictionary)
            {
                var tempTuple = (dictItem.Value.Item1 / _pixelPerMicroMeter, dictItem.Value.Item2);
                tempLengthDictionary.Add(dictItem.Key, tempTuple);
            }

            return tempLengthDictionary;
        }

        public List<(int, double, double)> GetLengthList(double _pixelPerMicroMeter)
        {
            return lengthDictionary.Select(dictItem =>
                (dictItem.Key, dictItem.Value.Item1, dictItem.Value.Item1 / _pixelPerMicroMeter)).ToList();
        }

        public PointF GetPositionbyNumber(int _imgNumber)
        {
            return this.lengthDictionary[_imgNumber].Item2;
        }

        public void ExportToCsv(string _path, double _pixelPerMicroMeter)
        {
            var FileName = "ASAP_EXPORT_" + DateTime.Now;
            using var writer = new StreamWriter(_path + Path.DirectorySeparatorChar + FileName);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            writer.WriteLine("img_number;length_in_px;length_in_um");
            csv.WriteRecords(GetLengthList(_pixelPerMicroMeter));
        }
    }
}
