using SharpDX;

namespace IF.Ray.WinRT.Models
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }

        public Vector3 Direction
        {
            get { return Target - Position; }
        }

        public float Scale { get; set; }
        
        public Camera(Vector3 position, Vector3 target)
        {
            Position = position;
            Target = target;
        }
    }
}