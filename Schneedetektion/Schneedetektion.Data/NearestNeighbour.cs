using System.Collections.Generic;
using System.Linq;

namespace Schneedetektion.Data
{
    public class NearestNeighbour
    {
        #region Fields
        private Image image;
        private List<Polygon> polygons = new List<Polygon>();
        private Dictionary<Polygon, Statistic> imageStatistics = new Dictionary<Polygon, Statistic>();
        private Dictionary<Polygon, Combined_Statistic> neighborStatistics = new Dictionary<Polygon, Combined_Statistic>();
        private Dictionary<Polygon, double> distances = new Dictionary<Polygon, double>();
        #endregion

        #region Properties
        public List<Polygon> Polygons
        {
            get { return polygons; }
            set { polygons = value; }
        }

        public Image Image
        {
            get { return image; }
            set { image = value; }
        }

        public Dictionary<Polygon, Statistic> ImageStatistics
        {
            get { return imageStatistics; }
            set { imageStatistics = value; }
        }

        public Dictionary<Polygon, Combined_Statistic> NeighbourStatistics
        {
            get { return neighborStatistics; }
            set { neighborStatistics = value; }
        }

        public Dictionary<Polygon, double> Distances
        {
            get { return distances; }
            set { distances = value; }
        }

        public double Distance
        {
            get
            {
                return distances.Values.Sum();
            }
        } 
        #endregion

        public void CalculateDistances()
        {
            foreach (var polygon in polygons)
            {
                Distances.Add(polygon, imageStatistics[polygon].DistanceTo(NeighbourStatistics[polygon].Statistic));
            }
        }
    }
}
