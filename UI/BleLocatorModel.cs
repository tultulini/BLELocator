using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Documents;
using BLELocator.Core;
using Newtonsoft.Json;

namespace UI
{
    public class BleLocatorModel
    {
        public List<BLEUdpListener> BleUdpListeners { get; set; }
        private BleSystemConfiguration _systemConfiguration;
        public BleLocatorModel()
        {
            BleUdpListeners = new List<BLEUdpListener>();
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            var directory = Directory.GetCurrentDirectory();
            var configFilePath = Path.Combine(directory, typeof (BleSystemConfiguration).Name + ".json");
            if (!File.Exists(configFilePath))
            {
                _systemConfiguration = new BleSystemConfiguration
                {
                    BleReceivers = new Dictionary<string, BleReceiver> { {"rec1",new BleReceiver{IncomingPort = 11000, IPAddress = new IPAddress(new byte[]{10,0,0,7}),LocationName = "first in hall"} }},
                    BleTransmitters = new Dictionary<string, BleTransmitter> { { "LBHCOFIPDFGE", new BleTransmitter { Name = "LBHCOFIPDFGE", MacAddress = "D0:39:72:E5:8F:35" } } }
                };
                var json = JsonConvert.SerializeObject(_systemConfiguration);
                using (var writer = new StreamWriter(configFilePath,false))
                {
                    writer.Write(json);
                }
                throw new Exception("Config not set");
            }
            foreach (var receiver in _systemConfiguration.BleReceivers)
            {
                var listener = new BLEUdpListener(receiver.Value.IncomingPort);
                listener.MessageParser.OnDeviceDiscovery += OnDeviceDiscovery;
                BleUdpListeners.Add(listener);
                
            }


        }

        private void OnDeviceDiscovery(DeviceDiscoveryEvent obj)
        {
            throw new NotImplementedException();
        }
    }
}