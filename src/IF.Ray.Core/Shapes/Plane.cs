using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public class Plane : IOccluder
    {
        public static Plane XY = new Plane(Vector3.Zero, Vector3.UnitZ);

        private Vector3 _normal;
        public Vector3 Position { get; set; }

        /// <summary>
        /// Plane normal, setting ensures it is in normalised form
        /// </summary>
        public Vector3 Normal
        {
            get { return _normal; }
            set
            {
                if (!value.IsNormalized)
                {
                    value.Normalize();
                }

                _normal = value;
            }
        }

        public float Width { get; set; }

        public Plane()
        {
            Width = 5;
        }

        public Plane(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
            Width = 5;
        }

        public IList<ZBufferItem> Trace(Ray ray, Vector3 translation)
        {
            // project the plane to XY
            var pProj = Plane.Project(XY, Position + translation);
            var hw = Width/2;

            // calculate bounds on the xy plane, 
            // and project each one back to our original plane
            var b1 = Project(this, Vector3.Add(pProj, new Vector3(-hw, +hw, 0)));
            var b2 = Project(this, Vector3.Add(pProj, new Vector3(+hw, +hw, 0)));
            var b3 = Project(this, Vector3.Add(pProj, new Vector3(+hw, -hw, 0)));
            var b4 = Project(this, Vector3.Add(pProj, new Vector3(-hw, -hw, 0)));
            
            // let's reuse the triangle intersection code
            var t1 = new Triangle(b1, b2, b3);
            var t2 = new Triangle(b3, b4, b1);

            var tx = t1.Trace(ray, Vector3.Zero).ToList();
            tx.AddRange(t2.Trace(ray, Vector3.Zero));

            var list = new List<ZBufferItem>();
            if (tx.Any())
            {
                // there can only be one intersection
                var intersection = tx.First().Intersection;
                var z = new ZBufferItem(this, intersection, translation);
                list.Add(z);
            }

            return list;
        }

        public Color Colorise(Scene scene, Ray ray, Vector3 translation, Vector3 intersection)
        {
            return Color.Blue;
        }

        private static Vector3 Project(Plane plane, Vector3 v)
        {
            var n = plane.Normal;
            var p = plane.Position;
            var numerator = n.X*p.X - n.X*v.X + n.Y*p.Y - n.Y*v.Y + n.Z*p.Z - n.Z*v.Z;
            var denominator = v.X*v.X + v.Y*v.Y + v.Z*v.Z;

            var t = numerator/denominator;

            return new Vector3
            {
                X = v.X + t*n.X,
                Y = v.Y + t*n.Y,
                Z = v.Z + t*n.Z
            };
        }
        
        public Color Colorise(IEnumerable<Light> lights, SharpDX.Ray ray, Vector3 point)
        {
            return new Color(0.5f,0f,0f,0.1f);
        }
    }
}