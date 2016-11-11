using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Schneedetektion.Data;
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

        [Obsolete]
        public List<float[]> GetHistogram(Drawing.Bitmap bitmap)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(bitmap);
            return GetHistogram(image);
        }

        public Statistic GetStatisticForPatch(string imagePath, IEnumerable<Point> pointCollection, out BitmapImage patchImage)
        {
            // Create Matrix 
            Mat matrix = new Mat(imagePath, LoadImageType.AnyColor);
            UMat uMatrix = matrix.ToUMat(AccessType.ReadWrite);
            // Create Bitmask
            Image<Gray, byte> bitmask = new Image<Gray, byte>(uMatrix.Cols, uMatrix.Rows, new Gray(0));

            // Scale and Transform Polygon
            List<Point> scaledPoints = GetScaledPoints(pointCollection, uMatrix.Cols, uMatrix.Rows);
            List<Drawing.Point> polygonPoints = GetInvertedPolygonPoints(scaledPoints, uMatrix.Cols, uMatrix.Rows);

            // Apply Polygon
            using (VectorOfPoint vPoint = new VectorOfPoint(polygonPoints.ToArray()))
            using (VectorOfVectorOfPoint vvPoint = new VectorOfVectorOfPoint(vPoint))
            {
                // Alles schwarz anmalen, dass nicht im Polygon ist
                CvInvoke.FillPoly(uMatrix, vvPoint, new Bgr(0, 0, 0).MCvScalar);
                // Alles weiss anmalen, dass nicht im Polygon ist
                CvInvoke.FillPoly(bitmask, vvPoint, new Gray(255).MCvScalar);
            }

            // Create Image from uMatrix
            Image<Bgr, byte> image = new Image<Bgr, byte>(uMatrix.Bitmap);

            // Crop Bitmaps
            image.ROI = GetRegionOfInterest(scaledPoints);
            bitmask.ROI = GetRegionOfInterest(scaledPoints);

            // Pro Kanal ein Grauwert-Bild erstellen
            Image<Gray, byte> blueChannel  = image[(int)EChannel.Blue];
            Image<Gray, byte> greenChannel = image[(int)EChannel.Green];
            Image<Gray, byte> redChannel   = image[(int)EChannel.Red];

            // Pro Kanal eine Liste der Länge 256
            List<int> blueHistogram = new List<int>();
            blueHistogram.AddRange(new int[256]);
            List<int> greenHistogram = new List<int>();
            greenHistogram.AddRange(new int[256]);
            List<int> redHistogram = new List<int>();
            redHistogram.AddRange(new int[256]);

            // List for other Statistic Values
            List<double> bluePixels  = new List<double>();
            List<double> greenPixels = new List<double>();
            List<double> redPixels   = new List<double>();

            // Ein Loop für alles
            for (int i = 0; i < image.Cols; i++)
            {
                for (int j = 0; j < image.Rows; j++)
                {
                    if (bitmask[j, i].MCvScalar.V0 == 0)
                    {
                        // Histogram abfüllen
                        blueHistogram[(int)blueChannel[j, i].MCvScalar.V0]++;
                        greenHistogram[(int)greenChannel[j, i].MCvScalar.V0]++;
                        redHistogram[(int)redChannel[j, i].MCvScalar.V0]++;

                        // Pixel in Listen abspitzen
                        bluePixels.Add(blueChannel[j, i].MCvScalar.V0);
                        greenPixels.Add(greenChannel[j, i].MCvScalar.V0);
                        redPixels.Add(redChannel[j, i].MCvScalar.V0);
                    }
                }
            }

            Statistic statistic = new Statistic();
            
            // Histogram
            statistic.SetHistogram(blueHistogram, EChannel.Blue);
            statistic.SetHistogram(greenHistogram, EChannel.Green);
            statistic.SetHistogram(redHistogram, EChannel.Red);

            // Mode
            statistic.ModeBlue  = blueHistogram.IndexOf(blueHistogram.Max());
            statistic.ModeGreen = greenHistogram.IndexOf(greenHistogram.Max());
            statistic.ModeRed   = redHistogram.IndexOf(redHistogram.Max());

            // Mean
            statistic.MeanBlue  = bluePixels.Average();
            statistic.MeanGreen = greenPixels.Average();
            statistic.MeanRed   = redPixels.Average();

            // Variance
            statistic.VarianceBlue  = bluePixels.Sum(i => Math.Pow(i - statistic.MeanBlue.Value, 2)) / (double)bluePixels.Count;
            statistic.VarianceGreen = greenPixels.Sum(i => Math.Pow(i - statistic.MeanGreen.Value, 2)) / (double)greenPixels.Count;
            statistic.VarianceRed   = redPixels.Sum(i => Math.Pow(i - statistic.MeanRed.Value, 2)) / (double)redPixels.Count;

            // Standard Deviation
            statistic.StandardDeviationBlue  = Math.Sqrt(statistic.VarianceBlue.Value);
            statistic.StandardDeviationGreen = Math.Sqrt(statistic.VarianceGreen.Value);
            statistic.StandardDeviationRed   = Math.Sqrt(statistic.VarianceRed.Value);

            // Minimum
            statistic.MinimumBlue  = bluePixels.Min();
            statistic.MinimumGreen = greenPixels.Min();
            statistic.MinimumRed   = redPixels.Min();

            // Maximum
            statistic.MaximumBlue  = bluePixels.Max();
            statistic.MaximumGreen = greenPixels.Max();
            statistic.MaximumRed   = redPixels.Max();

            // Contrast
            statistic.ContrastBlue  = (statistic.MaximumBlue.Value - statistic.MinimumBlue.Value) / (statistic.MaximumBlue.Value + statistic.MinimumBlue.Value);
            statistic.ContrastGreen = (statistic.MaximumGreen.Value - statistic.MinimumGreen.Value) / (statistic.MaximumGreen.Value + statistic.MinimumGreen.Value);
            statistic.ContrastRed  = (statistic.MaximumRed.Value - statistic.MinimumRed.Value) / (statistic.MaximumRed.Value + statistic.MinimumRed.Value);

            // Median
            bluePixels.Sort();
            greenPixels.Sort();
            redPixels.Sort();
            int middle = bluePixels.Count / 2;
            if (bluePixels.Count % 2 == 0) // Gerade Anzahl
            {
                statistic.MedianBlue = (bluePixels.ElementAt(middle) + bluePixels.ElementAt(middle - 1)) / 2d;
                statistic.MedianGreen = (greenPixels.ElementAt(middle) + greenPixels.ElementAt(middle - 1)) / 2d;
                statistic.MedianRed = (redPixels.ElementAt(middle) + redPixels.ElementAt(middle - 1)) / 2d;
            }
            else // Ungerade Anzahl
            {
                statistic.MedianBlue  = bluePixels.ElementAt(middle);
                statistic.MedianGreen = greenPixels.ElementAt(middle);
                statistic.MedianRed   = redPixels.ElementAt(middle);
            }

            // Return
            patchImage = OpenCVHelper.BitmapToBitmapImage(image.Bitmap);
            return statistic;
        }

        public Statistic GetStatisticForImage(string fileName)
        {
            return GetStatistic(new Image<Bgr, byte>(fileName));
        }

        public void SaveBitmask(string imagePath, string bitmaskPath, IEnumerable<Point> pointCollection)
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

            // Crop of Bitmask ROI
            bitMask.ROI = GetRegionOfInterest(scaledPoints);

            // TODO: Alle Werte hier berechnen (auch Histogram)

            bitMask.Save(bitmaskPath);
        }

        [Obsolete]
        public void GetMeanSdandardDeviationAndVariance(Drawing.Bitmap bitmap, string bitmaskPath, out OpenCVColor color, out OpenCVColor sdtDev, out OpenCVColor variance)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(bitmap);
            Image<Gray, byte> bitmask = new Image<Gray, byte>(bitmaskPath);

            Bgr average;
            MCvScalar standardDeviation;
            image.AvgSdv(out average, out standardDeviation, bitmask);

            color.Blue     = average.Blue;
            color.Green    = average.Green;
            color.Red      = average.Red;
            sdtDev.Blue    = standardDeviation.V0;
            sdtDev.Green   = standardDeviation.V1;
            sdtDev.Red     = standardDeviation.V2;
            variance.Blue  = Math.Pow(standardDeviation.V0, 2);
            variance.Green = Math.Pow(standardDeviation.V1, 2);
            variance.Red   = Math.Pow(standardDeviation.V2, 2);
        }

        [Obsolete]
        public void GetMinMaxMedianAndContrast(Drawing.Bitmap bitmap, string bitmaskPath,
            out OpenCVColor min, out OpenCVColor max, out OpenCVColor median, out OpenCVColor contrast)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(bitmap);
            Image<Gray, byte> bitmask = new Image<Gray, byte>(bitmaskPath);

            List<double> blue = new List<double>();
            List<double> green = new List<double>();
            List<double> red = new List<double>();

            for (int i = 0; i < image.Cols; i++)
            {
                for (int j = 0; j < image.Rows; j++)
                {
                    if (bitmask[j, i].MCvScalar.V0 > 0)
                    {
                        blue.Add(image[j, i].MCvScalar.V0);
                        green.Add(image[j, i].MCvScalar.V1);
                        red.Add(image[j, i].MCvScalar.V2);
                    }
                }
            }

            blue.Sort();
            green.Sort();
            red.Sort();

            int count = blue.Count();
            int middle = count / 2;
            if (count % 2 == 0) // Gerade Anzahl
            {
                median.Blue = blue.ElementAt(middle) + blue.ElementAt(middle - 1) / 2;
                median.Green = green.ElementAt(middle) + green.ElementAt(middle - 1) / 2;
                median.Red = red.ElementAt(middle) + red.ElementAt(middle - 1) / 2;
            }
            else // Ungerade Anzahl
            {
                median.Blue = blue.ElementAt(middle);
                median.Green = green.ElementAt(middle);
                median.Red = red.ElementAt(middle);
            }

            min.Blue       = blue.First();
            min.Green      = green.First();
            min.Red        = red.First();
            max.Blue       = blue.Last();
            max.Green      = green.Last();
            max.Red        = red.Last();
            contrast.Blue  = (max.Blue - min.Blue) / (max.Blue + min.Blue);
            contrast.Green = (max.Green - min.Green) / (max.Green + min.Green);
            contrast.Red   = (max.Red - min.Red) / (max.Red + min.Red);
        }
        #endregion

        #region Private Methods
        private List<Point> GetScaledPoints(IEnumerable<Point> polygonPoints, int numberOfCols, int numberOfRows)
        {
            List<Point> scaledPoints = new List<Point>();
            foreach (var point in polygonPoints)
            {
                scaledPoints.Add(new Point()
                {
                    X = point.X * numberOfCols,
                    Y = point.Y * numberOfRows
                });
            }
            return scaledPoints;
        }

        private List<Drawing.Point> GetInvertedPolygonPoints(List<Point> scaledPoints, int numberOfCols, int numberOfRows)
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

            DenseHistogram histogram = new DenseHistogram(256, new RangeF(0.0f, 256.0f));

            // Histogram per Color
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

        private Statistic GetStatistic(Image<Bgr, byte> image, Image<Gray, byte> bitmask = null)
        {
            // Pro Kanal ein Grauwert-Bild erstellen
            Image<Gray, byte> blueChannel = image[(int)EChannel.Blue];
            Image<Gray, byte> greenChannel = image[(int)EChannel.Green];
            Image<Gray, byte> redChannel = image[(int)EChannel.Red];

            // Pro Kanal eine Liste der Länge 256
            List<int> blueHistogram = new List<int>();
            blueHistogram.AddRange(new int[256]);
            List<int> greenHistogram = new List<int>();
            greenHistogram.AddRange(new int[256]);
            List<int> redHistogram = new List<int>();
            redHistogram.AddRange(new int[256]);

            // List for other Statistic Values
            List<double> bluePixels = new List<double>();
            List<double> greenPixels = new List<double>();
            List<double> redPixels = new List<double>();

            // Ein Loop für alles
            for (int i = 0; i < image.Cols; i++)
            {
                for (int j = 0; j < image.Rows; j++)
                {
                    // Wenn keine Bitmask angegeben wurde, oder die Bitmaske an dieser stelle schwarz ist
                    if (bitmask == null || bitmask[j, i].MCvScalar.V0 == 0)
                    {
                        // Histogram abfüllen
                        blueHistogram[(int)blueChannel[j, i].MCvScalar.V0]++;
                        greenHistogram[(int)greenChannel[j, i].MCvScalar.V0]++;
                        redHistogram[(int)redChannel[j, i].MCvScalar.V0]++;

                        // Pixel in Listen abspitzen
                        bluePixels.Add(blueChannel[j, i].MCvScalar.V0);
                        greenPixels.Add(greenChannel[j, i].MCvScalar.V0);
                        redPixels.Add(redChannel[j, i].MCvScalar.V0);
                    }
                }
            }

            Statistic statistic = new Statistic();

            // Histogram
            statistic.SetHistogram(blueHistogram, EChannel.Blue);
            statistic.SetHistogram(greenHistogram, EChannel.Green);
            statistic.SetHistogram(redHistogram, EChannel.Red);

            // Mode
            statistic.ModeBlue = blueHistogram.IndexOf(blueHistogram.Max());
            statistic.ModeGreen = greenHistogram.IndexOf(greenHistogram.Max());
            statistic.ModeRed = redHistogram.IndexOf(redHistogram.Max());

            // Mean
            statistic.MeanBlue = bluePixels.Average();
            statistic.MeanGreen = greenPixels.Average();
            statistic.MeanRed = redPixels.Average();

            // Variance
            statistic.VarianceBlue = bluePixels.Sum(i => Math.Pow(i - statistic.MeanBlue.Value, 2)) / (double)bluePixels.Count;
            statistic.VarianceGreen = greenPixels.Sum(i => Math.Pow(i - statistic.MeanGreen.Value, 2)) / (double)greenPixels.Count;
            statistic.VarianceRed = redPixels.Sum(i => Math.Pow(i - statistic.MeanRed.Value, 2)) / (double)redPixels.Count;

            // Standard Deviation
            statistic.StandardDeviationBlue = Math.Sqrt(statistic.VarianceBlue.Value);
            statistic.StandardDeviationGreen = Math.Sqrt(statistic.VarianceGreen.Value);
            statistic.StandardDeviationRed = Math.Sqrt(statistic.VarianceRed.Value);

            // Minimum
            statistic.MinimumBlue = bluePixels.Min();
            statistic.MinimumGreen = greenPixels.Min();
            statistic.MinimumRed = redPixels.Min();

            // Maximum
            statistic.MaximumBlue = bluePixels.Max();
            statistic.MaximumGreen = greenPixels.Max();
            statistic.MaximumRed = redPixels.Max();

            // Contrast
            statistic.ContrastBlue = (statistic.MaximumBlue.Value - statistic.MinimumBlue.Value) / (statistic.MaximumBlue.Value + statistic.MinimumBlue.Value);
            statistic.ContrastGreen = (statistic.MaximumGreen.Value - statistic.MinimumGreen.Value) / (statistic.MaximumGreen.Value + statistic.MinimumGreen.Value);
            statistic.ContrastRed = (statistic.MaximumRed.Value - statistic.MinimumRed.Value) / (statistic.MaximumRed.Value + statistic.MinimumRed.Value);

            // Median
            bluePixels.Sort();
            greenPixels.Sort();
            redPixels.Sort();
            int middle = bluePixels.Count / 2;
            if (bluePixels.Count % 2 == 0) // Gerade Anzahl
            {
                statistic.MedianBlue = (bluePixels.ElementAt(middle) + bluePixels.ElementAt(middle - 1)) / 2d;
                statistic.MedianGreen = (greenPixels.ElementAt(middle) + greenPixels.ElementAt(middle - 1)) / 2d;
                statistic.MedianRed = (redPixels.ElementAt(middle) + redPixels.ElementAt(middle - 1)) / 2d;
            }
            else // Ungerade Anzahl
            {
                statistic.MedianBlue = bluePixels.ElementAt(middle);
                statistic.MedianGreen = greenPixels.ElementAt(middle);
                statistic.MedianRed = redPixels.ElementAt(middle);
            }

            // Return
            return statistic;
        }
        #endregion
    }
}
