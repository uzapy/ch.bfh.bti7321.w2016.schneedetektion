using Schneedetektion.Data;
using Schneedetektion.OpenCV;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Schneedetektion.ImagePlayground
{
    public class PatchViewModel
    {
        #region Fields
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private BitmapImage patchImage;
        private Polygon polygon;
        // dbPatch
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
                mode.Blue = histogram.Blue.Max();
                mode.Green = histogram.Green.Max();
                mode.Red = histogram.Red.Max();
            }
        }
        public OpenCVColor Mean
        {
            get { return mean; }
            set { mean = value; }
        }
        public OpenCVColor Median
        {
            get { return median; }
            set { median = value; }
        }
        public OpenCVColor Minimum
        {
            get { return minimum; }
            set { minimum = value; }
        }
        public OpenCVColor Maximum
        {
            get { return maximum; }
            set { maximum = value; }
        }
        public OpenCVColor StandardDeviation
        {
            get { return standardDeviation; }
            set { standardDeviation = value; }
        }
        public OpenCVColor Variance
        {
            get { return variance; }
            set { variance = value; }
        }
        public OpenCVColor Contrast
        {
            get { return contrast; }
            set { contrast = value; }
        }
        #endregion
    }
}
