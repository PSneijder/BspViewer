using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace BspViewer.Desktop
{
    static class TexUtil
    {
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

        public static int PixelsToTexture()
        {
            var texture = GL.GenTexture();

            return texture;
        }
    }
}