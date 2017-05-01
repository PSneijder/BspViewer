using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BspViewer
{
    public static class TexUtil
    {
        public static Bitmap PixelsToTexture(uint[] pixelData, int width, int height, int channels)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var boundsRect = new Rectangle(0, 0, width, height);

            BitmapData bmpData = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            IntPtr pointer = bmpData.Scan0;
            int bytes = bmpData.Stride * bitmap.Height;

            var imgData = new byte[bytes];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var dataIndex = (x + y * width) * 4;
                    var pixelIndex = (x + y * width) * channels;
                    imgData[dataIndex + 0] = (byte)pixelData[pixelIndex + 0];
                    imgData[dataIndex + 1] = (byte)pixelData[pixelIndex + 1];
                    imgData[dataIndex + 2] = (byte)pixelData[pixelIndex + 2];

                    if (channels == 4)
                        imgData[dataIndex + 3] = (byte)pixelData[pixelIndex + 3];
                    else
                        imgData[dataIndex + 3] = 255;
                }
            }

            Marshal.Copy(imgData, 0, pointer, bytes);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        public static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public static int NextHighestPowerOfTwo(int x)
        {
            --x;
            for (var i = 1; i < 32; i <<= 1)
                x = x | x >> i;
            return x + 1;
        }
    }
}