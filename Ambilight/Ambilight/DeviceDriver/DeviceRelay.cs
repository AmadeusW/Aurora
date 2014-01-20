using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using AmadeusW.Ambilight.DataClasses;

namespace AmadeusW.Ambilight.DeviceDriver
{
    public class DeviceRelay
    {
        #region Fields and properties

        private readonly byte[] _buffer;
        private readonly byte[] _header;
        private Driver _dd;

        #endregion

        #region Events

        public event EventHandler NotifyLogicAboutError;
        public event EventHandler<AmbilightEventArgs> NotifyLogicAboutMessage;

        #endregion

        #region Constructor, destructor, configuration

        public DeviceRelay()
        {
            // Each pixel shifts out 24 bits of color information
            // Preceeded by a magic word (2 bytes) + led info (2 bytes) + checksum (1 byte)
            _buffer = new byte[Config.headerLength + Config.numberOfLeds*Config.colorsPerLed + 4];
            _header = new byte[Config.headerLength];

            _header[0] = (byte) 'A'; // Alignment
            _header[1] = (byte) 'W'; // Alignment
            _header[2] = Config.numberOfLeds >> 8; // LED count high
            _header[3] = Config.numberOfLeds; // LED count low
            _header[4] = (byte) (_header[2] ^ _header[3] ^ 0x55); // Checksum
        }

        public void Connect(AvailableDrivers requestedDriver)
        {
            Disconnect();

            switch (requestedDriver)
            {
                case AvailableDrivers.Teensy:
                    _dd = new TeensyDriver(NotifyLogicAboutMessage);
                    _dd.Initialize();
                    break;
                case AvailableDrivers.Simulator:
                    _dd = new SimulatedDriver();
                    _dd.Initialize();
                    break;
                case AvailableDrivers.Null:
                    _dd = new NullDriver();
                    _dd.Initialize();
                    break;
                default:
                    _dd = null;
                    break;
            }
        }

        private void Disconnect()
        {
            if (_dd != null)
            {
                _dd.Dispose();
                _dd = null;
            }
        }

        #endregion

        #region Data transfer

        public void RelayData(byte[] colors)
        {
            if (_dd == null)
                return;


            if (colors.Length != Config.numberOfLeds * 3)
            {
                throw new InvalidDataException("List of colors must be as long as there are LEDs");
            }

            for (int j = 0; j < Config.headerLength; j++)
            {
                _buffer[j] = _header[j];
            }

            int i = Config.headerLength;

            foreach (byte color in colors)
            {
                _buffer[i++] = color;
            }

            // Create a safety buffer to prevent flickering of the first light
            _buffer[i++] = 0;
            _buffer[i++] = 0;
            _buffer[i++] = 0;
            _buffer[i] = 0;

            try
            {
                _dd.SendData(_buffer);
            }
            catch (Exception)
            {
                NotifyLogicAboutError(this, null);
                Disconnect();
                throw;
            }

            // Also, write the raw output.
            /*
            foreach (byte t in _buffer)
            {
                Debug.Write(t);
            }
            */
        }

        #endregion

        public void SetPreset(Preset p)
        {
            if (_dd == null)
                return;

            _dd.SetPreset(p);
        }
    }
}