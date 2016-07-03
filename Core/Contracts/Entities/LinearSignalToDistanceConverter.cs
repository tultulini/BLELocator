using System;

namespace BLELocator.Core.Contracts.Entities
{
    public class LinearSignalToDistanceConverter : SignalToDistanceConverterBase
    {
        private readonly LineDetails _lineDetails;
        public LinearSignalToDistanceConverter(LineDetails lineDetails)
        {
            if(lineDetails == null)
                throw new ArgumentNullException("lineDetails");
            _lineDetails = lineDetails;
        }
        public override float GetDistance(int rssi)
        {
            return _lineDetails.Slope*rssi + _lineDetails.Offset;
        }
    }
}