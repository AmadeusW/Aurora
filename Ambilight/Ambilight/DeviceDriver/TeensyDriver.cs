using System;
using System.ComponentModel;
using System.IO;
using AmadeusW.Ambilight.DataClasses;
using System.IO.Ports;

namespace AmadeusW.Ambilight.DeviceDriver
{
    class TeensyDriver : Driver
    {
        private SerialPort teensy;
        private bool active = false;
        private EventHandler<AmbilightEventArgs> messageEventHandler;
        private BackgroundWorker connectionWorker = new BackgroundWorker();

        #region Constructor

        public TeensyDriver(EventHandler<AmbilightEventArgs> eventHandler )
        {
            messageEventHandler = eventHandler;
            connectionWorker.DoWork += ConnectionWorkerOnDoWork;
        }

        #endregion

        private void ConnectionWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            teensy = null;

            // Try to connect to the device
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();

            foreach (String portName in ports)
            {
                messageEventHandler(this, new AmbilightEventArgs(AmbilightEventArgs.AmbilightEventActions.UpdateStatus, "Connecting to " + portName + "..."));
                SerialPort sp = new SerialPort(portName, 9600);
                try
                {
                    sp.Open();
                    sp.ReadTimeout = 1000;
                    string newline = sp.NewLine;
                    for (int i = 0; i < 10; i++)
                    {
                        String message = sp.ReadLine();
                        if (message == "Hi")
                        {
                            teensy = sp;
                            //sp.DtrEnable = true;
                            messageEventHandler(this, new AmbilightEventArgs(AmbilightEventArgs.AmbilightEventActions.UpdateStatus, "Connected to " + portName));
                            active = true;
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    // This device might be in use. Try another port.
                    sp.Close();
                }
                finally
                {

                }
            }

            // Throw an exception after going through all serial ports without connecting to Teensy.
            if (teensy == null)
            {
                messageEventHandler(this, new AmbilightEventArgs(AmbilightEventArgs.AmbilightEventActions.UpdateStatus, "Unable to connect."));
                throw new IOException("Could not connect to teensy board. \nMake sure that Teensy Serial Driver is installed. \nOn Windows 8 it requires booting into 'advanced startup' that allows loading of unsigned drivers.");
            }
        }

        #region Overrides of Driver

        public override void Initialize()
        {
            connectionWorker.RunWorkerAsync();
        }

        public override void Dispose()
        {
            active = false;
            if (teensy != null)
            {
                try
                {
                    teensy.Close();
                }
                catch (Exception)
                {
                    // Swallow.        
                }
                finally
                {
                    teensy = null;   
                }
            }
        }

        public override void SendData(byte[] data)
        {
            if (active)
            {
                teensy.Write(data, 0, data.Length);
            }
        }

        public override void SetPreset(Preset preset)
        {
            if (preset == null)
            {
                throw new ArgumentNullException();
            }
        }

        #endregion
    }
}
