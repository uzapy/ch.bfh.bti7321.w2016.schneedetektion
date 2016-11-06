using Schneedetektion.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Schneedetektion.ImagePlayground
{
    public class PatchViewModel
    {
        #region Fields
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private BrushConverter brushConverter = new BrushConverter();
        private Patch patch;
        private BitmapImage patchBitmap;
        private ImageViewModel imageViewModel;
        private Polygon polygon;
        private Histogram histogram;
        #endregion

        #region Constructor
        public PatchViewModel(Patch patch, BitmapImage patchBitmap, ImageViewModel imageViewModel, Polygon polygon)
        {
            this.patch = patch;
            this.patchBitmap = patchBitmap;
            this.imageViewModel = imageViewModel;
            this.polygon = polygon;

            histogram = new Histogram(patch.BlueHistogramList, patch.GreenHistogramList, patch.RedHistogramList);
        }
        #endregion

        #region Properties
        public Patch Patch
        {
            get { return patch; }
        }

        public BitmapImage PatchBitmap
        {
            get { return patchBitmap; }
        }

        public Polygon Polygon
        {
            get { return polygon; }
        }

        // Histogram
        public Histogram Histogram
        {
            get { return histogram; }
        }

        // Mode
        public string ModeBlueText { get { return " B " + patch.ModeBlue + " "; } }
        public string ModeGreenText { get { return " G " + patch.ModeGreen + " "; } }
        public string ModeRedText { get { return " R " + patch.ModeRed + " "; } }
        public SolidColorBrush ModeBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)patch.ModeBlue)); } }
        public SolidColorBrush ModeGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)patch.ModeGreen, 0)); } }
        public SolidColorBrush ModeRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)patch.ModeRed, 0, 0)); } }
        public SolidColorBrush ModeBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb((byte)patch.ModeRed, (byte)patch.ModeGreen, (byte)patch.ModeBlue));
            }
        }

        // Mean
        public string MeanBlueText { get { return " B " + patch.MeanBlue.Value.ToString("0.00") + " "; } }
        public string MeanGreenText { get { return " G " + patch.MeanGreen.Value.ToString("0.00") + " "; } }
        public string MeanRedText { get { return " R " + patch.MeanRed.Value.ToString("0.00") + " "; } }
        public SolidColorBrush MeanBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)patch.MeanBlue)); } }
        public SolidColorBrush MeanGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)patch.MeanGreen, 0)); } }
        public SolidColorBrush MeanRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)patch.MeanRed, 0, 0)); } }
        public SolidColorBrush MeanBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb((byte)patch.MeanRed, (byte)patch.MeanGreen, (byte)patch.MeanBlue));
            }
        }

        // Median
        public string MedianBlueText { get { return " B " + patch.MedianBlue.Value.ToString("0.00") + " "; } }
        public string MedianGreenText { get { return " G " + patch.MedianGreen.Value.ToString("0.00") + " "; } }
        public string MedianRedText { get { return " R " + patch.MedianRed.Value.ToString("0.00") + " "; } }
        public SolidColorBrush MedianBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)patch.MedianBlue)); } }
        public SolidColorBrush MedianGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)patch.MedianGreen, 0)); } }
        public SolidColorBrush MedianRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)patch.MedianRed, 0, 0)); } }
        public SolidColorBrush MedianBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb((byte)patch.MedianRed, (byte)patch.MedianGreen, (byte)patch.MedianBlue));
            }
        }

        // Minimum
        public string MinimumBlueText { get { return " B " + patch.MinimumBlue + " "; } }
        public string MinimumGreenText { get { return " G " + patch.MinimumGreen + " "; } }
        public string MinimumRedText { get { return " R " + patch.MinimumRed + " "; } }
        public SolidColorBrush MinimumBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)patch.MinimumBlue)); } }
        public SolidColorBrush MinimumGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)patch.MinimumGreen, 0)); } }
        public SolidColorBrush MinimumRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)patch.MinimumRed, 0, 0)); } }
        public SolidColorBrush MinimumBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb((byte)patch.MinimumRed, (byte)patch.MinimumGreen, (byte)patch.MinimumBlue));
            }
        }

        // Maximum
        public string MaximumBlueText { get { return " B " + patch.MaximumBlue + " "; } }
        public string MaximumGreenText { get { return " G " + patch.MaximumGreen + " "; } }
        public string MaximumRedText { get { return " R " + patch.MaximumRed + " "; } }
        public SolidColorBrush MaximumBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)patch.MaximumBlue)); } }
        public SolidColorBrush MaximumGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)patch.MaximumGreen, 0)); } }
        public SolidColorBrush MaximumRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)patch.MaximumRed, 0, 0)); } }
        public SolidColorBrush MaximumBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb((byte)patch.MaximumRed, (byte)patch.MaximumGreen, (byte)patch.MaximumBlue));
            }
        }

        // Standard Deviation
        public string StandardDeviationBlueText { get { return " B " + patch.StandardDeviationBlue.Value.ToString("0.00") + " "; } }
        public string StandardDeviationGreenText { get { return " G " + patch.StandardDeviationGreen.Value.ToString("0.00") + " "; } }
        public string StandardDeviationRedText { get { return " R " + patch.StandardDeviationRed.Value.ToString("0.00") + " "; } }

        // Variance
        public string VarianceBlueText { get { return " B " + patch.VarianceBlue.Value.ToString("0.00") + " "; } }
        public string VarianceGreenText { get { return " G " + patch.VarianceGreen.Value.ToString("0.00") + " "; } }
        public string VarianceRedText { get { return " R " + patch.VarianceRed.Value.ToString("0.00") + " "; } }

        // Contrast
        public string ContrastBlueText { get { return " B " + patch.ContrastBlue.Value.ToString("0.00") + " "; } }
        public string ContrastGreenText { get { return " G " + patch.ContrastGreen.Value.ToString("0.00") + " "; } }
        public string ContrastRedText { get { return " R " + patch.ContrastRed.Value.ToString("0.00") + " "; } }
        #endregion
    }
}
