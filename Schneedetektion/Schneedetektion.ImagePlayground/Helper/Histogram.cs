using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Schneedetektion.ImagePlayground
{
    public class Histogram
    {
        private List<Line> histogramValues = new List<Line>();
        private static Thickness thickness = new Thickness(2);

        public Histogram(IEnumerable<float[]> values)
        {
            for (int i = 1; i < values.ElementAt(0).Count(); i++)
            {
                histogramValues.Add(new Line()
                {
                    Margin = thickness,
                    StrokeThickness = 1,
                    Stroke = Brushes.Blue,
                    Opacity = .55,
                    X1 = i * 1,
                    Y1 = 100,
                    X2 = i * 1,
                    Y2 = 100 - values.ElementAt(0)[i]
                });
                histogramValues.Add(new Line()
                {
                    Margin = thickness,
                    StrokeThickness = 1,
                    Stroke = Brushes.Green,
                    Opacity = .55,
                    X1 = i * 1,
                    Y1 = 100,
                    X2 = i * 1,
                    Y2 = 100 - values.ElementAt(1)[i]
                });
                histogramValues.Add(new Line()
                {
                    Margin = thickness,
                    StrokeThickness = 1,
                    Stroke = Brushes.Red,
                    Opacity = .55,
                    X1 = i * 1,
                    Y1 = 100,
                    X2 = i * 1,
                    Y2 = 100 - values.ElementAt(2)[i]
                });
            }
        }

        public List<Line> HistogramValues { get { return histogramValues; } }
    }
}
