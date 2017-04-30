
namespace BspViewer
{
    internal static class MathLib
    {
        public const float SQRT2 = 1.41421356237f;

        public static float DotProduct(float[] v1, float[] v2)
        {
            return (v1[0] * v2[0]) + (v1[1] * v2[1]) + (v1[2] * v2[2]);
        }
    }
}