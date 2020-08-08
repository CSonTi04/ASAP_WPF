using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace ASAP_WPF
{
    class LengthCollector
    {
        //TODO a programnak automatikusan kellene beállítani a mikrométerskálás dolgot a pontossághoz, erről meg kellene krédezni a Martint
        //Ezt át kellene szabni, úgy, hogy mondjuk egy dictionary egy dictionaryben - ez végül nem egy jó megoldás, mert ugye képenként megyünk nem sejtenként
        //Dictionary<int, (double, PointF)> lengthDictionary = new Dictionary<int, (double, PointF)>();
        Dictionary<int, double> lengthDictionary = new Dictionary<int, double>();
        Dictionary<int, Point> positionDictionary = new Dictionary<int, Point>();
        //Dictionary<int,Dictionary<PointF,double>> lengthDictionary = new Dictionary<int, Dictionary<PointF, double>>();
        //Lehet ez az osztódás után trackeléshez kellet


        public bool ContainsKey(int imgNumber)
        {
            return lengthDictionary.ContainsKey(imgNumber);
        }

        public void Add(int imgNumber, Point point, double length)
        {
            //var tempTuple = (_length, _point);
            this.lengthDictionary.Add(imgNumber, length);
            this.positionDictionary.Add(imgNumber,point);
            /*
            if (lengthDictionary.ContainsKey(_imgNumber))
            {
                var tempDict = lengthDictionary[_imgNumber];
                if (!tempDict.ContainsKey(_point))
                {
                    tempDict.Add(_point,_length);
                }
            }
            else
            {
                lengthDictionary.Add(_imgNumber, new Dictionary<PointF, double>());
                var tempDict = lengthDictionary[_imgNumber];
                tempDict.Add(_point,_length);
            }
            */
        }

        /*
        public void Add(int _imgNumber, List<(PointF, double)> _list)
        {
            foreach (var pair in _list)
            {
                Add(_imgNumber,pair.Item1,pair.Item2);
            }
        }
        */
        /*
        public Dictionary<int, (double, PointF)> GetLengthTuples(double _pixelPerMicroMeter)
        //public Dictionary<int, Dictionary<PointF, double>> GetLengthDictionary(double _pixelPerMicroMeter)
        {
            var tempLengthDictionary = lengthDictionary;
            foreach (var dictItem in tempLengthDictionary)
            {
                var tempTuple = (dictItem.Value.Item1 / _pixelPerMicroMeter, dictItem.Value.Item2);
                tempLengthDictionary.Add(dictItem.Key, tempTuple);

                foreach (var valueKey in dictItem.Value.Keys)
                {
                    dictItem.Value[valueKey] /= _pixelPerMicroMeter ;
                }

            }
            return tempLengthDictionary;
        }
        public List<(int, double, double)> GetLengthList(double _pixelPerMicroMeter)
        {
            return lengthDictionary.Select(dictItem =>
                (dictItem.Key, dictItem.Value.Item1, dictItem.Value.Item1 / _pixelPerMicroMeter)).ToList();
                //return(from lengthDictionaryItem in lengthDictionary.Keys let tempDict = lengthDictionary[lengthDictionaryItem] from keyValuePair in tempDict select (lengthDictionaryItem, keyValuePair.Value, keyValuePair.Value / _pixelPerMicroMeter)).ToList();
        }
        */


        //public List<(int, double, double)> GetLengthList(int _imgNumber,double _pixelPerMicroMeter)
        /*
        public List<LengthTriplet> GetLengthTripletList(int _imgNumnber, double _pixelPerMicroMeter)
        {
            //return lengthDictionary.Select(dictItem =>
                //(dictItem.Key, dictItem.Value, dictItem.Value / _pixelPerMicroMeter)).Where(dictItem => dictItem.Key == _imgNumber).ToList();
               return lengthDictionary.Select(innerDictValue => (_imgNumber, innerDictValue, innerDictValue / _pixelPerMicroMeter)).ToList();
        }
        */
        public List<LengthTriplet> GetLengthTripletList(int imgNumnber, double pixelPerMicroMeter)
        {
            return lengthDictionary.Select(entry => new LengthTriplet(imgNumnber, entry.Value, entry.Value / pixelPerMicroMeter)).ToList();
        }



        public Point GetPositionbyNumber(int imgNumber)
        {
            return this.positionDictionary[imgNumber];
        }


        public void ExportToCsv(string path,int imgNumber, double pixelPerMicroMeter)
        {
            var fileName = "ASAP_EXPORT_" + DateTime.Now;
            using var writer = new StreamWriter(path + Path.DirectorySeparatorChar + fileName);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            writer.WriteLine("img_number;length_in_px;length_in_um");
            csv.WriteRecords(GetLengthTripletList(imgNumber, pixelPerMicroMeter));
        }
    }
}
