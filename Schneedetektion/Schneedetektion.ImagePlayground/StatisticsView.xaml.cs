using Schneedetektion.Data;
using Schneedetektion.OpenCV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Schneedetektion.ImagePlayground
{
    public partial class StatisticsView : UserControl
    {
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private Random random = new Random();
        private PolygonHelper polygonHelper;
        private OpenCVHelper openCVHelper = new OpenCVHelper();
        private ImageViewModel imageViewModel;
        private IEnumerable<Polygon> polygons;

        public StatisticsView()
        {
            InitializeComponent();

            polygonHelper = new PolygonHelper(polygonCanvas);
        }

        internal void ShowImage(ImageViewModel selectedImage)
        {
            imageViewModel = selectedImage;
            statisticsImage.Source = imageViewModel.Bitmap;

            polygonCanvas.Children.Clear();
            polygons = dataContext.Polygons.Where(p => p.CameraName == imageViewModel.Image.Place);
            foreach (Polygon dbPolygon in polygons)
            {
                polygonHelper.LoadPolygon(dbPolygon.PolygonPointCollection, dbPolygon.ImageArea, 500, 409);
            }
        }

        private void LoadRandom_Click(object sender, RoutedEventArgs e)
        {
            int count = dataContext.Images.Where(i => i.Day.Value).Count();
            ShowImage(new ImageViewModel(dataContext.Images.Where(i => i.Day.Value).Skip(random.Next(0, count)).First()));
        }

        private void CropPatches_Click(object sender, RoutedEventArgs e)
        {
            imageContainer.Items.Clear();
            foreach (var polygon in polygons)
            {
                IEnumerable<Point> pointCollection = polygonHelper.GetPointCollection(polygon.PolygonPointCollection);
                imageContainer.Items.Add(openCVHelper.GetMaskedImage(imageViewModel.FileName, pointCollection));
            }
        }
    }
}
