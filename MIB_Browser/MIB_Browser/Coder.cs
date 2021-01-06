using System;
using System.Collections.Generic;


var types = new Dictionary<String, int>()
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


public class Coder
{
    public Boolean CheckConstraints(string type, string value)
    {
        Constraint toCheck = Types.GetConstraints(type);
        if (toCheck.isSize)
        {
            if (value.Length < Int32.Parse(toCheck.min) || value.Length > Int32.Parse(toCheck.max))
            {
                return false;
            }
        }
        else { 
            if(Int32.Parse(value) < Int32.Parse(toCheck.min) || Int32.Parse(value) > Int32.Parse(toCheck.max))
            {
                return false;
            }
        }
        return true;
    }
	
}
