using System;
using System.Collections.Generic;
using FortyTwoLib;

namespace FortyTwoClient.Models
{
	public class AdminModel
	{
		public List<Table> GetTables(int maxValue)
		{
			var ret = FortyTwoLib.Server.GetInstance().Tables;
			return ret;
		}
	}
}