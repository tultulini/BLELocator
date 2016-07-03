using System;
using BLELocator.Core.Contracts.Entities;
using GalaSoft.MvvmLight.Command;

namespace BLELocator.UI
{
    public class TransmitterEntryViewModel : TransmitterViewModel
    {
        public event Action<TransmitterEntryViewModel> OnRemove;
        private RelayCommand _removeTransmitterCommand;
        public RelayCommand RemoveTransmitterCommand
        {
            get { return _removeTransmitterCommand ?? (_removeTransmitterCommand = new RelayCommand(OnRemoveTransmitter)); }
        }

        private void OnRemoveTransmitter()
        {
            OnRemove(this);
        }
        public TransmitterEntryViewModel(BleTransmitter transmitter) : base(transmitter)
        {
        }
    }
}