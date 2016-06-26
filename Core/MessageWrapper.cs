using System.Collections.Generic;
using System.Text;

namespace BLELocator.Core
{
    public class MessageWrapper
    {
        private const int InitialMessageSize = 4096;
        //public List<string> MessageParts { get; set; }
        public StringBuilder MessageParts { get; set; }
        public int MessageStartCharIndex { get; set; }
        public int MessageEnd{ get; set; }

        public MessageWrapper()
        {
            MessageParts = new StringBuilder(InitialMessageSize);
            ResetIndices();
        }
        public void ResetIndices()
        {
            MessageStartCharIndex = -1;
            MessageEnd = -1;
        }
    }
}