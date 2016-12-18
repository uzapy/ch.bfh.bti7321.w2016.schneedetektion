﻿using Schneedetektion.Data;
using Schneedetektion.OpenCV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Schneedetektion.ImagePlayground
{
    public partial class SlidingNNNClassification : UserControl
    {
        #region Fields
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private OpenCVHelper openCVHelper = new OpenCVHelper();
        private ObservableCollection<string> combinationMethods = new ObservableCollection<string>();
        private ObservableCollection<string> cameraNames = new ObservableCollection<string>();
        private ObservableCollection<ClassificationViewModel> classificationViewModels = new ObservableCollection<ClassificationViewModel>();
        private List<Polygon> polygons = new List<Polygon>();
        private IQueryable<Combined_Statistic> combinedStatistics;
        private Random random = new Random();
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        #endregion

        #region Constructor
        public SlidingNNNClassification()
        {
            InitializeComponent();

            combinationMethodList.ItemsSource = combinationMethods;
            cameraList.ItemsSource            = cameraNames;
            imageContainer.ItemsSource        = classificationViewModels;

            IEnumerable<string> methods = dataContext.Combined_Statistics.Select(cs => cs.CombinationMethod).Distinct();
            foreach (var method in methods)
            {
                combinationMethods.Add(method);
            }

            IEnumerable<string> cameras = dataContext.Combined_Statistics.Select(cs => cs.Polygon.CameraName).Distinct().OrderBy(es => es);
            foreach (var camera in cameras)
            {
                cameraNames.Add(camera);
            }

            backgroundWorker.DoWork += BackgroundWorker_FindNearestNeighbours;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.WorkerReportsProgress = true;

            fTest.Text = "True Negatives:\t0\n" +
                "False Negatives:\t0\n" +
                "False Positives:\t0\n" +
                "True Positives:\t0\n" +
                "Sensitivity:\t0\n" +
                "Precision:\t0\n" +
                "F:\t\t0\n";
        }
        #endregion

        #region Event Handler
        private void cameraList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cameraList.SelectedItem != null)
            {
                string selectedCamera = cameraList.SelectedItem as String;

                double total = dataContext.Images.Where(i => i.Day.Value && i.Place == selectedCamera).Count();
                double snow = dataContext.Images.Where(i => i.Day.Value && i.Place == selectedCamera && i.Snow.Value).Count();

                ratio.Value = 100d / total * snow;
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (combinationMethodList.SelectedItem != null && cameraList.SelectedItem != null &&
                numberOfImages.Value.HasValue && numberOfNeighbours.Value.HasValue)
            {
                string selectedCombinationMethod = combinationMethodList.SelectedItem as String;
                string selectedCamera = cameraList.SelectedItem as String;

                // polygone laden
                polygons = dataContext.Polygons.Where(p => p.CameraName == selectedCamera).ToList();
                // statistiken laden
                combinedStatistics = from cs in dataContext.Combined_Statistics
                                     where cs.Polygon.CameraName == selectedCamera
                                     where cs.CombinationMethod == selectedCombinationMethod
                                     select cs;

                // n bilder mit schnee und m bilder ohne schnee laden
                int numberOfImagesTotal = numberOfImages.Value.Value;
                int numberOfImagesWithoutSnow = (int)((double)numberOfImagesTotal * (1d - (double)ratio.Value / 100d));
                int numberOfImagesWithSnow = numberOfImagesTotal - numberOfImagesWithoutSnow;

                // Zufällige Bilder ohne Schnee auswählen
                var dbImagesWithoutSnow = Shuffle(dataContext.Images
                    .Where(i => i.Day.Value && i.Place == selectedCamera && i.NoSnow.Value)
                    .ToList())
                    .Take(numberOfImagesWithoutSnow);
                // Zufällige Bilder mit Schnee auswählen
                var dbImagesWithSnow = Shuffle(dataContext.Images
                    .Where(i => i.Day.Value && i.Place == selectedCamera && i.Snow.Value)
                    .ToList())
                    .Take(numberOfImagesWithSnow);

                // beide Listen kombinieren und shuffeln
                // var selectedImages = Shuffle(dbImagesWithoutSnow.Concat(dbImagesWithSnow).ToList());

                // Beide Listen kombinieren und Bilder anzeigen
                foreach (var image in dbImagesWithSnow.Concat(dbImagesWithoutSnow))
                {
                    classificationViewModels.Add(new ClassificationViewModel(image));
                }
            }
        }

        private void Classify_Click(object sender, RoutedEventArgs e)
        {
            backgroundWorker.RunWorkerAsync(numberOfNeighbours.Value.Value);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            classificationViewModels.Clear();
        }
        #endregion

        #region Methods
        private List<T> Shuffle<T>(List<T> list)
        {
            int current = list.Count();
            while (current > 1)
            {
                current--;
                int other = random.Next(current + 1);
                T otherObject = list[other];
                list[other] = list[current];
                list[current] = otherObject;
            }
            return list;
        }

        private void BackgroundWorker_FindNearestNeighbours(object sender, DoWorkEventArgs e)
        {
            foreach (var classificationViewModel in classificationViewModels)
            {
                FindNearestNeighbours(classificationViewModel, (int)e.Argument);
            }
        }

        private void FindNearestNeighbours(ClassificationViewModel classificationViewModel, int numberOfNearestNeighbours)
        {
            var olderImage = dataContext.Images
                .Where(i => i.Place == classificationViewModel.Image.Place && i.UnixTime < classificationViewModel.Image.UnixTime)
                .OrderByDescending(i => i.UnixTime)
                .FirstOrDefault();

            var newerImage = dataContext.Images
                .Where(i => i.Place == classificationViewModel.Image.Place && i.UnixTime > classificationViewModel.Image.UnixTime)
                .OrderBy(i => i.UnixTime)
                .FirstOrDefault();

            // Statistiken für Patches des Bildes berechnen
            var imageStatistics = new Dictionary<Polygon, List<Statistic>>();
            foreach (var polygon in polygons)
            {
                IEnumerable<Point> polygonPoints = PolygonHelper.DeserializePointCollection(polygon.PolygonPointCollection);
                Statistic statistic = openCVHelper.GetStatisticForPatchFromImagePath(classificationViewModel.Image.FileName, polygonPoints);
                Statistic olderStatistic = openCVHelper.GetStatisticForPatchFromImagePath(olderImage.FileName, polygonPoints);
                Statistic newerStatistic = openCVHelper.GetStatisticForPatchFromImagePath(newerImage.FileName, polygonPoints);

                double distanceToOlder = statistic.DistanceTo(olderStatistic);
                double distanceToNewer = statistic.DistanceTo(newerStatistic);
                double distanceBetweenSurrounding = olderStatistic.DistanceTo(newerStatistic);

                if (distanceToOlder < 50 && distanceToNewer < 50 && distanceBetweenSurrounding < 50)
                {
                    imageStatistics.Add(polygon, (new Statistic[] { statistic, olderStatistic, newerStatistic }).ToList());
                }
                else if (distanceToOlder < distanceToNewer && distanceToOlder < distanceBetweenSurrounding)
                {
                    imageStatistics.Add(polygon, (new Statistic[] { statistic, olderStatistic }).ToList());
                }
                else if (distanceToNewer < distanceToOlder && distanceToNewer < distanceBetweenSurrounding)
                {
                    imageStatistics.Add(polygon, (new Statistic[] { statistic, newerStatistic }).ToList());
                }
                else if (distanceBetweenSurrounding < distanceToNewer && distanceBetweenSurrounding < distanceToOlder)
                {
                    imageStatistics.Add(polygon, (new Statistic[] { olderStatistic, newerStatistic }).ToList());
                }
            }

            int snow         = 0;
            int noSnow       = 0;
            int badLighting  = 0;
            int goodLighting = 0;
            int foggy        = 0;
            int rainy        = 0;
            int goodWeather  = 0;

            foreach (var polygon in polygons)
            {
                var combinedStatisticsForPolygon = this.combinedStatistics.Where(cs => cs.Polygon_ID == polygon.ID);
                Dictionary<Combined_Statistic, double> distances = new Dictionary<Combined_Statistic, double>();

                foreach (var sourceStatistics in imageStatistics[polygon])
                {
                    foreach (var combinedStatistics in combinedStatisticsForPolygon)
                    {
                        distances.Add(combinedStatistics, sourceStatistics.DistanceTo(combinedStatistics.Statistic));
                    }

                    var nearestNeighbours = distances.OrderBy(d => d.Value).Take(numberOfNearestNeighbours);

                    snow += nearestNeighbours.Where(nn => nn.Key.Snow.Value).Count();
                    noSnow += nearestNeighbours.Where(nn => !nn.Key.Snow.Value).Count();
                    badLighting += nearestNeighbours.Where(nn => nn.Key.BadLighting.Value).Count();
                    goodLighting += nearestNeighbours.Where(nn => !nn.Key.BadLighting.Value).Count();
                    foggy += nearestNeighbours.Where(nn => nn.Key.Foggy.Value).Count();
                    rainy += nearestNeighbours.Where(nn => nn.Key.Rainy.Value).Count();
                    goodWeather += nearestNeighbours.Where(nn => !nn.Key.Foggy.Value && !nn.Key.Rainy.Value).Count();

                    distances.Clear();
                }
            }

            // resultate speichern
            classificationViewModel.SetResults(
                snow * 1.5 > noSnow,
                foggy > goodWeather,
                rainy > goodWeather,
                badLighting > goodLighting);

            backgroundWorker.ReportProgress(0, null);
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var classifiedImages = classificationViewModels.Where(i => i.HasResults);

            double trueNegatives = classifiedImages.Where(i => i.TrueNegative).Count();
            double falseNegatives = classifiedImages.Where(i => i.FalseNegative).Count();
            double falsePositives = classifiedImages.Where(i => i.FalsePositive).Count();
            double truePositives = classifiedImages.Where(i => i.TruePositive).Count();
            double sensitivity = truePositives / (truePositives + falseNegatives);
            double precision = truePositives / (truePositives + falsePositives);
            double f = 2 * (precision * sensitivity) / (precision + sensitivity);

            fTest.Text = $"True Negatives:\t{trueNegatives}\n" +
                $"False Negatives:\t{falseNegatives}\n" +
                $"False Positives:\t{falsePositives}\n" +
                $"True Positives:\t{truePositives}\n" +
                $"Sensitivity:\t{sensitivity.ToString("0.00")}\n" +
                $"Precision:\t{precision.ToString("0.00")}\n" +
                $"F:\t\t{f.ToString("0.00")}\n";
        }
        #endregion
    }
}
