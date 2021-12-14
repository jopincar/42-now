using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FortyTwoLib.Logging;

namespace FortyTwoLib
{
	public class BidCalc : Calc
	{
		public double ProbWinLeadBid(Domino d, SuitEnum trumpSuit, SuitCount aSuitCounts, List<Domino> oPlayed, int handSize, Hand hand)
		{
			int lHighLeadCount;
			int lHighTrumpCount;
			int lTrumpSuit = (int)trumpSuit;
			int lLeadSuit;
			int lLeadCount;
			int lTrumpCount;

			if (d.IsA(trumpSuit))
			{
				lLeadSuit = lTrumpSuit;
				lHighTrumpCount = hand.HigherThanCount(d, lTrumpSuit, oPlayed);
				lTrumpCount = aSuitCounts.OutCount((SuitEnum)lTrumpSuit, hand);
				if (lHighTrumpCount > lTrumpCount)
				{
					lHighTrumpCount = lTrumpCount;
				}
				lHighLeadCount = 0;
			}
			else
			{
				lLeadSuit = d.Hi(lTrumpSuit);
				lHighLeadCount = hand.HigherThanCount(d, lTrumpSuit, oPlayed);
				lLeadCount = aSuitCounts.OutCount((SuitEnum)lLeadSuit, hand);
				if (lHighLeadCount > lLeadCount)
				{
					lHighLeadCount = lLeadCount;
				}
				lHighTrumpCount = 0;
			}
			return ProbWinLeadPrim(lTrumpSuit, aSuitCounts.OutCount((SuitEnum)lTrumpSuit, hand) - lHighTrumpCount, lHighTrumpCount,
				lLeadSuit, aSuitCounts.OutCount((SuitEnum)lLeadSuit, hand) - lHighLeadCount, lHighLeadCount,
				handSize, true, true,
				handSize, true, true,
				handSize, true, true);
		}

		protected List<Domino> SortByProbWinLead(Hand hand, SuitEnum trumpSuit, SuitCount suitCount, List<Domino> oPlayed, int handSize)
		{
			var ret = new List<Domino>();
			foreach (Domino dom in hand)
			{
				dom.LeadProb = ProbWinLeadBid(dom, trumpSuit, suitCount, oPlayed, handSize, hand);
				ret.Add(dom);
			}
			ret = ret.OrderByDescending(dom => dom.LeadProb).ToList();
			return ret;
		}

		public BidVal BidEvalSuit(Hand origHand, SuitEnum trumpSuit, ILog log)
		{
			var hand = origHand.GetCopy();
			hand.SetTrump(trumpSuit);

			var suitCount = new SuitCount(trumpSuit);
			double totProb = 1.0;
			int handSize = 7;
			int garbage = 0;
			int loss = 0;
			var calc = new Calc();
			var oPlayed = new List<Domino>();
			var oOtherPlays = new List<Domino>();

			if (trumpSuit != SuitEnum.followMe)
			{
				// See if ( we need to evaluate jumping gap of highest to third highest trump
				if (hand.Contains(SuitOrders.Get(trumpSuit, 6))
					&& !hand.Contains(SuitOrders.Get(trumpSuit, 5))
					&& hand.Contains(SuitOrders.Get(trumpSuit, 4))
					&& hand.SuitCount(trumpSuit) >= 3)
				{
					totProb = 1 - calc.CalcProbPrim(new[] { "LZ1 LT1", "RZ1 RT1" }, (int)trumpSuit,
						suitCount.OutCount(trumpSuit, hand) - 1, 1, (int)trumpSuit, 0, 0,
						7, true, true, 7, true, true, 7, true, true);
					if (calc.CalcProbPrim(new[] { "LT1 RT1 PT1" }, (int)trumpSuit,
						suitCount.OutCount(trumpSuit, hand), 0, (int)trumpSuit, 0, 0,
						7, true, true, 7, true, true, 7, true, true) > 0.5)
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 4;
					}
					else if (calc.CalcProbPrim(new[] { "LT1 RT1 PT00", "LT1 RT00 PT1", "LT00 RT1 PT1" }, (int)trumpSuit,
						suitCount.OutCount(trumpSuit, hand), 0, (int)trumpSuit, 0, 0,
						7, true, true, 7, true, true, 7, true, true) > 0.5)
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 3;
					}
					else
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 2;
					}
					var domino = SuitOrders.Get(trumpSuit, 6);
					log.WriteVerbose(string.Format("  {0} {1}", domino, domino.LeadProb));
					hand.Remove(domino);
					oPlayed.Add(domino);
					handSize = 6;
					domino = SuitOrders.Get(trumpSuit, 5);
					log.WriteVerbose(string.Format("  {0} {1}", domino, domino.LeadProb));
					oPlayed.Add(domino);
					oOtherPlays.Add(domino);
					log.WriteVerbose(string.Format("  Jumping first trump gap with probability: {0}", totProb));
				} else if (hand.Contains(SuitOrders.Get(trumpSuit, 6))
					&& !hand.Contains(SuitOrders.Get(trumpSuit, 5))
					&& !hand.Contains(SuitOrders.Get(trumpSuit, 4))
					&& hand.Contains(SuitOrders.Get(trumpSuit, 3))
					&& hand.SuitCount(trumpSuit) >= 4)
					// See if we need to evaluate jumping gap of highest to fourth highest trump
				{
					totProb = 1 - calc.CalcProbPrim(new[] { "LZ1 LT1", "RZ1 RT1" }, (int)trumpSuit,
						suitCount.OutCount(trumpSuit, hand) - 1, 1, (int)trumpSuit, 0, 0,
						7, true, true, 7, true, true, 7, true, true);
					if (calc.CalcProbPrim(new[] { "LT1 RT1 PT1" }, (int)trumpSuit,
						suitCount.OutCount(trumpSuit, hand), 0, (int)trumpSuit, 0, 0,
						7, true, true, 7, true, true, 7, true, true) > 0.5)
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 4;
					}
					else if (calc.CalcProbPrim(new[] { "LT1 RT1 PT00", "LT1 RT00 PT1", "LT00 RT1 PT1" }, (int)trumpSuit,
						suitCount.OutCount(trumpSuit, hand), 0, (int)trumpSuit, 0, 0,
						7, true, true, 7, true, true, 7, true, true) > 0.5)
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 3;
					}
					else
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 2;
					}
					var domino = SuitOrders.Get(trumpSuit, 6);
					log.WriteVerbose(string.Format("  {0} {1}", domino, domino.LeadProb));
					hand.Remove(domino);
					oPlayed.Add(domino);
					handSize = 6;
					domino = SuitOrders.Get(trumpSuit, 5);
					log.WriteVerbose(string.Format("  {0} {1}", domino, domino.LeadProb));
					oPlayed.Add(domino);
					oOtherPlays.Add(domino);
					domino = SuitOrders.Get(trumpSuit, 4);
					log.WriteVerbose(string.Format("  {0} {1}", domino, domino.LeadProb));
					oPlayed.Add(domino);
					oOtherPlays.Add(domino);
					log.WriteVerbose(string.Format("  Jumping two bone trump gap with probability: {0}", totProb));
				} else if ((hand.Contains(SuitOrders.Get(trumpSuit, 6)) &&
					hand.Contains(SuitOrders.Get(trumpSuit, 5)) &&
					!hand.Contains(SuitOrders.Get(trumpSuit, 4)) &&
					hand.Contains(SuitOrders.Get(trumpSuit, 3))))
				{
					totProb = 1 - calc.CalcProbPrim(new[] { "LZ1 LT2", "RZ1 RT2" }, (int)trumpSuit,
						suitCount.OutCount(trumpSuit, hand) - 1, 1, (int)trumpSuit, 0, 0,
						7, true, true, 7, true, true, 7, true, true);
					if (suitCount.OutCount(trumpSuit, hand) > 1)
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 4;
					}
					else
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 3;
					}
					var domino = SuitOrders.Get(trumpSuit, 6);
					hand.Remove(domino);
					oPlayed.Add(domino);
					log.WriteVerbose(string.Format("  {0} {1}", domino, domino.LeadProb));
					domino = SuitOrders.Get(trumpSuit, 5);
					hand.Remove(domino);
					oPlayed.Add(domino);
					handSize = 5;
					domino = SuitOrders.Get(trumpSuit, 4);
					oPlayed.Add(domino);
					log.WriteVerbose(string.Format("  {0} {1}", domino, domino.LeadProb));
					oOtherPlays.Add(domino);
					log.WriteVerbose(string.Format("  Jumping second trump gap with probability: {0}", totProb));
				}
				else if (!hand.Contains(SuitOrders.Get(trumpSuit, 6)) &&
					hand.Contains(SuitOrders.Get(trumpSuit, 5)) &&
					(hand.Contains("64") || (trumpSuit != SuitEnum.sixes)) &&
					(hand.SuitCount(trumpSuit) > 1))
				{
					totProb = 1 - calc.CalcProbPrim(new[] { "RZ1 RT1" }, (int)trumpSuit,
						suitCount.OutCount(trumpSuit, hand) - 1, 1, (int)trumpSuit, 0, 0,
						7, true, true, 7, true, true, 7, true, true);
					if (calc.CalcProbPrim(new[] { "LT1 RT1 PT1" }, (int)trumpSuit,
						suitCount.OutCount(trumpSuit, hand), 0, (int)trumpSuit, 0, 0,
						7, true, true, 7, true, true, 7, true, true) > 0.5)
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 4;
					}
					else if (calc.CalcProbPrim(new[] { "LT1 RT1 PT00", "LT1 RT00 PT1", "LT00 RT1 PT1" }, (int)trumpSuit,
						suitCount.OutCount(trumpSuit, hand), 0, (int)trumpSuit, 0, 0,
						7, true, true, 7, true, true, 7, true, true) > 0.5)
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 3;
					}
					else
					{
						suitCount[trumpSuit] = suitCount[trumpSuit] - 2;
					}
					log.WriteVerbose("  Playing lowest trump first");
					if ( trumpSuit == SuitEnum.blanks && !hand.Contains(new Domino("05")) )
					{
						log.WriteVerbose("    Assuming 05 lost -5.");
						loss += 5;
					}
					var domino = hand.Lowest(trumpSuit, trumpSuit, false);
					hand.Remove(domino);
					oPlayed.Add(domino);
					log.WriteVerbose(string.Format("  {0} played and lost", domino));
					handSize = 6;
					loss += domino.CountValue() + 1;
					if (domino.CountValue() == 0) garbage++;
					domino = SuitOrders.Get(trumpSuit, 6);
					loss += domino.CountValue();
					log.WriteVerbose(string.Format("  {0} assumed played by another player and lost", domino));
					oPlayed.Add(domino);
					oOtherPlays.Add(domino);
				}
			}

			// Special rule for bidding a suit not in hand -- assume you will lose all count of that suit
			if ( trumpSuit != SuitEnum.followMe && hand.SuitCount(trumpSuit) == 0 )
			{
				for ( int i = 0; i < 7; i++ )
				{
					var lostDom = SuitOrders.Get(trumpSuit, i);
					if (lostDom.CountValue() > 0)
					{
						log.WriteVerbose(string.Format("    Assuming {0} lost for -{1}", lostDom, lostDom.CountValue()));
						loss += lostDom.CountValue();
					}
				}
			}

			int domIdx;
			Domino d = null;
			List<Domino> jaBones;
			while (true)
			{
				jaBones = SortByProbWinLead(hand, trumpSuit, suitCount, oPlayed, handSize);

				domIdx = 0;
				while (domIdx < jaBones.Count && jaBones[domIdx].LeadProb == 1)
				{
					d = jaBones[domIdx];
					log.WriteVerbose(string.Format("  {0} {1}", d, d.LeadProb));
					if (d.IsA(trumpSuit))
					{
						if (calc.CalcProbPrim(new[] { "LT1 RT1 PT1" }, (int)trumpSuit,
							suitCount.OutCount(trumpSuit, hand), 0, (int)trumpSuit, 0, 0,
							handSize, true, true, handSize, true, true, handSize, true, true) > 0.5)
						{
							suitCount[trumpSuit] = suitCount[trumpSuit] - 4;
						}
						else if (calc.CalcProbPrim(new[] { "LT1 RT1 PT00", "LT1 RT00 PT1", "LT00 RT1 PT1" }, (int)trumpSuit, suitCount.OutCount(trumpSuit, hand), 0,
							(int)trumpSuit, 0, 0,
							handSize, true, true,
							handSize, true, true,
							handSize, true, true) > 0.5)
						{
							suitCount[trumpSuit] = suitCount[trumpSuit] - 3;
						}
						else if (suitCount.OutCount(trumpSuit, hand) >= 1)
						{
							suitCount[trumpSuit] = suitCount[trumpSuit] - 2;
						}
						else
						{
							suitCount[trumpSuit] = suitCount[trumpSuit] - 1;
						}
					}
					else
					{
						if (calc.CalcProbPrim(new[] { "LL1 RL1 PL1" }, (int)trumpSuit,
							suitCount.OutCount(trumpSuit, hand), 0, d.Hi((int)trumpSuit), suitCount.OutCount(d.Hi(trumpSuit), hand), 0,
							handSize, true, true, handSize, true, true, handSize, true, true) > 0.5)
						{
							suitCount[d.Hi(trumpSuit)] = suitCount[d.Hi(trumpSuit)] - 4;
						}
						else if (calc.CalcProbPrim(new[] { "LL1 RL1 PL00", "LL1 RL00 PL1", "LL00 RL1 PL1" }, (int)trumpSuit,
							suitCount.OutCount(trumpSuit, hand), 0, d.Hi((int)trumpSuit), suitCount.OutCount(d.Hi(trumpSuit), hand), 0,
							handSize, true, true, handSize, true, true, handSize, true, true) > 0.5)
						{
							suitCount[d.Hi(trumpSuit)] = suitCount[d.Hi(trumpSuit)] - 3;
						}
						else if ((suitCount.OutCount(d.Hi(trumpSuit), hand) >= 1))
						{
							suitCount[d.Hi(trumpSuit)] = suitCount[d.Hi(trumpSuit)] - 2;
						}
						else
						{
							suitCount[d.Hi(trumpSuit)] = suitCount[d.Hi(trumpSuit)] - 1;
						}
					}
					hand.Remove(d);
					oPlayed.Add(d);
					handSize--;
					domIdx++;
				}
				if (domIdx >= jaBones.Count) break;

				//'We had no sure winners
				if (domIdx == 0)
				{
					d = jaBones[domIdx];
					if (totProb * d.LeadProb <= 0.5) break;
					domIdx++;
					log.WriteVerbose(string.Format("  {0} {1}", d, d.LeadProb));
					totProb *= d.LeadProb;
					hand.Remove(d);
					oPlayed.Add(d);
					handSize--;
					if (d.IsA(trumpSuit))
					{
						if (suitCount.OutCount(trumpSuit, hand) > 5)
						{
							suitCount[trumpSuit] -= 4;
						} else if (suitCount.OutCount(trumpSuit, hand) > 3)
						{
							suitCount[trumpSuit] -= 3;
						} else if (suitCount.OutCount(trumpSuit, hand) >= 1)
						{
							suitCount[trumpSuit] -= 2;
						} else
						{
							suitCount[trumpSuit] -= 1;
						}
					}
					else
					{
						if (suitCount.OutCount(d.Hi(trumpSuit), hand) > 5)
						{
							suitCount[d.Hi(trumpSuit)] -= 4;
						} else if (suitCount.OutCount(d.Hi(trumpSuit), hand) > 3)
						{
							suitCount[d.Hi(trumpSuit)] -= 3;
						} else if (suitCount.OutCount(d.Hi(trumpSuit), hand) >= 1)
						{
							suitCount[d.Hi(trumpSuit)] -= 2;
						} else
						{
							suitCount[d.Hi(trumpSuit)] -= 1;
						}
					}
				}
			}
			log.WriteVerbose("  Cumulative probability: " + totProb);

			//'Don't re-eval bones unless we actually played some
			if (domIdx > 0 && handSize > 0)
			{
				jaBones = SortByProbWinLead(hand, trumpSuit, suitCount, oPlayed, handSize);
			}

			var oOffs = new List<Domino>();
			domIdx = 0;
			while (domIdx < handSize)
			{
				d = jaBones[domIdx];
				if ( d.Equals(new Domino("64")) && (origHand.Contains("66") && trumpSuit != SuitEnum.doubles) )
				{
					log.WriteVerbose("     " + d + " Special 64 exception when we also have 66.");
					domIdx++;
					continue;
				}
				log.WriteVerbose("    " + d + " " + d.LeadProb + " " + (d.CountValue() + 1));
				loss += d.CountValue() + 1;
				if (d.Hi(trumpSuit) == SuitEnum.sixes &&
					!(origHand.Contains("66") && trumpSuit != SuitEnum.doubles) &&
					!origHand.Contains("64") &&
					trumpSuit != SuitEnum.fours &&
					!oOffs.Contains(new Domino("64")))
				{
					log.WriteVerbose("      6 off -10");
					loss += 10;
					oOffs.Add(new Domino("64"));
				}
				else if (d.Hi(trumpSuit) == SuitEnum.fours &&
				  !(origHand.Contains("44") && trumpSuit != SuitEnum.doubles) &&
				  !origHand.Contains("64") &&
				  trumpSuit != SuitEnum.sixes &&
				  !oOffs.Contains(new Domino("64")))
				{
					log.WriteVerbose("      4 off -10");
					loss += 10;
					oOffs.Add(new Domino("64"));
				}
				else if (d.Hi(trumpSuit) == SuitEnum.fives &&
				  !origHand.Contains("55")  &&
				  !oOffs.Contains(new Domino("55")) &&
				  !oPlayed.Contains(new Domino("55")))
				{
					log.WriteVerbose("      5 off -10");
					loss += +10;
					oOffs.Add(new Domino("55"));
				}
				else if (d.Hi(trumpSuit) == SuitEnum.threes &&
				  !(origHand.Contains("33") && trumpSuit != SuitEnum.doubles) &&
				  !origHand.Contains("32") &&
				  trumpSuit != SuitEnum.twos &&
				  !oOffs.Contains(new Domino("32")))
				{
					log.WriteVerbose("      3 off -5");
					loss += 5;
					oOffs.Add(new Domino("32"));
				}
				else if ((d.CountValue() == 0))
				{
					garbage++;
					if (garbage % 3 == 0)
					{
						log.WriteVerbose("      Every third garbage dominoe -5");
						loss += +5;
					}
				}
				domIdx++;
			}
			log.WriteVerbose("    Expected loss = " + loss);

			return new BidVal { Loss = loss, Probability = totProb, Suit = trumpSuit, Low = false };
		}
		public BidVal BidEvalLow(Hand sourceHand, SuitEnum trumpSuit, ILog log)
		{
			if (trumpSuit != SuitEnum.followMe && trumpSuit != SuitEnum.doubles) throw new ArgumentException("trumpSuit must be follow me or doubles.");

			var hand = sourceHand.GetCopy();
			hand.SetTrump(trumpSuit);
			var aSuitCounts = new SuitCount(trumpSuit);
			int lTrumpSuit = (int)trumpSuit;
			var oPlayed = new List<Domino>();

			double dTotProb = 1;
			int lHandSize = 7;
			Domino d = null;
			Domino oLead = null;

			//'Find our best (worst) lead and play it
			var jaBones = new List<Domino>();
			foreach (Domino dom in hand)
			{
				if (dom.IsA(lTrumpSuit))
				{
					/*
				  'we lose if:
				  'oppleft has higher doubles and no lower doubles
				  'oppright has higher doubles and no lower doubles
					*/
					dom.LeadProb = CalcProbPrim(new[] { "LZ1 LT00", "RZ1 RT00" }, (int)lTrumpSuit,
						aSuitCounts.OutCount(trumpSuit, hand) - hand.HigherThanCount(dom, lTrumpSuit, oPlayed),
						hand.HigherThanCount(dom, lTrumpSuit, oPlayed), dom.Hi(lTrumpSuit), 0, 0,
						7, true, true, 7, true, true, 7, true, true);
				}
				else
				{
					/*
				  'we lose if:
				  'oppleft has higher lead and no lower lead
				  'oppright has higher lead and no lower lead
					*/
					dom.LeadProb = CalcProbPrim(new[] { "LH1 LL00", "RH1 RL00" }, (int)lTrumpSuit, 0, 0,
						dom.Hi(lTrumpSuit), aSuitCounts.OutCount(dom.Hi(trumpSuit), hand) - hand.HigherThanCount(dom, lTrumpSuit, oPlayed),
						hand.HigherThanCount(dom, lTrumpSuit, oPlayed),
						7, true, true, 7, true, true, 7, true, true);
				}
				dom.PlayNowProb = ProbLoseFollow(dom, lTrumpSuit, oPlayed, lHandSize, lHandSize, aSuitCounts, hand);
				jaBones.Add(dom);
			}
			jaBones = jaBones.OrderByDescending(x => x.LeadProb).ThenBy(x => x.PlayNowProb).ToList();

			d = jaBones[0];
			oLead = d;
			log.WriteVerbose("    " + d + " " + d.LeadProb + " " + d.PlayNowProb);
			log.WriteVerbose("    " + "Lose lead with " + d.LeadProb + " probability");
			if (d.IsA(lTrumpSuit))
			{
				if (CalcProbPrim(new[] { "LT1 RT1" }, lTrumpSuit,
					aSuitCounts.OutCount((SuitEnum)lTrumpSuit, hand), 0, lTrumpSuit, 0, 0,
					lHandSize, true, true, lHandSize, true, true, lHandSize, true, true) > 0.5)
				{
					aSuitCounts[lTrumpSuit] = aSuitCounts[lTrumpSuit] - 3;
				}
				else
				{
					aSuitCounts[lTrumpSuit] = aSuitCounts[lTrumpSuit] - 2;
				}
			}
			else
			{
				if (CalcProbPrim(new[] { "LL1 RL1" }, lTrumpSuit,
					aSuitCounts.OutCount(lTrumpSuit, hand), 0, d.Hi(lTrumpSuit), aSuitCounts.OutCount(d.Hi(lTrumpSuit), hand), 0,
					lHandSize, true, true, lHandSize, true, true, lHandSize, true, true) > 0.5)
				{
					aSuitCounts[d.Hi(lTrumpSuit)] = aSuitCounts[d.Hi(lTrumpSuit)] - 3;
					;
				}
				else
				{
					aSuitCounts[d.Hi(lTrumpSuit)] = aSuitCounts[d.Hi(lTrumpSuit)] - 2;
				}
			}
			hand.Remove(d);
			oPlayed.Add(d);
			lHandSize = lHandSize - 1;
			dTotProb = d.LeadProb;

			//'Take a snapshot of the 6 dominoes left in our hand
			var oOrigHand = hand.GetCopy();
			var aOrigSuitCounts = aSuitCounts.GetCopy();

			jaBones = new List<Domino>();

			int lD = 0;
			while (lD < oOrigHand.Count())
			{
				var p = oOrigHand[lD];
				int i = 0;
				while (i < hand.Count())
				{
					d = hand[i];
					var bHelps = false;
					if (p == d)
					{
						bHelps = false;
					}
					else if (lTrumpSuit == (int)SuitEnum.doubles)
					{
						if (p.IsA(SuitEnum.doubles))
						{
							bHelps = (d.IsA(SuitEnum.doubles) && (d.LeftDot < p.LeftDot));
						}
						else
						{
							bHelps = (!d.IsA(SuitEnum.doubles) &&
								(((d.LeftDot == p.LeftDot) && (d.RightDot < p.RightDot)) ||
									((d.RightDot == p.RightDot) && (d.LeftDot < p.LeftDot)) ||
										((d.LeftDot == p.RightDot) && (d.RightDot < p.LeftDot)) ||
											((d.RightDot == p.LeftDot) && (d.LeftDot < p.RightDot))));
						}
					}
					else
					{
						if (p.IsA(SuitEnum.doubles))
						{
							bHelps = (!d.IsA(SuitEnum.doubles) &&
								((d.LeftDot == p.LeftDot) ||
									(d.RightDot == p.RightDot) ||
										(d.LeftDot == p.RightDot) ||
											(d.RightDot == p.LeftDot)));
						}
						else
						{
							bHelps = (!d.IsA(SuitEnum.doubles) &&
								(((d.LeftDot == p.LeftDot) && (d.RightDot < p.RightDot)) ||
									((d.RightDot == p.RightDot) && (d.LeftDot < p.LeftDot)) ||
										((d.LeftDot == p.RightDot) && (d.RightDot < p.LeftDot)) ||
											((d.RightDot == p.LeftDot) && (d.LeftDot < p.RightDot))));
						}
					}
					if (bHelps)
					{
						if (d.IsA(lTrumpSuit))
						{
							if (CalcProbPrim(new[] { "LT1 RT1" }, lTrumpSuit,
								aSuitCounts.OutCount(lTrumpSuit, hand), 0, lTrumpSuit, 0, 0,
								lHandSize, true, true, lHandSize, true, true, lHandSize, true, true) > 0.5)
							{
								aSuitCounts[lTrumpSuit] = aSuitCounts[lTrumpSuit] - 3;
							}
							else
							{
								aSuitCounts[lTrumpSuit] = aSuitCounts[lTrumpSuit] - 2;
							}
						}
						else
						{
							if (CalcProbPrim(new[] { "LL1 RL1" }, lTrumpSuit,
								aSuitCounts.OutCount(lTrumpSuit, hand), 0, d.Hi(lTrumpSuit), aSuitCounts.OutCount(d.Hi(lTrumpSuit), hand), 0,
								lHandSize, true, true, lHandSize, true, true, lHandSize, true, true) > 0.5)
							{
								aSuitCounts[d.Hi(lTrumpSuit)] = aSuitCounts[d.Hi(lTrumpSuit)] - 3;
							}
							else
							{
								aSuitCounts[d.Hi(lTrumpSuit)] = aSuitCounts[d.Hi(lTrumpSuit)] - 2;
							}
						}
						hand.Remove(d);
						oPlayed.Add(d);
						lHandSize = lHandSize - 1;
					}
					else
					{
						i = i + 1;
					}
				}

				d = oOrigHand[lD];
				d.PlayNowProb = ProbLoseFollow(d, lTrumpSuit, oPlayed, lHandSize, lHandSize, aSuitCounts, hand);
				jaBones.Add(d);

				//'Restore hand to original state
				lHandSize = 6;
				hand = oOrigHand.GetCopy();
				oPlayed = new List<Domino>();
				aSuitCounts = aOrigSuitCounts.GetCopy();
				lD++;
			}

			jaBones = jaBones.OrderByDescending(x => x.PlayNowProb).ToList();
			lD = 0;
			var bFirst = true;
			int lDepth = 0;
			while (lD <= 5)
			{
				d = jaBones[lD];
				if (dTotProb * d.PlayNowProb > 0.5)
				{
					lDepth = lDepth + 1;
					dTotProb = dTotProb * d.PlayNowProb;
				}
				else
				{
					if ((bFirst))
					{
						bFirst = false;
						log.WriteVerbose("    Depth = " + lDepth);
					}
				}
				log.WriteVerbose("    " + d + " " + d.PlayNowProb + " " + dTotProb * d.PlayNowProb);
				if ((d.PlayNowProb <= 0.5))
				{
					break;
				}
				lD = lD + 1;
			}
			log.WriteVerbose("    Bones more than likely to win as followers = " + (6 - lD));

			hand.Add(oLead);
			var uRet = new BidVal() { Probability = dTotProb, Low = true, Suit = (SuitEnum)lTrumpSuit };
			if ((lD == 6) && (lDepth >= 4))
			{
				uRet.Loss = 0;
			}
			else
			{
				uRet.Loss = 42;
			}
			while (lD <= 5)
			{
				log.WriteVerbose("    " + d + " " + d.PlayNowProb + " " + dTotProb * d.PlayNowProb);
				d = jaBones[lD];
				lD = lD + 1;
			}
			return uRet;
		}

		public Bid GetBid(Player player, Table table, ILog log)
		{
			var hand = player.Hand;
			var least = new BidVal { Loss = 43, Suit = SuitEnum.none, Low = false };

			log.WriteVerbose(string.Format("{0}-{1} Bid Analysis: Low Follow Me", player.PlayerId, player.Name));
			hand.SetTrump(SuitEnum.followMe);
			var result = BidEvalLow(hand, SuitEnum.followMe, log);
			least = least.BestOf(result);

			log.WriteVerbose(string.Format("{0}-{1} Bid Analysis: Low Doubles", player.PlayerId, player.Name));
			hand.SetTrump(SuitEnum.followMe);
			result = BidEvalLow(hand, SuitEnum.doubles, log);
			least = least.BestOf(result);

			for (SuitEnum trumpSuit = SuitEnum.followMe; trumpSuit <= SuitEnum.doubles; trumpSuit++)
			{
				log.WriteVerbose(string.Format("{0}-{1} Bid Analysis: {2} as trump", player.PlayerId, player.Name, trumpSuit));
				hand.SetTrump(trumpSuit);
				if (trumpSuit == SuitEnum.followMe
					|| !table.CanPass
					|| (hand.SuitCount(trumpSuit) >= 3
						&& (hand.Contains(SuitOrders.Get(trumpSuit, 6))
							|| hand.Contains(SuitOrders.Get(trumpSuit, 5))))
					|| (hand.SuitCount(trumpSuit) == 2 && hand.Contains(SuitOrders.Get(trumpSuit, 6)))
				)
				{
					result = new BidCalc().BidEvalSuit(hand.GetCopy(), trumpSuit, log);
					least = least.BestOf(result);
				}
				else
				{
					log.WriteVerbose(string.Format("{0}-{1} Bid Analysis: Less than 2 or 3 and/or not enough high trumps", player.PlayerId, player.Name));
				}
			}

			// if we can pass && our hand sucks, do it.
			if (table.CanPass && (least.Loss > 12 || 42 - least.Loss < table.MinBid))
			{
				return new Bid {Amount = 0};
			}

			// if our hand is great, make the maximum bid, unless we are the last bidder
			if (least.Probability > 0.75 && least.Loss == 0 && table.BidCount < 3)
			{
				return new Bid {Amount = table.MaxBid, IsLow = least.Low, Trump = least.Suit};
			}

			var bid = new Bid { Amount = 0, IsLow = least.Low, Trump = least.Suit };
			if (table.CanPass)
			{
				if (table.BidCount == 3 && !least.Low) // Make lowest possible bid if ( we are last bidder
				{
					bid.Amount = table.MinBid;
				}
				else if (least.Loss <= 6 && least.Loss > 0)
				{
					bid.Amount = Math.Max(36, table.MinBid);
				}
				else
				{
					bid.Amount = 42 - least.Loss;
				}
			}
			else
			{
				if (least.Low)
				{
					bid.Amount = 42;
				}
				else
				{
					bid.Amount = table.MinBid;
				}
			}
			bid.Amount = Math.Max(table.MinBid, bid.Amount);

			return bid;
		}
	}
}
