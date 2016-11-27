using Schneedetektion.Data;
using System.ComponentModel;

namespace Schneedetektion.ImagePlayground
{
    public class ClassificationViewModel : ImageViewModel, INotifyPropertyChanged
    {
        #region Fields
        private bool? classifiedSnow;
        private bool? classifiedFoggy;
        private bool? classifiedRainy;
        private bool? classifiedBadLighting;
        #endregion

        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;

        public ClassificationViewModel(Image image) : base(image) { }

        public string TruthSnow
        {
            get
            {
                if (Image.Snow.Value)
                {
                    return "/Resources/snow.png";
                }
                else
                {
                    return "/Resources/sun.png";
                }
            }
        }

        public string TruthWeather
        {
            get
            {
                if (Image.Foggy.Value)
                {
                    return "/Resources/fog.png";
                }
                else if (Image.Rainy.Value)
                {
                    return "/Resources/rain.png";
                }
                else
                {
                    return "/Resources/sun.png";
                }
            }
        }

        public string TruthBadLighting
        {
            get
            {
                if (Image.BadLighting.Value)
                {
                    return "/Resources/sunglasses.png";
                }
                else
                {
                    return "/Resources/glasses.png";
                }
            }
        }

        public string ClassifiedSnow
        {
            get
            {
                if (classifiedSnow.HasValue)
                {
                    if (classifiedSnow.Value)
                    {
                        return "/Resources/snow.png";
                    }
                    else
                    {
                        return "/Resources/sun.png";
                    }
                }
                else
                {
                    return "/Resources/questionmark.png";
                }
            }
        }

        public string ClassifiedWeather
        {
            get
            {
                if (classifiedFoggy.HasValue && classifiedRainy.HasValue)
                {
                    if (classifiedFoggy.Value)
                    {
                        return "/Resources/fog.png";
                    }
                    else if (classifiedRainy.Value)
                    {
                        return "/Resources/rain.png";
                    }
                    else
                    {
                        return "/Resources/sun.png";
                    }
                }
                else
                {
                    return "/Resources/questionmark.png";
                }
            }
        }

        public string ClassifiedBadLighting
        {
            get
            {
                if (classifiedBadLighting.HasValue)
                {
                    if (classifiedBadLighting.Value)
                    {
                        return "/Resources/sunglasses.png";
                    }
                    else
                    {
                        return "/Resources/glasses.png";
                    }
                }
                else
                {
                    return "/Resources/questionmark.png";
                }
            }
        } 
        #endregion

        public void SetResults(bool snow, bool foggy, bool rainy, bool badLighting)
        {
            classifiedSnow = snow;
            classifiedFoggy = foggy;
            classifiedRainy = rainy;
            classifiedBadLighting = badLighting;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ClassifiedSnow)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ClassifiedWeather)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ClassifiedBadLighting)));
            }
        }
    }
}