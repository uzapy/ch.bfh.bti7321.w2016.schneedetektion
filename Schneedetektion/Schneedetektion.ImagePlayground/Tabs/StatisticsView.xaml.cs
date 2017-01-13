using Schneedetektion.Data;
using Schneedetektion.OpenCV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Schneedetektion.ImagePlayground
{
    public partial class StatisticsView : UserControl
    {
        #region Fields
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private Random random = new Random();
        private PolygonHelper polygonHelper;
        private OpenCVHelper openCVHelper = new OpenCVHelper();
        private ImageViewModel imageViewModel;
        private ObservableCollection<PatchViewModel> patches = new ObservableCollection<PatchViewModel>();
        private IEnumerable<Polygon> polygons;
        #endregion

        #region Constructor
        public StatisticsView()
        {
            InitializeComponent();
            polygonHelper = new PolygonHelper(polygonCanvas);
            imageContainer.ItemsSource = patches;
        }
        #endregion

        #region Methods
        internal void ShowImage(ImageViewModel selectedImage)
        {
            imageViewModel = selectedImage;
            statisticsImage.Source = imageViewModel.Bitmap;
            imageContainer.ItemsSource = patches;

            polygonCanvas.Children.Clear();
            polygons = dataContext.Polygons.Where(p => p.CameraName == imageViewModel.Image.Place);
            foreach (Polygon dbPolygon in polygons)
            {
                polygonHelper.LoadPolygon(dbPolygon.PolygonPointCollection, dbPolygon.ImageArea, 352, 288);
            }
        }
        #endregion

        #region Event Handler
        private void LoadRandom_Click(object sender, RoutedEventArgs e)
        {
            int count = dataContext.Images.Where(i => i.Day.Value).Count();
            ShowImage(new ImageViewModel(dataContext.Images.Where(i => i.Day.Value).Skip(random.Next(0, count)).First()));
        }

        private void CropPatches_Click(object sender, RoutedEventArgs e)
        {
            // Statistiken für das gesamte Bild berechnen
            Statistic completeImageStatistic = openCVHelper.GetStatisticForImage(imageViewModel.Image.FileName);
            // Statistiken und Bild verpacken und darstellen
            PatchViewModel completeImagePatchViewModel = new PatchViewModel(completeImageStatistic, imageViewModel);
            patches.Add(completeImagePatchViewModel);

            // Für jedes Segment des Bildes
            foreach (var polygon in polygons)
            {
                // Polygon des Segments auslesen
                IEnumerable<Point> pointCollection = PolygonHelper.DeserializePointCollection(polygon.PolygonPointCollection);
                // Bild ausschneiden und Statistiken für das Patch berechnen
                Statistic patchStatistic = openCVHelper.GetStatisticForPatchFromImagePath(imageViewModel.Image.FileName, pointCollection);
                // Patch-Bild generieren
                BitmapImage patchImage = openCVHelper.GetPatchBitmapImage(imageViewModel.Image.FileName, pointCollection);
                // Patch-Statistiken und Patch-Bild verpacken und darstellen
                PatchViewModel patchViewModel = new PatchViewModel(patchStatistic, patchImage, imageViewModel, polygon);
                patches.Add(patchViewModel);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            patches.Clear();
        }
        #endregion
    }
}
