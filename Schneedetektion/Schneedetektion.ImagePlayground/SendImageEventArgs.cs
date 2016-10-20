using System;

namespace Schneedetektion.ImagePlayground
{
    public class SendImageEventArgs : EventArgs
    {
        private ImageViewModel selectedImage;
        private int panel;

        public SendImageEventArgs(ImageViewModel selectedImage, int panel)
        {
            this.selectedImage = selectedImage;
            this.panel = panel;
        }

        public ImageViewModel SelectedImage { get { return selectedImage; } }
        public int Panel { get { return panel; } }
    }
}