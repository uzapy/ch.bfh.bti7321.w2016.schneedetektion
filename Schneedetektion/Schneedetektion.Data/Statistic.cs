using Newtonsoft.Json;
using System.Collections.Generic;

namespace Schneedetektion.Data
{
    public partial class Statistic
    {
        private List<int> blueHistogram = new List<int>();
        private List<int> greenHistogram = new List<int>();
        private List<int> redHistogram = new List<int>();

        public void SetHistogram(List<int> histogram, EChannel channel)
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

        public List<int> BlueHistogramList { get { return blueHistogram; } }
        public List<int> GreenHistogramList { get { return greenHistogram; } }
        public List<int> RedHistogramList { get { return redHistogram; } }
    }
}