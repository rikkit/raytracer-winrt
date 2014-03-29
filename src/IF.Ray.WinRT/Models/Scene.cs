using System.Collections.Generic;
using System.IO;
using SharpDX;

namespace IF.Ray.WinRT.Models
{
    public class Scene
    {
        private readonly IList<SceneBinding> _shapes;
        private readonly IList<Light> _lights;

        public IList<SceneBinding> Bindings
        {
            get { return _shapes; }
        }

        public int BindingCount
        {
            get { return _shapes.Count; }
        }

        public Vector4 Origin { get; private set; }
        public Camera Camera { get; set; }

        public IList<Light> Lights
        {
            get { return _lights; }
        }

        public Scene(Camera camera)
        {
            Origin = new Vector4(0, 0, 0, 0);
            _shapes = new List<SceneBinding>();
            _lights = new List<Light>();
            Camera = camera;
        }
    }

    public class Light
    {
        public Vector3 Position { get; set; }

        /// <summary>
        /// Value between 0 and 1
        /// </summary>
        public float Intensity { get; set; }

        public Light(Vector3 position, float intensity)
        {
            Position = position;
            Intensity = intensity;
        }
    }
}
