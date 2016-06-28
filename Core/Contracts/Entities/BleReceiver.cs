using System;
using System.Drawing;
using System.Net;

namespace BLELocator.Core.Contracts.Entities
{
    public class BleReceiver : IEquatable<BleReceiver>
    {
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
            return string.Format("IPAddress:{0}, IncomingPort: {1}, LocationName: {2}, Position: {3}",IPAddress,IncomingPort,LocationName,Position);
        }

        public override int GetHashCode()
        {
            return IPAddress.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BleReceiver);
        }
    }
}