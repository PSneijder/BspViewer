
namespace BspViewer
{
    public static class BspDefs
    {
        public const int CONTENTS_EMPTY = -1;
        public const int CONTENTS_SOLID = -2;
        public const int CONTENTS_WATER = -3;
        public const int CONTENTS_SLIME = -4;
        public const int CONTENTS_LAVA = -5;
        public const int CONTENTS_SKY = -6;
        public const int CONTENTS_ORIGIN = -7;
        public const int CONTENTS_CLIP = -8;
        public const int CONTENTS_CURRENT_0 = -9;
        public const int CONTENTS_CURRENT_90 = -10;
        public const int CONTENTS_CURRENT_180 = -11;
        public const int CONTENTS_CURRENT_270 = -12;
        public const int CONTENTS_CURRENT_UP = -13;
        public const int CONTENTS_CURRENT_DOWN = -14;
        public const int CONTENTS_TRANSLUCENT = -15;

        public const int LUMP_ENTITIES = 0;
        public const int LUMP_PLANES = 1;
        public const int LUMP_TEXTURES = 2;
        public const int LUMP_VERTEXES = 3;
        public const int LUMP_VISIBILITY = 4;
        public const int LUMP_NODES = 5;
        public const int LUMP_TEXINFO = 6;
        public const int LUMP_FACES = 7;
        public const int LUMP_LIGHTING = 8;
        public const int LUMP_CLIPNODES = 9;
        public const int LUMP_LEAFS = 10;
        public const int LUMP_MARKSURFACES = 11;
        public const int LUMP_EDGES = 12;
        public const int LUMP_SURFEDGES = 13;
        public const int LUMP_MODELS = 14;
        public const int HEADER_LUMPS = 15;

        public const int MAXLIGHTMAPS = 4;

        public const int PLANE_X = 0; // Plane is perpendicular to given axis
        public const int PLANE_Y = 1;
        public const int PLANE_Z = 2;
        public const int PLANE_ANYX = 3; // Non-axial plane is snapped to the nearest
        public const int PLANE_ANYY = 4;
        public const int PLANE_ANYZ = 5;
    }
}