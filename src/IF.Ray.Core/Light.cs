using System;
using SharpDX;

namespace IF.Ray.Core
{
    public class Light
    {
        public Vector3 Position { get; set; }

        /// <summary>
        /// Brighter lights have higher intensity
        /// </summary>
        public float Intensity { get; set; }

        public Color Colour { get; set; }

        public Light(Vector3 position, Color color, float intensity)
        {
            Position = position;
            Colour = color;
            Intensity = intensity;
        }

        /// <summary>
        /// Inverse square law
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public float IntensityAt(float distance)
        {
            var i = (float) Math.Min(1, Intensity*(1/Math.Pow(distance,2)));
            return i;
        }
    }
}