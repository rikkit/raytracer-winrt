using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public class Plane : IOccluder
    {
        public static Plane XY = new Plane(Vector3.Zero, Vector3.UnitX, Vector3.UnitY);

        public Vector3[] _points;

        private Vector3 _normal;

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

        public Vector3 Position
        {
            get { return _points[0]; }
        }

        public float Width { get; set; }
        
        /// <summary>
        /// Define a plane using three points
        /// First one is treated as the origin
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public Plane(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            _points = new[] {p1, p2, p3};
            Width = 5;
        }

        public IList<ZBufferItem> Trace(Ray ray, Vector3 translation)
        {
            // move this plane to the origin, now pProj is on the XY plane
            var planeVector = Position + translation;
            var hw = Width/2;

            // calculate bounds on the xy plane, 
            // and project each one back to our original plane
            // then translate
            var b1 = BarycentricToCartesian(new Vector3(-hw, +hw, 1));
            var b2 = BarycentricToCartesian(new Vector3(+hw, +hw, 1));
            var b3 = BarycentricToCartesian(new Vector3(+hw, -hw, 1));
            var b4 = BarycentricToCartesian(new Vector3(-hw, -hw, 1));
            
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

        private Vector3 BarycentricToCartesian(Vector3 b)
        {
            // barycentric.x * p0 + barycentric.y * p1 + barycentric.z * p2;

            return b.X * _points[0] + b.Y * _points[1] + b.Z * _points[2];
        }
        
        public Color Colorise(IEnumerable<Light> lights, SharpDX.Ray ray, Vector3 point)
        {
            return new Color(0.5f,0f,0f,0.1f);
        }
    }
}