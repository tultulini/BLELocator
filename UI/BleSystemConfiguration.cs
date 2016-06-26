using System.Collections.Generic;
using System.Windows.Documents;
using BLELocator.Core;

namespace UI
{
    public class BleSystemConfiguration
    {
        public Dictionary<string,BleReceiver> BleReceivers { get; set; }
        public Dictionary<string,BleTransmitter> BleTransmitters { get; set; }
    }
}