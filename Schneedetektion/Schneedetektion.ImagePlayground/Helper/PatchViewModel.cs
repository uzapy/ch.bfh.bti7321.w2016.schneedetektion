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
        private Statistic statistic;
        private BitmapImage patchBitmap;
        private ImageViewModel imageViewModel;
        private Polygon polygon;
        private HistogramViewModel histogram;
        #endregion

        #region Constructor
        public PatchViewModel(Statistic statistic, BitmapImage patchBitmap, ImageViewModel imageViewModel, Polygon polygon)
        {
            this.statistic = statistic;
            this.patchBitmap = patchBitmap;
            this.imageViewModel = imageViewModel;
            this.polygon = polygon;

            histogram = new HistogramViewModel(statistic.BlueHistogramList, statistic.GreenHistogramList, statistic.RedHistogramList);
        }

        public PatchViewModel(Statistic completeImageStatistic, ImageViewModel imageViewModel)
        {
            this.statistic = completeImageStatistic;
            this.patchBitmap = imageViewModel.Bitmap;
            this.imageViewModel = imageViewModel;

            histogram = new HistogramViewModel(statistic.BlueHistogramList, statistic.GreenHistogramList, statistic.RedHistogramList, true);
        }
        #endregion

        #region Properties
        public Statistic Statistic
        {
            get { return statistic; }
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
        public HistogramViewModel HistogramViewModel
        {
            get { return histogram; }
        }

        // Mode
        public string ModeBlueText { get { return " B " + statistic.ModeBlue + " "; } }
        public string ModeGreenText { get { return " G " + statistic.ModeGreen + " "; } }
        public string ModeRedText { get { return " R " + statistic.ModeRed + " "; } }
        public SolidColorBrush ModeBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)statistic.ModeBlue)); } }
        public SolidColorBrush ModeGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)statistic.ModeGreen, 0)); } }
        public SolidColorBrush ModeRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)statistic.ModeRed, 0, 0)); } }
        public SolidColorBrush ModeBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb((byte)statistic.ModeRed, (byte)statistic.ModeGreen, (byte)statistic.ModeBlue));
            }
        }

        // Mean
        public string MeanBlueText { get { return " B " + statistic.MeanBlue.Value.ToString("0.00") + " "; } }
        public string MeanGreenText { get { return " G " + statistic.MeanGreen.Value.ToString("0.00") + " "; } }
        public string MeanRedText { get { return " R " + statistic.MeanRed.Value.ToString("0.00") + " "; } }
        public SolidColorBrush MeanBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)statistic.MeanBlue)); } }
        public SolidColorBrush MeanGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)statistic.MeanGreen, 0)); } }
        public SolidColorBrush MeanRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)statistic.MeanRed, 0, 0)); } }
        public SolidColorBrush MeanBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb((byte)statistic.MeanRed, (byte)statistic.MeanGreen, (byte)statistic.MeanBlue));
            }
        }

        // Median
        public string MedianBlueText { get { return " B " + statistic.MedianBlue.Value.ToString("0.00") + " "; } }
        public string MedianGreenText { get { return " G " + statistic.MedianGreen.Value.ToString("0.00") + " "; } }
        public string MedianRedText { get { return " R " + statistic.MedianRed.Value.ToString("0.00") + " "; } }
        public SolidColorBrush MedianBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)statistic.MedianBlue)); } }
        public SolidColorBrush MedianGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)statistic.MedianGreen, 0)); } }
        public SolidColorBrush MedianRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)statistic.MedianRed, 0, 0)); } }
        public SolidColorBrush MedianBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb((byte)statistic.MedianRed, (byte)statistic.MedianGreen, (byte)statistic.MedianBlue));
            }
        }

        // Minimum
        public string MinimumBlueText { get { return " B " + statistic.MinimumBlue + " "; } }
        public string MinimumGreenText { get { return " G " + statistic.MinimumGreen + " "; } }
        public string MinimumRedText { get { return " R " + statistic.MinimumRed + " "; } }
        public SolidColorBrush MinimumBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)statistic.MinimumBlue)); } }
        public SolidColorBrush MinimumGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)statistic.MinimumGreen, 0)); } }
        public SolidColorBrush MinimumRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)statistic.MinimumRed, 0, 0)); } }
        public SolidColorBrush MinimumBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb((byte)statistic.MinimumRed, (byte)statistic.MinimumGreen, (byte)statistic.MinimumBlue));
            }
        }

        // Maximum
        public string MaximumBlueText { get { return " B " + statistic.MaximumBlue + " "; } }
        public string MaximumGreenText { get { return " G " + statistic.MaximumGreen + " "; } }
        public string MaximumRedText { get { return " R " + statistic.MaximumRed + " "; } }
        public SolidColorBrush MaximumBlueBrush { get { return new SolidColorBrush(Color.FromRgb(0, 0, (byte)statistic.MaximumBlue)); } }
        public SolidColorBrush MaximumGreenBrush { get { return new SolidColorBrush(Color.FromRgb(0, (byte)statistic.MaximumGreen, 0)); } }
        public SolidColorBrush MaximumRedBrush { get { return new SolidColorBrush(Color.FromRgb((byte)statistic.MaximumRed, 0, 0)); } }
        public SolidColorBrush MaximumBrush
        {
            get
            {
                return new SolidColorBrush(Color.FromRgb((byte)statistic.MaximumRed, (byte)statistic.MaximumGreen, (byte)statistic.MaximumBlue));
            }
        }

        // Standard Deviation
        public string StandardDeviationBlueText { get { return " B " + statistic.StandardDeviationBlue.Value.ToString("0.00") + " "; } }
        public string StandardDeviationGreenText { get { return " G " + statistic.StandardDeviationGreen.Value.ToString("0.00") + " "; } }
        public string StandardDeviationRedText { get { return " R " + statistic.StandardDeviationRed.Value.ToString("0.00") + " "; } }

        // Variance
        public string VarianceBlueText { get { return " B " + statistic.VarianceBlue.Value.ToString("0.00") + " "; } }
        public string VarianceGreenText { get { return " G " + statistic.VarianceGreen.Value.ToString("0.00") + " "; } }
        public string VarianceRedText { get { return " R " + statistic.VarianceRed.Value.ToString("0.00") + " "; } }

        // Contrast
        public string ContrastBlueText { get { return " B " + statistic.ContrastBlue.Value.ToString("0.00") + " "; } }
        public string ContrastGreenText { get { return " G " + statistic.ContrastGreen.Value.ToString("0.00") + " "; } }
        public string ContrastRedText { get { return " R " + statistic.ContrastRed.Value.ToString("0.00") + " "; } }
        #endregion
    }
}
