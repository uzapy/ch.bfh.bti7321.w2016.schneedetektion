using Schneedetektion.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using System;
using System.Windows;
using System.Windows.Input;

namespace Schneedetektion.ImagePlayground
{
    public partial class TimeLapse : UserControl
    {
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private List<ImageViewModel> images = new List<ImageViewModel>();
        private DispatcherTimer timer = new DispatcherTimer();

        public TimeLapse()
        {
            InitializeComponent();
        }

        internal void ShowImage(ImageViewModel selectedImage)
        {
            var imagesInDay = from i in dataContext.Images
                              where i.Place == selectedImage.Image.Place
                              where i.DateTime.Year == selectedImage.Image.DateTime.Year
                              where i.DateTime.Month == selectedImage.Image.DateTime.Month
                              where i.DateTime.Day == selectedImage.Image.DateTime.Day
                              select i;

            images.Clear();
            foreach (var i in imagesInDay)
            {
                images.Add(new ImageViewModel(i));
            }

            timeLapesImage.Source = images.First().Bitmap;
            slider.Maximum = images.Count - 1;

            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100); // 100 Milisekunden => 10fps
        }

        internal void HandleKey(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (timer.IsEnabled)
                {
                    timer.Stop();
                }
                else
                {
                    timer.Start();
                }
            }
            else if (e.Key == Key.Left)
            {
                slider.Value--;
            }
            else if (e.Key == Key.Right)
            {
                slider.Value++;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            slider.Value++;
            if ((slider.Value + 1) >= images.Count)
            {
                slider.Value = 0;
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timeLapesImage.Source = images[(int)slider.Value].Bitmap;
        }
    }
}
