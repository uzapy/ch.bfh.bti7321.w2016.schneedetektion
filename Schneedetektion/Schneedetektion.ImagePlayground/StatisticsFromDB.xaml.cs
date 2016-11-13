using Schneedetektion.Data;
using Schneedetektion.OpenCV;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Schneedetektion.ImagePlayground
{
    public partial class StatisticsFromDB : UserControl
    {
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private OpenCVHelper openCVHelper = new OpenCVHelper();
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

            foreach (var es in imageViewModel.Image.Entity_Statistics)
            {
                if (es.Polygon == null)
                {
                    PatchViewModel completeImagePatchViewModel = new PatchViewModel(es.Statistic, imageViewModel);
                    patches.Add(completeImagePatchViewModel);
                }
                else
                {
                    BitmapImage patchImage = openCVHelper.GetPatchBitmapImage(imageViewModel.FileName,
                        PolygonHelper.DeserializePointCollection(es.Polygon.PolygonPointCollection));
                    PatchViewModel patchViewModel = new PatchViewModel(es.Statistic, patchImage, imageViewModel, es.Polygon);
                    patches.Add(patchViewModel);
                }
            }
        }

        private void Clear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            patches.Clear();
        }
    }
}
