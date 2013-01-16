using System;
using System.IO;
using AmadeusW.Ambilight.DataClasses;
using System.IO.Ports;

namespace AmadeusW.Ambilight.DeviceDriver
{
    class TeensyDriver : Driver
    {
        private SerialPort teensy;
        private bool active = false;

        #region Overrides of Driver

        public override void Initialize()
        {
            teensy = null;

            // Try to connect to the device
            //string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            string[] ports = {"COM3"};
           
            foreach (String portName in ports)
            {
                SerialPort sp = new SerialPort(portName, 9600);
                try
                {
                    sp.Open();
                    sp.ReadTimeout = 1000;
                    string newline = sp.NewLine;
                    for (int i = 0; i < 10; i++ )
                    {
                        String message = sp.ReadLine();
                        if (message == "Hi")
                        {
                            teensy = sp;
                            //sp.DtrEnable = true;
                            active = true;
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    // This device might be in use. Swallow.
                    sp.Close();
                }
                finally
                {
                    
                }
            }

            if (teensy == null)
            {
                throw new IOException("Could not connect to teensy board. Connectivity with Teensy module works only on my computer until I make the Teensy HID compliant.");
            }

        }

        public override void Dispose()
        {
            active = false;
            teensy.Close();
            teensy = null;
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
