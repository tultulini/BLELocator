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
    public partial class MapWindow : Window
    {
        //private SizeF _mapSizeMetric;
        private SizeF _conversionRatio;
        private PointF _mapOrigin;
        private Dictionary<string, Dictionary<string, Ellipse>> _receiverDetections;


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
                AddReceiver(bleReceiver.Value);
                AddReceiverTransmitters(bleReceiver.Value, configuration.BleTransmitters);
            }
        }

        private void AddReceiverTransmitters(BleReceiver receiver, Dictionary<string, BleTransmitter> transmitters)
        {
            var receiverDetections = new Dictionary<string, Ellipse>();
            _receiverDetections.Add(receiver.IPAddress.ToString(),receiverDetections);
            var offset = 0;
            foreach (var bleTransmitter in transmitters)
            {
                    
                var ellipse = new Ellipse
                {
                    Stroke = new SolidColorBrush(bleTransmitter.Value.ColorCode.ToMediaColor()),
                    StrokeThickness = 4,
                    StrokeDashArray = new DoubleCollection(new Double[]{2,(transmitters.Count-1)*2}),
                    StrokeDashOffset = offset,
                    Width = 50,
                    Height = 50,
                    Style = (Style)this.Resources["AnimateTriggerStyle"]
                };
                offset += 2;
                receiverDetections.Add(bleTransmitter.Key,ellipse);
                MapCanvas.Children.Add(ellipse);
                SetElementPositionByCenter(ellipse,receiver.Position);
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
            SetElementPositionByCenter(ellipse,receiver.Position);
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
    }
}
