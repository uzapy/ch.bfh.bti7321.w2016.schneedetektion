using Schneedetektion.Data;
using Schneedetektion.OpenCV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Schneedetektion.ImagePlayground
{
    public partial class Statistic2DPlot : UserControl
    {
        #region Fields
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private OpenCVHelper openCVHelper = new OpenCVHelper();
        private ObservableCollection<string> cameraNames = new ObservableCollection<string>();
        private ObservableCollection<string> polygonNames = new ObservableCollection<string>();
        private List<object> initialElements = new List<object>();
        private string selectedCamera = String.Empty;
        private string selectedPolygon = String.Empty;
        private string selectedX = String.Empty;
        private string selectedY = String.Empty;
        private string selectedColorX = String.Empty;
        private string selectedColorY = String.Empty;
        #endregion

        #region Constructor
        public Statistic2DPlot()
        {
            InitializeComponent();

            cameraList.ItemsSource = cameraNames;
            polygonList.ItemsSource = polygonNames;

            IEnumerable<string> cameras = dataContext.Entity_Statistics
                .Where(es => es.Polygon != null)
                .Select(es => es.Image.Place)
                .Distinct()
                .OrderBy(es => es);

            foreach (var camera in cameras)
            {
                cameraNames.Add(camera);
            }

            foreach (var child in plotCanvas.Children)
            {
                initialElements.Add(child);
            }

            comboX_SelectionChanged(this, null);
            comboY_SelectionChanged(this, null);
            comboColorX_SelectionChanged(this, null);
            comboColorY_SelectionChanged(this, null);
        }
        #endregion

        #region Event Handler
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

        private void comboColorX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboColorX.SelectedValue != null)
            {
                selectedColorX = (comboColorX.SelectedValue as ComboBoxItem).Content as String;
            }
        }

        private void comboColorY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboColorY.SelectedValue != null)
            {
                selectedColorY = (comboColorY.SelectedValue as ComboBoxItem).Content as String;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClearCanvas();

            int polygonID = 0;

            if (!String.IsNullOrEmpty(selectedCamera) &&
                !String.IsNullOrEmpty(selectedPolygon) && Int32.TryParse(selectedPolygon.Split('-')[0], out polygonID) &&
                !String.IsNullOrEmpty(selectedX) &&
                !String.IsNullOrEmpty(selectedY) &&
                !String.IsNullOrEmpty(selectedColorX) &&
                !String.IsNullOrEmpty(selectedColorY))
            {
                var statisticsWithSnow = from es in dataContext.Entity_Statistics
                                         where es.Image.Place == selectedCamera
                                         where es.Image.Snow == true
                                         where es.Polygon.ID == polygonID
                                         select es.Statistic;

                DrawPoints(statisticsWithSnow, selectedX, selectedY, selectedColorX, selectedColorY, Brushes.Blue);

                var statisticsWithoutSnow = from es in dataContext.Entity_Statistics
                                            where es.Image.Place == selectedCamera
                                            where es.Image.Snow == false
                                            where es.Polygon.ID == polygonID
                                            select es.Statistic;

                DrawPoints(statisticsWithoutSnow, selectedX, selectedY, selectedColorX, selectedColorY, Brushes.Magenta);
            }
        }
        #endregion

        #region Private Methods
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

        private void DrawPoints(IEnumerable<Statistic> statistics,
            string selectedX, string selectedY, string selectedColorX, string selectedColorY, Brush brush)
        {
            foreach (var statistic in statistics)
            {
                double left = ScaleToCanvas(statistic.Get(selectedX, selectedColorX), selectedX, plotCanvas.ActualWidth - 10) + 5;
                double top = (plotCanvas.ActualHeight - 5) - ScaleToCanvas(statistic.Get(selectedY, selectedColorY), selectedY, plotCanvas.ActualHeight - 5);

                Ellipse e = new Ellipse()
                {
                    Width = 8,
                    Height = 8,
                    Fill = brush,
                    Opacity = 0.5,
                    Margin = new Thickness(left, top, 0, 0),
                    Tag = statistic.ID
                };

                e.MouseLeftButtonUp += new MouseButtonEventHandler(HandleEllipseClick);

                plotCanvas.Children.Add(e);
            }
        }

        private void HandleEllipseClick(object sender, RoutedEventArgs args)
        {
            int statisticID = (int)(sender as Ellipse).Tag;
            int polygonID = Int32.Parse(selectedPolygon.Split('-')[0]);

            Statistic statistic = dataContext.Statistics.Where(s => s.ID == statisticID).Single();
            Entity_Statistic entityStatitistic = statistic.Entity_Statistics.Where(es => es.Polygon.ID == polygonID).Single();
            ImageViewModel imageViewModel = new ImageViewModel(entityStatitistic.Image);

            fullImage.Source = imageViewModel.Bitmap;

            patchImage.Source = openCVHelper.GetPatchBitmapImage(
                imageViewModel.Image.FileName, PolygonHelper.DeserializePointCollection(entityStatitistic.Polygon.PolygonPointCollection));
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
        #endregion
    }
}
