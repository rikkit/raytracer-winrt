using SharpDX;

namespace IF.Ray.WinRT.Renderer
{
    public static class RenderEx
    {
        public static Vector3 AsVector3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}