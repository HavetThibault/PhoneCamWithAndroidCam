using System.Windows.Threading;
using Helper.MVVM;

namespace PhoneCamWithAndroidCam.Main
{
    public class MainViewModel : BindableClass
    {
        private StreamsAndStatsViewModels _displayStreamViewModel;

        public StreamsAndStatsViewModels ActiveViewModel
        {
            get => _displayStreamViewModel;
            set => SetProperty(ref _displayStreamViewModel, value);
        }

        public MainViewModel(Dispatcher uiDispatcher)
        {
            _displayStreamViewModel = new StreamsAndStatsViewModels(uiDispatcher);
        }

        public void Dispose()
        {
            _displayStreamViewModel.Dispose();
        }
    }
}
