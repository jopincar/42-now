using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using FortyTwoLib.Logging;

namespace FortyTwoLib
{
	public class Server
	{
		private static readonly Server _instance = new Server();

		private List<Table> _tables;
		private List<Player> _players;
		public Log Log { get; private set; }
		public Random _rand = new Random();
		private Timer _timer;
		private Timer _windDown;

		private Server()
		{
			Log = new Log();
			Log.AddSink(new FileLogSink(AppConfig.GetSetting("LogFile", @"logs\forty-two.log"), TraceEventType.Information));
			Log.WriteInfo("Server constructor.");
			Reset();
			_timer = new Timer(2000) {Enabled = true};
			_timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
			_windDown = new Timer(20000) { Enabled = true };
			_windDown.Elapsed += new ElapsedEventHandler(_windDown_Elapsed);
		}

		private void _windDown_Elapsed(object sender, ElapsedEventArgs e)
		{
			var zombies = _tables.Where(t => t.ZombieTime > 5 
				|| (t.GameStartTime.HasValue 
					&& !t.GameEndTime.HasValue 
					&& (DateTime.Now - t.LastStateChange).TotalMinutes > 25) 
			).ToList();
			foreach ( var table in zombies)
			{
				_tables.Remove(table);
			}
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			var newTables = _tables.Where(t => t.GameStartTime == null).ToList();
			foreach (var table in newTables)
			{
				FinishTestTableForPlayer(table);
			}
		}

		public static Server GetInstance()
		{
			return _instance;
		}

		public FindPlayerResult FindPlayer(int playerId)
		{
			var ret = new FindPlayerResult();
			ret.Table = GetTableByPlayerId(playerId, false);
			if ( ret.Table != null )
			{
				ret.Player = ret.Table.GetPlayers().Single(p => p.PlayerId == playerId);
			} else
			{
				ret.Player = _players.FirstOrDefault(p => p.PlayerId == playerId);
			}
			return ret;
		}

		public Table GetTableByPlayerId(int playerId, bool? gameOver)
		{
			var ret = _tables.SingleOrDefault(t => (gameOver.HasValue ? t.GameOver == gameOver : true) 
				&& t.GetPlayers().Any(p => p.PlayerId == playerId));
			if ( ret == null && gameOver.HasValue && !gameOver.Value )
			{
				ret = _tables.SingleOrDefault(t => t.GameOver
					&& t.GetPlayers().Any(p => p.PlayerId == playerId));
			}
			return ret;
		}

		public Table CreateTestTableForPlayer(int playerId, string playerName, string hand)
		{
			var table = new Table(Log)
			{
				Name = "Your Table",
				TableId = playerId,
				FixedHand = hand,
			};
			table.AddPlayer(new Player(Log)
			{
				Name = playerName,
				PlayerId = playerId,
				NetworkAddress = playerId.ToString(),
			}, 0);
			_tables.Add(table);
			return table;
		}

		private void FinishTestTableForPlayer(Table table)
		{
			int playerId = table.GetPlayers().First(p => p != null).PlayerId;
			table.AddPlayer(new BotPlayer(Log)
			{
				Name = "Monica",
				PlayerId = Math.Abs(playerId) * -2,
				NetworkAddress = (Math.Abs(playerId) * -2).ToString(),
			}, 1);
			table.AddPlayer(new BotPlayer(Log)
			{
				Name = "Alex",
				PlayerId = Math.Abs(playerId) * -3,
				NetworkAddress = (Math.Abs(playerId) * -3).ToString(),
			}, 2);
			table.AddPlayer(new BotPlayer(Log)
			{
				Name = "Lila",
				PlayerId = Math.Abs(playerId) * -4,
				NetworkAddress = (Math.Abs(playerId) * -4).ToString(),
			}, 3);

			table.StartMark();
			//table.QuickStart(playerId);
		}

		public void Reset()
		{
			Log.WriteInfo("Server reset.");
			_tables = new List<Table>();
			_players = new List<Player>();
		}

		public void RemoveTable(Table table, bool immediateRemove)
		{
			table.GetPlayers().ForEach(p => _players.Add(p));
			if ( immediateRemove ) _tables.Remove(table);
		}

		public List<Table> Tables
		{
			get { return _tables; }
		}
	}

	public class FindPlayerResult
	{
		public Player Player { get; set; }
		public Table Table { get; set; }
	}
}
