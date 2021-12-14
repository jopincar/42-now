using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FortyTwoLib
{
	public class SuitOrders
	{
		private static Dictionary<SuitEnum, Domino[]> _suits;

		static SuitOrders()
		{
			_suits = new Dictionary<SuitEnum, Domino[]>();
			for ( SuitEnum suit = SuitEnum.blanks; suit <= SuitEnum.doubles; suit++ )
			{
				_suits[suit] = new Domino[7];
				for ( int i = 0; i < 7; i++ )
				{
					if( suit == SuitEnum.doubles )
					{
						_suits[suit][i] = new Domino(i.ToString() + i.ToString());
					}
					else
					{
						var suitVal = (int) suit;
						if( i == 6 )
						{
							_suits[suit][i] = new Domino(suitVal.ToString() + suitVal.ToString());
						}
						else
						{
							_suits[suit][i] = new Domino(suitVal.ToString() + (i >= suitVal ? i + 1 : i).ToString());
						}
					}
				}
			}
		}

		public static Domino Get(int suit, int ordinal)
		{
			return Get((SuitEnum) suit, ordinal);
		}

		public static Domino Get(SuitEnum suit, int ordinal)
		{
			return _suits[suit][ordinal];
		}
	}
}
