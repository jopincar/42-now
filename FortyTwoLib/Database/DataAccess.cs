using System;
using System.Data.Linq.SqlClient;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;
using FortyTwoLib.Entities;

namespace FortyTwoLib.Database
{
	public class DataAccess
	{
		protected TRet ExecuteWithNolock<TRet>(Func<FortyTwoDataContext, TRet> queryCode)
		{
			using ( new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {IsolationLevel = IsolationLevel.ReadUncommitted}) )
			{
				return ExecuteWithDb(queryCode);
			}
		}

		protected TRet ExecuteWithDb<TRet>(Func<FortyTwoDataContext, TRet> queryFunc)
		{
			using ( var db = new FortyTwoDataContext(AppConfig.GetDefaultConnectionString()) )
			{
				return queryFunc(db);
			}
		}

		public void SaveGame(Table table, int markWinner, int bidWinner, int tricksPlayed)
		{
			var players = table.GetPlayers();
			using ( var db = new FortyTwoDataContext(AppConfig.GetDefaultConnectionString()) )
			{
				Game game = null;
				if ( table.TableDbId > 0 )
				{
					game = db.Game.Single(g => g.GameId == table.TableDbId);
				} else
				{
					game = new Game {
						StartDate = table.GameStartTime.Value,
					};
					db.Game.InsertOnSubmit(game);
				}
				game.HintCount = table.HintCount;
				game.EndDate = table.GameEndTime;
				game.WinningTeam = markWinner;
				game.FirstHintDate = table.FirstHintDate;

				foreach ( var player in players )
				{
					GamePlayer gamePlayer = null;
					if (  player.GamePlayerDbId > 0 )
					{
						gamePlayer = db.GamePlayer.Single(gp => gp.GamePlayerId == player.GamePlayerDbId);
					} else
					{
						gamePlayer = new GamePlayer {
							Seat = table.GetSeatByPlayerId(player.PlayerId),
							PlayerId = player is BotPlayer 
								? -table.GetSeatByPlayerId(player.PlayerId) 
								: player.PlayerId < 0 
									? -4  // unregistered
									: player.PlayerId,
						};
						game.GamePlayer.Add(gamePlayer);
					}
					gamePlayer.MarksLost = player.MarksLost;
					gamePlayer.LowMarksLost = player.LowMarksLost;
					gamePlayer.MarksWon = player.MarksWon;
					gamePlayer.LowMarksWon = player.LowMarksWon;
					gamePlayer.BidsMadeSum = player.BidsMadeSum;
					gamePlayer.BidsSetSum = player.BidsSetSum;
					gamePlayer.GamePlayerBid.Add(new GamePlayerBid {
						Amount = (byte) player.Bid.Amount,
						BidPosition = (byte) player.Bid.BidPosition,
						CreateDate = DateTime.Now,
						HandCount = (byte) table.HandCount,
						IsLow = player.Bid.IsLow,
						IsWinning = gamePlayer.Seat == bidWinner,
						TrumpSuit = (short) player.Bid.Trump,
						WasMade = markWinner == bidWinner,
						Hand = player.OriginalHand,
						TricksPlayed = (byte) tricksPlayed,
					});

				}

				byte trickOrder = 0;
				foreach (var trick in table.Tricks)
				{
					game.Trick.Add(new Trick
					{
						HandCount = (byte)table.HandCount,
						TrickOrder = trickOrder,
						Bones = trick.ToString(),
						LeadSeat = (byte)trick.Leader,
						WinSeat = (byte)trick.Winner,
						CreateDate = DateTime.Now,
					});
					trickOrder++;
				}

				db.SubmitChanges();
				
				if ( table.TableDbId > 0 ) return;

				table.TableDbId = game.GameId;
				foreach ( var player in players )
				{
					int targetId = player is BotPlayer
						? -table.GetSeatByPlayerId(player.PlayerId)
						: player.PlayerId < 0
							? -4 // unregistered
							: player.PlayerId;
					var gamePlayer = game.GamePlayer.Single(gp => gp.PlayerId == targetId);
					player.GamePlayerDbId = gamePlayer.GamePlayerId;
				}
			}
		}

		public List<PlayerStat> GetTopPlayers(int top, int lookBack, int minGamesPlayed)
		{
			return ExecuteWithNolock((db) => {
				var query =
					from p in db.Player
					join gs in (
						from gp in db.GamePlayer
						let lastMinuteHint = SqlMethods.DateDiffMinute(gp.Game.FirstHintDate, gp.Game.EndDate) <= 2
						where gp.Game.StartDate > DateTime.Today.AddDays(-lookBack)
						&& gp.Game.EndDate != null
						&& (gp.Game.HintCount == 0 || (lastMinuteHint && gp.Seat % 2 != gp.Game.WinningTeam))
						//&& gp.PlayerId != -2
						group gp by gp.PlayerId into gpg
						select new
						{
							PlayerId = gpg.Key,
							GamesWon = gpg.Count(g => g.Seat % 2 == g.Game.WinningTeam),
							GamesPlayed = gpg.Count(),
							BidsMade = gpg.Sum(g => g.MarksWon - g.LowMarksWon),
							BidsWon = gpg.Sum(g => (g.MarksLost - g.LowMarksLost) + (g.MarksWon - g.LowMarksWon)),
							LowBidsMade = gpg.Sum(g => g.LowMarksWon),
							LowBidsWon = gpg.Sum(g => g.LowMarksLost + g.LowMarksWon),
							Bids = gpg.Sum(g => g.GamePlayerBid.Count(gpb => gpb.Amount > 0)),
							LastPlayDate = gpg.Max(g => g.Game.StartDate),
						}
					) on p.PlayerId equals gs.PlayerId
					let winRate = gs.GamesWon / (decimal)gs.GamesPlayed
					let bidsMadeRate = gs.BidsWon == 0 ? 0 : gs.BidsMade / (decimal)gs.BidsWon
					let lowBidsMadeRate = gs.LowBidsWon == 0 ? 0 : gs.LowBidsMade / (decimal)gs.LowBidsWon
					where gs.GamesPlayed >= minGamesPlayed
					orderby winRate descending
					select new PlayerStat
					{
						PlayerId = p.PlayerId,
						PlayerName = p.PlayerName + (p.PlayerId < 0 && p.PlayerId > -4 ? "(bot)" : ""),
						WinRate = winRate,
						GamesWon = gs.GamesWon,
						GamesPlayed = gs.GamesPlayed,
						BidsMadeRate = bidsMadeRate,
						BidsMade = gs.BidsMade,
						BidsWon = gs.BidsWon,
						LowBidsMadeRate = lowBidsMadeRate,
						LowBidsMade = gs.LowBidsMade,
						LowBidsWon = gs.LowBidsWon,
						Bids = gs.Bids,
						LastPlayDate = gs.LastPlayDate,
					};
				return query.Take(top).ToList();
			});
		}

		public void SaveHand(string hand, int playerId)
		{
			var dots = hand.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < dots.Length; i++ )
			{
				dots[i] = new Domino(dots[i]).ToString();
			}
			string sortedHand = "";
			dots.ToList().OrderByDescending(d => d).ToList().ForEach(d => sortedHand += d + " ");
			hand = new Hand(sortedHand.TrimEnd()).ToString();
			
			using ( var db = new FortyTwoDataContext(AppConfig.GetDefaultConnectionString()) )
			{
				var savedHand = db.SavedHand.SingleOrDefault(h => h.PlayerId == playerId && h.Hand == hand);
				if ( savedHand != null ) return;
				savedHand = new SavedHand {
					PlayerId = playerId,
					Hand = hand,
					CreateDate = DateTime.Now,
				};
				db.SavedHand.InsertOnSubmit(savedHand);
				db.SubmitChanges();
			}
		}

		public List<FortyTwoLib.Entities.SavedHand> GetHands(int playerId)
		{
			return ExecuteWithNolock((db) => {
				var query =
					from h in db.SavedHand
					where h.PlayerId == playerId
					select new FortyTwoLib.Entities.SavedHand {
						Hand = h.Hand,
						PlayerId = h.PlayerId,
						SavedHandId = h.SavedHandId,
					}
				;
				return query.ToList();
			});
		}

		public void DeleteOldData()
		{
			using ( var db = new FortyTwoDataContext(AppConfig.GetDefaultConnectionString()) )
			{
				
			}
		}
	}
}
