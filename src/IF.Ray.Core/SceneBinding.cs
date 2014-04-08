using IF.Ray.Core.Shapes;
using SharpDX;

namespace IF.Ray.Core
{
    public class SceneBinding
    {
        public const int FaceVectorCount = 6;
        public IOccluder Shape { get; set; }
        public Vector3 Position { get; set; }
        public SceneBinding(IOccluder mesh, Vector3 position)
        {
            Shape = mesh;
            Position = position;
        }
    }
}