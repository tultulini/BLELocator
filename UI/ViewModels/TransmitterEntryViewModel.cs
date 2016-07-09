using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using BLELocator.Core.Contracts.Entities;
using GalaSoft.MvvmLight.Command;
using Brush = System.Drawing.Brush;
using Brushes = System.Drawing.Brushes;
using Color = System.Windows.Media.Color;

namespace BLELocator.UI.ViewModels
{
    public class TransmitterEntryViewModel : TransmitterViewModel
    {
        public event Action<TransmitterEntryViewModel> OnRemove;
        private RelayCommand _removeTransmitterCommand;
        private ObservableCollection<Color> _availableColors;

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
            AvailableColors = new ObservableCollection<Color>(new List<Color> { Colors.Lime, Colors.Orange, Colors.Cyan, Colors.Magenta, Colors.Red });
            
        }

        public ObservableCollection<Color> AvailableColors
        {
            get { return _availableColors; }
            set
            {
                _availableColors = value;
                RaisePropertyChanged(() => AvailableColors);
            }
        }
    }
}