using System;
using System.Collections.Generic;

public enum AccessEnum
{
	READ_ONLY,
	READ_WRITE,
	NON_ACCESSIBLE
}
public class Access
{

	private Dictionary<string, AccessEnum> accessDict = new Dictionary<string, AccessEnum>
	{
		["read-only"] = AccessEnum.READ_ONLY,
		["read-write"] = AccessEnum.READ_WRITE,
		["non-accessible"] = AccessEnum.NON_ACCESSIBLE,
	};

    public AccessEnum getAccess(string access) => accessDict.GetValueOrDefault(access);
} 