using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FortyTwoLib
{
	public enum MessageTypeEnum
	{
		TableAddedPlayer, TableCreated,
		MarkStarted, GetBid, BidMade, 
		GetPlay, PlayMade,
		BidWon,
		TrickWon,
		MarkWon,
		GameWon,
		ServerError
	};

	[Serializable]
	public abstract class Message
	{
		public readonly string MessageType;

		internal Message(MessageTypeEnum messageType)
		{
			MessageType = messageType.ToString();
		}
	}

	[Serializable]
	public class TableCreateMessage : Message
	{
		public TableCreateMessage() 
			: base(MessageTypeEnum.TableCreated)
		{}

		public int PlayerId { get; set; }
		public string PlayerName { get; set; }
		public int TableId { get; set; }
		public string TableName { get; set; }
	}

	[Serializable]
	public class TableAddPlayerMessage : Message
	{
		public TableAddPlayerMessage() 
			: base(MessageTypeEnum.TableAddedPlayer)
		{}

		public int TableId { get; set; }
		public string TableName { get; set; }
		public int NewPlayerId { get; set; }
		public string NewPlayerName { get; set; }
		public int Position { get; set; }
	}

	[Serializable]
	public class MarkStartedMessage : Message
	{
		public MarkStartedMessage() 
			: base(MessageTypeEnum.MarkStarted)
		{}

		public int PlayerId { get; set; }
		public string Hand { get; set; }
	}

	[Serializable]
	public class GetBidMessage : Message
	{
		public GetBidMessage()
			: base(MessageTypeEnum.GetBid)
		{ }

		public int WhoseTurn { get; set; }
		public int MinBid { get; set; }
		public int MaxBid { get; set; }
		public bool CanPass { get; set; }
	}

	[Serializable]
	public class BidWonMessage : Message
	{

		public BidWonMessage()
			: base(MessageTypeEnum.BidWon)
		{ }

		public int BidWinner { get; set; }
		public string BidWinnerName { get; set; }
		public int BidAmount { get; set; }
		public bool IsLow { get; set; }
		public int Trump { get; set; }
		public string TrumpName { get; set; }
	}

	[Serializable]
	public class GetPlayMessage : Message
	{
		public GetPlayMessage()
			: base(MessageTypeEnum.GetPlay)
		{ }

		public int WhoseTurn { get; set; }
	}

	[Serializable]
	public class PlayMadeMessage : Message
	{
		public PlayMadeMessage() 
			: base(MessageTypeEnum.PlayMade)
		{}

		public int WhoseTurn { get; set; }
		public string Dots { get; set; }
	}

	public class TrickWonMessage : Message
	{
		public TrickWonMessage()
			: base(MessageTypeEnum.TrickWon) 
		{}
		public int TrickWinner { get; set; }
		public string Trick { get; set; }
	}

	public class MarkWonMessage : Message
	{
		public MarkWonMessage()
			: base(MessageTypeEnum.MarkWon)
		{ }
		public int OurMarks { get; set; }
		public int TheirMarks { get; set; }
		public bool YouWon { get; set; }
		public int MarksBet { get; set; }
		public int BidWinner { get; set; }
	}

	public class GameWonMessage : Message
	{
		public GameWonMessage()
			: base(MessageTypeEnum.GameWon)
		{ }

		public bool YouWon { get; set; }
	}

	public class BidMadeMessage : Message
	{
		public BidMadeMessage() 
			: base(MessageTypeEnum.BidMade) 
		{}

		public int WhoseTurn { get; set; }
		public int BidAmount { get; set; }
	}

	public class ServerErrorMessage : Message
	{
		public ServerErrorMessage() 
			: base(MessageTypeEnum.ServerError) 
		{}

		public string Message { get; set; }
	}

}
