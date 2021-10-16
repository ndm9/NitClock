using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NitClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly double MAX_ZOOM_FACTOR = 100000d;
        private readonly double MIN_ZOOM_FACTOR = 0.1d;
        private readonly Point ZERO_POSITION = new Point(0, 0);
        private int currSec = -1;
        private double mainWinSize;
        private System.Timers.Timer timer = new System.Timers.Timer(10);
        private double zoomFactor = 1.0;

        public MainWindow()
        {
            InitializeComponent();

            DrawMarkings();

            mainWinSize = mainWin.Height;

            SetDynamicMenuItems();

            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;
        }

        public void SetZoomFactor(double value)
        {
            if (value < MIN_ZOOM_FACTOR)
                zoomFactor = MIN_ZOOM_FACTOR;
            else if (value > MAX_ZOOM_FACTOR)
                zoomFactor = MAX_ZOOM_FACTOR;
            else
                zoomFactor = value;

            allScale.ScaleX = zoomFactor;
            allScale.ScaleY = zoomFactor;

            mainWin.Height = mainWinSize * zoomFactor;
            mainWin.Width = mainWinSize * zoomFactor;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs args)
        {
            base.OnPreviewMouseDown(args);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (args.MiddleButton == MouseButtonState.Pressed)
                {
                    SetZoomFactor(1d);
                }
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs args)
        {
            base.OnPreviewMouseWheel(args);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                SetZoomFactor(zoomFactor + ((args.Delta > 0) ? 0.1d : -0.1d));
            }
        }

        private void DecreaseSize_Click(object sender, RoutedEventArgs e)
        {
            SetZoomFactor(zoomFactor - 0.1d);
        }

        private void DrawMarkings()
        {
            Polyline newMark; ;
            int zIndex = Canvas.GetZIndex(hourMark);
            //Hour
            for (int i = 1; i < 60; i++)
            {
                newMark = new Polyline()
                {
                    Stroke = hourMark.Stroke,
                    StrokeEndLineCap = hourMark.StrokeEndLineCap,
                    StrokeStartLineCap = hourMark.StrokeStartLineCap
                };

                if (i % 5 == 0)
                {
                    foreach (Point p in hourMark.Points)
                        newMark.Points.Add(p);
                    newMark.Opacity = hourMark.Opacity;
                    newMark.StrokeThickness = hourMark.StrokeThickness;
                }
                else
                {
                    newMark.Stroke = new SolidColorBrush(Colors.LightSeaGreen);
                    newMark.Points.Add(new Point(150, 20));
                    newMark.Points.Add(new Point(150, 12));
                    newMark.Opacity = hourMark.Opacity - 0.1;
                    newMark.StrokeThickness = hourMark.StrokeThickness - 2;
                }

                RotateTransform rt = new RotateTransform(6.0 * i, 150.0, 150.0);
                newMark.RenderTransform = rt;
                RootGrid.Children.Add(newMark);

                Canvas.SetZIndex(newMark, ++zIndex);

                //foreach (UIElement uiElem in RootGrid.Children)
                //{
                //    try
                //    {
                //        Console.WriteLine(UIElement.nam)
                //    }
                //    catch (Exception)
                //    {
                //        throw;
                //    }
                //}
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Grid_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void IncreaseSize_Click(object sender, RoutedEventArgs e)
        {
            SetZoomFactor(zoomFactor + 0.1d);
        }

        private void RecallSizeNPosition_Click(object sender, RoutedEventArgs e)
        {
            SetZoomFactor(Properties.Settings.Default.ZoomFactor);

            if (Properties.Settings.Default.Position != ZERO_POSITION)
            {
                mainWin.Left = Properties.Settings.Default.Position.X;
                mainWin.Top = Properties.Settings.Default.Position.Y;
            }
        }

        private void RememberPosition_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Position = new Point(Application.Current.MainWindow.Left, Application.Current.MainWindow.Top);
            Properties.Settings.Default.Save();
            SetDynamicMenuItems();
            //Console.WriteLine($"Position: {Application.Current.MainWindow.Left}, {Application.Current.MainWindow.Top}");
        }

        private void RememberSize_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ZoomFactor = zoomFactor;
            Properties.Settings.Default.Save();
            SetDynamicMenuItems();
        }

        private void ResetSize_Click(object sender, RoutedEventArgs e)
        {
            SetZoomFactor(1d);
        }

        private void SetDigiTime()
        {
            string s = DateTime.Now.ToString("HH:mm:ss");
            currSec = DateTime.Now.Second;

            digiTime.Text = s;
        }

        private void SetDynamicMenuItems()
        {
            //Recall Size and Position
            var caption = string.Empty;
            var hasSize = Properties.Settings.Default.ZoomFactor != 0;
            var hasPosition = Properties.Settings.Default.Position != ZERO_POSITION;

            if (hasSize && hasPosition)
                caption = "Recall Size and Position";
            else if (hasSize)
                caption = "Recall Size";
            else if (hasPosition)
                caption = "Recall Position";
            else
                caption = "Nothing To Recall";

            if ((string)mniRecallSnP.Header != caption)
                mniRecallSnP.Header = caption;

            mniRecallSnP.IsEnabled = (hasSize || hasPosition);
        }

        private void ShowDigitalClock_Checked(object sender, RoutedEventArgs e)
        {
            digiTime.Visibility = Visibility.Visible;
        }

        private void ShowDigitalClock_Unchecked(object sender, RoutedEventArgs e)
        {
            digiTime.Visibility = Visibility.Hidden;
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                secondHand.Angle = (double)DateTime.Now.Second * 6.0 + (double)DateTime.Now.Millisecond * 0.006;
                minuteHand.Angle = (double)DateTime.Now.Minute * 6.0 + (double)DateTime.Now.Second * 0.1 + (double)DateTime.Now.Millisecond * 0.0001;
                hourHand.Angle = (double)DateTime.Now.Hour * 30.0 + (double)DateTime.Now.Minute * 0.5 + (double)DateTime.Now.Second / 120.0;
                if (currSec != DateTime.Now.Second)
                    SetDigiTime();
            }));
        }
    }
}