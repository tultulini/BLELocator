using System;
using System.Drawing;
using System.Net;
using BLELocator.Core.Utils;
using Newtonsoft.Json;

namespace BLELocator.Core.Contracts.Entities
{
    public class BleReceiver : IEquatable<BleReceiver>
    {
        [JsonConverter(typeof(JsonConverterIPAddress))]
        public IPAddress IPAddress { get; set; }
        public int IncomingPort { get; set; }
        public string LocationName { get; set; }
        public PointF Position { get; set; }
        public bool Equals(BleReceiver other)
        {
            return other != null && Equals(IPAddress, other.IPAddress);
        }

        public override string ToString()
        {
            return string.Format("IPAddress:{0}, IncomingPort: {1}, LocationName: {2}, Position: {3}, IsEnabled: {4}",
                IPAddress, IncomingPort, LocationName, Position, IsEnabled);
        }

        public override int GetHashCode()
        {
            return IPAddress == null ? -1 : IPAddress.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BleReceiver);
        }

        public bool IsEnabled { get; set; }
    }
}