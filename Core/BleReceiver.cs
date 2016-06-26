using System.Drawing;
using System.Net;

namespace BLELocator.Core
{
    public class BleReceiver
    {
        public IPAddress IPAddress { get; set; }
        public int IncomingPort { get; set; }
        public string LocationName { get; set; }
        public PointF Position { get; set; }
        public override string ToString()
        {
            return string.Format("IPAddress:{0}, IncomingPort: {1}, LocationName: {2}, Position: {3}",IPAddress,IncomingPort,LocationName,Position);
        }
    }
}