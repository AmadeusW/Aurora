using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AmadeusW.Ambilight.DataClasses;
using AmadeusW.Ambilight.DeviceDriver;
using AmadeusW.Ambilight.Helpers;
using AmadeusW.Ambilight.ScreenshotLogic;
using AmadeusW.Ambilight.ScreenshotLogic.PlatformSpecificHelpers;
using AmadeusW.Ambilight.Commands;

namespace AmadeusW.Ambilight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private PresetModel _presets;
        private LogicManager _logic;

        // For sector configuration
        private int screenWidth;
        private int screenHeight;

        #endregion

        public PresetActionCommand PresetActionCommand { get; set; }

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            PresetActionCommand = new PresetActionCommand(this);
            DataContext = this; // TODO: remove this and use MVVM patterns.
        }

        #endregion

        #region Event Handlers

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            _logic = new LogicManager();
            _logic.NotifyGUI += logicToGUIEventHandler;
            getScreenInfo();

            initializePresets();

            // Select first item
            if (presetList.Items[0] != null)
                presetList.SelectedItem = presetList.Items[0];
        }

        private void initializePresets()
        {
            // TODO: this should go into appropriate function
            _presets = this.Resources["model"] as PresetModel;
            if (_presets == null)
            {
                throw new NullReferenceException("Could not connect to the preset model");
            }

            var loadedPresets = Preset.LoadPresets();
            foreach (var loadedPreset in loadedPresets)
            {
                _presets.Presets.Add(loadedPreset);
            }

            // IF nothing was loaded, show defaults
            if (_presets.Presets.Count == 0)
            {
                Preset p1 = new Preset();
                Preset p2 = new Preset();
                // Make p1 a high performance preset
                p1.Name = "Low footprint";
                p1.Framerate = 4;
                p1.AveragingParam = 3;
                // P2 should have appropriate names
                p2.Name = "High framerate";
                p1.PropertyChanged += PresetPropertyChanged;
                p2.PropertyChanged += PresetPropertyChanged;
                _presets.Presets.Add(p1); // For now 
                _presets.Presets.Add(p2); // For now 
                p1.Save();
                p2.Save();
            }
        }

        private void logicToGUIEventHandler(object sender, AmbilightEventArgs e)
        {
            // Make sure we are in GUI thread
            if (e.Action == AmbilightEventArgs.AmbilightEventActions.UpdateStatus)
            {
                Dispatcher.BeginInvoke(new Action(() => updateConnectionLabel(e.Details)));
            }
        }

        private void PresetPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "TopSectors" || propertyChangedEventArgs.PropertyName == "BottomSectors" || propertyChangedEventArgs.PropertyName == "VerticalSectors")
            {
                updateSectors(sender as Preset);
                updateSectorGui(sender as Preset);
            }
        }

        private void Button_Click_SetDevice(object sender, RoutedEventArgs e)
        {
            Button senderB = sender as Button;

            Preset selectedPreset = presetList.SelectedItem as Preset;
            if (selectedPreset == null)
            {
                throw new InvalidOperationException("Unable to apply preset: Selected preset is invalid");
            }
            // Make sure that the list of sectors has been created and is up to date
            updateSectors(selectedPreset);

            // See if we need to change the source
            _logic.TryUpdateShooter(SourceGDI.IsChecked.Value);

            // Begin the program logic
            try
            {
                switch (senderB.Tag.ToString())
                {
                    case "Teensy":
                        _logic.ApplyPreset(selectedPreset, AvailableDrivers.Teensy);
                        updateConnectionLabel("Connecting to the device");
                        break;
                    case "Simulator":
                        _logic.ApplyPreset(selectedPreset, AvailableDrivers.Simulator);
                        updateConnectionLabel("Connected to the simulator");
                        break;
                    default:
                        _logic.ApplyPreset(selectedPreset, AvailableDrivers.Off);
                        updateConnectionLabel("Not connected");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection error");
            }
        }

        private void TabItem_RequestBringIntoView_1(object sender, RequestBringIntoViewEventArgs e)
        {
            Preset selectedPreset = presetList.SelectedItem as Preset;
            if (selectedPreset != null)
            {
                updateSectors(selectedPreset);
                updateSectorGui(selectedPreset);
            }
        }

        private void presetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Preset selectedPreset = presetList.SelectedItem as Preset;
            if (selectedPreset != null)
            {
                updateSectors(selectedPreset);
                updateSectorGui(selectedPreset);
            }
        }

        private void PerformanceButtonClick(object sender, RoutedEventArgs e)
        {
            const int testIterations = 50;

            LogicOffButton.IsEnabled = false;
            LogicOnButton.IsEnabled = false;
            LogicSimButton.IsEnabled = false;
            
            Preset selectedPreset = presetList.SelectedItem as Preset;
            if (selectedPreset == null)
            {
                throw new InvalidOperationException("Unable to apply preset: Selected preset is invalid");
            }

            _logic.SetManualMode(true);
            _logic.ApplyPreset(selectedPreset, AvailableDrivers.Null);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < testIterations; i++ )
            {
                _logic.ManuallyDoWork();
            }

            sw.Stop();

            _logic.SetManualMode(false);
            LogicOffButton.IsEnabled = true;
            LogicOnButton.IsEnabled = true;
            LogicSimButton.IsEnabled = true;

            long totalTime = sw.ElapsedMilliseconds + 1;
            decimal avgTime = totalTime / testIterations;
            int maxFps = (int)((1000 * testIterations) / totalTime);

            PerformanceResult.Content = String.Format("{0} ms, avg {1} ms, max {2} fps", totalTime, avgTime, maxFps);
        }

        #endregion

        private void updateConnectionLabel(string newText)
        {
            ConnectionLabel.Text = newText;
        }

        private void getScreenInfo()
        {
            screenWidth = PlatformInvokeUSER32.GetSystemMetrics(PlatformInvokeUSER32.SM_CXSCREEN);
            screenHeight = PlatformInvokeUSER32.GetSystemMetrics(PlatformInvokeUSER32.SM_CYSCREEN);
        }

        // TODO: this should NOT be in GUI! Perhaps Preset would be a better place.
        public void updateSectors(Preset selectedPreset)
        {
            int i;
            int currentSector = 0;

            int verticalSectorHeight = screenHeight / selectedPreset.VerticalSectors;
            int topSectorWidth = (screenWidth - 2 * selectedPreset.VerticalSectorWidth) / selectedPreset.TopSectors;
            int bottomSectorWidth = ((int)(0.8 * screenWidth) - 2 * selectedPreset.VerticalSectorWidth) / selectedPreset.BottomSectors;
            int bottomSectorGap = (int) (0.1*screenWidth);

            if (2*selectedPreset.VerticalSectors + selectedPreset.TopSectors + selectedPreset.BottomSectors != Config.numberOfLeds)
            {
                return;
            }

            // Bottom edge, right side
            for (i = selectedPreset.BottomSectors / 2; i < selectedPreset.BottomSectors; i++)
            {
                selectedPreset.Sectors[currentSector] = new ScreenshotLogicCPP.Sector(2 * bottomSectorGap + selectedPreset.VerticalSectorWidth + i * bottomSectorWidth, screenHeight - selectedPreset.HorizontalSectorHeight, bottomSectorWidth, selectedPreset.HorizontalSectorHeight, currentSector);
                currentSector++;
            }

            // Right edge
            for (i = 0; i < selectedPreset.VerticalSectors; i++)
            {
                selectedPreset.Sectors[currentSector] = new ScreenshotLogicCPP.Sector(screenWidth - selectedPreset.VerticalSectorWidth, screenHeight - (i + 1) * verticalSectorHeight, selectedPreset.VerticalSectorWidth, verticalSectorHeight, currentSector);
                currentSector++;
            }

            // Top edge
            for (i = 0; i < selectedPreset.TopSectors; i++)
            {
                selectedPreset.Sectors[currentSector] = new ScreenshotLogicCPP.Sector(screenWidth - selectedPreset.VerticalSectorWidth - (i + 1) * topSectorWidth, 0, topSectorWidth, selectedPreset.HorizontalSectorHeight, currentSector);
                currentSector++;
            }

            // Left edge
            for (i = 0; i < selectedPreset.VerticalSectors; i++)
            {
                selectedPreset.Sectors[currentSector] = new ScreenshotLogicCPP.Sector(0, i * verticalSectorHeight, selectedPreset.VerticalSectorWidth, verticalSectorHeight, currentSector);
                currentSector++;
            }

            // Bottom edge, left side
            for (i = 0; i < selectedPreset.BottomSectors / 2; i++)
            {
                selectedPreset.Sectors[currentSector] = new ScreenshotLogicCPP.Sector(selectedPreset.VerticalSectorWidth + i * bottomSectorWidth, screenHeight - selectedPreset.HorizontalSectorHeight, bottomSectorWidth, selectedPreset.HorizontalSectorHeight, currentSector);
                currentSector++;
            }
        }

        public void updateSectorGui(Preset selectedPreset)
        {
            const double canvasWidth = 450;
            const double canvasHeight = 170;
            double ratio = Math.Min(canvasWidth / screenWidth, canvasHeight / screenHeight);

            if (ratio == 0)
                return;

            SectorPreview.Children.Clear();

            Rectangle frame = new Rectangle();
            frame.Width = screenWidth * ratio;
            frame.Height = screenHeight * ratio;
            if (2 * selectedPreset.VerticalSectors + selectedPreset.TopSectors + selectedPreset.BottomSectors != Config.numberOfLeds)
            {
                frame.Fill = new SolidColorBrush(Color.FromRgb(200, 50, 50));   
            }
            else
            {
                frame.Fill = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            }
            SectorPreview.Children.Add(frame);
            Canvas.SetTop(frame, 0);
            Canvas.SetLeft(frame, 0);

            for (int i = 0; i < Config.numberOfLeds; i++)
            {
                Rectangle r = new Rectangle();
                r.Width = (selectedPreset.Sectors[i].Right - selectedPreset.Sectors[i].Left) * ratio;
                r.Height = (selectedPreset.Sectors[i].Bottom - selectedPreset.Sectors[i].Top) * ratio;
                r.Fill = new SolidColorBrush(Color.FromRgb(20, (byte)(i * 10), 50));
                SectorPreview.Children.Add(r);
                Canvas.SetTop(r, selectedPreset.Sectors[i].Top * ratio);
                Canvas.SetLeft(r, selectedPreset.Sectors[i].Left * ratio);
            }
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            Preset selectedPreset = presetList.SelectedItem as Preset;
            if (selectedPreset == null)
                return;

            SuffixConverter sc = new SuffixConverter();
            switch ((sender as Control).Name)
            {
                case "MaxRed":
                    //selectedPreset.MaxRed = (int)(sc.ConvertBack((sender as TextBox).Text, typeof (int), "%", null));
                    _logic.OverrideNormalOperation((int)(255.0 * selectedPreset.MaxRed / 100.0), (int)(255.0 * selectedPreset.MaxGreen / 100.0), (int)(255.0 * selectedPreset.MaxBlue / 100.0));
                    break;
                case "MaxGreen":
                    //selectedPreset.MaxGreen = (int)(sc.ConvertBack((sender as TextBox).Text, typeof(int), "%", null));
                    _logic.OverrideNormalOperation((int)(255.0 * selectedPreset.MaxRed / 100.0), (int)(255.0 * selectedPreset.MaxGreen / 100.0), (int)(255.0 * selectedPreset.MaxBlue / 100.0));
                    break;
                case "MaxBlue":
                    //selectedPreset.MaxBlue = (int)(sc.ConvertBack((sender as TextBox).Text, typeof(int), "%", null));
                    _logic.OverrideNormalOperation((int)(255.0 * selectedPreset.MaxRed / 100.0), (int)(255.0 * selectedPreset.MaxGreen / 100.0), (int)(255.0 * selectedPreset.MaxBlue / 100.0));
                    break;
                default:
                    break;
            }
                

        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            _logic.RestoreNormalOperation();
        }

        internal void ExecutePresetAction(string action)
        {
            Preset selectedPreset = presetList.SelectedItem as Preset;
            if (selectedPreset == null)
            {
                throw new InvalidOperationException("Unable to apply preset: Selected preset is invalid");
            }

            if (action == "Save")
            {
                selectedPreset.Save();
            }
            else if (action == "Duplicate")
            {
                Preset newPreset = new Preset(selectedPreset);
                newPreset.Name += " copy";
                newPreset.Save();
                _presets.Presets.Add(newPreset);
            }
            else if (action == "Remove")
            {
                selectedPreset.Remove();
            }
        }
    }
}
