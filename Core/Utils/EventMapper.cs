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
        private const double HalfPi = Math.PI / 2.0;
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
                    if (receiverTransmitters.ContainsKey(t.TransmitterName))
                        return;
                    receiverTransmitters.Add(t.TransmitterName,
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
                    transmitter.Position = CalculatePointInBetween(transmitter.Position,
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
                double angle = GetAngle(signalEventDetails.BleReceiver.Position,
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
                var selectedNextPoint = GetDistance(nextMirrors.Item1, refPosition) <
                                        GetDistance(nextMirrors.Item2, refPosition)
                    ? nextMirrors.Item1
                    : nextMirrors.Item2;

                if (!eventPosition.HasValue)
                {
                    var currentMirrors = detectionMirrors[i];
                    refPosition = GetDistance(currentMirrors.Item1, selectedNextPoint) <
                                        GetDistance(currentMirrors.Item2, selectedNextPoint)
                    ? currentMirrors.Item1
                    : currentMirrors.Item2;
                }
                eventPosition = CalculatePointInBetween(refPosition, selectedNextPoint);
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
            if (!eventPosition.HasValue)
                return;
            transmitter.Position = eventPosition.Value;
            if (TransmitterPositionDiscovered != null)
                TransmitterPositionDiscovered(transmitter);
        }

        private PointF CalculatePointInBetween(PointF refPosition, PointF selectedNextPoint)
        {
            return new PointF((refPosition.X + selectedNextPoint.X) / 2, (refPosition.Y + selectedNextPoint.Y) / 2);
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
            if (!receiverTransmitters.TryGetValue(deviceDiscoveryEvent.DeviceDetails.Name, out eventDetails))
                return;
            eventDetails.Rssi = deviceDiscoveryEvent.Rssi;
            eventDetails.TimeStamp = deviceDiscoveryEvent.TimeStamp;
            eventDetails.Distance = _signalToDistanceConverter.GetDistance(deviceDiscoveryEvent.Rssi);
            if (TransmitterSignalDiscovered != null)
                TransmitterSignalDiscovered(eventDetails);
        }
    }
}