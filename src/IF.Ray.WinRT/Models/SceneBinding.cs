using SharpDX;

namespace IF.Ray.WinRT.Models
{
    public class SceneBinding
    {
        public const int FaceVectorCount = 6;
        public Mesh Mesh { get; set; }
        public Vector4 Position { get; set; }
        public SceneBinding(Mesh mesh, Vector4 position)
        {
            Mesh = mesh;
            Position = position;
        }
    }
}