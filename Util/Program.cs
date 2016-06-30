using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLELocator.Core;
using BLELocator.Core.Contracts.Entities;

namespace Util
{
    class Program
    {
        static void Main(string[] args)
        {
            //var listener = new BLEUdpListener(11000);
            ActivateUdpListener();
        }

        private static void ActivateUdpListener()
        {
            var listener = new BLEUdpListener(new BleReceiver { IncomingPort = 11000 });
            listener.OnDeviceDiscovery += Console.WriteLine;
            listener.StartListener();
        }

        private static void SimFileParser()
        {
            BleFileParser parser = new BleFileParser(@"C:\Users\talf\Documents\Visual Studio 2013\Projects\BLELocator\k1.txt", Console.WriteLine);
            
            parser.Start();
            Console.ReadLine();
        }
    }
}
