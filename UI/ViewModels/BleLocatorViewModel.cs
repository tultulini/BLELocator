using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Contracts.Enums;
using BLELocator.Core.Utils;
using BLELocator.UI.Models;
using BLELocator.UI.Views;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;

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
        private EventMapper _eventMapper;

        public RelayCommand EditConfigurationCommand
        {
            get { return _editConfigurationCommand ?? (_editConfigurationCommand = new RelayCommand(OnEditConfiguration)); }
        }

        public RelayCommand ListenToReceiversCommand
        {
            get { return _listenToReceiversCommand ?? (_listenToReceiversCommand = new RelayCommand(OnListenToReceivers,CanListenToReceivers)); }
        }

        private bool CanListenToReceivers()
        {
            return _model.BleSystemConfiguration.BleReceivers.HasSomething() &&
                   _model.BleSystemConfiguration.BleReceivers.Values.Any(r => r.IsEnabled);
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
            get { return _replayCaptureCommand ?? (_replayCaptureCommand = new RelayCommand(OnReplayCapture,CanReplayCapture)); }
        }

        private bool CanReplayCapture()
        {
            return !_model.ConnectedToListeners;
        }

        private void OnReplayCapture()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false
            };

            
            //var fileDialog = new OpenFileDialog();
            //fileDialog.Filter = "Json Files (*.json)|*.json";
            //dialog.Filters.Add(new CommonFileDialogFilter("Json files",".json"));
                
            var res = dialog.ShowDialog();
            if (res == CommonFileDialogResult.Ok)
            {
                var dir = dialog.FileName;
                var jsonFiles = Directory.GetFiles(dir, "*.json");
                var sessions = new List<EventCaptureSession>();
                foreach (var jsonFile in jsonFiles)
                {
                    string json;
                    using (var reader = new StreamReader(File.OpenRead(jsonFile)))
                    {
                        json = reader.ReadToEnd();
                    }
                    try
                    {
                        var session = JsonConvert.DeserializeObject<EventCaptureSession>(json);
                        sessions.Add(session);
                    }
                    catch (Exception)
                    {
                        
                        
                    }
                }
                if (sessions.IsNullOrEmpty())
                {
                    InsertMessage("Couldn't load any event sessions");
                    return;
                }
                sessions=sessions.OrderByDescending(s => s.Start).ToList();
                var capturePickerVm = new CapturePickerViewModel
                {
                    
                    CaptureSessions = new ObservableCollection<EventCaptureSession>(sessions)
                };
                var window = new CapturePickerWindow
                {
                    DataContext = capturePickerVm
                };
                capturePickerVm.OnOk += window.Close;
                var pickResult = window.ShowDialog();
                if (pickResult.HasValue && pickResult.Value && capturePickerVm.SelectedSession!=null)
                    Task.Run(() => _model.PlaySession(capturePickerVm.SelectedSession));
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

            var window = new MapWindow(_model.BleSystemConfiguration);
            _eventMapper.TransmitterSignalDiscovered +=
                s => window.HandleDiscoveryEvent(s.BleReceiver, s.Transmitter.MacAddress, s.Distance);
            _eventMapper.TransmitterPositionDiscovered += window.HandleTransmitterLocationEvent;
            window.Show();
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
            ReplayCaptureCommand.RaiseCanExecuteChanged();
        }

        private void OnEditConfiguration()
        {
            var window = new ConfigWindow();
            var configVM = new ConfigViewModel();
            
            window.DataContext = configVM;
            configVM.OnSaved += window.Close;
            window.ShowDialog();
            ListenToReceiversCommand.RaiseCanExecuteChanged();
        }

        public BleLocatorViewModel()
        {
            _model = BleLocatorModel.Instance;
            _model.OnLogMessage += InsertMessage;
            _model.OnConnectionStateChanged += OnConnectionStateChanged;
            _eventMapper = new EventMapper(_model.BleSystemConfiguration);

            _model.OnRegisteredTransmitterEvent += e => _eventMapper.HandleDiscoveryEvent(e);
        }

        private void OnConnectionStateChanged(BleConnectionState obj)
        {
            RaisePropertyChanged(()=>IsListeningToReceivers);
        }
    }
}