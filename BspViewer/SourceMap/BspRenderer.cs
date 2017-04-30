
namespace BspViewer
{
    struct FaceCoord
    {
        public float Vec3s { get; set; }
        public float Vec3t { get; set; }
    }

    public abstract class BspRenderer
    {
        protected BspMap Map { get; private set; }

        protected BspRenderer(BspMap map)
        {
            Map = map;
        }
        
        #region BSP Traversal

        protected int TraverseTree(BspVector position, int nodeIndex)
        {
            var node = Map.Nodes[nodeIndex];

            // Run once for each child
            for (int i = 0; i < 2; i++)
            {
                // If the index is positive  it is an index into the nodes array
                if ((node.Children[i]) >= 0)
                {
                    if (PointInBox(position, Map.Nodes[node.Children[i]].Mins, Map.Nodes[node.Children[i]].Maxs))
                        return TraverseTree(position, node.Children[i]);
                }
                // Else, bitwise inversed, it is an index into the leaf array
                // Do not test solid leaf 0
                else if (~node.Children[i] != 0)
                {
                    if (PointInBox(position, Map.Leafs[~(node.Children[i])].Mins, Map.Leafs[~(node.Children[i])].Maxs))
                        return ~(node.Children[i]);
                }
            }

            return -1;
        }

        private bool PointInBox(BspVector vPoint, short[] vMin, short[] vMax)
        {
            if (vMin[0] <= vPoint.X && vPoint.X <= vMax[0] &&
                vMin[1] <= vPoint.Y && vPoint.Y <= vMax[1] &&
                vMin[2] <= vPoint.Z && vPoint.Z <= vMax[2] ||
                vMin[0] >= vPoint.X && vPoint.X >= vMax[0] &&
                vMin[1] >= vPoint.Y && vPoint.Y >= vMax[1] &&
                vMin[2] >= vPoint.Z && vPoint.Z >= vMax[2])
                return true;
            else
                return false;
        }
        
        #endregion

        #region BSP Rendering

        protected virtual void RenderLevel(BspVector cameraPos)
        {
            // Get the leaf where the camera is in
            int cameraLeaf = TraverseTree(cameraPos, 0);

            // Start the render traversal on the static geometry
            RenderNode(0, cameraLeaf, cameraPos);
        }

        protected void RenderNode(int nodeIndex, int cameraLeaf, BspVector cameraPos)
        {
            if (nodeIndex < 0)
            {
                if (nodeIndex == -1) // Solid leaf 0
                    return;

                if (cameraLeaf > 0)
                    if (Map.Header.Lumps[BspDefs.LUMP_VISIBILITY].Length != 0
                        && Map.VisList != null
                        && Map.VisList[cameraLeaf - 1] != null
                        && !Map.VisList[cameraLeaf - 1][~nodeIndex - 1])
                        return;

                RenderLeaf(~nodeIndex);

                return;
            }

            float distance;

            var node = Map.Nodes[nodeIndex];
            var plane = Map.Planes[node.PlaneId];

            switch (plane.Type)
            {
                case BspDefs.PLANE_X:
                    distance = cameraPos.X - plane.Distance;
                    break;
                case BspDefs.PLANE_Y:
                    distance = cameraPos.Y - plane.Distance;
                    break;
                case BspDefs.PLANE_Z:
                    distance = cameraPos.Z - plane.Distance;
                    break;
                default:
                    var normal = plane.Normal;
                    distance = DotProduct(new BspVector(normal[0], normal[1], normal[2]), cameraPos) - plane.Distance;
                    break;
            }

            if (distance > 0.0f)
            {
                RenderNode(node.Children[1], cameraLeaf, cameraPos);
                RenderNode(node.Children[0], cameraLeaf, cameraPos);
            }
            else
            {
                RenderNode(node.Children[0], cameraLeaf, cameraPos);
                RenderNode(node.Children[1], cameraLeaf, cameraPos);
            }
        }

        private void RenderLeaf(int leafIndex)
        {
            var leaf = Map.Leafs[leafIndex];

            // Loop through each face in this leaf
            for (int i = 0; i < leaf.NumMarkSurfaces; i++)
                RenderFace(Map.MarkSurfaces[leaf.FirstMarkSurface + i]);
        }

        protected abstract void RenderFace(int faceIndex);

        protected float DotProduct(BspVector v1, BspVector v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z);
        }
        
        #endregion
    }
}