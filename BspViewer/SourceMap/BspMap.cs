using System.Drawing;

namespace BspViewer
{
    public sealed class BspMap
    {
        private string _name;

        public BspMap(string name)
        {
            _name = name;
        }

        public BspHeader Header { get; internal set; }
        public Entities Entities { get; internal set; }
        public BspModel[] Models { get; internal set; }
        public BspPlane[] Planes { get; internal set; }
        public BspFace[] Faces { get; internal set; }
        public BspVertex[] Vertices { get; internal set; }
        public BspEdge[] Edges { get; internal set; }
        public BspLeaf[] Leafs { get; internal set; }
        public BspNode[] Nodes { get; internal set; }
        public BspClipNode[] ClipNodes { get; internal set; }
        public short[] MarkSurfaces { get; internal set; }
        public int[] SurfEdges { get; internal set; }
        public BspMipTexture[] MipTextures { get; internal set; }
        public BspTextureInfo[] TextureInfos { get; internal set; }

        public bool[][] VisList { get; internal set; }
        public Color[] FaceColors { get; internal set; }
    }
}