using System.Collections.Generic;
using BLELocator.Core.Utils;
using Newtonsoft.Json;

namespace BLELocator.Core.Contracts.Entities
{
    public class BleSystemConfiguration
    {
        public BleSystemConfiguration()
        {
            BleReceivers = new Dictionary<BleReceiver, BleReceiver>();
            BleTransmitters = new Dictionary<string, BleTransmitter>();
            ReceiverPaths = new List<ReceiverPath>(); 
        }
        [JsonConverter(typeof(JsonConverterDictionary<BleReceiver, BleReceiver>))]
        public Dictionary<BleReceiver, BleReceiver> BleReceivers { get; set; }
        [JsonConverter(typeof(JsonConverterDictionary<string, BleTransmitter>))]
        public Dictionary<string, BleTransmitter> BleTransmitters { get; set; }

        public List<ReceiverPath> ReceiverPaths { get; set; }
        public bool KeepCheckingIsAlive { get; set; }
        public int KeepAliveInterval { get; set; }
        public bool UseWeightedPaths { get; set; }
    }
}