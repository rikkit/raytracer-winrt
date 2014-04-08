using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public class Plane : IOccluder
    {
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
            // distance of the plane from origin
            var d = Vector3.Distance(Position + translation, Vector3.Zero);

            var numerator = -d - Vector3.Dot(ray.Origin, Normal);
            var denominator = Vector3.Dot(ray.Direction, Normal);

            var list = new List<ZBufferItem>();
            if (Math.Abs(numerator) < float.Epsilon && Math.Abs(denominator) < float.Epsilon)
            {
                // ray doesn't intersect plane
            }
            else
            {
                var t = numerator/denominator;
                var rayDir = ray.Direction;
                if (!rayDir.IsNormalized) rayDir.Normalize();
                var p = ray.Origin + (rayDir * t);

                // the ray intersects the plane, now check if p is in bounds

                if (InBounds(p))
                {
                    var z = new ZBufferItem(this, p, translation);
                    list.Add(z);
                }

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

        public static Plane XY = new Plane(Vector3.Zero, Vector3.UnitZ);

        /// <summary>
        /// Tests if the provided point is in the bounds of the plane
        /// Plane can only be square (provide the width) atm
        /// </summary>
        /// <param name="v">Point to test</param>
        /// <returns>Whether point is on the plane</returns>
        private bool InBounds(Vector3 v)
        {
            // assuming v is already on the plane
            // project the plane to XY
            var pProj = Plane.Project(XY, Position);
            
            // project v to XY
            var vProj = Plane.Project(XY, v);

            // now test if testV is in bounds
            var hw = Width/2;
            var bounds = new Vector3[4]
            {
                Vector3.Add(pProj, new Vector3(-hw, +hw, 0)),
                Vector3.Add(pProj, new Vector3(+hw, +hw, 0)),
                Vector3.Add(pProj, new Vector3(+hw, -hw, 0)),
                Vector3.Add(pProj, new Vector3(-hw, -hw, 0))
            };

            // make triangles and get the areas..
            // ABP BCP CDP DAP
            var areas = bounds.Select((b, i) => TwiceTriangleArea(bounds[i%4], bounds[(i + 1)%4], vProj)).ToList();

            var sum = areas.Sum();

            return areas.All(a => a > 0);
        }

        /// <summary>
        /// Returns twice the area of a triangle made by these three points
        /// </summary>
        private float TwiceTriangleArea(Vector3 a, Vector3 b, Vector3 p)
        {
            return (p.X*b.Y - b.X*p.Y) - (p.X*a.Y - a.X*p.Y) + (b.X*a.Y - a.X*b.Y);
        }

        public void Reset()
        {
        }

        public void TranslateTo(Vector3 position)
        {
            Position = position;
        }
        
        public Color Colorise(IEnumerable<Light> lights, SharpDX.Ray ray, Vector3 point)
        {
            return new Color(0.5f,0f,0f,0.1f);
        }
    }
}