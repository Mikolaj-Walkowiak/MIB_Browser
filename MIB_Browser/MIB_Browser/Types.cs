using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

public abstract class IType
{
    public string name { get; set; }
    public string classId { get; set; }
    public string addr { get; set; }
    public string isExplicit { get; set; }
    public long min { get; set; } = Int64.MinValue;
    public long max { get; set; } = Int64.MaxValue;
    public abstract bool check(string value);
    public abstract string encode(string value);
    public abstract string decode(ASPFile context, string value);
    public abstract IType derive(string name, long min, long max, string classId = null, string addr = null, string isExplicit = null);
    public abstract string getRange();
}

public class IntegerType : IType
{

    public override string getRange() { return min + ", " + max; }
    public IntegerType(string name, long min, long max, string classId, string addr, string isExplicit)
    {
        this.name = name;
        this.min = min;
        this.max = max;
        this.classId = classId;
        this.addr = addr;
        this.isExplicit = isExplicit;
    }
    public override bool check(string value)
    {
        return Int64.Parse(value) >= min && Int64.Parse(value) <= max;
    }
    public override string decode(ASPFile context, string value)
    {
        if(isExplicit == "EXPLICIT")
        {
            var info = context.decodeType(value);
            return "(" + classId + " " + addr + "):" + info.Item1.decode(context, info.Item3);
        }
        else
        {
            var hex = string.Join("",Enumerable.Range(0, value.Length / 8).Select(i => Convert.ToByte(value.Substring(i * 8, 8), 2).ToString("X2")));
            return name + ": " + BigInteger.Parse(hex, NumberStyles.AllowHexSpecifier).ToString();
        }
    }

    public override string encode(string value)
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

    public override IType derive(string name, long min, long max, string classId, string addr, string isExplicit)
    {
        return new IntegerType(name, min, max,
            classId != null ? classId : this.classId,
            addr != null ? addr : this.addr,
            isExplicit != null ? isExplicit : this.isExplicit
            );
    }
}

public class EnumIntegerType : IntegerType
{
    public EnumIntegerType(string name, Dictionary<string, long> d, string classId, string addr, string isExplicit) : base(name, Int64.MinValue, Int64.MaxValue, classId, addr, isExplicit)
    {
        enumDict = d;
    }
    public EnumIntegerType(string name, Dictionary<string, long> d) : base(name, Int64.MinValue, Int64.MaxValue, null, null, null)
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

    public override IType derive(string name, long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }
}

public class BoolType : IType
{
    public BoolType(string name, string classId, string addr, string isExplicit)
    {
        this.name = name;
        this.classId = classId;
        this.addr = addr;
        this.isExplicit = isExplicit;
    }

    public override bool check(string value)
    {
        return new string[] { "TRUE", "FALSE" }.Contains(value);
    }
    public override string decode(ASPFile context, string value)
    {
        if (isExplicit == "EXPLICIT")
        {
            var info = context.decodeType(value);
            return "(" + classId + " " + addr + "):" + info.Item1.decode(context, info.Item3);
        }
        else
        {
            return name + ": " + (value == "11111111" ? "TRUE" : "FALSE");
        }
    }

    public override string encode(string value)
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

    public override IType derive(string name, long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new BoolType(name, classId, addr, isExplicit);
    }

    public override string getRange()
    {
        throw new NotImplementedException();
    }
}

public class StringType : IType
{
    public override string getRange() { return min + ", " + max; }

    public StringType(string name, long min, long max, string classId, string addr, string isExplicit)
    {
        this.name = name;
        this.min = min;
        this.max = max;
        this.classId = classId;
        this.addr = addr;
        this.isExplicit = isExplicit;
    }
    public override bool check(string value)
    {
        return value.Length >= min && value.Length <= max;
    }
    public override string decode(ASPFile context, string value)
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
            return name + ": " + "\"" + str + "\"";
        }
    }

    public override string encode(string value)
    {
        if (!check(value)) { return null; }

        string encodedMsg = "";

        if (!check(value)) { Console.WriteLine("Error in encoding, value doesn't match constraints"); return null; }

        if (classId == addr && addr == isExplicit && isExplicit == null)
        { //basic types
            int typeIdentyfier = 4;
            encodedMsg = "00" + "0";
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
                    exp = "00000100" + exp;
                    long length = exp.Length / 8;
                    string binaryLen = Utils.SizeHelper(length);
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
    public override IType derive(string name, long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new StringType(name, min, max,
            classId != null ? classId : this.classId,
            addr != null ? addr : this.addr,
            isExplicit != null ? isExplicit : this.isExplicit
            );
    }
}

public class OIDType : IType
{
    public override string getRange() { return min + ", " + max; }

    public OIDType(string name, long min, long max, string classId, string addr, string isExplicit)
    {
        this.name = name;
        this.min = min;
        this.max = max;
        this.classId = classId;
        this.addr = addr;
        this.isExplicit = isExplicit;
    }
    public override bool check(string value)
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

    public override string decode(ASPFile context, string value)
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
                string v = value.Substring(0, 8);
                string fullval = v.Substring(1, 7);
                while(v[0] == '1')
                {
                    value = value.Substring(8);
                    v = value.Substring(0, 8);
                    fullval += v.Substring(1, 7);
                }
                oid = Convert.ToInt32(fullval, 2);
                value = value.Substring(8);
                str += "." + oid.ToString();
            }
            ITreeNode node = context.findPath(str.Split("."), context.root);
            //if (node != null) return name + ": " + node.getName();
            //else 
            return name + ": " + str;
        }
    }

    public override IType derive(string name, long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new OIDType(name, min, max, classId, addr, isExplicit);
    }


    public override string encode(string value)
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
}

public class NullType : IType
{

    public NullType(string name, string classId, string addr, string isExplicit)
    {
        this.name = name;
        this.classId = classId;
        this.addr = addr;
        this.isExplicit = isExplicit;
    }

    public override string getRange() { return "is Null"; }
    public override bool check(string value)
    {
        return true;
    }

    public override string decode(ASPFile context, string value)
    {
        return name + ": " + "NULL";
    }

    public override IType derive(string name, long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new NullType(name, classId, addr, isExplicit);
    }

    public override string encode(string value)
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
}

public class SequenceType : IType
{
    public override string getRange() { return "is Sequence"; }
    Dictionary<string, IType> members;

    public SequenceType(string name, Dictionary<string, IType> members, string classId, string addr, string isExplicit)
    {
        this.name = name;
        this.members = members;
        this.classId = classId;
        this.addr = addr;
        this.isExplicit = isExplicit;
    }

    public override bool check(string value)
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

    public override string decode(ASPFile context, string value)
    {
        string ret = "{ ";
        while(value.Length > 0)
        {
            var info = context.decodeType(value);
            ret += info.Item1.decode(context, info.Item3.Substring(0, (int)info.Item2*8)) + ", ";
            value = info.Item3.Substring((int)info.Item2*8);
        }
        return name + ": " + ret.Substring(0, ret.Length-2) + " }";
    }

    public override IType derive(string name, long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new SequenceType(name, members, classId, addr, isExplicit);
    }

    public override string encode(string value)
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
        string pc = "00";
        if (classId == "CONTEXT-SPECIFIC") pc = "10";
        else if (classId == "APPLICATION") pc = "01";
        else if (classId == "PRIVATE") pc = "11";
        return pc + "1" + (addr != null ? Utils.LocationHelper(addr) : "10000") + Utils.SizeHelper(children.Length / 8) + children;
    }
}

public class SequenceOfType : IType
{
    public override string getRange() { return "is Sequence"; }
    IType baseType;

    public SequenceOfType(string name, IType b)
    {
        this.name = name;
        baseType = b;
    }

    public override bool check(string value)
    {
        return true;
    }

    public override string decode(ASPFile context, string value)
    {
        throw new NotImplementedException();
    }

    public override IType derive(string name, long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new SequenceOfType(name, baseType);
    }

    public override string encode(string value)
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
}

public class ChoiceType : IType
{
    public override string getRange() { return "is Choice"; }
    Dictionary<string, IType> members;

    public ChoiceType(string name, Dictionary<string, IType> members)
    {
        this.name = name;
        this.members = members;
    }

    public override bool check(string value)
    {
        if (value.IndexOf(" ") == -1) return false;
        string choiceName = value.Substring(0, value.IndexOf(" "));
        return members.ContainsKey(choiceName);
    }

    public override string decode(ASPFile context, string value)
    {
        throw new NotImplementedException();
    }

    public override IType derive(string name, long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        return new ChoiceType(name, members);
    }

    public override string encode(string value)
    {
        if (!check(value)) return null;
        string[] spl = value.Split(" ");
        string choiceName = spl[0].Trim();
        string choiceValue = string.Join(' ', spl.AsSpan(1, spl.Length - 1).ToArray());
        return members[choiceName].encode(choiceValue);
    }
}