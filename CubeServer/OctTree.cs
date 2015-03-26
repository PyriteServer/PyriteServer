// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="OctTree.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.Xna.Framework;

	public class OctTree<TObject> where TObject : IBounds<TObject>
	{
		private const int DEFAULT_MIN_SIZE = 1;

		private readonly OctTree<TObject>[] childNodes = new OctTree<TObject>[8];

		private readonly uint[] debruijnPosition =
		{
			0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30, 8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26,
			5, 4, 31
		};

		private readonly Queue<TObject> insertionQueue = new Queue<TObject>();

		private readonly int minimumSize = DEFAULT_MIN_SIZE;
		private readonly List<TObject> objects;

		private byte activeNodes = 0;

		private OctTree<TObject> parent;
		private BoundingBox region;
		private bool treeBuilt = false;
		private bool treeReady = false;

		public OctTree() : this(new BoundingBox(Vector3.Zero, Vector3.Zero))
		{
		}

		public OctTree(BoundingBox region) : this(region, new TObject[] { })
		{
		}

		public OctTree(BoundingBox region, IEnumerable<TObject> objList) : this(region, objList, DEFAULT_MIN_SIZE)
		{
		}

		public OctTree(BoundingBox region, IEnumerable<TObject> objList, int minSize)
		{
			this.region = region;
			this.objects = new List<TObject>(objList);
			this.minimumSize = minSize;
		}

		public OctTree<TObject>[] Child
		{
			get { return this.childNodes; }
		}

		public byte ChildMask
		{
			get { return this.activeNodes; }
		}

		public bool HasChildren
		{
			get { return this.activeNodes != 0; }
		}

		public bool IsRoot
		{
			get { return this.parent == null; }
		}

		public IList<TObject> Objects
		{
			get { return this.objects; }
		}

		public BoundingBox Region
		{
			get { return this.region; }
		}

		private bool IsEmpty //untested
		{
			get
			{
				if (this.objects.Count != 0)
				{
					return false;
				}

				for (int a = 0; a < 8; a++)
				{
					//note that we have to do this recursively. 
					//Just checking child nodes for the current node doesn't mean that their children won't have objects.
					if (this.childNodes[a] != null && !this.childNodes[a].IsEmpty)
					{
						return false;
					}
				}

				return true;
			}
		}

		public override string ToString()
		{
			return String.Format("Region:{0} Children:{1}b Objects:{2}", this.Region, Convert.ToString(this.activeNodes, 2).PadLeft(8,'0'), this.objects.Count);
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

		public Intersection<TObject> NearestIntersection(Ray ray)
		{
			if (!this.treeReady)
			{
				this.UpdateTree();
			}

			IEnumerable<Intersection<TObject>> intersections = GetIntersection(ray);

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
				else if (obj.BoundingSphere.Radius != 0)
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

			//delist every moved object from this node.
			foreach (TObject obj in delist)
			{
				this.objects.Remove(obj);
			}

			//Create child nodes where there are items contained in the bounding region
			for (int a = 0; a < 8; a++)
			{
				if (octList[a].Count != 0)
				{
					this.childNodes[a] = CreateNode(octant[a], octList[a]);
					this.activeNodes |= (byte)(1 << a);
					this.childNodes[a].BuildTree();
				}
			}

			this.treeBuilt = true;
			this.treeReady = true;
		}

		private OctTree<TObject> CreateNode(BoundingBox region, IEnumerable<TObject> objList) //complete & tested
		{
			if (!objList.Any())
			{
				return null;
			}

			OctTree<TObject> ret = new OctTree<TObject>(region, objList);
			ret.parent = this;

			return ret;
		}

		private OctTree<TObject> CreateNode(BoundingBox region, TObject item)
		{
			OctTree<TObject> ret = new OctTree<TObject>(region, new[] { item });
			ret.parent = this;
			return ret;
		}

		private IEnumerable<Intersection<TObject>> GetIntersection(BoundingFrustum frustum)
		{
			if (this.objects.Count == 0 && this.HasChildren == false)
			{
				return null;
			}

			List<Intersection<TObject>> ret = new List<Intersection<TObject>>();

			foreach (TObject obj in this.objects)
			{
				//test for intersection
				Intersection<TObject> ir = obj.Intersects(frustum);
				if (ir != null)
				{
					ret.Add(ir);
				}
			}

			//test each object in the list for intersection
			for (int a = 0; a < 8; a++)
			{
				if (this.childNodes[a] != null &&
					(frustum.Contains(this.childNodes[a].region) == ContainmentType.Intersects ||
					 frustum.Contains(this.childNodes[a].region) == ContainmentType.Contains))
				{
					IEnumerable<Intersection<TObject>> hitList = this.childNodes[a].GetIntersection(frustum);
					if (hitList != null)
					{
						foreach (Intersection<TObject> ir in hitList)
						{
							ret.Add(ir);
						}
					}
				}
			}
			return ret;
		}

		private IEnumerable<Intersection<TObject>> GetIntersection(Ray intersectRay)
		{
			if (this.objects.Count == 0 && this.HasChildren == false) //terminator for any recursion
			{
				return null;
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
				if (this.childNodes[a] != null && this.childNodes[a].region.Intersects(intersectRay) != null)
				{
					IEnumerable<Intersection<TObject>> hits = this.childNodes[a].GetIntersection(intersectRay);
					if (hits != null)
					{
						foreach (Intersection<TObject> ir in hits)
						{
							ret.Add(ir);
						}
					}
				}
			}

			return ret;
		}

		private IEnumerable<Intersection<TObject>> GetIntersection(IEnumerable<TObject> parentObjs)
		{
			List<Intersection<TObject>> intersections = new List<Intersection<TObject>>();
			//assume all parent objects have already been processed for collisions against each other.
			//check all parent objects against all objects in our local node
			foreach (TObject pObj in parentObjs)
			{
				foreach (TObject lObj in this.objects)
				{
					//We let the two objects check for collision against each other. They can figure out how to do the coarse and granular checks.
					//all we're concerned about is whether or not a collision actually happened.
					Intersection<TObject> ir = pObj.Intersects(lObj);
					if (ir != null)
					{
						if (intersections.Contains(ir))
						{
							int a = 0;
							a++;
						}
						intersections.Add(ir);
					}
				}
			}

			//now, check all our local objects against all other local objects in the node
			if (this.objects.Count > 1)
			{
				#region self-congratulation

				/*
				 * This is a rather brilliant section of code. Normally, you'd just have two foreach loops, like so:
				 * foreach(TObject lObj1 in m_objects)
				 * {
				 *      foreach(TObject lObj2 in m_objects)
				 *      {
				 *           //intersection check code
				 *      }
				 * }
				 * 
				 * The problem is that this runs in O(N*N) time and that we're checking for collisions with objects which have already been checked.
				 * Imagine you have a set of four items: {1,2,3,4}
				 * You'd first check: {1} vs {1,2,3,4}
				 * Next, you'd check {2} vs {1,2,3,4}
				 * but we already checked {1} vs {2}, so it's a waste to check {2} vs. {1}. What if we could skip this check by removing {1}?
				 * We'd have a total of 4+3+2+1 collision checks, which equates to O(N(N+1)/2) time. If N is 10, we are already doing half as many collision checks as necessary.
				 * Now, we can't just remove an item at the end of the 2nd for loop since that would break the iterator in the first foreach loop, so we'd have to use a
				 * regular for(int i=0;i<size;i++) style loop for the first loop and reduce size each iteration. This works...but look at the for loop: we're allocating memory for
				 * two additional variables: i and size. What if we could figure out some way to eliminate those variables?
				 * So, who says that we have to start from the front of a list? We can start from the back end and still get the same end results. With this in mind,
				 * we can completely get rid of a for loop and use a while loop which has a conditional on the capacity of a temporary list being greater than 0.
				 * since we can poll the list capacity for free, we can use the capacity as an indexer into the list items. Now we don't have to increment an indexer either!
				 * The result is below.
				 */

				#endregion

				List<TObject> tmp = new List<TObject>(this.objects.Count);
				tmp.AddRange(this.objects);
				while (tmp.Count > 0)
				{
					foreach (TObject lObj2 in tmp)
					{
						if (tmp[tmp.Count - 1].Equals(lObj2))
						{
							continue;
						}

						Intersection<TObject> ir = tmp[tmp.Count - 1].Intersects(lObj2);
						if (ir != null)
						{
							intersections.Add(ir);
						}
					}

					//remove this object from the temp list so that we can run in O(N(N+1)/2) time instead of O(N*N)
					tmp.RemoveAt(tmp.Count - 1);
				}
			}

			IEnumerable<TObject> allObjects = parentObjs.Concat(this.objects);

			//each child node will give us a list of intersection records, which we then merge with our own intersection records.
			for (int flags = this.activeNodes, index = 0; flags > 0; flags >>= 1, index++)
			{
				if ((flags & 1) == 1)
				{
					intersections.AddRange(this.childNodes[index].GetIntersection(allObjects));
				}
			}

			return intersections;
		}

		/// <summary>
		/// A tree has already been created, so we're going to try to insert an item into the tree without rebuilding the whole thing
		/// </summary>
		/// <typeparam name="T">A physical object</typeparam>
		/// <param name="item">The physical object to insert into the tree</param>
		private void Insert(TObject item)
		{
			/*make sure we're not inserting an object any deeper into the tree than we have to.
				-if the current node is an empty leaf node, just insert and leave it.*/
			if (this.objects.Count <= 1 && this.activeNodes == 0)
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
			childOctant[0] = (this.childNodes[0] != null) ? this.childNodes[0].region : new BoundingBox(this.region.Min, center);
			childOctant[1] = (this.childNodes[1] != null)
				? this.childNodes[1].region
				: new BoundingBox(new Vector3(center.X, this.region.Min.Y, this.region.Min.Z), new Vector3(this.region.Max.X, center.Y, center.Z));
			childOctant[2] = (this.childNodes[2] != null)
				? this.childNodes[2].region
				: new BoundingBox(new Vector3(center.X, this.region.Min.Y, center.Z), new Vector3(this.region.Max.X, center.Y, this.region.Max.Z));
			childOctant[3] = (this.childNodes[3] != null)
				? this.childNodes[3].region
				: new BoundingBox(new Vector3(this.region.Min.X, this.region.Min.Y, center.Z), new Vector3(center.X, center.Y, this.region.Max.Z));
			childOctant[4] = (this.childNodes[4] != null)
				? this.childNodes[4].region
				: new BoundingBox(new Vector3(this.region.Min.X, center.Y, this.region.Min.Z), new Vector3(center.X, this.region.Max.Y, center.Z));
			childOctant[5] = (this.childNodes[5] != null)
				? this.childNodes[5].region
				: new BoundingBox(new Vector3(center.X, center.Y, this.region.Min.Z), new Vector3(this.region.Max.X, this.region.Max.Y, center.Z));
			childOctant[6] = (this.childNodes[6] != null) ? this.childNodes[6].region : new BoundingBox(center, this.region.Max);
			childOctant[7] = (this.childNodes[7] != null)
				? this.childNodes[7].region
				: new BoundingBox(new Vector3(this.region.Min.X, center.Y, center.Z), new Vector3(center.X, this.region.Max.Y, this.region.Max.Z));

			//First, is the item completely contained within the root bounding box?
			//note2: I shouldn't actually have to compensate for this. If an object is out of our predefined bounds, then we have a problem/error.
			//          Wrong. Our initial bounding box for the terrain is constricting its height to the highest peak. Flying units will be above that.
			//             Fix: I resized the enclosing box to 256x256x256. This should be sufficient.
			if (item.BoundingBox.Max != item.BoundingBox.Min && this.region.Contains(item.BoundingBox) == ContainmentType.Contains)
			{
				bool found = false;
				//we will try to place the object into a child node. If we can't fit it in a child node, then we insert it into the current node object list.
				for (int a = 0; a < 8; a++)
				{
					//is the object fully contained within a quadrant?
					if (childOctant[a].Contains(item.BoundingBox) == ContainmentType.Contains)
					{
						if (this.childNodes[a] != null)
						{
							this.childNodes[a].Insert(item); //Add the item into that tree and let the child tree figure out what to do with it
						}
						else
						{
							this.childNodes[a] = CreateNode(childOctant[a], item); //create a new tree node with the item
							this.activeNodes |= (byte)(1 << a);
						}
						found = true;
					}
				}
				if (!found)
				{
					this.objects.Add(item);
				}
			}
			else if (item.BoundingSphere.Radius != 0f && this.region.Contains(item.BoundingSphere) == ContainmentType.Contains)
			{
				bool found = false;
				//we will try to place the object into a child node. If we can't fit it in a child node, then we insert it into the current node object list.
				for (int a = 0; a < 8; a++)
				{
					//is the object contained within a child quadrant?
					if (childOctant[a].Contains(item.BoundingSphere) == ContainmentType.Contains)
					{
						if (this.childNodes[a] != null)
						{
							this.childNodes[a].Insert(item); //Add the item into that tree and let the child tree figure out what to do with it
						}
						else
						{
							this.childNodes[a] = CreateNode(childOctant[a], item); //create a new tree node with the item
							this.activeNodes |= (byte)(1 << a);
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
			v |= v >> 1; // first round down to one less than a power of 2 
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;

			int r = (int)this.debruijnPosition[(uint)(v * 0x07C4ACDDU) >> 27];

			return 1 << (r + 1);
		}

		/// <summary>
		/// This finds the dimensions of the bounding box necessary to tightly enclose all items in the object list.
		/// </summary>
		private void SetEnclosingBox()
		{
			Vector3 global_min = this.region.Min, global_max = this.region.Max;

			//go through all the objects in the list and find the extremes for their bounding areas.
			foreach (TObject obj in this.objects)
			{
				Vector3 local_min = Vector3.Zero, local_max = Vector3.Zero;

				if (obj.BoundingBox.Max != obj.BoundingBox.Min)
				{
					local_min = obj.BoundingBox.Min;
					local_max = obj.BoundingBox.Max;
				}

				if (obj.BoundingSphere.Radius != 0.0f)
				{
					local_min = new Vector3(
						obj.BoundingSphere.Center.X - obj.BoundingSphere.Radius,
						obj.BoundingSphere.Center.Y - obj.BoundingSphere.Radius,
						obj.BoundingSphere.Center.Z - obj.BoundingSphere.Radius);

					local_max = new Vector3(
						obj.BoundingSphere.Center.X + obj.BoundingSphere.Radius,
						obj.BoundingSphere.Center.Y + obj.BoundingSphere.Radius,
						obj.BoundingSphere.Center.Z + obj.BoundingSphere.Radius);
				}

				if (local_min.X < global_min.X)
				{
					global_min.X = local_min.X;
				}
				if (local_min.Y < global_min.Y)
				{
					global_min.Y = local_min.Y;
				}
				if (local_min.Z < global_min.Z)
				{
					global_min.Z = local_min.Z;
				}

				if (local_max.X > global_max.X)
				{
					global_max.X = local_max.X;
				}
				if (local_max.Y > global_max.Y)
				{
					global_max.Y = local_max.Y;
				}
				if (local_max.Z > global_max.Z)
				{
					global_max.Z = local_max.Z;
				}
			}

			this.region.Min = global_min;
			this.region.Max = global_max;
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