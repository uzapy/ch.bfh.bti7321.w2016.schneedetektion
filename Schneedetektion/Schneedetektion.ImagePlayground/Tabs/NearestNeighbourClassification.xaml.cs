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
    public partial class NearestNeighbourClassification : UserControl
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
        public NearestNeighbourClassification()
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

            IEnumerable<string> cameras = dataContext.Combined_Statistics.Select(cs => cs.Polygon.CameraName).Distinct();
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
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (combinationMethodList.SelectedItem != null &&
                cameraList.SelectedItem != null &&
                numberOfImages.Value.HasValue &&
                numberOfNeighbours.Value.HasValue)
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

                int numberOfImagesTotal = numberOfImages.Value.Value;
                int numberOfImagesWithoutSnow = (int)((double)numberOfImagesTotal * (1d - (double)ratio.Value / 100d));
                int numberOfImagesWithSnow = numberOfImagesTotal - numberOfImagesWithoutSnow;

                // Zufällige Bilder ohne Schnee auswählen
                var dbImagesWithoutSnow = dataContext.Images
                    .Where(i => i.Day.Value && i.Place == selectedCamera && i.NoSnow.Value)
                    .Take(numberOfImagesWithoutSnow);
                // Zufällige Bilder mit Schnee auswählen
                var dbImagesWithSnow = dataContext.Images
                    .Where(i => i.Day.Value && i.Place == selectedCamera && i.Snow.Value)
                    .Take(numberOfImagesWithSnow);

                // beide Listen kombinieren und shuffeln
                var selectedImages = Shuffle(dbImagesWithoutSnow.Concat(dbImagesWithSnow).ToList());

                // Bilder anzeigen
                foreach (var image in selectedImages)
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
            // Statistiken für Patches des Bilder berechnen
            Dictionary<Polygon, Statistic> imageStatistics = new Dictionary<Polygon, Statistic>();
            foreach (var polygon in polygons)
            {
                imageStatistics.Add(polygon,
                    openCVHelper.GetStatisticForPatchFromImagePath(
                        classificationViewModel.FileName,
                        PolygonHelper.DeserializePointCollection(polygon.PolygonPointCollection)));
            }

            // kombinierte statistiken nach Bild-Gruppen gruppieren
            var groupedStatistics = combinedStatistics.GroupBy(cs => cs.Images);

            List<NearestNeighbour> neighbours = new List<NearestNeighbour>();
            // Pro Gruppe Nearest-Neighbor erstellen
            foreach (var group in groupedStatistics)
            {
                NearestNeighbour neighbour = new NearestNeighbour();
                neighbour.Polygons = polygons;
                neighbour.Image = classificationViewModel.Image;
                // berechnete Statistiken vom aktuellen bild
                neighbour.ImageStatistics = imageStatistics;

                // vorberechnete Statistiken
                foreach (var polygon in polygons)
                {
                    neighbour.NeighbourStatistics.Add(polygon, group.Where(g => g.Polygon_ID.Value == polygon.ID).Single());
                }

                // Distanzen berechnen
                neighbour.CalculateDistances();
                neighbours.Add(neighbour);
            }

            // nachbaren nach distanz ordnen - nächste nachbaren auswählen
            List<NearestNeighbour> nearestNeighbours = neighbours.OrderBy(n => n.Distance).Take(numberOfNearestNeighbours).ToList();

            // eigenschaften der nächsten nachbaren für die klassifizierung nutzen
            IEnumerable<Combined_Statistic> nearestCombinedStatistics = nearestNeighbours.SelectMany(nn => nn.NeighbourStatistics.Values);
            int snow = nearestCombinedStatistics.Where(ns => ns.Snow.Value).Count();
            int noSnow = nearestCombinedStatistics.Where(ns => !ns.Snow.Value).Count();
            int badLighting = nearestCombinedStatistics.Where(ns => ns.BadLighting.Value).Count();
            int goodLighting = nearestCombinedStatistics.Where(ns => !ns.BadLighting.Value).Count();
            int foggy = nearestCombinedStatistics.Where(ns => ns.Foggy.Value).Count();
            int rainy = nearestCombinedStatistics.Where(ns => ns.Rainy.Value).Count();
            int goodWeather = nearestCombinedStatistics.Where(ns => !ns.Foggy.Value && !ns.Rainy.Value).Count();

            // resultate speichern
            classificationViewModel.SetResults(
                snow > noSnow,
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
