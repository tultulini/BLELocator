using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;
using BLELocator.UI.Models;
using BLELocator.UI.Views;
using GalaSoft.MvvmLight.Command;

namespace BLELocator.UI.ViewModels
{
    public class ConfigViewModel : LoggedViewModel
    {
        private RelayCommand _addReceiverCommand;
        private RelayCommand _addTransmitterCommand;
        private RelayCommand _saveConfigurationCommnand;
        private bool _keepCheckingIsAlive;
        private int _keepAliveInterval;
        private ObservableCollection<TransmitterEntryViewModel> _transmitters;
        private RelayCommand _configureWayPointsCommand;
        public event Action OnSaved;

        public RelayCommand ConfigureWayPointsCommand
        {
            get { return _configureWayPointsCommand ?? (_configureWayPointsCommand = new RelayCommand(OnConfigureWayPoints)); }
            
        }

        private void OnConfigureWayPoints()
        {
            var config = BleLocatorModel.Instance.BleSystemConfiguration;
            var wayPointConfigVM = new WayPointsConfigViewModel(config.ReceiverPaths);
            var window = new WayPointConfigWindow
            {
                DataContext = wayPointConfigVM
            };
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                ReceiverPaths.Clear();
                ReceiverPaths.AddRange(wayPointConfigVM.ReceiverPaths.Select(r=>r.ReceiverPath));
                
            }

        }

        public ConfigViewModel()
        {
            Transmitters = new ObservableCollection<TransmitterEntryViewModel>();
            Receivers = new ObservableCollection<ReceiverViewModel>();
            Messages = new ObservableCollection<string>();
            
            LoadExistingConfiguration();
            InsertMessage("Loaded");
        }

        
        private void LoadExistingConfiguration()
        {
            var config = BleLocatorModel.Instance.BleSystemConfiguration;
            ReceiverPaths = config.ReceiverPaths.HasSomething()? new List<ReceiverPath>(config.ReceiverPaths):new List<ReceiverPath>();
            if (config.BleReceivers.HasSomething())
            {
                foreach (var bleReceiver in config.BleReceivers)
                {
                    var viewModel = new ReceiverEntryViewModel(bleReceiver.Value);
                    viewModel.OnRemove += vm => Receivers.Remove(vm);
                    Receivers.Add(viewModel);
                }
            }
            else
            {
                var viewModel = new ReceiverEntryViewModel(new BleReceiver());
                viewModel.OnRemove += vm => Receivers.Remove(vm);
                Receivers.Add(viewModel);
            }
            if (config.BleTransmitters.HasSomething())
            {
                foreach (var transmitter in config.BleTransmitters)
                {
                    var viewModel = new TransmitterEntryViewModel(transmitter.Value);
                    viewModel.OnRemove += vm => Transmitters.Remove(vm);
                    Transmitters.Add(viewModel);
                }
            }
            else
            {
                var viewModel = new TransmitterEntryViewModel(new BleTransmitter());
                viewModel.OnRemove += vm => Transmitters.Remove(vm);
                Transmitters.Add(viewModel);
            }
        }

        public ObservableCollection<TransmitterEntryViewModel> Transmitters
        {
            get { return _transmitters; }
            set
            {
                _transmitters = value; 
                RaisePropertyChanged(()=>Transmitters);
            }
        }

        public bool KeepCheckingIsAlive
        {
            get { return _keepCheckingIsAlive; }
            set
            {
                _keepCheckingIsAlive = value; 
                RaisePropertyChanged(()=>KeepCheckingIsAlive);
            }
        }

        public int KeepAliveInterval
        {
            get { return _keepAliveInterval; }
            set
            {
                _keepAliveInterval = value;
                RaisePropertyChanged(() => KeepAliveInterval);
                if (KeepAliveInterval <= 0)
                    KeepCheckingIsAlive = false;
            }
        }

        public ObservableCollection<ReceiverViewModel> Receivers { get; set; }
        public List<ReceiverPath> ReceiverPaths { get; set; }
        public RelayCommand AddReceiverCommand
        {
            get { return _addReceiverCommand ?? (_addReceiverCommand = new RelayCommand(OnAddReceiver)); }
        }

        public RelayCommand SaveConfigurationCommnand
        {
            get { return _saveConfigurationCommnand ?? (_saveConfigurationCommnand = new RelayCommand(OnSaveConfiguration)); }
        }

        private bool ValidateConfiguration()
        {
            if (KeepCheckingIsAlive && KeepAliveInterval < 5000)
            {
                InsertMessage("Keep alive needs to be at least 5000 msec");
                return false;
            }
            var ips = new HashSet<string>();
            var incomingPorts = new HashSet<int>();
            foreach (var receiverViewModel in Receivers)
            {
                if (!receiverViewModel.IsValid)
                {
                    InsertMessage("not all receivers are valid => aborting save");
                    return false;
                }
                if (ips.Contains(receiverViewModel.IPAddress))
                {
                    InsertMessage(string.Format("Receiver IP {0} exists more than once => aborting save", receiverViewModel.IPAddress));
                    return false;
                }
                ips.Add(receiverViewModel.IPAddress);
                if (incomingPorts.Contains(receiverViewModel.IncomingPort))
                {
                    InsertMessage(string.Format("Incoming port from receiver [{0}] exists more than once => aborting save", receiverViewModel.IncomingPort));
                    return false;
                }
                incomingPorts.Add(receiverViewModel.IncomingPort);
            }

            var transmitterNames = new HashSet<string>();
            var transmitterMacAddress = new HashSet<string>();
            foreach (var transmitterViewModel in Transmitters)
            {
                if (!transmitterViewModel.IsValid)
                {
                    InsertMessage("not all transmitters are valid => aborting save");
                    return false;
                }
                if (transmitterNames.Contains(transmitterViewModel.TransmitterName))
                {
                    InsertMessage(string.Format("Transmitter Name {0} exists more than once => aborting save", transmitterViewModel.TransmitterName));
                    return false;
                    
                }
                transmitterNames.Add(transmitterViewModel.TransmitterName);
                if (transmitterMacAddress.Contains(transmitterViewModel.MacAddress))
                {
                    InsertMessage(string.Format("Transmitter Mac Address {0} exists more than once => aborting save", transmitterViewModel.MacAddress));
                    return false;

                }
                transmitterMacAddress.Add(transmitterViewModel.MacAddress);
            }

            return true;
        }


       
        private void OnSaveConfiguration()
        {
            if(!ValidateConfiguration())
                return;
            var model = BleLocatorModel.Instance;
            var config = model.BleSystemConfiguration;
            config.BleReceivers.Clear();
            config.BleTransmitters.Clear();
            config.ReceiverPaths.Clear();
            config.ReceiverPaths.AddRange(ReceiverPaths);
            foreach (var receiverViewModel in Receivers)
            {
                receiverViewModel.UpdateEntity();
                config.BleReceivers[receiverViewModel.BleReceiver] = receiverViewModel.BleReceiver;
            }

            foreach (var transmitterViewModel in Transmitters)
            {
                transmitterViewModel.UpdateEntity();
                var transmitter = transmitterViewModel.BleTransmitter;
                config.BleTransmitters[transmitter.MacAddress] = transmitter;
            }
            model.SaveConfiguration();
            InsertMessage("Configuration Saved");
            if (OnSaved != null)
                OnSaved();

        }

        private void OnAddReceiver()
        {
            var viewModel = new ReceiverEntryViewModel(new BleReceiver());
            viewModel.OnRemove += vm => Receivers.Remove(vm);
            Receivers.Add(viewModel);
        }

        public RelayCommand AddTransmitterCommand
        {
            get { return _addTransmitterCommand ?? (_addTransmitterCommand = new RelayCommand(OnAddTransmitter)); }
        }

        private void OnAddTransmitter()
        {
            var bleTrans = new BleTransmitter();
            var viewModel = new TransmitterEntryViewModel(bleTrans);
            viewModel.OnRemove += vm => Transmitters.Remove(vm);
            Transmitters.Add(viewModel);
        }
    }
}