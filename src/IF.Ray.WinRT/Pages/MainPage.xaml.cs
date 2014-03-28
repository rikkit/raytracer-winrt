using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace IF.Ray.WinRT.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }
        
        private void OnRotationSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (D3DUserControl == null)
            {
                return;
            }

            D3DUserControl.UpdateRotation((float)RotationXSlider.Value, (float)RotationYSlider.Value, (float)RotationZSlider.Value);
        }

        private void OnZoomSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (D3DUserControl == null)
            {
                return;
            }

            D3DUserControl.UpdateZoom((float) ZoomSlider.Value);
        }

        private async void OnRenderButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            if (D3DUserControl.CanRender)
            {
                await D3DUserControl.RenderAsync();
            }
        }
    }
}
