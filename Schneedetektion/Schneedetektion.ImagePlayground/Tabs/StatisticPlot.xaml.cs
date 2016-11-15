using Schneedetektion.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Schneedetektion.ImagePlayground
{
    public partial class StatisticPlot : UserControl
    {
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private ObservableCollection<string> cameraNames = new ObservableCollection<string>();
        private ObservableCollection<string> polygonNames = new ObservableCollection<string>();
        private List<object> initialElements = new List<object>();
        private string selectedCamera = String.Empty;
        private string selectedPolygon = String.Empty;
        private string selectedX = String.Empty;
        private string selectedY = String.Empty;
        private string selectedColor = String.Empty;

        public StatisticPlot()
        {
            InitializeComponent();

            cameraList.ItemsSource = cameraNames;
            polygonList.ItemsSource = polygonNames;

            IEnumerable<string> cameras = dataContext.Entity_Statistics.Where(es => es.Polygon != null).Select(es => es.Image.Place).Distinct();
            foreach (var camera in cameras)
            {
                cameraNames.Add(camera);
            }

            foreach (var child in plotCanvas.Children)
            {
                initialElements.Add(child);
            }
        }

        private void cameraList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cameraList.SelectedItem != null)
            {
                selectedCamera = cameraList.SelectedItem as String;

                polygonNames.Clear();
                IEnumerable<string> polygons = dataContext.Polygons.Where(p => p.CameraName == selectedCamera).Select(p => p.ID + "-" + p.ImageArea);
                foreach (var polygon in polygons)
                {
                    polygonNames.Add(polygon);
                }
            }
        }

        private void polygonList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (polygonList.SelectedItem != null)
            {
                selectedPolygon = polygonList.SelectedItem as String;
            }
        }

        private void comboX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboX.SelectedValue != null)
            {
                selectedX = (comboX.SelectedValue as ComboBoxItem).Content as String;
            }
        }

        private void comboY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboY.SelectedValue != null)
            {
                selectedY = (comboY.SelectedValue as ComboBoxItem).Content as String;
            }
        }

        private void comboColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboColor.SelectedValue != null)
            {
                selectedColor = (comboColor.SelectedValue as ComboBoxItem).Content as String;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClearCanvas();

            int polygonID = 0;

            if (!String.IsNullOrEmpty(selectedCamera) &&
                !String.IsNullOrEmpty(selectedPolygon) && Int32.TryParse(selectedPolygon.Split('-')[0], out polygonID) &&
                !String.IsNullOrEmpty(selectedX) &&
                !String.IsNullOrEmpty(selectedX) &&
                !String.IsNullOrEmpty(selectedColor))
            {
                var statisticsWithSnow = from es in dataContext.Entity_Statistics
                                         where es.Image.Place == selectedCamera
                                         where es.Image.Snow == true
                                         where es.Polygon.ID == polygonID
                                         select es.Statistic;

                DrawPoints(statisticsWithSnow, selectedX, selectedY, selectedColor, Brushes.Blue);

                var statisticsWithoutSnow = from es in dataContext.Entity_Statistics
                                            where es.Image.Place == selectedCamera
                                            where es.Image.Snow == false
                                            where es.Polygon.ID == polygonID
                                            select es.Statistic;

                DrawPoints(statisticsWithoutSnow, selectedX, selectedY, selectedColor, Brushes.Green);
            }
        }

        private void ClearCanvas()
        {
            plotCanvas.Children.Clear();
            foreach (var element in initialElements)
            {
                plotCanvas.Children.Add(element as UIElement);
            }
        }

        private void plotCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            xLine.X1 = 0;
            xLine.Y1 = 0;
            xLine.X2 = ((Canvas)e.OriginalSource).ActualWidth;
            xLine.Y2 = 0;

            yLine.X1 = 0;
            yLine.Y1 = 0;
            yLine.X2 = 0;
            yLine.Y2 = ((Canvas)e.OriginalSource).ActualHeight;
        }

        private void DrawPoints(IEnumerable<Statistic> statistics, string selectedX, string selectedY, string selectedColor, Brush brush)
        {
            foreach (var statistic in statistics)
            {
                double left = ScaleToCanvas(statistic.Get(selectedX, selectedColor), selectedX, plotCanvas.ActualWidth) - 5;
                double top = (plotCanvas.ActualHeight - 5) - ScaleToCanvas(statistic.Get(selectedY, selectedColor), selectedY, plotCanvas.ActualHeight);

                plotCanvas.Children.Add(
                    new Ellipse()
                    {
                        Width = 5,
                        Height = 5,
                        Fill = brush,
                        Opacity = 0.3,
                        Margin = new Thickness(left, top, 0, 0)
                    });
            }
        }

        private double ScaleToCanvas(double value, string property, double scale)
        {
            // Mode / Mean / Median / Minimum / Maximum => 0 - 255
            // StandardDeviation                        => 0 - 127
            // Variance                                 => 0 - 16129
            // Contrast                                 => 0 - 1

            double max = 1;
            if (property == "Mode" || property == "Mean" || property == "Median" || property == "Minimum" || property == "Maximum")
            {
                max = 255;
            }
            else if (property == "StandardDeviation")
            {
                max = 128;
            }
            else if (property == "Variance")
            {
                max = 16384;
            }
            else if (property == "Contrast")
            {
                max = 1;
            }

            return value / max * scale;
        }
    }
}
