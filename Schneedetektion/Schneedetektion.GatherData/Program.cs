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
using System.Windows.Media;
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

        static void Main(string[] args)
        {
            //cameraNames = dataContext.Cameras.Select(c => c.Name).ToList();
            //cameraNames = new List<string>() { "mvk021", "mvk022", "mvk050", "mvk066", "mvk099", "mvk101", "mvk102", "mvk104", "mvk105", "mvk107", "mvk108",
            //    "mvk109", "mvk110", "mvk112", "mvk114", "mvk115", "mvk116", "mvk117", "mvk118", "mvk119", "mvk120", "mvk121", "mvk122", "mvk123", "mvk125",
            //    "mvk126", "mvk127", "mvk128", "mvk129", "mvk131", "mvk132", "mvk134", "mvk156", "mvk157", "mvk158", "mvk159", "mvk160", "mvk161", "mvk162",
            //    "mvk163", "mvk164" };
            //cameraNames = new List<string>() { "mvk106" };

            CombineStatistics("mvk106");

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
            DateTime date = dataContext.Images.Where(i => i.Place == camera).Min(i => i.DateTime).Date;

            while (date < DateTime.Today)
            {
                var imagesOfWeek = from i in dataContext.Images
                                   where i.DateTime > date
                                   where i.DateTime <= date.AddDays(7)
                                   where i.Place == camera
                                   where i.Day == true
                                   select i;

                int startHour = 6;
                int endHour = 8;

                for (int j = 0; j < 7; j++)
                {
                    var imagesInSlot = from i in imagesOfWeek
                                       where i.DateTime.Hour >= startHour && i.DateTime.Hour <= endHour
                                       select i;

                    // var entity_statistics = imagesInSlot.SelectMany(i => i.Entity_Statistics);
                    // var statistics = entity_statistics.Where(es => es.Polygon.CameraName == camera).OrderBy(es => es.Polygon_ID);
                    var polygons = dataContext.Polygons.Where(p => p.CameraName == camera);

                    foreach (var polygon in polygons)
                    {
                        Combined_Statistic combinedStatistic = new Combined_Statistic();
                        combinedStatistic.Statistic = new Statistic();
                        combinedStatistic.Polygon = polygon;
                        combinedStatistic.StartTime = startHour;
                        combinedStatistic.EndTime = endHour;

                        var entity_statistics = imagesInSlot.SelectMany(i => i.Entity_Statistics).Where(es => es.Polygon_ID == polygon.ID);
                    }

                    startHour += 2;
                    endHour += 2;
                }

                date.AddDays(7);
            }
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
            BitmapImage patchImage = new BitmapImage();

            IEnumerable<Image> images = (from i in dataContext.Images
                                            where i.Place == camera
                                            where i.Day == true
                                            where (from es in i.Entity_Statistics where es.Polygon_ID != null select es).Count() == 0
                                            select i).Take(take);

            foreach (var image in images)
            {

                foreach (var polygon in polygons)
                {
                    var polygonPoints = JsonConvert.DeserializeObject<PointCollection>(polygon.PolygonPointCollection);
                    Statistic imageStatistic = openCVHelper.GetStatisticForPatch(
                    Path.Combine(folderName, image.Place, image.Name + ".jpg"), polygonPoints, out patchImage);

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
