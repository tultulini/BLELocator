using System;
using System.CodeDom;
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
            _mapOrigin = new PointF(20,20);
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
            AddRoomContour();
        }

        private void AddRoomContour()
        {
            var passageMetricSize = new SizeF(45, 4);
            var passageSize = MetricToLocal(passageMetricSize);
            PassageRect.Width = passageSize.Width;
            PassageRect.Height = passageSize.Height;
            SetElementPosition(PassageRect, new PointF(0, -2));

            var passageMetricSizeVert = new SizeF(4, 19);
            var passageSizeVert = MetricToLocal(passageMetricSizeVert);
            PassageRect2.Width = passageSizeVert.Width;
            PassageRect2.Height = passageSizeVert.Height;
            SetElementPosition(PassageRect2, new PointF(0, 2));


            var elevatorMetricSize = new SizeF(4, 2);
            var elevatorSize = MetricToLocal(elevatorMetricSize);
            ElevatorRect.Width = elevatorSize.Width;
            ElevatorRect.Height = elevatorSize.Height;
            SetElementPosition(ElevatorRect, new PointF(5, -2 - elevatorMetricSize.Height));

            var controlRoomMetricSize = new SizeF(8, 10);
            var controlRoomSize = MetricToLocal(controlRoomMetricSize);
            ControlRoomRect.Width = controlRoomSize.Width;
            ControlRoomRect.Height = controlRoomSize.Height;
            SetElementPosition(ControlRoomRect, new PointF(0 - controlRoomMetricSize.Width, -2));
            SetElementPosition(ExitText90Deg, new PointF(passageMetricSize.Width, 0));
            SetElementPosition(ExitTextHor, new PointF(0, passageMetricSizeVert.Height+2));

        }

        private void AddTransmitter( BleTransmitter bleTransmitter)
        {
            var grid = new Grid
            {
                RenderTransform = new ScaleTransform(0.1, 0.1),
                Style = (Style)this.Resources["AnimateTransmitterStyle"],
                Visibility = Visibility.Hidden,
                ToolTip = bleTransmitter.TransmitterName
            
            };

            var manPath1 = new Path
            {
                Data =
                    Geometry.Parse(
                        "m256.062225,124.192871c14.424774,0 26.085876,-11.200264 26.085876,-25.129318c0,-13.842712 -11.661102,-25.103043 -26.085876,-25.103043c-14.335449,0 -25.996521,11.26033 -25.996521,25.103043c0.003906,13.929054 11.665009,25.129318 25.996521,25.129318z"),
                Fill =new SolidColorBrush(bleTransmitter.ColorCode.ToMediaColor())
            };
            var manPath2 = new Path
            {
                Data =
                    Geometry.Parse(
                        "m220.869019,367.93457c0,7.79895 6.436981,14.104919 14.514008,14.104919c8.04599,0 14.608002,-6.305969 14.608002,-14.104919l0,-118.204041l12.235992,0l0,118.204041c0,7.79895 6.561005,14.104919 14.608002,14.104919c8.076996,0 14.544983,-6.305969 14.544983,-14.104919l0.08902,-203.801056l12.147003,0l0,75.128998c0,15.130005 20.375977,15.130005 20.375977,0l0,-76.77002c0,-16.68396 -13.453003,-32.988983 -33.677002,-32.988983l-68.994995,-0.085999c-18.494995,0 -33.312012,14.545013 -33.312012,32.610016l0,77.237976c0,14.95401 20.496002,14.95401 20.496002,0l0,-75.131989l12.361023,0l0,203.801056l0.003998,0z"),
                Fill = new SolidColorBrush(bleTransmitter.ColorCode.ToMediaColor())
            };
            
            grid.Children.Add(manPath1);
            grid.Children.Add(manPath2);
            _transmitterPositions.Add(bleTransmitter.MacAddress, grid);
            MapCanvas.Children.Add(grid);
            //var humanAverageSize = MetricToLocal(new SizeF(0.5f, 1.8f));
            //var shape = new RectangleShape
            //{
            //    Width = humanAverageSize.Width,
            //    Height = humanAverageSize.Height,
            //    Stroke = new SolidColorBrush(bleTransmitter.ColorCode.ToMediaColor()),
            //    StrokeThickness = 4,
            //    Fill = Brushes.DimGray,
            //    Style = (Style)this.Resources["AnimateTransmitterStyle"],
            //    Visibility = Visibility.Hidden
                
            //};
            //_transmitterPositions.Add(bleTransmitter.TransmitterName,shape);
            //MapCanvas.Children.Add(shape);
        }

        public void HandleTransmitterLocationEvent(BleTransmitter transmitter)
        {
            WPFMethodInvoker.InvokeAsync(() =>
            {
                FrameworkElement transmitterElement;
                if (!_transmitterPositions.TryGetValue(transmitter.MacAddress, out transmitterElement))
                {
                    return;
                }
                transmitterElement.Opacity = 1;
                transmitterElement.Visibility = Visibility.Hidden;
                var position = new PointF(transmitter.Position.X + transmitter.VisualOffset, transmitter.Position.Y + transmitter.VisualOffset - 1);
                SetElementPosition(transmitterElement, position);
                transmitterElement.Visibility = Visibility.Visible;
            });
        }

        public void HandleDiscoveryEvent(BleReceiver receiver, string transmitterMacAddress, float distance)
        {
           WPFMethodInvoker.InvokeAsync(() =>
           {
               Dictionary<string, Ellipse> receiverDetections;
               if (!_receiverDetections.TryGetValue(receiver.IPAddress.ToString(), out receiverDetections))
                   return;
               Ellipse ellipse;
               if (!receiverDetections.TryGetValue(transmitterMacAddress, out ellipse))
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
                    Style = (Style)this.Resources["AnimateReceiverStyle"],
                    
                };
                offset += 2;
                receiverDetections.Add(bleTransmitter.Key, ellipse);
                MapCanvas.Children.Add(ellipse);
                SetElementPositionByCenter(ellipse, receiver.Position);
            }
        }

        private string FormatReceiverTag(BleReceiver receiver)
        {
            return string.Format("{0} - ({1},{2})", receiver.IPAddress, receiver.Position.X, receiver.Position.Y);
        }

        private void AddReceiver(BleReceiver receiver)
        {
            var ellipse = new Ellipse
            {
                Fill = Brushes.DarkRed,
                Stroke = Brushes.DimGray,
                Width = 10,
                Height = 10,
                    ToolTip = FormatReceiverTag(receiver),

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
        private void SetElementPosition(FrameworkElement element, PointF metricPosition)
        {
            metricPosition.X += _mapOrigin.X;
            metricPosition.Y += _mapOrigin.Y;
            var localPosition = MetricToLocal(metricPosition);

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
