using System;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using IF.Ray.Core;
using IF.Ray.WinRT.ViewModels;
using Ninject;

namespace IF.Ray.WinRT.Pages
{
    public sealed partial class MainPage : PageBase
    {
        private MainViewModel _vm;
        private int _currentFrameIndex;
        private DispatcherTimer _timer;

        public MainPage()
        {
            _vm = App.Kernel.Get<MainViewModel>();

            ViewModel = _vm;

            DataContext = ViewModel;

            InitializeComponent();
        }

        /// <summary>
        /// Takes the rendered frames and displays them on a timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShowAnimationClick(object sender, TappedRoutedEventArgs e)
        {
            if (_vm.Frames == null || (_timer!= null && _timer.IsEnabled))
            {
                return;
            }

            AnimationSurface.Visibility = Visibility.Visible;
            _currentFrameIndex = 0;

            // set up the timer
            _timer = new DispatcherTimer();
            const int frameintervalMs = (int) (1000f/SceneRenderer.AnimationFps);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, frameintervalMs);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object sender, object e)
        {
            if (_currentFrameIndex >= _vm.Frames.Count)
            {
                _timer.Stop();
                AnimationSurface.Visibility = Visibility.Collapsed;
                return;
            }

            var frame = _vm.Frames[_currentFrameIndex];
            AnimationSurface.Source = frame;
            _currentFrameIndex++;
        }
    }
}
