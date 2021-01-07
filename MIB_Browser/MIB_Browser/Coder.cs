using System;
using System.Collections.Generic;
using System.Linq;

public static class Coder
{
    private static Dictionary<String, int> types = new Dictionary<String, int>()
    {
        {"BOOLEAN",1 },
        {"INTEGER",2 },
        {"BIT STRING",3 },
        {"OCTET STRING",4 },
        {"OBJECT IDENTIFIER",6 },
        {"ObjectDescriptor",7 },
        {"EXTERNAL",8 },
        {"INSTANCE OF",8 },
        {"REAL",9 },
        {"ENUMERATED",10 },
        {"EMBEDDED PDV",11 },
        {"UTF8String",12 },
        {"RELATIVE-OID",13 },
        {"SEQUENCE",16 },
        {"SEQUENCE OF",16 },
        {"SET",17 },
        {"SET OF",17 },
        {"NumericString",18 },
        {"PrintableString",19 },
        {"TeletexString",20 },
        {"T61String",20 },
        {"VideotexString",21 },
        {"IA5String",22 },
        {"UTCTime",23 },
        {"GeneralizedTime",24},
        {"GraphicString",25 },
        {"VisibleString",26 },
        {"ISO646String",26 },
        {"GeneralString",27 },
        {"UniversalString",28 },
        {"CHARACTER STRING",29 },
        {"BMPString",30 }
    };
    private static Dictionary<String, String> PC = new Dictionary<String, String>() // only the ones used in presentation
    {
        {"INTEGER","0" },
        {"NULL","0" },
        {"OCTET STRING","1" }, // 2hard4me
        {"SEQUENCE","1" },
        {"SEQUENCE OF","1" }
    };

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
    public static Boolean CheckConstraints(string type, string value)
    {
        Constraint toCheck = Types.GetConstraints(type);
        if (toCheck.isSize)
        {
            if (value.Length < Int64.Parse(toCheck.min) || value.Length > Int64.Parse(toCheck.max))
            {
                return false;
            }
        }
        else { 
            if(Int64.Parse(value) < Int64.Parse(toCheck.min) || Int64.Parse(value) > Int64.Parse(toCheck.max))
            {
                return false;
            }
        }
        return true;
    }
    public static string EncodeHelper(string type, string value)
    {
        string encodedMsg = "";
        if (types.ContainsKey(type))
        {
            int typeIdentyfier = types[type];

            if (typeIdentyfier == 2)
            {
                string binary = EncodeToBono(value);
                int length = binary.Length / 8;
                string binaryLen = Convert.ToString(length, 2).PadLeft(8, '0');
                encodedMsg = encodedMsg + binaryLen;
                encodedMsg = encodedMsg + binary;
                return encodedMsg;

            }
        }
        return encodedMsg;
    }





    public static ObjectType Encode(ObjectType toEncode, string value)
    {
        string encodedMsg = "";
        string type = toEncode.syntax;
        if (!CheckConstraints(type, value)) { Console.WriteLine("Error in encoding, value doesn't match constraints"); return toEncode; }

        if (types.ContainsKey(type)) { //basic types
            int typeIdentyfier = types[type];
            encodedMsg = "00" + PC[type];
            string binary = Convert.ToString(typeIdentyfier, 2).PadLeft(5, '0');
            encodedMsg = encodedMsg + binary;
            if( typeIdentyfier == 2)
            {
                binary = EncodeToBono(value);
                int length = binary.Length / 8;
                string binaryLen = Convert.ToString(length, 2).PadLeft(8, '0');
                encodedMsg = encodedMsg + binaryLen;
                encodedMsg = encodedMsg + binary;
                toEncode.value = encodedMsg;

            }
        }
        else
        {
            Constraint tmp = Types.GetConstraints(type);
            string[] loc = tmp.location.Split(' ');
            if(loc[0] == "APLICATION")// TODO add other types
            {
                encodedMsg = "01" + PC[tmp.parentType];
                string binary = Convert.ToString(Int64.Parse(loc[1]), 2).PadLeft(5, '0');
                encodedMsg = encodedMsg + binary;
            }
            if (tmp.isExplicit)
            {
                string exp = EncodeHelper(tmp.parentType, value);
                int length = exp.Length / 8;
                string binaryLen = Convert.ToString(length, 2).PadLeft(8, '0');
                encodedMsg = encodedMsg + binaryLen + exp;
            }
            else
            {
                encodedMsg = encodedMsg + EncodeHelper(tmp.parentType, value);
            }


            toEncode.value = encodedMsg;

        }


        return toEncode;
    }
	
}
