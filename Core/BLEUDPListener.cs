using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Contracts.Enums;
using BLELocator.Core.Utils;

namespace BLELocator.Core
{
    public class BLEUdpListener
    {
        private readonly int _listenPort;//11000
        private UdpClient _listener;
        private BleMessageParser MessageParser { get; set; }
        public event Action<DeviceDiscoveryEvent> OnDeviceDiscovery = delegate {  };
        public event Action<BleConnectionStateMessage> OnConnectionStateChanged;
        private bool _keepReading;
        private readonly BleReceiver _receiver;
        public BLEUdpListener(BleReceiver   receiver)
        {
            if(receiver == null)
                throw new ArgumentNullException("receiver");
            _receiver = receiver;
            _listenPort = _receiver.IncomingPort;

            MessageParser = new BleMessageParser(string.Format("Port = {0}", _listenPort), de =>
            {
                de.BleReceiver = _receiver;
                OnDeviceDiscovery(de);
            });
        }

        public void StartListener()
        {
            _keepReading = true;
            try
            {
                if (_listener != null)
                    _listener.Close();
                _listener = new UdpClient(new IPEndPoint(IPAddress.Any, _listenPort));

            }
            catch (Exception exception)
            {
                var errorMessage = string.Format("Couldn't listen to Port {0}. Exception: {1}", _listenPort,
                    exception);
                Console.WriteLine(errorMessage);
                if (OnConnectionStateChanged != null)
                {
                    var connectionStateMessage = new BleConnectionStateMessage
                    {
                        Receiver = _receiver,
                        ConnectionState = BleConnectionState.Disconnected,
                        Message = errorMessage
                    };
                    OnConnectionStateChanged(connectionStateMessage);
                }
                return;

            }
            if (OnConnectionStateChanged != null)
            {
                var connectionStateMessage = new BleConnectionStateMessage
                {
                    Receiver = _receiver,
                    ConnectionState = BleConnectionState.Connected,
                    Message = string.Format("Listening to port {0}",_receiver.IncomingPort)
                };
                OnConnectionStateChanged(connectionStateMessage);
            }
            Task.Run(() => ListenerLoop());

        }

        private async Task ListenerLoop()
        {
            try
            {
                while (_keepReading)
                {
                    //await Task.Delay(100);


                    //Console.WriteLine("Waiting for broadcast");
                    var bufferResult = await _listener.ReceiveAsync();
                    if (bufferResult.Buffer.IsNullOrEmpty())
                        continue;

                    var message = Encoding.ASCII.GetString(bufferResult.Buffer);
                    //Console.Write(
                    //    message);
                    MessageParser.ProcessMessage(message);
                }

            }
            catch (Exception exception)
            {
                var errorMessage = string.Format("Exception occured in ListenerLoop. Not listening to Port {0}. Exception: {1}", _listenPort,
                    exception);
                Console.WriteLine(errorMessage);
                if (OnConnectionStateChanged != null)
                {
                    var connectionStateMessage = new BleConnectionStateMessage
                    {
                        Receiver = _receiver,
                        ConnectionState = BleConnectionState.Disconnected,
                        Message = errorMessage
                    };
                    OnConnectionStateChanged(connectionStateMessage);
                }
            }

        }
        public void Stop()
        {
            _keepReading = false;
            if (_listener == null)
                return;
            _listener.Close();
            if (OnConnectionStateChanged != null)
            {
                var connectionStateMessage = new BleConnectionStateMessage
                {
                    Receiver = _receiver,
                    ConnectionState = BleConnectionState.Disconnected,
                    Message = string.Format("Stopped listening to port {0}", _receiver.IncomingPort)
                };
                OnConnectionStateChanged(connectionStateMessage);
            }
        }
    }
}