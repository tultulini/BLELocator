using System.Windows;

namespace BLELocator.UI
{
    /// <summary>
    /// Interaction logic for CaptureSessionDetailsView.xaml
    /// </summary>
    public partial class CaptureSessionDetailsView : Window
    {
        public CaptureSessionDetailsView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
