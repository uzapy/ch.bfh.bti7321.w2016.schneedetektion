using Schneedetektion.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Schneedetektion.ImagePlayground
{
    /// <summary>
    /// Interaction logic for RecordCategories.xaml
    /// </summary>
    public partial class RecordCategories : UserControl
    {
        #region Fields
        private bool isNightNext = true;
        private StrassenbilderMetaDataContext dataContext = new StrassenbilderMetaDataContext();
        private ObservableCollection<ImageViewModel> images = new ObservableCollection<ImageViewModel>();
        private int currentImageViewModel = 0;
        #endregion

        #region Constructor
        public RecordCategories()
        {
            InitializeComponent();

            noSnowButton.IsChecked = true;
            dayButton.IsChecked = true;
            goodLightingButton.IsChecked = true;
            
            var imageInfos = dataContext.Images.Where(i => i.Place == "mvk102" && i.Snow == null).OrderBy(i => i.ID);
            foreach (var image in imageInfos)
            {
                images.Add(new ImageViewModel(image));
            }

            if (images.Count > 0)
            {
                currentImage.Source = images[currentImageViewModel].Bitmap;
            }
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        internal void HandleKey(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                SaveCategories();
                ChangeImage(-1);
            }
            else if (e.Key == Key.Right)
            {
                SaveCategories();
                ChangeImage(1);
            }
            else if (e.Key == Key.S || e.Key == Key.D1)
            {
                snowButton.IsChecked = !snowButton.IsChecked;
                noSnowButton.IsChecked = !noSnowButton.IsChecked;
            }
            else if (e.Key == Key.H || e.Key == Key.D2)
            {
                if (nightButton.IsChecked.HasValue && nightButton.IsChecked.Value)
                {
                    nightButton.IsChecked = false;
                    duskButton.IsChecked = true;
                    dayButton.IsChecked = false;
                }
                else if (duskButton.IsChecked.HasValue && duskButton.IsChecked.Value)
                {
                    nightButton.IsChecked = isNightNext;
                    duskButton.IsChecked = false;
                    dayButton.IsChecked = !isNightNext;
                    isNightNext = !isNightNext;
                }
                else if (dayButton.IsChecked.HasValue && dayButton.IsChecked.Value)
                {
                    nightButton.IsChecked = false;
                    duskButton.IsChecked = true;
                    dayButton.IsChecked = false;
                }
            }
            else if (e.Key == Key.W || e.Key == Key.D3)
            {
                if (foggyButton.IsChecked.HasValue && foggyButton.IsChecked.Value)
                {
                    foggyButton.IsChecked = false;
                    cloudyButton.IsChecked = true;
                    rainyButton.IsChecked = false;
                }
                else if (cloudyButton.IsChecked.HasValue && cloudyButton.IsChecked.Value)
                {
                    foggyButton.IsChecked = false;
                    cloudyButton.IsChecked = false;
                    rainyButton.IsChecked = true;
                }
                else if (rainyButton.IsChecked.HasValue && rainyButton.IsChecked.Value)
                {
                    foggyButton.IsChecked = false;
                    cloudyButton.IsChecked = false;
                    rainyButton.IsChecked = false;
                }
                else
                {
                    foggyButton.IsChecked = true;
                    cloudyButton.IsChecked = false;
                    rainyButton.IsChecked = false;
                }
            }
            else if (e.Key == Key.V || e.Key == Key.D4)
            {
                badLightingButton.IsChecked = !badLightingButton.IsChecked;
                goodLightingButton.IsChecked = !goodLightingButton.IsChecked;
            }
        }

        private void SaveCategories()
        {
            images[currentImageViewModel].Image.SaveCategories(
                snowButton.IsChecked,
                noSnowButton.IsChecked,
                nightButton.IsChecked,
                duskButton.IsChecked,
                dayButton.IsChecked,
                foggyButton.IsChecked,
                cloudyButton.IsChecked,
                rainyButton.IsChecked,
                badLightingButton.IsChecked,
                goodLightingButton.IsChecked);
            dataContext.SubmitChanges();
        }

        private void ChangeImage(int direction)
        {
            if (direction < 0 && currentImageViewModel > 0)
            {
                currentImageViewModel--;
            }
            else if (direction > 0 && currentImageViewModel < images.Count - 1)
            {
                currentImageViewModel++;
            }
            if (images.Count > 0)
            {
                currentImage.Source = images[currentImageViewModel].Bitmap;
            }
        }
        #endregion

        #region Event Handler
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCategories();
            ChangeImage(-1);
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCategories();
            ChangeImage(1);
        }

        private void snowButton_Click(object sender, RoutedEventArgs e)
        {
            noSnowButton.IsChecked = false;
        }

        private void noSnowButton_Click(object sender, RoutedEventArgs e)
        {
            snowButton.IsChecked = false;
        }

        private void nightButton_Click(object sender, RoutedEventArgs e)
        {
            duskButton.IsChecked = false;
            dayButton.IsChecked = false;
            isNightNext = false;
        }

        private void duskButton_Click(object sender, RoutedEventArgs e)
        {
            nightButton.IsChecked = false;
            dayButton.IsChecked = false;
        }

        private void dayButton_Click(object sender, RoutedEventArgs e)
        {
            nightButton.IsChecked = false;
            duskButton.IsChecked = false;
            isNightNext = true;
        }

        private void foggyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cloudyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rainyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void badLightingButton_Click(object sender, RoutedEventArgs e)
        {
            goodLightingButton.IsChecked = false;
        }

        private void goodLightingButton_Click(object sender, RoutedEventArgs e)
        {
            badLightingButton.IsChecked = false;
        }
        #endregion
    }
}
