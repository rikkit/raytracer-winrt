using System;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public class Shader
    {
        private const float AmbientFraction = 0.1f;

        public Color Colour { get; set; }
        public bool IsReflective { get; set; }

        private Shader()
        {
            
        }

        public static Shader MirrorShader()
        {
            var s = new Shader
            {
                IsReflective = true,
                Colour = new Color(1f,1f,1f,0.2f)
            };
            return s;
        }

        public static Shader MattShaderFromColour(Color color)
        {
            var s = new Shader();
            s.Colour = color;
            s.IsReflective = false;
            return s;
        }

        /// <summary>
        /// Calculate lambertian (diffuse) contribution
        /// </summary>
        /// <param name="light">light source</param>
        /// <param name="lightv">direction of light ray to point of intersection</param>
        /// <param name="normal">normal of surface</param>
        /// <param name="distance">distance from intersection to the light</param>
        /// <returns>Diffuse colour</returns>
        public Color Lambertian(Vector3 normal, Light light, Vector3 lightv, float distance)
        {
            var cosAngle = Math.Abs(Vector3.Dot(lightv, normal));
            return Colour * cosAngle * light.IntensityAt(distance) * light.Colour;
        }

        /// <summary>
        /// Calculate specular contribution (blinn-phong)
        /// </summary>
        /// <param name="light">light source</param>
        /// <param name="rayDir">incoming direction of tracer ray</param>
        /// <param name="lightv">incoming direction of light ray</param>
        /// <param name="normal">normal of the surface</param>
        /// <param name="distance">distance from intersection to the light</param>
        /// <returns></returns>
        public Color Specular(Vector3 normal, Vector3 rayDir, Light light, Vector3 lightv, float distance)
        {
            // get the h component
            var added = -rayDir + -lightv; // rayDir and lightDir are the wrong way round
            added.Normalize();
            var h = added/Vector3.Distance(added, Vector3.Zero);

            const int phongExponent = 100;
            var dot = Vector3.Dot(h, normal);
            var specular =(float) Math.Pow(dot, phongExponent);

            return specular*light.IntensityAt(distance) * light.Colour;
        }

        /// <summary>
        /// Calculate Ambient contribution, fraction of light colour and intensity
        /// </summary>
        /// <param name="light">Light source</param>
        /// <param name="distance"></param>
        /// <returns>Ambient colour</returns>
        public Color Ambient(Light light, float distance)
        {
            var a = AmbientFraction*light.IntensityAt(distance)*light.Colour;
            return a;
        }
    }
}