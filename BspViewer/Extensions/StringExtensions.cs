using System;
using System.Globalization;

namespace BspViewer.Extensions
{
    public static class StringExtensions
    {
        public static float[] AsSingleArray(this string key)
        {
            float[] results = new float[3];
            string[] nums = key.Split(' ');

            for (int i = 0; i < results.Length && i < nums.Length; ++i)
            {
                try
                {
                    results[i] = Single.Parse(nums[i], NumberStyles.Float);
                }
                catch
                {
                    results[i] = 0;
                }
            }

            return results;
        }
    }
}