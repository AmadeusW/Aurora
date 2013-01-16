using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmadeusW.Ambilight.DataClasses;

namespace AmadeusW.Ambilight.DeviceDriver
{
    class NullDriver : Driver
    {
        #region Overrides of Driver

        public override void Initialize()
        {
            return;
        }

        public override void Dispose()
        {
            return;
        }

        public override void SendData(byte[] data)
        {
            return;
        }

        public override void SetPreset(Preset preset)
        {
            return;
        }

        #endregion
    }
}
