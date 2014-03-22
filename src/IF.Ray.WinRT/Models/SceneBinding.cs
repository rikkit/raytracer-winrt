using SharpDX;

namespace IF.Ray.WinRT.Models
{
    public class SceneBinding
    {
        public Shape Shape { get; set; }
        public Vector3 Position { get; set; }

        public SceneBinding(Shape shape, Vector3 position)
        {
            Shape = shape;
            Position = position;
        }
    }
}