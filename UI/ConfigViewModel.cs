using System.Collections.ObjectModel;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace BLELocator.UI
{
    public class ConfigViewModel : ViewModelBase
    {
        private RelayCommand _addReceiverCommand;
        private RelayCommand _addTransmitterCommand;

        public ConfigViewModel()
        {
            Transmitters = new ObservableCollection<TransmitterViewModel>();
            Receivers = new ObservableCollection<ReceiverViewModel>();
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
            if (config.BleTransmitters.HasSomething())
            {
                foreach (var transmitter in config.BleTransmitters)
                {
                    var viewModel = new TransmitterViewModel(transmitter.Value);
                    viewModel.OnRemove += (vm) => Transmitters.Remove(vm);
                    Transmitters.Add(viewModel);
                }
            }
        }
        public ObservableCollection<TransmitterViewModel> Transmitters { get; set; }
        public ObservableCollection<ReceiverViewModel> Receivers { get; set; }

        public RelayCommand AddReceiverCommand
        {
            get { return _addReceiverCommand ?? (_addReceiverCommand = new RelayCommand(OnAddReceiver)); }
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