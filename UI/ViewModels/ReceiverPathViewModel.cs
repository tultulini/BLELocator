using System.Drawing;
using BLELocator.Core.Contracts.Entities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace BLELocator.UI.ViewModels
{
    public class ReceiverPathViewModel :ViewModelBase
    {
        private RelayCommand _addNewWayPointCommand;
        private float _newWayPointX;
        private float _newWayPointY;
        private RelayCommand _clearWayPointsCommand;
        public ReceiverPath ReceiverPath { get; set; }

        public ReceiverPathViewModel(ReceiverPath receiverPath)
        {
            ReceiverPath = receiverPath;
        }

        public RelayCommand AddNewWayPointCommand
        {
            get { return _addNewWayPointCommand ?? (_addNewWayPointCommand = new RelayCommand(OnAddNewWayPoint)); }
            
        }

        public RelayCommand ClearWayPointsCommand
        {
            get { return _clearWayPointsCommand ?? (_clearWayPointsCommand = new RelayCommand(OnClearWayPoints)); }
        }

        private void OnClearWayPoints()
        {
            ReceiverPath.WayPoints.Clear();
        }

        public float NewWayPointX
        {
            get { return _newWayPointX; }
            set
            {
                _newWayPointX = value;
                RaisePropertyChanged(()=>NewWayPointX);
            }
        }

        public float NewWayPointY
        {
            get { return _newWayPointY; }
            set
            {
                _newWayPointY = value;
                RaisePropertyChanged(() => NewWayPointY);
                
            }
        }

        private void OnAddNewWayPoint()
        {
            ReceiverPath.WayPoints.Add(new PointF(NewWayPointX,NewWayPointY));
        }
    }
}