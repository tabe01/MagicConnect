using System;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Utils
{

	public static void SetObjLayer(GameObject obj, int layer)
	{
		foreach(Transform child in obj.transform)
		{
			child.gameObject.layer = layer;
		}
		
		obj.layer = layer;
	}
	
	public static int RandomNum(int min, int max)
	{
		return UnityEngine.Random.Range(min, max);
	}
	
	public static Hashtable ReadJsonFile(string fileName)
	{
		if(!System.IO.File.Exists(fileName))
		{
			return new Hashtable();
		}
		
		System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
		string fileStr = "";
		string fileLine;
		
		while((fileLine = sr.ReadLine()) != null)
		{
			fileLine = fileLine.Trim();
			if(fileLine.Length >= 2 && fileLine[0] == '/' && fileLine[1] == '/')
			{
				continue;
			}
			
			fileStr += fileLine;
		}
		
		Hashtable tbl = (Hashtable)MiniJSON.JsonDecode(fileStr);
		
		sr.Close();
		return tbl;
	}
	
	public static void WriteJsonFile(string fileName, object content)
	{
		string fileContent = MiniJSON.JsonEncode(content);
		
		System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName);
		sw.Write(fileContent);
		
		sw.Close();
	}

	/// <summary>
	/// 获得字符串中开始和结束字符串中间得值
	/// </summary>
	/// <param name="str">字符串</param>
	/// <param name="s">开始</param>
	/// <param name="e">结束</param>
	/// <returns></returns> 
	public static string GetValue(string str, string s, string e)
	{
		if (string.IsNullOrEmpty(str))
			return "";
		string regex = @"^.*" + s + "(?<content>.+)" + e + ".*$";
		Regex rgClass = new Regex(regex, RegexOptions.Singleline);
		Match match = rgClass.Match(str);
		return match.Groups["content"].Value;
	}

	public static DateTime Stamp2Time(int timeStamp)
	{
		DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
		long lTime = long.Parse(timeStamp.ToString() + "0000000");
		TimeSpan toNow = new TimeSpan(lTime);

		return  dtStart.Add(toNow);
	}

	public static int Time2Stamp(DateTime dateTime)
	{
		DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
		return (int)(dateTime - startTime).TotalSeconds;
	}
}

