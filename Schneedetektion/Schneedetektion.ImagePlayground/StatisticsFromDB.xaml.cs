using Schneedetektion.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Schneedetektion.ImagePlayground
{
    public partial class StatisticsFromDB : UserControl
    {
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private PolygonHelper polygonHelper;
        private ImageViewModel imageViewModel;
        private ObservableCollection<PatchViewModel> patches = new ObservableCollection<PatchViewModel>();
        private IEnumerable<Polygon> polygons;

        public StatisticsFromDB()
        {
            InitializeComponent();
            polygonHelper = new PolygonHelper(polygonCanvas);
            imageContainer.ItemsSource = patches;
        }

        internal void ShowImage(ImageViewModel selectedImage)
        {
            imageViewModel = selectedImage;
            statisticsImage.Source = imageViewModel.Bitmap;

            polygonCanvas.Children.Clear();
            polygons = dataContext.Polygons.Where(p => p.CameraName == imageViewModel.Image.Place);
            foreach (Polygon dbPolygon in polygons)
            {
                polygonHelper.LoadPolygon(dbPolygon.PolygonPointCollection, dbPolygon.ImageArea, 352, 288);
            }

            IEnumerable<Statistic> statistics = imageViewModel.Image.Entity_Statistics.Select(s => s.Statistic);

            if (statistics.Count() > 0)
            {
                Statistic statistic = statistics.First();
                PatchViewModel completeImagePatchViewModel = new PatchViewModel(statistic, imageViewModel);
                patches.Add(completeImagePatchViewModel);
            }
        }
    }
}
