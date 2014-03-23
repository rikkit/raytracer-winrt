using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IF.Ray.WinRT.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ScaleSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (D3DUserControl != null && ScaleSlider != null)
            {
                var transform = new CompositeTransform();
                //transform.Rotation = RotateSlider.Value;
                transform.ScaleX = ScaleSlider.Value;
                transform.ScaleY = ScaleSlider.Value;
                D3DUserControl.RenderTransform = transform;
            }
        }

        private void OnRotationSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            D3DUserControl.UpdateRotation((float)RotationXSlider.Value, (float)RotationYSlider.Value, (float)RotationZSlider.Value);
        }
    }
}
