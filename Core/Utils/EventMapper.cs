using System;
using System.Collections.Generic;
using System.Timers;
using BLELocator.Core.Contracts.Entities;

namespace BLELocator.Core.Utils
{
    public class EventMapper
    {
        private readonly Dictionary<BleReceiver, Dictionary<string, SignalEventDetails>> _eventsByReceiver;
        private List<BleTransmitter> _monitoredTransmitters;
        private List<BleReceiver> _monitoredReceivers;
        private readonly SignalToDistanceConverterBase _signalToDistanceConverter = new LinearSignalToDistanceConverter(new LineDetails{Slope = -.2f,Offset = -6});
        public const int DefaultEventTimeout = 2;
        private static TimeSpan _eventLifeSpan = new TimeSpan(0, 0, 0, DefaultEventTimeout);
        private const int ScanInterval = 1000;
        private System.Timers.Timer _scanTimer;
        public EventMapper(List<BleReceiver>receivers, List<BleTransmitter> transmitters )
        {
            _eventsByReceiver = new Dictionary<BleReceiver, Dictionary<string, SignalEventDetails>>(receivers.Count);
            _monitoredReceivers = receivers;
            _monitoredTransmitters = transmitters;
            foreach (var bleReceiver in receivers)
            {
                if(_eventsByReceiver.ContainsKey(bleReceiver))
                    continue;
                var receiverTransmitters = new Dictionary<string, SignalEventDetails>(transmitters.Count);
                transmitters.ForEach(t =>
                {
                    if(receiverTransmitters.ContainsKey(t.TransmitterName))
                        return;
                    receiverTransmitters.Add(t.TransmitterName, new SignalEventDetails());
                });
                _eventsByReceiver.Add(bleReceiver, receiverTransmitters);
            }
            _scanTimer = new Timer(ScanInterval);
            _scanTimer.Elapsed += ScanIntervalElapsed;
        }
        private void ScanIntervalElapsed(object sender, ElapsedEventArgs e)
        {
            _scanTimer.Stop();
            var now = DateTime.Now;
            foreach (var monitoredTransmitter in _monitoredTransmitters)
            {
                var relevantSignals = new List<SignalEventDetails>(_monitoredReceivers.Count);
                foreach (var monitoredReceiver in _monitoredReceivers)
                {
                    var signalDetails = _eventsByReceiver[monitoredReceiver][monitoredTransmitter.TransmitterName].Clone();
                    if ((signalDetails.TimeStamp - now) < _eventLifeSpan)
                    {
                        relevantSignals.Add(signalDetails);
                    }
                }
            }

        }
        public void HandleDiscoveryEvent(DeviceDiscoveryEvent deviceDiscoveryEvent)
        {
            Dictionary<string, SignalEventDetails> receiverTransmitters;
            if(!_eventsByReceiver.TryGetValue(deviceDiscoveryEvent.BleReceiver, out receiverTransmitters))
                return;
            SignalEventDetails eventDetails;
            if(!receiverTransmitters.TryGetValue(deviceDiscoveryEvent.DeviceDetails.Name, out eventDetails))
                return;
            eventDetails.Rssi = deviceDiscoveryEvent.Rssi;
            eventDetails.TimeStamp = deviceDiscoveryEvent.TimeStamp;
            eventDetails.Distance = _signalToDistanceConverter.GetDistance(deviceDiscoveryEvent.Rssi);
        }
    }
}