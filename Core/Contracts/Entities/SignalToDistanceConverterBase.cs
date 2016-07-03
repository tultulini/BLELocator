using System;

namespace BLELocator.Core.Contracts.Entities
{
    public abstract class SignalToDistanceConverterBase
    {
        public abstract float GetDistance(int rssi);

    }
}