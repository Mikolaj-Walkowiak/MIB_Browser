using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

public interface IType
{
    bool check(string value);
    string encode(string value);
    string decode(ASPFile context, string value);
    IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null);
    string getRange();
    string getClassId();
    string getAddr();
}

public class IntegerType : IType
{
    public long min { get; } = Int64.MinValue;
    public long max { get; } = Int64.MaxValue;
    public string classId;
    public string addr;
    public string isExplicit;

    public string getRange() { return min + ", " + max; }
    public IntegerType(long min, long max, string classId, string addr, string isExplicit)
    {
        this.min = min;
        this.max = max;
        this.classId = classId;
        this.addr = addr;
        this.isExplicit = isExplicit;
    }
    public virtual bool check(string value)
    {
        return Int64.Parse(value) >= min && Int64.Parse(value) <= max;
    }
    public virtual string decode(ASPFile context, string value)
    {
        if(isExplicit == "EXPLICIT")
        {
            var info = context.decodeType(value);
            return "(" + classId + " " + addr + "):" + info.Item1.decode(context, info.Item3);
        }
        else
        {
            var hex = string.Join("",Enumerable.Range(0, value.Length / 8).Select(i => Convert.ToByte(value.Substring(i * 8, 8), 2).ToString("X2")));
            return BigInteger.Parse(hex, NumberStyles.AllowHexSpecifier).ToString();
        }
    }

    public virtual string encode(string value)
    {
        string encodedMsg = "";

        if (!check(value)) { return null; }

        if (classId == addr && addr == isExplicit && isExplicit == null)
        { //basic types
            int typeIdentyfier = 2;
            encodedMsg = "00" + "0";
            string binary = Convert.ToString(typeIdentyfier, 2).PadLeft(5, '0');
            encodedMsg = encodedMsg + binary;
            if (value != null)
            {
                encodedMsg = encodedMsg + Utils.EncodeHelper(2, value);
            }
            else
            {
                encodedMsg += "00000000";
            }
            return encodedMsg;
        }
        else
        {
            if (classId != null)
            {
                if (classId == "APPLICATION")
                {
                    encodedMsg = "01" + "0"; //PC[tmp.parentType];
                }
                else if (classId == "CONTEXT-SPECIFIC")
                {
                    encodedMsg = "10" + "0";
                }
                else if (classId == "PRIVATE")
                {
                    encodedMsg = "11" + "0";
                }
                string binary = Utils.LocationHelper(addr);
                encodedMsg = encodedMsg + binary;
            }
            else
            {
                encodedMsg = "10" + "0";
                string binary = "";
                if (addr != null)
                {
                    binary = Utils.LocationHelper(addr);
                    isExplicit = "EXPLICIT";
                }
                encodedMsg = encodedMsg + binary;
            }
            if (isExplicit == "EXPLICIT")
            {
                char[] array = encodedMsg.ToCharArray();
                array[2] = '1';
                encodedMsg = new string(array);
                if (value != null)
                {
                    string exp = Utils.EncodeHelper(2, value);
                    long length = exp.Length / 8;
                    string binaryLen = Utils.SizeHelper(length);
                    exp = binaryLen + exp;
                    length = exp.Length / 8;
                    binaryLen = Utils.SizeHelper(length);
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
                    encodedMsg = encodedMsg + Utils.EncodeHelper(2, value);
                }
                else
                {
                    encodedMsg += "00000000";
                }
            }
            return encodedMsg;
        }
    }

    public virtual IType derive(long min, long max, string classId, string addr, string isExplicit)
    {
        return new IntegerType(min, max,
            classId != null ? classId : this.classId,
            addr != null ? addr : this.addr,
            isExplicit != null ? isExplicit : this.isExplicit
            );
    }

    public string getClassId()
    {
        return classId;
    }

    public string getAddr()
    {
        return addr;
    }
}

public class EnumIntegerType : IntegerType
{
    public EnumIntegerType(Dictionary<string, long> d, string classId, string addr, string isExplicit) : base(Int64.MinValue, Int64.MaxValue, classId, addr, isExplicit)
    {
        enumDict = d;
    }
    public EnumIntegerType(Dictionary<string, long> d) : base(Int64.MinValue, Int64.MaxValue, null, null, null)
    {
        enumDict = d;
    }
    private Dictionary<string, long> enumDict;
    public override bool check(string value)
    {
        if (Int32.TryParse(value, out int num)) return enumDict.ContainsValue(num);
        else return enumDict.ContainsKey(value);
    }
    public override string decode(ASPFile context, string value)
    {
        throw new NotImplementedException();
    }

    public override string encode(string value)
    {
        if (!check(value)) { return null; }
        if (Int32.TryParse(value, out int num)) return base.encode(value);
        else return base.encode(enumDict[value].ToString());
    }

    public override IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }
}

public class BoolType : IType
{
    public string classId;
    public string addr;
    public string isExplicit;
    public BoolType(string classId, string addr, string isExplicit)
    {
        this.classId = classId;
        this.addr = addr;
        this.isExplicit = isExplicit;
    }

    public bool check(string value)
    {
        return new string[] { "TRUE", "FALSE" }.Contains(value);
    }
    public string decode(ASPFile context, string value)
    {
        if (isExplicit == "EXPLICIT")
        {
            var info = context.decodeType(value);
            return "(" + classId + " " + addr + "):" + info.Item1.decode(context, info.Item3);
        }
        else
        {
            return value == "11111111" ? "TRUE" : "FALSE";
        }
    }

    public string encode(string value)
    {
        if (!check(value)) { return null; }
        string encodedMsg = "";
        if (classId == "APPLICATION")
        {
            encodedMsg = "010"; //PC[tmp.parentType];
        }
        else if (classId == "CONTEXT-SPECIFIC")
        {
            encodedMsg = "100";
        }
        else if (classId == "PRIVATE")
        {
            encodedMsg = "110";
        }
        else
        {
            encodedMsg = "000";
        }
        string binAddr = addr != null ? Utils.LocationHelper(addr) : "00001";
        string val = value == "FALSE" ? "00000000" : "11111111";
        return encodedMsg + binAddr + "00000001" + val;
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new BoolType(classId, addr, isExplicit);
    }

    public string getRange()
    {
        throw new NotImplementedException();
    }

    public string getClassId()
    {
        return classId;
    }

    public string getAddr()
    {
        return addr;
    }
}

public class StringType : IType
{
    public string getRange() { return min + ", " + max; }
    public long min { get; } = 0;
    public long max { get; } = Int64.MaxValue;
    public string classId;
    public string addr;
    public string isExplicit;

    public StringType(long min, long max, string classId, string addr, string isExplicit)
    {
        this.min = min;
        this.max = max;
        this.classId = classId;
        this.addr = addr;
        this.isExplicit = isExplicit;
    }
    public bool check(string value)
    {
        return value.Length >= min && value.Length <= max;
    }
    public string decode(ASPFile context, string value)
    {
        if (isExplicit == "EXPLICIT")
        {
            var info = context.decodeType(value);
            return "(" + classId + " " + addr + "):" + info.Item1.decode(context, info.Item3);
        }
        else
        {
            string str = "";
            while(value.Length > 0)
            {
                str += (char)Convert.ToInt32(value.Substring(0, 8), 2);
                value = value.Substring(8);
            }
            return "\"" + str + "\"";
        }
    }

    public string encode(string value)
    {
        if (!check(value)) { return null; }

        string encodedMsg = "";

        if (!check(value)) { Console.WriteLine("Error in encoding, value doesn't match constraints"); return null; }

        if (classId == addr && addr == isExplicit && isExplicit == null)
        { //basic types
            int typeIdentyfier = 4;
            encodedMsg = "00" + "1";
            string binary = Convert.ToString(typeIdentyfier, 2).PadLeft(5, '0');
            encodedMsg = encodedMsg + binary;
            if (value != null)
            {
                encodedMsg = encodedMsg + Utils.EncodeHelper(4, value);
            }
            else
            {
                encodedMsg += "00000000";
            }
            return encodedMsg;
        }
        else
        {
            if (classId != null)
            {
                if (classId == "APPLICATION")
                {
                    encodedMsg = "01" + "1"; //PC[tmp.parentType];
                }
                else if (classId == "CONTEXT-SPECIFIC")
                {
                    encodedMsg = "10" + "1";
                }
                else if (classId == "PRIVATE")
                {
                    encodedMsg = "11" + "1";
                }
                string binary = Utils.LocationHelper(addr);
                encodedMsg = encodedMsg + binary;
            }
            else
            {
                encodedMsg = "10" + "1";
                string binary = "";
                if (addr != null)
                {
                    binary = Utils.LocationHelper(addr);
                    isExplicit = "EXPLICIT";
                }
                encodedMsg = encodedMsg + binary;
            }
            if (isExplicit == "EXPLICIT")
            {
                char[] array = encodedMsg.ToCharArray();
                array[2] = '1';
                encodedMsg = new string(array);
                if (value != null)
                {
                    string exp = Utils.EncodeHelper(4, value);
                    long length = exp.Length / 8;
                    string binaryLen = Utils.SizeHelper(length);
                    exp = binaryLen + exp;
                    length = exp.Length / 8;
                    binaryLen = Utils.SizeHelper(length);
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
                    encodedMsg = encodedMsg + Utils.EncodeHelper(4, value);
                }
                else
                {
                    encodedMsg += "00000000";
                }
            }
            return encodedMsg;
        }
    }
    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new StringType(min, max,
            classId != null ? classId : this.classId,
            addr != null ? addr : this.addr,
            isExplicit != null ? isExplicit : this.isExplicit
            );
    }

    public string getClassId()
    {
        return classId;
    }

    public string getAddr()
    {
        return addr;
    }
}

public class OIDType : IType
{
    public string getRange() { return min + ", " + max; }
    public long min { get; } = 0;
    public long max { get; } = Int64.MaxValue;
    public string classId;
    public string addr;
    public string isExplicit;

    public OIDType(long min, long max, string classId, string addr, string isExplicit)
    {
        this.min = min;
        this.max = max;
        this.classId = classId;
        this.addr = addr;
        this.isExplicit = isExplicit;
    }
    public bool check(string value)
    {
        string[] numbers = Regex.Split(value, @"\D+");
        var temp = new List<string>();
        foreach (var s in numbers)
        {
            if (!string.IsNullOrEmpty(s))
                temp.Add(s);
        }
        numbers = temp.ToArray();
        return numbers.Length >= min && numbers.Length <= max;
    }

    public string decode(ASPFile context, string value)
    {
        if (isExplicit == "EXPLICIT")
        {
            var info = context.decodeType(value);
            return "(" + classId + " " + addr + "):" + info.Item1.decode(context, info.Item3);
        }
        else
        {
            var oid = Convert.ToInt32(value.Substring(0, 8), 2);
            value = value.Substring(8);
            string str = (oid/40).ToString() + "." + (oid%40).ToString();
            while(value.Length > 0)
            {
                oid = Convert.ToInt32(value.Substring(0, 8), 2);
                value = value.Substring(8);
                str += "." + oid.ToString();
            }
            return str;
        }
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new OIDType(min, max, classId, addr, isExplicit);
    }


    public string encode(string value)
    {
        if (!check(value)) { return null; }

        string encodedMsg = "";

        if (classId == null && addr == null && isExplicit == null)
        { //basic types
            int typeIdentyfier = 6;
            encodedMsg = "00" + "0";
            string binary = Convert.ToString(typeIdentyfier, 2).PadLeft(5, '0');
            encodedMsg = encodedMsg + binary;
            if (value != null)
            {
                encodedMsg = encodedMsg + Utils.EncodeHelper(6, value);
            }
            else
            {
                encodedMsg += "00000000";
            }
            return encodedMsg;
        }
        else
        {
            if (classId != null)
            {
                if (classId == "APPLICATION")
                {
                    encodedMsg = "01" + "0"; //PC[tmp.parentType];
                }
                else if (classId == "CONTEXT-SPECIFIC")
                {
                    encodedMsg = "10" + "0";
                }
                else if (classId == "PRIVATE")
                {
                    encodedMsg = "11" + "0";
                }
                string binary = Utils.LocationHelper(addr);
                encodedMsg = encodedMsg + binary;
            }
            else
            {
                encodedMsg = "10" + "0";
                string binary = "";
                if (addr != null)
                {
                    binary = Utils.LocationHelper(addr);
                    isExplicit = "EXPLICIT";
                }
                encodedMsg = encodedMsg + binary;
            }
            if (isExplicit == "EXPLICIT")
            {
                char[] array = encodedMsg.ToCharArray();
                array[2] = '1';
                encodedMsg = new string(array);
                if (value != null)
                {
                    string exp = Utils.EncodeHelper(6, value);
                    long length = exp.Length / 8;
                    string binaryLen = Utils.SizeHelper(length);
                    exp = binaryLen + exp;
                    length = exp.Length / 8;
                    binaryLen = Utils.SizeHelper(length);
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
                    encodedMsg = encodedMsg + Utils.EncodeHelper(6, value);
                }
                else
                {
                    encodedMsg += "00000000";
                }
            }
            return encodedMsg;
        }
    }

    public string getClassId()
    {
        return classId;
    }

    public string getAddr()
    {
        return addr;
    }
}

public class NullType : IType
{
    public string classId;
    public string addr;

    public NullType(string classId, string addr)
    {
        this.classId = classId;
        this.addr = addr;
    }

    public string getRange() { return "is Null"; }
    public bool check(string value)
    {
        return true;
    }

    public string decode(ASPFile context, string value)
    {
        return "NULL";
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new NullType(classId, addr);
    }

    public string encode(string value)
    {
        string encodedMsg = "";
        if (classId == "APPLICATION")
        {
            encodedMsg = "010"; //PC[tmp.parentType];
        }
        else if (classId == "CONTEXT-SPECIFIC")
        {
            encodedMsg = "100";
        }
        else if (classId == "PRIVATE")
        {
            encodedMsg = "110";
        }
        else
        {
            encodedMsg = "000";
        }
        string binAddr = addr != null ? Utils.LocationHelper(addr) : "00101";
        return encodedMsg + binAddr + "00000000";
    }

    public string getClassId()
    {
        return classId;
    }

    public string getAddr()
    {
        return addr;
    }
}

public class SequenceType : IType
{
    public string getRange() { return "is Sequence"; }
    Dictionary<string, IType> members;

    public SequenceType(Dictionary<string, IType> members)
    {
        this.members = members;
    }

    public bool check(string value)
    {
        value = value.Substring(1, value.Length - 2);
        List<Tuple<string, string>> elements = new List<Tuple<string, string>>();
        for (Match valueMatch = Expressions.Types.Sequences.inputSplit.Match(value); valueMatch.Success; valueMatch = valueMatch.NextMatch())
            elements.Add(new Tuple<string, string>(valueMatch.Groups[1].Value.Trim(), valueMatch.Groups[2].Value.Trim()));
        List<string> encoded = new List<string>();
        foreach (Tuple<string, string> el in elements) 
            if(!members.ContainsKey(el.Item1)) return false;
        return true;
    }

    public string decode(ASPFile context, string value)
    {
        string ret = "{ ";
        while(value.Length > 0)
        {
            var info = context.decodeType(value);
            ret += info.Item1.decode(context, info.Item3.Substring(0, (int)info.Item2*8)) + ", ";
            value = info.Item3.Substring((int)info.Item2*8);
        }
        return ret.Substring(0, ret.Length-2) + " }";
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
        if (!check(value)) { return null; }

        value = value.Substring(1, value.Length - 2);
        List<Tuple<string, string>> elements = new List<Tuple<string, string>>();
        for (Match valueMatch = Expressions.Types.Sequences.inputSplit.Match(value); valueMatch.Success; valueMatch = valueMatch.NextMatch())
            elements.Add(new Tuple<string, string>(valueMatch.Groups[1].Value.Trim(), valueMatch.Groups[2].Value.Trim()));
        List<string> encoded = new List<string>();
        foreach (Tuple<string, string> el in elements)
        {
            string enc = members[el.Item1].encode(el.Item2);
            if (enc != null) encoded.Add(enc);
            else return null;
        }
        string children = string.Join("", encoded);
        return "00110000" + Utils.SizeHelper(children.Length / 8) + children;
    }

    public string getClassId()
    {
        return null;
    }

    public string getAddr()
    {
        return null;
    }
}

public class SequenceOfType : IType
{
    public string getRange() { return "is Sequence"; }
    IType baseType;

    public SequenceOfType(IType b)
    {
        baseType = b;
    }

    public bool check(string value)
    {
        return true;
    }

    public string decode(ASPFile context, string value)
    {
        throw new NotImplementedException();
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
        if (!check(value)) { return null; }

        value = value.Substring(1, value.Length - 2);
        List<string> elements = new List<string>();
        for (Match valueMatch = Expressions.Types.Sequences.seqOfSplit.Match(value); valueMatch.Success; valueMatch = valueMatch.NextMatch())
            if(valueMatch.Value.Trim().Length > 0) elements.Add(valueMatch.Value.Trim());
        List<string> encoded = new List<string>();
        foreach (string el in elements)
        {
            string enc = baseType.encode(el);
            if (enc != null) encoded.Add(enc);
            else return null;
        }
        string children = string.Join("", encoded);
        return "00110000" + Utils.SizeHelper(children.Length / 8) + children;
    }

    public string getClassId()
    {
        return null;
    }

    public string getAddr()
    {
        return null;
    }
}

public class ChoiceType : IType
{
    public string getRange() { return "is Choice"; }
    Dictionary<string, IType> members;

    public ChoiceType(Dictionary<string, IType> members)
    {
        this.members = members;
    }

    public bool check(string value)
    {
        if (value.IndexOf(" ") == -1) return false;
        string choiceName = value.Substring(0, value.IndexOf(" "));
        return members.ContainsKey(choiceName);
    }

    public string decode(ASPFile context, string value)
    {
        throw new NotImplementedException();
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
        if (!check(value)) return null;
        string[] spl = value.Split(" ");
        string choiceName = spl[0].Trim();
        string choiceValue = string.Join(' ', spl.AsSpan(1, spl.Length - 1).ToArray());
        return members[choiceName].encode(choiceValue);
    }

    public string getClassId()
    {
        return null;
    }

    public string getAddr()
    {
        return null;
    }
}