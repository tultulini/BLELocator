using System.Drawing;

namespace BLELocator.Core
{
    public class BleTransmitter
    {
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public PointF Position { get; set; }
        public float ErrorRadius { get; set; }
    }
}