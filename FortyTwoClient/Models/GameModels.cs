using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FortyTwoLib;
using FortyTwoLib.Database;
using Player = FortyTwoLib.Player;
using Trick = FortyTwoLib.Trick;

namespace FortyTwoClient.Models
{
	public class TableModel
	{
		public Table Table { get; private set; }
		public int PlayerId { get; private set; }

		public TableModel(Table table, int playerId)
		{
			if( table == null ) throw new ArgumentNullException("table");
			//if( playerId < 0 ) throw new ArgumentOutOfRangeException("playerId");
			Bid = new Bid();
			Table = table;
			PlayerId = playerId;
		}

		public Player GetPlayerBySeat(int position)
		{
			return Table.GetPlayerBySeat(PlayerId, position);
		}

		public string GetPlayedDotsBySeat(int position)
		{
			var s = Table.GetPlayedDotsBySeat(PlayerId, position); 
			if ( position == 0 && s == "  " ) s = " ~";
			return s;
		}

		public string PlayerNameBySeat(int seat)
		{
			var player = Table.GetPlayerBySeat(PlayerId, seat);
			if ( player == null ) return " ";
			var ret = player.Name;
			if ( Table.Bidding() && player.Bid.Amount >= 0 )
			{
				ret += ": " + (player.Bid.Amount == 0 ? "Pass" : player.Bid.Amount.ToString());
			}
			return ret;
		}

		public string BidWinnerName()
		{
			if ( Table.BidWinner() == null ) return "";
			return Table.BidWinner().Name;
		}

		public string TrumpName()
		{
			if (Table.BidWinner() == null) return "";
			return Table.BidWinner().Bid.Trump.ToString();
		}

		public string BidDescription()
		{
			if ( Table.BidWinner() == null ) return "";
			return "Bid Winner: " + BidWinnerName() + ", " + TrumpName() + ", " + BidAmount().ToString() + (BidIsLow() ? " low" : "");
		}

		public int BidAmount()
		{
			if (Table.BidWinner() == null) return 0;
			return Table.BidWinner().Bid.Amount;
		}

		public bool BidIsLow()
		{
			if (Table.BidWinner() == null) return false;
			return Table.BidWinner().Bid.IsLow;
		}

		public string GotBid(int playerId, Bid bid)
		{
			return Table.GotBid(playerId, bid);
		}

		public string Play(int playerId, string dots)
		{
			return Table.PlayDomino(playerId, dots);
		}

		public int WhoseTurn
		{
			get { return Table.LocalSeat(PlayerId, Table.WhoseTurn); }
		}

		public int OurMarks 
		{
			get { return Table.GetTeamByPlayerId(PlayerId).Marks; }
		}

		public int TheirMarks
		{
			get { return Table.GetOpponentsByPlayerId(PlayerId).Marks; }
		}

		public string GetPlayerHeaderClass(int seat)
		{
			return WhoseTurn == seat ? "highlighted" : "";
		}

		public string GetBidDivStyle()
		{
			return Table.Bidding() && Table.WhoseTurn == Table.GetSeatByPlayerId(PlayerId) ? "" : "display: none";
		}

		public string GetTricksDivStyle()
		{
			return !(Table.Bidding() && Table.WhoseTurn == Table.GetSeatByPlayerId(PlayerId)) ? "" : "display: none";
		}

		public string GetPlayDivStyle()
		{
			return !Table.Bidding() && Table.WhoseTurn == Table.GetSeatByPlayerId(PlayerId) ? "text-align: center" : "display: none; text-align: center";
		}

		public Bid Bid { get; set; }

		public IEnumerable<SelectListItem> Suits()
		{
			return Suits(Bid.IsLow);
		}

		public IEnumerable<SelectListItem> Suits(bool isLow) 
		{
			return new List<SuitEnum>() { SuitEnum.none, SuitEnum.blanks, SuitEnum.ones, SuitEnum.twos, SuitEnum.threes,
				SuitEnum.fours, SuitEnum.fives, SuitEnum.sixes, SuitEnum.doubles, SuitEnum.followMe }
				.Where(suit => !isLow || suit == SuitEnum.followMe || suit == SuitEnum.doubles)
				.Select(suit => new SelectListItem {
					Text = suit.ToString(),
					Value = suit.ToString(),
				}
			);
		}

		public IEnumerable<SelectListItem> ValidBidAmounts()
		{
			return ValidBidAmounts(Bid.IsLow);
		}

		public IEnumerable<SelectListItem> ValidBidAmounts(bool isLow)
		{
			var ret = new List<SelectListItem>();
			if ( Table.CanPass )
			{
				ret.Add(new SelectListItem {Text = "Pass", Value = "0"});
			}

			for (int i = Table.MinBid; i <= Table.MaxBid; i++)
			{
				if ( (i <= 42 && !isLow) || i % 42 == 0)
				{
					ret.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
				}
			}
			return ret;
		}

		public List<Trick> OurTricks()
		{
			return Table.GetTeamByPlayerId(PlayerId).Tricks;
		}

		public List<Trick> TheirTricks()
		{
			return Table.GetOpponentsByPlayerId(PlayerId).Tricks;
		}

		public bool GameOver()
		{
			return Table.GameOver;
		}

		public bool AllowHints()
		{
			return Table.AllowHints();
		}

		public int DominoWidth
		{
			get { return 50;  }
		}

		public int HandDominoWidth
		{
			get { return 100; }
		}

		public string SizeDescription
		{
			get { return "Small";  }
		}
	}

	public class HandModel
	{
		public int PlayerId {get; set; }

		[Required, Display(Name = "Enter Hand"), StringLength(20)]
		public string Hand { get; set; }

		public string Validate()
		{
			var bones = Hand.Split(' ').ToList();
			if ( bones.Count != 7 ) return "You must enter 7 dominoes.";
			foreach ( var b in bones )
			{
				if ( b.Length != 2 || b[0] < '0' || b[0] > '6' || b[1] < '0' || b[1] > '6' )
				{
					return string.Format("{0} is not valid.  Enter two numbers between 0 and 6", b);
				}
			}
			var dupe = bones.GroupBy(b => b).FirstOrDefault(bg => bg.Count() > 1);
			if ( dupe != null )
			{
				return string.Format("{0} can only be in your hand once.", dupe.Key);
			}

			return null;
		}

		public List<FortyTwoLib.Entities.SavedHand> GetHands()
		{
			return new DataAccess().GetHands(PlayerId);
		}
		
	}

	public class ResetModel
	{
		private Table _table;
		
		public ResetModel(int userId)
		{
			_table = Server.GetInstance().GetTableByPlayerId(userId, false);
		}

		public bool HasTable
		{
			get { return _table != null; }
		}

		public void RemoveTable()
		{
			var server = Server.GetInstance();
			server.Log.WriteError(string.Format("Table reset by user: " + _table.DisplayState()));
			server.RemoveTable(_table, true);
		}
	}
}