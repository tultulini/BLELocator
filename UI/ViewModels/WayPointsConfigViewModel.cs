using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BLELocator.Core.Contracts.Entities;
using BLELocator.UI.Models;
using GalaSoft.MvvmLight;

namespace BLELocator.UI.ViewModels
{
    public class WayPointsConfigViewModel :ViewModelBase
    {

        

        public WayPointsConfigViewModel(List<ReceiverPath> receiverPaths)
        {
            
            BuildPairs(receiverPaths);
        }


        private void BuildPairs(List<ReceiverPath> receiverPaths)
        {
            var model = BleLocatorModel.Instance;
            var receivers = model.BleSystemConfiguration.BleReceivers;
            
            var pathDictionary = receiverPaths.ToDictionary(r=>r);
            foreach (var currentReceiver in receivers.Values)
            {
                foreach (var otherReceiver in receivers.Values)
                {
                    if(Equals(currentReceiver,otherReceiver))
                        continue;
                    var path = new ReceiverPath(currentReceiver, otherReceiver);
                    if (pathDictionary.ContainsKey(path))
                        continue;
                    pathDictionary.Add(path,path);
                }
            }


            ReceiverPaths = new ObservableCollection<ReceiverPathViewModel>(pathDictionary.Values.Select(p => new ReceiverPathViewModel(p)));
        }

        public ObservableCollection<ReceiverPathViewModel> ReceiverPaths { get; set; }
    }
}