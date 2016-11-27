using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Schneedetektion.Data
{
    public partial class Statistic
    {
        #region Histogram
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
        #endregion

        #region Get Property Value
        public double Get(string property, string color)
        {
            if (property == "Mode")
            {
                if (color == "Blue")
                {
                    return ModeBlue.Value;
                }
                else if (color == "Green")
                {
                    return ModeGreen.Value;
                }
                else if (color == "Red")
                {
                    return ModeRed.Value;
                }
                else if (color == "BGR")
                {
                    return (ModeBlue.Value + ModeGreen.Value + ModeRed.Value) / 3;
                }
            }
            else if (property == "Mean")
            {
                if (color == "Blue")
                {
                    return MeanBlue.Value;
                }
                else if (color == "Green")
                {
                    return MeanGreen.Value;
                }
                else if (color == "Red")
                {
                    return MeanRed.Value;
                }
                else if (color == "BGR")
                {
                    return (MeanBlue.Value + MeanGreen.Value + MeanRed.Value) / 3;
                }
            }
            else if (property == "Median")
            {
                if (color == "Blue")
                {
                    return MedianBlue.Value;
                }
                else if (color == "Green")
                {
                    return MedianGreen.Value;
                }
                else if (color == "Red")
                {
                    return MedianRed.Value;
                }
                else if (color == "BGR")
                {
                    return (MedianBlue.Value + MedianGreen.Value + MedianRed.Value) / 3;
                }
            }
            else if (property == "Minimum")
            {
                if (color == "Blue")
                {
                    return MinimumBlue.Value;
                }
                else if (color == "Green")
                {
                    return MinimumGreen.Value;
                }
                else if (color == "Red")
                {
                    return MinimumRed.Value;
                }
                else if (color == "BGR")
                {
                    return (MinimumBlue.Value + MinimumGreen.Value + MinimumRed.Value) / 3;
                }
            }
            else if (property == "Maximum")
            {
                if (color == "Blue")
                {
                    return MaximumBlue.Value;
                }
                else if (color == "Green")
                {
                    return MaximumGreen.Value;
                }
                else if (color == "Red")
                {
                    return MaximumRed.Value;
                }
                else if (color == "BGR")
                {
                    return (MaximumBlue.Value + MaximumGreen.Value + MaximumRed.Value) / 3;
                }
            }
            else if (property == "StandardDeviation")
            {
                if (color == "Blue")
                {
                    return StandardDeviationBlue.Value;
                }
                else if (color == "Green")
                {
                    return StandardDeviationGreen.Value;
                }
                else if (color == "Red")
                {
                    return StandardDeviationRed.Value;
                }
                else if (color == "BGR")
                {
                    return (StandardDeviationBlue.Value + StandardDeviationGreen.Value + StandardDeviationRed.Value) / 3;
                }
            }
            else if (property == "Variance")
            {
                if (color == "Blue")
                {
                    return VarianceBlue.Value;
                }
                else if (color == "Green")
                {
                    return VarianceGreen.Value;
                }
                else if (color == "Red")
                {
                    return VarianceRed.Value;
                }
                else if (color == "BGR")
                {
                    return (VarianceBlue.Value + VarianceGreen.Value + VarianceRed.Value) / 3;
                }
            }
            else if (property == "Contrast")
            {
                if (color == "Blue")
                {
                    return ContrastBlue.Value;
                }
                else if (color == "Green")
                {
                    return ContrastGreen.Value;
                }
                else if (color == "Red")
                {
                    return ContrastRed.Value;
                }
                else if (color == "BGR")
                {
                    return (ContrastBlue.Value + ContrastGreen.Value + ContrastRed.Value) / 3;
                }
            }

            return 0;
        }
        #endregion

        #region Distance
        public double DistanceTo(Statistic other)
        {
            return Math.Sqrt(
                Math.Pow(this.ModeBlue.Value - other.ModeBlue.Value, 2) +
                Math.Pow(this.ModeGreen.Value - other.ModeGreen.Value, 2) +
                Math.Pow(this.ModeRed.Value - other.ModeRed.Value, 2) +
                Math.Pow(this.MeanBlue.Value - other.MeanBlue.Value, 2) +
                Math.Pow(this.MeanGreen.Value - other.MeanGreen.Value, 2) +
                Math.Pow(this.MeanRed.Value - other.MeanRed.Value, 2) +
                Math.Pow(this.MedianBlue.Value - other.MedianBlue.Value, 2) +
                Math.Pow(this.MedianGreen.Value - other.MedianGreen.Value, 2) +
                Math.Pow(this.MedianRed.Value - other.MedianRed.Value, 2) +
                Math.Pow(this.MinimumBlue.Value - other.MinimumBlue.Value, 2) +
                Math.Pow(this.MinimumGreen.Value - other.MinimumGreen.Value, 2) +
                Math.Pow(this.MinimumRed.Value - other.MinimumRed.Value, 2) +
                Math.Pow(this.MaximumBlue.Value - other.MaximumBlue.Value, 2) +
                Math.Pow(this.MaximumGreen.Value - other.MaximumGreen.Value, 2) +
                Math.Pow(this.MaximumRed.Value - other.MaximumRed.Value, 2) +
                Math.Pow((this.StandardDeviationBlue.Value - other.StandardDeviationBlue.Value) * 2, 2) +
                Math.Pow((this.StandardDeviationGreen.Value - other.StandardDeviationGreen.Value) * 2, 2) +
                Math.Pow((this.StandardDeviationRed.Value - other.StandardDeviationRed.Value) * 2, 2) +
                Math.Pow((this.ContrastBlue.Value - other.ContrastBlue.Value) * 255, 2) +
                Math.Pow((this.ContrastGreen.Value - other.ContrastGreen.Value) * 255, 2) +
                Math.Pow((this.ContrastRed.Value - other.ContrastRed.Value) * 255, 2));
        }
        #endregion
    }
}