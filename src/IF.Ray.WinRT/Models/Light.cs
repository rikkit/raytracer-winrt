using SharpDX;

namespace IF.Ray.WinRT.Models
{
    public class Light
    {
        public Vector3 Position { get; set; }

        /// <summary>
        /// Value between 0 and 1
        /// </summary>
        public float Intensity { get; set; }

        public Color Colour { get; set; }

        public Light(Vector3 position, Color color, float intensity)
        {
            Position = position;
            Colour = color;
            Intensity = intensity;
        }
    }
}