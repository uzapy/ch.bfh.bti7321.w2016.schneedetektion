﻿using Schneedetektion.Data.Properties;
using System;

namespace Schneedetektion.Data
{
    public partial class Image
    {
        private static string folderName = Settings.Default.WorkingFolder;

        public void SaveCategories(bool? isSnow, bool? isNoSnow, bool? isNight, bool? isDusk, bool? isDay, bool? isFoggy, bool? isCloudy, bool? isRainy,
            bool? isBadLighting, bool? isGoodLighting)
        {
            this.Snow = isSnow;
            this.NoSnow = isNoSnow;
            this.Night = isNight;
            this.Dusk = isDusk;
            this.Day = isDay;
            this.Foggy = isFoggy;
            this.Cloudy = isCloudy;
            this.Rainy = isRainy;
            this.BadLighting = isBadLighting;
            this.GoodLighting = isGoodLighting;
        }
    }
}
