using Schneedetektion.Data;
using Schneedetektion.ImagePlayground.Properties;
using Schneedetektion.OpenCV;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Schneedetektion.ImagePlayground
{
    public partial class MaskingView : UserControl
    {
        #region Fields
        public static string folderName = Settings.Default.WorkingFolder;
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private PolygonHelper polygonHelper;
        private ImageViewModel imageViewModel;
        #endregion

        #region Constructor
        public MaskingView()
        {
            InitializeComponent();

            polygonHelper = new PolygonHelper(polygonCanvas);
            selectedArea.ItemsSource = polygonHelper.ImageAreas;
            selectedArea.SelectedItem = selectedArea.Items[0];
        }

        #endregion

        #region Event Handler
        private void selectedArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            polygonHelper.SetSelectedArea(selectedArea.SelectedIndex);
        }

        private void newPolygon_Click(object sender, RoutedEventArgs e)
        {
            polygonHelper.NewPolygon(selectedArea.SelectedIndex);
        }

        private void savePolygon_Click(object sender, RoutedEventArgs e)
        {
            Polygon p = new Polygon();
            p.CameraName = imageViewModel.Image.Place;
            p.ImageArea = polygonHelper.ImageAreas[selectedArea.SelectedIndex];
            p.ImageWidth = maskToolImage.ActualWidth;
            p.ImageHeight = maskToolImage.ActualHeight;
            p.PolygonPointCollection = PolygonHelper.SerializePointCollection(polygonHelper.CurrentPolygon, maskToolImage.ActualWidth, maskToolImage.ActualHeight);
            dataContext.Polygons.InsertOnSubmit(p);
            dataContext.SubmitChanges();
        }

        private void deletePoint_Click(object sender, RoutedEventArgs e)
        {
            polygonHelper.DeleteLastPoint();
        }

        private void maskToolImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            imageWidth.Text = "W: " + maskToolImage.ActualWidth.ToString("0.00");
            imageHeight.Text = "H: " + maskToolImage.ActualHeight.ToString("0.00");
            LoadSavedPolygons();
        }

        private void polygonCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            pointerXPosition.Text = "X: " + e.GetPosition(maskToolImage).X.ToString("0:00");
            pointerYPosition.Text = "Y: " + e.GetPosition(maskToolImage).Y.ToString("0:00");
        }

        private void polygonCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            polygonHelper.SetPoint(e.GetPosition(maskToolImage));
        }

        private void reloadPolygons_Click(object sender, RoutedEventArgs e)
        {
            ShowImage(imageViewModel);
        }

        private void createBitmasks_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Polygon> polygons = dataContext.Polygons.Where(p => p.CameraName == imageViewModel.Image.Place);
            OpenCVHelper openCVHelper = new OpenCVHelper();
            foreach (var polygon in polygons)
            {
                string bitmaskFileName = Path.Combine(folderName, "bitmasks", polygon.CameraName + "_" + polygon.ID + ".png");
                openCVHelper.SaveBitmask(imageViewModel.FileName, bitmaskFileName,
                    PolygonHelper.DeserializePointCollection(polygon.PolygonPointCollection));
                polygon.Bitmask = bitmaskFileName;
                dataContext.SubmitChanges();
            }
        }
        #endregion

        #region Methods
        public void ShowImage(ImageViewModel selectedImage)
        {
            imageViewModel = selectedImage;
            maskToolImage.Source = selectedImage.Bitmap;
            imageName.Text = "Picture: " + selectedImage.Image.Name;
            LoadSavedPolygons();
        }

        private void LoadSavedPolygons()
        {
            polygonCanvas.Children.Clear();
            var dbPolygons = dataContext.Polygons.Where(p => p.CameraName == imageViewModel.Image.Place);
            foreach (Polygon dbPolygon in dbPolygons)
            {
                polygonHelper.LoadPolygon(dbPolygon.PolygonPointCollection, dbPolygon.ImageArea, maskToolImage.ActualWidth, maskToolImage.ActualHeight);
            }
        }
        #endregion
    }
}
