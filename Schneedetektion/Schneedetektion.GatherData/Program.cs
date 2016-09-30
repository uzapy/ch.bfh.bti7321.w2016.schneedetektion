using Schneedetektion.Data;
using Schneedetektion.GatherData.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

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

        static void Main(string[] args)
        {
            //cameraNames = dataContext.Cameras.Select(c => c.Name).ToList();
            cameraNames = new List<string>() { "mvk021", "mvk022", "mvk050", "mvk066", "mvk099", "mvk101", "mvk102", "mvk104", "mvk105", "mvk107", "mvk108",
                "mvk109", "mvk110", "mvk112", "mvk114", "mvk115", "mvk116", "mvk117", "mvk118", "mvk119", "mvk120", "mvk121", "mvk122", "mvk123", "mvk125",
                "mvk126", "mvk127", "mvk128", "mvk129", "mvk131", "mvk132", "mvk134", "mvk156", "mvk157", "mvk158", "mvk159", "mvk160", "mvk161", "mvk162",
                "mvk163", "mvk164" };

            // RegisterImagesInDB();
            // UpdateDateTime();
            // RemoveDataWithoutFile();
            // GetLiveImage();
            MoveOldPictures();
        }

        private static void RegisterImagesInDB()
        {
            foreach (var cameraName in cameraNames)
            {
                Console.WriteLine(cameraName);

                string folder = folderName + "\\" + cameraName;

                foreach (var subFolder in Directory.GetDirectories(folder))
                {
                    Console.WriteLine(subFolder);

                    foreach (var imageName in Directory.GetFiles(subFolder))
                    {
                        Image image = new Image();
                        image.Name = Path.GetFileNameWithoutExtension(imageName);
                        image.Place = cameraName;
                        int year = Int32.Parse(image.Name.Substring(7, 4));
                        int month = Int32.Parse(image.Name.Substring(11, 2));
                        int day = Int32.Parse(image.Name.Substring(13, 2));
                        int hour = Int32.Parse(image.Name.Substring(16, 2));
                        int minutes = Int32.Parse(image.Name.Substring(18, 2));
                        int seconds = Int32.Parse(image.Name.Substring(20, 2));
                        image.DateTime = new DateTime(year, month, day, hour, minutes, seconds);
                        dataContext.Images.InsertOnSubmit(image);
                    }
                }
                dataContext.SubmitChanges();

                Console.WriteLine("Finished!");
            }
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
    }
}
