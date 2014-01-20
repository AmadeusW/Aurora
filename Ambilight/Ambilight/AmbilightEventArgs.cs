using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmadeusW.Ambilight
{
    /// <summary>
    /// This class is used to explore custom event and message passing.
    /// It is mostly used by code in threads to communicate with the GUI.
    /// </summary>
    public class AmbilightEventArgs : EventArgs
    {
        public enum AmbilightEventActions { UpdateStatus, Unknown }
        public AmbilightEventActions Action;
        public String Details;

        public AmbilightEventArgs (AmbilightEventActions action, String details)
        {
            Action = action;
            Details = details;
        }
    }
}
