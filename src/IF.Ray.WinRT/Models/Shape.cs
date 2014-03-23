using System.Collections.Generic;
using CjClutter.ObjLoader.WinRT.Data;
using CjClutter.ObjLoader.WinRT.Data.Elements;
using CjClutter.ObjLoader.WinRT.Data.VertexData;

namespace IF.Ray.WinRT.Models
{
    public abstract class Shape
    {
        public IList<Vertex> Vertices { get; set; }
        public IList<Texture> Textures { get; set; }
        public IList<Normal> Normals { get; set; }
        public IList<Group> Groups { get; set; }
        public IList<Material> Materials { get; set; }

    }

    public class Cube : Shape
    {
    }

    public class Cylinder : Shape
    {
    }
}
