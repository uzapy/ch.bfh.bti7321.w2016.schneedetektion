using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Schneedetektion.ImagePlayground
{
    public class HistogramViewModel
    {
        private List<double> blue;
        private List<double> green;
        private List<double> red;
        private List<Line> histogramValues = new List<Line>();
        private static Thickness thickness = new Thickness(2);

        public HistogramViewModel(List<double> blueHistogram, List<double> greenHistogram, List<double> redHistogram, bool normalizeHistogram = false)
        {
            blue = blueHistogram;
            green = greenHistogram;
            red = redHistogram;

            if (normalizeHistogram)
            {
                double maxIntensity = (new double[3] { blue.Max(), green.Max(), red.Max() }).Max();
                for (int i = 0; i < blue.Count; i++)
                {
                    blue[i] = blue[i] / maxIntensity * 100;
                    green[i] = green[i] / maxIntensity * 100;
                    red[i] = red[i] / maxIntensity * 100;
                }
            }

            for (int i = 0; i < blue.Count(); i++)
            {
                histogramValues.Add(new Line()
                {
                    Margin = thickness,
                    StrokeThickness = 1,
                    Stroke = Brushes.Blue,
                    Opacity = .5,
                    X1 = i * 1,
                    Y1 = 100,
                    X2 = i * 1,
                    Y2 = 100 - blue[i]
                });
                histogramValues.Add(new Line()
                {
                    Margin = thickness,
                    StrokeThickness = 1,
                    Stroke = Brushes.Green,
                    Opacity = .5,
                    X1 = i * 1,
                    Y1 = 100,
                    X2 = i * 1,
                    Y2 = 100 - green[i]
                });
                histogramValues.Add(new Line()
                {
                    Margin = thickness,
                    StrokeThickness = 1,
                    Stroke = Brushes.Red,
                    Opacity = .5,
                    X1 = i * 1,
                    Y1 = 100,
                    X2 = i * 1,
                    Y2 = 100 - red[i]
                });
            }
        }

        public List<Line> HistogramValues { get { return histogramValues; } }
    }
}
