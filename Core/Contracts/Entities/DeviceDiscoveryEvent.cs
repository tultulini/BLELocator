using System;

namespace BLELocator.Core.Contracts.Entities
{
    public class DeviceDiscoveryEvent
    {
        public BleReceiver BleReceiver { get; set; }
        public DeviceDetails DeviceDetails { get; set; }
        public DateTime TimeStamp { get; set; }
        public string SourceAddress { get; set; }
        public int Rssi { get; set; }
        public override string ToString()
        {
            return string.Format("DeviceDetails: [{0}], SourceAddress: {1}, Rssi: {2}, TimeStamp: {3:yyyy-MM-dd hh:mm:ss.fff}", DeviceDetails,
                SourceAddress, Rssi, TimeStamp);
        }
    }
}