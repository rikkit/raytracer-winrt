using System;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public class Shader
    {
        public Color Colour { get; set; }

        private Shader()
        {
            
        }

        public static Shader ShaderFromColour(Color color)
        {
            var s = new Shader();
            s.Colour = color;
            return s;
        }

        public Color Lambertian(Light light, Vector3 lightv, Vector3 normal)
        {
            // get the angle between the lightray and the normal of the surface... lambertian reflection
            var cosAngle = Math.Abs(Vector3.Dot(lightv, normal));
            return Colour * cosAngle * light.Intensity * light.Colour;
        }

        public Color Specular(Light light, Vector3 lightV, Vector3 normal)
        {
            return Color.White;
        }
    }
}