using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FortyTwoLib.Entities
{
	public class PlayerStat
	{
		public decimal BidsMadeRate { get; set; }
		public int BidsMade { get; set; }
		public int BidsWon { get; set; }
		public decimal LowBidsMadeRate { get; set; }
		public int LowBidsMade { get; set; }
		public int LowBidsWon { get; set; }
		public int Bids { get; set; }
		public int PlayerId { get; set; }
		public string PlayerName { get; set; }
		public decimal WinRate { get; set; }
		public int GamesWon { get; set; }
		public int GamesPlayed { get; set; }
		public DateTime LastPlayDate { get; set; }
	}
}
