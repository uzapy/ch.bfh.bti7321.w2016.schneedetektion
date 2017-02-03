using Newtonsoft.Json;
using Schneedetektion.Data;
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
            cameraList.ItemsSource = cameraNames;
            imageContainer.ItemsSource = classificationViewModels;

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
                // Gewählte Kamera aus der Liste auslesen
                string selectedCamera = cameraList.SelectedItem as String;

                // Anzahl aller Bilder dieser Kamera am Tag auslesen
                double total = dataContext.Images.Where(i => i.Day.Value && i.Place == selectedCamera).Count();
                // Anzahl aller Schneebilder dieser Kamera am Tag auslesen
                double snow = dataContext.Images.Where(i => i.Day.Value && i.Place == selectedCamera && i.Snow.Value).Count();

                // Verhältnis zwische Schneebildern und schneefreien Bildern
                // ausrechenen und dem Lsider zuweisen
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

            // Neue Kollektion für statisitsche Werte initialisieren
            var imageStatistics = new Dictionary<Polygon, List<Statistic>>();
            // Für jedes Patch des Bildes
            foreach (var polygon in polygons)
            {
                IEnumerable<Point> polygonPoints = PolygonHelper.DeserializePointCollection(polygon.PolygonPointCollection);
                // Statisitsche Werte für das aktuelle Bild Berechnen
                Statistic statistic = openCVHelper.GetStatisticForPatchFromImagePath(classificationViewModel.Image.FileName, polygonPoints);
                if (olderImage != null && newerImage != null)
                {
                    // Statisitsche Werte für das ältere Bild berechnen
                    Statistic olderStatistic = openCVHelper.GetStatisticForPatchFromImagePath(olderImage.FileName, polygonPoints);
                    // Statisitsche Werte für das jüngere Bild berechnen
                    Statistic newerStatistic = openCVHelper.GetStatisticForPatchFromImagePath(newerImage.FileName, polygonPoints);

                    // Distanz zwischen aktuellem und älterem Patch berechnen
                    double distanceToOlder = statistic.DistanceTo(olderStatistic);
                    // Distanz zwischen aktuellem und jüngerem Patch berechnen
                    double distanceToNewer = statistic.DistanceTo(newerStatistic);
                    // Distanz zwischen älterem und jüngerem Patch berechnen
                    double distanceBetweenSurrounding = olderStatistic.DistanceTo(newerStatistic);

                    // Falls die DIstanzen zwischen den drei beobachteten Patches klein ist,
                    // werden alle berücksichtigt für die Klassifikation
                    if (distanceToOlder < 50 && distanceToNewer < 50 && distanceBetweenSurrounding < 50)
                    {
                        imageStatistics.Add(polygon, (new Statistic[] { statistic, olderStatistic, newerStatistic }).ToList());
                    }
                    // Das jüngere Bild ist der Ausreisser und wird verworfen
                    else if (distanceToOlder < distanceToNewer && distanceToOlder < distanceBetweenSurrounding)
                    {
                        imageStatistics.Add(polygon, (new Statistic[] { statistic, olderStatistic }).ToList());
                    }
                    // Das ältere Bild ist der Ausreisser und wird verworfen
                    else if (distanceToNewer < distanceToOlder && distanceToNewer < distanceBetweenSurrounding)
                    {
                        imageStatistics.Add(polygon, (new Statistic[] { statistic, newerStatistic }).ToList());
                    }
                    // Das aktuelle Bild ist der Ausreisser und wird verworfen
                    else if (distanceBetweenSurrounding < distanceToNewer && distanceBetweenSurrounding < distanceToOlder)
                    {
                        imageStatistics.Add(polygon, (new Statistic[] { olderStatistic, newerStatistic }).ToList());
                    }
                }
            }

            int snow = 0;
            int noSnow = 0;
            int badLighting = 0;
            int goodLighting = 0;
            int foggy = 0;
            int rainy = 0;
            int goodWeather = 0;

            // Für jedes Patch der Kamera
            foreach (var polygon in polygons)
            {
                // Alle Kombinierte Statistiken des Pes Polygons laden
                var combinedStatisticsForPolygon = this.combinedStatistics.Where(cs => cs.Polygon_ID == polygon.ID);

                // Nur Bilder aus dem aktuellen Timeslot wählen
                //from cs in combinedStatistics
                //where cs.Polygon_ID == polygon.ID
                //where cs.StartTime <= classificationViewModel.Image.DateTime.Hour
                //where cs.EndTime > classificationViewModel.Image.DateTime.Hour
                //select cs;


                // Leere Dictionary erstellen, die einen kombinierten statistischen Wert mit der
                // Distanz zu den statistischen Werten des aktuell betrachteten Patches verbindet
                Dictionary<Combined_Statistic, double> distances = new Dictionary<Combined_Statistic, double>();

                // Für alle 
                foreach (var sourceStatistics in imageStatistics[polygon])
                {
                    // Für jede kombinierte Statistik
                    foreach (var combinedStatistic in combinedStatisticsForPolygon)
                    {
                        if (!combinedStatistic.Snow.Value && JsonConvert.DeserializeObject<IEnumerable<int>>(combinedStatistic.Images).Count() < 5)
                        {
                            continue;
                        }

                        // Distanz zu den statistischen Werten des aktuell betrachteten Patches berechnen
                        // und Kollektion ablegen
                        distances.Add(combinedStatistic, sourceStatistics.DistanceTo(combinedStatistic.Statistic));
                    }

                    // Kollektion nach Distanz sortieren
                    // Kollektion auf die erstren k Elemente reduzieren (take)
                    var nearestNeighbours = distances.OrderBy(d => d.Value).Take(numberOfNearestNeighbours);

                    // Anzahl Schnee-Bilder unter nächsten Nachbaren bestimmen
                    snow += nearestNeighbours.Where(nn => nn.Key.Snow.Value).Count();
                    // Anzahl Nicht-Schnee-Bilder unter nächsten Nachbaren Bestimmen
                    noSnow += nearestNeighbours.Where(nn => !nn.Key.Snow.Value).Count();
                    // Anzahl Bilder mit schlechten Lichtverhältnissen unter nächsten Nachbaren Bestimmen
                    badLighting += nearestNeighbours.Where(nn => nn.Key.BadLighting.Value).Count();
                    // Anzahl Bilder mit guten Lichtverhältnissen unter nächsten Nachbaren Bestimmen
                    goodLighting += nearestNeighbours.Where(nn => !nn.Key.BadLighting.Value).Count();
                    // Anzahl Bilder mit Nebel unter nächsten Nachbaren Bestimmen
                    foggy += nearestNeighbours.Where(nn => nn.Key.Foggy.Value).Count();
                    // Anzahl Bilder mit Regen unter nächsten Nachbaren Bestimmen
                    rainy += nearestNeighbours.Where(nn => nn.Key.Rainy.Value).Count();
                    // Anzahl Bilder mit gutem Wetter unter nächsten Nachbaren Bestimmen
                    goodWeather += nearestNeighbours.Where(nn => !nn.Key.Foggy.Value && !nn.Key.Rainy.Value).Count();

                    distances.Clear();
                }
            }

            // resultate speichern
            classificationViewModel.SetResults(
                // Falls es mehr nächste Nachbaren gibt die, die Kategorie 'Schnee' haben als 'kein Schnee',
                // Wird die Kategorie 'Schnee' gesetzt.
                snow * 1.25 > noSnow,
                // Mehr nächste Nachbaren mit Nebel als mit gutem Wetter?
                foggy > goodWeather,
                // Mehr nächste Nachbaren mit Regen als mit gutem Wetter?
                rainy > goodWeather,
                // Mehr nächste Nachbaren mit schlechten Lichtverhältnissen als mit guten?
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
