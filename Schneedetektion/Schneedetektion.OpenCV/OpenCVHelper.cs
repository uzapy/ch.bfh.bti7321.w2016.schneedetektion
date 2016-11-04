using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;

namespace Schneedetektion.OpenCV
{
    public class OpenCVHelper
    {
        #region Static Methods
        public static BitmapImage BitmapToBitmapImage(Drawing.Bitmap bitmap)
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

        public static Drawing.Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            Drawing.Bitmap bitmap;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                bitmap = new Drawing.Bitmap(stream);
            }
            return bitmap;
        }
        #endregion

        #region Public Methods
        public List<float[]> GetHistogram(string fileName)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(fileName);
            return GetHistogram(image);
        }

        public List<float[]> GetHistogram(Drawing.Bitmap bitmap)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(bitmap);
            return GetHistogram(image);
        }

        public BitmapImage GetMaskedImage(string imagePath, IEnumerable<Point> pointCollection)
        {
            Mat matrix = new Mat(imagePath, LoadImageType.AnyColor);
            UMat uMatrix = matrix.ToUMat(AccessType.ReadWrite);

            // Scale Polygon
            List<Point> scaledPoints = GetScaledPoints(pointCollection, uMatrix.Rows, uMatrix.Cols);

            List<Drawing.Point> polygonPoints = GetInvertedPolygonPoints(scaledPoints, uMatrix.Rows, uMatrix.Cols);

            // Apply Polygon
            using (VectorOfPoint vPoint = new VectorOfPoint(polygonPoints.ToArray()))
            using (VectorOfVectorOfPoint vvPoint = new VectorOfVectorOfPoint(vPoint))
            {
                CvInvoke.FillPoly(uMatrix, vvPoint, new Bgr(0, 0, 0).MCvScalar);
            }

            Image<Bgr, byte> image = new Image<Bgr, byte>(uMatrix.Bitmap);
            // Crop Bitmap
            image.ROI = GetRegionOfInterest(scaledPoints);

            return OpenCVHelper.BitmapToBitmapImage(image.Bitmap);
        }

        public BitmapImage SaveBitmask(string imagePath, string bitMaskPath, IEnumerable<Point> pointCollection)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(imagePath);
            Image<Gray, byte> bitMask = new Image<Gray, byte>(image.Cols, image.Rows, new Gray(0));

            // Point to Drawing.Point
            List<Point> scaledPoints = GetScaledPoints(pointCollection, image.Cols, image.Rows);
            List<Drawing.Point> scaledDrawingPoints = new List<Drawing.Point>();
            foreach (var point in scaledPoints)
            {
                scaledDrawingPoints.Add(new Drawing.Point((int)point.X, (int)point.Y));
            }

            // Apply Polygon
            using (VectorOfPoint vPoint = new VectorOfPoint(scaledDrawingPoints.ToArray()))
            using (VectorOfVectorOfPoint vvPoint = new VectorOfVectorOfPoint(vPoint))
            {
                CvInvoke.FillPoly(bitMask, vvPoint, new Gray(255).MCvScalar);
            }

            // Crop ROI
            bitMask.ROI = GetRegionOfInterest(scaledPoints);
            return BitmapToBitmapImage(bitMask.Bitmap);
        }

        public StatistcValue GetMean(Drawing.Bitmap bitmap)
        {
            StatistcValue result = new StatistcValue();
            Image<Bgr, byte> image = new Image<Bgr, byte>(bitmap);

            // Durchschnitt Berechnen
            for (int i = 0; i < image.Width; i++) // zuerst X
            {
                for (int j = 0; j < image.Height; j++) // dann Y
                {
                    result.Blue += image[i, j].Blue > 0 ? image[i, j].Blue : 0;
                }
            }
            return result;
        }
        #endregion

        #region Private Methods
        private List<Point> GetScaledPoints(IEnumerable<Point> polygonPoints, int numberOfRows, int numberOfCols)
        {
            List<Point> scaledPoints = new List<Point>();
            foreach (var point in polygonPoints)
            {
                scaledPoints.Add(new Point()
                {
                    X = point.X * numberOfRows * 1.21,
                    Y = point.Y * numberOfCols * 0.81
                });
            }
            return scaledPoints;
        }

        private List<Drawing.Point> GetInvertedPolygonPoints(List<Point> scaledPoints, int numberOfRows, int numberOfCols)
        {
            // Element finden, das am nächsten zum Nullpunkt ist
            Point p0 = scaledPoints.OrderBy(p => Math.Sqrt(Math.Pow(p.X, 2) + Math.Pow(p.Y, 2))).First();

            // Create Polygon
            List<Drawing.Point> polygon = new List<Drawing.Point>();
            polygon.Add(new Drawing.Point(0, 0));
            polygon.Add(new Drawing.Point(0, numberOfRows));
            polygon.Add(new Drawing.Point(numberOfCols, numberOfRows));
            polygon.Add(new Drawing.Point(numberOfCols, 0));
            polygon.Add(new Drawing.Point(0, 0));

            // Punkte in der richtigen Reihenfolge laden
            int element = scaledPoints.IndexOf(p0);
            int i = element;
            while (i < scaledPoints.Count)
            {
                polygon.Add(new Drawing.Point((int)(scaledPoints[i].X), (int)(scaledPoints[i].Y)));
                i++;
            }
            int j = 0;
            while (j < element)
            {
                polygon.Add(new Drawing.Point((int)(scaledPoints[j].X), (int)(scaledPoints[j].Y)));
                j++;
            }

            // Noch einmal zu Ursprung zurück
            polygon.Add(new Drawing.Point((int)(p0.X), (int)(p0.Y)));
            polygon.Add(new Drawing.Point(0, 0));

            return polygon;
        }

        private Drawing.Rectangle GetRegionOfInterest(List<Point> points)
        {
            int left = (int)points.Min(p => p.X);
            int top = (int)points.Min(p => p.Y);
            int width = (int)points.Max(p => p.X) - left;
            int height = (int)points.Max(p => p.Y) - top;

            return new Drawing.Rectangle(left, top, width, height);
        }

        private List<float[]> GetHistogram(Image<Bgr, byte> image)
        {
            // http://docs.opencv.org/2.4/doc/tutorials/imgproc/histograms/histogram_calculation/histogram_calculation.html
            // Split into RGB
            Image<Gray, byte> imageBlue = image[0];
            Image<Gray, byte> imageGreen = image[1];
            Image<Gray, byte> imageRed = image[2];

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
        #endregion
    }
}
