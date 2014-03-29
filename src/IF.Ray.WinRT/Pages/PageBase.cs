using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using IF.Common.Metro.Mvvm;

namespace IF.Ray.WinRT.Pages
{
    public class PageBase : Page, IView
    {
        public IViewModel ViewModel { get; protected set; }

        public PageBase()
        {
            Loaded += AfterPageLoaded;
        }

        protected void AfterPageLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.AfterPageLoadedAsync().AsAsyncAction();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.BeforeNavigatedFromAsync().AsAsyncAction();

            base.OnNavigatedFrom(e);
        }
    }
}