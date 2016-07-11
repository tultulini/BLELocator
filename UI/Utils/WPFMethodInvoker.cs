using System;
using System.Security.RightsManagement;
using System.Windows;
using System.Windows.Threading;

namespace BLELocator.UI.Utils
{
    public class WPFMethodInvoker
    {
        public static void InvokeAsync(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Delegate)action);
            
        }
    }
}