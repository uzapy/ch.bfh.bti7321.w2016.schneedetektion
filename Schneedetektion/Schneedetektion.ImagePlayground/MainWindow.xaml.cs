using System.Windows;
using System.Windows.Input;

namespace Schneedetektion.ImagePlayground
{
    public partial class MainWindow : Window
    {
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            galleryView.SendImage += GalleryView_SendImage;
        }
        #endregion

        #region Event Handler
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (recordCategoriesTab.IsSelected)
            {
                recordCategories.HandleKey(e); 
            }
        }

        private void GalleryView_SendImage(object sender, SendImageEventArgs e)
        {
            if (e.Panel == EPanel.MaskTool)
            {
                maskingTool.ShowImage(e.SelectedImage);
                maskingToolTab.IsSelected = true;
            }
            else
            {
                histogramViewer.ShowImage(e.SelectedImage, e.Panel);
                histogramTab.IsSelected = true;
            }
        } 
        #endregion
    }
}
