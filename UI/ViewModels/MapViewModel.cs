using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;
using BLELocator.UI.Models;

namespace BLELocator.UI.ViewModels
{
    public class MapViewModel : LoggedViewModel
    {
        private BleLocatorModel _model;

        
        public ObservableCollection<TransmitterViewModel> Transmitters { get; set; }
        private  EventMapper _eventMapper;
        public MapViewModel()
        {

            Init();
            
        }

        
        private void Init()
        {
            _model = BleLocatorModel.Instance;
            var config = _model.BleSystemConfiguration;
            _eventMapper = new EventMapper(config.BleReceivers.Values.ToList(), config.BleTransmitters.Values.ToList());
            foreach (var bleReceiver in config.BleReceivers)
            {
                var receiverTransmitters = new Dictionary<string, DeviceDiscoveryEvent>();

                foreach (var bleTransmitter in config.BleTransmitters)
                {
                    receiverTransmitters.Add(bleTransmitter.Value.TransmitterName, null);
                    Transmitters.Add(new TransmitterViewModel(bleTransmitter.Value));
                }
                //_eventsByReceiver.Add(new ReceiverViewModel(bleReceiver.Value), receiverTransmitters);
            }
          

        }

       
    }
}