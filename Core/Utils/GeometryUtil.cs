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
            var ratio = (float)(distance / distanceBetweenPoints);

            return new PointF((selectedNextPoint.X - refPosition.X) * ratio + refPosition.X, (selectedNextPoint.Y - refPosition.Y) * ratio + refPosition.Y);
        }


        public static bool PointOnPath(PointF objective, PointF pathStart, PointF pathEnd, out double distanceFromStart)
        {
            distanceFromStart = 0;
            const double allowedError = 0.1;
            double maxY, minY, minX, maxX;
            if (pathEnd.X > pathStart.X)
            {
                maxX = pathEnd.X + allowedError;
                minX = pathStart.X - allowedError;
            }
            else
            {
                minX = pathEnd.X-allowedError;
                maxX = pathStart.X+allowedError;

            }

            if (pathEnd.Y > pathStart.Y)
            {
                maxY = pathEnd.Y+allowedError;
                minY = pathStart.Y-allowedError;
            }
            else
            {
                minY = pathEnd.Y-allowedError;
                maxY = pathStart.Y+allowedError;

            }
            //check if in region
            if (objective.X > maxX || objective.X < minX || objective.Y > maxY || objective.Y < minY)
                return false;

            
            var pathLength = GetDistance(pathStart, pathEnd);
            var startToObjectiveLength = GetDistance(pathStart, objective);
            var bjectiveToEndLength = GetDistance(objective,pathEnd);
            distanceFromStart = startToObjectiveLength;
            return Math.Abs(startToObjectiveLength + bjectiveToEndLength - pathLength) < allowedError;
        }
    }
}