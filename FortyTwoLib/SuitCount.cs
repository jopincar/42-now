using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FortyTwoLib
{
	public class SuitCount
	{
		private int[] _suitCount;
		private SuitEnum _trump;

		public SuitCount(SuitEnum trumpSuit)
		{
			_trump = trumpSuit;
			_suitCount = new int[8];
			for (int i = 0; i <= 7; i++)
			{
				_suitCount[i] = trumpSuit == SuitEnum.followMe 
						|| i == (int) trumpSuit 
						|| i == (int) SuitEnum.doubles 
					? 7 
					: 6;
			}
		}

		public int OutCount(int suit, Hand hand)
		{
			return OutCount((SuitEnum) suit, hand);
		}

		public int OutCount(SuitEnum suit, Hand hand)
		{
			return suit == SuitEnum.followMe 
				? 0 
				: _suitCount[(int) suit] - hand.SuitCount(suit);
		}

		public int this[SuitEnum suit]
		{
			get { return _suitCount[(int) suit]; }
			set { _suitCount[(int) suit] = value; }
		}

		public int this[int suit]
		{
			get { return _suitCount[suit]; }
			set { _suitCount[suit] = value; }
		}

		public SuitCount GetCopy()
		{
			var ret = new SuitCount(_trump);
			ret._suitCount = new int[8];
			Array.Copy(_suitCount, ret._suitCount, 8);
			return ret;
		}
	}
}
