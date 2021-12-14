using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FortyTwoLib.Database;
using FortyTwoLib.Entities;

namespace FortyTwoClient.Models
{
	public class StatsModel
	{
		public List<PlayerStat> GetTopPlayers(int maxResults)
		{
			return new DataAccess().GetTopPlayers(maxResults, 60, 5);
		}
	}
}