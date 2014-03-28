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
        public Vector3[] Vertices { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Plane Plane { get; private set; }

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vertices = new[] {v0, v1, v2};
            Plane = new Plane(v0, v1, v2);
        }

        public void SetNormals(Vector3 n0, Vector3 n1, Vector3 n2)
        {
            Normals = new[] {n0, n1, n2};
        }

        public Vector3[] ToWorldCoordinates(Vector3 position)
        {
            return Vertices.Select((v, i) => v + position[i]).ToArray();
        }

        public Triangle TranslateTo(Vector4 position)
        {
            var vs = this.Vertices.Select(v => new Vector3(v.X + position.X, v.Y + position.Y, v.Z + position.Z)).ToArray();
            var triangle = new Triangle(vs[0], vs[1], vs[2]);
            triangle.SetNormals(Normals[0], Normals[1], Normals[2]);
            return triangle;
        }

        public bool Intersects(SharpDX.Ray ray, out Vector3 point)
        {
            return Plane.Intersects(ref ray, out point);
        }
    }
}
