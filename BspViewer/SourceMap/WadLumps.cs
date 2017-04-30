
namespace BspViewer
{
    public struct WadLump
    {
        public int Offset { get; set; }
        public int CompressedSize { get; set; }
        public int Size { get; set; }
        public sbyte Type { get; set; }
        public bool Compression { get; set; }
        public char[] Name { get; set; }
    }

    public struct WadHeader
    {
        public static readonly int SizeInBytes = 12;

        public string Magic { get; set; } // Should be WAD2/WAD3
        public int Dirs { get; set; } // Number of directory entries
        public int DirOffset { get; set; } // Offset into directory

        public override string ToString()
        {
            return string.Format("DirOffset: {0} Dirs: {1}", DirOffset, Dirs);
        }
    }
}