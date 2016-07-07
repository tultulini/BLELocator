using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace BLELocator.UI.ViewModels
{
    public abstract class LoggedViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private ObservableCollection<string> _messages;
        private const int MaxMessageCount = 50;

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
            var action = new Action(() =>
            {
                if(Messages.Count>MaxMessageCount)
                    Messages.RemoveAt(Messages.Count-1);
                Messages.Insert(0, message);
            });
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,(Delegate) action);

            

        }

         
    }
}