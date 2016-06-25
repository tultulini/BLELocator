using System;

namespace BLELocator.Core
{
    public class DeviceDiscoveryEvent
    {
        public DeviceDetails DeviceDetails { get; set; }
        public DateTime TimeStamp { get; set; }
        public string SourceAddress { get; set; }
    }
}