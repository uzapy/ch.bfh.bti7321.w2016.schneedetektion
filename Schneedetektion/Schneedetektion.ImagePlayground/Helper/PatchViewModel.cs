using Schneedetektion.Data;
using Schneedetektion.OpenCV;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Schneedetektion.ImagePlayground
{
    public class PatchViewModel
    {
        #region Fields
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private BrushConverter brushConverter = new BrushConverter();
        private BitmapImage patchImage;
        // dbPatch
        private Polygon polygon;
        private ImageViewModel imageViewModel;
        private Histogram histogram;
        private OpenCVColor mode;
        private OpenCVColor mean;
        private OpenCVColor median;
        private OpenCVColor minimum;
        private OpenCVColor maximum;
        private OpenCVColor standardDeviation;
        private OpenCVColor variance;
        private OpenCVColor contrast;
        #endregion

        #region Constructor
        public PatchViewModel(BitmapImage bitmapImage, ImageViewModel imageViewModel, Polygon polygon)
        {
            this.patchImage = bitmapImage;
            this.imageViewModel = imageViewModel;
            this.polygon = polygon;
        }
        #endregion

        #region Properties
        public BitmapImage PatchImage
        {
            get { return patchImage; }
        }
        public Polygon Polygon
        {
            get { return polygon; }
        }
        public Histogram Histogram
        {
            get { return histogram; }
        }
        public List<float[]> HistogramValues
        {
            set
            {
                histogram = new Histogram(value);
                mode.Blue = histogram.Blue.IndexOf(histogram.Blue.Max());
                mode.Green = histogram.Green.IndexOf(histogram.Green.Max());
                mode.Red = histogram.Red.IndexOf(histogram.Red.Max());
            }
        }
        public OpenCVColor Mode
        {
            get { return mode; }
            set { mode = value; }
        }
        public string ModeBlueText { get { return " B " + Mode.Blue.ToString("0.00") + " "; } }
        public string ModeGreenText { get { return " G " + Mode.Green.ToString("0.00") + " "; } }
        public string ModeRedText { get { return " R " + Mode.Red.ToString("0.00") + " "; } }
        public SolidColorBrush ModeBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)Mode.Blue)); } }
        public SolidColorBrush ModeGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)Mode.Green, 0)); } }
        public SolidColorBrush ModeRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)Mode.Red, 0, 0)); } }
        public SolidColorBrush ModeBrush { get { return new SolidColorBrush(Color.FromRgb((byte)Mode.Red, (byte)Mode.Green, (byte)Mode.Blue)); } }
        public OpenCVColor Mean
        {
            get { return mean; }
            set { mean = value; }
        }
        public string MeanBlueText { get { return " B " + Mean.Blue.ToString("0.00") + " "; } }
        public string MeanGreenText { get { return " G " + Mean.Green.ToString("0.00") + " "; } }
        public string MeanRedText { get { return " R " + Mean.Red.ToString("0.00") + " "; } }
        public SolidColorBrush MeanBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)Mean.Blue)); } }
        public SolidColorBrush MeanGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)Mean.Green, 0)); } }
        public SolidColorBrush MeanRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)Mean.Red, 0, 0)); } }
        public SolidColorBrush MeanBrush { get { return new SolidColorBrush(Color.FromRgb((byte)Mean.Red, (byte)Mean.Green, (byte)Mean.Blue)); } }
        public OpenCVColor Median
        {
            get { return median; }
            set { median = value; }
        }
        public string MedianBlueText { get { return " B " + Median.Blue.ToString("0.00") + " "; } }
        public string MedianGreenText { get { return " G " + Median.Green.ToString("0.00") + " "; } }
        public string MedianRedText { get { return " R " + Median.Red.ToString("0.00") + " "; } }
        public SolidColorBrush MedianBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)Median.Blue)); } }
        public SolidColorBrush MedianGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)Median.Green, 0)); } }
        public SolidColorBrush MedianRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)Median.Red, 0, 0)); } }
        public SolidColorBrush MedianBrush { get { return new SolidColorBrush(Color.FromRgb((byte)Median.Red, (byte)Median.Green, (byte)Median.Blue)); } }
        public OpenCVColor Minimum
        {
            get { return minimum; }
            set { minimum = value; }
        }
        public string MinimumBlueText { get { return " B " + Minimum.Blue.ToString("0.00") + " "; } }
        public string MinimumGreenText { get { return " G " + Minimum.Green.ToString("0.00") + " "; } }
        public string MinimumRedText { get { return " R " + Minimum.Red.ToString("0.00") + " "; } }
        public SolidColorBrush MinimumBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)Minimum.Blue)); } }
        public SolidColorBrush MinimumGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)Minimum.Green, 0)); } }
        public SolidColorBrush MinimumRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)Minimum.Red, 0, 0)); } }
        public SolidColorBrush MinimumBrush { get { return new SolidColorBrush(Color.FromRgb((byte)Minimum.Red, (byte)Minimum.Green, (byte)Minimum.Blue)); } }
        public OpenCVColor Maximum
        {
            get { return maximum; }
            set { maximum = value; }
        }
        public string MaximumBlueText { get { return " B " + Maximum.Blue.ToString("0.00") + " "; } }
        public string MaximumGreenText { get { return " G " + Maximum.Green.ToString("0.00") + " "; } }
        public string MaximumRedText { get { return " R " + Maximum.Red.ToString("0.00") + " "; } }
        public SolidColorBrush MaximumBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)Maximum.Blue)); } }
        public SolidColorBrush MaximumGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)Maximum.Green, 0)); } }
        public SolidColorBrush MaximumRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)Maximum.Red, 0, 0)); } }
        public SolidColorBrush MaximumBrush { get { return new SolidColorBrush(Color.FromRgb((byte)Maximum.Red, (byte)Maximum.Green, (byte)Maximum.Blue)); } }
        public OpenCVColor StandardDeviation
        {
            get { return standardDeviation; }
            set { standardDeviation = value; }
        }
        public string StandardDeviationBlueText { get { return " B " + StandardDeviation.Blue.ToString("0.00") + " "; } }
        public string StandardDeviationGreenText { get { return " G " + StandardDeviation.Green.ToString("0.00") + " "; } }
        public string StandardDeviationRedText { get { return " R " + StandardDeviation.Red.ToString("0.00") + " "; } }
        public OpenCVColor Variance
        {
            get { return variance; }
            set { variance = value; }
        }
        public string VarianceBlueText { get { return " B " + Variance.Blue.ToString("0.00") + " "; } }
        public string VarianceGreenText { get { return " G " + Variance.Green.ToString("0.00") + " "; } }
        public string VarianceRedText { get { return " R " + Variance.Red.ToString("0.00") + " "; } }
        public OpenCVColor Contrast
        {
            get { return contrast; }
            set { contrast = value; }
        }
        public string ContrastBlueText { get { return " B " + Contrast.Blue.ToString("0.00") + " "; } }
        public string ContrastGreenText { get { return " G " + Contrast.Green.ToString("0.00") + " "; } }
        public string ContrastRedText { get { return " R " + Contrast.Red.ToString("0.00") + " "; } }
        #endregion
    }
}
