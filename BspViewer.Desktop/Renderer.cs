using HalfLife.Sharp.Utils;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BspViewer
{
    struct FaceBuffer
    {
        public int Start { get; set; }
        public int Count { get; set; }
    }

    sealed class Renderer
        : BspRenderer
    {
        private FaceBuffer[] _faceBufferRegions;

        private int _vertexBuffer;
        private int _normalBuffer;

        public Renderer(BspMap map)
            : base(map)
        {
            PreRender();
        }

        public void Render(Vector3 position)
        {
            GL.Enable(EnableCap.DepthTest);

            RenderLevel(new BspVector(position.X, position.Y, position.Z));
            //RenderLeavesOutlines();

            GL.Disable(EnableCap.DepthTest);
        }

        protected override void PreRender()
        {
            var vertexList = new List<Vector3>();
            var normalList = new List<float>();

            _faceBufferRegions = new FaceBuffer[Map.Faces.Length];
            var elements = 0;

            // for each face
            for (var i = 0; i < Map.Faces.Length; i++)
            {
                var face = Map.Faces[i];

                _faceBufferRegions[i] = new FaceBuffer
                {
                    Start = elements,
                    Count = face.NumEdges
                };

                //var texInfo = Map.TextureInfos[face.TextureInfo];
                var plane = Map.Planes[face.PlaneId];

                var normal = plane.Normal;

                for (var j = 0; j < face.NumEdges; j++)
                {
                    var edgeIndex = Map.SurfEdges[face.FirstEdge + j]; // This gives the index into the edge lump

                    int vertexIndex;
                    if (edgeIndex > 0)
                    {
                        var edge = Map.Edges[edgeIndex];
                        vertexIndex = edge.Vertex[0];
                    }
                    else
                    {
                        edgeIndex *= -1;
                        var edge = Map.Edges[edgeIndex];
                        vertexIndex = edge.Vertex[1];
                    }

                    var vertex = Map.Vertices[vertexIndex].Point;

                    // Write to buffers
                    //vertexList.Add(vertex[0]);
                    //vertexList.Add(vertex[1]);
                    //vertexList.Add(vertex[2]);
                    vertexList.Add(new Vector3(vertex[0], vertex[1], vertex[2]));

                    normalList.Add(normal[0]);
                    normalList.Add(normal[1]);
                    normalList.Add(normal[2]);

                    elements += 1;
                }
            }

            var vertices = vertexList.ToArray();
            var normals = normalList.ToArray();

            // Create all buffers

            _vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * BlittableValueType.StrideOf(vertices)), vertices, BufferUsageHint.StaticDraw);

            //_normalBuffer = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _normalBuffer);
            //GL.BufferData(BufferTarget.ArrayBuffer, (normals.Length * sizeof(float)), normals, BufferUsageHint.StaticDraw);
        }

        protected override void RenderFace(int faceIndex)
        {
            var face = Map.Faces[faceIndex];

            if (Map.Faces[faceIndex].Styles[0] == 0xFF) // Skip sky faces
                return;

            // if the light map offset is not -1 and the lightmap lump is not empty, there are lightmaps
            var lightmapAvailable = face.LightmapOffset != -1 && Map.Header.Lumps[BspDefs.LUMP_LIGHTING].Length > 0;

            GL.Color3(Map.FaceColors[faceIndex]);

            BeginMode mode;
            switch (face.NumEdges)
            {
                case 3:
                    mode = BeginMode.Triangles;
                    break;
                case 4:
                    mode = BeginMode.Quads;
                    break;
                default:
                    mode = BeginMode.Polygon;
                    break;
            }
            GL.Begin(mode);
            for (int i = 0; i < Map.Faces[faceIndex].NumEdges; i++)
            {
                var vNormal = Map.Planes[face.PlaneId].Normal;
                var normal = new Vector3(vNormal[0], vNormal[1], vNormal[2]);

                if (Map.Faces[faceIndex].PlaneSide != 0)
                    normal = normal * -1;

                GL.Normal3(normal.X, normal.Y, normal.Z);

                int iEdge = Map.SurfEdges[face.FirstEdge + i]; // This gives the index into the edge lump

                if (iEdge > 0)
                {
                    var edge = Map.Edges[iEdge];
                    var point = Map.Vertices[edge.Vertex[0]].Point;
                    var vertex = new Vector3(point[0], point[1], point[2]);

                    GL.Vertex3(vertex.X, vertex.Y, vertex.Z);
                }
                else
                {
                    iEdge *= -1;

                    var edge = Map.Edges[iEdge];
                    var point = Map.Vertices[edge.Vertex[1]].Point;
                    var vertex = new Vector3(point[0], point[1], point[2]);

                    GL.Vertex3(vertex.X, vertex.Y, vertex.Z);
                }
            }
            GL.End();
        }

        protected override void RenderLevel(BspVector cameraPos)
        {
            // enable/disable the required attribute arrays
            //GL.EnableVertexAttribArray(SimpleGame.TexCoordLocation);
            //GL.EnableVertexAttribArray(SimpleGame.LightmapCoordLocation);
            //GL.EnableVertexAttribArray(SimpleGame.NormalLocation);
            //GL.DisableVertexAttribArray(SimpleGame.ColorLocation);

            //// Bind the vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);

            //GL.VertexAttribPointer(SimpleGame.PositionLocation, 3, VertexAttribPointerType.Float, false, 0, 0);

            //// Bind the normal buffer
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _normalBuffer);
            //GL.VertexAttribPointer(SimpleGame.NormalLocation, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Get the leaf where the camera is in
            int cameraLeaf = TraverseTree(cameraPos, 0);

            // Start the render traversal on the static geometry
            RenderNode(0, cameraLeaf, cameraPos);
        }

        #region BSP Rendering Debug

        private void RenderLeavesOutlines()
        {
            GL.LineWidth(1.0f);
            GL.LineStipple(1, 0xF0F0);

            GL.Enable(EnableCap.LineStipple);
            for (int i = 0; i < Map.Leafs.Length; i++)
            {
                RenderLeafOutlines(i);
            }
            GL.Disable(EnableCap.LineStipple);
            GL.Color3(1.0f, 1.0f, 1.0f);
        }

        private void RenderLeafOutlines(int leafIndex)
        {
            GL.Color3(ColorUtils.GenerateRandomColor(Color.Red));

            BspLeaf leaf = Map.Leafs[leafIndex];

            GL.Begin(BeginMode.Lines);
            // Draw right face of bounding box
            GL.Vertex3(leaf.Maxs[0], leaf.Maxs[1], leaf.Maxs[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Mins[1], leaf.Maxs[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Mins[1], leaf.Maxs[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Mins[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Mins[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Maxs[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Maxs[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Maxs[1], leaf.Maxs[2]);

            // Draw left face of bounding box
            GL.Vertex3(leaf.Mins[0], leaf.Mins[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Mins[0], leaf.Maxs[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Mins[0], leaf.Maxs[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Mins[0], leaf.Maxs[1], leaf.Maxs[2]);
            GL.Vertex3(leaf.Mins[0], leaf.Maxs[1], leaf.Maxs[2]);
            GL.Vertex3(leaf.Mins[0], leaf.Mins[1], leaf.Maxs[2]);
            GL.Vertex3(leaf.Mins[0], leaf.Mins[1], leaf.Maxs[2]);
            GL.Vertex3(leaf.Mins[0], leaf.Mins[1], leaf.Mins[2]);

            // Connect the faces
            GL.Vertex3(leaf.Mins[0], leaf.Maxs[1], leaf.Maxs[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Maxs[1], leaf.Maxs[2]);
            GL.Vertex3(leaf.Mins[0], leaf.Maxs[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Maxs[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Mins[0], leaf.Mins[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Mins[1], leaf.Mins[2]);
            GL.Vertex3(leaf.Mins[0], leaf.Mins[1], leaf.Maxs[2]);
            GL.Vertex3(leaf.Maxs[0], leaf.Mins[1], leaf.Maxs[2]);
            GL.End();
        }

        #endregion
    }
}
