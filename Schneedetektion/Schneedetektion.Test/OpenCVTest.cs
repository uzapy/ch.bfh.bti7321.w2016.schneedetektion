using Microsoft.VisualStudio.TestTools.UnitTesting;
using Schneedetektion.Data;
using Schneedetektion.ImagePlayground;
using Schneedetektion.OpenCV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Schneedetektion.Test
{
    [TestClass]
    public class OpenCVTest
    {
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private OpenCVHelper openCVHelper = new OpenCVHelper();
        private Random random = new Random();

        [TestMethod]
        public void DrawHistogramTest()
        {
            // Zufälliges Bild auswählen
            int count = dataContext.Images.Where(i => i.Day.Value).Count();
            ImageViewModel imageViewModel = new ImageViewModel(dataContext.Images.Where(i => i.Day.Value).Skip(random.Next(0, count)).First());
            string image = imageViewModel.Image.FileName;

            // Open CV
            OpenCVHelper helper = new OpenCVHelper();
            List<float[]> histogram = helper.GetHistogram(image);

            // Write to File
            //PngBitmapEncoder encoder = new PngBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(histogram));
            //using (var fileStream = new FileStream(@"C:\Users\uzapy\Desktop\test\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png", FileMode.Create))
            //{
            //    encoder.Save(fileStream);
            //}
        }

        [TestMethod]
        public void GetStatisticsTest()
        {
            // Zufälliges Bild auswählen
            int count = dataContext.Images.Where(i => i.Day.Value).Count();
            ImageViewModel imageViewModel = new ImageViewModel(dataContext.Images.Where(i => i.Day.Value).Skip(random.Next(0, count)).First());
            // Polygone laden
            IEnumerable<Polygon> polygons = dataContext.Polygons.Where(p => p.CameraName == imageViewModel.Image.Place);
            // Bild-Patches croppen
            List<PatchViewModel> patches = new List<PatchViewModel>();
            foreach (var polygon in polygons)
            {
                IEnumerable<Point> pointCollection = PolygonHelper.DeserializePointCollection(polygon.PolygonPointCollection);
                Statistic statistic = openCVHelper.GetStatisticForPatchFromImagePath(imageViewModel.Image.FileName, pointCollection);
                BitmapImage patchImage = openCVHelper.GetPatchBitmapImage(imageViewModel.Image.FileName, pointCollection);
                patches.Add(new PatchViewModel(statistic, patchImage, imageViewModel, polygon));
            }

            // Statistiken berechnen
            //foreach (var patch in patches)
            //{
            //    List<float[]> histogram = openCVHelper.GetHistogram(OpenCVHelper.BitmapImageToBitmap(patch.PatchBitmap));

            //    OpenCVColor mean;
            //    OpenCVColor standardDeviation;
            //    OpenCVColor variance;
            //    openCVHelper.GetMeanSdandardDeviationAndVariance(OpenCVHelper.BitmapImageToBitmap(patch.PatchBitmap), patch.Polygon.Bitmask, out mean, out standardDeviation, out variance);

            //    OpenCVColor median;
            //    OpenCVColor min;
            //    OpenCVColor max;
            //    OpenCVColor contrast;
            //    openCVHelper.GetMinMaxMedianAndContrast(OpenCVHelper.BitmapImageToBitmap(patch.PatchBitmap), patch.Polygon.Bitmask, out median, out min, out max, out contrast);
            //}
        }

        [TestMethod]
        public void PolygonToBitMask()
        {
            int count = dataContext.Images.Where(i => i.Day.Value).Count();
            ImageViewModel imageViewModel = new ImageViewModel(dataContext.Images.Where(i => i.Day.Value).Skip(random.Next(0, count)).First());

            Polygon polygon = dataContext.Polygons.Where(p => p.CameraName == imageViewModel.Image.Place).First();
            openCVHelper.SaveBitmask(
                imageViewModel.Image.FileName,
                @"C:\Users\uzapy\Desktop\astra2016\bitmasks\1.png",
                PolygonHelper.DeserializePointCollection(polygon.PolygonPointCollection));
        }

        [TestMethod]
        public void CombinePictures()
        {
            int[] ids = new int[] { 400049, 400050, 400051, 400052, 400053, 400054, 400194, 400195, 400196, 400197, 400338, 400339, 400340, 400341, 400342, 400481, 400482, 400483, 400484, 400485, 400486 };
            IEnumerable<string> imagePaths = dataContext.Images
                .Where(i => ids.Contains(i.ID))
                .Select(i => Path.Combine(@"C:\Users\uzapy\Desktop\astra2016", i.Place, i.Name + ".jpg"));

            openCVHelper.CombineImagesMean(imagePaths);
        }

        [TestMethod]
        public void StatisticsDistanceTest()
        {
            var statistics = dataContext.Statistics.OrderByDescending(s => s.ID).Take(2);
            Statistic one = statistics.First();
            Statistic other = statistics.Skip(1).First();

            double distance = one.DistanceTo(other);

            var orderedStatistics = dataContext.Combined_Statistics.Where(cs => cs.Polygon.CameraName == "mvk106").GroupBy(cs => cs.Images);
            var polygons = dataContext.Polygons.Where(p => p.CameraName == "mvk106");
            foreach (var groupStatistics in orderedStatistics)
            {
                foreach (var statistic in groupStatistics.OrderBy(gs => gs.Polygon_ID))
                {
                    Trace.WriteLine($"Poly: {statistic.Polygon_ID} - Stat: {statistic.ID}");
                }
            }
        }
    }
}
