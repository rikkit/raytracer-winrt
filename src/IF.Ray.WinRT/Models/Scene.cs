﻿using System.Collections.Generic;
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

        public Vector3 Origin { get; private set; }

        public Scene()
        {
            Origin = new Vector3(0,0,0);
            _shapes = new List<SceneBinding>();
        }

        public void AddShape(Shape shape, Vector3 position)
        {
            var binding = new SceneBinding(shape, position);
            _shapes.Add(binding);
        }
    }
}