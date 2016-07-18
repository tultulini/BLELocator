using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using BLELocator.Core.Utils;

namespace BLELocator.Core.Contracts.Entities
{
    public class ReceiverPath:IEquatable<ReceiverPath>
    {
        private double _distance;

        public override int GetHashCode()
        {
            var fromHash = From.GetHashCode();
            var toHash = To.GetHashCode();
            return (fromHash > toHash) ? ((fromHash*397) ^ toHash) : ((toHash*397) ^ fromHash);
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
                if(_distance>0)
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
                        for (int i = 0; i < wayPointCount-1; i++)
                        {
                            _distance += GeometryUtil.GetDistance(WayPoints[i], WayPoints[i + 1]);
                        }
                    }
                }
                return _distance;
            }
            
        }

        public PointF FindPointInPath(PointF start, double distance)
        {
            
            if (start == From.Position)
            {
                if (distance >= Distance)
                    return To.Position;
                if (WayPoints.IsNullOrEmpty())
                    return GeometryUtil.CalculatePointInBetween(start, To.Position, distance);
                var firstWayPoint = WayPoints.First();

                var currentDistance = GeometryUtil.GetDistance(start, firstWayPoint);

                if (distance < currentDistance)
                {
                    return GeometryUtil.CalculatePointInBetween(start, firstWayPoint, distance);
                }
                

            }
            if (start == To.Position)
            {
                if (distance >= Distance)
                    return From.Position;
                if (WayPoints.IsNullOrEmpty())
                    return GeometryUtil.CalculatePointInBetween(start, From.Position, distance);
                var lastWayPoint = WayPoints.First();
                var currentDistance = GeometryUtil.GetDistance(start, lastWayPoint);

                if (distance < currentDistance)
                {
                    return GeometryUtil.CalculatePointInBetween(start, lastWayPoint, distance);
                }

            }
        }
        public BleReceiver From { get; set; }
        public BleReceiver To { get; set; } 
        public ObservableCollection<PointF> WayPoints { get; set; }
        public bool IsDirect { get { return WayPoints.IsNullOrEmpty(); }}
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