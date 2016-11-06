using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Schneedetektion.ImagePlayground
{
    public class Histogram
    {
        private List<float> blue = new List<float>();
        private List<float> green = new List<float>();
        private List<float> red = new List<float>();
        private List<Line> histogramValues = new List<Line>();
        private static Thickness thickness = new Thickness(2);

        public Histogram(IEnumerable<float[]> values)
        {
            blue.AddRange(values.ElementAt(0));
            green.AddRange(values.ElementAt(1));
            red.AddRange(values.ElementAt(2));
            // TODO: Nuller nicht mitnehmen - wir starten bei 1
            blue.RemoveAt(0);
            green.RemoveAt(0);
            red.RemoveAt(0);

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
        public List<float> Blue { get { return blue; } }
        public List<float> Green { get { return green; } }
        public List<float> Red { get { return red; } }
    }
}
