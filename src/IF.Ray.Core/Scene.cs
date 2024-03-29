﻿using System.Collections.Generic;
using IF.Ray.Core.Shapes;
using SharpDX;

namespace IF.Ray.Core
{
    public class Scene : IOccluder
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

        public Vector3 Origin { get; private set; }
        public Camera Camera { get; set; }

        public IList<Light> Lights
        {
            get { return _lights; }
        }

        public Scene(Camera camera)
        {
            Origin = new Vector3(0, 0, 0);
            _shapes = new List<SceneBinding>();
            _lights = new List<Light>();
            Camera = camera;
        }

        public void AddBinding(IOccluder plane, Vector3 zero)
        {
            Bindings.Add(new SceneBinding(plane, zero));
        }

        public Shader Shader { get; set; }

        public Vector3 Normal { get; private set; }

        public List<ZBufferItem> Trace(Shapes.Ray ray, Matrix transform, Vector3 translation)
        {
            var intersecting = new List<ZBufferItem>();
            foreach (var binding in Bindings)
            {
                var bindingIntersecting = binding.Shape.Trace(ray, transform, binding.Position);
                intersecting.AddRange(bindingIntersecting);
            }

            return intersecting;
        }

        public Color Colorise(Scene scene, Shapes.Ray ray, Matrix transform, Vector3 translation, Vector3 intersection)
        {
            throw new System.NotImplementedException();
        }

        public Color Ambient()
        {
            var ambient = new Color();
            foreach (var light in _lights)
            {
                ambient += Shader.Ambient(light, 1);
            }
            return ambient;
        }
    }
}
