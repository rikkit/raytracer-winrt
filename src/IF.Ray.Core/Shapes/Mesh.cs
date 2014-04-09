using System.Collections.Generic;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public abstract class Mesh : IOccluder
    {
        private Shader _shader;
        public IList<Triangle> Triangles { get; set; }

        public Shader Shader
        {
            get { return _shader; }
            set
            {
                _shader = value;

                foreach (var triangle in Triangles)
                {
                    triangle.Shader = value;
                }
            }
        }

        public virtual IList<ZBufferItem> Trace(Shapes.Ray ray, Matrix transform, Vector3 translation)
        {
            var intersections = new List<ZBufferItem>();

            foreach (var primitive in Triangles)
            {
                var items = primitive.Trace(ray, transform, translation);
                intersections.AddRange(items);
            }

            return intersections;
        }

        public Color Colorise(Scene scene, Ray ray, Vector3 translation, Vector3 intersection)
        {
            // this is never called
            return Color.BlanchedAlmond;
        }
    }
}
