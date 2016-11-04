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
        private Polygon currentPolygon;
        private Canvas polygonCanvas;

        private int selectedAreaIndex = 0;
        private string[] imageAreas = new string[] { "Lane", "Emergency Lane", "Median", "Marking", "Grass", "Tree", "Background", "Sky" };
        private Brush[] fillBrushes = { Brushes.Blue, Brushes.Red, Brushes.Yellow, Brushes.Brown, Brushes.Violet, Brushes.Orange, Brushes.Magenta, Brushes.Gold };
        private Brush[] strokeBrushes = { Brushes.LightBlue, Brushes.OrangeRed, Brushes.Khaki, Brushes.Sienna, Brushes.Pink, Brushes.SandyBrown, Brushes.Orchid, Brushes.Wheat };
        #endregion

        #region Constructor
        public PolygonHelper() { }
        public PolygonHelper(Canvas polygonCanvas)
        {
            this.polygonCanvas = polygonCanvas;
        }
        #endregion

        #region Properties
        public string[] ImageAreas { get { return imageAreas; } }
        public Polygon CurrentPolygon { get { return currentPolygon; } }
        #endregion

        #region Methods
        internal void SetSelectedArea(int areaIndex)
        {
            if (currentPolygon != null)
            {
                selectedAreaIndex = areaIndex;
                currentPolygon.Stroke = fillBrushes[selectedAreaIndex];
                currentPolygon.Fill = strokeBrushes[selectedAreaIndex];
            }
        }

        internal void LoadPolygon(string polygonPointCollection, string imageArea, double viewWidth, double viewHeight)
        {
            Polygon polygon = new Polygon();
            polygon.Stroke = fillBrushes[Array.IndexOf(imageAreas, imageArea)];
            polygon.Fill = strokeBrushes[Array.IndexOf(imageAreas, imageArea)];
            polygon.Opacity = 0.33d;
            polygonCanvas?.Children.Add(polygon);
            foreach (Point point in JsonConvert.DeserializeObject<PointCollection>(polygonPointCollection))
            {
                polygon.Points.Add(new Point(point.X * viewWidth, point.Y * viewHeight));
            }
        }

        internal void NewPolygon(int areaIndex)
        {
            polygonCanvas?.Children.Remove(currentPolygon);
            selectedAreaIndex = areaIndex;

            currentPolygon = new Polygon();
            currentPolygon.Stroke = fillBrushes[selectedAreaIndex];
            currentPolygon.Fill = strokeBrushes[selectedAreaIndex];
            currentPolygon.Opacity = 0.33d;
            polygonCanvas?.Children.Add(currentPolygon);
        }

        internal void SetPoint(Point point)
        {
            if (currentPolygon != null)
            {
                currentPolygon.Points.Add(point);
            }
        }

        internal void DeleteLastPoint()
        {
            if (currentPolygon != null && currentPolygon.Points.Count > 0)
            {
                currentPolygon.Points.RemoveAt(currentPolygon.Points.Count - 1);
            }
        }
        #endregion

        #region Static Methods
        public static IEnumerable<Point> DeserializePointCollection(string polygonPointCollection)
        {
            foreach (Point point in JsonConvert.DeserializeObject<PointCollection>(polygonPointCollection))
            {
                yield return new Point(point.X, point.Y);
            }
        }

        public static string SerializePointCollection(Polygon polygon, double viewWidth, double viewHeight)
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