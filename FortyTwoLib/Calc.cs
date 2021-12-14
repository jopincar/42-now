using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FortyTwoLib.Logging;

namespace FortyTwoLib
{
	public struct HandSpec
	{
		public int zl;
		public int zh;
		public int tl;
		public int th;
		public int ll;
		public int lh;
		public int hl;
		public int hh;
	}


	public class Calc
	{
		private const int MAX_FACTOR = 7;
		private static int[,] _factors;

		static Calc()
		{
			_factors = new int[MAX_FACTOR + 1, MAX_FACTOR + 1];
			for (int n = 0; n <= MAX_FACTOR; n++)
			{
				for (int r = 0; r <= MAX_FACTOR; r++)
				{
					_factors[n, r] = SlowPermute(n, r);
				}
			}
		}

		private static int SlowPermute(int n, int r)
		{
			if (r > n) return 0;
			int ret = 1;
			while (r > 0)
			{
				ret *= n;
				n--;
				r--;
			}
			return ret;
		}

		public static int Perm(int n, int r)
		{
			return _factors[n, r];
		}

		public static int Fact(int n)
		{
			return _factors[n, n];
		}

		public static int Comb(int n, int r)
		{
			return _factors[n, r] / _factors[r, r];
		}

		/*
		'This routine computes the exact probability of a specified configuration
		'of trumps && leads in the other players' hands, taking into account
		'every piece of information the player could have without cheating.
		'Brute force was required to insure accuracy because almost all non-trivial
		'probabilities calculated in determining a play involve non-mutually
		'exclusive, non-independent events.
		'
		'This routine is based on the following formula where hn is the number
		'of dominoes in player n's hand, T is the total number of trumps, tn is
		'the number of trumps in a player n's hand, L is the total number of leads,
		'ln is the number of leads in player n's hand, O is the total number of
		'non-trump, non-leads, ln is the number of non-trump, non-leads in player
		'n's hand.

		'General Multinomial Coefficient formula counts the number of permutations
		'of n things of k different kinds, there being r1 of the first kind, r2
		'of the second kind,... && rk of the kth kind:
		'     n!
		'-------------
		'r1! r2!...rk!
		'
		'I "proved" the following through trial && error:
		'
		'(h0 + h1 + h2)!         h0!               h1!               h2!
		'--------------- = --------------- * --------------- * ---------------
		'Z! T! H! L! O!    z0!t0!h0!l0!o0!   z1!t1!h1!l1!o1!   z2!t2!h2!l2!o2!
		'
		'where:
		' t0 + t1 + t2 = T && l0 + l1 + l2 = L && o0 + o1 + o2 = O and
		' t0 + l0 + l2 = h0 && t1 + l1 + o1 = h1 && t2 + l2 + o2 = h2
		'
		' This formula is valuable because it allows us to determine the
		' probability that a player's hand contains any number of trump, lead, and
		' other dominoes, taking into account the interaction between all players'
		' hands, EG if ( player 1 has no trumps, ) { all other players are more
		' likely to have trumps, EG2 if ( I have a trump, ) { I am less likely to
		' have lead, etc.
		'
		' This routine works by generating all possible instances of the formula
		' above that meet known contraints about the composition of each player's
		' hand && ) { counting the total number of hands that are possible.  At
		' the same time, each possible instance of the formula above is compared
		' with the type of hand we are trying to determine the probability of
		' occuring.  if it matches, we add the number of hands for that
		' instance of the formula to the total number of hands that match.
		' The function returns total matching hands / total hands possible.
		*/
		public double CalcProbPrim(string[] aConjuncts,
			int lTrumpSuit, int lTrumpCount, int lHighTrumpCount,
			int lLeadSuit, int lLeadCount, int lHighLeadCount,
			int lBoneCount0, bool bHasTrump0, bool bHasLead0,
			int lBoneCount1, bool bHasTrump1, bool bHasLead1,
			int lBoneCount2, bool bHasTrump2, bool bHasLead2)
		{
			var aDivs = new Double[20];
			bool bMatch = false;
			if (lLeadSuit == lTrumpSuit)
			{
				lLeadCount = 0;
				lHighLeadCount = 0;
			}

			/*
			' Weird things can happen when we are doing bid analysis, so make sure
			' we don't have more dominoes in leads && trumps ) { are actually
			' left in the game
			*/
			int lOverage = lTrumpCount + lHighTrumpCount + lLeadCount + lHighLeadCount - lBoneCount0 - lBoneCount1 - lBoneCount2;
			if (lOverage > 0)
			{
				if (lOverage > lLeadCount)
				{
					lOverage = lOverage - lLeadCount;
					lLeadCount = 0;
					if (lOverage > lHighLeadCount)
					{
						lOverage = lOverage - lHighLeadCount;
						lHighLeadCount = 0;
						if (lOverage > lTrumpCount)
						{
							lOverage = lOverage - lTrumpCount;
							lTrumpCount = 0;
							if (lOverage > lHighTrumpCount)
							{
								lHighTrumpCount = 0;
							}
							else
							{
								lHighTrumpCount = lHighTrumpCount - lOverage;
							}
						}
						else
						{
							lTrumpCount = lTrumpCount - lOverage;
						}
					}
					else
					{
						lHighLeadCount = lHighLeadCount - lOverage;
					}
				}
				else
				{
					lLeadCount = lLeadCount - lOverage;
				}
			}

			int lOtherCount = lBoneCount0 + lBoneCount1 + lBoneCount2 - lTrumpCount - lHighTrumpCount - lLeadCount - lHighLeadCount;

			int lConjCount = aConjuncts.Length;

			var cs = new HandSpec[20, 3];
			for (int j = 0; j < aConjuncts.Length; j++)
			{
				for (int i = 0; i <= 2; i++)
				{
					cs[j, i].zl = 0;
					cs[j, i].zh = 9;
					cs[j, i].tl = 0;
					cs[j, i].th = 9;
					cs[j, i].ll = 0;
					cs[j, i].lh = 9;
					cs[j, i].hl = 0;
					cs[j, i].hh = 9;
					aDivs[j] = 1;
				}

				string sOrigCrit = aConjuncts[j].ToUpper();
				string sCrit = sOrigCrit + " ";

				if (sCrit.Contains("/"))
				{
					var split = sCrit.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
					aDivs[j] = Convert.ToDouble(split[1]);
					sCrit = split[0];
				}

				var players = sCrit.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string sToken in players)
				{
					string playerType = sToken.Substring(0, 1);
					int i = "LPR".IndexOf(playerType);
					if (i < 0) throw new ArgumentException(string.Format("Invalid player {0} indicated.", playerType));
					string boneType = sToken.Substring(1, 1);
					if (boneType == "T")
					{
						cs[j, i].tl = sToken[2] - '0';
						if (sToken.Length > 3)
						{
							cs[j, i].th = sToken[3] - '0';
						}
					}
					else if (boneType == "Z")
					{
						cs[j, i].zl = sToken[2] - '0';
						if (sToken.Length > 3)
						{
							cs[j, i].zh = sToken[3] - '0';
						}
					}
					else if (boneType == "H")
					{
						cs[j, i].hl = sToken[2] - '0';
						if (sToken.Length > 3)
						{
							cs[j, i].hh = sToken[3] - '0';
						}
					}
					else if (boneType == "L")
					{
						cs[j, i].ll = sToken[2] - '0';
						if (sToken.Length > 3)
						{
							cs[j, i].lh = sToken[3] - '0';
						}
					}
					else
					{
						throw new ArgumentException(string.Format("Invalid kind of bone {0} indicated", boneType));
					}
				}
			}

			int lIter = 0;
			double dTotHands = 0;
			double dMatchHands = 0;

			int z0 = 0;
			while ((z0 <= lHighTrumpCount) && (z0 == 0 || bHasTrump0) && (z0 <= lBoneCount0))
			{
				int z1 = 0;
				while ((z1 + z0 <= lHighTrumpCount) && (z1 == 0 || bHasTrump1) && (z1 <= lBoneCount1))
				{
					int z2 = 0;
					while ((z2 + z1 + z0 <= lHighTrumpCount) && (z2 == 0 || bHasTrump2) && (z2 <= lBoneCount2))
					{
						if ((z2 + z1 + z0 == lHighTrumpCount))
						{
							int t0 = 0;
							while ((t0 <= lTrumpCount) && (t0 == 0 || bHasTrump0) && (z0 + t0 <= lBoneCount0))
							{
								int t1 = 0;
								while ((t1 + t0 <= lTrumpCount) && (t1 == 0 || bHasTrump1) && (z1 + t1 <= lBoneCount1))
								{
									int t2 = 0;
									while ((t2 + t1 + t0 <= lTrumpCount) && (t2 == 0 || bHasTrump2) && (z2 + t2 <= lBoneCount2))
									{
										if (t2 + t1 + t0 == lTrumpCount)
										{
											int h0 = 0;
											while ((h0 <= lHighLeadCount) && (h0 == 0 || bHasLead0) && (z0 + t0 + h0 <= lBoneCount0))
											{
												int h1 = 0;
												while ((h1 + h0 <= lHighLeadCount) && (h1 == 0 || bHasLead1) && (z1 + t1 + h1 <= lBoneCount1))
												{
													int h2 = 0;
													while ((h2 + h1 + h0 <= lHighLeadCount) && (h2 == 0 || bHasLead2) && (z2 + t2 + h2 <= lBoneCount2))
													{
														if ((h2 + h1 + h0 == lHighLeadCount))
														{
															int l0 = 0;
															while ((l0 <= lLeadCount) && (l0 == 0 || bHasLead0) && (z0 + t0 + h0 + l0 <= lBoneCount0))
															{
																int l1 = 0;
																while ((l1 + l0 <= lLeadCount) && (l1 == 0 || bHasLead1) && (z1 + t1 + h1 + l1 <= lBoneCount1))
																{
																	int l2 = 0;
																	while ((l2 + l1 + l0 <= lLeadCount) && (l2 == 0 || bHasLead2) && (z2 + t2 + h2 + l2 <= lBoneCount2))
																	{
																		if ((l2 + l1 + l0 == lLeadCount))
																		{
																			int o0 = 0;
																			while ((o0 <= lOtherCount) && (z0 + t0 + h0 + l0 + o0 <= lBoneCount0))
																			{
																				int o1 = 0;
																				while ((o1 + o0 <= lOtherCount) && (z1 + t1 + h1 + l1 + o1 <= lBoneCount1))
																				{
																					int o2 = 0;
																					while ((o2 + o1 + o0 <= lOtherCount) && (z2 + t2 + h2 + l2 + o2 <= lBoneCount2))
																					{
																						lIter = lIter + 1;
																						if ((o2 + o1 + o0 == lOtherCount))
																						{
																							if ((z0 + t0 + l0 + o0 + h0 == lBoneCount0) &&
																								(z1 + t1 + l1 + o1 + h1 == lBoneCount1) &&
																									(z2 + t2 + l2 + o2 + h2 == lBoneCount2))
																							{

//'Compute the number of hands that have this config
double dHands = Fact(lBoneCount0) / (double)(Fact(z0) * Fact(t0) * Fact(l0) * Fact(o0) * Fact(h0)) *
	Fact(lBoneCount1) / (double)(Fact(z1) * Fact(t1) * Fact(l1) * Fact(o1) * Fact(h1)) *
	Fact(lBoneCount2) / (double)(Fact(z2) * Fact(t2) * Fact(l2) * Fact(o2) * Fact(h2));
dTotHands = dTotHands + dHands;
int j = 0;
while (j < lConjCount)
{
	int i = 0;
	bMatch = true;
	while (bMatch && (i <= 2))
	{
		switch (i)
		{
			case 0:
				bMatch = (t0 >= cs[j, i].tl) && (t0 <= cs[j, i].th) &&
					(z0 >= cs[j, i].zl) && (z0 <= cs[j, i].zh) &&
						(l0 >= cs[j, i].ll) && (l0 <= cs[j, i].lh) &&
							(h0 >= cs[j, i].hl) && (h0 <= cs[j, i].hh);
				break;
			case 1:
				bMatch = (t1 >= cs[j, i].tl) && (t1 <= cs[j, i].th) &&
					(z1 >= cs[j, i].zl) && (z1 <= cs[j, i].zh) &&
						(l1 >= cs[j, i].ll) && (l1 <= cs[j, i].lh) &&
							(h1 >= cs[j, i].hl) && (h1 <= cs[j, i].hh);
				break;
			case 2:
				bMatch = (t2 >= cs[j, i].tl) && (t2 <= cs[j, i].th) &&
					(z2 >= cs[j, i].zl) && (z2 <= cs[j, i].zh) &&
						(l2 >= cs[j, i].ll) && (l2 <= cs[j, i].lh) &&
							(h2 >= cs[j, i].hl) && (h2 <= cs[j, i].hh);
				break;
		}
		i++;
	}
	if (bMatch) break;
	j++;
}
if (bMatch)
{
	dMatchHands = dMatchHands + (dHands / aDivs[j]);
}
																							}
																						}
																						o2 = o2 + 1;
																					}
																					o1 = o1 + 1;
																				}
																				o0 = o0 + 1;
																			}
																		}
																		l2 = l2 + 1;
																	}
																	l1 = l1 + 1;
																}
																l0 = l0 + 1;
															}
														}
														h2 = h2 + 1;
													}
													h1 = h1 + 1;
												}
												h0 = h0 + 1;
											}
										}
										t2 = t2 + 1;
									}
									t1 = t1 + 1;
								}
								t0 = t0 + 1;
							}
						}
						z2 = z2 + 1;
					}
					z1 = z1 + 1;
				}
				z0 = z0 + 1;
			}

			if ((dTotHands == 0)) return 0;

			double ret = dMatchHands / dTotHands;
		    if ( ret < 0 || ret > 1 ) throw new ApplicationException(string.Format("Invalid probability returned: {0}", ret));
			return ret;

		}

		public double ProbWinLeadPrim(int lTrumpSuit, int lTrumpSuitCount, int lHighTrumpCount, 
			int lLeadSuit, int lLeadCount, int lHighLeadCount, 
			int lBoneCount0, bool bHasTrump0, bool bHasLead0, 
			int lBoneCount1, bool bHasTrump1, bool bHasLead1, 
			int lBoneCount2, bool bHasTrump2, bool bHasLead2)
		{
			double dProb;

			if( lTrumpSuit == lLeadSuit )
			{
				/*
				'We win if:
				'Neither opponent has higher trump
				'All three other players have higher trump / 3
				'Partner has higher trump, oppright has higher trump, oppleft has no higher trump / 2
				'Partner has higher trump, oppleft has higher trump, oppright has no higher trump / 2
				*/
				dProb = CalcProbPrim(new[] {"LZ00 RZ00", "PZ1 LZ1 RZ1 /3", "PZ1 RZ1 LZ00 /2", "PZ1 LZ1 RZ00 /2"},
					lTrumpSuit, lTrumpSuitCount, lHighTrumpCount,
					lLeadSuit, lLeadCount, lHighLeadCount,
					lBoneCount0, bHasTrump0, bHasLead0,
					lBoneCount1, bHasTrump1, bHasLead1,
					lBoneCount2, bHasTrump2, bHasLead2);
			}
			else
			{
				/*
				'We lose if:
				'oppleft has higher lead and partner has no higher lead and has lower lead
				'oppleft has higher lead and partner has no higher lead and has no trump
				'oppright has higher lead and partner has no higher lead and has lower lead
				'oppright has higher lead and partner has no higher lead and has no trump
				'oppleft has trump and has no lead and partner has no trump
				'oppleft has trump and has no lead and partner has lead
				'oppright has trump and has no lead and partner has no trump
				'oppright has trump and has no lead and partner has lead
				'oppleft has higher lead and partner has higher lead and oppright has only lower lead / 2
				'oppright has higher lead and partner has higher lead and oppleft has only lower lead / 2
				'oppleft has higher lead and partner has higher lead and oppright has no trump and no higher lead / 2
				'oppright has higher lead and partner has higher lead and oppleft has no trump and no higher lead / 2
				'oppleft, oppright, and partner have higher lead / 1.5
				'oppleft, oppright, and partner have trump and no lead / 1.5
				*/
				dProb = CalcProbPrim(new[] {
					"LH1 PH00 PL1",
					"LH1 PH00 PT00",
					"RH1 PH00 PL1",
					"RH1 PH00 PT00",
					"LT1 LH00 LL00 PT00",
					"LT1 LH00 LL00 PL1",
					"LT1 LH00 LL00 PH1",
					"RT1 RH00 RL00 PT00",
					"RT1 RH00 RL00 PL1",
					"RT1 RH00 RL00 PH1",
					"LH1 PH1 RL1 RH00 /2",
					"RH1 PH1 LL1 LH00 /2",
					"LH1 PH1 RT00 RH00 /2",
					"RH1 PH1 LT00 LH00 /2",
					"LH1 RH1 PH1 /1.5",
					"LT1 LH00 LL00 RT1 RH00 RL00 PT1 PH00 PL00 /1.5"
					},
					lTrumpSuit, lTrumpSuitCount, lHighTrumpCount,
					lLeadSuit, lLeadCount, lHighLeadCount,
					lBoneCount0, bHasTrump0, bHasLead0,
					lBoneCount1, bHasTrump1, bHasLead1,
					lBoneCount2, bHasTrump2, bHasLead2);
				dProb = 1 - dProb;
			}
			return dProb;
		}

		public double CalcProbFollow(string[] aConjuncts,
			int lLeadSuit, int lLeadCount, int lLowLeadCount, int lLeadLeadCount,
			int lBoneCount0, bool bHasLead0,
			int lBoneCount1, bool bHasLead1,
			int lBoneCount2, bool bHasLead2)
		{
			var aDivs = new Double[20];
			bool bMatch = false;

			/*
			' Weird things can happen when we are doing bid analysis, so make sure
			' we don't have more dominoes in leads && trumps ) { are actually
			' left in the game
			*/
			int lOverage = lLeadLeadCount + lLeadCount + lLowLeadCount - lBoneCount0 - lBoneCount1 - lBoneCount2;
			if (lOverage > 0)
			{
				if (lOverage > lLeadCount)
				{
					lOverage = lOverage - lLeadCount;
					lLeadCount = 0;
					if (lOverage > lLowLeadCount)
					{
						lOverage = lOverage - lLowLeadCount;
						lLowLeadCount = 0;
						if (lOverage > lLeadLeadCount)
						{
							lOverage = lOverage - lLeadLeadCount;
							lLeadLeadCount = 0;
						}
						else
						{
							lLeadLeadCount = lLeadLeadCount - lOverage;
						}
					}
					else
					{
						lLowLeadCount = lLowLeadCount - lOverage;
					}
				}
				else
				{
					lLeadCount = lLeadCount - lOverage;
				}
			}

			int lOtherCount = lBoneCount0 + lBoneCount1 + lBoneCount2 - lLeadLeadCount - lLeadCount - lLowLeadCount;

			int lConjCount = aConjuncts.Length;

			var cs = new HandSpec[20, 3];
			for (int j = 0; j < aConjuncts.Length; j++)
			{
				for (int i = 0; i <= 2; i++)
				{
					cs[j, i].zl = 0;
					cs[j, i].zh = 9;
					cs[j, i].tl = 0;
					cs[j, i].th = 9;
					cs[j, i].ll = 0;
					cs[j, i].lh = 9;
					cs[j, i].hl = 0;
					cs[j, i].hh = 9;
					aDivs[j] = 1;
				}

				string sOrigCrit = aConjuncts[j].ToUpper();
				string sCrit = sOrigCrit + " ";

				if (sCrit.Contains("/"))
				{
					var split = sCrit.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
					aDivs[j] = Convert.ToDouble(split[1]);
					sCrit = split[0];
				}

				var players = sCrit.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string sToken in players)
				{
					string playerType = sToken.Substring(0, 1);
					int i = "LPR".IndexOf(playerType);
					if (i < 0) throw new ArgumentException(string.Format("Invalid player {0} indicated.", playerType));
					string boneType = sToken.Substring(1, 1);
					if (boneType == "T")
					{
						cs[j, i].tl = sToken[2] - '0';
						if (sToken.Length > 3)
						{
							cs[j, i].th = sToken[3] - '0';
						}
					}
					else if (boneType == "Z")
					{
						cs[j, i].zl = sToken[2] - '0';
						if (sToken.Length > 3)
						{
							cs[j, i].zh = sToken[3] - '0';
						}
					}
					else if (boneType == "H")
					{
						cs[j, i].hl = sToken[2] - '0';
						if (sToken.Length > 3)
						{
							cs[j, i].hh = sToken[3] - '0';
						}
					}
					else if (boneType == "L")
					{
						cs[j, i].ll = sToken[2] - '0';
						if (sToken.Length > 3)
						{
							cs[j, i].lh = sToken[3] - '0';
						}
					}
					else
					{
						throw new ArgumentException(string.Format("Invalid kind of bone {0} indicated", boneType));
					}
				}
			}

			int lIter = 0;
			double dTotHands = 0;
			double dMatchHands = 0;

			int t0 = 0;
			while ((t0 <= lLeadLeadCount) && (t0 == 0 || bHasLead0) && (t0 <= lBoneCount0))
			{
				int t1 = 0;
				while ((t1 + t0 <= lLeadLeadCount) && (t1 == 0 || bHasLead1) && (t1 <= lBoneCount1))
				{
					int t2 = 0;
					while ((t2 + t1 + t0 <= lLeadLeadCount) && (t2 == 0 || bHasLead2) && (t2 <= lBoneCount2))
					{
						if (t2 + t1 + t0 == lLeadLeadCount)
						{
							int h0 = 0;
							while ((h0 <= lLowLeadCount) && (h0 == 0 || bHasLead0) && (t0 + h0 <= lBoneCount0))
							{
								int h1 = 0;
								while ((h1 + h0 <= lLowLeadCount) && (h1 == 0 || bHasLead1) && (t1 + h1 <= lBoneCount1))
								{
									int h2 = 0;
									while ((h2 + h1 + h0 <= lLowLeadCount) && (h2 == 0 || bHasLead2) && (t2 + h2 <= lBoneCount2))
									{
										if ((h2 + h1 + h0 == lLowLeadCount))
										{
											int l0 = 0;
											while ((l0 <= lLeadCount) && (l0 == 0 || bHasLead0) && (t0 + h0 + l0 <= lBoneCount0))
											{
												int l1 = 0;
												while ((l1 + l0 <= lLeadCount) && (l1 == 0 || bHasLead1) && (t1 + h1 + l1 <= lBoneCount1))
												{
													int l2 = 0;
													while ((l2 + l1 + l0 <= lLeadCount) && (l2 == 0 || bHasLead2) && (t2 + h2 + l2 <= lBoneCount2))
													{
														if ((l2 + l1 + l0 == lLeadCount))
														{
															int o0 = 0;
															while ((o0 <= lOtherCount) && (t0 + h0 + l0 + o0 <= lBoneCount0))
															{
																int o1 = 0;
																while ((o1 + o0 <= lOtherCount) && (t1 + h1 + l1 + o1 <= lBoneCount1))
																{
																	int o2 = 0;
																	while ((o2 + o1 + o0 <= lOtherCount) && (t2 + h2 + l2 + o2 <= lBoneCount2))
																	{
																		lIter = lIter + 1;
																		if ((o2 + o1 + o0 == lOtherCount))
																		{
																			if ((t0 + l0 + o0 + h0 == lBoneCount0) &&
																				(t1 + l1 + o1 + h1 == lBoneCount1) &&
																				(t2 + l2 + o2 + h2 == lBoneCount2))
																			{

																				//'Compute the number of hands that have this config
																				double dHands = Fact(lBoneCount0) / (double)(Fact(t0) * Fact(l0) * Fact(o0) * Fact(h0)) *
																					Fact(lBoneCount1) / (double)(Fact(t1) * Fact(l1) * Fact(o1) * Fact(h1)) *
																					Fact(lBoneCount2) / (double)(Fact(t2) * Fact(l2) * Fact(o2) * Fact(h2));
																				dTotHands = dTotHands + dHands;
																				int j = 0;
																				while (j < lConjCount)
																				{
																					int i = 0;
																					bMatch = true;
																					while (bMatch && (i <= 2))
																					{
																						switch (i)
																						{
																							case 0:
																								bMatch = (t0 >= cs[j, i].tl) && (t0 <= cs[j, i].th) &&
																										(l0 >= cs[j, i].ll) && (l0 <= cs[j, i].lh) &&
																										(h0 >= cs[j, i].hl) && (h0 <= cs[j, i].hh);
																								break;
																							case 1:
																								bMatch = (t1 >= cs[j, i].tl) && (t1 <= cs[j, i].th) &&
																										(l1 >= cs[j, i].ll) && (l1 <= cs[j, i].lh) &&
																											(h1 >= cs[j, i].hl) && (h1 <= cs[j, i].hh);
																								break;
																							case 2:
																								bMatch = (t2 >= cs[j, i].tl) && (t2 <= cs[j, i].th) &&
																										(l2 >= cs[j, i].ll) && (l2 <= cs[j, i].lh) &&
																											(h2 >= cs[j, i].hl) && (h2 <= cs[j, i].hh);
																								break;
																						}
																						i++;
																					}
																					if (bMatch) break;
																					j++;
																				}
																				if (bMatch)
																				{
																					dMatchHands = dMatchHands + (dHands / aDivs[j]);
																				}
																			}
																		}
																		o2 = o2 + 1;
																	}
																	o1 = o1 + 1;
																}
																o0 = o0 + 1;
															}
														}
														l2 = l2 + 1;
													}
													l1 = l1 + 1;
												}
												l0 = l0 + 1;
											}
										}
										h2 = h2 + 1;
									}
									h1 = h1 + 1;
								}
								h0 = h0 + 1;
							}
						}
						t2 = t2 + 1;
					}
					t1 = t1 + 1;
				}
				t0 = t0 + 1;
			}

			if ((dTotHands == 0)) return 0;

			double ret = dMatchHands / dTotHands;
			if (ret < 0 || ret > 1) throw new ApplicationException(string.Format("Invalid probability returned: {0}", ret));
			return ret;

		}

		public double ProbLoseFollow(Domino d, int lTrumpSuit, List<Domino> oPlayed, int lLeftHandSize, int lRightHandSize,
			SuitCount aSuitcounts, Hand hand)
		{
			//'Blanks can't lose as a following bone because nobody can lead them
			if (d.Lo(lTrumpSuit) == 0)
			{
				return 1;
			}
			var lLeadSuit = d.Hi(lTrumpSuit);
			//'Determine how many bones lower than this one lead in this suit
			var lLeadLead = hand.LowerThanLeadCount(d, lTrumpSuit, oPlayed, false);
			var lLowLead = hand.LowerThanCount(d, lTrumpSuit, oPlayed, false);
			/*
			'oppLeft has hower leadable lead and oppRight has lower leadable lead
			'oppLeft has lower leadable lead and oppright has lower lead
			'oppLeft has lower leadable lead and oppright has no lead
			'oppRight has lower leadable lead and oppleft has lower lead
			'oppRight has lower leadable lead and oppleft has no lead
			*/
			var dHiProb = CalcProbFollow(new[] { "LT1 RT1", "LT1 RH1", "LT1 RT00 RH00 RL00", "RT1 LH1", "RT1 LT00 LH00 LL00" },
				lLeadSuit, aSuitcounts.OutCount((SuitEnum)lLeadSuit, hand) - lLowLead, lLowLead - lLeadLead, lLeadLead,
				lLeftHandSize, true, 7, true, lRightHandSize, true);
			if (dHiProb < 0 || dHiProb > 1)
			{
				throw new ApplicationException(string.Format("dHiProb invalid: {0}", dHiProb));
			}
			double dLoProb = 0;
			if (!d.IsA(SuitEnum.doubles))
			{
				lLeadSuit = d.Lo(lTrumpSuit);
				//'Determine how many bones lower than this one lead in this suit
				lLeadLead = hand.LowerThanLeadCount(d, lTrumpSuit, oPlayed, true);
				lLowLead = hand.LowerThanCount(d, lTrumpSuit, oPlayed, true);
				/*
				'oppLeft has hower leadable lead and oppRight has lower leadable lead
				'oppLeft has lower leadable lead and oppright has lower lead
				'oppLeft has lower leadable lead and oppright has no lead
				'oppRight has lower leadable lead and oppleft has lower lead
				'oppRight has lower leadable lead and oppleft has no lead
				*/
				dLoProb = CalcProbFollow(new[] { "LT1 RT1", "LT1 RH1", "LT1 RT00 RH00 RL00", "RT1 LH1", "RT1 LT00 LH00 LL00" },
					lLeadSuit, aSuitcounts.OutCount((SuitEnum)lLeadSuit, hand) - lLowLead, lLowLead - lLeadLead, lLeadLead,
					lLeftHandSize, true, 7, true, lRightHandSize, true);
				if (dLoProb < 0 || dLoProb > 1)
				{
					throw new ApplicationException(string.Format("dLoProb invalid: {0}", dLoProb));
				}
			}
			double dProb = 1 - (dHiProb + dLoProb);
			if (dProb < 0) dProb = 0;
			if (dProb < 0 || dProb > 1)
			{
				throw new ApplicationException(string.Format("dProb invalid: {0}", dProb));
			}
			return dProb;
		}


	}
}