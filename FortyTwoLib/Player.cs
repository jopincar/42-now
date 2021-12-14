using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web.Security;
using FortyTwoLib.Logging;

namespace FortyTwoLib
{
	public class Player
	{
		public ILog Log { get; set; }
		private Messenger _messenger;

		public int PlayerId { get; set; }
		public int GamePlayerDbId { get; set; }
		public string Name { get; set; }
		public string NetworkAddress { get; set; }
		public Bid Bid { get; set; }
		public Hand Hand { get; set; }
		public string OriginalHand { get; set; }
		public int DominoWidth { get; set; } // This is per player based on their viewing device
		public int MarksWon { get; set; }
		public int LowMarksWon { get; set; }
		public int MarksLost { get; set; }
		public int LowMarksLost { get; set; }
		public int BidsSetSum { get; set; }
		public int BidsMadeSum { get; set; }

		public bool IsRegistered()
		{
			return !(this is BotPlayer) && PlayerId > 0;
		}

		public Player(ILog log)
		{
			Log = log;
			PlayerId = -1;
			Name = "<Unknown>";
			Hand = new Hand();
			Bid = new Bid();
			_messenger = new Messenger(log);
			DominoWidth = 50;
		}

		public void TableAddedPlayer(Table table, Player player, int position)
		{
			_messenger.SendTableAddedPlayer(NetworkAddress, table, player, position);
		}

		public void StartMark(Hand hand)
		{
			Bid = new Bid();
			Hand = hand;
			OriginalHand = hand.ToString();
			var domPosX = 9;
			foreach ( Domino d in hand )
			{
				d.Position = new Point(domPosX, 10); // Really need this somewhere else but not sure where
				domPosX += DominoWidth + 9;
			}
			_messenger.SendMarkStarted(NetworkAddress, this);
		}

		public virtual void GetBid(int whoseTurn, int minBid, int maxBid, bool canPass)
		{
			_messenger.SendGetBid(NetworkAddress, whoseTurn, minBid, maxBid, canPass);
		}

		public virtual void GetPlay(int whoseTurn)
		{
			_messenger.SendGetPlay(NetworkAddress, whoseTurn);
		}

		public void PlayMade(int whoseTurn, string dots)
		{
			_messenger.SendPlayMade(NetworkAddress, whoseTurn, dots);
		}

		public void BidWon(int bidWinnerSeat, Player bidWinner)
		{
			_messenger.SendBidwon(NetworkAddress, bidWinnerSeat, bidWinner.Name, bidWinner.Bid);
		}

		public void TrickWon(int winningSeat, Trick trick)
		{
			_messenger.SendTrickWon(NetworkAddress, winningSeat, trick);
		}

		public void MarkWon(int ourMarks, int theirMarks, bool youWon, int marksBet, int bidWinner)
		{
			_messenger.SendMarkWon(NetworkAddress, ourMarks, theirMarks, youWon, marksBet, bidWinner);
		}

		public void GameWon(bool youWon)
		{
			_messenger.SendGameWon(NetworkAddress, youWon);
		}

		public void BidMade(int whoseTurn, int amount)
		{
			_messenger.SendBidMade(NetworkAddress, whoseTurn, amount);
		}

		public void OnException(Exception exception)
		{
			_messenger.SendServerErrorMessage(NetworkAddress, exception);
		}
	}

}
