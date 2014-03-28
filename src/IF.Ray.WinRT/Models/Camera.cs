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

        /// <summary>
        /// Get a matrix projection representing this camera
        /// </summary>
        public Matrix Matrix
        {
            get
            {
                return Matrix.LookAtLH(Position * Scale, Target, Vector3.UnitY);
            }
        }

        public Camera(Vector3 position, Vector3 target)
        {
            Position = position;
            Target = target;
        }
    }
}