using System;
using System.Collections.Generic;
using System.Linq;

public interface IType
{
    bool check(string value);
    string encode(string value);
    string decode(string value);
    IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null);
}

public class IntegerType : IType
{
    public long min { get; } = Int64.MinValue;
    public long max { get; } = Int64.MaxValue;
    public string classId;
    public string addr;
    public string isExplicit;

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
    public virtual string decode(string value)
    {
        throw new NotImplementedException();
    }

    public virtual string encode(string value)
    {
        string encodedMsg = "";

        if (!check(value)) { Console.WriteLine("Error in encoding, value doesn't match constraints"); return null; }

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
                if (classId == "APLICATION")
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
                string binary = Utils.LocationHelper(addr);
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
}

public class EnumIntegerType : IntegerType
{
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
    public override string decode(string value)
    {
        throw new NotImplementedException();
    }

    public override string encode(string value)
    {
        if (Int32.TryParse(value, out int num)) return base.encode(value);
        else return base.encode(enumDict[value].ToString());
    }

    public override IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }
}

public class StringType : IType
{
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
    public string decode(string value)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
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
                if (classId == "APLICATION")
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
                string binary = Utils.LocationHelper(addr);
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
}

public class OIDType : IType
{
    public bool check(string value)
    {
        throw new NotImplementedException();
    }

    public string decode(string value)
    {
        throw new NotImplementedException();
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
        throw new NotImplementedException();
    }
}

public class NullType : IType
{
    public bool check(string value)
    {
        throw new NotImplementedException();
    }

    public string decode(string value)
    {
        throw new NotImplementedException();
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
        throw new NotImplementedException();
    }
}

public class SequenceType : IType
{
    Dictionary<string, IType> members;

    public SequenceType(Dictionary<string, IType> members)
    {
        this.members = members;
    }

    public bool check(string value)
    {
        throw new NotImplementedException();
    }

    public string decode(string value)
    {
        throw new NotImplementedException();
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
        throw new NotImplementedException();
    }
}

public class SequenceOfType : IType
{
    IType baseType;

    public SequenceOfType(IType b)
    {
        baseType = b;
    }

    public bool check(string value)
    {
        throw new NotImplementedException();
    }

    public string decode(string value)
    {
        throw new NotImplementedException();
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
        throw new NotImplementedException();
    }
}

public class ChoiceType : IType
{
    Dictionary<string, IType> members;

    public ChoiceType(Dictionary<string, IType> members)
    {
        this.members = members;
    }

    public bool check(string value)
    {
        throw new NotImplementedException();
    }

    public string decode(string value)
    {
        throw new NotImplementedException();
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
        throw new NotImplementedException();
    }
}