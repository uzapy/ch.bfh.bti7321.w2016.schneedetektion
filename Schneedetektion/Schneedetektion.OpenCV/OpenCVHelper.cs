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

            //Image<Gray, byte> blueHistogram = histogram.ToImage<Gray, byte>();
            //Image<Gray, byte> greenHistogram = histogram.ToImage<Gray, byte>();
            //Image<Gray, byte> redHistogram = histogram.ToImage<Gray, byte>();

            //for (int i = 0; i < 256; i++)
            //{
                //histogramValues[0].Add(blueHistogram.Data[i, 0, 0]);
                //histogramValues[1].Add(greenHistogram.Data[i, 0, 0]);
                //histogramValues[2].Add(redHistogram.Data[i, 0, 0]);
            //}

            //Mat histogramImage = new Mat(new Size(512, 512), DepthType.Cv8U, 3);
            //CvInvoke.Normalize(blueHistogram, blueHistogram, 1, 0, NormType.MinMax, DepthType.Default, histogramImage);
            //CvInvoke.Normalize(greenHistogram, greenHistogram, 1, 0, NormType.MinMax, DepthType.Default, histogramImage);
            //CvInvoke.Normalize(redHistogram, redHistogram, 1, 0, NormType.MinMax, DepthType.Default, histogramImage);
            

            //for (int i = 0; i < 512; i++)
            //{
            //    //CvInvoke.Line(histogramImage, 
            //    //    new Point(2*(512-i), 512 - blueHistogram[))
            //}

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
