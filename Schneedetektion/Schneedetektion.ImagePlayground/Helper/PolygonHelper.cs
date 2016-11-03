using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Schneedetektion.ImagePlayground
{
    public class PolygonHelper
    {
        #region Fields
        private Polygon polygon;
        private Canvas polygonCanvas;

        private int selectedAreaIndex = 0;
        private string[] imageAreas = new string[] { "Lane", "Emergency Lane", "Median", "Marking", "Grass", "Tree", "Background", "Sky" };
        private Brush[] fillBrushes = { Brushes.Blue, Brushes.Red, Brushes.Yellow, Brushes.Brown, Brushes.Violet, Brushes.Orange, Brushes.Magenta, Brushes.Gold };
        private Brush[] strokeBrushes = { Brushes.LightBlue, Brushes.OrangeRed, Brushes.Khaki, Brushes.Sienna, Brushes.Pink, Brushes.SandyBrown, Brushes.Orchid, Brushes.Wheat };
        #endregion

        #region Constructor
        public PolygonHelper(Canvas polygonCanvas)
        {
            this.polygonCanvas = polygonCanvas;
        }
        #endregion

        #region Properties
        public string[] ImageAreas { get { return imageAreas; } }
        #endregion

        #region Methods
        internal void SetSelectedArea(int areaIndex)
        {
            if (polygon != null)
            {
                selectedAreaIndex = areaIndex;
                polygon.Stroke = fillBrushes[selectedAreaIndex];
                polygon.Fill = strokeBrushes[selectedAreaIndex];
            }
        }

        internal void LoadPolygon(string polygonPointCollection, string imageArea, double viewWidth, double viewHeight)
        {
            Polygon polygon = new Polygon();
            polygon.Stroke = fillBrushes[Array.IndexOf(imageAreas, imageArea)];
            polygon.Fill = strokeBrushes[Array.IndexOf(imageAreas, imageArea)];
            polygon.Opacity = 0.33d;
            polygonCanvas.Children.Add(polygon);
            foreach (Point point in JsonConvert.DeserializeObject<PointCollection>(polygonPointCollection))
            {
                polygon.Points.Add(new Point(point.X * viewWidth, point.Y * viewHeight));
            }
        }

        internal IEnumerable<Point> GetPointCollection(string polygonPointCollection)
        {
            foreach (Point point in JsonConvert.DeserializeObject<PointCollection>(polygonPointCollection))
            {
                yield return new Point(point.X, point.Y);
            }
        }

        internal void NewPolygon(int areaIndex)
        {
            polygonCanvas.Children.Remove(polygon);
            selectedAreaIndex = areaIndex;

            polygon = new Polygon();
            polygon.Stroke = fillBrushes[selectedAreaIndex];
            polygon.Fill = strokeBrushes[selectedAreaIndex];
            polygon.Opacity = 0.33d;
            polygonCanvas.Children.Add(polygon);
        }

        internal void SetPoint(Point point)
        {
            if (polygon != null)
            {
                polygon.Points.Add(point);
            }
        }

        internal void DeleteLastPoint()
        {
            if (polygon != null && polygon.Points.Count > 0)
            {
                polygon.Points.RemoveAt(polygon.Points.Count - 1);
            }
        }

        internal string GetPointCollection(double viewWidth, double viewHeight)
        {
            PointCollection pointCollection = new PointCollection();
            foreach (Point point in polygon.Points)
            {
                pointCollection.Add(new Point(1 / viewWidth * point.X, 1 / viewHeight * point.Y));
            }
            return JsonConvert.SerializeObject(pointCollection);
        }
        #endregion
    }
}