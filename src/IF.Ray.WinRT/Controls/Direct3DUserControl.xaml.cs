using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IF.Ray.WinRT.Renderer;

namespace IF.Ray.WinRT.Controls
{
    public sealed partial class Direct3DUserControl : UserControl
    {
        SceneRenderer _sceneRenderer;
        
        public bool CanRender { get; set; }

        public Direct3DUserControl()
        {
            this.InitializeComponent();

            _sceneRenderer = new SceneRenderer();
            this.Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }

            await Task.Run(async () => await _sceneRenderer.InitialiseSceneAsync());
            CanRender = true;
        }

        public async Task RenderAsync()
        {
            CanRender = false;
            var width = (int)this.Width;
            var height = (int)this.Height;
            var image = await _sceneRenderer.RenderAsync(width, height);
            
            RaytracedImage.Source = null;
            RaytracedImage.Source = image;
            
            CanRender = true;
        }

        public void UpdateZoom(float value)
        {
            if (_sceneRenderer == null)
            {
                return;
            }

            _sceneRenderer.Zoom = value;
        }

        public void UpdateRotation(float x, float y, float z)
        {
            if (_sceneRenderer == null)
            {
                return;
            }

            _sceneRenderer.RotationX = x;
            _sceneRenderer.RotationY = y;
            _sceneRenderer.RotationZ = z;
        }
    }
}
