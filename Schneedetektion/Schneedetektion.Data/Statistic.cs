using Newtonsoft.Json;
using System.Collections.Generic;

namespace Schneedetektion.Data
{
    public partial class Statistic
    {
        private List<double> blueHistogram = new List<double>();
        private List<double> greenHistogram = new List<double>();
        private List<double> redHistogram = new List<double>();

        public List<double> BlueHistogramList
        {
            get
            {
                if (blueHistogram.Count == 0)
                {
                    blueHistogram = JsonConvert.DeserializeObject<List<double>>(BlueHistogram);
                }
                return blueHistogram;
            }
            set
            {
                blueHistogram = value;
                BlueHistogram = JsonConvert.SerializeObject(blueHistogram);
            }
        }
        public List<double> GreenHistogramList
        {
            get
            {
                if (greenHistogram.Count == 0)
                {
                    greenHistogram = JsonConvert.DeserializeObject<List<double>>(GreenHistogram);
                }
                return greenHistogram;
            }
            set
            {
                greenHistogram = value;
                GreenHistogram = JsonConvert.SerializeObject(greenHistogram);
            }
        }

        public List<double> RedHistogramList
        {
            get
            {
                if (redHistogram.Count == 0)
                {
                    redHistogram = JsonConvert.DeserializeObject<List<double>>(RedHistogram);
                }
                return redHistogram;
            }
            set
            {
                redHistogram = value;
                RedHistogram = JsonConvert.SerializeObject(redHistogram);
            }
        }
    }
}