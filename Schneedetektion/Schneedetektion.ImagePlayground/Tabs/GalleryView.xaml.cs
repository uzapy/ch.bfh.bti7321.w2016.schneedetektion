using Schneedetektion.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Schneedetektion.ImagePlayground
{
    public partial class GalleryView : UserControl
    {
        #region Fields
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();

        private ObservableCollection<string> cameraNames = new ObservableCollection<string>()
        {
            "all", "mvk021", "mvk022", "mvk101", "mvk102", "mvk105", "mvk106", "mvk107", "mvk108", "mvk110", "mvk120", "mvk122", "mvk131"
        };
        private ObservableCollection<string> categoryNames = new ObservableCollection<string>()
        {
            "all", "Snow", "No Snow", "Night", "Dusk", "Day", "Foggy", "Cloudy", "Precipitation", "Bad Lighting", "Good Lighting"
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

        private int skip = 0;
        private int page = 130;
        #endregion

        #region Constructor
        public GalleryView()
        {
            InitializeComponent();

            cameraList.ItemsSource = cameraNames;
            categoryList.ItemsSource = categoryNames;

            imageContainer.ItemsSource = images;
            foreach (var i in dataContext.Images.Take(page))
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
            skip = 0;
            ReloadImages();
        }

        private void nextPage_Click(object sender, RoutedEventArgs e)
        {
            skip += page;
            ReloadImages();
        }

        private void previousPage_Click(object sender, RoutedEventArgs e)
        {
            if (skip > 0)
            {
                skip -= page;
                ReloadImages();
            }
        }

        private void imageContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (imageContainer.SelectedValue != null)
            {
                selectedImage = (ImageViewModel)imageContainer.SelectedValue;
            }
        }

        private void imageContainer_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            menuItemSnow.IsChecked         = selectedImage.Image.Snow.Value;
            menuItemNoSnow.IsChecked       = selectedImage.Image.NoSnow.Value;
            menuItemNight.IsChecked        = selectedImage.Image.Night.Value;
            menuItemDusk.IsChecked         = selectedImage.Image.Dusk.Value;
            menuItemDay.IsChecked          = selectedImage.Image.Day.Value;
            menuItemFoggy.IsChecked        = selectedImage.Image.Foggy.Value;
            menuItemCloudy.IsChecked       = selectedImage.Image.Cloudy.Value;
            menuItemCloudy.IsChecked       = selectedImage.Image.Cloudy.Value;
            menuItemRainy.IsChecked        = selectedImage.Image.Rainy.Value;
            menuItemBadLighting.IsChecked  = selectedImage.Image.BadLighting.Value;
            menuItemGoodLighting.IsChecked = selectedImage.Image.GoodLighting.Value;
        }

        private void ShowFileInFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", $"/select, {selectedImage.Image.FileName}");
        }

        private void ShowImageTimeLapse_Click(object sender, RoutedEventArgs e)
        {
            if (SendImage != null)
            {
                this.SendImage(this, new SendImageEventArgs(selectedImage, EPanel.TimeLapse));
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

        private void SelectForStatisticsTool_Click(object sender, RoutedEventArgs e)
        {
            if (SendImage != null)
            {
                this.SendImage(this, new SendImageEventArgs(selectedImage, EPanel.Statistics));
            }
        }

        private void SelectForStatisticsFromDB_Click(object sender, RoutedEventArgs e)
        {
            if (SendImage != null)
            {
                this.SendImage(this, new SendImageEventArgs(selectedImage, EPanel.StatisticsFromDB));
            }
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((MenuItem)sender).CommandParameter;
            bool isChecked = (bool)((MenuItem)sender).IsChecked;

            foreach (ImageViewModel selectedImage in imageContainer.SelectedItems)
            {
                switch (tag)
                {
                    case "Snow":
                    case "NoSnow":
                        selectedImage.Image.Snow = !selectedImage.Image.Snow;
                        selectedImage.Image.NoSnow = !selectedImage.Image.NoSnow;
                        break;

                    case "Night":
                        if (isChecked)
                        {
                            selectedImage.Image.Night = isChecked;
                            selectedImage.Image.Dusk = !isChecked;
                            selectedImage.Image.Day = !isChecked;
                        }
                        break;
                    case "Dusk":
                        if (isChecked)
                        {
                            selectedImage.Image.Night = !isChecked;
                            selectedImage.Image.Dusk = isChecked;
                            selectedImage.Image.Day = !isChecked;
                        }
                        break;
                    case "Day":
                        if (isChecked)
                        {
                            selectedImage.Image.Night = !isChecked;
                            selectedImage.Image.Dusk = !isChecked;
                            selectedImage.Image.Day = isChecked;
                        }
                        break;

                    case "Foggy":
                        selectedImage.Image.Foggy = !selectedImage.Image.Foggy;
                        break;
                    case "Cloudy":
                        selectedImage.Image.Cloudy = !selectedImage.Image.Cloudy;
                        break;
                    case "Rainy":
                        selectedImage.Image.Rainy = !selectedImage.Image.Rainy;
                        break;

                    case "BadLighting":
                    case "GoodLighting":
                        selectedImage.Image.BadLighting = !selectedImage.Image.BadLighting;
                        selectedImage.Image.GoodLighting = !selectedImage.Image.GoodLighting;
                        break;

                    default:
                        break;
                } 
            }

            dataContext.SubmitChanges();
            ReloadImages();
        }
        #endregion

        #region Helper Methods
        private void ReloadImages()
        {
            images.Clear();

            int minuteSpan = 15;
            DateTime exactTime = new DateTime(year, month, day, hour, minute, 0);

            // "all", "Snow", "No Snow", "Night", "Dusk", "Day", "Foggy", "Cloudy", "Precipitation", "Bad Lighting", "Good Lighting"
            bool allCategories = selectedCategories.Contains("all") || selectedCategories.Count() >= 9;
            bool snow          = selectedCategories.Contains("Snow");
            bool noSnow        = selectedCategories.Contains("No Snow");
            bool night         = selectedCategories.Contains("Night");
            bool dusk          = selectedCategories.Contains("Dusk");
            bool dayTime       = selectedCategories.Contains("Day");
            bool foggy         = selectedCategories.Contains("Foggy");
            bool cloudy        = selectedCategories.Contains("Cloudy");
            bool precipitation = selectedCategories.Contains("Precipitation");
            bool badLighting   = selectedCategories.Contains("Bad Lighting");
            bool goodLighting  = selectedCategories.Contains("Good Lighting");

            var loadedImages = from i in dataContext.Images select i;

            if (!selectedCameras.Contains("all"))
            {
                loadedImages = from i in loadedImages
                               where selectedCameras.Contains(i.Place)
                               select i;
            }

            if (hasDate)
            {
                loadedImages = from i in loadedImages
                               where i.DateTime.Year == year
                               where i.DateTime.Month == month
                               where i.DateTime.Day == day
                               select i;
            }

            if (hasTime)
            {
                loadedImages = from i in loadedImages
                               where i.DateTime.Hour == hour
                               where Math.Abs(i.DateTime.Minute - minute) < minuteSpan
                               select i;
            }

            if (!allCategories && snow)
            {
                loadedImages = loadedImages.Where(i => i.Snow == snow);
            }
            else if (!allCategories && noSnow)
            {
                loadedImages = loadedImages.Where(i => i.NoSnow == noSnow);
            }

            if (!allCategories && night)
            {
                loadedImages = loadedImages.Where(i => i.Night == night);
            }

            if (!allCategories && dusk)
            {
                loadedImages = loadedImages.Where(i => i.Dusk == dusk);
            }

            if (!allCategories && dayTime)
            {
                loadedImages = loadedImages.Where(i => i.Day == dayTime);
            }

            if (!allCategories && foggy)
            {
                loadedImages = loadedImages.Where(i => i.Foggy == foggy);
            }

            if (!allCategories && cloudy)
            {
                loadedImages = loadedImages.Where(i => i.Cloudy == cloudy);
            }

            if (!allCategories && precipitation)
            {
                loadedImages = loadedImages.Where(i => i.Rainy == precipitation);
            }

            if (!allCategories && badLighting)
            {
                loadedImages = loadedImages.Where(i => i.BadLighting == badLighting);
            }
            else if (!allCategories && goodLighting)
            {
                loadedImages = loadedImages.Where(i => i.GoodLighting == goodLighting);
            }

            //var loadedImages = (from i in dataContext.Images
            //                    where i.DateTime.Year == year || !hasDate
            //                    where i.DateTime.Month == month || !hasDate
            //                    where i.DateTime.Day == day || !hasDate
            //                    where i.DateTime.Hour == hour || !hasTime
            //                    where Math.Abs(i.DateTime.Minute - minute) < minuteSpan || !hasTime
            //                    where i.Snow == snow || allCategorie
            //                    where i.NoSnow == noSnow || allCategorie
            //                    where i.Night == night || allCategorie
            //                    where i.Dusk == dusk || allCategorie
            //                    where i.Day == dayTime || allCategorie
            //                    where i.Foggy == foggy || allCategorie
            //                    where i.Cloudy == cloudy || allCategorie
            //                    where i.Rainy == precipitation || allCategorie
            //                    where i.BadLighting == badLighting || allCategorie
            //                    where i.GoodLighting == goodLighting || allCategorie
            //                    where selectedCameras.Contains(i.Place) || selectedCameras.Contains("all")
            //                    select i).Distinct().Skip(skip).Take(page);

            foreach (var i in loadedImages.Skip(skip).Take(page))
            {
                images.Add(new ImageViewModel(i));
            }
        }
        #endregion
    }
}
