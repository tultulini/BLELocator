using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BLELocator.Core;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Contracts.Enums;
using BLELocator.Core.Utils;
using Newtonsoft.Json;

namespace BLELocator.UI.Models
{
    public class BleLocatorModel
    {
        public List<BLEUdpListener> BleUdpListeners { get; set; }
        private EventCaptureSession _capturedEventsSession;
        public event Action<BleConnectionState> OnConnectionStateChanged;
        private static BleLocatorModel _instance;
        private readonly static object _instanceLock = new object();

        private static string _configFilePath = Path.Combine(Directory.GetCurrentDirectory(),
            typeof(BleSystemConfiguration).Name + ".json");

        private const int InitialEventCount = 10000;
        private bool _capturingEvents = false;
        private bool _monitoringEvents = false;
        private readonly ActionBlock<DeviceDiscoveryEvent> _discoveredEventHandler;

        private object _captureLock = new object();
        public event Action<string> OnLogMessage;
        public event Action<DeviceDiscoveryEvent> OnRegisteredTransmitterEvent;

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
            _discoveredEventHandler = new ActionBlock<DeviceDiscoveryEvent>((ce) => HandleDiscoveryEvent(ce), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });
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
            if (BleSystemConfiguration.BleTransmitters.HasSomething())
            {
                float offset = 0;
                foreach (var t in BleSystemConfiguration.BleTransmitters.Values)
                {
                    t.VisualOffset = offset;
                    offset += 0.2f;
                }
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
                if (BleSystemConfiguration.BleTransmitters.HasSomething())
                {
                    float offset = 0;
                    foreach (var t  in BleSystemConfiguration.BleTransmitters.Values)
                    {
                        t.VisualOffset = offset;
                        offset += 0.2f;
                        t.Position = new PointF(0,0);
                    }
                }
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
                if (!receiver.Value.IsEnabled)
                    continue;
                var listener = new BLEUdpListener(receiver.Value);
                listener.OnDeviceDiscovery += OnDeviceDiscovery;
                BleUdpListeners.Add(listener);
                listener.StartListener();
            }
            _monitoringEvents = BleUdpListeners.Any(l => l.IsListening);
            if (OnConnectionStateChanged != null)
                OnConnectionStateChanged(ConnectionState);
        }

        public bool ConnectedToListeners
        {
            get { return BleUdpListeners.HasSomething() && BleUdpListeners.Any(r => r.IsListening); }
        }

        public BleConnectionState ConnectionState
        {
            get
            {
                return ConnectedToListeners
                    ? BleConnectionState.Connected
                    : BleConnectionState.Disconnected;
            }
        }
        public void Disconnect()
        {
            _monitoringEvents = false;
            if (BleUdpListeners.HasSomething())
            {
                foreach (var bleUdpListener in BleUdpListeners)
                {
                    bleUdpListener.Stop();
                    bleUdpListener.OnDeviceDiscovery -= OnDeviceDiscovery;
                }
                BleUdpListeners.Clear();
            }
            if (OnConnectionStateChanged != null)
                OnConnectionStateChanged(ConnectionState);
        }

        public void CapturingEventsStart()
        {
            StopCapturing();
            _capturedEventsSession.Reset();
            _capturedEventsSession.Start = DateTime.Now;
            OnLogMessage("Starting Capture");
            foreach (var bleTransmitter in BleSystemConfiguration.BleTransmitters)
            {
                _capturedEventsSession.CapturedEvents.Add(bleTransmitter.Value.MacAddress, new List<DeviceDiscoveryEvent>(InitialEventCount));
            }
            _capturingEvents = true;
        }
        private void HandleDiscoveryEvent(DeviceDiscoveryEvent discoveryEvent)
        {
            if (!_monitoringEvents)
                return;
            if (OnRegisteredTransmitterEvent != null)
                OnRegisteredTransmitterEvent(discoveryEvent);
            if(!_capturingEvents)
                return;
            List<DeviceDiscoveryEvent> events;
            lock (_captureLock)
            {
                if (!_capturingEvents || discoveryEvent.DeviceDetails.MacAddress.IsNullOrEmpty())
                    return;
                var signalStrength = discoveryEvent.Rssi;
                var receiver = discoveryEvent.BleReceiver;
                if (signalStrength < receiver.SignalPassLowerBound || signalStrength > receiver.SignalPassUpperBound)
                {
                    OnLogMessage(string.Format("Rssi={0} out of range [{1}:{2}] for {3}", signalStrength,
                        receiver.SignalPassLowerBound, receiver.SignalPassUpperBound, receiver.IPAddress));
                    return;
                }
                if (!_capturedEventsSession.CapturedEvents.TryGetValue(discoveryEvent.DeviceDetails.MacAddress, out events))
                    return;
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
                    string.Format("DeciveEventCapture{0:yyyy_MM_dd_hhmmssfff}.json", now));
                var json = JsonConvert.SerializeObject(_capturedEventsSession);
                using (var writer = new StreamWriter(new FileStream(jsonFilePath, FileMode.Create, FileAccess.Write)))
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
            if (_monitoringEvents)
            {

                if (discoveryEvent.DeviceDetails.MacAddress.IsNullOrEmpty() || !BleSystemConfiguration.BleTransmitters.ContainsKey(discoveryEvent.DeviceDetails.MacAddress))
                    return;
                _discoveredEventHandler.Post(discoveryEvent);
                OnLogMessage(string.Format("Discovered: Name: {0}, Mac: {1} RSSI: {2}, IP: {3}", discoveryEvent.DeviceDetails.Name,discoveryEvent.DeviceDetails.MacAddress,
                    discoveryEvent.Rssi,discoveryEvent.BleReceiver.IPAddress));

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

        public async Task ReplayCapture(string fileName)
        {
            string json;
            if (fileName.IsNullOrEmpty() || !File.Exists(fileName))
            {
                OnLogMessage(string.Format("{0} file doesn't exist=> aborting replay", fileName));
                return;
            }
            using (var reader = new StreamReader(File.OpenRead(fileName)))
            {
                json = reader.ReadToEnd();
            }
            if (json.IsNullOrEmpty())
            {
                OnLogMessage(string.Format("{0} is empty => aborting replay", fileName));
                return;
            }
            EventCaptureSession captureSession;
            try
            {
               captureSession = JsonConvert.DeserializeObject<EventCaptureSession>(json);
            }
            catch (Exception exception)
            {
                OnLogMessage(string.Format("Couldn't deserialize {0} => exception: {1}", json,exception));
                return;

                
            }
            OnLogMessage(string.Format("Replay Comments: {0} ", captureSession.Comments));
            await PlaySession(captureSession);
            OnLogMessage(string.Format("Replay {0} finalized ", fileName));

        }

        public async Task PlaySession(EventCaptureSession captureSession)
        {
            _monitoringEvents = true;
            var discoveryEvents = new List<DeviceDiscoveryEvent>();
            foreach (var capturedEvents in captureSession.CapturedEvents)
            {
                discoveryEvents.AddRange(capturedEvents.Value);
            }
            discoveryEvents = discoveryEvents.OrderBy(x => x.TimeStamp).ToList();
            var timeDiffs = new TimeSpan[discoveryEvents.Count];
            for (int i = 0; i < discoveryEvents.Count - 1; i++)
            {
                timeDiffs[i] = discoveryEvents[i + 1].TimeStamp - discoveryEvents[i].TimeStamp;
            }
            for (int i = 0; i < discoveryEvents.Count - 1; i++)
            {
                if (i > 0)
                {
                    await Task.Delay(timeDiffs[i - 1]);
                }
                var discoveryEvent = discoveryEvents[i];
                discoveryEvent.TimeStamp = DateTime.Now;
                OnDeviceDiscovery(discoveryEvent);
            }
            _monitoringEvents = false;
        }
    }
}