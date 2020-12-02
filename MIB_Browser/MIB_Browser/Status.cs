using System;
using System.Collections.Generic;

public enum StatusEnum
{
	MANDATORY,
	DEPRECATED,
	CURRENT
}
public class Status
{

	private Dictionary<string, StatusEnum> statusDict = new Dictionary<string, StatusEnum>
	{
		["mandatory"] = StatusEnum.MANDATORY,
		["deprecated"] = StatusEnum.DEPRECATED,
		["current"] = StatusEnum.CURRENT,
	};

	public StatusEnum getStatus(string access) => statusDict.GetValueOrDefault(access);
}