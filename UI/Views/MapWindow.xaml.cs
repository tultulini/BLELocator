using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BLELocator.Core.Contracts.Entities;
using BLELocator.UI.Utils;
using Brushes = System.Windows.Media.Brushes;

namespace BLELocator.UI.Views
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    using RectangleShape=System.Windows.Shapes.Rectangle;
    public partial class MapWindow : Window
    {
        //private SizeF _mapSizeMetric;
        private SizeF _conversionRatio;
        private PointF _mapOrigin;
        private Dictionary<string, Dictionary<string, Ellipse>> _receiverDetections;
        private Dictionary<string, FrameworkElement> _transmitterPositions;

        
        public MapWindow(BleSystemConfiguration configuration)
        {
            InitializeComponent();
            PrepareMap(configuration);
        }

        private void PrepareMap(BleSystemConfiguration configuration)
        {
            //_mapSizeMetric = new SizeF(60f,60f);
            //_conversionRatio = new SizeF((float) (MapCanvas.Width/_mapSizeMetric.Width), (float) (MapCanvas.Height/_mapSizeMetric.Height));
            _conversionRatio = new SizeF(20f, 20f);
            _mapOrigin = new PointF(10, 20);
            SetOrigin();
            _receiverDetections = new Dictionary<string, Dictionary<string, Ellipse>>();
            foreach (var bleReceiver in configuration.BleReceivers)
            {
                if(!bleReceiver.Value.IsEnabled)
                    continue;
                AddReceiver(bleReceiver.Value);
                AddReceiverTransmitters(bleReceiver.Value, configuration.BleTransmitters);
            }
            _transmitterPositions = new Dictionary<string, FrameworkElement>(configuration.BleTransmitters.Count);
            foreach (var bleTransmitter in configuration.BleTransmitters)
            {
                AddTransmitter(bleTransmitter.Value);

            }
        }

        private void AddTransmitter( BleTransmitter bleTransmitter)
        {
            var humanAverageSize = MetricToLocal(new SizeF(0.5f, 1.8f));
            var shape = new RectangleShape
            {
                Width = humanAverageSize.Width,
                Height = humanAverageSize.Height,
                Stroke = new SolidColorBrush(bleTransmitter.ColorCode.ToMediaColor()),
                StrokeThickness = 4,
                Fill = Brushes.DimGray,
                Style = (Style)this.Resources["AnimateTransmitterStyle"],
                Visibility = Visibility.Hidden
                
            };
            _transmitterPositions.Add(bleTransmitter.TransmitterName,shape);
            MapCanvas.Children.Add(shape);
        }

        public void HandleTransmitterLocationEvent(BleTransmitter transmitter)
        {
            WPFMethodInvoker.InvokeAsync(() =>
            {
                FrameworkElement rectangle;
                if (!_transmitterPositions.TryGetValue(transmitter.TransmitterName, out rectangle))
                {
                    return;
                }
                rectangle.Opacity = 1;
                rectangle.Visibility = Visibility.Hidden;
                SetElementPositionByCenter(rectangle, transmitter.Position);
                rectangle.Visibility = Visibility.Visible;
            });
        }

        public void HandleDiscoveryEvent(BleReceiver receiver, string transmitterName, float distance)
        {
           WPFMethodInvoker.InvokeAsync(() =>
           {
               Dictionary<string, Ellipse> receiverDetections;
               if (!_receiverDetections.TryGetValue(receiver.IPAddress.ToString(), out receiverDetections))
                   return;
               Ellipse ellipse;
               if (!receiverDetections.TryGetValue(transmitterName, out ellipse))
               {
                   return;

               }

               ellipse.Width = distance * _conversionRatio.Width * 2;
               ellipse.Height = distance * _conversionRatio.Height * 2;
               SetElementPositionByCenter(ellipse, receiver.Position);
               ellipse.Opacity = 1;
               ellipse.Visibility = Visibility.Hidden;
               ellipse.Visibility = Visibility.Visible;
           });
        }
        private void AddReceiverTransmitters(BleReceiver receiver, Dictionary<string, BleTransmitter> transmitters)
        {
            var receiverDetections = new Dictionary<string, Ellipse>();
            _receiverDetections.Add(receiver.IPAddress.ToString(), receiverDetections);
            var offset = 0;
            foreach (var bleTransmitter in transmitters)
            {

                var ellipse = new Ellipse
                {
                    Stroke = new SolidColorBrush(bleTransmitter.Value.ColorCode.ToMediaColor()),
                    StrokeThickness = 4,
                    StrokeDashArray = new DoubleCollection(new Double[] { 2, (transmitters.Count - 1) * 2 }),
                    StrokeDashOffset = offset,
                    Width = 50,
                    Height = 50,
                    Style = (Style)this.Resources["AnimateReceiverStyle"]
                };
                offset += 2;
                receiverDetections.Add(bleTransmitter.Key, ellipse);
                MapCanvas.Children.Add(ellipse);
                SetElementPositionByCenter(ellipse, receiver.Position);
            }
        }

        private void AddReceiver(BleReceiver receiver)
        {
            var ellipse = new Ellipse
            {
                Fill = Brushes.DarkRed,
                Stroke = Brushes.DimGray,
                Width = 10,
                Height = 10

            };
            MapCanvas.Children.Add(ellipse);
            SetElementPositionByCenter(ellipse, receiver.Position);
        }

        private void SetOrigin()
        {
            SetElementPositionByCenter(OriginEllipse, new PointF(0, 0));
            //SetElementPositionByCenter(OriginPolygon, new PointF(0, 0));


        }

        private void SetElementPositionByCenter(FrameworkElement element, PointF metricPosition)
        {
            metricPosition.X += _mapOrigin.X;
            metricPosition.Y += _mapOrigin.Y;
            var localPosition = MetricToLocal(metricPosition);
            localPosition.X -= (float)element.Width / 2f;
            localPosition.Y -= (float)element.Height / 2f;

            Canvas.SetLeft(element, localPosition.X);
            Canvas.SetTop(element, localPosition.Y);
        }
        private PointF MetricToLocal(PointF metricPoint)
        {
            return new PointF(metricPoint.X * _conversionRatio.Width, metricPoint.Y * _conversionRatio.Height);
        }
        private SizeF MetricToLocal(SizeF metricSize)
        {
            return new SizeF(metricSize.Width * _conversionRatio.Width, metricSize.Height * _conversionRatio.Height);
        }
    }
}
