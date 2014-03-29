using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace IF.Ray.WinRT.Models
{
    public abstract class Mesh
    {
        public IList<Triangle> Triangles { get; set; }
    }

    public class Cube : Mesh
    {
    }

    public class Cylinder : Mesh
    {
    }

    public class Triangle
    {
        private readonly Vector3[] _vertices;

        public Vector3[] Vertices {
            get { return _vertices; }
        }
        public Vector3[] Transformed { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Plane Plane { get; private set; }

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            _vertices = new[] {v0, v1, v2};
            Reset();
        }

        public void Reset()
        {
            Transformed = _vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
            Plane = new Plane(_vertices[0], _vertices[1], _vertices[2]);
        }

        public void SetNormals(Vector3 n0, Vector3 n1, Vector3 n2)
        {
            Normals = new[] {n0, n1, n2};
        }

        public Vector3[] ToWorldCoordinates(Vector3 position)
        {
            return Vertices.Select((v, i) => v + position[i]).ToArray();
        }

        public Triangle TranslateTo(Vector3 position)
        {
            Transformed = Vertices.Select(v => new Vector3(v.X + position.X, v.Y + position.Y, v.Z + position.Z)).ToArray();
            return this;
        }

        public bool Intersects(SharpDX.Ray ray, out Vector3 point)
        {
            var intersects = ray.Intersects(ref Transformed[0], ref Transformed[1], ref Transformed[2]);

            point = Vector3.Zero;
            if (intersects)
            {
                Plane.Intersects(ref ray, out point);
            }

            return intersects;
        }


        public Triangle Transform(Matrix m)
        {
            var transformed = Transformed.Select(t => Vector3.TransformCoordinate(t, m));
            Transformed = transformed.ToArray();
            return this;
        }

        /// <summary>
        /// Get the colour at this point on this triangle
        /// </summary>
        /// <param name="scene">The scene this triangle is part of, for the lights</param>
        /// <param name="ray">The source ray</param>
        /// <param name="point">The point on the triangle</param>
        /// <returns>Colour at the point</returns>
        public Color Colorise(IEnumerable<Light> lights, SharpDX.Ray ray, Vector3 point)
        {
            var colour = new Color();

            foreach (var light in lights)
            {
                // get the vector between the light and the point
                var lightv = Vector3.Normalize(point - light.Position);
                var camerav = Vector3.Normalize(ray.Direction);

                // get the angle between the lightray and the cameraray
                var cosAngle = Vector3.Dot(lightv, camerav);

                // get the texture colour
                var triangleColour = Color.Blue;

                var scaledColour = triangleColour*cosAngle;

                colour += scaledColour;
            }

            return colour;
        }
    }
}
