using System;
using System.Collections.Generic;
using System.Linq;

public class Coder
{

    private ASPFile file;

    public Coder(ASPFile f)
    {
        file = f;
    }

    private static Dictionary<String, int> types = new Dictionary<String, int>()
    {
        {"BOOLEAN",1 },
        {"INTEGER",2 },
        {"BIT STRING",3 },
        {"OCTET STRING",4 },
        {"NULL",5 },
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

    public string EncodeToBono(string value)
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
    public Boolean CheckConstraints(string type, string value)
    {
        Constraint toCheck = file.GetConstraints(type);
        if (((int)toCheck.rangeType & 0b0100) > 0) 
            return value.Length >= toCheck.min && value.Length <= toCheck.max;
        else 
            return Int64.Parse(value) >= toCheck.min && Int64.Parse(value) <= toCheck.max;

    }
    public string EncodeHelper(string type, string value)
    {
        string encodedMsg = "";
        if (types.ContainsKey(type))
        {
            int typeIdentyfier = types[type];
            if (typeIdentyfier == 1)
            {
                string binary = "";
                if(value == "TRUE")
                {
                    binary = "11111111";
                }
                else
                {
                    binary = "00000000";
                }
                int length = binary.Length / 8;
                string binaryLen = Convert.ToString(length, 2).PadLeft(8, '0');
                encodedMsg = encodedMsg + binaryLen;
                encodedMsg = encodedMsg + binary;
                return encodedMsg;

            }
            if (typeIdentyfier == 2)
            {
                string binary = EncodeToBono(value);
                int length = binary.Length / 8;
                string binaryLen = Convert.ToString(length, 2).PadLeft(8, '0');
                encodedMsg = encodedMsg + binaryLen;
                encodedMsg = encodedMsg + binary;
                return encodedMsg;

            }
            if (typeIdentyfier == 4)
            {
                string binary = "";
                foreach (char c in value)
                    binary+=(Convert.ToString(c, 2).PadLeft(8, '0'));
                int length = binary.Length / 8;
                string binaryLen = Convert.ToString(length, 2).PadLeft(8, '0');
                encodedMsg = encodedMsg + binaryLen;
                encodedMsg = encodedMsg + binary;
                return encodedMsg;
            }
            if (typeIdentyfier == 16)
            {
                //podobno wiesz jak to zrobić xd

            }
        }
        return encodedMsg;
    }

    public static String LocationHelper (string loc)
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
            for (int i = 1; i <bigBoy.Length + 1; ++i)
            {
                encoded = bigBoy[bigBoy.Length - i] + encoded;
                if(i%7 == 0)
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

    public ObjectType Encode(ObjectType toEncode, string value)
    {
        string encodedMsg = "";
        string type = toEncode.syntax;

        if (!CheckConstraints(type, value)) { Console.WriteLine("Error in encoding, value doesn't match constraints"); return toEncode; }

        if (types.ContainsKey(type)) { //basic types
            int typeIdentyfier = types[type];
            encodedMsg = "00" + PC[type];
            string binary = Convert.ToString(typeIdentyfier, 2).PadLeft(5, '0');
            encodedMsg = encodedMsg + binary;
            if (value != null)
            {
                encodedMsg = encodedMsg + EncodeHelper(type, value);
            }
            else
            {
                encodedMsg += "00000000";
            }
            toEncode.value = encodedMsg;

        }
        else
        {
            Constraint tmp = file.GetConstraints(type);
            string[] loc = tmp.location.Split(' ');
            if (loc.Length > 1) { 
            if(loc[0] == "APLICATION")
            {
                encodedMsg = "01" + PC[tmp.parentType];
            }
            else if (loc[0] == "CONTEXT-SPECIFIC")
            {
                encodedMsg = "10" + PC[tmp.parentType];
            }
            else if (loc[0] == "PRIVATE")
            {
                encodedMsg = "11" + PC[tmp.parentType];
            }
                string binary = LocationHelper(loc[1]);
                encodedMsg = encodedMsg + binary;
            }
            else
            {
                encodedMsg = "10" + PC[tmp.parentType];
                string binary = Convert.ToString(Int64.Parse(loc[0]), 2).PadLeft(5, '0');
                encodedMsg = encodedMsg + binary;
            }
            
            
            if (tmp.rangeType == ConstraintRangeType.EXPLICIT || tmp.rangeType == ConstraintRangeType.SIZE_EXPLICIT)
            {
                char[] array = encodedMsg.ToCharArray();
                array[2] = '1';
                encodedMsg = new string(array);
                if (value != null)
                {
                    string exp = EncodeHelper(tmp.parentType, value);
                    int length = exp.Length / 8;
                    string binaryLen = Convert.ToString(length, 2).PadLeft(8, '0');
                    exp = binaryLen + exp;
                    length = exp.Length / 8;
                    binaryLen = Convert.ToString(length, 2).PadLeft(8, '0');
                    encodedMsg = encodedMsg + binaryLen + exp;
                }
                else
                {
                    encodedMsg += "00000000";
                }
               
            }
            else
            {
                if (value != null)
                {
                    encodedMsg = encodedMsg + EncodeHelper(tmp.parentType, value);
                }
                else
                {
                    encodedMsg += "00000000";
                }
            }
            toEncode.value = encodedMsg;
        }


        return toEncode;
    }
	
}
