using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public class Ray
    {
        private SharpDX.Ray _sharpDx;
        private Vector3 _direction;

        public Vector3 Origin { get; set; }

        /// <summary>
        /// Normalised vector representing direction of the ray
        /// </summary>
        public Vector3 Direction
        {
            get { return _direction; }
            set
            {
                if (!value.IsNormalized)
                {
                    value.Normalize();
                }
                _direction = value;
            }
        }

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
            _sharpDx = new SharpDX.Ray(origin, direction);
        }
    }
}