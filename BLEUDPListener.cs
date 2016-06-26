using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BLELocator
{
    public class BLEUDPListener
    {
        private const int listenPort = 11000;

        public BLEUDPListener()
        {
            StartListener();
        }

        private static void StartListener()
        {
            bool done = false;

            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

            try
            {
                while (!done)
                {
                    Thread.Sleep(100);
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);

                    Console.Write(
                        Encoding.ASCII.GetString(bytes, 0, bytes.Length));
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