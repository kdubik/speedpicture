using System;

namespace SpeedPicture
{
    public class Tools
    {         
        public static int CreateInt(byte b1, byte b2, byte b3, byte b4)
        {
            byte[] bytes  = { b1, b2, b3, b4 };

            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            // Convert
            int i = BitConverter.ToInt32(bytes, 0);

            return i;
        }

        public static UInt32 CreateUInt(byte b1, byte b2, byte b3, byte b4)
        {
            byte[] bytes  = { b1, b2, b3, b4 };

            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            // Convert
            UInt32 i = BitConverter.ToUInt32(bytes, 0);

            return i;
        }

        public static UInt16 CreateWord(byte b1, byte b2)
        {
            byte[] bytes  = { b1, b2 };

            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            // Convert
            UInt16 i = BitConverter.ToUInt16(bytes, 0);

            return i;
        }
    
        /// <summary>
        /// Get SColor value from keyword.static
        /// Accepted are basic colors: white, red, blue...
        /// </summary>
        /// <param name="colorName">String with name of color</param>
        /// <returns>Returns SColor version of desired color.</returns>
        public static SColor TC(string colorName)
        {
            SColor res = new SColor(0,0,0);

            switch (colorName.ToLower())
            {
                case "white":
                    res.SetColor(255,255,255);
                    break;

                case "black":
                    res.SetColor(0,0,0);
                    break;

                case "red":
                    res.SetColor(255,0,0);
                    break;

                case "lime":
                    res.SetColor(0,255,0);
                    break;

                case "blue":
                    res.SetColor(0,0,255);
                    break;

                case "gray":
                    res.SetColor(128,128,128);
                    break;

                case "yellow":
                    res.SetColor(255,255,0);
                    break;

                case "cyan":
                    res.SetColor(0,255,255);
                    break;

                case "magenta":
                    res.SetColor(255,0,255);
                    break;

                case "silver":
                    res.SetColor(192,192,192);
                    break;

                case "maroon":
                    res.SetColor(128,0,0);
                    break;

                case "olive":
                    res.SetColor(128,128,0);
                    break;

                case "green":
                    res.SetColor(0,128,0);
                    break;

                case "purple":
                    res.SetColor(128,0, 128);
                    break;

                case "teal":
                    res.SetColor(0, 128, 128);
                    break;

                case "navy":
                    res.SetColor(0, 0, 128);
                    break;
                /*
                default:
                    res.SetColor(0,0,0);
                    break;
                */
            }

            return res;
        }
    }
}