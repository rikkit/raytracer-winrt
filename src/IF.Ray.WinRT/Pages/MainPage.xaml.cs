using IF.Ray.WinRT.ViewModels;
using Ninject;

namespace IF.Ray.WinRT.Pages
{
    public sealed partial class MainPage : PageBase
    {
        public MainPage()
        {
            ViewModel = App.Kernel.Get<MainViewModel>();

            DataContext = ViewModel;

            InitializeComponent();
        }
    }
}
