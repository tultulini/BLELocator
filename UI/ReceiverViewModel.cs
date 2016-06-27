using System;
using System.Drawing;
using System.Net;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace BLELocator.UI
{
    public class ReceiverViewModel : ViewModelBase
    {
        private float _positionX;
        private float _positionY;
        private string _ipAddress;
        private int _incomingPort;
        private string _locationName;
        private RelayCommand _removeReceiverCommand;
        public BleReceiver BleReceiver { get; private set; }
        public event Action<ReceiverViewModel> OnRemove;

        public RelayCommand RemoveReceiverCommand
        {
            get { return _removeReceiverCommand ?? (_removeReceiverCommand = new RelayCommand(OnRemoveReceiver)); }
            
        }

        private void OnRemoveReceiver()
        {
            OnRemove(this);
        }

        public ReceiverViewModel(BleReceiver receiver)
        {
            BleReceiver = receiver;
            IPAddress = receiver.IPAddress.ToString();
            IncomingPort = receiver.IncomingPort;
            LocationName = receiver.LocationName;
            PositionX = receiver.Position.X;
            PositionY = receiver.Position.Y;

        }

        public void UpdateEntity()
        {

            BleReceiver.IPAddress = IPAddress.IsNullOrEmpty()?null:System.Net.IPAddress.Parse(IPAddress);
            BleReceiver.IncomingPort = IncomingPort;
            BleReceiver.LocationName = LocationName;
            BleReceiver.Position = new PointF(PositionX, PositionY);

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
                RaisePropertyChanged(()=>IPAddress);
            }
        }

        public int IncomingPort
        {
            get { return _incomingPort; }
            set
            {
                _incomingPort = value;
                RaisePropertyChanged(() => IncomingPort);
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
    }
}