using System.IO;
using System.Linq;
using System.Text;

namespace BspViewer.Extensions
{
    internal static class BinaryReaderExtensions
    {
        public static float[] ReadSingleArray(this BinaryReader reader, int count = 3)
        {
            float[] floats = new float[count];

            for (int i = 0; i < count; i++)
            {
                floats[i] = reader.ReadSingle();
            }

            return floats;
        }

        public static int[] ReadInt32Array(this BinaryReader reader, int count = 3)
        {
            int[] integers = new int[count];

            for (int i = 0; i < count; i++)
            {
                integers[i] = reader.ReadInt32();
            }

            return integers;
        }

        public static short[] ReadInt16Array(this BinaryReader reader, int count = 3)
        {
            short[] shorts = new short[count];

            for (int i = 0; i < count; i++)
            {
                shorts[i] = reader.ReadInt16();
            }

            return shorts;
        }

        public static uint[] ReadUInt32Array(this BinaryReader reader, int count = 3)
        {
            uint[] integers = new uint[count];

            for (int i = 0; i < count; i++)
            {
                integers[i] = reader.ReadUInt32();
            }

            return integers;
        }

        public static ushort[] ReadUInt16Array(this BinaryReader reader, int count = 2)
        {
            ushort[] shorts = new ushort[count];

            for (int i = 0; i < count; i++)
            {
                shorts[i] = reader.ReadUInt16();
            }

            return shorts;
        }

        public static string ReadString(this BinaryReader reader, int count = 16)
        {
            byte[] bytes = reader.ReadBytes(count);

            if (bytes == null)
            {
                return null;
            }

            char[] dirtyCharacters = Encoding.UTF8.GetChars(bytes);

            char[] cleanedCharacters = dirtyCharacters
                .Where(c => !char.IsControl(c))
                    .ToArray();

            return new string(cleanedCharacters);
        }
    }
}