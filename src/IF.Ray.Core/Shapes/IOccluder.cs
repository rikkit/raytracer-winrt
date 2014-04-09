using System.Collections.Generic;
using SharpDX;

namespace IF.Ray.Core.Shapes
{
    public interface IOccluder
    {
        Shader Shader { get; set; }

        /// <summary>
        /// Get a list of intersections the ray has with this occluder
        /// </summary>
        List<ZBufferItem> Trace(Ray ray, Matrix transform, Vector3 translation);

        /// <summary>
        /// Get the colour of the point of intersection
        /// </summary>
        Color Colorise(Scene scene, Ray ray, Matrix transform, Vector3 translation, Vector3 intersection);
    }
}