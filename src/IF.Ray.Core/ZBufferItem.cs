using IF.Ray.Core.Shapes;
using SharpDX;

namespace IF.Ray.Core
{
    public class ZBufferItem
    {
        public IOccluder Primitive { get; set; }
        public Vector3 Intersection { get; set; }
        public Vector3 Translation { get; set; }

        public ZBufferItem(IOccluder o, Vector3 i, Vector3 t)
        {
            Primitive = o;
            Intersection = i;
            Translation = t;
        }

        /// <summary>
        /// The distance between provided vector and point of intersection
        /// </summary>
        public float Distance(Vector3 rayOrigin)
        {
            return Vector3.Distance(rayOrigin, Intersection);
        }
    }
}