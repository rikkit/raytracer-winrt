using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;
using IF.Common.Metro.Mvvm;
using IF.Common.Metro.Progress;
using IF.Ray.WinRT.Renderer;

namespace IF.Ray.WinRT.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly SceneRenderer _sceneRenderer;
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

        public float RotationX
        {
            get { return _sceneRenderer.RotationX; }
            set
            {
                if (_sceneRenderer.RotationX.Equals(value))
                {
                    return;
                }

                _sceneRenderer.RotationX = value;
                RaisePropertyChanged();
            }
        }

        public float RotationY
        {
            get { return _sceneRenderer.RotationY; }
            set
            {
                if (_sceneRenderer.RotationY.Equals(value))
                {
                    return;
                }

                _sceneRenderer.RotationY = value;
                RaisePropertyChanged();
            }
        }

        public float RotationZ
        {
            get { return _sceneRenderer.RotationZ; }
            set
            {
                if (_sceneRenderer.RotationZ.Equals(value))
                {
                    return;
                }

                _sceneRenderer.RotationZ = value;
                RaisePropertyChanged();
            }
        }

        public float Zoom
        {
            get { return _sceneRenderer.Zoom; }
            set
            {
                if (_sceneRenderer.Zoom.Equals(value))
                {
                    return;
                }

                _sceneRenderer.Zoom = value;
                RaisePropertyChanged();
            }
        }

        public ICommand RenderCommand { get; set; }

        public MainViewModel(IProgressAggregator progress) : base(progress)
        {
            _sceneRenderer = new SceneRenderer();
            Zoom = 1;
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
