using Schneedetektion.Data;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Schneedetektion.ImagePlayground
{
    public class PatchViewModel
    {
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private BitmapImage patchImage;
        // dbPatch
        private ImageViewModel imageViewModel;
        private List<float[]> histogram;
        private float mean;
        private float median;
        private float contrast;
        private float standardDeviation;
        private float variance;

        public PatchViewModel(BitmapImage bitmapImage, ImageViewModel imageViewModel)
        {
            this.patchImage = bitmapImage;
            this.imageViewModel = imageViewModel;
        }

        public BitmapImage PatchImage { get { return patchImage; } }
    }
}
