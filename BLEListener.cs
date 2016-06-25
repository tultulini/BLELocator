using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BLELocator
{
    public class BLEListener
    {
        private TcpListener _tcpListener;

        public BLEListener()
        {
            Init();
        }

        private void Init()
        {
            IPAddress address = IPAddress.Parse("0.0.0.0");
            _tcpListener = new TcpListener(address, 1234);
            StartListening();
            
        }

        private void StartListening()
        {
            _tcpListener.Start();
            var client = _tcpListener.AcceptTcpClient();
            while (true)
            {
                Thread.Sleep(100);
                var stream = client.GetStream();
                var bytes = new Byte[16000];
                var readByteCount = stream.Read(bytes, 0, bytes.Length);
                ProcessMessage(client, bytes, readByteCount);
            }
        }

        private void ProcessMessage(TcpClient client, byte[] bytes, int readByteCount)
        {
            if(readByteCount<=0)
                return;
            var text = ASCIIEncoding.ASCII.GetString(bytes, 0, readByteCount);
            Console.Write(text);

        }
    }
}