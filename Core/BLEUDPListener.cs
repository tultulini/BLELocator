using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BLELocator.Core
{
    public class BLEUdpListener
    {
        private readonly int _listenPort;//11000
        public BleMessageParser MessageParser { get; set; }
        public BLEUdpListener(int listenPort)
        {
            _listenPort = listenPort;
            
            MessageParser = new BleMessageParser(string.Format("Port = {0}", _listenPort));
        }

        public  void StartListener()
        {
            var done = false;

            var listener = new UdpClient(_listenPort);
            var groupEP = new IPEndPoint(IPAddress.Any, _listenPort);

            try
            {
                while (!done)
                {
                    Thread.Sleep(100);
                    //Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);
                    
                    var message = Encoding.ASCII.GetString(bytes);
                    //Console.Write(
                    //    message);
                    MessageParser.ProcessMessage(message);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener.Close();
            }
        } 
    }
}