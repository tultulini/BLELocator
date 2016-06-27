using GalaSoft.MvvmLight.Command;

namespace BLELocator.UI
{
    public class BleLocatorViewModel :GalaSoft.MvvmLight.ViewModelBase
    {
        private BleLocatorModel _model;
        private RelayCommand _editConfigurationCommand;

        public RelayCommand EditConfigurationCommand
        {
            get { return _editConfigurationCommand ?? (_editConfigurationCommand = new RelayCommand(OnEditConfiguration)); }
        }

        private void OnEditConfiguration()
        {
            var window = new ConfigWindow();
            window.DataContext = new ConfigViewModel();
            window.ShowDialog();
        }

        public BleLocatorViewModel()
        {
            _model = BleLocatorModel.Instance;

        }
    }
}