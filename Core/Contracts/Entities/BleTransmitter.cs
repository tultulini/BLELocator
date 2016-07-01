using System.Drawing;

namespace BLELocator.Core.Contracts.Entities
{
    public class BleTransmitter
    {
        public TagHolder TagHolder { get; set; }
        public string TransmitterName { get; set; }
        public string MacAddress { get; set; }
        public PointF Position { get; set; }
        public float ErrorRadius { get; set; }
    }
}