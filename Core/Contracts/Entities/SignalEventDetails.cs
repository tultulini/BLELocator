using System;

namespace BLELocator.Core.Contracts.Entities
{
    public class SignalEventDetails
    {
        public SignalEventDetails()
        {
            TimeStamp = DateTime.MinValue;
            Rssi = 0;
            Distance = -1;
        }
        public DateTime TimeStamp { get; set; }
        public int Rssi { get; set; }
        public float Distance { get; set; }
        public BleTransmitter Transmitter { get; set; }
        public SignalEventDetails Clone()
        {
            return (SignalEventDetails) MemberwiseClone();
        }
    }
}