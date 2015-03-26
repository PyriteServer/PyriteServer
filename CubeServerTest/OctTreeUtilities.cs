namespace CubeServerTest
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using CubeServer;

    public class OctTreeUtilities
    {
        public static void Dump(OctTree<CubeBounds> octTree)
        {
            Queue<OctTree<CubeBounds>> enumeration = new Queue<OctTree<CubeBounds>>();
            enumeration.Enqueue(octTree);

            while (enumeration.Count > 0)
            {
                OctTree<CubeBounds> nextOctTree = enumeration.Dequeue();

                Trace.WriteLine(nextOctTree.ToString());

                foreach (CubeBounds obj in nextOctTree.Objects)
                {
                    Trace.WriteLine(obj.ToString());
                }

                if (nextOctTree.HasChildren)
                {
                    byte active = nextOctTree.OctantMask;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        if (((active >> bit) & 0x01) == 0x01)
                        {
                            OctTree<CubeBounds> childNode = nextOctTree.Octant[bit];
                            if (childNode != null)
                            {
                                enumeration.Enqueue(childNode);
                            }
                        }
                    }
                }
            }
        }
    }
}