using System;
using System.Drawing;
using System.Net;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;
using GalaSoft.MvvmLight;

namespace BLELocator.UI.ViewModels
{
    public class ReceiverViewModel : ViewModelBase, IEquatable<ReceiverViewModel>
    {
        private float _positionX;
        private float _positionY;
        private string _ipAddress;
        private int _incomingPort;
        private string _locationName;
        private bool _isEnabled;
        private int _signalPassUpperBound;
        private int _signalPassLowerBound;
        public BleReceiver BleReceiver { get; private set; }

        public override int GetHashCode()
        {
            return BleReceiver.GetHashCode();
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            return Equals(obj as ReceiverViewModel);
        }
        public bool Equals(ReceiverViewModel other)
        {
            return other != null && IPAddress == other.IPAddress;
        }

        public ReceiverViewModel(BleReceiver receiver)
        {
            BleReceiver = receiver;
            if (receiver.IPAddress != null)
            {
                IPAddress = receiver.IPAddress.ToString();

            }
            IncomingPort = receiver.IncomingPort;
            LocationName = receiver.LocationName;
            PositionX = receiver.Position.X;
            PositionY = receiver.Position.Y;
            IsEnabled = receiver.IsEnabled;
            SignalPassUpperBound = receiver.SignalPassUpperBound;
            SignalPassLowerBound = receiver.SignalPassLowerBound;
        }

        public void UpdateEntity()
        {

            BleReceiver.IPAddress = IPAddress.IsNullOrEmpty() ? null : System.Net.IPAddress.Parse(IPAddress);
            BleReceiver.IncomingPort = IncomingPort;
            BleReceiver.LocationName = LocationName;
            BleReceiver.Position = new PointF(PositionX, PositionY);
            BleReceiver.IsEnabled = IsEnabled;
            BleReceiver.SignalPassLowerBound = SignalPassLowerBound;
            BleReceiver.SignalPassUpperBound = SignalPassUpperBound;
        }
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                IPAddress ipAddress;
                if (System.Net.IPAddress.TryParse(value, out ipAddress))
                {
                    _ipAddress = value;
                }
                RaisePropertyChanged(() => IPAddress);
                RaisePropertyChanged(() => IsValid);

            }
        }

        public int IncomingPort
        {
            get { return _incomingPort; }
            set
            {
                _incomingPort = value;
                RaisePropertyChanged(() => IncomingPort);
                RaisePropertyChanged(() => IsValid);
            }
        }

        public string LocationName
        {
            get { return _locationName; }
            set
            {
                _locationName = value;
                RaisePropertyChanged(() => LocationName);
            }
        }

        public float PositionX
        {
            get { return _positionX; }
            set
            {
                _positionX = value;
                RaisePropertyChanged(() => PositionX);
            }
        }

        public float PositionY
        {
            get { return _positionY; }
            set
            {
                _positionY = value;
                RaisePropertyChanged(() => PositionY);
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value; 
                RaisePropertyChanged(()=>IsEnabled);
            }
        }

        public int SignalPassUpperBound
        {
            get { return _signalPassUpperBound; }
            set
            {
                _signalPassUpperBound = value;
                RaisePropertyChanged(()=>SignalPassUpperBound);
            }
        }

        public int SignalPassLowerBound
        {
            get { return _signalPassLowerBound; }
            set
            {
                _signalPassLowerBound = value;
                RaisePropertyChanged(() => SignalPassLowerBound);
                
            }
        }

        public bool IsValid { get { return IPAddress.IsValidIP() && IncomingPort > 0; } }
      
    }
}