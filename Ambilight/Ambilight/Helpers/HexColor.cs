using System;
using System.Globalization;

namespace AmadeusW.Ambilight.Helpers
{
    public class HexColor
    {
        public string HexValue { get; set; }
        public int Brightness { get { return m_brightness; } }

        private int m_brightness;
        private int m_red, m_green, m_blue;

        #region Constructor

        public HexColor()
        {
            m_blue = 0;
            m_green = 0;
            m_red = 0;
            m_brightness = 0;
        }

        public HexColor(string value)
        {
            // TODO: Do some checks here
            HexValue = value;
            m_red = int.Parse(HexValue.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            m_green = int.Parse(HexValue.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            m_blue = int.Parse(HexValue.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            m_brightness = (m_red + m_green + m_blue) / 3;
        }

        #endregion

        // TODO: use a converter

        public override string ToString()
        {
            return HexValue;
        } 
    }
}
