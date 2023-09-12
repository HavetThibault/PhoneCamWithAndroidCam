using System.Windows.Threading;
using Helper.MVVM;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class MainViewModel : BindableClass
    {
        private DisplayStreamViewModel _displayStreamViewModel;

        public DisplayStreamViewModel ActiveViewModel
        {
            get => _displayStreamViewModel;
            set => SetProperty(ref _displayStreamViewModel, value);
        }

        public MainViewModel(Dispatcher uiDispatcher)
        {
            _displayStreamViewModel = new DisplayStreamViewModel(uiDispatcher);
        }

        public void Dispose()
        {
            _displayStreamViewModel.Dispose();
        }
    }
}
