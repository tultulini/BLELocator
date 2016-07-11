using System;
using System.Collections.ObjectModel;
using BLELocator.Core.Contracts.Entities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace BLELocator.UI.ViewModels
{
    public class CapturePickerViewModel:ViewModelBase
    {
        private EventCaptureSession _selectedSession;
        private RelayCommand _okCommand;
        public ObservableCollection<EventCaptureSession> CaptureSessions { get; set; }
        public event Action OnOk;
        public RelayCommand OkCommand
        {
            get { return _okCommand ?? (_okCommand = new RelayCommand(OnOkCommand)); }
        }

        private void OnOkCommand()
        {
            if (OnOk != null)
                OnOk();
        }

        public EventCaptureSession SelectedSession
        {
            get { return _selectedSession; }
            set
            {
                _selectedSession = value;
                RaisePropertyChanged(()=>SelectedSession);
            }
        }
    }
}