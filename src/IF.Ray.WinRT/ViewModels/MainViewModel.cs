using System;
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
        private TimeSpan _animationLength;

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

        public TimeSpan AnimationLength
        {
            get { return _animationLength; }
            set
            {
                // bug: this will never be true
                if (_animationLength.Equals(value))
                {
                    return;
                }

                var actualTimeSpan = new TimeSpan(0, 0, value.Hours, value.Minutes);
                _animationLength = actualTimeSpan;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Set of parameters to use for rendering
        /// Also used for before parameters in the animation
        /// </summary>
        public ParameterBinding RenderParameters { get; set; }

        /// <summary>
        /// Set of parameters used as the end parameters in the animation
        /// </summary>s
        public ParameterBinding AnimationParameters { get; set; }

        public ICommand RenderCommand { get; set; }
        public ICommand AnimateCommand { get; set; }

        public MainViewModel(IProgressAggregator progress) : base(progress)
        {
            _sceneRenderer = new SceneRenderer();
            RenderWidth = 400;
            RenderHeight = 300;

            RenderCommand = new AsyncDelegateCommand(Render);
            RenderParameters = new ParameterBinding(UiDispatcher);
            AnimationParameters = new ParameterBinding(UiDispatcher);
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
            var image = await _sceneRenderer.RenderAsync(RenderWidth, RenderHeight, RenderParameters, token);

            RaytracedImage = null;
            RaytracedImage = image;

            Progress.Finalise(token);

            CanRender = true;
        }

        private async Task Animate()
        {
            if (!CanRender)
            {
                return;
            }

            var token = Progress.RaiseLoading("Animating", false);

            CanRender = false;
            var frames = await _sceneRenderer.Animate(RenderWidth, RenderHeight, AnimationLength, token, RenderParameters, AnimationParameters);

            throw new NotImplementedException();
            CanRender = true;

            Progress.Finalise(token);
        }
    }
}
