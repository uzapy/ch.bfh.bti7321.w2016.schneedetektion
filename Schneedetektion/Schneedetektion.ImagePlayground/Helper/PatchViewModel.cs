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
        private StatistcValue mean;
        private StatistcValue median;
        private StatistcValue contrast;
        private StatistcValue standardDeviation;
        private StatistcValue variance;
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
        public Histogram Histogram
        {
            get { return histogram; }
        }
        public List<float[]> HistogramValues
        {
            set { histogram = new Histogram(value); }
        }
        public StatistcValue Mean
        {
            get { return mean; }
            set { mean = value; }
        }
        public StatistcValue Median
        {
            get { return median; }
            set { median = value; }
        }
        public StatistcValue Contrast
        {
            get { return contrast; }
            set { contrast = value; }
        }
        public StatistcValue StandardDeviation
        {
            get { return standardDeviation; }
            set { standardDeviation = value; }
        }
        public StatistcValue Variance
        {
            get { return variance; }
            set { variance = value; }
        } 
        #endregion
    }
}
