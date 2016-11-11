using Newtonsoft.Json;
using System.Collections.Generic;

namespace Schneedetektion.Data
{
    public partial class Statistic
    {
        private List<double> blueHistogram = new List<double>();
        private List<double> greenHistogram = new List<double>();
        private List<double> redHistogram = new List<double>();

        public void SetHistogram(List<double> histogram, EChannel channel)
        {
            switch (channel)
            {
                case EChannel.Blue:
                    blueHistogram = histogram;
                    BlueHistogram = JsonConvert.SerializeObject(blueHistogram);
                    break;
                case EChannel.Green:
                    greenHistogram = histogram;
                    GreenHistogram = JsonConvert.SerializeObject(greenHistogram);
                    break;
                case EChannel.Red:
                    redHistogram = histogram;
                    RedHistogram = JsonConvert.SerializeObject(redHistogram);
                    break;
            }
        }

        public List<double> BlueHistogramList { get { return blueHistogram; } }
        public List<double> GreenHistogramList { get { return greenHistogram; } }
        public List<double> RedHistogramList { get { return redHistogram; } }
    }
}