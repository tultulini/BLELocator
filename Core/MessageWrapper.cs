using System.Collections.Generic;

namespace BLELocator.Core
{
    public class MessageWrapper
    {
        public List<string> MessageParts { get; set; }
        public int MessageStartCharIndex { get; set; }
        public int MessageEnd{ get; set; }
    }
}