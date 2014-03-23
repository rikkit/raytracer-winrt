// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

using System;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CommonDX;
using IF.Ray.WinRT.Common;
using IF.Ray.WinRT.Renderer;

namespace IF.Ray.WinRT.Controls
{
    public sealed partial class Direct3DUserControl : SwapChainPanel
    {
        DeviceManager _deviceManager;
        SwapChainPanelTarget _target;
        SceneRenderer _sceneRenderer;

        public static readonly DependencyProperty DesignModeD3DRenderingProperty =
            DependencyProperty.Register("DesignModeD3DRendering", typeof(bool), typeof(Direct3DUserControl), new PropertyMetadata(false));

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

        public bool DesignModeD3DRendering
        {
            get
            {
                return (bool)GetValue(DesignModeD3DRenderingProperty);
            }
            set
            {
                SetValue(DesignModeD3DRenderingProperty, value);
            }
        }

        public Direct3DUserControl()
        {
            this.InitializeComponent();
            // Do D3D initialization when element is loaded, because DesignModeD3DRendering is yet not set in ctor
            this.Loaded += Direct3DUserControl_Loaded;            
        }

        void Direct3DUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Do not initialize D3D in design mode as default, since it may cause designer crashes
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled && !DesignModeD3DRendering)
                return;

            // Safely dispose any previous instance
            // Creates a new DeviceManager (Direct3D, Direct2D, DirectWrite, WIC)
            _deviceManager = new DeviceManager();            

            // Use current control as the rendering _target (Initialize SwapChain, RenderTargetView, DepthStencilView, BitmapTarget)
            _target = new SwapChainPanelTarget(this);

            // Add Initializer to device manager
            _deviceManager.OnInitialize += _target.Initialize;

            // scene renderer
            _sceneRenderer = new SceneRenderer();
            _deviceManager.OnInitialize += _sceneRenderer.Initialise;
            _target.OnRender += _sceneRenderer.Render;

            // Initialize the device manager and all registered _deviceManager.OnInitialize             
            try
            {
                _deviceManager.Initialize(DisplayInformation.GetForCurrentView().LogicalDpi);
                DisplayInformation.GetForCurrentView().DpiChanged += DisplayInformation_LogicalDpiChanged;
            } catch (Exception ex) {
                //DisplayInformation.GetForCurrentView() will throw exception in designer
                _deviceManager.Initialize(96.0f);
            }

            // Setup rendering callback
            CompositionTargetEx.Rendering += CompositionTarget_Rendering;
        }

        void DisplayInformation_LogicalDpiChanged(DisplayInformation displayInformation, object sender)
        {
            _deviceManager.Dpi = displayInformation.LogicalDpi;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            _target.RenderAll();
            _target.Present();
        }
    }
}
