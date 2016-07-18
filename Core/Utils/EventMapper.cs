using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using BLELocator.Core.Contracts.Entities;

namespace BLELocator.Core.Utils
{
    public class EventMapper
    {
        private readonly Dictionary<BleReceiver, Dictionary<string, SignalEventDetails>> _eventsByReceiver;
        private List<BleTransmitter> _monitoredTransmitters;
        private List<BleReceiver> _monitoredReceivers;
        private readonly SignalToDistanceConverterBase _signalToDistanceConverter = new LinearSignalToDistanceConverter(new LineDetails { Slope = -.2f, Offset = -6 });
        public const int DefaultEventTimeoutMsec = 1200;
        private static TimeSpan _eventLifeSpan = new TimeSpan(0, 0, 0, 0, DefaultEventTimeoutMsec);
        private const int ScanInterval = 1000;
        private Timer _scanTimer;
        private readonly ActionBlock<List<SignalEventDetails>> _eventGroupHandler;
            public event Action<BleTransmitter> TransmitterPositionDiscovered;
        public event Action<SignalEventDetails> TransmitterSignalDiscovered;
        public EventMapper(List<BleReceiver> receivers, List<BleTransmitter> transmitters)
        {
            _eventGroupHandler = new ActionBlock<List<SignalEventDetails>>(deg => HandleEventGroup(deg), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });
            _eventsByReceiver = new Dictionary<BleReceiver, Dictionary<string, SignalEventDetails>>(receivers.Count);
            _monitoredReceivers = receivers;
            _monitoredTransmitters = transmitters;
            foreach (var bleReceiver in receivers)
            {
                if (_eventsByReceiver.ContainsKey(bleReceiver))
                    continue;
                var receiverTransmitters = new Dictionary<string, SignalEventDetails>(transmitters.Count);
                transmitters.ForEach(t =>
                {
                    if (receiverTransmitters.ContainsKey(t.MacAddress))
                        return;
                    receiverTransmitters.Add(t.MacAddress,
                        new SignalEventDetails { Transmitter = t, BleReceiver = bleReceiver });
                });
                _eventsByReceiver.Add(bleReceiver, receiverTransmitters);
            }
            _scanTimer = new Timer(ScanInterval);
            _scanTimer.Elapsed += ScanIntervalElapsed;
            _scanTimer.Enabled = true;
        }

        private void HandleEventGroup(List<SignalEventDetails> eventGroup)
        {
            if (eventGroup.IsNullOrEmpty())
                return;
            var groupCount = eventGroup.Count;
            var transmitter = eventGroup.First().Transmitter;

            if (groupCount == 1)
            {

                var signalEventDetails = eventGroup.First();
                if (transmitter.Position == PointF.Empty)
                {
                    transmitter.Position = signalEventDetails.BleReceiver.Position;
                }
                else
                {
                    transmitter.Position = GeometryUtil.CalculatePointInBetween(transmitter.Position,
                        signalEventDetails.BleReceiver.Position);
                }
                if (TransmitterPositionDiscovered != null)
                    TransmitterPositionDiscovered(transmitter);
                return;
            }
            var orderedGroup = eventGroup.OrderByDescending(e => e.Rssi).ToList();

            var detectionMirrors = new List<Tuple<PointF, PointF>>(groupCount);
            int totalSignalWeight = 0;
            for (int i = 0; i < groupCount; i++)
            {
                var signalEventDetails = orderedGroup[i];
                signalEventDetails.Distance = _signalToDistanceConverter.GetDistance(signalEventDetails.Rssi);
                var otherSignalEventDetails = i < groupCount - 1 ? orderedGroup[i + 1] : orderedGroup[i - 1];
                double angle = GeometryUtil.GetAngle(signalEventDetails.BleReceiver.Position,
                    otherSignalEventDetails.BleReceiver.Position);
                var mirrors = CreateMirroredPoints(signalEventDetails.BleReceiver.Position, signalEventDetails.Distance,
                    angle);
                detectionMirrors.Add(mirrors);
                totalSignalWeight += signalEventDetails.Rssi;
            }
            PointF? eventPosition = null;
            //get first mirror closest to leading sensor then advance to each mirror closest to event position
            for (int i = 1; i < groupCount - 1; i++)
            {
                var signalEventDetails = orderedGroup[i];
                var nextMirrors = detectionMirrors[i + 1];
                PointF refPosition;
                if (eventPosition.HasValue)
                    refPosition = eventPosition.Value;
                else
                {
                    refPosition = signalEventDetails.BleReceiver.Position;

                }
                var selectedNextPoint = GeometryUtil.GetDistance(nextMirrors.Item1, refPosition) <
                                        GeometryUtil.GetDistance(nextMirrors.Item2, refPosition)
                    ? nextMirrors.Item1
                    : nextMirrors.Item2;

                if (!eventPosition.HasValue)
                {
                    var currentMirrors = detectionMirrors[i];
                    refPosition = GeometryUtil.GetDistance(currentMirrors.Item1, selectedNextPoint) <
                                        GeometryUtil.GetDistance(currentMirrors.Item2, selectedNextPoint)
                    ? currentMirrors.Item1
                    : currentMirrors.Item2;
                }
                eventPosition = GeometryUtil.CalculatePointInBetween(refPosition, selectedNextPoint);
               

            }
            if (!eventPosition.HasValue)
                return;
            transmitter.Position = eventPosition.Value;
            if (TransmitterPositionDiscovered != null)
                TransmitterPositionDiscovered(transmitter);
        }

        

       
       

        private Tuple<PointF, PointF> CreateMirroredPoints(PointF origin, double distance, double angle)
        {
            //TODO optimize with explicit caluculation (for straight angles)
            var dy = (float)(distance * Math.Sin(angle));
            var dx = (float)(distance * Math.Cos(angle));
            return new Tuple<PointF, PointF>(new PointF(origin.X - dx, origin.X - dy),
                new PointF(origin.X + dx, origin.Y + dy));

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
                    var signalDetails = _eventsByReceiver[monitoredReceiver][monitoredTransmitter.MacAddress].Clone();
                    if ((now - signalDetails.TimeStamp) < _eventLifeSpan)
                    {
                        relevantSignals.Add(signalDetails);
                    }
                }
                if (relevantSignals.IsNullOrEmpty())
                    continue;
                _eventGroupHandler.Post(relevantSignals);
            }
            _scanTimer.Start();
        }
        public void HandleDiscoveryEvent(DeviceDiscoveryEvent deviceDiscoveryEvent)
        {
            Dictionary<string, SignalEventDetails> receiverTransmitters;
            if (!_eventsByReceiver.TryGetValue(deviceDiscoveryEvent.BleReceiver, out receiverTransmitters))
                return;
            SignalEventDetails eventDetails;
            if (!receiverTransmitters.TryGetValue(deviceDiscoveryEvent.DeviceDetails.MacAddress, out eventDetails))
                return;
            eventDetails.Rssi = deviceDiscoveryEvent.Rssi;
            eventDetails.TimeStamp = deviceDiscoveryEvent.TimeStamp;
            eventDetails.Distance = _signalToDistanceConverter.GetDistance(deviceDiscoveryEvent.Rssi);
            if (TransmitterSignalDiscovered != null)
                TransmitterSignalDiscovered(eventDetails);
        }
    }
}