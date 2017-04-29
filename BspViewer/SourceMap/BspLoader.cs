using System.Collections.Generic;
using System.IO;
using System.Text;
using BspViewer.Extensions;
using System.Drawing;
using HalfLife.Sharp.Utils;
using System;

namespace BspViewer
{
    public sealed class BspLoader
    {
        private string _fileName;

        private BspHeader _header;
        private Entities _entities;
        private BspModel[] _models;
        private BspPlane[] _planes;
        private BspFace[] _faces;
        private BspVertex[] _vertices;
        private BspEdge[] _edges;
        private BspLeaf[] _leafs;
        private BspNode[] _nodes;
        private BspClipNode[] _clipNodes;
        private short[] _markSurfaces;
        private int[] _surfEdges;
        private BspMipTexture[] _mipTextures;
        private BspTextureInfo[] _textureInfos;

        private BspVisData _visData;
        private bool[][] _vis;

        public BspLoader(string fileName)
        {
            _fileName = fileName;
        }

        public BspMap Load()
        {
            if(!File.Exists(_fileName))
            {
                throw new FileNotFoundException(_fileName);
            }

            using (var reader = new BinaryReader(new FileStream(_fileName, FileMode.Open)))
            {
                _header = ReadHeader(reader);
                _models = ReadModels(reader);
                _entities = ReadEntities(reader);
                _planes = ReadPlanes(reader);
                _faces = ReadFaces(reader);
                _vertices = ReadVertices(reader);
                _edges = ReadEdges(reader);
                _leafs = ReadLeafs(reader);
                _nodes = ReadNodes(reader);
                _clipNodes = ReadClipNodes(reader);
                _markSurfaces = ReadMarkSurfaces(reader);
                _surfEdges = ReadSurfEdges(reader);
                _mipTextures = ReadMipTextures(reader);
                _textureInfos = ReadTextureInfos(reader);

                _visData = ReadVisData(reader);
                //_vis = LoadVis(reader);
            }

            string name = Path.GetFileNameWithoutExtension(_fileName);

            return new BspMap(name)
            {
                Header = _header,
                Entities = _entities,
                Models = _models,
                Planes = _planes,
                Faces = _faces,
                Vertices = _vertices,
                Edges = _edges,
                Leafs = _leafs,
                Nodes = _nodes,
                ClipNodes = _clipNodes,
                MarkSurfaces = _markSurfaces,
                SurfEdges = _surfEdges,
                MipTextures = _mipTextures,
                TextureInfos = _textureInfos,

                //VisList = _vis,
                FaceColors = CreateRandomFaceColors(),
            };
        }

        private Color[] CreateRandomFaceColors()
        {
            var colors = new Color[_faces.Length];

            for (int i = 0; i < _faces.Length; i++)
            {
                colors[i] = ColorUtils.GenerateRandomBrushColour();
            }

            return colors;
        }

        #region Structure Read

        private BspTextureInfo[] ReadTextureInfos(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_TEXINFO];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);

            int numTextureInfos = entitiesLump.Length / 40;

            var textureInfos = new List<BspTextureInfo>();
            for (int i = 0; i < numTextureInfos; i++)
            {
                textureInfos.Add(new BspTextureInfo { Vec3s = reader.ReadSingleArray(), SShift = reader.ReadSingle(), Vec3t = reader.ReadSingleArray(), TShift = reader.ReadSingle(), MipTex = reader.ReadUInt32(), Flags = reader.ReadUInt32() });
            }

            return textureInfos.ToArray();
        }

        private BspMipTexture[] ReadMipTextures(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_TEXTURES];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);

            int numberOfTextures = reader.ReadInt32();
            int[] mipTexturesOffset = new int[numberOfTextures];

            for (int i = 0; i < numberOfTextures; i++)
            {
                mipTexturesOffset[i] = (entitiesLump.Offset + reader.ReadInt32());
            }

            var mipTextures = new List<BspMipTexture>();
            for (int i = 0; i < numberOfTextures; i++)
            {
                int textureOffset = mipTexturesOffset[i];
                reader.BaseStream.Seek(textureOffset, SeekOrigin.Begin);
                
                mipTextures.Add(new BspMipTexture { Name = reader.ReadString(16), Width = reader.ReadUInt32(), Height = reader.ReadUInt32(), Offsets = reader.ReadUInt32Array(4) });
            }
                
            return mipTextures.ToArray();
        }

        private BspModel[] ReadModels(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_MODELS];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);
            int numModels = entitiesLump.Length / 64;
            
            var models = new List<BspModel>();
            for (int i = 0; i < numModels; i++)
            {
                models.Add(new BspModel { Mins = reader.ReadSingleArray(), Maxs = reader.ReadSingleArray(), Origin = reader.ReadSingleArray(), Nodes = reader.ReadInt32Array(), NumLeafs = reader.ReadInt32(), FirstFace = reader.ReadInt32(), NumFaces = reader.ReadInt32() });
            }

            return models.ToArray();
        }

        private BspVisData ReadVisData(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_VISIBILITY];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);

            byte[] vis = reader.ReadBytes(entitiesLump.Length);

            return new BspVisData { CompressedVis = vis };
        }

        private byte[] ReadVisData(BinaryReader reader, int length)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_VISIBILITY];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);

            byte[] vis = reader.ReadBytes(length);

            return vis;
        }

        private int[] ReadSurfEdges(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_SURFEDGES];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);
            int numSurfedges = entitiesLump.Length / 4;

            var surfedges = new List<int>();
            for (int i = 0; i < numSurfedges; i++)
            {
                surfedges.Add(reader.ReadInt32());
            }

            return surfedges.ToArray();
        }

        private short[] ReadMarkSurfaces(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_MARKSURFACES];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);
            int numMarkSurfaces = entitiesLump.Length / 2;

            var markSurfaces = new List<short>();
            for (int i = 0; i < numMarkSurfaces; i++)
            {
                markSurfaces.Add(reader.ReadInt16());
            }

            return markSurfaces.ToArray();
        }

        private BspClipNode[] ReadClipNodes(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_CLIPNODES];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);
            int numClipNodes = entitiesLump.Length / BspClipNode.SizeInBytes;

            var clipNodes = new List<BspClipNode>();
            for (int i = 0; i < numClipNodes; i++)
            {
                clipNodes.Add(new BspClipNode { PlaneId = reader.ReadInt32(), Children = reader.ReadInt16Array(2) });
            }

            return clipNodes.ToArray();
        }

        private BspNode[] ReadNodes(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_NODES];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);
            int numNodes = entitiesLump.Length / BspNode.SizeInBytes;

            var nodes = new List<BspNode>();
            for (int i = 0; i < numNodes; i++)
            {
                nodes.Add(new BspNode { PlaneId = reader.ReadInt32(), Children = reader.ReadInt16Array(2), Mins = reader.ReadInt16Array(), Maxs = reader.ReadInt16Array(), FirstFace = reader.ReadUInt16(), NumFaces = reader.ReadUInt16() });
            }

            return nodes.ToArray();
        }

        private BspLeaf[] ReadLeafs(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_LEAFS];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);
            int numLeafs = entitiesLump.Length / BspLeaf.SizeInBytes;

            var leafs = new List<BspLeaf>();
            for (int i = 0; i < numLeafs; i++)
            {
                leafs.Add(new BspLeaf { Contents = reader.ReadInt32(), VisOffset = reader.ReadInt32(), Mins = reader.ReadInt16Array(), Maxs = reader.ReadInt16Array(), FirstMarkSurface = reader.ReadUInt16(), NumMarkSurfaces = reader.ReadUInt16(), AmbientLevels = reader.ReadBytes(4) });
            }

            return leafs.ToArray();
        }

        private BspEdge[] ReadEdges(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_EDGES];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);
            int numEdges = entitiesLump.Length / BspEdge.SizeInBytes;

            var edges = new List<BspEdge>();
            for (int i = 0; i < numEdges; i++)
            {
                edges.Add(new BspEdge { Vertex = reader.ReadUInt16Array() });
            }

            return edges.ToArray();
        }

        private BspVertex[] ReadVertices(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_VERTEXES];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);
            int numVertices = entitiesLump.Length / BspVertex.SizeInBytes;

            var vertices = new List<BspVertex>();
            for (int i = 0; i < numVertices; i++)
            {
                vertices.Add(new BspVertex { Point = reader.ReadSingleArray() });
            }

            return vertices.ToArray();
        }

        private BspFace[] ReadFaces(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_FACES];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);
            int numFaces = entitiesLump.Length / BspFace.SizeInBytes;

            var faces = new List<BspFace>();
            for (int i = 0; i < numFaces; i++)
            {
                faces.Add(new BspFace { PlaneId = reader.ReadInt16(), PlaneSide = reader.ReadInt16(), FirstEdge = reader.ReadInt32(), NumEdges = reader.ReadInt16(), TextureInfo = reader.ReadInt16(), Styles = reader.ReadBytes(4), LightmapOffset = reader.ReadInt32() });
            }

            return faces.ToArray();
        }

        private BspPlane[] ReadPlanes(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_PLANES];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);
            int numPlanes = entitiesLump.Length / BspPlane.SizeInBytes;

            var planes = new List<BspPlane>();
            for (int i = 0; i < numPlanes; i++)
            {
                planes.Add(new BspPlane { Normal = reader.ReadSingleArray(), Distance = reader.ReadSingle(), Type = reader.ReadInt32() });
            }

            return planes.ToArray();
        }

        private Entities ReadEntities(BinaryReader reader)
        {
            BspLump entitiesLump = _header.Lumps[BspDefs.LUMP_ENTITIES];
            reader.BaseStream.Seek(entitiesLump.Offset, SeekOrigin.Begin);

            var bytes = reader.ReadBytes(entitiesLump.Length);
            string rawEntities = Encoding.ASCII.GetString(bytes);

            var parser = new EntityParser(bytes);
            var entities = parser.Parse();

            return entities;
        }

        private BspHeader ReadHeader(BinaryReader reader)
        {
            var version = reader.ReadInt32();

            var lumps = new List<BspLump>();
            for (int i = 0; i < 15; i++)
            {
                lumps.Add(new BspLump { Offset = reader.ReadInt32(), Length = reader.ReadInt32() });
            }

            return new BspHeader { Version = version, Lumps = lumps.ToArray() };
        }

        #endregion

        #region PVS
               
        // Loads and decompresses the PVS (Potentially Visible Set) from the bsp file.
        public bool[][] LoadVis(BinaryReader reader)
        {
            if (_visData.CompressedVis.Length > 0)
            {
                var visLeaves = CountVisLeafs(0);

                var visLists = new bool[visLeaves][];

                for (var i = 0; i < visLeaves; i++)
                {
                    if (_leafs[i + 1].VisOffset >= 0)
                        visLists[i] = GetPVS(reader, i + 1, visLeaves);
                    else
                        visLists[i] = null;
                }

                return visLists;
            }

            return null;
        }

        private int CountVisLeafs(int nodeIndex)
        {
            if (nodeIndex < 0)
            {
                // leaf 0
                if (nodeIndex == -1)
                    return 0;

                if (_leafs[~nodeIndex].Contents == BspDefs.CONTENTS_SOLID)
                    return 0;

                return 1;
            }

            var node = _nodes[nodeIndex];

            return CountVisLeafs(node.Children[0]) + CountVisLeafs(node.Children[1]);
        }

        private bool[] GetPVS(BinaryReader reader, int leafIndex, int visLeaves)
        {
            var list = new bool[_leafs.Length - 1];

            for (var i = 0; i < list.Length; i++)
                list[i] = false;

            var compressed = ReadVisData(reader, _header.Lumps[BspDefs.LUMP_VISIBILITY].Offset + _leafs[leafIndex].VisOffset);
            var writeIndex = 0; // Index that moves through the destination bool array (list)

            for (var curByte = 0; writeIndex < visLeaves; curByte++)
            {
                // Check for a run of 0s
                if (compressed[curByte] == 0)
                {
                    // Advance past this run of 0s
                    curByte++;
                    // Move the write pointer the number of compressed 0s
                    writeIndex += 8 * compressed[curByte];
                }
                else
                {
                    // Iterate through this byte with bit shifting till the bit has moved beyond the 8th digit
                    for (var mask = 0x01; mask != 0x0100; writeIndex++, mask <<= 1)
                    {
                        // Test a bit of the compressed PVS with the bit mask
                        if ((compressed[curByte] & mask) == 1 && (writeIndex < visLeaves))
                            list[writeIndex] = true;
                    }
                }
            }

            return list;
        }

        #endregion
    }
}