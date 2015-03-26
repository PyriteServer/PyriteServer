// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="IntersectionRecord.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    using Microsoft.Xna.Framework;

    public class IntersectionRecord<TObject>
    {
        private readonly bool m_hasHit = false;

        public IntersectionRecord()
        {
            this.Position = Vector3.Zero;
            this.Normal = Vector3.Zero;
            this.Ray = new Ray();
            this.Distance = float.MaxValue;
            this.Object = default(TObject);
        }

        public IntersectionRecord(Vector3 hitPos, Vector3 hitNormal, Ray ray, double distance)
        {
            this.Position = hitPos;
            this.Normal = hitNormal;
            this.Ray = ray;
            this.Distance = distance;
            this.m_hasHit = true;
        }

        public IntersectionRecord(TObject hitObject = default(TObject))
        {
            this.m_hasHit = hitObject != null;
            this.Object = hitObject;
            this.Position = Vector3.Zero;
            this.Normal = Vector3.Zero;
            this.Ray = new Ray();
            this.Distance = 0.0f;
        }

        public double Distance { get; private set; }

        public bool HasHit
        {
            get { return this.m_hasHit; }
        }

        public Vector3 Normal { get; private set; }
        public TObject Object { get; set; }
        public TObject OtherObject { get; set; }
        public Vector3 Position { get; private set; }
        public Ray Ray { get; private set; }

        public override bool Equals(object otherRecord)
        {
            IntersectionRecord<TObject> o = (IntersectionRecord<TObject>)otherRecord;
            return o.Equals(this);
        }
    }
}