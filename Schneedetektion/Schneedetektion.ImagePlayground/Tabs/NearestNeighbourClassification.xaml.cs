using Schneedetektion.Data;
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
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private ObservableCollection<string> cameraNames = new ObservableCollection<string>();
        private ObservableCollection<ImageViewModel> images = new ObservableCollection<ImageViewModel>();
        private List<Combined_Statistic> combinedStatistics = new List<Combined_Statistic>();
        private Random random = new Random();

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cameraList.SelectedItem != null && numberOfImages.Value.HasValue && numberOfNeighbours.Value.HasValue)
            {
                string selectedCamera = cameraList.SelectedItem as String;

                combinedStatistics = dataContext.Combined_Statistics.Where(cs => cs.Polygon.CameraName == selectedCamera).ToList();

                // Zufällige Bilder auswählen
                var dbImages = dataContext.Images.Where(i => i.Day.Value && i.Place == selectedCamera).ToList();
                var selectedImages = Shuffle(dbImages).Take(numberOfImages.Value.Value);

                foreach (var image in selectedImages)
                {
                    images.Add(new ImageViewModel(image));

                    //double distance = 0;
                    LinkedList<double> topNDistances = new LinkedList<double>();
                    LinkedList<Combined_Statistic> topN = new LinkedList<Combined_Statistic>();

                    foreach (var combindStatistic in combinedStatistics)
                    {
                    }
                }
            }
        }

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
    }
}
