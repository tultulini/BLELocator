using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace BLELocator.UI
{
    public class ConfigViewModel : LoggedViewModel
    {
        private RelayCommand _addReceiverCommand;
        private RelayCommand _addTransmitterCommand;
        private RelayCommand _saveConfigurationCommnand;
        private ObservableCollection<string> _messages;

        public ConfigViewModel()
        {
            Transmitters = new ObservableCollection<TransmitterViewModel>();
            Receivers = new ObservableCollection<ReceiverViewModel>();
            Messages = new ObservableCollection<string>();
            LoadExistingConfiguration();
            InsertMessage("Loaded");
        }

        private void LoadExistingConfiguration()
        {
            var config = BleLocatorModel.Instance.BleSystemConfiguration;
            if (config.BleReceivers.HasSomething())
            {
                foreach (var bleReceiver in config.BleReceivers)
                {
                    var viewModel = new ReceiverViewModel(bleReceiver.Value);
                    viewModel.OnRemove += (vm) => Receivers.Remove(vm);
                    Receivers.Add(viewModel);
                }
            }
            else
            {
                var viewModel = new ReceiverViewModel(new BleReceiver());
                viewModel.OnRemove += (vm) => Receivers.Remove(vm);
                Receivers.Add(viewModel);
            }
            if (config.BleTransmitters.HasSomething())
            {
                foreach (var transmitter in config.BleTransmitters)
                {
                    var viewModel = new TransmitterViewModel(transmitter.Value);
                    viewModel.OnRemove += (vm) => Transmitters.Remove(vm);
                    Transmitters.Add(viewModel);
                }
            }
            else
            {
                var viewModel = new TransmitterViewModel(new BleTransmitter());
                viewModel.OnRemove += (vm) => Transmitters.Remove(vm);
                Transmitters.Add(viewModel);
            }
        }

        public ObservableCollection<TransmitterViewModel> Transmitters { get; set; }
        public ObservableCollection<ReceiverViewModel> Receivers { get; set; }

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
            foreach (var receiverViewModel in Receivers)
            {
                receiverViewModel.UpdateEntity();
                config.BleReceivers[receiverViewModel.BleReceiver] = receiverViewModel.BleReceiver;
            }

            foreach (var transmitterViewModel in Transmitters)
            {
                transmitterViewModel.UpdateEntity();
                var transmitter = transmitterViewModel.BleTransmitter;
                config.BleTransmitters[transmitter.TransmitterName] = transmitter;
            }
            model.SaveConfiguration();
            InsertMessage("Configuration Saved");
        }

        private void OnAddReceiver()
        {
            var viewModel = new ReceiverViewModel(new BleReceiver());
            viewModel.OnRemove += (vm) => Receivers.Remove(vm);
            Receivers.Add(viewModel);
        }

        public RelayCommand AddTransmitterCommand
        {
            get { return _addTransmitterCommand ?? (_addTransmitterCommand = new RelayCommand(OnAddTransmitter)); }
        }

        private void OnAddTransmitter()
        {
            var bleTrans = new BleTransmitter();
            var viewModel = new TransmitterViewModel(bleTrans);
            viewModel.OnRemove += (vm) => Transmitters.Remove(vm);
            Transmitters.Add(viewModel);
        }
    }
}