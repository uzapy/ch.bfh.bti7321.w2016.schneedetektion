﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Schneedetektion.ImagePlayground
{
    public class Histogram
    {
        private List<int> blue;
        private List<int> green;
        private List<int> red;
        private List<Line> histogramValues = new List<Line>();
        private static Thickness thickness = new Thickness(2);

        public Histogram(List<int> blueHistogram, List<int> greenHistogram, List<int> redHistogram)
        {
            blue = blueHistogram;
            green = greenHistogram;
            red = redHistogram;

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
        public List<int> Blue { get { return blue; } }
        public List<int> Green { get { return green; } }
        public List<int> Red { get { return red; } }
    }
}
