namespace CubeServerTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using CubeServer;

    public class OctTreeUtilities
    {
        public static void Dump(OctTree<CubeBounds> octTree)
        {
            Queue<Tuple<OctTree<CubeBounds>, int>> enumeration = new Queue<Tuple<OctTree<CubeBounds>, int>>();
            enumeration.Enqueue(new Tuple<OctTree<CubeBounds>, int>(octTree,0));

            while (enumeration.Count > 0)
            {
                Tuple<OctTree<CubeBounds>, int> next = enumeration.Dequeue();
                OctTree<CubeBounds> nextOctTree = next.Item1;
                int indent = next.Item2;

                Trace.IndentLevel = indent;
                Trace.WriteLine(nextOctTree.ToString());

                foreach (CubeBounds obj in nextOctTree.Objects)
                {
                    Trace.WriteLine(" " + obj.ToString());
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
                                enumeration.Enqueue(new Tuple<OctTree<CubeBounds>, int>(childNode, indent + 1));
                            }
                        }
                    }
                }
            }
        }
    }
}