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
            switch (e.Panel)
            {
                case EPanel.HistogramLeft:
                case EPanel.HistogramRight:
                    histogramViewer.ShowImage(e.SelectedImage, e.Panel);
                    histogramTab.IsSelected = true;
                    break;
                case EPanel.MaskTool:
                    maskingTool.ShowImage(e.SelectedImage);
                    maskingToolTab.IsSelected = true;
                    break;
                case EPanel.Statistics:
                    statisticsView.ShowImage(e.SelectedImage);
                    statisticsViewTab.IsSelected = true;
                    break;
            }
        } 
        #endregion
    }
}
