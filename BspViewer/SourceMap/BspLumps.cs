using System.Collections;

namespace BspViewer
{
    public struct BspLump
    {
        public int Offset { get; set; } // File offset to data
        public int Length { get; set; } // Length of data

        public override string ToString()
        {
            return string.Format("Offset: {0} Size: {1}", Offset, Length);
        }
    }

    public struct BspHeader
    {
        public int Version { get; set; }
        public BspLump[] Lumps { get; set; }
    }

    public struct BspPlane
    {
        public static readonly int SizeInBytes = 20;
        
        public float[] Normal { get; set; } // The planes normal vector
        public float Distance { get; set; } // Plane equation is: vNormal * X = fDist
        public int Type { get; set; } // Plane type, see #defines
    }

    public struct BspFace
    {
        public static readonly int SizeInBytes = 20;

        public short PlaneId { get; set; } // Plane the face is parallel to
        public short PlaneSide { get; set; } // Set if different normals orientation
        public int FirstEdge { get; set; } // Index of the first surfedge
        public short NumEdges { get; set; } // Number of consecutive surfedges
        public short TextureInfo { get; set; } // Index of the texture info structure
        public byte[] Styles { get; set; } // Specify lighting styles
        public int LightmapOffset { get; set; }  // Offsets into the raw lightmap data
    }

    public struct BspVertex
    {
        public static readonly int SizeInBytes = 12;

        public float[] Point { get; set; }
    }

    public struct BspEdge
    {
        public static readonly int SizeInBytes = 4;

        public ushort[] Vertices { get; set; } // Indices into vertex array
    }

    public struct BspLeaf
    {
        public static readonly int SizeInBytes = 28;

        public int Contents { get; set; } // Contents enumeration
        public int VisOffset { get; set; } // Offset into the visibility lump
        public short[] Mins { get; set; } // Defines bounding box
        public short[] Maxs { get; set; }
        public ushort FirstMarkSurface { get; set; } // Index and count into marksurfaces array
        public ushort NumMarkSurfaces { get; set; }
        public byte[] AmbientLevels { get; set; } // Ambient sound levels
        public BitArray Pvs { get; set; }
    }

    public struct BspNode
    {
        public static readonly int SizeInBytes = 24;

        public int PlaneId { get; set; } // Index into Planes lump
        public short[] Children { get; set; } // If > 0, then indices into Nodes // otherwise bitwise inverse indices into Leafs
        public short[] Mins { get; set; } // Defines bounding box
        public short[] Maxs { get; set; }
        public ushort FirstFace { get; set; } // Index and count into Faces
        public ushort NumFaces { get; set; }
    }

    public struct BspClipNode
    {
        public static readonly int SizeInBytes = 8;

        public int PlaneId { get; set; } // Index into Planes lump
        public short[] Children { get; set; } // negative numbers are contents
    }

    public struct BspVisData
    {
        public byte[] CompressedVis { get; set; }
    }

    public struct BspModel
    {
        public float[] Mins { get; set; } // Defines bounding box
        public float[] Maxs { get; set; } // Defines bounding box       
        public float[] Origin { get; set; } // Coordinates to move the // coordinate system
        public int[] Nodes { get; set; } // [0] index of first BSP node, [1] index of the first Clip node, [2] index of the second Clip node, [3] usually zero
        public int NumLeafs { get; set; } // number of BSP leaves
        public int FirstFace { get; set; }
        public int NumFaces { get; set; } // Index and count into faces lump
    }

    public struct BspMipTexture
    {
        public string Name { get; set; } // Name of texture
        public uint Width { get; set; } // Extends of the texture
        public uint Height { get; set; } 
        public uint[] Offsets { get; set; } // Offsets to texture mipmaps BSPMIPTEX;

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Name, Width, Height);
        }
    }

    public struct BspTextureInfo
    {
        public float[] Vec3s { get; set; }
        public float SShift { get; set; } // Texture shift in s direction
        public float[] Vec3t { get; set; }
        public float TShift { get; set; } // Texture shift in t direction
        public uint MipTex { get; set; } // Index into textures array
        public uint Flags { get; set; } // Texture flags, seem to always be 0
    }

    public struct BspTextureHeader
    {
        public int NumberMipTextures { get; set; }
        public int[] Offsets { get; set; }
    }

    public struct BspVector
    {
        public BspVector(float x, float y, float z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}