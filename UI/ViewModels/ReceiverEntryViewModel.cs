using System;
using BLELocator.Core.Contracts.Entities;
using GalaSoft.MvvmLight.Command;

namespace BLELocator.UI.ViewModels
{
    public class ReceiverEntryViewModel : ReceiverViewModel
    {
        public ReceiverEntryViewModel(BleReceiver receiver) : base(receiver)
        {
        }

        public event Action<ReceiverEntryViewModel> OnRemove;
        private RelayCommand _removeReceiverCommand;
        public RelayCommand RemoveReceiverCommand
        {
            get { return _removeReceiverCommand ?? (_removeReceiverCommand = new RelayCommand(OnRemoveReceiver)); }

        }

        private void OnRemoveReceiver()
        {
            OnRemove(this);
        }
    }
}