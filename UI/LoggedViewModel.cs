using System;
using System.Collections.ObjectModel;

namespace BLELocator.UI
{
    public abstract class LoggedViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private ObservableCollection<string> _messages;

        public LoggedViewModel()
        {
            Messages = new ObservableCollection<string>();
        }
        public ObservableCollection<string> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                RaisePropertyChanged(() => Messages);
            }
        }
        protected void InsertMessage(string message)
        {
            Messages.Insert(0, string.Format("{0:s} - {1}", DateTime.Now, message));

        }

         
    }
}