using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using BLELocator.Core;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;
using Newtonsoft.Json;

namespace BLELocator.UI
{
    public class BleLocatorModel
    {
        public List<BLEUdpListener> BleUdpListeners { get; set; }
        private EventCaptureSession _capturedEventsSession;
        public event Action<BleReceiver,ConnectionState> OnConnectionStateChanged;
        private static BleLocatorModel _instance;
        private readonly static object _instanceLock = new object();

        private static string _configFilePath = Path.Combine(Directory.GetCurrentDirectory(),
            typeof (BleSystemConfiguration).Name + ".json");

        private const int InitialEventCount = 10000;
        private bool _capturingEvents = false;
        private readonly ActionBlock<DeviceDiscoveryEvent> _captureEventHandler;

        private object _captureLock = new object();
        public event Action<string> OnLogMessage;

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
            _capturedEventsSession = new EventCaptureSession(); 
            _captureEventHandler = new ActionBlock<DeviceDiscoveryEvent>((ce) => CaptureEvent(ce), new ExecutionDataflowBlockOptions{MaxDegreeOfParallelism = 5});
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
            else
            {
                string json;
                using (var reader = new StreamReader(File.OpenRead(_configFilePath)))
                {
                    json = reader.ReadToEnd();
                }

                BleSystemConfiguration = JsonConvert.DeserializeObject<BleSystemConfiguration>(json);
            }


        }

        public void Connect()
        {
            if (BleUdpListeners.HasSomething())
            {
                foreach (var bleUdpListener in BleUdpListeners)
                {
                    bleUdpListener.Stop();
                    bleUdpListener.OnDeviceDiscovery -= OnDeviceDiscovery;
                }
                BleUdpListeners.Clear();
            }
            foreach (var receiver in BleSystemConfiguration.BleReceivers)
            {
                if(!receiver.Value.IsEnabled)
                    continue;
                var listener = new BLEUdpListener(receiver.Value);
                listener.OnDeviceDiscovery += OnDeviceDiscovery;
                BleUdpListeners.Add(listener);
                listener.StartListener();
            }
            
        }

        public void Disconnect()
        {
            if (BleUdpListeners.HasSomething())
            {
                foreach (var bleUdpListener in BleUdpListeners)
                {
                    bleUdpListener.Stop();
                    bleUdpListener.OnDeviceDiscovery -= OnDeviceDiscovery;
                }
                BleUdpListeners.Clear();
            }
        }

        public void CapturingEventsStart()
        {
            StopCapturing();
            _capturedEventsSession.Reset();
            _capturedEventsSession.Start = DateTime.Now;
            OnLogMessage("Starting Capture");
            foreach (var bleTransmitter in BleSystemConfiguration.BleTransmitters)
            {
                _capturedEventsSession.CapturedEvents.Add(bleTransmitter.Value.TransmitterName, new List<DeviceDiscoveryEvent>(InitialEventCount));
            }
            _capturingEvents = true;
        }
        private  void CaptureEvent(DeviceDiscoveryEvent discoveryEvent)
        {
            if (!_capturingEvents)
                return;
            List<DeviceDiscoveryEvent> events;
            lock (_captureLock)
            {
                if (!_capturingEvents || discoveryEvent.DeviceDetails.Name.IsNullOrEmpty())
                    return;
                if (_capturedEventsSession.CapturedEvents.TryGetValue(discoveryEvent.DeviceDetails.Name, out events))
                    events.Add(discoveryEvent); 
            }
        }

        public void CapturingEventsFinalize(string comments)
        {

            lock (_captureLock)
            {
                OnLogMessage("Finalizing Capture");

                var captureDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Captures");
                _capturedEventsSession.Comments = comments;
                if (!Directory.Exists(captureDirectory))
                    Directory.CreateDirectory(captureDirectory);
                var now = DateTime.Now;
                var jsonFilePath = Path.Combine(captureDirectory,
                    string.Format("DeciveEventCapture{0:yyyy_MM_dd_hhmmssfff}.json",now));
                var json = JsonConvert.SerializeObject(_capturedEventsSession);
                using (var writer = new StreamWriter(new FileStream(jsonFilePath,FileMode.Create,FileAccess.Write)))
                {
                    writer.Write(json);
                }
                _capturedEventsSession.Reset();
                Process.Start("explorer.exe", string.Format("/select,\"{0}\"", jsonFilePath));
            }
        }

        public void StopCapturing()
        {
            lock (_captureLock)
            {
                OnLogMessage("Halting Capture");
                _capturingEvents = false;
                //_captureEventHandler.Complete();
                //_captureEventHandler.Completion.Wait();
                _capturedEventsSession.End = DateTime.Now;
                
            }
        }
        private void OnDeviceDiscovery(DeviceDiscoveryEvent discoveryEvent)
        {
            if (_capturingEvents)
            {
                _captureEventHandler.Post(discoveryEvent);
                if(discoveryEvent.DeviceDetails.Name.IsNullOrEmpty() || !BleSystemConfiguration.BleTransmitters.ContainsKey(discoveryEvent.DeviceDetails.Name))
                    return;
                OnLogMessage(string.Format("Discovered: Name: {0}, RSSI: {1}", discoveryEvent.DeviceDetails.Name,
                    discoveryEvent.Rssi));

            }
        }

        public void CheckConnectivity()
        {
            foreach (var bleReceiver in BleSystemConfiguration.BleReceivers.Values)
            {
                var ping = new Ping();
                PingReply reply = ping.Send(bleReceiver.IPAddress);
                if (reply == null || reply.Status != IPStatus.Success)
                {
                    OnLogMessage(string.Format("No response from {0}", bleReceiver.IPAddress));
                }
                else
                {
                    OnLogMessage(string.Format("{0} is connected", bleReceiver.IPAddress));
                    
                }
            }
        }
    }
}