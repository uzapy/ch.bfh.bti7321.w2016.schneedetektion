using Schneedetektion.Data;
using Schneedetektion.OpenCV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<string> cameraNames = new ObservableCollection<string>();
        private ObservableCollection<ClassificationViewModel> images = new ObservableCollection<ClassificationViewModel>();
        private List<Polygon> polygons = new List<Polygon>();
        private IQueryable<Combined_Statistic> combinedStatistics;
        private Random random = new Random();
        #endregion

        #region Constructor
        public NearestNeighbourClassification()
        {
            InitializeComponent();

            cameraList.ItemsSource = cameraNames;
            imageContainer.ItemsSource = images;

            IEnumerable<string> cameras = dataContext.Combined_Statistics.Select(cs => cs.Polygon.CameraName).Distinct();
            foreach (var camera in cameras)
            {
                cameraNames.Add(camera);
            }
        }
        #endregion

        #region Event Handler
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (cameraList.SelectedItem != null && numberOfImages.Value.HasValue && numberOfNeighbours.Value.HasValue)
            {
                string selectedCamera = cameraList.SelectedItem as String;

                // polygone laden
                polygons = dataContext.Polygons.Where(p => p.CameraName == selectedCamera).ToList();
                // statistiken laden
                combinedStatistics = dataContext.Combined_Statistics.Where(cs => cs.Polygon.CameraName == selectedCamera);

                // Zufällige Bilder auswählen
                var dbImages = dataContext.Images.Where(i => i.Day.Value && i.Place == selectedCamera).ToList();
                var selectedImages = Shuffle(dbImages).Take(numberOfImages.Value.Value);

                // Bilder anzeigen
                foreach (var image in selectedImages)
                {
                    images.Add(new ClassificationViewModel(image));
                }
            }
        }

        private void Classify_Click(object sender, RoutedEventArgs e)
        {
            foreach (var image in images)
            {
                FindNearestNeighbours(image);
            }
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

        private void FindNearestNeighbours(ClassificationViewModel classificationViewModel)
        {
            // Statistiken für Patches des Bilder berechnen
            Dictionary<Polygon, Statistic> imageStatistics = new Dictionary<Polygon, Statistic>();
            foreach (var polygon in polygons)
            {
                imageStatistics.Add(polygon,
                    openCVHelper.GetStatisticForPatchFromImagePath(classificationViewModel.FileName, PolygonHelper.DeserializePointCollection(polygon.PolygonPointCollection)));
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
            List<NearestNeighbour> nearestNeighbours = neighbours.OrderBy(n => n.Distance).Take(numberOfNeighbours.Value.Value).ToList();

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
        }
        #endregion
    }
}
