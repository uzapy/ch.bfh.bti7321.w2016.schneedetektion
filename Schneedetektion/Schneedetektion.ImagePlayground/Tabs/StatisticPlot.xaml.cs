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
        #region Fields
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private ObservableCollection<string> cameraNames = new ObservableCollection<string>();
        private ObservableCollection<string> polygonNames = new ObservableCollection<string>();
        private List<object> initialElements = new List<object>();
        private string selectedCamera = String.Empty;
        private string selectedPolygon = String.Empty;
        private string selectedAttribute = String.Empty;
        private string selectedColor = String.Empty;
        private static Thickness thickness = new Thickness(2);
        #endregion

        #region Constructor
        public StatisticPlot()
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
            comboColor_SelectionChanged(this, null);
        }
        #endregion

        #region Event Handler
        private void cameraList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cameraList.SelectedItem != null)
            {
                // Gewählte Kamera auslesen
                selectedCamera = cameraList.SelectedItem as String;

                // Patch-Liste leeren
                polygonNames.Clear();
                // Patches der Kamera auslesen.
                // Aus ID und Bildregion wird der Listenname kombiniert
                IEnumerable<string> polygons = dataContext.Polygons.Where(p => p.CameraName == selectedCamera).Select(p => p.ID + "-" + p.ImageArea);
                // Abfüllen in die Patch-Auswahl
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
                selectedAttribute = (comboX.SelectedValue as ComboBoxItem).Content as String;
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
                !String.IsNullOrEmpty(selectedAttribute) &&
                !String.IsNullOrEmpty(selectedColor))
            {
                // Statistische Werte für die gewählte Kamera und den gewählten Patch mit Schnee auslesen
                var statisticsWithSnow = from es in dataContext.Entity_Statistics
                                         where es.Image.Place == selectedCamera
                                         where es.Image.Snow == true
                                         where es.Polygon.ID == polygonID
                                         select es.Statistic;

                // Balken für Statistiken mit Schnee zeichnen (Blau)
                DrawBars(statisticsWithSnow, selectedAttribute, selectedColor, Brushes.Blue);

                // Statistische Werte für die gewählte Kamera und den gewählten Patch ohne Schnee auslesen
                var statisticsWithoutSnow = from es in dataContext.Entity_Statistics
                                            where es.Image.Place == selectedCamera
                                            where es.Image.Snow == false
                                            where es.Polygon.ID == polygonID
                                            select es.Statistic;

                // Balken für Statistiken ohne Schnee zeichnen (Rot)
                DrawBars(statisticsWithoutSnow, selectedAttribute, selectedColor, Brushes.Red);
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

        private void DrawBars(IEnumerable<Statistic> statistics, string selectedAttribute, string selectedColor, Brush brush)
        {
            // 256 leere Balken: Leere Liste der Länge 256 erstellen
            List<double> bars = new List<double>();
            bars.AddRange(new double[256]);

            // Für alle Statistik-Objekte
            foreach (var statistic in statistics)
            {
                // 1. Wert aus dem Statistik-Objekt auslesen in der gewählten Farbe
                // 2. Werte bei Bedarf skalieren auf den Werteberich [0,255]
                // 3. Auf die nächste Ganzzahl abrunden
                int flooredValue = (int)ScaleToCanvas(statistic.Get(selectedAttribute, selectedColor), selectedAttribute);
                // Den Balken in der Liste um 1 vergrössern,
                // dessen Position in der Liste dem Statistik-Wert entspricht
                bars[flooredValue]++;
            }

            // Die Breite eines Balkens ist abhängig von der aktuellen Breite der Zeichnungsebene.
            double strokeWidth = plotCanvas.ActualWidth / 256;

            // Für alle Balken in der Liste eine Linie in der Zeichnungsebene zeichnen
            for (int i = 0; i < bars.Count; i++)
            {
                // Balken zur Zeichnungsebene hinzufügen
                plotCanvas.Children.Add(new Line()
                {
                    Margin = thickness,
                    // Breite des Balkens
                    StrokeThickness = strokeWidth,
                    // Farbe des Balkens (Blau oder Rot)
                    Stroke = brush,
                    // 50% Transparent
                    Opacity = 0.5,
                    // Horizonale Startpostion des Balkens ist abhängig von der Posiion in der Liste
                    X1 = i * strokeWidth,
                    // Vertikale Startposion: Grundlinie
                    Y1 = plotCanvas.ActualHeight - 3,
                    // Horizonale Endpostion ist die gleiche wie die Startposition
                    X2 = i * strokeWidth,
                    // Vertikale Endposition der Linie: Höhe der Zeichnungsebene minus Balkenwert
                    Y2 = plotCanvas.ActualHeight - bars[i] - 3
                });
            }
        }

        private double ScaleToCanvas(double value, string property)
        {
            // Mode / Mean / Median / Minimum / Maximum => 0 - 255
            // StandardDeviation                        => 0 - 127
            // Variance                                 => 0 - 16129
            // Contrast                                 => 0 - 1

            double max = 1;
            if (property == "Mode" || property == "Mean" || property == "Median" || property == "Minimum" || property == "Maximum")
            {
                return value;
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

            return value / max * 255;
        } 
        #endregion
    }
}
