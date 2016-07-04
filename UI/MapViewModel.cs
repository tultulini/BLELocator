using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;

namespace BLELocator.UI
{
    public class MapViewModel : LoggedViewModel
    {
        private BleLocatorModel _model;

        
        public ObservableCollection<TransmitterViewModel> Transmitters { get; set; }
        public MapViewModel()
        {

            Init();
        }

        private void Init()
        {
            _model = BleLocatorModel.Instance;
            var config = _model.BleSystemConfiguration;
            foreach (var bleReceiver in config.BleReceivers)
            {
                var receiverTransmitters = new Dictionary<string, DeviceDiscoveryEvent>();

                foreach (var bleTransmitter in config.BleTransmitters)
                {
                    receiverTransmitters.Add(bleTransmitter.Value.TransmitterName, null);
                    Transmitters.Add(new TransmitterViewModel(bleTransmitter.Value));
                }
                _eventsByReceiver.Add(new ReceiverViewModel(bleReceiver.Value), receiverTransmitters);
            }
          

        }

       
    }
}