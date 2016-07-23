using System.Drawing;
using System.Threading.Tasks;

namespace BLELocator.Core.Contracts.Entities
{
    public class BleTransmitter
    {
        public BleTransmitter()
        {
            Position = new PointF(float.MaxValue, float.MaxValue);
        }

        private bool _initialized = false;
        private const double BigAssNumber = 10e6;

        public bool PositionInitialized
        {
            get
            {
                if (_initialized)
                    return true;
                _initialized = Position.X < BigAssNumber;
                return _initialized;
            }
        }

        public TagHolder TagHolder { get; set; }
        public string TransmitterName { get; set; }
        public string MacAddress { get; set; }
        public PointF Position { get; set; }
        public float ErrorRadius { get; set; }
        public Color ColorCode{ get; set; }
        public float VisualOffset { get; set; }
    }
}