using Schneedetektion.Data;
using Schneedetektion.OpenCV;
using System.Collections.Generic;
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
        private OpenCVColor mean;
        private OpenCVColor median;
        private double contrast;
        private OpenCVColor standardDeviation;
        private OpenCVColor variance;
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
            set { histogram = new Histogram(value); }
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
        public double Contrast
        {
            get { return contrast; }
            set { contrast = value; }
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
        #endregion
    }
}
