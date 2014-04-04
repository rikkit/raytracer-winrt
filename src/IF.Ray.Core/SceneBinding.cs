using SharpDX;

namespace IF.Ray.Core
{
    public class SceneBinding
    {
        public const int FaceVectorCount = 6;
        public Mesh Mesh { get; set; }
        public Vector3 Position { get; set; }
        public SceneBinding(Mesh mesh, Vector3 position)
        {
            Mesh = mesh;
            Position = position;
        }
    }
}