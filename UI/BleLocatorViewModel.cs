using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;

namespace BLELocator.UI
{
    public class BleLocatorViewModel :LoggedViewModel
    {
        private BleLocatorModel _model;
        private RelayCommand _editConfigurationCommand;
        private RelayCommand _listenToReceiversCommand;
        private RelayCommand _captureEventsCommand;
        private RelayCommand _stopCaptureEventsCommand;
        private RelayCommand _openMapCommand;

        public RelayCommand EditConfigurationCommand
        {
            get { return _editConfigurationCommand ?? (_editConfigurationCommand = new RelayCommand(OnEditConfiguration)); }
        }

        public RelayCommand ListenToReceiversCommand
        {
            get { return _listenToReceiversCommand ?? (_listenToReceiversCommand = new RelayCommand(OnListenToReceivers)); }
        }

        public RelayCommand CaptureEventsCommand
        {
            get { return _captureEventsCommand ?? (_captureEventsCommand = new RelayCommand(OnCaptureStart)); }
        }

        private void OnCaptureStart()
        {
            _model.CapturingEventsStart();
        }

        public RelayCommand StopCaptureEventsCommand
        {
            get { return _stopCaptureEventsCommand ?? (_stopCaptureEventsCommand = new RelayCommand(OnStopCapturing)); }
            
        }

        public RelayCommand OpenMapCommand
        {
            get { return _openMapCommand ?? (_openMapCommand = new RelayCommand(OnOpenMap)); }
        }

        private void OnOpenMap()
        {
            

        }

        private void OnStopCapturing()
        {
            _model.StopCapturing();
            var detailsVm = new CaptureSessionDetailsViewModel();
            var view = new CaptureSessionDetailsView{DataContext = detailsVm};
            view.ShowDialog();
            _model.CapturingEventsFinalize(detailsVm.Comments);
        }

        private void OnListenToReceivers()
        {
            
            _model.Connect();
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
            _model.OnLogMessage += InsertMessage;

        }
    }
}