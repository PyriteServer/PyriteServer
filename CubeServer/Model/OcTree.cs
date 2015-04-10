// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="OctTree.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CubeServer.Contracts;
    using Microsoft.Xna.Framework;

    public class OcTree<TObject> where TObject : IBounds<TObject>
    {
        private const int DEFAULT_MIN_SIZE = 1;

        private readonly uint[] debruijnPosition = { 0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30, 8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31 };

        private readonly Queue<TObject> insertionQueue = new Queue<TObject>();

        private readonly int minimumSize = DEFAULT_MIN_SIZE;
        private readonly List<TObject> objects;
        private readonly OcTree<TObject>[] octants = new OcTree<TObject>[8];

        private byte activeOctants = 0;

        private OcTree<TObject> parent;
        private BoundingBox region;
        private bool treeBuilt = false;
        private bool treeReady = false;

        public OcTree() : this(new BoundingBox(Vector3.Zero, Vector3.Zero))
        {
        }

        public OcTree(BoundingBox region) : this(region, new TObject[] { })
        {
        }

        public OcTree(BoundingBox region, IEnumerable<TObject> objList) : this(region, objList, DEFAULT_MIN_SIZE)
        {
        }

        public OcTree(BoundingBox region, IEnumerable<TObject> objList, int minSize)
        {
            this.region = region;
            this.objects = new List<TObject>(objList);
            this.minimumSize = minSize;
        }

        public int MinimumSize
        {
            get { return this.minimumSize; }
        }

        public byte OctantMask
        {
            get { return this.activeOctants; }
        }

        public bool HasChildren
        {
            get { return this.activeOctants != 0; }
        }

        public bool IsEmpty
        {
            get
            {
                if (this.objects.Count != 0)
                {
                    return false;
                }

                if (activeOctants != 0)
                {
                    for (int a = 0; a < 8; a++)
                    {
                        if (this.octants[a] != null && !this.octants[a].IsEmpty)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public bool IsRoot
        {
            get { return this.parent == null; }
        }

        public IList<TObject> Objects
        {
            get { return this.objects; }
        }

        public OcTree<TObject>[] Octant
        {
            get { return this.octants; }
        }

        public BoundingBox Region
        {
            get { return this.region; }
        }

        public override string ToString()
        {
            return String.Format("Region:{0} Children:{1}b Objects:{2}", this.Region, Convert.ToString(this.activeOctants, 2).PadLeft(8, '0'), this.objects.Count);
        }

        public void Add(IEnumerable<TObject> items)
        {
            foreach (TObject item in items)
            {
                this.insertionQueue.Enqueue(item);
                this.treeReady = false;
            }
        }

        public void Add(TObject item)
        {
            this.Add(new[] { item });
        }

        public IEnumerable<Intersection<TObject>> AllIntersections(Ray ray)
        {
            if (!this.treeReady)
            {
                this.UpdateTree();
            }

            return this.GetIntersection(ray);
        }

        public IEnumerable<Intersection<TObject>> AllIntersections(BoundingFrustum frustrum)
        {
            if (!this.treeReady)
            {
                this.UpdateTree();
            }

            return this.GetIntersection(frustrum);
        }

        public IEnumerable<Intersection<TObject>> AllIntersections(BoundingBox box)
        {
            if (!this.treeReady)
            {
                this.UpdateTree();
            }

            return this.GetIntersection(box);
        }


        public Intersection<TObject> NearestIntersection(Ray ray)
        {
            if (!this.treeReady)
            {
                this.UpdateTree();
            }

            IEnumerable<Intersection<TObject>> intersections = this.GetIntersection(ray);

            Intersection<TObject> nearest = new Intersection<TObject>();

            foreach (Intersection<TObject> ir in intersections)
            {
                if (nearest.HasHit == false)
                {
                    nearest = ir;
                    continue;
                }

                if (ir.Distance < nearest.Distance)
                {
                    nearest = ir;
                }
            }

            return nearest;
        }

        public void Remove(TObject item)
        {
            this.objects.Remove(item);
        }

        public void UpdateTree()
        {
            if (!this.treeBuilt)
            {
                while (this.insertionQueue.Count != 0)
                {
                    this.objects.Add(this.insertionQueue.Dequeue());
                }
                this.BuildTree();
            }
            else
            {
                while (this.insertionQueue.Count != 0)
                {
                    this.Insert(this.insertionQueue.Dequeue());
                }
            }

            this.treeReady = true;
        }

        private void BuildTree()
        {
            if (this.objects.Count <= 1)
            {
                return;
            }

            Vector3 dimensions = this.region.Max - this.region.Min;

            if (dimensions == Vector3.Zero)
            {
                this.SetEnclosingCube();
                dimensions = this.region.Max - this.region.Min;
            }

            //Check to see if the dimensions of the box are greater than the minimum dimensions
            if (dimensions.X <= this.minimumSize && dimensions.Y <= this.minimumSize && dimensions.Z <= this.minimumSize)
            {
                return;
            }

            Vector3 half = dimensions / 2.0f;
            Vector3 center = this.region.Min + half;

            BoundingBox[] octant = new BoundingBox[8];
            octant[0] = new BoundingBox(this.region.Min, center);
            octant[1] = new BoundingBox(new Vector3(center.X, this.region.Min.Y, this.region.Min.Z), new Vector3(this.region.Max.X, center.Y, center.Z));
            octant[2] = new BoundingBox(new Vector3(center.X, this.region.Min.Y, center.Z), new Vector3(this.region.Max.X, center.Y, this.region.Max.Z));
            octant[3] = new BoundingBox(new Vector3(this.region.Min.X, this.region.Min.Y, center.Z), new Vector3(center.X, center.Y, this.region.Max.Z));
            octant[4] = new BoundingBox(new Vector3(this.region.Min.X, center.Y, this.region.Min.Z), new Vector3(center.X, this.region.Max.Y, center.Z));
            octant[5] = new BoundingBox(new Vector3(center.X, center.Y, this.region.Min.Z), new Vector3(this.region.Max.X, this.region.Max.Y, center.Z));
            octant[6] = new BoundingBox(center, this.region.Max);
            octant[7] = new BoundingBox(new Vector3(this.region.Min.X, center.Y, center.Z), new Vector3(center.X, this.region.Max.Y, this.region.Max.Z));

            //This will contain all of our objects which fit within each respective octant.
            List<TObject>[] octList = new List<TObject>[8];
            for (int i = 0; i < 8; i++)
            {
                octList[i] = new List<TObject>();
            }

            //this list contains all of the objects which got moved down the tree and can be delisted from this node.
            List<TObject> delist = new List<TObject>();

            foreach (TObject obj in this.objects)
            {
                if (obj.BoundingBox.Min != obj.BoundingBox.Max)
                {
                    for (int a = 0; a < 8; a++)
                    {
                        if (octant[a].Contains(obj.BoundingBox) == ContainmentType.Contains)
                        {
                            octList[a].Add(obj);
                            delist.Add(obj);
                            break;
                        }
                    }
                }
                else if (!obj.BoundingSphere.Radius.Equals(0f))
                {
                    for (int a = 0; a < 8; a++)
                    {
                        if (octant[a].Contains(obj.BoundingSphere) == ContainmentType.Contains)
                        {
                            octList[a].Add(obj);
                            delist.Add(obj);
                            break;
                        }
                    }
                }
            }

            foreach (TObject obj in delist)
            {
                this.objects.Remove(obj);
            }

            //Create child nodes where there are items contained in the bounding region
            for (int a = 0; a < 8; a++)
            {
                if (octList[a].Count != 0)
                {
                    this.octants[a] = this.CreateNode(octant[a], octList[a]);
                    this.activeOctants |= (byte)(1 << a);
                    this.octants[a].BuildTree();
                }
            }

            this.treeBuilt = true;
            this.treeReady = true;
        }

        private OcTree<TObject> CreateNode(BoundingBox boundingBox, IEnumerable<TObject> objList) //complete & tested
        {
            if (!objList.Any())
            {
                return null;
            }

            OcTree<TObject> ret = new OcTree<TObject>(boundingBox, objList, this.MinimumSize);
            ret.parent = this;

            return ret;
        }

        private OcTree<TObject> CreateNode(BoundingBox boundingBox, TObject item)
        {
            OcTree<TObject> ret = new OcTree<TObject>(boundingBox, new[] { item }, this.MinimumSize);
            ret.parent = this;
            return ret;
        }

        private static IEnumerable<Intersection<TObject>> NoIntersections = new Intersection<TObject>[] { };

        private IEnumerable<Intersection<TObject>> GetIntersection(BoundingBox box)
        {
            if (this.objects.Count == 0 && this.HasChildren == false)
            {
                return NoIntersections;
            }

            List<Intersection<TObject>> ret = new List<Intersection<TObject>>();

            foreach (TObject obj in this.objects)
            {
                // test for intersection
                Intersection<TObject> ir = obj.Intersects(box);
                if (ir != null)
                {
                    ret.Add(ir);
                }
            }

            // test each object in the list for intersection
            for (int a = 0; a < 8; a++)
            {
                if (this.octants[a] == null)
                {
                    continue;
                }

                var octantRegion = this.octants[a].region;

                if((box.Contains(octantRegion) == ContainmentType.Intersects || box.Contains(octantRegion) == ContainmentType.Contains))
                {
                    IEnumerable<Intersection<TObject>> hitList = this.octants[a].GetIntersection(box);
                    ret.AddRange(hitList);
                }
            }
            return ret;
        }

        private IEnumerable<Intersection<TObject>> GetIntersection(BoundingFrustum frustum)
        {
            if (this.objects.Count == 0 && this.HasChildren == false)
            {
                return NoIntersections;
            }

            List<Intersection<TObject>> ret = new List<Intersection<TObject>>();

            foreach (TObject obj in this.objects)
            {
                // test for intersection
                Intersection<TObject> ir = obj.Intersects(frustum);
                if (ir != null)
                {
                    ret.Add(ir);
                }
            }

            // test each object in the list for intersection
            for (int a = 0; a < 8; a++)
            {
                if (this.octants[a] != null &&
                    (frustum.Contains(this.octants[a].region) == ContainmentType.Intersects ||
                     frustum.Contains(this.octants[a].region) == ContainmentType.Contains))
                {
                    IEnumerable<Intersection<TObject>> hitList = this.octants[a].GetIntersection(frustum);
                    ret.AddRange(hitList);
                }
            }
            return ret;
        }

        private IEnumerable<Intersection<TObject>> GetIntersection(Ray intersectRay)
        {
            if (this.objects.Count == 0 && this.HasChildren == false) //terminator for any recursion
            {
                return NoIntersections;
            }

            List<Intersection<TObject>> ret = new List<Intersection<TObject>>();

            //the ray is intersecting this region, so we have to check for intersection with all of our contained objects and child regions.

            //test each object in the list for intersection
            foreach (TObject obj in this.objects)
            {
                if (obj.BoundingBox.Intersects(intersectRay) != null)
                {
                    Intersection<TObject> ir = obj.Intersects(intersectRay);
                    if (ir.HasHit)
                    {
                        ret.Add(ir);
                    }
                }
            }

            // test each child octant for intersection
            for (int a = 0; a < 8; a++)
            {
                if (this.octants[a] != null && this.octants[a].region.Intersects(intersectRay) != null)
                {
                    IEnumerable<Intersection<TObject>> hits = this.octants[a].GetIntersection(intersectRay);
                    ret.AddRange(hits);
                }
            }

            return ret;
        }

        private void Insert(TObject item)
        {
            /*make sure we're not inserting an object any deeper into the tree than we have to.
                -if the current node is an empty leaf node, just insert and leave it.*/
            if (this.objects.Count <= 1 && this.activeOctants == 0)
            {
                this.objects.Add(item);
                return;
            }

            Vector3 dimensions = this.region.Max - this.region.Min;
            //Check to see if the dimensions of the box are greater than the minimum dimensions
            if (dimensions.X <= this.minimumSize && dimensions.Y <= this.minimumSize && dimensions.Z <= this.minimumSize)
            {
                this.objects.Add(item);
                return;
            }
            Vector3 half = dimensions / 2.0f;
            Vector3 center = this.region.Min + half;

            //Find or create subdivided regions for each octant in the current region
            BoundingBox[] childOctant = new BoundingBox[8];
            childOctant[0] = (this.octants[0] != null) ? this.octants[0].region : new BoundingBox(this.region.Min, center);
            childOctant[1] = (this.octants[1] != null)
                ? this.octants[1].region
                : new BoundingBox(new Vector3(center.X, this.region.Min.Y, this.region.Min.Z), new Vector3(this.region.Max.X, center.Y, center.Z));
            childOctant[2] = (this.octants[2] != null)
                ? this.octants[2].region
                : new BoundingBox(new Vector3(center.X, this.region.Min.Y, center.Z), new Vector3(this.region.Max.X, center.Y, this.region.Max.Z));
            childOctant[3] = (this.octants[3] != null)
                ? this.octants[3].region
                : new BoundingBox(new Vector3(this.region.Min.X, this.region.Min.Y, center.Z), new Vector3(center.X, center.Y, this.region.Max.Z));
            childOctant[4] = (this.octants[4] != null)
                ? this.octants[4].region
                : new BoundingBox(new Vector3(this.region.Min.X, center.Y, this.region.Min.Z), new Vector3(center.X, this.region.Max.Y, center.Z));
            childOctant[5] = (this.octants[5] != null)
                ? this.octants[5].region
                : new BoundingBox(new Vector3(center.X, center.Y, this.region.Min.Z), new Vector3(this.region.Max.X, this.region.Max.Y, center.Z));
            childOctant[6] = (this.octants[6] != null) ? this.octants[6].region : new BoundingBox(center, this.region.Max);
            childOctant[7] = (this.octants[7] != null)
                ? this.octants[7].region
                : new BoundingBox(new Vector3(this.region.Min.X, center.Y, center.Z), new Vector3(center.X, this.region.Max.Y, this.region.Max.Z));

            if (item.BoundingBox.Max != item.BoundingBox.Min && this.region.Contains(item.BoundingBox) == ContainmentType.Contains)
            {
                bool found = false;

                // we will try to place the object into a child node. If we can't fit it in a child node, then we insert it into the current node object list.
                for (int a = 0; a < 8; a++)
                {
                    //is the object fully contained within a quadrant?
                    if (childOctant[a].Contains(item.BoundingBox) == ContainmentType.Contains)
                    {
                        if (this.octants[a] != null)
                        {
                            this.octants[a].Insert(item); //Add the item into that tree and let the child tree figure out what to do with it
                        }
                        else
                        {
                            this.octants[a] = this.CreateNode(childOctant[a], item); //create a new tree node with the item
                            this.activeOctants |= (byte)(1 << a);
                        }
                        found = true;
                    }
                }
                if (!found)
                {
                    this.objects.Add(item);
                }
            }
            else if ((!item.BoundingSphere.Radius.Equals(0f)) && this.region.Contains(item.BoundingSphere) == ContainmentType.Contains)
            {
                bool found = false;
                //we will try to place the object into a child node. If we can't fit it in a child node, then we insert it into the current node object list.
                for (int a = 0; a < 8; a++)
                {
                    //is the object contained within a child quadrant?
                    if (childOctant[a].Contains(item.BoundingSphere) == ContainmentType.Contains)
                    {
                        if (this.octants[a] != null)
                        {
                            this.octants[a].Insert(item); //Add the item into that tree and let the child tree figure out what to do with it
                        }
                        else
                        {
                            this.octants[a] = this.CreateNode(childOctant[a], item); //create a new tree node with the item
                            this.activeOctants |= (byte)(1 << a);
                        }
                        found = true;
                    }
                }
                if (!found)
                {
                    this.objects.Add(item);
                }
            }
            else
            {
                //either the item lies outside of the enclosed bounding box or it is intersecting it. Either way, we need to rebuild
                //the entire tree by enlarging the containing bounding box
                this.BuildTree();
            }
        }

        private int NextPowerTwo(int v)
        {
            // first round down to one less than a power of 2
            v |= v >> 1;  
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;

            // Debruijn sequence to find most significant bit position
            int r = (int)this.debruijnPosition[(uint)(v * 0x07C4ACDDU) >> 27];

            // Shift MSB left to find next power 2.
            return 1 << (r + 1);
        }

        /// <summary>
        /// This finds the dimensions of the bounding box necessary to tightly enclose all items in the object list.
        /// </summary>
        private void SetEnclosingBox()
        {
            Vector3 globalMin = this.region.Min;
            Vector3 globalMax = this.region.Max;

            //go through all the objects in the list and find the extremes for their bounding areas.
            foreach (TObject obj in this.objects)
            {
                Vector3 localMin = Vector3.Zero;
                Vector3 localMax = Vector3.Zero;

                if (obj.BoundingBox.Max != obj.BoundingBox.Min)
                {
                    localMin = obj.BoundingBox.Min;
                    localMax = obj.BoundingBox.Max;
                }

                if (!obj.BoundingSphere.Radius.Equals(0f))
                {
                    localMin = new Vector3(
                        obj.BoundingSphere.Center.X - obj.BoundingSphere.Radius,
                        obj.BoundingSphere.Center.Y - obj.BoundingSphere.Radius,
                        obj.BoundingSphere.Center.Z - obj.BoundingSphere.Radius);

                    localMax = new Vector3(
                        obj.BoundingSphere.Center.X + obj.BoundingSphere.Radius,
                        obj.BoundingSphere.Center.Y + obj.BoundingSphere.Radius,
                        obj.BoundingSphere.Center.Z + obj.BoundingSphere.Radius);
                }

                if (localMin.X < globalMin.X)
                {
                    globalMin.X = localMin.X;
                }
                if (localMin.Y < globalMin.Y)
                {
                    globalMin.Y = localMin.Y;
                }
                if (localMin.Z < globalMin.Z)
                {
                    globalMin.Z = localMin.Z;
                }

                if (localMax.X > globalMax.X)
                {
                    globalMax.X = localMax.X;
                }
                if (localMax.Y > globalMax.Y)
                {
                    globalMax.Y = localMax.Y;
                }
                if (localMax.Z > globalMax.Z)
                {
                    globalMax.Z = localMax.Z;
                }
            }

            this.region.Min = globalMin;
            this.region.Max = globalMax;
        }

        /// <summary>
        /// This finds the smallest enclosing cube which is a power of 2, for all objects in the list.
        /// </summary>
        private void SetEnclosingCube()
        {
            this.SetEnclosingBox();

            //find the min offset from (0,0,0) and translate by it for a short while
            Vector3 offset = this.region.Min - Vector3.Zero;
            this.region.Min += offset;
            this.region.Max += offset;

            //find the nearest power of two for the max values
            int highX = (int)Math.Floor(Math.Max(Math.Max(this.region.Max.X, this.region.Max.Y), this.region.Max.Z));

            //see if we're already at a power of 2
            for (int bit = 0; bit < 32; bit++)
            {
                if (highX == 1 << bit)
                {
                    this.region.Max = new Vector3(highX, highX, highX);

                    this.region.Min -= offset;
                    this.region.Max -= offset;
                    return;
                }
            }

            //gets the most significant bit value, so that we essentially do a Ceiling(X) with the 
            //ceiling result being to the nearest power of 2 rather than the nearest integer.
            int x = this.NextPowerTwo(highX);

            this.region.Max = new Vector3(x, x, x);

            this.region.Min -= offset;
            this.region.Max -= offset;
        }
    }
}