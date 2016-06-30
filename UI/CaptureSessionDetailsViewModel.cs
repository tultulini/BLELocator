using GalaSoft.MvvmLight;

namespace BLELocator.UI
{
    public class CaptureSessionDetailsViewModel : ViewModelBase
    {
        private string _comments;
        public string Comments
        {
            get { return _comments; }
            set
            {
                _comments = value; 
                RaisePropertyChanged(()=>Comments);
            }
        }
    }
}