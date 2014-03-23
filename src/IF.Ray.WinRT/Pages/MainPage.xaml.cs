using Windows.UI.Xaml;
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
            InitializeComponent();
            SizeChanged += OnPageSizeChanged;
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            D3DUserControl.Width = PageGrid.ColumnDefinitions[1].ActualWidth;
            D3DUserControl.Height = PageGrid.ActualHeight;
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
    }
}
