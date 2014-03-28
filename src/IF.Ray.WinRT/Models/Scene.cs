using System.Collections.Generic;
using System.IO;
using SharpDX;

namespace IF.Ray.WinRT.Models
{
    public class Scene
    {
        private readonly IList<SceneBinding> _shapes;

        /// <summary>
        /// TODO Get a better name for this
        /// </summary>
        public IEnumerable<SceneBinding> Bindings
        {
            get { return _shapes; }
        }

        public int BindingCount
        {
            get { return _shapes.Count; }
        }

        public Vector4 Origin { get; private set; }
        public Camera Camera { get; set; }
        
        public Scene(Camera camera)
        {
            Origin = new Vector4(0, 0, 0, 0);
            _shapes = new List<SceneBinding>();
            Camera = camera;
        }

        public void AddShape(Mesh mesh, Vector4 position)
        {
            var binding = new SceneBinding(mesh, position);
            _shapes.Add(binding);
        }
    }
}
