using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using Schneedetektion.Data;
using Schneedetektion.GatherData.Properties;
using Schneedetektion.OpenCV;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Media = System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Schneedetektion.GatherData
{
    public class Program
    {
        private static string oldFolderName = @"C:\Users\uzapy\Desktop\astra";
        private static string folderName = Settings.Default.WorkingFolder;
        private static List<string> cameraNames = new List<string>(); // { "mvk021", "mvk101", "mvk105", "mvk107", "mvk110", "mvk120", "mvk122", "mvk131" };
        private static string[] subDirectories;
        private static string[] fileNames;
        private static StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private static OpenCVHelper openCVHelper = new OpenCVHelper();
        private static int take = 1000;
        private static List<ManualResetEvent> resetEvents = new List<ManualResetEvent>();
        private static int count = 0;

        static void Main(string[] args)
        {
            //cameraNames = dataContext.Cameras.Select(c => c.Name).ToList();
            //cameraNames = new List<string>() { "mvk021", "mvk022", "mvk050", "mvk066", "mvk099", "mvk101", "mvk102", "mvk104", "mvk105", "mvk107", "mvk108",
            //    "mvk109", "mvk110", "mvk112", "mvk114", "mvk115", "mvk116", "mvk117", "mvk118", "mvk119", "mvk120", "mvk121", "mvk122", "mvk123", "mvk125",
            //    "mvk126", "mvk127", "mvk128", "mvk129", "mvk131", "mvk132", "mvk134", "mvk156", "mvk157", "mvk158", "mvk159", "mvk160", "mvk161", "mvk162",
            //    "mvk163", "mvk164" };
            //cameraNames = new List<string>() { "mvk106" };

            CombineStatistics("mvk108");

            //CalculateImageStatistics();
            //CalculatePatchStatistics();
            // RegisterImagesInDB();
            // UpdateDateTime();
            // RemoveDataWithoutFile();
            // GetLiveImage();
            // MoveOldPictures();
        }

        private static void CombineStatistics(string camera)
        {
            Console.WriteLine($"Camera: {camera}");

            // oldest picture of camera
            DateTime date = dataContext.Images.Where(i => i.Place == camera).Min(i => i.DateTime).Date;
            // go back to monday
            date = date.AddDays(-(int)date.DayOfWeek + 1);
            Console.WriteLine($"Start Date: {date.ToShortDateString()}");

            // select polygons for camera
            var polygons = dataContext.Polygons.Where(p => p.CameraName == camera);

            // starting from this date
            while (date < DateTime.Today)
            {
                // select images in current monday to sunday period
                var imagesOfWeek = from i in dataContext.Images
                                   where i.DateTime > date
                                   where i.DateTime <= date.AddDays(7)
                                   where i.Place == camera
                                   where i.Day == true
                                   select i;

                // iterate through time slots
                for (int slotStart = 6; slotStart < 20; slotStart += 2)
                {
                    // select images in current 2 hour time slot
                    var imagesInTimeSlot = from i in imagesOfWeek
                                           where i.DateTime.Hour >= slotStart && i.DateTime.Hour <= (slotStart + 1)
                                           select i;
                    Console.WriteLine($"Time Slot: {slotStart} - {slotStart + 2}");
                    
                    // all category-permutations
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, true,  true,  false, false);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, true,  true,  true,  false);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, true,  true,  false, true);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, true,  false, false, false);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, true,  false, true,  false);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, true,  false, false, true);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, false, true,  false, false);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, false, true,  true,  false);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, false, true,  false, true);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, false, false, false, false);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, false, false, true,  false);
                    CreateCombinedStatisticFromImages(imagesInTimeSlot, polygons, date, slotStart, false, false, false, true);
                }

                // save changes (one week worth)
                dataContext.SubmitChanges();
                Console.WriteLine("Saved!");

                // next week
                date = date.AddDays(7);
                Console.WriteLine($"Start Date: {date.ToShortDateString()}");
            }
        }

        private static void CreateCombinedStatisticFromStatistics(IQueryable<Image> images, IQueryable<Polygon> polygons, DateTime date, int slotStart,
            bool snow, bool badlighting, bool foggy, bool rainy)
        {
            images = images.Where(i => i.Snow == snow && i.BadLighting == badlighting && i.Foggy == foggy && i.Rainy == rainy);

            // wenn weniger als 2 bilder in kollektion => verwerfen
            if (images.Count() < 2)
            {
                return;
            }

            foreach (var polygon in polygons)
            {
                Combined_Statistic combinedStatistic = new Combined_Statistic();
                combinedStatistic.Statistic = new Statistic();
                combinedStatistic.Polygon = polygon;
                combinedStatistic.Images = JsonConvert.SerializeObject(images.Select(i => i.ID));
                combinedStatistic.StartTime = slotStart;
                combinedStatistic.EndTime = (slotStart + 2);
                combinedStatistic.StartOfWeek = date;
                combinedStatistic.Snow = snow;
                combinedStatistic.BadLighting = badlighting;
                combinedStatistic.Foggy = foggy;
                combinedStatistic.Rainy = rainy;

                var statistics = images.SelectMany(i => i.Entity_Statistics).Where(es => es.Polygon_ID == polygon.ID).Select(es => es.Statistic);

                combinedStatistic.Statistic.BlueHistogramList      = GetAverageHistogram(statistics.Select(s => s.BlueHistogramList));
                combinedStatistic.Statistic.GreenHistogramList     = GetAverageHistogram(statistics.Select(s => s.GreenHistogramList));
                combinedStatistic.Statistic.RedHistogramList       = GetAverageHistogram(statistics.Select(s => s.RedHistogramList));
                combinedStatistic.Statistic.ModeBlue               = statistics.Select(s => s.ModeBlue).Average();
                combinedStatistic.Statistic.ModeGreen              = statistics.Select(s => s.ModeGreen).Average();
                combinedStatistic.Statistic.ModeRed                = statistics.Select(s => s.ModeRed).Average();
                combinedStatistic.Statistic.MeanBlue               = statistics.Select(s => s.MeanBlue).Average();
                combinedStatistic.Statistic.MeanGreen              = statistics.Select(s => s.MeanGreen).Average();
                combinedStatistic.Statistic.MeanRed                = statistics.Select(s => s.MeanRed).Average();
                combinedStatistic.Statistic.MedianBlue             = statistics.Select(s => s.MedianBlue).Average();
                combinedStatistic.Statistic.MedianGreen            = statistics.Select(s => s.MedianGreen).Average();
                combinedStatistic.Statistic.MedianRed              = statistics.Select(s => s.MedianRed).Average();
                combinedStatistic.Statistic.MinimumBlue            = statistics.Select(s => s.MinimumBlue).Average();
                combinedStatistic.Statistic.MinimumGreen           = statistics.Select(s => s.MinimumGreen).Average();
                combinedStatistic.Statistic.MinimumRed             = statistics.Select(s => s.MinimumRed).Average();
                combinedStatistic.Statistic.MaximumBlue            = statistics.Select(s => s.MaximumBlue).Average();
                combinedStatistic.Statistic.MaximumGreen           = statistics.Select(s => s.MaximumGreen).Average();
                combinedStatistic.Statistic.MaximumRed             = statistics.Select(s => s.MaximumRed).Average();
                combinedStatistic.Statistic.StandardDeviationBlue  = statistics.Select(s => s.StandardDeviationBlue).Average();
                combinedStatistic.Statistic.StandardDeviationGreen = statistics.Select(s => s.StandardDeviationGreen).Average();
                combinedStatistic.Statistic.StandardDeviationRed   = statistics.Select(s => s.StandardDeviationRed).Average();
                combinedStatistic.Statistic.VarianceBlue           = statistics.Select(s => s.VarianceBlue).Average();
                combinedStatistic.Statistic.VarianceGreen          = statistics.Select(s => s.VarianceGreen).Average();
                combinedStatistic.Statistic.VarianceRed            = statistics.Select(s => s.VarianceRed).Average();
                combinedStatistic.Statistic.ContrastBlue           = statistics.Select(s => s.ContrastBlue).Average();
                combinedStatistic.Statistic.ContrastGreen          = statistics.Select(s => s.ContrastGreen).Average();
                combinedStatistic.Statistic.ContrastRed            = statistics.Select(s => s.ContrastRed).Average();
            }
        }

        private static void CreateCombinedStatisticFromImages(IQueryable<Image> images, IQueryable<Polygon> polygons, DateTime date, int slotStart,
            bool snow, bool badlighting, bool foggy, bool rainy)
        {
            // filter catergory
            images = images.Where(i => i.Snow == snow && i.BadLighting == badlighting && i.Foggy == foggy && i.Rainy == rainy);

            // wenn weniger als 2 bilder in kollektion => verwerfen
            if (images.Count() < 2)
            {
                return;
            }
            Console.WriteLine($"Snow = {snow}\t Bad Lighting = {badlighting}\t Foggy = {foggy}\t Rainy {rainy}");
            Console.WriteLine($"Found Images: {images.Count()}");

            // combine images
            Image<Bgr, byte> combinedImage = openCVHelper.CombineImages(images.Select(i => Path.Combine(folderName, i.Place, i.Name + ".jpg")));
            combinedImage.Save(@"C:\Users\uzapy\Desktop\test\" + count++ + ".png");
            Console.WriteLine(@"C:\Users\uzapy\Desktop\test\" + (count-1) + ".png");

            foreach (var polygon in polygons)
            {
                var polygonPoints = JsonConvert.DeserializeObject<Media.PointCollection>(polygon.PolygonPointCollection);
                // calculate statistic
                Statistic statistic = openCVHelper.GetStatisticForPatchFromImage(combinedImage, polygonPoints);

                Combined_Statistic combinedStatistic = new Combined_Statistic();
                combinedStatistic.Statistic = statistic;
                combinedStatistic.Polygon = polygon;
                combinedStatistic.Images = JsonConvert.SerializeObject(images.Select(i => i.ID));
                combinedStatistic.StartTime = slotStart;
                combinedStatistic.EndTime = (slotStart + 2);
                combinedStatistic.StartOfWeek = date;
                combinedStatistic.Snow = snow;
                combinedStatistic.BadLighting = badlighting;
                combinedStatistic.Foggy = foggy;
                combinedStatistic.Rainy = rainy;

                Console.WriteLine($"Polygon: {combinedStatistic.Polygon.ID}");
            }
        }

        private static List<double> GetAverageHistogram(IQueryable<List<double>> histograms)
        {
            List<List<double>> histogramList = histograms.ToList();
            List<double> averageHistogram = new List<double>();
            averageHistogram.AddRange(new double[256]);

            for (int i = 0; i < averageHistogram.Count; i++)
            {
                averageHistogram[i] = histogramList.Select(h => h[i]).Average();
            }

            return averageHistogram;
        }

        private static void CalculateImageStatistics()
        {
            IEnumerable<Image> images = (from i in dataContext.Images
                                        where i.Place == "mvk108"
                                        where i.Day == true
                                        where i.Entity_Statistics.Count == 0
                                        select i).Take(take);
            
            foreach (var image in images)
            {
                ManualResetEvent resetEvent = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(arg =>
                {
                    Statistic imageStatistic = openCVHelper.GetStatisticForImage(Path.Combine(folderName, image.Place, image.Name + ".jpg"));
                    Console.WriteLine(image.ID + " - " + image.Name);

                    image.Entity_Statistics.Add(new Entity_Statistic()
                    {
                        Statistic = imageStatistic
                    });
                    resetEvent.Set();
                });
                resetEvents.Add(resetEvent);
            }

            WaitHandle.WaitAll(resetEvents.ToArray());

            dataContext.SubmitChanges();
            Console.WriteLine("Saved!");

            if (images.Count() > 0)
            {
                resetEvents.Clear();
                CalculateImageStatistics();
            }
            else
            {
                Console.ReadLine();
            }
        }

        private static void CalculatePatchStatistics()
        {
            string camera = "mvk108";
            IEnumerable<Polygon> polygons = dataContext.Polygons.Where(p => p.CameraName == camera);

            IEnumerable<Image> images = (from i in dataContext.Images
                                            where i.Place == camera
                                            where i.Day == true
                                            where (from es in i.Entity_Statistics where es.Polygon_ID != null select es).Count() == 0
                                            select i).Take(take);

            foreach (var image in images)
            {

                foreach (var polygon in polygons)
                {
                    var imagePath = Path.Combine(folderName, image.Place, image.Name + ".jpg");
                    var polygonPoints = JsonConvert.DeserializeObject<Media.PointCollection>(polygon.PolygonPointCollection);
                    Statistic imageStatistic = openCVHelper.GetStatisticForPatchFromImagePath(imagePath, polygonPoints);

                    Console.WriteLine("I: " + image.ID + " - " + image.Name + " - P:" + polygon.ID + " - " + polygon.ImageArea);

                    image.Entity_Statistics.Add(new Entity_Statistic()
                    {
                        Statistic = imageStatistic,
                        Polygon = polygon
                    });
                }
            }

            dataContext.SubmitChanges();
            Console.WriteLine("Saved!");

            CalculatePatchStatistics();
        }

        private static void MoveOldPictures()
        {
            foreach (string folder in cameraNames)
            {
                if (Directory.Exists(oldFolderName + "\\" + folder))
                {
                    subDirectories = Directory.GetDirectories(oldFolderName + "\\" + folder);

                    foreach (var subDir in subDirectories)
                    {
                        fileNames = Directory.GetFiles(subDir);

                        foreach (var file in fileNames)
                        {
                            string newFile = Path.Combine(folderName, folder, Path.GetFileName(file));
                            File.Copy(file, newFile, true);
                            Console.WriteLine(file + " => " + Path.Combine(folderName, folder, Path.GetFileName(file)));
                        }
                    }
                }
            }
        }

        private static void RegisterImagesInDB()
        {
            foreach (var cameraName in cameraNames)
            {
                Console.WriteLine(cameraName);

                string folder = folderName + "\\" + cameraName;

                Console.WriteLine(folder);

                DateTime lowerLimit = new DateTime(2014, 1, 1);

                if (Directory.Exists(folder))
                {
                    foreach (var imageName in Directory.GetFiles(folder))
                    {
                        Image image = new Image();
                        image.Name = Path.GetFileNameWithoutExtension(imageName);
                        image.Place = cameraName;

                        string fileContent = Encoding.ASCII.GetString(File.ReadAllBytes(imageName));

                        if (!string.IsNullOrEmpty(fileContent))
                        {
                            string[] splitFileContent = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                            string dat = splitFileContent.Where(sfc => sfc.StartsWith("DAT")).FirstOrDefault().Split('=').Last();
                            string tim = splitFileContent.Where(sfc => sfc.StartsWith("TIM")).FirstOrDefault().Split('=').Last();
                            string tzn = splitFileContent.Where(sfc => sfc.StartsWith("TZN")).FirstOrDefault().Split('=').Last();
                            string tit = splitFileContent.Where(sfc => sfc.StartsWith("TIT")).FirstOrDefault().Split('=').Last();

                            try
                            {
                                DateTime dateTime = DateTime.ParseExact((dat + " " + tim), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                                image.DateTime = dateTime;
                                image.TimeZone = tzn;
                                image.UnixTime = double.Parse(tit, CultureInfo.InvariantCulture);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        else
                        {
                            continue;
                        }


                        if (!(image.DateTime < lowerLimit || image.DateTime > DateTime.Now || image.UnixTime < 1417500000))
                        {
                            dataContext.Images.InsertOnSubmit(image);
                        }
                        Console.WriteLine(image.Name + " | " + image.Place + " | " + image.DateTime + " | " + image.UnixTime);
                        // dataContext.SubmitChanges();
                    }
                }

                dataContext.SubmitChanges();

                Console.WriteLine("Finished!");
            }

            Console.ReadLine();
        }

        private static void UpdateDateTime()
        {
            foreach (string cameraName in cameraNames)
            {
                Console.WriteLine(cameraName);

                string folder = folderName + "\\" + cameraName;

                foreach (var subFolder in Directory.GetDirectories(folder))
                {
                    Console.WriteLine(subFolder);

                    foreach (var imageName in Directory.GetFiles(subFolder))
                    {
                        Console.WriteLine(imageName);
                        string fileName = Path.GetFileNameWithoutExtension(imageName);
                        Image image = dataContext.Images.Where(i => i.Name == fileName).FirstOrDefault();

                        string fileContent = Encoding.ASCII.GetString(File.ReadAllBytes(imageName));

                        if (!string.IsNullOrEmpty(fileContent))
                        {
                            string[] splitFileContent = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                            string dat = splitFileContent.Where(sfc => sfc.StartsWith("DAT")).FirstOrDefault().Split('=').Last();
                            string tim = splitFileContent.Where(sfc => sfc.StartsWith("TIM")).FirstOrDefault().Split('=').Last();
                            string tzn = splitFileContent.Where(sfc => sfc.StartsWith("TZN")).FirstOrDefault().Split('=').Last();
                            string tit = splitFileContent.Where(sfc => sfc.StartsWith("TIT")).FirstOrDefault().Split('=').Last();

                            try
                            {
                                DateTime dateTime = DateTime.ParseExact((dat + " " + tim), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                                image.DateTime = dateTime;
                                image.TimeZone = tzn;
                                image.UnixTime = double.Parse(tit, CultureInfo.InvariantCulture);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("File deleted: " + imageName);
                            //File.Delete(imageName);
                            File.Move(imageName, @"C:\Users\uzapy\Desktop\astra\meta\delete candidates\" + Path.GetFileName(imageName));
                            dataContext.Images.DeleteOnSubmit(image);
                        }
                    }
                }

                dataContext.SubmitChanges();
                Console.WriteLine("Finished!");
            }
        }

        private static void RemoveDataWithoutFile()
        {
            foreach (Image image in dataContext.Images.Where(i => i.UnixTime == null))
            {
                string imagePath = folderName + "\\" + image.Place + "\\" + image.Name.Split('_')[1] + "\\" + image.Name + ".jpg";
                if (!File.Exists(imagePath))
                {
                    Console.WriteLine("Image deleted: " + image.Name);
                    dataContext.Images.DeleteOnSubmit(image);
                }
            }

            dataContext.SubmitChanges();
            Console.WriteLine("Finished!");
        }

        private static void GetLiveImage()
        {
            WebClient webClient = new WebClient();
            webClient.Headers["Cookie"] = "PHPSESSID=6dook56psrptp83461mh3mpip4";
            for (int i = 0; i < 30; i++)
            {
                foreach (var cameraName in cameraNames)
                {
                    if (!Directory.Exists("C:\\Users\\uzapy\\Desktop\\astra\\live\\" + cameraName + "\\20160113"))
                    {
                        Directory.CreateDirectory("C:\\Users\\uzapy\\Desktop\\astra\\live\\" + cameraName + "\\20160113");
                    }

                    string imageFullPath = "C:\\Users\\uzapy\\Desktop\\astra\\live\\" + cameraName + "\\20160113\\"
                        + cameraName + "_20160113_" + DateTime.Now.ToString("HHmmss") + ".jpg";


                    webClient.DownloadFile("http://www.astramobcam.ch/kamera/" + cameraName + "/live.jpg", imageFullPath);

                    Console.WriteLine(imageFullPath);

                    System.Threading.Thread.Sleep(1000);
                }
            }

            Console.ReadLine();
        }
    }
}
