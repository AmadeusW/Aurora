using System;
using AmadeusW.Ambilight.DataClasses;

namespace AmadeusW.Ambilight.DeviceDriver
{
    public class SimulatedDriver : Driver
    {
        #region Fields and properties

        private SimulatedDriverOutput outputWindow;
        private bool active = false;

        #endregion

        #region Overrides of Driver

        public override void Initialize()
        {
            outputWindow = new SimulatedDriverOutput();
            outputWindow.Show();
        }

        public override void Dispose()
        {
            active = false;
            outputWindow.Close();
            outputWindow = null;
        }

        public override void SendData(byte[] data)
        {
            if (active)
            {
                outputWindow.ReceiveData(data);
            }
        }

        public override void SetPreset(Preset preset)
        {
            if (preset == null)
            {
                throw new ArgumentNullException();
            }
            outputWindow.PrepareCanvas(preset);
            active = true;
        }

        #endregion
    }
}
