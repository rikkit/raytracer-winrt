using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using IF.Common.Metro.Mvvm;
using IF.Common.Metro.Progress;
using IF.Common.Metro.Properties;
using Ninject;

namespace IF.Ray.WinRT.ViewModels
{
    public class ViewModelBase : IViewModel
    {
        public bool Initialised { get; protected set; }
        public string PageTitle { get; protected set; }

        public Color AccentColour { get; protected set; }

        public IProgressAggregator Progress { get; protected set; }

        public CoreDispatcher UiDispatcher { get; private set; }

        protected ViewModelBase(IProgressAggregator progress)
        {
            Progress = progress;
            UiDispatcher = App.Kernel.Get<CoreDispatcher>();
        }

        /// <summary>
        /// Called after navigation to a page completes.
        /// </summary>
        public async virtual Task AfterNavigatedToAsync(object parameter)
        {
            //TODO page logging
        }

        /// <summary>
        /// Called before a navigation to a page starts.
        /// </summary>
        /// <returns></returns>
        public async virtual Task BeforeNavigatedFromAsync()
        {
            //TODO page logging
        }

        /// <summary>
        /// Called after a page is fully rendered.
        /// </summary>
        /// <returns></returns>
        public async virtual Task AfterPageLoadedAsync()
        {
            //TODO page logging
        }

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                UiDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => handler(this, new PropertyChangedEventArgs(propertyName)));
            }
        }

        #endregion

    }
}
