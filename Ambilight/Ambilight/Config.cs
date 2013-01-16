using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmadeusW.Ambilight
{
    public static class Config
    {
        public const int numberOfLeds = 25;
        public const int numberOfSectors = 25; // Getting ready for scenario whith multiple sectors per one LED or vice versa
        public const int colorsPerLed = 3;
        public const int headerLength = 5; // magic word + led info + checksum
    }
}
