using System;
using System.Configuration;
using System.Xml;

namespace FortyTwoLib
{
	public class AppConfig
	{
		private AppConfig() { }

		public static T GetSetting<T>(string name, T defaultValue)
		{
			string val = System.Configuration.ConfigurationManager.AppSettings[name];
			if (val == null)
			{
				return defaultValue;
			}
			return (T)Convert.ChangeType(val, typeof(T));
		}

		/// <summary>
		/// Lets us get the default database without having to reference ent lib.
		/// Note that we can have a defaultDatabase section in our config file without entlib using:
		/// <section name="dataConfiguration" type="System.Configuration.SingleTagSectionHandler"/>
		/// </summary>
		/// <returns></returns>
		public static string GetDefaultDatabase()
		{
			string ret = null;
			var doc = new XmlDocument();
			try
			{
				doc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
				var node = doc.SelectSingleNode("configuration/dataConfiguration");
				ret = node.Attributes["defaultDatabase"].Value;
			}
			catch
			{
				// Intentionally eating any exceptions from above code
			}
			return ret;
		}

		public static string GetDefaultConnectionString()
		{
			return ConfigurationManager.ConnectionStrings[GetDefaultDatabase()].ConnectionString;
		}

	}
}
