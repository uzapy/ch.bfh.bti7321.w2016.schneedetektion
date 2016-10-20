using System;
using System.Windows;
using System.Windows.Input;

namespace Schneedetektion.ImagePlayground
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            galleryView.SendImage += GalleryView_SendImage;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            recordCategories.HandleKey(e);
        }

        private void GalleryView_SendImage(object sender, SendImageEventArgs e)
        {
            histogramViewer.ShowImage(e.SelectedImage, e.Panel);
        }
    }
}
