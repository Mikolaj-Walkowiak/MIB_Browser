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
    public bool check(string value)
    {
        return Int64.Parse(value) >= min && Int64.Parse(value) <= max;
    }
    public string decode(string value)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
        throw new NotImplementedException();
    }

    public IType derive(long min, long max, string classId, string addr, string isExplicit)
    {
        return new IntegerType(min, max,
            classId != null ? classId : this.classId,
            addr != null ? addr : this.addr,
            isExplicit != null ? isExplicit : this.isExplicit
            );
    }
}

public class EnumIntegerType : IType
{
    public EnumIntegerType(Dictionary<string, long> d)
    {
        enumDict = d;
    }
    private Dictionary<string, long> enumDict;
    public string location;
    public bool check(string value)
    {
        if (Int32.TryParse(value, out int num)) return enumDict.ContainsValue(num);
        else return enumDict.ContainsKey(value);
    }
    public string decode(string value)
    {
        throw new NotImplementedException();
    }

    public string encode(string value)
    {
        throw new NotImplementedException();
    }

    public IType derive(long min, long max, string classId = null, string addr = null, string isExplicit = null)
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
        throw new NotImplementedException();
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