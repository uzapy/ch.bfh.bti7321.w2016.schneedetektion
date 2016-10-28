using Microsoft.VisualStudio.TestTools.UnitTesting;
using Schneedetektion.OpenCV;
using System.Collections.Generic;

namespace Schneedetektion.Test
{
    [TestClass]
    public class OpenCVTest
    {
        [TestMethod]
        public void DrawHistogramTest()
        {
            // Load Bitmap
            string image = @"C:\Users\uzapy\Desktop\astra2016\mvk110\mvk110_20141202_123001.jpg";

            // Open CV
            OpenCVHelper helper = new OpenCVHelper();
            List<float[]> histogram = helper.GetHistogram(image);

            // Write to File
            //PngBitmapEncoder encoder = new PngBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(histogram));
            //using (var fileStream = new FileStream(@"C:\Users\uzapy\Desktop\test\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png", FileMode.Create))
            //{
            //    encoder.Save(fileStream);
            //}
        }
    }
}
