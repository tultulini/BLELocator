using System;
using System.Collections.Generic;

namespace BLELocator.Core.Contracts.Entities
{
    public class EventCaptureSession
    {
        public EventCaptureSession()
        {
            CapturedEvents = new Dictionary<string, List<DeviceDiscoveryEvent>>();
        }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Comments { get; set; }
        public Dictionary<string, List<DeviceDiscoveryEvent>> CapturedEvents { get; set; }

        public void Reset()
        {
            Comments = null;
            CapturedEvents.Clear();
            
        }
    }
}