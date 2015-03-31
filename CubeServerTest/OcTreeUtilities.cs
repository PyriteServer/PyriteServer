namespace CubeServerTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using CubeServer;

    public class OcTreeUtilities
    {
        public static void Dump(OcTree<CubeBounds> ocTree)
        {
            Queue<Tuple<OcTree<CubeBounds>, int>> enumeration = new Queue<Tuple<OcTree<CubeBounds>, int>>();
            enumeration.Enqueue(new Tuple<OcTree<CubeBounds>, int>(ocTree,0));

            int octantCount = 0;
            int objectCount = 0;
            int maxDepth = 0;

            while (enumeration.Count > 0)
            {
                Tuple<OcTree<CubeBounds>, int> next = enumeration.Dequeue();
                OcTree<CubeBounds> nextOcTree = next.Item1;
                int indent = next.Item2;

                octantCount++;

                Trace.IndentLevel = indent;
                Trace.WriteLine(nextOcTree.ToString());

                foreach (CubeBounds obj in nextOcTree.Objects)
                {
                    objectCount++;
                    Trace.WriteLine(" " + obj.ToString());
                }

                if (nextOcTree.HasChildren)
                {
                    int nextIndent = indent + 1;
                    if (nextIndent > maxDepth)
                    {
                        maxDepth = nextIndent;
                    }

                    byte active = nextOcTree.OctantMask;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        if (((active >> bit) & 0x01) == 0x01)
                        {
                            OcTree<CubeBounds> childNode = nextOcTree.Octant[bit];
                            if (childNode != null)
                            {
                                enumeration.Enqueue(new Tuple<OcTree<CubeBounds>, int>(childNode, nextIndent));
                            }
                        }
                    }
                }
            }

            Trace.IndentLevel = 0;
            Trace.WriteLine(String.Format("Maximum Depth: {0}", maxDepth));
            Trace.WriteLine(String.Format("Octant count: {0}", octantCount)); 
            Trace.WriteLine(String.Format("Object count: {0}", objectCount));
        }
    }
}