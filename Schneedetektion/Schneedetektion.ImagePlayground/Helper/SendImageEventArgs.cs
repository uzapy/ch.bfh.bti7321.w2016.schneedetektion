using System;

namespace Schneedetektion.ImagePlayground
{
    public class SendImageEventArgs : EventArgs
    {
        private ImageViewModel selectedImage;
        private EPanel panel;

        public SendImageEventArgs(ImageViewModel selectedImage, EPanel panel)
        {
            this.selectedImage = selectedImage;
            this.panel = panel;
        }

        public ImageViewModel SelectedImage { get { return selectedImage; } }
        public EPanel Panel { get { return panel; } }
    }
}