using System;
using System.Collections.Generic;
using System.Linq;

public interface IType
{
	bool check(string value);
	string encode(string value);
	string decode(string value);
}

public class IntegerType : IType
{
    public int min { get; } = Int32.MinValue;
    public int max { get; } = Int32.MaxValue;

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
        int typeIdentifier = 2;
        string encodedMsg = "000";
        string binary = Convert.ToString(typeIdentifier, 2).PadLeft(5, '0');
        encodedMsg = encodedMsg + binary;
        binary = Utils.EncodeToBono(Int64.Parse(value));
        int length = binary.Length / 8;
        string binaryLen = Convert.ToString(length, 2).PadLeft(8, '0');
        encodedMsg = encodedMsg + binaryLen + binary;
        return encodedMsg;
    }
}

public class EnumIntegerType : IType
{
    public EnumIntegerType(Dictionary<string, long> d)
    {
        enumDict = d;
    }
    private Dictionary<string, long> enumDict;
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
        int typeIdentifier = 2;
        string encodedMsg = "000";
        string binary = Convert.ToString(typeIdentifier, 2).PadLeft(5, '0');
        encodedMsg = encodedMsg + binary;
        if (Int64.TryParse(value, out long num))
            binary = Utils.EncodeToBono(num);
        else
            binary = Utils.EncodeToBono(enumDict[value]);
        int length = binary.Length / 8;
        string binaryLen = Convert.ToString(length, 2).PadLeft(8, '0');
        encodedMsg = encodedMsg + binaryLen + binary;
        return encodedMsg;
    }
}

public class StringType : IType
{
    public int min { get; } = 0;
    public int max { get; } = Int32.MaxValue;

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
}

public class SequenceType : IType
{
	List<IType> members;

    public bool check(string value)
    {
        throw new NotImplementedException();
    }

    public string decode(string value)
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

    public string encode(string value)
    {
        throw new NotImplementedException();
    }
}
