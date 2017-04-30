using System.Drawing;
using System.Drawing.Imaging;

namespace BspViewer
{
    sealed class TexUtil
    {
        public static void SaveTexture(BspTextureData texture)
        {
            using (var bitmap = new Bitmap(texture.Width, texture.Height))
            { 
                for (var j = 0; j < texture.Height; j++)
                {
                    for (var i = 0; i < texture.Width; i++)
                    {
                        var color = Color.FromArgb(
                            (int)texture.TextureData[i + j * texture.Width],
                            (int)texture.TextureData[i + j * texture.Width],
                            (int)texture.TextureData[i + j * texture.Width]);

                        bitmap.SetPixel(i, j, color);
                    }
                }

                Image image = bitmap;
                image.Save("Textures\\" + texture.Name + ".bmp", ImageFormat.Bmp);
            }
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