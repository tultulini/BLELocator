using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BLELocator.Core;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;
using Newtonsoft.Json;

namespace BLELocator.UI
{
    public class BleLocatorModel
    {
        public List<BLEUdpListener> BleUdpListeners { get; set; }
        
        private static BleLocatorModel _instance;
        private readonly static object _instanceLock = new object();

        private static string _configFilePath = Path.Combine(Directory.GetCurrentDirectory(),
            typeof (BleSystemConfiguration).Name + ".json");

        public static BleLocatorModel Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                lock (_instanceLock)
                {
                    if (_instance != null)
                        return _instance;
                    var temp = new BleLocatorModel();
                    Thread.MemoryBarrier();
                    _instance = temp;
                    return _instance;
                }
            }
            
        }

        private BleLocatorModel()
        {
            BleUdpListeners = new List<BLEUdpListener>();
            LoadConfiguration();
        }

        public BleSystemConfiguration BleSystemConfiguration { get; private set; }

        public void SaveConfiguration()
        {
            Disconnect();
            var json = JsonConvert.SerializeObject(BleSystemConfiguration);
            using (var writer = new StreamWriter(_configFilePath, false))
            {
                writer.Write(json);
            }
        }
        private void LoadConfiguration()
        {
            var directory = Directory.GetCurrentDirectory();
            if (!File.Exists(_configFilePath))
            {
                BleSystemConfiguration = new BleSystemConfiguration();
               
                return;
            }


        }

        public void Connect()
        {
            if (BleUdpListeners.HasSomething())
            {
                foreach (var bleUdpListener in BleUdpListeners)
                {
                    bleUdpListener.Stop();
                    bleUdpListener.MessageParser.OnDeviceDiscovery -= OnDeviceDiscovery;
                }
                BleUdpListeners.Clear();
            }
            foreach (var receiver in BleSystemConfiguration.BleReceivers)
            {

                var listener = new BLEUdpListener(receiver.Value.IncomingPort);
                listener.MessageParser.OnDeviceDiscovery += OnDeviceDiscovery;
                BleUdpListeners.Add(listener);

            }
            
        }

        public void Disconnect()
        {
            if (BleUdpListeners.HasSomething())
            {
                foreach (var bleUdpListener in BleUdpListeners)
                {
                    bleUdpListener.Stop();
                    bleUdpListener.MessageParser.OnDeviceDiscovery -= OnDeviceDiscovery;
                }
                BleUdpListeners.Clear();
            }
        }

        private void OnDeviceDiscovery(DeviceDiscoveryEvent obj)
        {
            throw new NotImplementedException();
        }
    }
}