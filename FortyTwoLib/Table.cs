using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FortyTwoLib.Database;
using FortyTwoLib.Logging;

namespace FortyTwoLib
{
	public enum TableStateEnum
	{
		Created, AddingPlayers,
		WaitingForBid,
		GameStarted,
		WaitingForPlay,
		GameWon,
		TableRemoved,
		BidWon,
		GotBid,
		DominoPlayed,
		TrickWon,
		MarkWon
	}

	[Serializable]
	public class Table
	{
		private const int SEAT_COUNT = 4;
		private const int SHUFFLE_COUNT = 500;
		private const int MARKS_TO_WIN = 7;

		public int TableId { get; set; }
		public int TableDbId { get; set; }
		public string Name { get; set; }
		public bool IsSingleHand { get; set; }

		public ILog Log { get; private set; }

		public DateTime LastStateChange { get; protected set; }
		public List<TableStateEnum> _stateHistory;
		public TableStateEnum CurrentState
		{
			get { return _stateHistory[_stateHistory.Count - 1]; }
			set
			{
				if ( _stateHistory == null )
				{
					_stateHistory = new List<TableStateEnum> {TableStateEnum.Created};
				}
				LastStateChange = DateTime.Now;
				_stateHistory.Add(value);
			}
		}

		protected List<Player> _players = new List<Player>() { null, null, null, null };
		private List<Team> _teams = new List<Team> { new Team(), new Team()};
		public List<Trick> Tricks { get; protected set; }
		private int _shuffler;
		private List<Domino> _played = new List<Domino>() {null, null, null, null};
		private readonly Random _rand = new Random();
		private int _whoseTurn;
		private int _bidCount;
		private int _bidWinner;
		public int HandCount { get; private set; }
		public DateTime? GameEndTime { get; private set; }
		public DateTime? GameStartTime { get; private set; }
		private bool[,] _hasNo = new bool[4,8];
		private List<Domino> _allPlayed;
		public int HintCount { get; set; }
		public DateTime? FirstHintDate { get; set; }
		public string FixedHand { get; set; }

		private int _maxBid;
		private int _minBid;
		private bool _canPass;
		private int _leader;
		private SuitCount _suitCount;

		public DateTime? EndTime
		{
			get { return GameEndTime; }
		}

		public int OutSeat
		{
			get
			{
				if ( !WinningBid.IsLow ) throw new InvalidOperationException("Bid winner did not go low.");
				return (_bidWinner + 2) % SEAT_COUNT;
			}
		}

		public int Leader
		{
			get { return _leader; }
		}


		public Table(ILog log)
		{
			Log = log;
			_shuffler = _rand.Next(4);
			_bidWinner = -1;
			_whoseTurn = -1;
			_maxBid = 84;
			_minBid = 30;
			_canPass = true;
		}

		/// <summary>
		/// Returns only non-null (seated) players
		/// </summary>
		public List<Player> GetPlayers()
		{
			return _players.Where(p => p != null).ToList();
		}

		public int PlayCount
		{
			get
			{
				int ret = _played.Count(d => d != null);
				return ret + (WinningBid.IsLow && ret > 0 ? 1 : 0); // pretend we got a play from the person that's sitting out
			}
		}

		public int TrickWinner()
		{
			var trickWinner = _leader;
			for (int i = 1; i < SEAT_COUNT; i++)
			{
				int whoseTurn = (_leader + i) % SEAT_COUNT;
				if ( LowPartner(whoseTurn) ) continue;
				if (_played[whoseTurn] != null && _played[whoseTurn].GreaterThan(_played[trickWinner], LeadPlay, Trump, WinningBid.IsLow))
				{
					trickWinner = whoseTurn;
				}
			}
			return trickWinner;
		}

		public List<Team> GetTeamBySeat()
		{
			return _teams;
		}

		public int GetSeatByPlayerId(int playerId)
		{
			int seat = _players.FindIndex(p => p != null && p.PlayerId == playerId);
			if ( seat < 0 ) throw new ArgumentOutOfRangeException("playerId", string.Format("PlayerId {0} was not found at table {1}.", playerId, TableId));
			return seat;
		}

		public Player GetPlayerBySeat(int refPlayerId, int position)
		{
			return _players[(position + GetSeatByPlayerId(refPlayerId)) % SEAT_COUNT];
		}

		public string GetPlayedDotsBySeat(int refPlayerId, int position)
		{
			var d = _played[(position + GetSeatByPlayerId(refPlayerId)) % SEAT_COUNT];
			if ( d == null ) return "  ";
			return d.ToString();
		}

		public SuitCount SuitCount
		{
			get { return _suitCount; }
		}

		public SuitEnum LeadSuit
		{
			get { return LeadPlay.IsA(Trump) ? (SuitEnum) Trump : (SuitEnum) LeadPlay.Hi(Trump); }
		}

		public bool GameOver
		{
			get { return GameEndTime.HasValue;  }
		}

		public Player BidWinner()
		{
			return _bidWinner < 0 ? null : _players[_bidWinner];
		}

		public void SetBidWinner(int seat)
		{
			_bidWinner = seat;
		}

		public int WhoseTurn
		{
			get { return _whoseTurn;  }
			set { _whoseTurn = value; }
		}

		public int LocalSeat(int refPlayerId, int seat)
		{
			return (seat + (SEAT_COUNT - GetSeatByPlayerId(refPlayerId))) % SEAT_COUNT;
		}

		public Domino LeadPlay
		{
			get { return _played[_leader]; }
		}

		public int Trump
		{
			get { return (int)WinningBid.Trump; }
		}

		private void ClearPlayed()
		{
			_played = new List<Domino>() { null, null, null, null };
		}

		public Bid WinningBid
		{
			get { return _players[_bidWinner].Bid; }
		}

		public Team GetTeamByPlayerId(int playerId)
		{
			return _teams[GetSeatByPlayerId(playerId) % 2];
		}

		public Team GetOpponentsByPlayerId(int playerId)
		{
			return _teams[(GetSeatByPlayerId(playerId) + 1) % 2];
		}

		public int MinBid { get { return _minBid;  } }

		public int MaxBid { get { return _maxBid; } }

		public bool CanPass { get { return _canPass; } }

		public int BidCount { get { return _bidCount;  } }

		public void AddPlayer(Player player, int position)
		{
			if( player == null ) throw new ArgumentNullException("player");
			if ( position < 0 || position > 3 ) throw new ArgumentOutOfRangeException("position", "From 0 to 3");
			if ( _players[position] != null ) throw new ArgumentException("Already occupied", "position");
			_players[position] = player;
			CurrentState = GetPlayers().Count < 4 ? TableStateEnum.AddingPlayers : TableStateEnum.GameStarted;
			GetPlayers().ForEach(p => p.TableAddedPlayer(this, player, position));
		}

		public void StartMark()
		{
			StartMark(null, null);
		}

		protected void WriteInfo(string message)
		{
			//Log.WriteInfo("T" + TableId + ": " + message);
		}

		public void StartMark(int? shuffler, BonePile bonePile)
		{
			HandCount++;
			if ( GameStartTime == null )
			{
				GameStartTime = DateTime.Now;
				_players.ForEach(p => {
					p.MarksWon = 0; 
					p.MarksLost = 0;
				});
			}
			_teams.ForEach(t => t.StartMark());
			Tricks = new List<Trick>(7);
			ClearPlayed();

			if ( FixedHand != null )
			{
				_shuffler = 0;
			}
			else if ( shuffler == null )
			{
				_shuffler = (_shuffler + 1) % 4;
			} else
			{
				_shuffler = shuffler.Value;
			}
			WriteInfo(string.Format("{0} shakes", _players[_shuffler].PlayerId));

			if ( bonePile == null ) bonePile = new BonePile(SHUFFLE_COUNT, _rand, FixedHand);

			for (int p = 0; p < 4; p++)
			{
				WriteInfo(string.Format("{0} has {1}", _players[p].PlayerId, bonePile.GetHandForSeat(p)));
				_players[p].StartMark(bonePile.GetHandForSeat(p));
			}

			_whoseTurn = (_shuffler + 1) % 4;
			_bidCount = 0;
			_bidWinner = -1;

			for ( int i = 0; i < SEAT_COUNT; i++)
			{
				for ( int j = 0; j < 8; j++ ) _hasNo[i, j] = false;
			}
			_allPlayed = new List<Domino>();

			GetBid();
		}

		public void QuickStart(int playerId)
		{
			_bidCount = 4;
			_bidWinner = GetSeatByPlayerId(playerId);
			_leader = _bidWinner;
			_players[_bidWinner].Bid = new Bid {Amount = 30, Trump = SuitEnum.followMe};
			_whoseTurn = _bidWinner;
			_players.ForEach(p => p.Hand.SetTrump(WinningBid.Trump));
			_players.ForEach(p => p.BidWon(_bidWinner, _players[_bidWinner]));
			GetPlay();
		}

		public void GetBid()
		{
			_maxBid = 84;
			_minBid = 30;
			bool anyBid= false;
			int bids = 1;
			while ( bids <= _bidCount )
			{
				int i = (_shuffler + bids) % 4;
				anyBid = anyBid || _players[i].Bid.Amount != 0;
				if( _players[i].Bid.Amount == 0 )
				{
					_minBid = Math.Max(_minBid, 30);
				} else if( _players[i].Bid.Amount >= 30 && _players[i].Bid.Amount <= 41 )
				{
					_minBid = Math.Max(_players[i].Bid.Amount + 1, _minBid);
				} else
				{
					_minBid = Math.Max(_minBid, _players[i].Bid.Amount + 42);
				}
				bids++;
			}
			if ( _minBid >= 30 && _minBid <= 41 )
			{
				_maxBid = _bidCount == 0 ? 84 : 42;
			} else
			{
				_maxBid = _minBid + 42;
			}
			_canPass = _bidCount < 3 || anyBid;

			//WriteInfo(string.Format("{0}'s bid", _players[_whoseTurn].PlayerId));
			CurrentState = TableStateEnum.WaitingForBid;
			_players.ForEach(p => p.GetBid(LocalSeat(p.PlayerId, _whoseTurn), _minBid, _maxBid, _canPass));
		}

		public string GotBid(int playerId, Bid bid)
		{
			CurrentState = TableStateEnum.GotBid;
			if ( FixedHand != null && GetSeatByPlayerId(playerId) != 0 )
			{
				bid.Amount = 0;
			}

			WriteInfo(string.Format("{0} bid = {1} {2} {3}", playerId, bid.Amount, bid.Trump, bid.IsLow));

			if ( !Bidding() ) return "Not bidding.";

			if ( GetSeatByPlayerId(playerId) != _whoseTurn )
			{
				return "It's not your turn to bid.";
			}

			if ( bid.Amount == 0 && !CanPass ) return "You can't pass.";

			if ( bid.Amount > 0 )
			{

				if ( bid.Trump == SuitEnum.none ) return "You must specify a trump suit.  FollowMe means no trump.";

				if ( bid.IsLow && !bid.GetvalidSuits().Contains(bid.Trump) ) return "Low bids must be doubles or follow me";

				if ( (bid.Amount < MinBid || bid.Amount > MaxBid) ) return string.Format("You must bid between {0} and {1}", MinBid, MaxBid);

				if ( bid.IsLow && bid.Amount % 42 != 0) return "To play low your bid must be at least 42";
			}
			bid.BidPosition = _bidCount;

			_players[_whoseTurn].Bid = bid;

			_players.ForEach(p => p.BidMade(LocalSeat(p.PlayerId, _whoseTurn), bid.Amount));
			_bidCount++;
			if (_bidCount < SEAT_COUNT)
			{
				_whoseTurn = (_whoseTurn + 1) % SEAT_COUNT;
				GetBid();
				return "";
			}

			BidWon();

			return "";
		}

		private void BidWon()
		{
			CurrentState = TableStateEnum.BidWon;
			_bidWinner = 0;
			for ( int i = 1; i < SEAT_COUNT; i++ )
			{
				if ( _players[i].Bid.Amount > _players[_bidWinner].Bid.Amount )
				{
					_bidWinner = i;
				}
			}
			_leader = _bidWinner;
			_whoseTurn = _bidWinner;

			WriteInfo(string.Format("{0} won bid", _players[_bidWinner].PlayerId));
			_players.ForEach(p => p.Hand.SetTrump(WinningBid.Trump));
			_players.ForEach(p => p.BidWon(_bidWinner, _players[_bidWinner]));

			_suitCount = new SuitCount(WinningBid.Trump);

			GetPlay();
		}

		private void GetPlay()
		{
			CurrentState = TableStateEnum.WaitingForPlay;
			foreach (var p in _players)
			{
				int seat = LocalSeat(p.PlayerId, _whoseTurn);
				p.GetPlay(seat);
			}
		}

		public string PlayDomino(int playerId, string dots)
		{
			CurrentState = TableStateEnum.DominoPlayed;
			WriteInfo(string.Format("{0} {1}", playerId, dots));
			if ( Bidding() ) return "Waiting for bids.";

			var player = _players.Single(p => p.PlayerId == playerId);
			int seat = GetSeatByPlayerId(playerId);
			if (  seat != _whoseTurn  ) return string.Format("{0} played out of turn.", player.Name);

			var result = player.Hand.CanPlay(dots, _played[_leader], (int) WinningBid.Trump);
			if ( result != "" ) return result;

			player.Hand.Remove(dots);
			var played = new Domino(dots);
			played = new Domino(played.HiLo(LeadPlay ?? played, (int) WinningBid.Trump));
			_played[seat] = played;
			_players.ForEach(p => p.PlayMade(LocalSeat(p.PlayerId, _whoseTurn), played.ToString()));

			_allPlayed.Add(played);
			if ( played.IsA(Trump) )
			{
				_suitCount[Trump]--;
			} else
			{
				_suitCount[played.Hi(Trump)]--;
				if ( !played.IsA(SuitEnum.doubles) ) _suitCount[played.Lo(Trump)]--;
			}

			if ( _whoseTurn != _bidWinner )
			{
				if ( !LastPlay(_whoseTurn).IsA(LeadSuit) || 
					(Trump == (int) SuitEnum.doubles && LastPlay(_whoseTurn).IsA(SuitEnum.doubles) && LeadSuit != SuitEnum.doubles) )
				{
    				_hasNo[_whoseTurn, (int) LeadSuit] = true;
				}
			}

			_whoseTurn = (_whoseTurn + 1) % SEAT_COUNT;
			if ( LowPartner(_whoseTurn) )
			{
				_whoseTurn = (_whoseTurn + 1) % SEAT_COUNT;
			}

			if ( _played.Count(d => d != null) < MaxPlayCount() )
			{
				GetPlay();
				return ""; // Need more dominos to finish trick
			}

			TrickWon();

			return "";
		}

		public List<Domino> Played
		{
			get { return _allPlayed;  }
		}

		public int TrickCount
		{
			get { return _teams.Sum(t => t.Tricks.Count()); }
		}

		public double ZombieTime
		{
			get
			{
				if ( !GameEndTime.HasValue ) return 0;
				return (DateTime.Now - GameEndTime.Value).TotalSeconds;
			}
		}

		public string PlayerNames
		{
			get { 
				var ret = new StringBuilder();
				_players.ForEach(p => ret.Append(p == null || p.Name == null ? "<null>" : p.Name + ", "));
				ret.Remove(ret.Length - 2, 2);
				return ret.ToString();
			}
		}

		private int MaxPlayCount()
		{
			return SEAT_COUNT - (WinningBid.IsLow ? 1 : 0);
		}

		private bool LowPartner(int seat)
		{
			bool ret = (((seat + 2) % SEAT_COUNT) == _bidWinner) && WinningBid.IsLow;
			return ret;
		}

		public bool HasNo(int seat, int suit)
		{
			if ( suit == (int) SuitEnum.followMe ) return true;
			return _hasNo[seat, suit];
		}

		private void TrickWon()
		{
			CurrentState = TableStateEnum.TrickWon;
			var trickWinner = _leader;
			var dominos = new Domino[4];
			dominos[0] = _played[_leader];
			int winner = 0;
			for ( int i = 1; i < SEAT_COUNT; i++ )
			{
				int whoseTurn = (_leader + i) % SEAT_COUNT;
				if ( LowPartner(whoseTurn) ) continue;
				dominos[i] = _played[whoseTurn];
				if ( _played[whoseTurn].GreaterThan(_played[trickWinner], LeadPlay, Trump, WinningBid.IsLow) )
				{
					trickWinner = whoseTurn;
					winner = i;
				}
			}
			WriteInfo(string.Format("{0} won trick", _players[trickWinner].PlayerId));
			var trick = new Trick(dominos[0], dominos[1], dominos[2], dominos[3], winner, _leader);
			_teams[trickWinner % 2].Tricks.Add(trick);
			_players.ForEach(p => p.TrickWon(LocalSeat(p.PlayerId, trickWinner), trick));

			Tricks.Add(new Trick(_played[0], _played[1], _played[2], _played[3], winner, _leader));
			ClearPlayed();

			bool markWon = false;
			int markWinner;
			if ( !WinningBid.IsLow )
			{
				markWon = trickWinner % 2 == _bidWinner % 2
					? _teams[trickWinner % 2].CountValue() >= WinningBid.MakeBidAmount
					: _teams[trickWinner % 2].CountValue() > 42 - WinningBid.MakeBidAmount;
				markWinner = trickWinner % 2;
			} else
			{
				if ( trickWinner == _bidWinner )
				{
					markWon = true;
				} else if ( _teams[trickWinner % 2].Tricks.Count == 7 ) 
				{
					markWon = true;
				}
				markWinner = (trickWinner + 1) % 2;
			}

			if ( !markWon )
			{
				_leader = trickWinner;
				_whoseTurn = trickWinner;
				GetPlay();
				return;
			}

			MarkWon(markWinner);
		}

		public int Team0Marks
		{
			get { return _teams[0].Marks; }
		}

		public int Team1Marks
		{
			get { return _teams[1].Marks; }
		}

		private void MarkWon(int markWinner)
		{
			CurrentState = TableStateEnum.MarkWon;
			int marksBet = WinningBid.Amount <= 42 ? 1 : WinningBid.Amount / 42;
			_teams[markWinner].Marks += marksBet;
			if ( _bidWinner % 2 == markWinner  )
			{
				_players[_bidWinner].MarksWon += marksBet;
				if ( WinningBid.IsLow )
				{
					_players[_bidWinner].LowMarksWon += marksBet;
				} else
				{
					_players[_bidWinner].BidsMadeSum += WinningBid.Amount;
				}
			} else
			{
				_players[_bidWinner].MarksLost += marksBet;
				if (WinningBid.IsLow)
				{
					_players[_bidWinner].LowMarksLost += marksBet;
				} else
				{
					_players[_bidWinner].BidsSetSum += WinningBid.Amount;
				}
			}
			string winningTeam = _players[markWinner * 2].PlayerId + "/" + _players[(markWinner * 2) + 1].PlayerId;
			WriteInfo(string.Format("{0} won {1} mark", winningTeam, marksBet));
			_players.ForEach(p => p.MarkWon(_teams[GetSeatByPlayerId(p.PlayerId) % 2].Marks, 
				_teams[(GetSeatByPlayerId(p.PlayerId) + 1) % 2].Marks, 
				GetSeatByPlayerId(p.PlayerId) % 2 == markWinner,
				marksBet, LocalSeat(p.PlayerId, _bidWinner)
			));

			if (FixedHand == null)
			{
				if (_teams[markWinner].Marks >= MARKS_TO_WIN) GameEndTime = DateTime.Now;
				new DataAccess().SaveGame(this, markWinner, _bidWinner, _teams.Sum(t => t.Tricks.Count));
			}

			if (_teams[markWinner].Marks < MARKS_TO_WIN || FixedHand != null)
			{
				StartMark();
				return;
			}

			CurrentState = TableStateEnum.GameWon;
			WriteInfo(string.Format("{0} won game", winningTeam));

			_players.ForEach(p => p.GameWon(GetSeatByPlayerId(p.PlayerId) % 2 == markWinner));

			Server.GetInstance().RemoveTable(this, false);
		}

		public bool Bidding()
		{
			return _bidWinner < 0;
		}

		public int OppLeftSeat(int refPlayerId)
		{
			return (GetSeatByPlayerId(refPlayerId) + 1) % SEAT_COUNT;
		}

		public Player OppLeft(int refPlayerId)
		{
			return _players[OppLeftSeat(refPlayerId)];
		}

		public int PartnerSeat(int refPlayerId)
		{
			return (GetSeatByPlayerId(refPlayerId) + 2) % SEAT_COUNT;
		}

		public Player Partner(int refPlayerId)
		{
			return _players[PartnerSeat(refPlayerId)];
		}

		public int OppRightSeat(int refPlayerId)
		{
			return (GetSeatByPlayerId(refPlayerId) + 3) % SEAT_COUNT;
		}

		public Player OppRight(int refPlayerId)
		{
			return _players[OppRightSeat(refPlayerId)];
		}

		public Domino WinningPlay()
		{
			return _played[TrickWinner()];
		}

		public Domino LastPlay(int seat)
		{
			return _played[seat];
		}

		public int CountSoFar()
		{
			return _played.Where(d => d != null).Sum(d => d.CountValue());
		}

		public void OnException(Exception ex)
		{
			GetPlayers().ForEach(p => p.OnException(ex));
		}

		public bool AllowHints()
		{
			return true;
		}

		public void GotHint()
		{
			HintCount++;
			FirstHintDate = FirstHintDate ?? DateTime.Now;
		}

		public string DisplayState()
		{
			return string.Format("ID {0}; DBID {1}; PID {2}; State: {3}; Turn {4}; Start {5:MM/dd/yy HH:mm}; End: {6:MM/dd/yy HH:mm}; T0: {7}; T1: {8}; History: {9}",
				TableId, TableDbId, _players[0].PlayerId, CurrentState, WhoseTurn, GameStartTime, GameEndTime, Team0Marks, Team1Marks, GetHistory());
		}

		private string GetHistory()
		{
			var ret = new StringBuilder();
			_stateHistory.ForEach(h => ret.Append(h + ", "));
			ret.Remove(ret.Length - 2, 2);
			return ret.ToString();
		}

		public bool AnyRealPlayers()
		{
			return GetPlayers().Any(p => p != null && p.PlayerId > 0);
		}
	}
}
