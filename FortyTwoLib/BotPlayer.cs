using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FortyTwoLib.Logging;

namespace FortyTwoLib
{
	public class BotPlayer : Player
	{
		public BotPlayer(ILog log) : base(log)
		{
		}

		public override void GetPlay(int whoseTurn)
		{
			if ( whoseTurn != 0 ) return;
			var table = Server.GetInstance().GetTableByPlayerId(PlayerId, null);
			if ( table == null ) throw new ApplicationException(string.Format("Could not find table for bot player id {0}", PlayerId));

			var play = new PlayCalc().GetPlay(this, table, Log);
			table.PlayDomino(PlayerId, play.ToString());
		}

		public override void GetBid(int whoseTurn, int minBid, int maxBid, bool canPass)
		{
			if (whoseTurn != 0) return;
			var table = Server.GetInstance().GetTableByPlayerId(PlayerId, null);
			if (table == null) throw new ApplicationException(string.Format("Could not find table for bot player id {0}", PlayerId));

			var bid = new BidCalc().GetBid(this, table, Log);
			table.GotBid(PlayerId, bid);
		}

	}
}
