using OpenTK;
using System;

namespace BspViewer
{
    sealed class MathLib
    {
        private const float EPSILON = 0.03125f; // 1/32

        public static Func<float, float> DegToRad = (x) => ((x) * (float) Math.PI / 180.0f);
        public static Func<float, float> RadToDeg = (x) => ((x) * 180.0f / (float) Math.PI);

        public static Vector3 RotateX(float a, Vector3 v)
        {
            a = DegToRad(a);

            Vector3 res;
            res.X = v.X;
            res.Y = v.Y * (float) Math.Cos(a) + v.Z * (float) -Math.Sin(a);
            res.Z = v.Y * (float) Math.Sin(a) + v.Z * (float) Math.Cos(a);
            return res;
        }

        public static Vector3 RotateY(float a, Vector3 v)
        {
            a = DegToRad(a);

            Vector3 res;
            res.X = v.X * (float) Math.Cos(a) + v.Z * (float) Math.Sin(a);
            res.Y = v.Y;
            res.Z = v.X * (float) -Math.Sin(a) + v.Z * (float) Math.Cos(a);
            return res;
        }

        public static Vector3 RotateZ(float a, Vector3 v)
        {
            a = DegToRad(a);

            Vector3 res;
            res.X = v.X * (float) Math.Cos(a) + v.Y * (float) -Math.Sin(a);
            res.Y = v.X * (float) Math.Sin(a) + v.Y * (float) Math.Cos(a);
            res.Z = v.Z;
            return res;
        }
    }
}