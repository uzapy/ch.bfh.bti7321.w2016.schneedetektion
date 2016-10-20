using Schneedetektion.OpenCV;
using System.Windows.Controls;

namespace Schneedetektion.ImagePlayground
{
    public partial class HistogramViewer : UserControl
    {
        #region MyRegion
        private OpenCVHelper openCVHelper;
        private ImageViewModel imageViewModel1;
        private ImageViewModel imageViewModel2;
        #endregion

        #region Constructor
        public HistogramViewer()
        {
            InitializeComponent();
            openCVHelper = new OpenCVHelper();
        }

        public void ShowImage(ImageViewModel selectedImage, int panel)
        {
            if (panel == 1)
            {
                imageViewModel1 = selectedImage;
                image1.Source = imageViewModel1.Bitmap;
                histogram1.Source = openCVHelper.GetHistogram(imageViewModel1.Bitmap);
            }
            else if (panel == 2)
            {
                imageViewModel2 = selectedImage;
                image2.Source = imageViewModel2.Bitmap;
                histogram2.Source = openCVHelper.GetHistogram(imageViewModel2.Bitmap);
            }
        }
        #endregion
    }
}
