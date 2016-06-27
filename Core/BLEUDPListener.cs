using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BLELocator.Core.Utils;

namespace BLELocator.Core
{
    public class BLEUdpListener
    {
        private readonly int _listenPort;//11000
        private UdpClient _listener;
        public BleMessageParser MessageParser { get; set; }
        private bool _keepReading;
        public BLEUdpListener(int listenPort)
        {
            _listenPort = listenPort;
            
            MessageParser = new BleMessageParser(string.Format("Port = {0}", _listenPort));
        }

        public  void StartListener()
        {
            _keepReading = true;
            if(_listener !=null)
                _listener.Close();
            _listener = new UdpClient( new IPEndPoint(IPAddress.Any,_listenPort));

            
            Task.Run(() => ListenerLoop());

        }

        private async Task ListenerLoop()
        {
            try
            {
                while (!_keepReading)
                {
                    //await Task.Delay(100);
                    
                    
                    //Console.WriteLine("Waiting for broadcast");
                    var bufferResult =await _listener.ReceiveAsync();
                    if(bufferResult.Buffer.IsNullOrEmpty())
                        continue;
                    
                    var message = Encoding.ASCII.GetString(bufferResult.Buffer);
                    //Console.Write(
                    //    message);
                    MessageParser.ProcessMessage(message);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }
        public void Stop()
        {
            _keepReading = false;
            if(_listener==null)
                return;
            _listener.Close();
        }
    }
}