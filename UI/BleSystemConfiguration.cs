using System.Collections.Generic;
using BLELocator.Core.Contracts.Entities;

namespace BLELocator.UI
{
    public class BleSystemConfiguration
    {
        public Dictionary<BleReceiver, BleReceiver> BleReceivers { get; set; }
        public Dictionary<string,BleTransmitter> BleTransmitters { get; set; }
    }
}