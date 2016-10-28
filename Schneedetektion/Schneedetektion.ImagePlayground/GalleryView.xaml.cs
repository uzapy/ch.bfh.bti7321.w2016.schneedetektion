using Schneedetektion.Data;
using Schneedetektion.ImagePlayground.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System;

namespace Schneedetektion.ImagePlayground
{
    public partial class GalleryView : UserControl
    {
        #region Fields
        public static string folderName = Settings.Default.WorkingFolder;
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();

        private ObservableCollection<string> cameraNames = new ObservableCollection<string>()
        {
            "all", "mvk021", "mvk022", "mvk101", "mvk102", "mvk105", "mvk106", "mvk107", "mvk108", "mvk110", "mvk120", "mvk122", "mvk131"
        };
        private ObservableCollection<string> categoryNames = new ObservableCollection<string>()
        {
            "all", "snow", "no snow", "night", "dusk", "day", "fog", "precipitation", "bad light", "good light"
        };
        private ObservableCollection<ImageViewModel> images = new ObservableCollection<ImageViewModel>();

        private List<string> selectedCameras = new List<string>() { "all" };
        private List<string> selectedCategories = new List<string>() { "all" };
        private ImageViewModel selectedImage;

        private int year = 2014;
        private int month = 12;
        private int day = 2;
        private bool hasDate = false;
        private int hour = 0;
        private int minute = 0;
        private bool hasTime = false;
        #endregion

        #region Constructor
        public GalleryView()
        {
            InitializeComponent();

            cameraList.ItemsSource = cameraNames;
            categoryList.ItemsSource = categoryNames;

            imageContainer.ItemsSource = images;
            foreach (var i in dataContext.Images.Take(265))
            {
                images.Add(new ImageViewModel(i));
            }
        }
        #endregion

        #region Properties
        public event EventHandler<SendImageEventArgs> SendImage;
        #endregion

        #region Event Handler
        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            hasDate = datePicker.SelectedDate.HasValue;

            if (datePicker.SelectedDate.HasValue)
            {
                year = datePicker.SelectedDate.Value.Year;
                month = datePicker.SelectedDate.Value.Month;
                day = datePicker.SelectedDate.Value.Day;
            }
        }

        private void timePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            hasTime = timePicker.Value.HasValue;

            if (timePicker.Value.HasValue)
            {
                hour = timePicker.Value.Value.Hour;
                minute = timePicker.Value.Value.Minute;
            }
        }

        private void cameraList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCameras.Clear();

            if (cameraList.SelectedItems.Count == 0 || cameraList.SelectedItems.OfType<string>().Contains("all"))
            {
                selectedCameras.Add("all");
            }
            else
            {
                selectedCameras = cameraList.SelectedItems.OfType<string>().ToList();
            }
        }

        private void categoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCategories.Clear();

            if (categoryList.SelectedItems.Count == 0 || categoryList.SelectedItems.OfType<string>().Contains("all"))
            {
                selectedCategories.Add("all");
            }
            else
            {
                selectedCategories = categoryList.SelectedItems.OfType<string>().ToList();
            }
        }

        private void applyFilter_Click(object sender, RoutedEventArgs e)
        {
            ReloadImages();
        }

        private void imageContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (imageContainer.SelectedIndex >= 0)
            {
                if (selectedImage?.Image.ID != images[imageContainer.SelectedIndex].Image.ID)
                {
                    selectedImage = images[imageContainer.SelectedIndex];
                }
            }
        }

        private void ShowImageHistogram1_Click(object sender, RoutedEventArgs e)
        {
            if (SendImage != null)
            {
                this.SendImage(this, new SendImageEventArgs(selectedImage, EPanel.HistogramLeft));
            }
        }

        private void ShowImageHistogram2_Click(object sender, RoutedEventArgs e)
        {
            if (SendImage != null)
            {
                this.SendImage(this, new SendImageEventArgs(selectedImage, EPanel.HistogramRight));
            }
        }

        private void SelectForMaskTool_Click(object sender, RoutedEventArgs e)
        {
            if (SendImage != null)
            {
                this.SendImage(this, new SendImageEventArgs(selectedImage, EPanel.MaskTool));
            }
        }
        #endregion

        #region Helper Methods
        private void ReloadImages()
        {
            images.Clear();

            int minuteSpan = 6;
            DateTime exactTime = new DateTime(year, month, day, hour, minute, 0);

            // "all", "snow", "no snow", "night", "dusk", "day", "fog", "precipitation", "bad light", "good light"
            bool all           = selectedCategories.Contains("all") || selectedCategories.Count() >= 9;
            bool snow          = selectedCategories.Contains("snow");
            bool noSnow        = selectedCategories.Contains("no snow");
            bool night         = selectedCategories.Contains("night");
            bool dusk          = selectedCategories.Contains("dusk");
            bool dayTime       = selectedCategories.Contains("day");
            bool fog           = selectedCategories.Contains("fog");
            bool precipitation = selectedCategories.Contains("precipitation");
            bool badLight      = selectedCategories.Contains("bad light");
            bool goodLight     = selectedCategories.Contains("good light");

            var loadedImages = (from i in dataContext.Images
                                where i.DateTime.Year == year || !hasDate
                                where i.DateTime.Month == month || !hasDate
                                where i.DateTime.Day == day || !hasDate
                                where i.DateTime.Hour == hour || !hasTime
                                where Math.Abs(i.DateTime.Minute - minute) < minuteSpan || !hasTime
                                where i.Snow == snow || all
                                where i.NoSnow == noSnow || all
                                where i.Night == night || all
                                where i.Dusk == dusk || all
                                where i.Day == dayTime || all
                                where i.Foggy == fog || all
                                where i.Rainy == precipitation || all
                                where i.BadLighting == badLight || all
                                where i.GoodLighting == goodLight || all
                                where selectedCameras.Contains(i.Place) || selectedCameras.Contains("all")
                                select i).Distinct().Take(512);

            foreach (var i in loadedImages)
            {
                images.Add(new ImageViewModel(i));
            }
        }
        #endregion
    }
}
