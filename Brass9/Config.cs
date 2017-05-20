using System;
using System.Configuration;

namespace Brass9
{
	public class Config
	{
		public static string Get(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}
	}
}
