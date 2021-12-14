using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using FortyTwoLib.Logging;

namespace FortyTwoLib
{
	public class Messenger
	{
		JavaScriptSerializer _serializer = new JavaScriptSerializer();

		public ILog Log { get; set; }

		public Messenger(ILog log)
		{
			Log = log;
		}

		private void SendMessage(Message msg, string address)
		{
			string msgString = _serializer.Serialize(msg);
			Log.WriteVerbose(string.Format("Message sent to {0}: {1}", address, msgString));
			MailBox.GetInstance().Send(address, msg);
		}

		public void SendTableAddedPlayer(string address, Table table, Player newPlayer, int position)
		{

			var msg = new TableAddPlayerMessage {
				TableId = table.TableId,
				TableName = table.Name,
				NewPlayerName = newPlayer.Name,
				NewPlayerId = newPlayer.PlayerId,
				Position = position,
			};
			SendMessage(msg, address);
		}

		public void SendMarkStarted(string address, Player player)
		{
			var msg = new MarkStartedMessage {
				PlayerId = player.PlayerId,
				Hand = string.Join(",", player.Hand.Select(h => h.ToString()).ToArray()),
			};
			SendMessage(msg, address);
		}

		public void SendGetBid(string address, int whoseTurn, int minBid, int maxBid, bool canPass)
		{
			var msg = new GetBidMessage {
				WhoseTurn = whoseTurn,
				MinBid = minBid,
				MaxBid = maxBid,
				CanPass = canPass,
			};
			SendMessage(msg, address);
		}

		public void SendPlayMade(string address, int whoseTurn, string dots)
		{
			var msg = new PlayMadeMessage
			{
				WhoseTurn = whoseTurn,
				Dots = dots,
			};
			SendMessage(msg, address);
		}

		public void SendBidwon(string networkAddress, int bidWinner, string bidWinnerName, Bid winningBid)
		{
			var msg = new BidWonMessage {
				BidWinner = bidWinner,
				BidWinnerName = bidWinnerName,
				BidAmount = winningBid.Amount,
				IsLow = winningBid.IsLow,
				Trump = (int) winningBid.Trump,
				TrumpName = Enum.GetName(typeof (SuitEnum), winningBid.Trump),
			};
			SendMessage(msg, networkAddress);
		}

		public void SendGetPlay(string networkAddress, int whoseTurn)
		{
			var msg = new GetPlayMessage {
				WhoseTurn = whoseTurn,
			};
			SendMessage(msg, networkAddress);
		}

		public void SendTrickWon(string networkAddress, int winningSeat, Trick trick)
		{
			var msg = new TrickWonMessage {
				TrickWinner = winningSeat,
				Trick = string.Join(",", trick.Where(t => t != null).Select(t => t.ToString())),
			};
			SendMessage(msg, networkAddress);
		}

		public void SendMarkWon(string networkAddress, int ourMarks, int theirMarks, bool youWon, int marksBet, int bidWinner)
		{
			var msg = new MarkWonMessage
			{
				OurMarks = ourMarks,
				TheirMarks = theirMarks,
				YouWon = youWon,
				MarksBet = marksBet,
				BidWinner = bidWinner,
			};
			SendMessage(msg, networkAddress);
		}

		public void SendGameWon(string networkAddress, bool youWon)
		{
			var msg = new GameWonMessage {
				YouWon = youWon,
			};
			SendMessage(msg, networkAddress);
		}

		public void SendBidMade(string networkAddress, int whoseTurn, int amount)
		{
			var msg = new BidMadeMessage {
				WhoseTurn = whoseTurn,
				BidAmount = amount,
			};
			SendMessage(msg, networkAddress);
		}

		public void SendServerErrorMessage(string networkAddress, Exception exception)
		{
			var msg = new ServerErrorMessage()
			{
				Message = "An error occurred on the game server.  Please contact the site adiministrator if you experience problems. (" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + ")",
			};
			SendMessage(msg, networkAddress);
		}
	}

}