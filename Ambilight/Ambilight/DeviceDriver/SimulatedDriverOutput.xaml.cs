using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AmadeusW.Ambilight.DataClasses;
using AmadeusW.Ambilight.Helpers;
using AmadeusW.Ambilight.ScreenshotLogic.PlatformSpecificHelpers;

namespace AmadeusW.Ambilight.DeviceDriver
{
    /// <summary>
    /// Interaction logic for SimulatedDriverOutput.xaml
    /// </summary>
    public partial class SimulatedDriverOutput : Window
    {
        private List<Rectangle> rectangles;
        private int screenWidth;
        private int screenHeight;
        private Preset preset;

        public SimulatedDriverOutput()
        {
            InitializeComponent();
            getScreenInfo();
        }

        private void getScreenInfo()
        {
            screenWidth = PlatformInvokeUSER32.GetSystemMetrics(PlatformInvokeUSER32.SM_CXSCREEN);
            screenHeight = PlatformInvokeUSER32.GetSystemMetrics(PlatformInvokeUSER32.SM_CYSCREEN);
        }

        public void ReceiveData(byte[] data)
        {
            if (this.Dispatcher.CheckAccess())
            {
                // Ignore the header
                int offset = Config.headerLength;
                int counter = 0;

                foreach (Rectangle r in rectangles)
                {
                    r.Fill = new SolidColorBrush(Color.FromRgb(data[3 * counter + 0 + offset], data[3 * counter + 1 + offset], data[3 * counter + 2 + offset]));
                    counter++;
                }
                
                return;
            }
            else
            {
                this.Dispatcher.Invoke(new Action(() => ReceiveData(data)));
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        private void setRectangleSizes()
        {
            double canvasWidth = this.Width - 20;
            double canvasHeight = this.Height - 20;
            double ratio = Math.Min(canvasWidth / screenWidth, canvasHeight / screenHeight);

            SimulatorCanvas.Children.Clear();

            Rectangle frame = new Rectangle();
            frame.Width = screenWidth * ratio;
            frame.Height = screenHeight * ratio;
            frame.Fill = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            SimulatorCanvas.Children.Add(frame);
            Canvas.SetTop(frame, 0);
            Canvas.SetLeft(frame, 0);

            for (int i = 0; i < Config.numberOfLeds; i++)
            {
                Rectangle r = rectangles[i];
                ScreenshotLogicCPP.Sector sector = preset.Sectors[i];

                r.Width = (sector.Right - sector.Left) * ratio;
                r.Height = (sector.Bottom - sector.Top) * ratio;
                r.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                Canvas.SetTop(r, sector.Top * ratio);
                Canvas.SetLeft(r, sector.Left * ratio);

                SimulatorCanvas.Children.Add(r);
            }
        }

        public void PrepareCanvas(Preset newPreset)
        {
            if (this.Dispatcher.CheckAccess())
            {
                // Update the preset
                preset = new Preset(newPreset);

                rectangles = new List<Rectangle>(Config.numberOfLeds);

                for (int i = 0; i < Config.numberOfLeds; i++)
                {
                    Rectangle r = new Rectangle();
                    rectangles.Add(r);
                }

                setRectangleSizes();
            }
            else
            {
                this.Dispatcher.Invoke(new Action(() => PrepareCanvas(newPreset)));
            }
        }

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            // Do this only when preset is not null, which also excludes setting size in *ctor
            if (preset != null)
                setRectangleSizes();
        }
    }
}
