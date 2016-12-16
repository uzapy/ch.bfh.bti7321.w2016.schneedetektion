﻿using Schneedetektion.Data;
using Schneedetektion.ImagePlayground.Properties;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Schneedetektion.ImagePlayground
{
    public class ImageViewModel
    {
        private static string folderName = Settings.Default.WorkingFolder;
        private Image image;
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
                            bitmap = new BitmapImage(new Uri(image.FileName));
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

        public Image Image { get { return image; } }
    }
}
