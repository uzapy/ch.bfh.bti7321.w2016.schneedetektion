using Schneedetektion.ImagePlayground.Properties;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Schneedetektion.ImagePlayground
{
    public class ImageViewModel
    {
        private static string folderName = Settings.Default.WorkingFolder;
        private Data.Image image;
        private BitmapImage bitmap;

        public ImageViewModel(Data.Image i)
        {
            image = i;
        }

        public BitmapImage Bitmap
        {
            get
            {
                if (bitmap == null)
                {
                    try
                    {
                        if (Directory.Exists(folderName))
                        {
                            bitmap = new BitmapImage(new Uri(Path.Combine(folderName, image.Place, image.Name + ".jpg")));
                        }
                        else
                        {
                            bitmap = new BitmapImage();
                        }
                    }
                    catch (Exception)
                    {
                        bitmap = new BitmapImage();
                    }
                }
                return bitmap;
            }
            set
            {
                bitmap = value;
            }
        }

        public Data.Image Image
        {
            get
            {
                return image;
            }
        }
    }
}
