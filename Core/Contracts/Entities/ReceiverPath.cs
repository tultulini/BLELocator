using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using BLELocator.Core.Utils;

namespace BLELocator.Core.Contracts.Entities
{
    public class ReceiverPath : IEquatable<ReceiverPath>
    {
        private double _distance;

        public override int GetHashCode()
        {
            var fromHash = From.GetHashCode();
            var toHash = To.GetHashCode();
            return (fromHash > toHash) ? ((fromHash * 397) ^ toHash) : ((toHash * 397) ^ fromHash);
        }

        public ReceiverPath(BleReceiver from, BleReceiver to)
        {
            From = from;
            To = to;
            WayPoints = new ObservableCollection<PointF>();
        }

        public double Distance
        {
            get
            {
                if (_distance > 0)
                    return _distance;
                if (WayPoints.IsNullOrEmpty())
                    _distance = GeometryUtil.GetDistance(From.Position, To.Position);
                else
                {
                    _distance = GeometryUtil.GetDistance(From.Position, WayPoints.First()) +
                                GeometryUtil.GetDistance(WayPoints.Last(), To.Position);
                    var wayPointCount = WayPoints.Count;
                    if (wayPointCount > 1)
                    {
                        for (int i = 0; i < wayPointCount - 1; i++)
                        {
                            _distance += GeometryUtil.GetDistance(WayPoints[i], WayPoints[i + 1]);
                        }
                    }
                }
                return _distance;
            }

        }

        public BleReceiver GetOther(BleReceiver receiver)
        {
            if (Equals(receiver, From))
                return To;
            if (Equals(receiver, To))
                return From;
            return null;
        }
        public bool PointOnPath(BleReceiver origin, PointF location, out double distance)
        {
            PointF start = origin.Position;
            distance = 0;
            PointF end;
            bool backwards;
            if (Equals(origin,From))
            {
                end = To.Position;
                backwards = false;
            }
            else if (Equals(origin,To))
            {
                end = From.Position;
                backwards = true;
            }
            else
            {
                return false;
            }
            if (WayPoints.IsNullOrEmpty())
                return GeometryUtil.PointOnPath(location, start, end, out distance);
            PointF firstWayPoint, lastWayPoint;
            var waypPontCount = WayPoints.Count;

            if (backwards)
            {
                firstWayPoint = WayPoints.Last();
                if (GeometryUtil.PointOnPath(location, start, firstWayPoint, out distance))
                    return true;
                var totalDistance = GeometryUtil.GetDistance(start, firstWayPoint);
                for (int i = waypPontCount - 1; i > 0; i--)
                {
                    if (GeometryUtil.PointOnPath(location, WayPoints[i], WayPoints[i - 1], out distance))
                    {
                        distance += totalDistance;
                        return true;
                    }
                    totalDistance += GeometryUtil.GetDistance(WayPoints[i], WayPoints[i + 1]);
                }
                lastWayPoint = WayPoints.First();
                if (GeometryUtil.PointOnPath(location, lastWayPoint, end, out distance))
                {
                    distance += totalDistance;

                    return true;
                }

            }

            firstWayPoint = WayPoints.First();
            if (GeometryUtil.PointOnPath(location, start, firstWayPoint, out distance))
                return true;

            for (int i = 0; i < waypPontCount - 1; i++)
            {
                if (GeometryUtil.PointOnPath(location, WayPoints[i], WayPoints[i + 1], out distance))
                    return true;
            }
            lastWayPoint = WayPoints.Last();
            return GeometryUtil.PointOnPath(location, lastWayPoint, end, out distance);

        }
        public PointF FindPointInPath(BleReceiver source, double distance)
        {
            PointF end, lastWayPoint;
            double diff;
            var start = source.Position;
            if (Equals(source,From))
            {
                end = To.Position;
                if (distance >= Distance)
                    return end;
                if (WayPoints.IsNullOrEmpty())
                    return GeometryUtil.CalculatePointInBetween(start, end, distance);
                var firstWayPoint = WayPoints.First();

                var currentDistance = GeometryUtil.GetDistance(start, firstWayPoint);
                if (distance < currentDistance)
                {
                    return GeometryUtil.CalculatePointInBetween(start, firstWayPoint, distance);
                }
                var acccumelativeDistance = currentDistance;
                var wayPointCount = WayPoints.Count;
                for (int i = 0; i < wayPointCount - 1; i++)
                {
                    currentDistance = GeometryUtil.GetDistance(WayPoints[i], WayPoints[i + 1]);
                    //if (acccumelativeDistance + currentDistance > distance)
                    //{

                    //}
                    acccumelativeDistance += currentDistance;
                    if (acccumelativeDistance < distance)
                        continue;
                    diff = currentDistance - (acccumelativeDistance - distance);
                    return GeometryUtil.CalculatePointInBetween(WayPoints[i], WayPoints[i + 1], diff);
                }
                lastWayPoint = WayPoints.Last();
                //lastWayPoint = WayPoints.Last();

            }
            else if (Equals(source,To))
            {
                end = From.Position;
                if (distance >= Distance)
                    return end;
                if (WayPoints.IsNullOrEmpty())
                    return GeometryUtil.CalculatePointInBetween(start, end, distance);
                lastWayPoint = WayPoints.First();
                var currentDistance = GeometryUtil.GetDistance(start, lastWayPoint);

                if (distance < currentDistance)
                {
                    return GeometryUtil.CalculatePointInBetween(start, lastWayPoint, distance);
                }
                var acccumelativeDistance = currentDistance;
                var wayPointCount = WayPoints.Count;
                for (int i = wayPointCount - 1; i > 0; i--)
                {
                    currentDistance = GeometryUtil.GetDistance(WayPoints[i], WayPoints[i - 1]);
                    acccumelativeDistance += currentDistance;
                    if (acccumelativeDistance < distance)
                        continue;
                    diff = currentDistance - (acccumelativeDistance - distance);
                    return GeometryUtil.CalculatePointInBetween(WayPoints[i], WayPoints[i - 1], diff);
                }
            }
            else
            {

                //TODO log warning
                return start;
            }
            diff = GeometryUtil.GetDistance(lastWayPoint, end) - (Distance - distance);
            return GeometryUtil.CalculatePointInBetween(lastWayPoint, end, diff);



        }
        public BleReceiver From { get; set; }
        public BleReceiver To { get; set; }
        public ObservableCollection<PointF> WayPoints { get; set; }
        public bool IsDirect { get { return WayPoints.IsNullOrEmpty(); } }
        public bool Equals(ReceiverPath other)
        {
            return other != null && ((Equals(other.From, From) && Equals(other.To, To)) || (Equals(other.To, From) &&
                                                                                            Equals(other.From, To)));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ReceiverPath);
        }

        public override string ToString()
        {
            string path = null;
            if (WayPoints.HasSomething())
            {
                var pathBuilder = new StringBuilder();
                foreach (var wayPoint in WayPoints)
                {
                    pathBuilder.AppendFormat("[X:{0}, Y:{1}], ", wayPoint.X, wayPoint.Y);
                }
                pathBuilder.Remove(pathBuilder.Length - 2, 2);
                path = string.Format(", WayPoints: {0}", pathBuilder);
            }
            return string.Format("From:{0}, To: {1}", From, To);
        }
    }
}