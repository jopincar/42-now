using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FortyTwoLib.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FortyTwoLib.Test
{
	[TestClass]
	public class PlayCalcTest
	{
		private Log _log;
		private Table _table;

		[TestInitialize]
		public void Init()
		{
			_log = new Log();
			var sink = new TraceLogSink();
			sink.MinSeverityToLog = System.Diagnostics.TraceEventType.Verbose;
			_log.AddSink(sink);
			_table = CreateTestTable();
		}

		private Table CreateTestTable()
		{
			var table = new Table(_log)
			{
				Name = "John's Table",
				TableId = 1,
			};
			table.AddPlayer(new Player(_log)
			{
				Name = "John",
				PlayerId = 0,
				NetworkAddress = "0",
			}, 0);

			table.AddPlayer(new Player(_log)
			{
				Name = "Monica",
				PlayerId = 1,
				NetworkAddress = 1.ToString(),
			}, 1);
			table.AddPlayer(new Player(_log)
			{
				Name = "Alex",
				PlayerId = 2,
				NetworkAddress = 2.ToString(),
			}, 2);
			table.AddPlayer(new Player(_log)
			{
				Name = "Lila",
				PlayerId = 3,
				NetworkAddress = 3.ToString(),
			}, 3);
			return table;
		}

		[TestMethod]
		public void BidWinnersPartnerKeepsLeadingDoubles()
		{
			var bonePile = new BonePile(new[] {
				"53,65,41,22,32,66,44", 
				"42,60,40,50,10,30,54", 
				"52,51,61,20,63,11,31", 
				"64,21,00,43,33,62,55"
			});
			_table.StartMark(1, bonePile);
			foreach (int playerId in new[] {2, 3, 0} ) _table.GotBid(playerId, new Bid {Amount = 0});
			_table.GotBid(1, new Bid {Amount = 30, IsLow = false, Trump = SuitEnum.blanks});
			PlayDominos(1, "10,20,00,53");
			PlayDominos(3, "55,65,54,52");
			var played = new PlayCalc().GetPlay(_table.GetPlayerBySeat(0, 3), _table, _log);
			Assert.AreEqual(new Domino("33"), played);
		}

		[TestMethod]
		public void DontWasteDoubles()
		{
			var bonePile = new BonePile(new string[] {
				"20,53,65,11,31,40,64",
				"22,60,66,63,00,62,42",
				"55,61,44,33,51,41,50",
				"43,10,52,30,21,32,54"
			});
			_table.StartMark(1, bonePile);
			foreach (int playerId in new[] { 2, 3, 0 }) _table.GotBid(playerId, new Bid { Amount = 0 });
			_table.GotBid(1, new Bid { Amount = 36, IsLow = false, Trump = SuitEnum.twos });
			PlayDominos(1, "22,61,32,20");
			_table.PlayDomino(1, "62");
			var played = new PlayCalc().GetPlay(_table.GetPlayerBySeat(0, 2), _table, _log);
			Assert.AreEqual(new Domino("51"), played);
		}

		[TestMethod]
		public void DontLeadOffsBackToBidWinningPartnerWithLowBid()
		{
			
			var bonePile = new BonePile(new string[] {
				"61,51,31,52,10,33,00",
				"40,22,44,42,63,66,32",
				"20,50,54,30,43,11,53",
				"62,64,21,60,55,41,65"
			});

			_table.StartMark(0, bonePile);
			foreach (int playerId in new[] { 1, 2, 3 }) _table.GotBid(playerId, new Bid { Amount = 0 });
			_table.GotBid(0, new Bid { Amount = 30, IsLow = false, Trump = SuitEnum.ones });
			PlayDominos(0, "10,40,11,12");
			var played = new PlayCalc().GetPlay(_table.GetPlayerBySeat(0, 2), _table, _log);
			_log.WriteInfo("Played " + played.ToString());
			Assert.AreNotEqual(new Domino("54"), played);
		}

		[TestMethod]
		public void DontLead05WhenPartnerWonBid()
		{
			var bonePile = new BonePile(new string[] {
				"11 40 00 52 41 42 44",
				"62 60 32 20 10 53 66",
				"54 64 43 50 61 51 22",
				"65 31 33 63 30 21 55"
			});
			_table.StartMark(0, bonePile);
			foreach (int playerId in new[] { 1, 2, 3 }) _table.GotBid(playerId, new Bid { Amount = 0 });
			_table.GotBid(0, new Bid { Amount = 31, IsLow = false, Trump = SuitEnum.fours });
			PlayDominos(0, "44 10 64 31");
			PlayDominos(0, "40 60 54 30");
			var played = new PlayCalc().GetPlay(_table.GetPlayerBySeat(0, 2), _table, _log);
			_log.WriteInfo("Played " + played.ToString());
			Assert.AreNotEqual(new Domino("50"), played);
		}

		[TestMethod]
		public void DontLead64WhenYouHaveDoubles()
		{
			var bonePile = new BonePile(new string[] {
				"20 41 44 00 53 22 32",
				"43 54 51 33 66 10 30",
				"11 64 40 55 52 21 62",
				"42 50 31 61 63 65 60"
			});
			_table.StartMark(0, bonePile);
			foreach (int playerId in new[] { 1, 2, 3 }) _table.GotBid(playerId, new Bid { Amount = 0 });
			_table.GotBid(0, new Bid { Amount = 30, IsLow = false, Trump = SuitEnum.twos });
			PlayDominos(0, "22 10 21 42");
			PlayDominos(0, "20 51 52 61");
			PlayDominos(2, "55 65 53 54");
			var played = new PlayCalc().GetPlay(_table.GetPlayerBySeat(0, 2), _table, _log);
			_log.WriteInfo("Played " + played.ToString());
			Assert.AreEqual(new Domino("11"), played);
		}

		[TestMethod]
		public void DontLead64WhenYouHaveDoubles2()
		{
			var bonePile = new BonePile(new string[] {
				"20 41 44 00 53 22 32",
				"43 54 51 33 66 10 30",
				"11 64 40 55 52 21 62",
				"42 50 31 61 63 65 60"
			});
			_table.StartMark(0, bonePile);
			foreach (int playerId in new[] { 1, 2, 3 }) _table.GotBid(playerId, new Bid { Amount = 0 });
			_table.GotBid(0, new Bid { Amount = 30, IsLow = false, Trump = SuitEnum.twos });
			PlayDominos(0, "22 10 21 42");
			PlayDominos(0, "20 51 52 61");
			PlayDominos(2, "55 65 53 54");
			PlayDominos(2, "11 31 41 30");
			var played = new PlayCalc().GetPlay(_table.GetPlayerBySeat(0, 2), _table, _log);
			_log.WriteInfo("Played " + played.ToString());
			Assert.AreEqual(new Domino("40"), played);
		}

		[TestMethod]
		public void DontLead14WhenYouHaveDoubles2()
		{
			var bonePile = new BonePile(new string[] {
				"21 42 61 20 22 00 50",
				"30 44 63 52 11 64 41",
				"32 55 31 43 53 51 62",
				"40 33 60 10 66 65 54"
			});
			_table.StartMark(3, bonePile);
			foreach (int playerId in new[] { 0, 1, 2 }) _table.GotBid(playerId, new Bid { Amount = 0 });
			_table.GotBid(3, new Bid { Amount = 30, IsLow = false, Trump = SuitEnum.sixes });
			PlayDominos(3, "66 61 64 62");
			PlayDominos(3, "65 20 63 31");
			PlayDominos(3, "33 21 30 53");
			PlayDominos(3, "40 42 44 43");
			var played = new PlayCalc().GetPlay(_table.GetPlayerBySeat(0, 1), _table, _log);
			_log.WriteInfo("Played " + played.ToString());
			// Very tricky -- 41 is 100% winner because either your partner has 54 or he can trump and neither opponent has trump.
			// However, 11 is also 100% winner.  Should we play 11 or 41????
			Assert.AreEqual(new Domino("41"), played); 
		}

		[TestMethod]
		public void PlayCountToSet()
		{
			var bonePile = new BonePile(new string[] {
				"10 55 61 52 50 40 41",
				"60 62 51 43 44 63 31",
				"33 30 21 00 65 32 20",
				"22 42 53 54 66 11 64"			
			});
			_table.StartMark(3, bonePile);
			foreach (int playerId in new[] { 0, 1, 2 }) _table.GotBid(playerId, new Bid { Amount = 0 });
			_table.GotBid(3, new Bid { Amount = 30, IsLow = false, Trump = SuitEnum.fours });
			PlayDominos(3, "42 41 44 65");
			PlayDominos(1, "43 30 64 40");
			PlayDominos(3, "22 52 62 21");
			PlayDominos(3, "66 61 60 20");
			PlayDominos(3, "11 10 51 00");
			_table.PlayDomino(3, "53");
			_table.PlayDomino(0, "55");
			_table.PlayDomino(1, "63");

			var played = new PlayCalc().GetPlay(_table.GetPlayerBySeat(0, 2), _table, _log);
			_log.WriteInfo("Played " + played.ToString());
			// Very tricky -- 41 is 100% winner because either your partner has 54 or he can trump and neither opponent has trump.
			// However, 11 is also 100% winner.  Should we play 11 or 41????
			Assert.AreEqual(new Domino("32"), played);
		}

		private void PlayDominos(int playerId, string dotsList)
		{
			foreach ( var s in dotsList.Split(new[] {',', ' '}))
			{
				_table.PlayDomino(playerId, s);
				playerId = (playerId + 1) % 4;
			}
		}
	}

}
