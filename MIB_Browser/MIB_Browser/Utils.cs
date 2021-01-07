using System;
using System.Collections.Generic;

public static class Utils
{
    public static string EncodeToBono(long value)
    {
        long toEncode = value;
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
} 