using System;
using System.Drawing;

namespace BLELocator.Core.Utils
{
    public class GeometryUtil
    {
        private const double HalfPi = Math.PI / 2.0;
        public static double GetDistance(PointF positionA, PointF positionB)
        {
            var dx = (positionB.X - positionA.X);
            var dy = (positionB.Y - positionA.Y);
            if (Math.Abs(dx) < 0.01)
                return Math.Abs(dy);
            if (Math.Abs(dy) < 0.01)
                return Math.Abs(dx);

            return Math.Sqrt(dx * dx + dy * dy);
        }
        public static double GetAngle(PointF positionA, PointF positionB)
        {
            var dx = (positionB.X - positionA.X);
            var dy = (positionB.Y - positionA.Y);
            if (Math.Abs(dx) < 0.01)
                return dy > 0 ? HalfPi : (-1.0) * HalfPi;
            if (Math.Abs(dy) < 0.01)
                return dx > 0 ? 0 : Math.PI;
            return Math.Atan(dy / dx);
        }
        public static PointF CalculatePointInBetween(PointF refPosition, PointF selectedNextPoint)
        {
            return new PointF((refPosition.X + selectedNextPoint.X) / 2, (refPosition.Y + selectedNextPoint.Y) / 2);
        }
        public static PointF CalculatePointInBetween(PointF refPosition, PointF selectedNextPoint, double distance)
        {
            var distanceBetweenPoints = GetDistance(refPosition, selectedNextPoint);
            if (distanceBetweenPoints <= distance)
                return selectedNextPoint;
            var ratio = (float)(distance/distanceBetweenPoints);

            return new PointF((selectedNextPoint.X - refPosition.X) * ratio + refPosition.X, (selectedNextPoint.Y - refPosition.Y) * ratio + refPosition.Y);
        }
        
    }
}