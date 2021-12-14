using System;
using System.Collections.Generic;
using System.Linq;
using FortyTwoLib.Logging;

namespace FortyTwoLib
{
	public class PlayCalc : Calc
	{
		private double CalcProb(Player player, Table table, string[] aConjuncts, int lHighTrumpCount, int lHighLeadCount)
		{
			return this.CalcProb(player, table, aConjuncts, lHighTrumpCount, lHighLeadCount, (int) table.LeadSuit);
		}

		private double CalcProb(Player player, Table table, string[] aConjuncts, int lHighTrumpCount, int lHighLeadCount, int leadSuit)
		{
			/* This routine simplifies calling the real workhorse when calculating probs
			'for real moves after bidding has occurred and we can use the actual match
			'to supply most values */

			return CalcProbPrim(aConjuncts, table.Trump, table.SuitCount.OutCount(table.Trump, player.Hand) - lHighTrumpCount, lHighTrumpCount,
				(int) leadSuit, table.SuitCount.OutCount(leadSuit, player.Hand) - lHighLeadCount, lHighLeadCount,
				table.OppLeft(player.PlayerId).Hand.Count(), !table.HasNo(table.OppLeftSeat(player.PlayerId), table.Trump), !table.HasNo(table.OppLeftSeat(player.PlayerId), leadSuit),
				table.Partner(player.PlayerId).Hand.Count(), !table.HasNo(table.PartnerSeat(player.PlayerId), table.Trump), !table.HasNo(table.PartnerSeat(player.PlayerId), leadSuit),
				table.OppRight(player.PlayerId).Hand.Count(), !table.HasNo(table.OppRightSeat(player.PlayerId), table.Trump), !table.HasNo(table.OppRightSeat(player.PlayerId), leadSuit));
		}

		public double ProbWinLead(Domino d, Player player, Table table)
		{
			int lHighLeadCount;
			int lHighTrumpCount;
			int lLeadSuit;

			if( d.IsA(table.Trump) )
			{
				lLeadSuit = table.Trump;
				lHighTrumpCount = player.Hand.HigherThanCount(d, table.Trump, table.Played);
				lHighLeadCount = 0;
			}
			else
			{
				lLeadSuit = d.Hi(table.Trump);
				lHighLeadCount = player.Hand.HigherThanCount(d, table.Trump, table.Played);
				lHighTrumpCount = 0;
			}
			return ProbWinLeadPrim(table.Trump, table.SuitCount.OutCount(table.Trump, player.Hand) - lHighTrumpCount, lHighTrumpCount,
				lLeadSuit, table.SuitCount.OutCount(lLeadSuit, player.Hand) - lHighLeadCount, lHighLeadCount,
				table.OppLeft(player.PlayerId).Hand.Count(), !table.HasNo(table.OppLeftSeat(player.PlayerId), table.Trump), !table.HasNo(table.OppLeftSeat(player.PlayerId), lLeadSuit),
				table.Partner(player.PlayerId).Hand.Count(), !table.HasNo(table.PartnerSeat(player.PlayerId), table.Trump), !table.HasNo(table.PartnerSeat(player.PlayerId), lLeadSuit),
				table.OppRight(player.PlayerId).Hand.Count(), !table.HasNo(table.OppRightSeat(player.PlayerId), table.Trump), !table.HasNo(table.OppRightSeat(player.PlayerId), lLeadSuit));
		}

		/// <summary>
		/// Return the probability that either me or my partner will win if I play dominoe d from my hand
		/// </summary>
		public double ProbWin(Domino d, Player player, Table table)
		{
			return ProbWin(d, player, table, table.PlayCount);
		}
 
		public double ProbWin(Domino d, Player player, Table table, int playCount)
		{
			double dProb;

			//'Playing last
			if( playCount == 3 )
			{
				if( table.PartnerSeat(player.PlayerId) == table.TrickWinner() )
				{
					dProb = 1;
				}
				else
				{
					dProb = d.GreaterThan(table.WinningPlay(), table.LeadPlay, table.Trump, false) ? 1 : 0;
				}

				//'Opponent to left will make last play after me, partner lead
			}
			else if( playCount == 2 )
			{
				//'if ( my partner's play is better than mine, then the odds of winning
				//'are his odds of winning
				if( table.PartnerSeat(player.PlayerId) == table.TrickWinner() )
				{
					if( table.WinningPlay().GreaterThan(d, table.LeadPlay, table.Trump, false) )
					{
						d = table.WinningPlay();
					}
				}

				//'if ( d is not greater than the oppRight's play, d has
				//'no chance of winning. Also consider lo, where oppright not playing
				if( d.GreaterThan(table.LastPlay(table.OppRightSeat(player.PlayerId)), table.LeadPlay, table.Trump, false) )
				{
					if( d.IsA(table.Trump) )
					{
						//'We win if:
						//'oppleft has no higher trump
						//'oppleft has lead suit
						dProb = CalcProb(player, table, new[] {"LZ00", "LL1"}, player.Hand.HigherThanCount(d, table.Trump, table.Played), 0);

						//'d is not a trump
					}
					else
					{
						/* We lose if:
						'oppLeft has a higher lead suit, or,
						'oppleft has no lead suit and oppleft has trump */
						dProb = 1 - CalcProb(player, table, new[] {"LH1", "LT1 LL00 LH00"}, 0, player.Hand.HigherThanCount(d, table.Trump, table.Played));
					}
				}
				else
				{
					dProb = 0;
				}

				//'Opponent to right lead, partner will make last play
			}
			else if( playCount == 1 )
			{
				/*'if ( d is not greater than oppRight's play, d's only chance of winning
			    'is based on what my partner plays */
				if( d.GreaterThan(table.LastPlay(table.OppRightSeat(player.PlayerId)), table.LeadPlay, table.Trump, false) )
				{
					if( d.IsA(table.Trump) )
					{
						if( (int) table.LeadSuit == table.Trump )
						{
							/*'We lose if:
							  'oppleft has higher trump and partner has no higher trump, or
							  'oppleft has higher trump and partner has higher trump / 2 */
							dProb = 1 - CalcProb(player, table, new[] {"LZ1 PZ00", "LZ1 PZ1 /2"},
								player.Hand.HigherThanCount(d, table.Trump, table.Played), 0);
						}
						else
						{
							/*'We lose if:
							  'oppleft has higher trump and no lead and partner has no higher trump
							  'oppleft has higher trump and no lead and partner has lead
							  'oppleft has higher trump and no lead and partner has higher trump and no lead / 2 */
							dProb = 1 - CalcProb(player, table, new[] {"LZ1 LL00 PZ00", "LZ1 LL00 PL1", "LZ1 LL00 PZ1 PL00 /2"},
								player.Hand.HigherThanCount(d, table.Trump, table.Played), 0);
						}

						/*'d is not a trump.  Note that if d beats lead dominoe and d is not
					      'a trump, the the lead cannot be a trump*/
					}
					else
					{
						/*'We win if:
						'oppleft has no higher lead and lower lead
						'oppleft has no higher lead and no trump
						'oppleft has no lead and trump and partner has no lead and trump / 2
						'oppleft has higher lead and partner has higher lead / 2*/
						dProb = CalcProb(player, table, new[] {"LH00 LL1", "LH00 LT00", "LH00 LL00 LT1 PH00 PL00 PT1 /2", "LH1 PH1 /2"},
							0, player.Hand.HigherThanCount(d, table.Trump, table.Played));
					}

					//'Depending on partner to win
				}
				else
				{
					if( (int) table.LeadSuit == table.Trump )
					{
						/*'We win if:
						'partner has higher trump and oppleft has no higher trump
						'partner has higher trump and oppleft has higher trump / 2*/
						dProb = CalcProb(player, table, new[] {"PZ1 LZ00", "PZ1 LZ1 /2"}, player.Hand.HigherThanCount(table.LeadPlay, table.Trump, table.Played), 0);
					}
					else
					{
						/*'We win if:
						'partner has higher lead and oppleft has no higher lead and lower lead
						'partner has trump and no lead and oppleft has lower lead
						'partner has trump and no lead and oppleft has higher lead
						'partner has higher lead and oppleft has higher lead / 2
						'partner has trump and no lead and oppleft has trump and no lead / 2 */
						dProb = CalcProb(player, table, new[] {"PH1 LH00 LL1", "PT1 PL00 PH00 LL1", "PT1 PL00 PH00 LH1", "PH1 LH1 /2", "PT1 PH00 PL00 LT1 LH00 LL00 /2"},
							0, player.Hand.HigherThanCount(table.LeadPlay, table.Trump, table.Played));
					}
				}
			//'Leading
			} else
			{
				dProb = ProbWinLead(d, player, table);
			}
			return dProb;
		}
		public double ProbLose(Domino d, Player player, Table table)
		{
			return ProbLose(d, player, table, table.PlayCount);
		}

		public double ProbLose(Domino d, Player player, Table table, int playCount)
		{
			/* 'Return the probability that neither me nor my partner will win if I
				'play dominoe d from my hand */

			double dProb;

			int lOutSeat = table.OutSeat;

			//'Playing last
			if( playCount == 3 )
			{
				if( table.PartnerSeat(player.PlayerId) == table.TrickWinner() )
				{
					dProb = 0;
				}
				else
				{
					dProb = d.GreaterThan(table.WinningPlay(), table.LeadPlay, table.Trump, true) ? 0 : 1;
				}

				//'Opponent to left will make last play after me, partner lead (oppLeft lo)
			}
			else if( playCount == 2 && lOutSeat == table.OppRightSeat(player.PlayerId) )
			{
				/*'if my partner's play is higher than mine, then the odds of losing
				'are his odds of losing */
				if( table.WinningPlay().GreaterThan(d, table.LeadPlay, table.Trump, true) )
				{
					d = table.WinningPlay();
				}
				//'Trump only means doubles are a suit of their own for lo
				if( d.IsA(table.Trump) )
				{
					/*'We lose if:
					'Oppleft has higher doubles and no lower doubles */
					dProb = CalcProb(player, table, new[] {"LZ1 LT00"}, player.Hand.HigherThanCount(d, table.Trump, table.Played), 0);
				}
				else
				{
					/*'We lose if:
					'OppLeft has higher lead and no lower lead */
					dProb = CalcProb(player, table, new[] {"LH1 LL00"}, 0, player.Hand.HigherThanCount(d, table.Trump, table.Played));
				}

				//'Partner will make last play after me, oppRight lead (oppRight lo)
			}
			else if( playCount == 2 && table.OutSeat == table.OppLeftSeat(player.PlayerId) )
			{
				//'if d is greather than oppRight's play, then we can't lose
				if( d.GreaterThan(table.LeadPlay, table.LeadPlay, table.Trump, true) )
				{
					dProb = 0;
				}
				else
				{
					//'Trump only means doubles are a suit of their own for lo
					if( table.LeadPlay.IsA(table.Trump) )
					{
						/*'we lose if:
						'my partner has a lower double than oppright's*/
						dProb = CalcProb(player, table, new[] {"PT1"}, player.Hand.HigherThanCount(table.LeadPlay, table.Trump, table.Played), 0);
					}
					else
					{
						/*'we lose if:
						'my partner has a lower lead than oppright's*/
						dProb = CalcProb(player, table, new[] {"PL1"}, 0, player.Hand.HigherThanCount(table.LeadPlay, table.Trump, table.Played));
					}
				}

				//'Opponent to left will make last play after me, oppRight lead (I am lo)
			}
			else if( playCount == 2 && table.OutSeat == table.PartnerSeat(player.PlayerId) )
			{
				/*'if d is greater than oppright's lead, then we lose only if oppLeft
				'has to take trick*/
				if( d.GreaterThan(table.LeadPlay, table.LeadPlay, table.Trump, true) )
				{
					//'Trump only means doubles are a suit of their own for lo
					if( d.IsA(table.Trump) )
					{
						/*'We lose if:
						'oppleft has higher doubles and no lower doubles*/
						dProb = CalcProb(player, table, new[] {"LZ1 LT00"}, player.Hand.HigherThanCount(d, table.Trump, table.Played), 0);
					}
					else
					{
						/*'We lose if:
						'oppleft has higher lead and no lower lead*/
						dProb = CalcProb(player, table, new[] {"LH1 LL00"}, 0, player.Hand.HigherThanCount(d, table.Trump, table.Played));
					}
					//'if d is less than oppright's lead, we lose regardless of oppleft's play
				}
				else
				{
					dProb = 1;
				}

				//'Leading, oppLeft and oppRight will follow (I am low)
			}
			else if( playCount == 0 && table.PartnerSeat(player.PlayerId) == lOutSeat )
			{
				//'Trump only means doubles are a suit of their own for lo
				if( d.IsA(table.Trump) )
				{
					/*'we lose if:
					  'oppleft has higher doubles and no lower doubles
					  'oppright has higher doubles and no lower doubles*/
					dProb = CalcProb(player, table, new[] {"LZ1 LT00", "RZ1 RT00"}, player.Hand.HigherThanCount(d, table.Trump, table.Played), 0, d.Hi(table.Trump));
				}
				else
				{
					/*'we lose if:
					  'oppleft has higher lead and no lower lead
					  'oppright has higher lead and no lower lead */
					dProb = CalcProb(player, table, new[] {"LH1 LL00", "RH1 RL00"}, 0, player.Hand.HigherThanCount(d, table.Trump, table.Played), d.Hi(table.Trump));
				}

				//'Leading, oppLeft and partner will follow (oppLeft low)
			}
			else if( playCount == 0 && table.OppRightSeat(player.PlayerId) == lOutSeat )
			{
				//'Trump only means doubles are a suit of their own for lo
				if( d.IsA(table.Trump) )
				{
					/*'We lose if:
					  'oppleft has higher doubles and no lower doubles and partner has no higher doubles
					  'oppleft has higher doubles and no lower doubles and partner has higher doubles and now lower doubles / 2*/
					dProb = CalcProb(player, table, new[] {"LZ1 LT00 PZ00", "LZ1 LT00 PZ1 PT00 /2"}, player.Hand.HigherThanCount(d, table.Trump, table.Played), 0, d.Hi(table.Trump));
				}
				else
				{
					/*'We lose if:
					  'oppleft has higher lead and no lower lead and partner has no higher lead
					  'oppleft has higher lead and no lower lead and partner has higher lead and no lower lead / 2*/
					dProb = CalcProb(player, table, new[] {"LH1 LL00 PH00", "LH1 LL00 PH1 PL00 /2"}, 0, player.Hand.HigherThanCount(d, table.Trump, table.Played), d.Hi(table.Trump));
				}

				//'Leading, partner and oppRight will follow (oppright low)
			}
			else if( playCount == 0 && table.OppLeftSeat(player.PlayerId) == lOutSeat )
			{
				//'Trump only means doubles are a suit of their own for lo
				if( d.IsA(table.Trump) )
				{
					/*'We lose if:
					  'oppright has higher doubles and no lower doubles and partner has no higher doubles
					  'oppright has higher doubles and no lower doubles and partner has higher doubles and now lower doubles / 2*/
					dProb = CalcProb(player, table, new[] {"RZ1 RT00 PZ00", "RZ1 RT00 PZ1 PT00 /2"}, player.Hand.HigherThanCount(d, table.Trump, table.Played), 0, d.Hi(table.Trump));
				}
				else
				{
					/*'We lose if:
					  'oppright has higher lead and no lower lead and partner has no higher lead
					  'oppright has higher lead and no lower lead and partner has higher lead and no lower lead / 2*/
					dProb = CalcProb(player, table, new[] {"RH1 RL00 PH00", "RH1 RL00 PH1 PL00 /2"}, 0, player.Hand.HigherThanCount(d, table.Trump, table.Played), d.Hi(table.Trump));
				}
			}
			else
			{
				//'invalid play position for lo
				throw new InvalidOperationException("Invalid play position for low.");
			}

			return dProb;
		}

		public Domino GetPlay(Player player, Table table, ILog log)
		{
			var hand = player.Hand;
			if( hand.Count() == 1 ) return hand.First();
			var mustPlay = hand.MustPlay(table.LeadPlay, table.Trump);
			if( mustPlay.Count() == 1 ) return mustPlay.First();

			/*' if ( I am not leading, determine which dominoes, if any, I must
				' play based on the lead dominoe*/
			var oMust = player.Hand.MustPlay(table.LeadPlay, table.Trump);

			var oChoices = oMust.Count() > 0 ? oMust : player.Hand.ToList();

			//'if ( I have only one playable dominoe, play it without wasting time
			if( oChoices.Count() == 1 ) return oChoices.First();

			//'Determine how many points each team needs to win this round
			int lGoal = 0;
			int lTheirGoal = 0;
			if( table.GetSeatByPlayerId(player.PlayerId) % 2 == table.GetSeatByPlayerId(table.BidWinner().PlayerId) % 2 )
			{
				lGoal = table.WinningBid.MakeBidAmount - table.GetTeamByPlayerId(player.PlayerId).CountValue();
				lTheirGoal = (42 - table.WinningBid.MakeBidAmount) + 1 - table.GetOpponentsByPlayerId(player.PlayerId).CountValue();
			}
			else
			{
				lGoal = (42 - table.WinningBid.MakeBidAmount) + 1 - table.GetTeamByPlayerId(player.PlayerId).CountValue();
				lTheirGoal = table.WinningBid.MakeBidAmount - table.GetOpponentsByPlayerId(player.PlayerId).CountValue();
			}

			Domino d = null;
			var oCount = new List<Domino>();
			var oTrumps = new List<Domino>();
			var oDoubles = new List<Domino>();
			var oOthers = new List<Domino>();
			for ( int lD = 0; lD < oChoices.Count; lD++ )
			{
				d = oChoices[lD];
				if( d.CountValue() > 0 ) oCount.Add(d);
				if( d.IsA(table.Trump) ) {
					oTrumps.Add(d);
				} else if( d.IsA(SuitEnum.doubles) )
				{
					oDoubles.Add(d);
				} else
				{
					oOthers.Add(d);
				}

				//' if this is not lo, see if there is a play that is sure to end round favorably
				if ( !table.WinningBid.IsLow && d.CountValue() + table.CountSoFar() + 1 >= lGoal )
				{
					if ( ProbWin(d, player, table) == 1 ) return d;
				}
			}

			/*'if ( this is not lo, I am leading, I am the bid winner, and
			  'I called a trump suit
			  'I have more than one trump left*/
			if( !table.WinningBid.IsLow &&
				table.PlayCount == 0 &&
					table.BidWinner() == player &&
						table.Trump != (int) SuitEnum.followMe &&
							player.Hand.SuitCount(table.Trump) > 1 )
			{
				//'I have the highest remaining trump
				var highest = player.Hand.Highest(table.Trump, table);
				if( player.Hand.HigherThanCount(highest, table.Trump, table.Played) == 0 ) return highest;

				//'I this is first play of hand and I do not have highest trump
				if( table.TrickCount == 0 &&
					!player.Hand.Contains(SuitOrders.Get(table.Trump, 6)) &&
						player.Hand.Contains(SuitOrders.Get(table.Trump, 5
						)) )
				{
					return player.Hand.Lowest(table.Trump, table);
				}
			}

			/*'Rank dominoes taking into consideration count in play so far, count of
			  'dominoe, probability of winning, and the value of the dominoe as a lead
			  'play*/

			var jaBones = new List<Domino>();
			if( table.WinningBid.IsLow && table.BidWinner() == player )
			{
				for ( int lD = 0; lD < oChoices.Count; lD++ )
				{
					d = oChoices[lD];
					if( table.TrickCount == 0 )
					{
						if( d.IsA(table.Trump) )
						{
							/*'we lose if:
							  'oppleft has higher doubles and no lower doubles
							  'oppright has higher doubles and no lower doubles*/
							d.PlayNowProb = CalcProb(player, table, new[] {"LZ1 LT00", "RZ1 RT00"}, player.Hand.HigherThanCount(d, table.Trump, table.Played), 0, d.Hi(table.Trump));
						}
						else
						{
							/*'we lose if:
							  'oppleft has higher lead and no lower lead
							  'oppright has higher lead and no lower lead*/
							d.PlayNowProb = CalcProb(player, table, new[] {"LH1 LL00", "RH1 RL00"}, 0, player.Hand.HigherThanCount(d, table.Trump, table.Played), d.Hi(table.Trump));
						}
					}
					else
					{
						d.PlayNowProb = ProbLose(d, player, table);
					}
					d.LeadProb = ProbLoseFollow(d, table.Trump, table.Played, table.OppLeft(player.PlayerId).Hand.Count(), table.OppRight(player.PlayerId).Hand.Count(), table.SuitCount, player.Hand);
					d.SortKey = d.PlayNowProb.ToString("0.00000") + (1 - d.LeadProb).ToString("0.00000");
					jaBones.Add(d);
				}
			} else
			{
				for ( int lD = 0; lD < oChoices.Count; lD++ )
				{
					d = oChoices[lD];
					string sKey = "";
					string sKey2 = "";

					if( !table.WinningBid.IsLow )
					{
						d.PlayNowProb = ProbWin(d, player, table);
						int countValue = d.CountValue();
						int leadSuit = d.Hi(table.Trump);
						if ( d.PlayNowProb < 1 // this isn't a sure winner
							&& !d.IsA(SuitEnum.doubles) // not a double
							&& table.PlayCount == 0 // I'm leading
							&& table.BidWinner() == table.Partner(player.PlayerId) // My partner won the bid
							&& table.WinningBid.Amount < 32 // partner could have 4, 5, 6 off (or is a poor bidder)
							&& !d.IsA(table.Trump) // not a trump
							&& leadSuit >= 4 // leads as a 4, 5, or 6
							&& (((leadSuit == 4 || leadSuit == 6) && StillOut("64", player, table)) // The big count is still out
								|| ((leadSuit == 5) && StillOut("55", player, table)))
							&& !table.HasNo(table.PartnerSeat(player.PlayerId), leadSuit) // my partner could still have this suit
							&& !(table.HasNo(table.OppLeftSeat(player.PlayerId), leadSuit) && table.HasNo(table.OppRightSeat(player.PlayerId), leadSuit))
							&& countValue < 10 // Not the big boy himself
						)
						{
							// Add in the count we will lose if the big boy comes out
							countValue += 10;
							d.PlayNowProb = 0.33333; // Prevent bot partner from leading 05 back even if he doesn't think my probability of having a 5 is high
						}
						//double val = 50 + ((countValue + 1 + table.CountSoFar()) * ((2 * d.PlayNowProb) - 1));
						var factor = table.BidWinner() == table.Partner(player.PlayerId) 
							? d.IsA(table.Trump) 
								? 0.0 
								: 2.0 
							: 1.0;
						var factor2 = table.BidWinner() == table.Partner(player.PlayerId) ? 0.9 : 1.0;
						double val = 50 + (factor * (d.PlayNowProb - 0.5)) + ((countValue + 1 + table.CountSoFar()) * ((factor2 * d.PlayNowProb) - 0.5));
						sKey = val.ToString("00.00000");
					}
					else
					{
						d.PlayNowProb = ProbLose(d, player, table);
						sKey = d.PlayNowProb.ToString("00.00000");
					}
					//'Don't waste time recomputing same value
					if( table.PlayCount == 0 )
					{
						d.LeadProb = d.PlayNowProb;
					}
					else
					{
						if( !table.WinningBid.IsLow )
						{
							d.LeadProb = ProbWin(d, player, table, 0);
						}
						else
						{
							d.LeadProb = ProbLose(d, player, table, 0);
						}
					}
					if( !table.WinningBid.IsLow )
					{
						sKey2 = (50 - ((d.CountValue() + 1) * ((2 * d.LeadProb) - 1))).ToString("00.000000");
					}
					else
					{
						sKey2 = d.LeadProb.ToString("00.00000");
					}
					d.SortKey = sKey + sKey2;
					jaBones.Add(d);
				}
			}
			jaBones = jaBones.OrderByDescending(b => b.SortKey).ToList();
			jaBones.ForEach(dom => log.WriteVerbose("   Domino:" + dom + " NowProb:" + dom.PlayNowProb + " LeadProb:" + dom.LeadProb + " SortKey:" + dom.SortKey));

			/*'Save last trump to try and trump in on any outstanding 10 count
			  'if I am leading, I am the bidwinner, I called a trump suit,
			  'this is not lo, I have only one trump left, and
			  'either the 64 is out and I don't have the 66 or the 55 is out*/
			if( table.PlayCount == 0 &&
				table.BidWinner() == player &&
				table.Trump != (int) SuitEnum.followMe &&
				!table.WinningBid.IsLow &&
				player.Hand.SuitCount(table.Trump) == 1 &&
				jaBones[0].IsA(table.Trump) &&
				((!(player.Hand.Contains("64") || table.Played.Contains(new Domino("64")) || table.Trump == 6 || player.Hand.Contains("66"))) ||
				(!(player.Hand.Contains("55") || table.Played.Contains(new Domino("55")) || table.Trump == 5))) )
			{
				return jaBones[1];
			} else
			{
				return jaBones[0];
			}
		}

		private bool StillOut(string dots, Player player, Table table)
		{
			return !player.Hand.Contains(dots) && !table.Played.Contains(new Domino(dots));
		}
	}
}
