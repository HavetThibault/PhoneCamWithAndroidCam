using System.Windows.Threading;
using Helper.MVVM;

namespace PhoneCamWithAndroidCam.ViewModels
{
    public class MainViewModel : BindableClass
    {
        private BindableClass _currentViewModel;
        private DisplayStreamViewModel _displayStreamViewModel;

        public BindableClass BigViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public MainViewModel(Dispatcher uiDispatcher)
        {
            _displayStreamViewModel = new DisplayStreamViewModel(uiDispatcher, new(uiDispatcher));
            BigViewModel = _displayStreamViewModel;
        }

        public void Dispose()
        {
            _displayStreamViewModel.Dispose();
        }
    }
}
