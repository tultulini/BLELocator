using System.Threading.Tasks;
using BLELocator.Core.Contracts.Enums;
using BLELocator.UI.Models;
using BLELocator.UI.Views;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;

namespace BLELocator.UI.ViewModels
{
    public class BleLocatorViewModel :LoggedViewModel
    {
        private BleLocatorModel _model;
        private RelayCommand _editConfigurationCommand;
        private RelayCommand _listenToReceiversCommand;
        private RelayCommand _captureEventsCommand;
        private RelayCommand _stopCaptureEventsCommand;
        private RelayCommand _openMapCommand;
        private RelayCommand _replayCaptureCommand;
        private bool _isListeningToReceivers;

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

        public RelayCommand ReplayCaptureCommand
        {
            get { return _replayCaptureCommand ?? (_replayCaptureCommand = new RelayCommand(OnReplayCapture)); }
        }

        private void OnReplayCapture()
        {
            
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Json Files (*.json)|*.json";
            var res = fileDialog.ShowDialog();
            if (res.HasValue && res.Value)
            {
                Task.Run(() => _model.ReplayCapture(fileDialog.FileName));
            }
        }

        public RelayCommand OpenMapCommand
        {
            get { return _openMapCommand ?? (_openMapCommand = new RelayCommand(OnOpenMap)); }
        }

        public bool IsListeningToReceivers
        {
            get
            {
                _isListeningToReceivers = _model.ConnectedToListeners;
                return _isListeningToReceivers;
            }
            set
            {
                _isListeningToReceivers = value;
                RaisePropertyChanged(() => IsListeningToReceivers);
            }
        }

        private void OnOpenMap()
        {

            //var mapVM = new MapViewModel();
            var window = new MapWindow(_model.BleSystemConfiguration);
            //window.DataContext = mapVM;
            window.ShowDialog();
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
            if (_model.ConnectedToListeners)
            {
                _model.Disconnect();
            }
            else
            {
                _model.Connect(); 

            }
        }

        private void OnEditConfiguration()
        {
            var window = new ConfigWindow();
            var configVM = new ConfigViewModel();
            
            window.DataContext = configVM;
            configVM.OnSaved += window.Close;
            window.ShowDialog();
        }

        public BleLocatorViewModel()
        {
            _model = BleLocatorModel.Instance;
            _model.OnLogMessage += InsertMessage;
            _model.OnConnectionStateChanged += OnConnectionStateChanged;

        }

        private void OnConnectionStateChanged(BleConnectionState obj)
        {
            RaisePropertyChanged(()=>IsListeningToReceivers);
        }
    }
}