using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

namespace BLELocator.Core
{
    public class BleMessageParser
    {
        private const string MessageStart = ">";
        private MessageWrapper _previousMessage;
        public event Action<DeviceDiscoveryEvent> OnDeviceDiscovery;
        private readonly ActionBlock<MessageWrapper> _finalizedMessageProcessor;
        private const string UnkownDeviceName = "Unknown";


        private const string MacAddressKey = "bdaddr";
        private const string DeviceNameKey = "Complete local name";
        readonly Regex _regexMacAddress = new Regex(@"^[0-9a-fA-F]{2}(((:[0-9a-fA-F]{2}){5})|((:[0-9a-fA-F]{2}){5}))$");
        private const string RssiKey = "RSSI:";
        private readonly string _messageSource;
        public BleMessageParser(string messageSource)
        {
            if(messageSource.IsNullOrEmpty())
                throw new ArgumentNullException("messageSource");
            _messageSource = messageSource;
            _finalizedMessageProcessor = new ActionBlock<MessageWrapper>(messageWrapper=>ProcessFinalizedMessage(messageWrapper));
            _previousMessage = new MessageWrapper();
            
        }

        private void ProcessFinalizedMessage(MessageWrapper messageWrapper)
        {
            var now = DateTime.Now;
            var message = messageWrapper.MessageParts.ToString(messageWrapper.MessageStartCharIndex,
                messageWrapper.MessageEnd - messageWrapper.MessageStartCharIndex + 1);
            
            if (message.IsNullOrEmpty() || message.IndexOf(UnkownDeviceName, StringComparison.Ordinal) >= 0)
            {
                return;
            }
            var lines = message.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            var extractions = new Dictionary<string, Action<DeviceDiscoveryEvent, string>>()
            {
                {MacAddressKey, ExtractMacAddress},
                {DeviceNameKey, ExtractDeviceName},
                {RssiKey, ExtractRSSI}
            };
            var discoveryEvent = new DeviceDiscoveryEvent{DeviceDetails = new DeviceDetails(),SourceAddress = _messageSource, TimeStamp = now};
            foreach (var line in lines)
            {
                string foundKey = null;
                foreach (var extraction in extractions)
                {
                    if (line.IndexOf(extraction.Key, StringComparison.Ordinal) < 0) 
                        continue;

                    foundKey = extraction.Key;
                    extraction.Value(discoveryEvent, line);
                    break;
                }
                if (foundKey != null)
                {
                    extractions.Remove(foundKey);
                }
            }
            OnDeviceDiscovery(discoveryEvent);
        }

        private void ExtractMacAddress(DeviceDiscoveryEvent discoveryEvent, string line)
        {
            var lineParts = line.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var linePart in lineParts)
            {
                if (_regexMacAddress.IsMatch(linePart))
                {
                    discoveryEvent.DeviceDetails.MacAddress = linePart;
                    return;
                }
                
            }
        }
        private void ExtractDeviceName(DeviceDiscoveryEvent discoveryEvent, string line)
        {
            var nameStartIndex = line.IndexOf("'", StringComparison.Ordinal);
            if (nameStartIndex < 0 || nameStartIndex>line.Length-3)
            {
                Console.WriteLine("Couldn't extract device name from  {0}", line);
                return;
            }
            nameStartIndex++;
            var nameEndIndex = line.IndexOf("'", nameStartIndex , StringComparison.Ordinal);
            if (nameEndIndex < 0)
            {
                Console.WriteLine("Couldn't extract device name from  {0} - couldn't find end of name", line);
                return;
            }
            discoveryEvent.DeviceDetails.Name = line.Substring(nameStartIndex, nameEndIndex - nameStartIndex);
        }
        private void ExtractRSSI(DeviceDiscoveryEvent discoveryEvent, string line)
        {
            var lineParts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (lineParts.Length <= 1)
            {
                Console.WriteLine("Can't extract RSSI from {0}", line);
                return;
            }
            foreach (var linePart in lineParts)
            {
                int rssi;
                if (int.TryParse(linePart, out rssi))
                {
                    discoveryEvent.Rssi = rssi;
                    return;
                }
            }
        }
        public void ProcessMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            var messageBuilder = _previousMessage.MessageParts;
            _previousMessage.MessageParts.Append(message);
            int messageStartIndex;
            var messageEndIndex = -1;
            while (true)
            {
                message = _previousMessage.MessageParts.ToString();
                messageStartIndex = _previousMessage.MessageStartCharIndex;
                if (messageStartIndex< 0)
                {
                     messageStartIndex = message.IndexOf(MessageStart, messageEndIndex + 1, StringComparison.Ordinal);
                    if (messageStartIndex < 0)
                    {
                        return;
                    }
                    _previousMessage.MessageStartCharIndex = messageStartIndex;
                }
                messageEndIndex = message.IndexOf(MessageStart, messageStartIndex + 1, StringComparison.Ordinal);
                _previousMessage.MessageEnd = messageEndIndex - 1;
                if (messageEndIndex < 0)
                    return;
                ProcessFinalizedMessage(_previousMessage);

                _previousMessage.MessageParts.Remove(0, messageEndIndex);
                messageEndIndex = -1;
                _previousMessage.ResetIndices();
                if(_previousMessage.MessageParts.Length == 0)
                    return;
            }
        }
    }
}