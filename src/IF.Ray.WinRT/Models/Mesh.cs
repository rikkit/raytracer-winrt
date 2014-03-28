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

    public class Triangle : Mesh
    {
        public Vector3[] Vertices { get; set; }
        public Vector3[] Normals { get; set; }

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vertices = new[] {v0, v1, v2};
        }

        public void SetNormals(Vector3 n0, Vector3 n1, Vector3 n2)
        {
            Normals = new[] {n0, n1, n2};
        }

        public Vector3[] ToWorldCoordinates(Vector3 position)
        {
            return Vertices.Select((v, i) => v + position[i]).ToArray();
        }
    }
}
