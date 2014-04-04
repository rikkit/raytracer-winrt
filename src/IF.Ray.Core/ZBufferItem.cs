using SharpDX;

namespace IF.Ray.Core
{
    public class ZBufferItem
    {
        public Triangle Triangle { get; set; }
        public Vector3 Intersection { get; set; }

        public ZBufferItem(Triangle t, Vector3 i)
        {
            Triangle = t;
            Intersection = i;
        }

        public float Distance(SharpDX.Ray ray)
        {
            return Vector3.Distance(ray.Position, Intersection);
        }
    }
}