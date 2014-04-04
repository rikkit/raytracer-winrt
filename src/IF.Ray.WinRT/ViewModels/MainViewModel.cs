using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;
using IF.Common.Metro.Mvvm;
using IF.Common.Metro.Progress;
using IF.Ray.Core;
using Ninject.Parameters;

namespace IF.Ray.WinRT.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private SceneRenderer _sceneRenderer;
        private WriteableBitmap _raytracedImage;
        private bool _canRender;
        private int _renderWidth;
        private int _renderHeight;

        public bool CanRender
        {
            get { return _canRender; }
            private set
            {
                if (_canRender.Equals(value))
                {
                    return;
                }

                _canRender = value;
                RaisePropertyChanged();
            }
        }

        public WriteableBitmap RaytracedImage
        {
            get { return _raytracedImage; }
            set
            {
                if (_raytracedImage != null && _raytracedImage.Equals(value))
                {
                    return;
                }

                _raytracedImage = value;
                RaisePropertyChanged();
            }
        }

        public int RenderWidth
        {
            get { return _renderWidth; }
            set
            {
                if (_renderWidth.Equals(value))
                {
                    return;
                }

                _renderWidth = value;
                RaisePropertyChanged();
            }
        }

        public int RenderHeight
        {
            get { return _renderHeight; }
            set
            {
                if (_renderHeight.Equals(value))
                {
                    return;
                }

                _renderHeight = value;
                RaisePropertyChanged();
            }
        }

        public SceneRenderer RayTracer
        {
            get { return _sceneRenderer; }
            private set
            {
                if (_sceneRenderer != null &&_sceneRenderer.Equals(value))
                {
                    return;
                }

                _sceneRenderer = value;
                RaisePropertyChanged();
            }
        }

        public ICommand RenderCommand { get; set; }

        public MainViewModel(IProgressAggregator progress) : base(progress)
        {
            RayTracer = new SceneRenderer(UiDispatcher);
            RenderWidth = 400;
            RenderHeight = 300;

            RenderCommand = new AsyncDelegateCommand(Render);
        }
        
        public async override Task AfterPageLoadedAsync()
        {
            await _sceneRenderer.InitialiseSceneAsync();
            CanRender = true;
        }

        private async Task Render()
        {
            if (!CanRender)
            {
                return;
            }

            var token = Progress.RaiseLoading("Rendering", false);

            CanRender = false;
            var image = await _sceneRenderer.RenderAsync(RenderWidth, RenderHeight, token);

            RaytracedImage = null;
            RaytracedImage = image;

            Progress.Finalise(token);

            CanRender = true;
        }
    }
}
