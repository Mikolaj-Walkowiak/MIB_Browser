using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class Utils
{
    public static string EncodeToBono(string value)
    {
        long toEncode = Int64.Parse(value);
        string toRet = "";
        if (toEncode < 0)
        {
            toEncode = toEncode * -1;
            toEncode = ~toEncode;
            toEncode += 1;
            toRet = string.Format("{0:X}", toEncode);
            while (toRet[0] == toRet[1] & toRet[0] == 'F')
            {
                toRet = toRet.Remove(0, 2);
            }
        }
        else
        {
            toRet = string.Format("{0:X}", toEncode);
            if (toRet.Length % 2 == 1)
            {
                toRet = '0' + toRet;
            }
            if ("89ABCDEF".Contains(toRet[0]))
            {
                toRet = "00" + toRet;
            }
        }
        //toRet = Convert.ToString(Convert.ToInt64(toRet.ToString(), 16), 2).PadLeft(4, '0');
        var s = String.Join("",
          toRet.Select(x => Convert.ToString(Convert.ToInt32(x + "", 16), 2).PadLeft(4, '0')));
        return s;
    }
    public static string OIDHelper(long len)
    {
        string toRet = "";
        if (len < 128)
        {
            toRet = Convert.ToString(len, 2).PadLeft(8, '0');
        }
        else
        {
            toRet = "0";
            string bigBoy = Convert.ToString(len, 2);
            string encoded = "";
            for (int i = 1; i < bigBoy.Length + 1; ++i)
            {
                encoded = bigBoy[bigBoy.Length - i] + encoded;
                if (i % 7 == 0)
                {
                    encoded = "1" + encoded;
                }
            }
            while ((encoded.Length + 1) % 8 != 0) { encoded = "0" + encoded; }
            encoded = "0" + encoded;
            toRet = toRet + encoded;
        }

        return toRet;

    }
    public static String SizeHelper(long len)
    {
        //Convert.ToString(length, 2).PadLeft(8, '0');
        string toRet = "";
        if (len < 128)
        {
            toRet = Convert.ToString(len, 2).PadLeft(8, '0');
        }
        else
        {
            toRet = "1";
            string bigBoy = Convert.ToString(len, 2);
            while ((bigBoy.Length) % 8 != 0) { bigBoy = "0" + bigBoy; }
            toRet = toRet + Convert.ToString((bigBoy.Length / 8), 2).PadLeft(7, '0');
            toRet = toRet + bigBoy;
        }


        return toRet;
    }
    public static string EncodeHelper(int typeIdentyfier, string value)
    {
        string encodedMsg = "";

        if (typeIdentyfier == 1)
        {
            string binary = "";
            if (value == "TRUE")
            {
                binary = "11111111";
            }
            else
            {
                binary = "00000000";
            }
            long length = binary.Length / 8;
            string binaryLen = SizeHelper(length);
            encodedMsg = encodedMsg + binaryLen;
            encodedMsg = encodedMsg + binary;
            return encodedMsg;

        }
        if (typeIdentyfier == 2)
        {
            string binary = EncodeToBono(value);
            long length = binary.Length / 8;
            string binaryLen = SizeHelper(length);
            encodedMsg = encodedMsg + binaryLen;
            encodedMsg = encodedMsg + binary;
            return encodedMsg;

        }
        if (typeIdentyfier == 4)
        {
            string binary = "";
            foreach (char c in value)
                binary += (Convert.ToString(c, 2).PadLeft(8, '0'));
            long length = binary.Length / 8;
            string binaryLen = SizeHelper(length);
            encodedMsg = encodedMsg + binaryLen;
            encodedMsg = encodedMsg + binary;
            return encodedMsg;
        }
        if (typeIdentyfier == 6)
        {
            string[] numbers = Regex.Split(value, @"\D+");
            var temp = new List<string>();
            foreach (var s in numbers)
            {
                if (!string.IsNullOrEmpty(s))
                    temp.Add(s);
            }
            numbers = temp.ToArray();
            int i = 40 * int.Parse(numbers[0]);
            if (numbers.Length > 2)
            {
                i += int.Parse(numbers[1]);
                encodedMsg = encodedMsg + OIDHelper(i);
                for (int j = 2; j < numbers.Length; ++j)
                {
                    encodedMsg = encodedMsg + "0";
                    encodedMsg = encodedMsg + OIDHelper(Convert.ToInt64(numbers[j]));
                }
            }
            else if (numbers.Length == 2)
            {
                i += int.Parse(numbers[1]);
                encodedMsg = encodedMsg + OIDHelper(i);
            }

            else
            {
                encodedMsg = encodedMsg + OIDHelper(i);
            }
            long length = encodedMsg.Length / 8;
            string binaryLen = SizeHelper(length);
            encodedMsg = binaryLen + encodedMsg;
            return encodedMsg;
        }
        if (typeIdentyfier == 16)
        {
            //podobno wiesz jak to zrobić xd

        }
        return encodedMsg;
    }
    public static String LocationHelper(string loc)
    {
        // Convert.ToString(Int64.Parse(loc[1]), 2).PadLeft(5, '0');
        string toRet = "";
        long addr = Int64.Parse(loc);
        if (addr < 32)
        {
            toRet = Convert.ToString(addr, 2).PadLeft(5, '0');
        }
        else
        {
            Boolean notProudOfThis = false;
            toRet = "11111";
            string bigBoy = Convert.ToString(addr, 2);
            string encoded = "";
            for (int i = 1; i < bigBoy.Length + 1; ++i)
            {
                encoded = bigBoy[bigBoy.Length - i] + encoded;
                if (i % 7 == 0)
                {
                    if (notProudOfThis) { encoded = "0" + encoded; }
                    else { encoded = "1" + encoded; }
                }
            }
            while ((encoded.Length + 1) % 8 != 0) { encoded = "0" + encoded; }
            if (!notProudOfThis) { encoded = "0" + encoded; }
            else { encoded = "1" + encoded; }
            toRet = toRet + encoded;
        }


        return toRet;
    }

}