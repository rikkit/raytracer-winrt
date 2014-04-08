using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public class Triangle : IOccluder
    {
        private readonly Vector3[] _vertices;

        public Vector3[] Vertices {
            get { return _vertices; }
        }
        public Vector3[] Normals { get; private set; }

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            _vertices = new[] {v0, v1, v2};
        }
        
        public void SetNormals(Vector3 n0, Vector3 n1, Vector3 n2)
        {
            Normals = new[] {n0, n1, n2};
        }

        /// <summary>
        /// Möller–Trumbore ray-triangle intersection algorithm
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        public IList<ZBufferItem> Trace(Shapes.Ray ray, Vector3 translation)
        {
            var buffer = new List<ZBufferItem>();

            var ts = Vertices.Select(vertex => vertex + translation).ToArray();

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
                return new ZBufferItem[0];
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
        public Color Colorise(Scene scene, Shapes.Ray ray, Vector3 translation, Vector3 intersection)
        {
            var colour = new Color();
            var ts = Vertices.Select(vertex => vertex + translation).ToArray();

            foreach (var light in scene.Lights)
            {
                // get the vector between the light and the point
                var lightv = Vector3.Normalize(intersection - light.Position);

                var edge1 = ts[1] - ts[0];
                var edge2 = ts[2] - ts[0];

                var normal = Vector3.Cross(edge1, edge2); // TODO take into account point normals
                normal.Normalize();

                // get the angle between the lightray and the normal of the surface... lambertian reflection
                var cosAngle = Vector3.Dot(lightv, normal);

                // get the texture colour
                var triangleColour = Color.Red;

                var scaledColour = triangleColour * cosAngle;

                colour += scaledColour;
            }

            return colour;
        }
    }
}