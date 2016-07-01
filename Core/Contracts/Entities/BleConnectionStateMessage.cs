using BLELocator.Core.Contracts.Enums;

namespace BLELocator.Core.Contracts.Entities
{
    public class BleConnectionStateMessage
    {
        public BleReceiver Receiver { get; set; }
        public BleConnectionState ConnectionState { get; set; }
        public string Message { get; set; }
    }
}