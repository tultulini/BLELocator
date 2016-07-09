using System;
using System.Drawing;
using System.Windows.Media;
using BLELocator.Core.Contracts.Entities;
using BLELocator.Core.Utils;
using GalaSoft.MvvmLight;
using Brush = System.Windows.Media.Brush;
using MediaColor = System.Windows.Media.Color;

namespace BLELocator.UI.ViewModels
{
    public class TransmitterViewModel:ViewModelBase
    {
        private string _transmitterName;
        private string _macAddress;
        private string _holderFirstName;
        private string _holderLastName;
        private DateTime _lastSeenTime;
        private MediaColor _colorCode;
        public BleTransmitter BleTransmitter { get; set; }

        

        public void UpdateEntity()
        {

            BleTransmitter.TransmitterName = TransmitterName;
            BleTransmitter.TagHolder.FirstName = HolderFirstName;
            BleTransmitter.MacAddress = MacAddress;
            BleTransmitter.TagHolder.LastName = HolderLastName;
            BleTransmitter.ColorCode = System.Drawing.Color.FromArgb(ColorCode.A, ColorCode.R, ColorCode.G, ColorCode.B);
        }
        public TransmitterViewModel(BleTransmitter transmitter)
        {
            BleTransmitter = transmitter;
            TransmitterName = transmitter.TransmitterName;
            MacAddress = transmitter.MacAddress;
            if(transmitter.TagHolder == null)
                transmitter.TagHolder = new TagHolder();
            HolderFirstName = transmitter.TagHolder.FirstName;
            HolderLastName= transmitter.TagHolder.LastName;
            ColorCode =
                (System.Windows.Media.Color.FromArgb(transmitter.ColorCode.A, transmitter.ColorCode.R,
                    transmitter.ColorCode.G, transmitter.ColorCode.B));
        }
        public string TransmitterName
        {
            get { return _transmitterName; }
            set
            {
                _transmitterName = value;
                RaisePropertyChanged(() => TransmitterName);
                RaisePropertyChanged(() => IsValid);
            }
        }

        public string MacAddress
        {
            get { return _macAddress; }
            set
            {
                _macAddress = value;
                RaisePropertyChanged(() => MacAddress);
                RaisePropertyChanged(() => IsValid);
            }
        }

        public bool IsValid { get { return MacAddress.IsValidMacAddress() && TransmitterName.HasSomething(); }}
       

       

        public string HolderFirstName
        {
            get { return _holderFirstName; }
            set
            {
                _holderFirstName = value;
                RaisePropertyChanged(() => HolderFirstName);
            }
        }

        public string HolderLastName
        {
            get { return _holderLastName; }
            set
            {
                _holderLastName = value;
                RaisePropertyChanged(() => HolderLastName);
            }
        }

        public DateTime LastSeenTime
        {
            get { return _lastSeenTime; }
            set
            {
                _lastSeenTime = value; 
                RaisePropertyChanged(()=>LastSeenTime);
            }
        }

        public MediaColor ColorCode
        {
            get { return _colorCode; }
            set
            {
                _colorCode = value;
                RaisePropertyChanged(() => ColorCode);
            }
        }
    }
}