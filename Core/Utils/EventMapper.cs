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
        public const int DefaultEventTimeout = 2;
        private static TimeSpan _eventLifeSpan = new TimeSpan(0, 0, 0, DefaultEventTimeout);
        private const int ScanInterval = 1000;
        private System.Timers.Timer _scanTimer;
        private readonly ActionBlock<List<SignalEventDetails>> _eventGroupHandler;
        private const double HalfPi = Math.PI / 2.0;
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
                    if (receiverTransmitters.ContainsKey(t.TransmitterName))
                        return;
                    receiverTransmitters.Add(t.TransmitterName,
                        new SignalEventDetails { Transmitter = t, BleReceiver = bleReceiver });
                });
                _eventsByReceiver.Add(bleReceiver, receiverTransmitters);
            }
            _scanTimer = new Timer(ScanInterval);
            _scanTimer.Elapsed += ScanIntervalElapsed;
        }

        private void HandleEventGroup(List<SignalEventDetails> eventGroup)
        {
            var orderedGroup = eventGroup.OrderByDescending(e => e.Rssi).ToList();
            var groupCount = orderedGroup.Count;
            var detectionMirrors = new List<Tuple<PointF, PointF>>(groupCount);
            int totalSignalWeight = 0;
            for (int i = 0; i < groupCount; i++)
            {
                var signalEventDetails = orderedGroup[i];
                signalEventDetails.Distance = _signalToDistanceConverter.GetDistance(signalEventDetails.Rssi);
                var otherSignalEventDetails = i < groupCount - 1 ? orderedGroup[i + 1] : orderedGroup[i - 1];
                double angle = GetAngle(signalEventDetails.BleReceiver.Position,
                    otherSignalEventDetails.BleReceiver.Position);
                var mirrors = CreateMirroredPoints(signalEventDetails.BleReceiver.Position, signalEventDetails.Distance,
                    angle);
                detectionMirrors.Add(mirrors);
                totalSignalWeight += signalEventDetails.Rssi;
            }
            PointF? eventPosition;
            //get first mirror closest to leading sensor then advance to each mirror closest to event position
            for (int i = 0; i < groupCount-1; i++)
            {
                var currentMirrors = detectionMirrors[i];
                var signalEventDetails = orderedGroup[i];
                var nextMirrors = detectionMirrors[i + 1];
                var receiverPosition = signalEventDetails.BleReceiver.Position;
                var selectedNextPoint = GetDistance(nextMirrors.Item1, receiverPosition) <
                                        GetDistance(nextMirrors.Item2, receiverPosition)
                    ? nextMirrors.Item1
                    : nextMirrors.Item2;
                //var distance1_1 = GetDistance(currentMirrors.Item1, nextMirrors.Item1);
                //var distance2_1 = GetDistance(currentMirrors.Item2, nextMirrors.Item1);
                //var distance1_2 = GetDistance(currentMirrors.Item1, nextMirrors.Item2);
                //var distance2_2 = GetDistance(currentMirrors.Item2, nextMirrors.Item2);
                //if (distance1_1 <= distance2_1)
                //{
                //    if (distance1_1 < distance1_2)
                //    {

                //    }

                //}
            }
        }

        private double GetAngle(PointF positionA, PointF positionB)
        {
            var dx = (positionB.X - positionA.X);
            var dy = (positionB.Y - positionA.Y);
            if (Math.Abs(dx) < 0.01)
                return dy > 0 ? HalfPi : (-1.0) * HalfPi;
            if (Math.Abs(dy) < 0.01)
                return dx > 0 ? 0 : Math.PI;
            return Math.Atan(dy / dx);
        }
        private double GetDistance(PointF positionA, PointF positionB)
        {
            var dx = (positionB.X - positionA.X);
            var dy = (positionB.Y - positionA.Y);
            if (Math.Abs(dx) < 0.01)
                return Math.Abs(dy);
            if (Math.Abs(dy) < 0.01)
                return Math.Abs(dx);

            return Math.Sqrt(dx * dx + dy * dy);
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
                    var signalDetails = _eventsByReceiver[monitoredReceiver][monitoredTransmitter.TransmitterName].Clone();
                    if ((signalDetails.TimeStamp - now) < _eventLifeSpan)
                    {
                        relevantSignals.Add(signalDetails);
                    }
                }
                if (relevantSignals.IsNullOrEmpty())
                    continue;
                _eventGroupHandler.Post(relevantSignals);
            }

        }
        public void HandleDiscoveryEvent(DeviceDiscoveryEvent deviceDiscoveryEvent)
        {
            Dictionary<string, SignalEventDetails> receiverTransmitters;
            if (!_eventsByReceiver.TryGetValue(deviceDiscoveryEvent.BleReceiver, out receiverTransmitters))
                return;
            SignalEventDetails eventDetails;
            if (!receiverTransmitters.TryGetValue(deviceDiscoveryEvent.DeviceDetails.Name, out eventDetails))
                return;
            eventDetails.Rssi = deviceDiscoveryEvent.Rssi;
            eventDetails.TimeStamp = deviceDiscoveryEvent.TimeStamp;
            //eventDetails.Distance = _signalToDistanceConverter.GetDistance(deviceDiscoveryEvent.Rssi);
        }
    }
}