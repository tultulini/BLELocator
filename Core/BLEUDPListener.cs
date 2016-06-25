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
        List<string> messages = new List<string>(5000);
        public BLEUdpListener(int listenPort)
        {
            _listenPort = listenPort;
            StartListener();
        }

        private  void StartListener()
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
                    Console.Write(
                        message);
                    messages.Add(message);
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