using CommonDX;

namespace IF.Ray.WinRT.Renderer
{
    public interface IRenderer
    {
        void Initialise(DeviceManager manager);
        void Render(TargetBase render);
    }
}