using UnityEngine;

//Conversion found on http://wiki.unity3d.com/index.php?title=HexConverter

namespace Offworld.SystemCore
{
    public class HTMLColorConverter
    {
        public static string RGBToHex(Color color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

        public static Color HexToRGBA(string hex)
        {
            if (hex.Length == 6 || hex.Length == 8)
            {
                byte r;
                byte g;
                byte b;
                byte a;

                bool successfulParse = byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out r);
                successfulParse &= byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out g);
                successfulParse &= byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out b);

                if (hex.Length == 6)
                    a = 255;
                else
                    successfulParse &= byte.TryParse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, null, out a);

                if (successfulParse)
                    return new Color(r, g, b, a);
            }

            Debug.LogError("HexToRGBA: Invalid input " + hex);
            return Color.magenta;
        }
    }
}
