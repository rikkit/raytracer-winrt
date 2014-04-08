using System.Collections.Generic;
using System.Linq;
using IF.Common.Metro.Properties;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public abstract class Mesh : IOccluder
    {
        public IList<Triangle> Triangles { get; set; }

        public virtual IList<ZBufferItem> Trace(Shapes.Ray ray, Vector3 translation)
        {
            var intersections = new List<ZBufferItem>();

            foreach (var primitive in Triangles)
            {
                var items = primitive.Trace(ray, translation);
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
