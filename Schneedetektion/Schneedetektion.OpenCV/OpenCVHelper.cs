using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Schneedetektion.OpenCV
{
    public class OpenCVHelper
    {
        public object HistogramViewer { get; private set; }

        public List<float[]> GetHistogram(string fileName)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(fileName);
            // http://docs.opencv.org/2.4/doc/tutorials/imgproc/histograms/histogram_calculation/histogram_calculation.html
            // Split into RGB
            Image<Gray, byte> imageBlue  = image[0];
            Image<Gray, byte> imageGreen = image[1];
            Image<Gray, byte> imageRed   = image[2];

            List<float[]> histogramValues = new List<float[]>();

            // Histogram per Color
            DenseHistogram histogram = new DenseHistogram(256, new RangeF(0.0f, 256.0f));

            histogram.Calculate(new Image<Gray, byte>[] { imageBlue }, false, null);
            histogramValues.Add(histogram.GetBinValues());
            histogram.Clear();

            histogram.Calculate(new Image<Gray, byte>[] { imageGreen }, false, null);
            histogramValues.Add(histogram.GetBinValues());
            histogram.Clear();

            histogram.Calculate(new Image<Gray, byte>[] { imageRed }, false, null);
            histogramValues.Add(histogram.GetBinValues());

            return histogramValues;
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage resultImage;

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                resultImage = new BitmapImage();
                resultImage.BeginInit();
                resultImage.StreamSource = stream;
                resultImage.CacheOption = BitmapCacheOption.OnLoad;
                resultImage.EndInit();
            }
            return resultImage;
        }

        private Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            Bitmap bitmap;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                bitmap = new Bitmap(stream);
            }
            return bitmap;
        }
    }
}
