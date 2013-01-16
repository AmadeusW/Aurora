using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmadeusW.Ambilight.DataClasses;

namespace AmadeusW.Ambilight.DeviceDriver
{
    public enum AvailableDrivers
    {
        Off, Teensy, Simulator, Null
    }

    public abstract class Driver : IDisposable
    {
        public abstract void Initialize();
        public abstract void Dispose();
        public abstract void SendData(byte[] data);
        public abstract void SetPreset(Preset preset);
    }
}
