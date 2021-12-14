using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FortyTwoLib
{
	/// <summary>
	/// Winner and leader are seat positions, not player ids
	/// </summary>
	public class Trick : IEnumerable<Domino>
	{
		private List<Domino> _dominoes;
		public int Winner { get; private set; }
		public int Leader { get; set; }

		public Trick(Domino d1, Domino d2, Domino d3, Domino d4, int winner, int leader)
		{
			_dominoes = new List<Domino> {d1, d2, d3, d4};
			Winner = winner;
			Leader = leader;
		}

		public Domino this[int idx]
		{
			get { return _dominoes[idx]; }
		}

		public int CountValue()
		{
			return _dominoes.Where(d => d != null).Sum(d => d.CountValue());
		}

		IEnumerator<Domino> IEnumerable<Domino>.GetEnumerator()
		{
			return _dominoes.GetEnumerator();
		}

		public override string ToString()
		{
			string ret = "";
			_dominoes.ForEach(d => ret = ret + string.Format("{0} ", d == null ? "  " : d.ToString()));
			return ret.TrimEnd();
		}

		public IEnumerator GetEnumerator()
		{
			return _dominoes.GetEnumerator();
		}
	}
}
