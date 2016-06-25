using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace BLELocator.Core
{
    public abstract class BleListenerBase
    {
        private const string MessageStart = ">";
        private MessageWrapper _previousMessage;
        public event Action<DeviceDiscoveryEvent> OnDeviceDiscovery;
        private readonly ActionBlock<MessageWrapper> _finalizedMessageProcessor;

        protected BleListenerBase()
        {
            _finalizedMessageProcessor = new ActionBlock<MessageWrapper>(messageWrapper=>ProcessFinalizedMessage(messageWrapper));
        }

        private void ProcessFinalizedMessage(MessageWrapper messageWrapper)
        {

            var discoveryEvent = new DeviceDiscoveryEvent();
        }

        protected void ProcessMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            while (true)
            {
                var messageEndIndex = -1;

                var messageStartIndex = message.IndexOf(MessageStart, messageEndIndex + 1, StringComparison.Ordinal);
                if (messageStartIndex < 0)
                {
                    if (_previousMessage == null)
                        return;
                    _previousMessage.MessageParts.Add(message);
                    return;
                }
                if (_previousMessage == null)
                    _previousMessage = new MessageWrapper
                    {
                        MessageParts = new List<string> { message },
                        MessageStartCharIndex = messageStartIndex
                    };
                else
                {//finalize previous message
                    if (messageStartIndex > 0)
                    {
                        _previousMessage.MessageParts.Add(message);
                        _previousMessage.MessageEnd = messageStartIndex - 1;
                    }
                    else
                    {
                        _previousMessage.MessageEnd = _previousMessage.MessageParts.Last().Length;
                        
                    }

                }
                messageEndIndex = message.IndexOf(MessageStart, messageStartIndex + 1, StringComparison.Ordinal);
                if (messageEndIndex == -1)
                    return; 
            }

        }
    }
}