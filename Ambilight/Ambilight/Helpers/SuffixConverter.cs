using System;
using System.Globalization;
using System.Windows.Data;

namespace AmadeusW.Ambilight.Helpers
{
    [ValueConversion(typeof (int), typeof (String))]
    public class SuffixConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Get the suffix from the parameter
            return value + " " + parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String incoming = value.ToString().Trim();
            if (incoming.Length == 0)
            {
                // This will cause an exception and the empty textbox will get a red stroke
                return null;
            }

            // Convert chars until we've encountered a not a number
            if (targetType == typeof (int))
            {
                int returnValue = 0;
                bool returnValueSet = false;

                for (int i = 0; i < incoming.Length; i++)
                {
                    if (!Char.IsDigit(incoming[i]))
                    {
                        returnValue = Int32.Parse(incoming.Substring(0, i));
                        returnValueSet = true;
                        break;
                    }
                }
                if (!returnValueSet)
                {
                    try
                    {
                        returnValue = Int32.Parse(incoming);
                    }
                    catch (FormatException)
                    {
                        // Swallow
                    }
                }
                return returnValue;
            }
            if (targetType == typeof (double))
            {
                double returnValue = 0;
                bool returnValueSet = false;
                for (int i = 0; i < incoming.Length; i++)
                {
                    if (!Char.IsDigit(incoming[i]) && incoming[i] != '.')
                    {
                        returnValue = Double.Parse(incoming.Substring(0, i));
                        returnValueSet = true;
                        break;
                    }
                }
                if (!returnValueSet)
                {
                    try
                    {
                        returnValue = Int32.Parse(incoming);
                    }
                    catch (FormatException)
                    {
                        // Swallow
                    }
                }
                return returnValue;
            }

            return 0;
        }

        #endregion
    }
}