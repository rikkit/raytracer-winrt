using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public class Triangle : IOccluder
    {
        private readonly Vector3[] _vertices;

        public Shader Shader { get; set; }

        public Vector3[] Vertices {
            get { return _vertices; }
        }
        public Vector3 Normal { get; private set; }

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 n)
        {
            _vertices = new[] {v0, v1, v2};
            Normal = n;
        }

        /// <summary>
        /// Möller–Trumbore ray-triangle intersection algorithm
        /// </summary>
        /// <param name="ray">ray to test</param>
        /// <param name="transform">world rotation transform</param>
        /// <param name="translation">translation vector</param>
        /// <returns>list of intersections</returns>
        public List<ZBufferItem> Trace(Ray ray, Matrix transform, Vector3 translation)
        {
            var buffer = new List<ZBufferItem>();
            
            var ts = Vertices.Select(vertex => Vector3.TransformCoordinate(vertex, transform)).Select(vertex => vertex + translation).ToArray();

            // find vectors for two edges sharing ts[0]
            var edge1 = ts[1] - ts[0];
            var edge2 = ts[2] - ts[0];

            var cross = Vector3.Cross(ray.Direction, edge2);
            var det = Vector3.Dot(edge1, cross);

            // if det is near 0 then ray is in the plane of the triangle i.e. parallel
            if (det > -float.Epsilon && det < float.Epsilon)
            {
                return buffer;
            }

            var invDet = 1f/det;

            var T = ray.Origin - ts[0];
            var u = Vector3.Dot(T, cross)*invDet;

            // if u is not 0->1 then intersection is outside the triangle
            if (u < 0f || u > 1f)
            {
                return buffer;
            }

            var Q = Vector3.Cross(T, edge1);
            var v = Vector3.Dot(ray.Direction, Q)*invDet;

            // then intersection is outside the triangle
            if (v < 0f || u + v > 1f)
            {
                return new List<ZBufferItem>();
            }

            var t = Vector3.Dot(edge2, Q)*invDet;

            // ray intersection!
            if (t > float.Epsilon)
            {
                // get the point of intersection
                var ix = ray.Origin + t*ray.Direction;
                buffer.Add(new ZBufferItem(this, ix, translation));
            }

            return buffer;
        }

        /// <summary>
        /// Get the colour at this point on this triangle
        /// </summary>
        public Color Colorise(Scene scene, Shapes.Ray ray, Matrix transform, Vector3 translation, Vector3 intersection)
        {
            var colour = new Color();
            
            foreach (var light in scene.Lights)
            {
                // get the vector between the light and the point
                var lightv = Vector3.Normalize(intersection - light.Position);
                var distance = Vector3.Distance(intersection, light.Position);

                // get the texture colour
                var a = Shader.Ambient(light, distance);

                colour += a;

                var lightraydir = light.Position - intersection;
                lightraydir.Normalize();

                // don't intersect with the object i'm on
                var lightrayorigin = intersection + 1*lightraydir;
                var lightray = new Ray(lightrayorigin, lightraydir);

                var lightx = scene.Trace(lightray, transform, translation);

                if (!lightx.Any())
                {
                    var l = Shader.Lambertian(Normal, light, lightv, distance);
                    var s = Shader.Specular(Normal, ray.Direction, light, lightv, distance);

                    colour += l;
                    colour += s;
                }
            }
            
            return colour;
        }
    }
}