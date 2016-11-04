using Schneedetektion.OpenCV;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Schneedetektion.ImagePlayground
{
    public partial class HistogramViewer : UserControl
    {
        #region Fields
        private OpenCVHelper openCVHelper = new OpenCVHelper();
        private ImageViewModel imageViewModel1;
        private ImageViewModel imageViewModel2;
        private static Thickness thickness = new Thickness(2);
        private List<float[]> histogram1;
        private List<float[]> histogram2;
        #endregion

        #region Constructor
        public HistogramViewer()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        public void ShowImage(ImageViewModel selectedImage, EPanel panel)
        {
            if (panel == EPanel.HistogramLeft)
            {
                imageViewModel1 = selectedImage;
                image1.Source = imageViewModel1.Bitmap;
                histogram1 = openCVHelper.GetHistogram(imageViewModel1.FileName);

                canvas1.Children.Clear();
                for (int i = 0; i < histogram1[0].Length; i++)
                {
                    canvas1.Children.Add(GetLine(histogram1[0][i], Brushes.Blue, i));
                    canvas1.Children.Add(GetLine(histogram1[1][i], Brushes.Green, i));
                    canvas1.Children.Add(GetLine(histogram1[2][i], Brushes.Red, i));
                }
            }
            else if (panel == EPanel.HistogramRight)
            {
                imageViewModel2 = selectedImage;
                image2.Source = imageViewModel2.Bitmap;
                histogram2 = openCVHelper.GetHistogram(imageViewModel2.FileName);

                canvas2.Children.Clear();
                for (int i = 0; i < histogram2[0].Length; i++)
                {
                    canvas2.Children.Add(GetLine(histogram2[0][i], Brushes.Blue, i));
                    canvas2.Children.Add(GetLine(histogram2[1][i], Brushes.Green, i));
                    canvas2.Children.Add(GetLine(histogram2[2][i], Brushes.Red, i));
                }
            }
        }

        private Line GetLine(float value, SolidColorBrush brush, int position)
        {
            Line l = new Line();
            l.Margin = thickness;
            l.StrokeThickness = 3;
            l.Stroke = brush;
            l.X1 = position * 3 + 100;
            l.Y1 = 333;
            l.X2 = position * 3 + 100;
            l.Y2 = 333d - (333d / 3000d * value);
            l.Opacity = .55;
            return l;
        }
        #endregion
    }
}
