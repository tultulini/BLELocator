using System.Collections.Generic;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;
using Newtonsoft.Json;

namespace BLELocator.UI
{
    public class BleSystemConfiguration
    {
        public BleSystemConfiguration()
        {
            BleReceivers = new Dictionary<BleReceiver, BleReceiver>();
            BleTransmitters = new Dictionary<string, BleTransmitter>();
        }
        [JsonConverter(typeof(JsonConverterDictionary<BleReceiver, BleReceiver>))]
        public Dictionary<BleReceiver, BleReceiver> BleReceivers { get; set; }
        [JsonConverter(typeof(JsonConverterDictionary<string, BleTransmitter>))]
        public Dictionary<string, BleTransmitter> BleTransmitters { get; set; }
    }
}