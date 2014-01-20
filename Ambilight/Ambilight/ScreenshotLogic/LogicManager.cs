using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AmadeusW.Ambilight.DataClasses;
using AmadeusW.Ambilight.DeviceDriver;
using System.Diagnostics;

namespace AmadeusW.Ambilight.ScreenshotLogic
{
    public enum DisplayStates
    {
        Win,
        Const,
        DX9,
        DX11,
        OpenGL,
        Null
    };

    public class LogicManager
    {
        #region Fields and properties

        private readonly Timer _scheduler;
        private bool _canStart = false;
        private bool _paused = true;
        private bool _needToApplyPreset = false;
        private Preset _queuedPreset = null;
        private int _ledsLeft = 0;
        private DisplayStates _state;

        // Work objects
        private ScreenshotLogicCPP.Shooter _shooter;
        private readonly DeviceRelay _relay;

        // Postprocessing
        private byte valueAmbient = 15;
        private byte valueLower = 85;
        private byte valueUpper = 171;
        private byte valueFlash = 240;
        private float parameterALower = 1;
        private int parameterBLower = 0;
        private float parameterAMiddle = 1;
        private int parameterBMiddle = 0;
        private float parameterAUpper = 1;
        private int parameterBUpper = 0;

        // This goes from shooter to relay
        private byte[] outputColors;

        // Performance measurement thingy
        private bool manualMode;

        // Show colors as requested by GUI
        private bool m_OverridenOutput;
        private byte[] m_OverridenColors;

        #endregion

        #region Events

        public event EventHandler<AmbilightEventArgs> NotifyGUI;

        #endregion

        #region Constructor

        public LogicManager()
        {
            manualMode = false;
            outputColors = new byte[Config.numberOfLeds * 3];

            _scheduler = new Timer { Enabled = false };
            _scheduler.Elapsed += TimerElapsed;

            _state = DisplayStates.Null;
            assignShooter();

            _relay = new DeviceRelay();
            _relay.NotifyLogicAboutError += relayErrorEvent;
            _relay.NotifyLogicAboutMessage += relayMessageEvent;
        }

        #endregion

        #region Communication with GUI

        public void ApplyPreset(Preset preset, AvailableDrivers driver)
        {
            Debug.WriteLine("ApplyPreset " + preset + ", " + driver.ToString());

            // Save us from working if it is not needed now
            _paused = (driver == AvailableDrivers.Off);

            _queuedPreset = new Preset(preset);
            _needToApplyPreset = true;

            _relay.Connect(driver);

            // If this is the first time applying the preset, it is safe to execute this
            if (!_canStart || manualMode)
            {
                SafeApplyQueuedPreset();
            }
        }

        public void SetManualMode(bool enableManualMode)
        {
            if (enableManualMode)
            {
                // Disable the timer
                _scheduler.Enabled = false;
                // Force going to SafeApplyQueuedPreset
                _canStart = false;
                // Pretend we're clean
                _ledsLeft = 0;
            }
            else
            {
                // Disable the timer
                _scheduler.Enabled = false;
                // We can't start right away. Do some prep.
                _canStart = false;
                // Pretend we're clean
                _ledsLeft = 0;
            }
        }

        public void ManuallyDoWork()
        {
            TimerElapsed(null, null);
        }

        #endregion

        #region Event Handlers

        // TODO: have some event that will be raised by the relay in case of errors
        // or in any case, to forward messages to the GUI
        private void relayErrorEvent(object sender, EventArgs e)
        {
            _paused = true;
            // Update GUI message
            relayMessageEvent(this, new AmbilightEventArgs(AmbilightEventArgs.AmbilightEventActions.UpdateStatus, "Connection error"));
        }

        private void relayMessageEvent(object sender, AmbilightEventArgs e)
        {
            // Update GUI message
            NotifyGUI(sender, e);
        }

        #endregion

        #region Working

        private void assignShooter()
        {
            if (_shooter != null)
            {
                _shooter.Destroy();
                _shooter = null;
            }

            switch (_state)
            {
                case DisplayStates.Win:
                    _shooter = new ScreenshotLogicCPP.ShooterGDI();
                    break;
                case DisplayStates.Const:
                    _shooter = new ScreenshotLogicCPP.ShooterConst();
                    break;
                default:
                    _shooter = new ScreenshotLogicCPP.ShooterNull();
                    break;
            }

            _shooter.Initialize();
            // Do not set preset yet. No preset has been selected by the user
        }

        private void SafeApplyQueuedPreset()
        {
            Debug.WriteLine("SafeApplyQueuedPreset");
            if (_ledsLeft > 0)
            {
                throw new InvalidOperationException("Unable to change settings: something is still processing an LED.");
            }

            _scheduler.Enabled = false;
            _canStart = true;

            // TODO: this is an ugly hack. Create a way to convey more information to the shooter
            if (_shooter.GetType() == typeof(ScreenshotLogicCPP.ShooterConst))
            {
                _queuedPreset.AveragingParam = 0;
                _queuedPreset.AveragingParam += (byte)_queuedPreset.MaxRed;
                _queuedPreset.AveragingParam = _queuedPreset.AveragingParam << 8;
                _queuedPreset.AveragingParam += (byte)_queuedPreset.MaxGreen;
                _queuedPreset.AveragingParam = _queuedPreset.AveragingParam << 8;
                _queuedPreset.AveragingParam += (byte)_queuedPreset.MaxBlue;
                _queuedPreset.AveragingParam = _queuedPreset.AveragingParam << 8;
            }

            _shooter.SetPreset(_queuedPreset.Sectors, _queuedPreset.AveragingParam, _queuedPreset.MinColor.Brightness); // TODO: maybe just call assignShooter next time.
            _relay.SetPreset(_queuedPreset);
            _needToApplyPreset = false;

            valueAmbient = (byte)(_queuedPreset.MinColor.Brightness);
            valueFlash = (byte)(_queuedPreset.MaxColor.Brightness);
            parameterALower = 0.3f;
            parameterAUpper = 0.3f;
            int lowerMiddleBoundary = valueAmbient + (int)((valueLower - valueAmbient)*parameterALower);
            int upperMiddleBoundary = valueFlash - (int)((valueFlash - valueUpper)*parameterAUpper);
            parameterBLower = lowerMiddleBoundary - (int) (parameterALower * valueLower);
            parameterBUpper = upperMiddleBoundary - (int)(parameterAUpper * valueUpper);

            parameterAMiddle = (int)((upperMiddleBoundary - lowerMiddleBoundary)/(valueUpper - valueLower));
            parameterBMiddle = lowerMiddleBoundary - (int)(parameterAMiddle * valueLower);

            // ReSharper disable PossibleLossOfFraction
            _scheduler.Interval = 1000 / _queuedPreset.Framerate;
            // ReSharper restore PossibleLossOfFraction

            if (!manualMode)
                _scheduler.Enabled = true;
        }

        private void TimerElapsed(object o, ElapsedEventArgs e)
        {
            //Debug.WriteLine("Ping");
            // Abort if we are still working on some LEDs, or we don't need to work at all
            if (_ledsLeft > 0 || _paused)
            {
                return;
            }

            // Do some housekeeping
            if (_needToApplyPreset)
            {
                Debug.WriteLine("Housekeeping!");
                SafeApplyQueuedPreset();
            }

            // Run through all sectors
            _ledsLeft = 1;

            if (m_OverridenOutput)
            {
                _relay.RelayData(m_OverridenColors);
            }
            else
            {
                _shooter.DoWork(outputColors);

                // Do post processing
                postProcessing(outputColors);
                // Figure out colors for all LEDs
            
                // Send out data to LEDs
                _relay.RelayData(outputColors);
            }

            _ledsLeft = 0;
        }

        // Post processing that clips boundary colors and increases contrast
        private void postProcessing(byte[] outputColors)
        {
            const int singleColorBoost = 30;
            const int twoColorBoost = 20;
            const int dominanceDelta = 20;

            for (int i = 0; i < outputColors.Length; i++)
            {
                if (false)
                {
                    // Increase brightness of two brightest colors from a trio
                    if (i%3 == 0)
                    {
                        int deltaRG = outputColors[i] - outputColors[i + 1];
                        int deltaRB = outputColors[i] - outputColors[i + 2];
                        int deltaGB = outputColors[i + 1] - outputColors[i + 2];

                        bool redDarkerThanGreen = (deltaRG < -dominanceDelta);
                        bool redDarkerThanBlue = (deltaRB < -dominanceDelta);
                        bool redBrighterThanGreen = (deltaRG > dominanceDelta);
                        bool redBrighterThanBlue = (deltaRB > dominanceDelta);
                        bool greenDarkerThanBlue = (deltaGB < -dominanceDelta);
                        bool greenBrighterThanBlue = (deltaGB > dominanceDelta);

                        // Dominance of red
                        if (redBrighterThanBlue && redBrighterThanGreen)
                        {
                            outputColors[i] = Math.Min((byte) 255,
                                                       (byte)
                                                       (outputColors[i] + (deltaRG + deltaRB - 2*dominanceDelta)/1));
                        }
                            // Dominance of green
                        else if (redDarkerThanGreen && greenBrighterThanBlue)
                        {
                            outputColors[i + 1] = Math.Min((byte)255,
                                                           (byte)
                                                           (outputColors[i + 2] +
                                                            (-deltaRG + deltaGB - 2*dominanceDelta)/1));
                        }
                            // Dominance of blue
                        else if (redDarkerThanBlue && greenDarkerThanBlue)
                        {
                            outputColors[i + 2] = Math.Min((byte)255,
                                                           (byte)
                                                           (outputColors[i + 2] +
                                                            (-deltaRB - deltaGB - 2*dominanceDelta)/1));
                        }

                            // Dominance of yellow
                        else if (redBrighterThanBlue && greenDarkerThanBlue)
                        {
                            outputColors[i] = Math.Min((byte)255,
                                                       (byte) (outputColors[i] + (deltaRB - dominanceDelta)/1));
                            outputColors[i + 1] = Math.Min((byte)255,
                                                           (byte) (outputColors[i + 1] + (deltaGB - dominanceDelta)/1));
                        }
                            // Dominance of cyan
                        else if (redDarkerThanGreen && redDarkerThanBlue)
                        {
                            outputColors[i + 1] = Math.Min((byte)255,
                                                           (byte) (outputColors[i + 1] + (-deltaRG - dominanceDelta)/1));
                            outputColors[i + 2] = Math.Min((byte)255,
                                                           (byte) (outputColors[i + 2] + (-deltaRB - dominanceDelta)/1));
                        }
                            // Dominance of magenta
                        else if (redBrighterThanGreen && greenDarkerThanBlue)
                        {
                            outputColors[i] = Math.Min((byte)255,
                                                       (byte) (outputColors[i] + (deltaRG - dominanceDelta)/1));
                            outputColors[i + 2] = Math.Min((byte)255,
                                                           (byte) (outputColors[i + 2] + (-deltaGB - dominanceDelta)/1));
                        }
                    }
                }

                if (outputColors[i] < valueAmbient)
                {
                    outputColors[i] = valueAmbient; // TODO: temporarily allow to go below ambient color
                }
                else if (outputColors[i] < valueLower)
                {
                    outputColors[i] = (byte)(outputColors[i]*parameterALower + parameterBLower);
                }
                else if (outputColors[i] < valueUpper)
                {
                    outputColors[i] = (byte)(outputColors[i] * parameterAMiddle + parameterBMiddle);
                }
                else if (outputColors[i] < valueFlash)
                {
                    outputColors[i] = (byte)(outputColors[i] * parameterAUpper + parameterBUpper);
                }
                else // value >= valueFlash
                {
                    outputColors[i] = valueFlash; // TODO: temporarily allow to go above flash color
                }

                // Adjust for LED color and wall color
                if (i % 3 == 0)
                {
                    outputColors[i] = (byte)(outputColors[i] * _queuedPreset.MaxRed / 100.0d);
                }
                else if (i % 3 == 1)
                {
                    outputColors[i] = (byte)(outputColors[i] * _queuedPreset.MaxGreen / 100.0d);
                }
                else if (i % 3 == 2)
                {
                    outputColors[i] = (byte)(outputColors[i] * _queuedPreset.MaxBlue / 100.0d);
                }
            }
        }

        #endregion

        public void OverrideNormalOperation(int p0, int p1, int p2)
        {
            m_OverridenColors = new byte[Config.numberOfSectors * Config.colorsPerLed];
            for (int i = 0; i < m_OverridenColors.Length; i+=3)
            {
                m_OverridenColors[i] = (byte) Math.Min(255, p0);
                m_OverridenColors[i + 1] = (byte)Math.Min(255, p1);
                m_OverridenColors[i + 2] = (byte)Math.Min(255, p2);
            }
            m_OverridenOutput = true;
        }

        public void RestoreNormalOperation()
        {
            m_OverridenOutput = false;
        }

        internal void TryUpdateShooter(bool useGDIShooter)
        {
            DisplayStates newState = useGDIShooter ? DisplayStates.Win : DisplayStates.Const;
            if (newState != _state)
            {
                _state = newState;
                assignShooter();
            }
        }
    }
}
