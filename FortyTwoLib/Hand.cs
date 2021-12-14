using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FortyTwoLib
{
	public class Hand : IEnumerable<Domino>
	{
		private List<Domino> _dominoes = new List<Domino>();
		private int _trump;
		private int[] _suitCounts = new int[SuitEnum.doubles - SuitEnum.blanks + 1];

		public Hand()
		{
		}

		public Hand(string dotsList)
		{
			dotsList.Split(new[] { ',', ' ' }).ToList().ForEach(d => this.Add(new Domino(d)));
		}

		public void SetTrump(SuitEnum suit)
		{
			_trump = (int) suit;
			for (int i = 0; i < _suitCounts.Length; i++) _suitCounts[i] = 0;
			_dominoes.ForEach(d => UpdateSuiteCounts(d, 1));
		}

		private string MakeKey(string key)
		{
			if ( key[0] > key[1] ) return key;
			return key.Substring(1, 1) + key.Substring(0, 1);
		}

		public Domino this[string key]
		{
			get
			{
				key = MakeKey(key);
				return _dominoes.SingleOrDefault(d => d.ToString() == key);
			}
		}

		public Domino this[int idx]
		{
			get { return _dominoes[idx]; }
		}


		public bool Contains(string key)
		{
			return this[key] != null;
		}

		public void Add(Domino d)
		{
			_dominoes.Add(d);
			UpdateSuiteCounts(d, 1);
		}

		public void Remove(Domino d)
		{
			if ( _dominoes.Remove(d) ) UpdateSuiteCounts(d, -1);
		}

		public int SuitCount(SuitEnum suit)
		{
			return SuitCount((int) suit);
		}

		public int SuitCount(int suit)
		{
			if ( suit == (int) SuitEnum.followMe ) return 0;
			return _suitCounts[suit];
		}

		private void UpdateSuiteCounts(Domino d, int addend)
		{
			if (d.IsA(_trump))
			{
				_suitCounts[_trump] += addend;
				return;
			}
			_suitCounts[d.Hi(_trump)] += addend;
			if (!d.IsA((int)SuitEnum.doubles)) _suitCounts[d.Lo(_trump)] += addend;
		}

		IEnumerator<Domino> IEnumerable<Domino>.GetEnumerator()
		{
			return _dominoes.GetEnumerator();
		}

		public override string ToString()
		{
			string ret = "";
			_dominoes.ForEach(d => ret += d.ToString() + " ");
			return ret.TrimEnd();
		}

		public IEnumerator GetEnumerator()
		{
			return _dominoes.GetEnumerator();
		}

		public Domino Find(string dots)
		{
			var target = new Domino(dots);
			var domino = _dominoes.SingleOrDefault(d => d.Equals(target));
			if (domino == null) throw new DominoNotFoundException(string.Format("{0} not found in this hand.", dots));
			return domino;
		}

		public void Flip(string dots)
		{
			Find(dots).Flip();
		}

		public void Move(string dots, Point position)
		{
			Find(dots).Position = position;
		}

		public void Remove(string dots)
		{
			this.Remove(Find(dots));
		}

		public List<Domino> MustPlay(Domino lead, int trump)
		{
			if (lead == null) return new List<Domino>(); // Leader can play anything

			var leadSuit = lead.IsA(trump) ? trump : lead.Hi(trump);
			var mustPlay = new List<Domino>();
			if (lead.IsA(SuitEnum.doubles) && trump == (int)SuitEnum.doubles)
			{
				mustPlay.AddRange(this.Where(d => d.IsA(SuitEnum.doubles)));
			}
			else if (lead.IsA(trump))
			{
				mustPlay.AddRange(this.Where(d => d.IsA(trump)));
			}
			else
			{
				mustPlay.AddRange(this.Where(d => d.IsA(leadSuit) && !d.IsA(trump)));
			}
			return mustPlay;
		}

		public string CanPlay(string dots, Domino lead, int trump)
		{
			var domino = Find(dots);
			if( domino == null ) return string.Format("{0} not found in your hand.", dots);

			var mustPlay = MustPlay(lead, trump);
			if( mustPlay.Count > 0 && !mustPlay.Any(d => d.Equals(domino)) )
			{
				return "You must play other dominos in your hand.";
			}

			return "";
		}

		public Domino Lowest(SuitEnum suit, SuitEnum trump, bool isLow)
		{
			Domino d = null;
			foreach (Domino dom in this)
			{
				if (dom.IsA(suit))
				{
					if (d == null)
					{
						d = dom;
					}
					else
					{
						if (d.GreaterThan(dom, dom, (int)trump, isLow)) d = dom;
					}
				}
			}
			return d;
		}

		public int HigherThanCount(Domino d, SuitEnum trump, List<Domino> oPlayed)
		{
			return HigherThanCount(d, (int) trump, oPlayed);
		}

		public int HigherThanCount(Domino d, int lTrump, List<Domino> oPlayed)
		{
			/*
			'Returns the number of dominoes not in my hand in the same suit as
			'd that are greater than d
			'NOTE: does not consider dominoes currently in play
			*/

			int lSuit;
			int i;
			int lHigher;
			if (d.IsA(SuitEnum.doubles) && lTrump == (int)SuitEnum.doubles)
			{
				lSuit = (int)SuitEnum.doubles;
			}
			else
			{
				lSuit = d.Hi(lTrump);
			}

			i = 6;
			lHigher = 0;
			while (i >= 0 && !SuitOrders.Get((SuitEnum)lSuit, i).Equals(d))
			{
				if (!this.Contains(SuitOrders.Get((SuitEnum)lSuit, i)) &&
					!oPlayed.Contains(SuitOrders.Get((SuitEnum)lSuit, i)) &&
						!((SuitOrders.Get((SuitEnum)lSuit, i).IsA(lTrump) && (lSuit != lTrump))))
				{
					lHigher++;
				}
				i--;
			}
			return lHigher;
		}

		public Hand GetCopy()
		{
			var ret = new Hand();
			ret.SetTrump((SuitEnum) _trump);
			_dominoes.ForEach(d => ret.Add(new Domino(d.ToString())));
			return ret;
		}

		public int LowerThanCount(Domino d, int lTrump, List<Domino> oPlayed, bool bLo)
		{
			return LowerThanCount(d, (SuitEnum) lTrump, oPlayed, bLo);
		}

		public int LowerThanCount(Domino d, SuitEnum lTrump, List<Domino> oPlayed, bool bLo)
		{
			/*'Returns the number of dominoes not in my hand in the same suit as
		  'd that are less than d
		  'NOTE: does not consider dominoes currently in play
		   */

			SuitEnum lSuit;
			if (d.IsA(SuitEnum.doubles) && lTrump == SuitEnum.doubles)
			{
				lSuit = SuitEnum.doubles;
			}
			else
			{
				if (bLo)
				{
					lSuit = d.Lo(lTrump);
				}
				else
				{
					lSuit = d.Hi(lTrump);
				}
			}

			int i = 0;
			int lLower = 0;
			while ((i <= 6) && (SuitOrders.Get(lSuit, i) != d))
			{
				if (!this.Contains(SuitOrders.Get(lSuit, i)) &&
					(!oPlayed.Contains(SuitOrders.Get(lSuit, i))) &&
						!(SuitOrders.Get(lSuit, i).IsA(lTrump) && (lSuit != lTrump)))
				{
					lLower++;
				}
				i++;
			}

			return lLower;
		}

		public int LowerThanLeadCount(Domino d, int lTrump, List<Domino> oPlayed, bool bLo)
		{
			return LowerThanLeadCount(d, (SuitEnum) lTrump, oPlayed, bLo);
		}

		public int LowerThanLeadCount(Domino d, SuitEnum lTrump, List<Domino> oPlayed, bool bLo)
		{
			/*
		  'Returns the number of dominoes not in my hand in the same suit as
		  'd that are less than d and can lead as the specified suit
		  'NOTE: does not consider dominoes currently in play
			*/

			SuitEnum lSuit;
			if( d.IsA(SuitEnum.doubles) && lTrump == SuitEnum.doubles )
			{
				lSuit = SuitEnum.doubles;
			}
			else
			{
				if( bLo )
				{
					lSuit = d.Lo(lTrump);
				}
				else
				{
					lSuit = d.Hi(lTrump);
				}
			}

			int i = 0;
			int lLower = 0;
			while ( (i <= 6) && (SuitOrders.Get(lSuit, i) != d) )
			{
				if( !this.Contains(SuitOrders.Get(lSuit, i)) &&
					(!oPlayed.Contains(SuitOrders.Get(lSuit, i))) &&
						((lSuit == SuitEnum.doubles) || (SuitOrders.Get(lSuit, i).Lo(lSuit) <= lSuit)) &&
							!(SuitOrders.Get(lSuit, i).IsA(lTrump) && (lSuit != lTrump)) )
				{
					lLower++;
				}
				i++;
			}
			return lLower;
		}

		public Domino Highest(int lSuit, Table table)
		{
			Domino ret = null;
			foreach ( Domino d in this )
			{
				if ( d.IsA(lSuit) )
				{
					if ( ret == null )
					{
						ret = d;
					} else
					{
						if ( d.GreaterThan(ret, ret, table.Trump, table.WinningBid.IsLow) ) ret = d;
					}
				}
			}
			return ret;
		}

		public Domino Lowest(int lSuit, Table table)
		{
			Domino ret = null;
			foreach (Domino d in this)
			{
				if (d.IsA(lSuit))
				{
					if (ret == null)
					{
						ret = d;
					}
					else
					{
						if (ret.GreaterThan(d, d, table.Trump, table.WinningBid.IsLow)) ret = d;
					}
				}
			}
			return ret;
		}
	}
}