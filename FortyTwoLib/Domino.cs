using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FortyTwoLib
{
	public enum SuitEnum
	{
		none = -2,
		followMe = -1,
		blanks = 0,
		ones = 1,
		twos = 2,
		threes = 3,
		fours = 4,
		fives = 5,
		sixes = 6,
		doubles = 7,
	} 

	public class Domino
	{
		private List<int> _dots;

		public double PlayNowProb { get; set; }
		public double LeadProb { get; set; }
		public string SortKey { get; set; }

		public Domino(string dots)
		{
			_dots = new List<int> {0, 0};
			if ( dots[0] > dots[1] )
			{
				_dots[0] = dots[0] - '0';
				_dots[1] = dots[1] - '0';
			} else
			{
				_dots[0] = dots[1] - '0';
				_dots[1] = dots[0] - '0';
			}
			if ( _dots[0] > 6 || _dots[0] < 0 ) throw new ArgumentOutOfRangeException("dots", "Must be between 0 and 6");
			if ( _dots[1] > 6 || _dots[1] < 0) throw new ArgumentOutOfRangeException("dots", "Must be between 0 and 6");
		}

		public int CountValue()
		{
			int sum = _dots.Sum();
			return sum % 5 == 0 ? sum : 0;
		}

		public override string ToString()
		{
			return _dots[0].ToString() + _dots[1].ToString();
		}

		public string FlippedString()
		{
			return _dots[1].ToString() + _dots[0].ToString();
		}

		public override bool Equals(object obj)
		{
			var other = obj as Domino;
			if ( other == null ) return false;
			return other.ToString() == this.ToString() || other.ToString() == this.FlippedString();
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public bool IsA(SuitEnum suit)
		{
			return IsA((int) suit);
		}

		public bool IsA(int suit)
		{
			if ( suit == (int) SuitEnum.followMe ) return false;
			if ( suit == (int) SuitEnum.doubles ) return _dots[0] == _dots[1];
			return _dots.Contains((int) suit);
		}

		public SuitEnum Hi(SuitEnum trump)
		{
			return (SuitEnum) Hi((int) trump);
		}

		public int Hi(int trump)
		{
			if ( this.IsA(trump) )
			{
				if ( trump == (int) SuitEnum.doubles ) return _dots[0];
				return trump;
			}
			return _dots.Max();
		}

		public SuitEnum Lo(SuitEnum trump)
		{
			return (SuitEnum) Lo((int) trump);
		}

		public int Lo(int trump)
		{
			if ( this.IsA(trump) )
			{
				if ( _dots[0] == trump ) return _dots[1];
				return _dots[0];
			}
			return _dots.Min();
		}

		public bool GreaterThan(Domino other, Domino lead, int trump, bool low)
		{
			int leadSuit = lead.Hi(trump);

			if ( low ) // Only follow me or doubles can be bid
			{
				if ( lead.IsA(trump) )
				{
					if ( this.IsA(trump) && other.IsA(trump) ) return this.Lo(trump) > other.Lo(trump);
					return this.IsA(trump);
				} else
				{
					if ( this.IsA(trump) && !other.IsA(trump) ) return false;
					if ( this.IsA(leadSuit) && other.IsA(leadSuit) )
					{
						if ( this.IsA((int) SuitEnum.doubles) ) return true;
						if ( other.IsA((int) SuitEnum.doubles) ) return false;
						return this.Lo(leadSuit) > other.Lo(leadSuit);
					} else
					{
						return this.IsA(leadSuit);
					}
				}
			}

			if ( this.IsA(trump) && other.IsA(trump) )
			{
				if ( this.IsA((int) SuitEnum.doubles) && other.IsA((int) SuitEnum.doubles) ) return this.Lo(trump) > other.Lo(trump);
				if ( this.IsA((int) SuitEnum.doubles) ) return true;
				if ( other.IsA((int) SuitEnum.doubles) ) return false;
				return this.Lo(trump) > other.Lo(trump);
			} 

			if ( this.IsA(trump) ) return true;

			if ( other.IsA(trump) ) return false;

			if ( this.IsA(leadSuit) && other.IsA(leadSuit) )
			{
				if ( this.IsA((int) SuitEnum.doubles) ) return true;
				if ( other.IsA((int) SuitEnum.doubles) ) return false;
				return this.Lo(leadSuit) > other.Lo(leadSuit);
			}

			if ( this.IsA(leadSuit) ) return true;
			
			return false;
		}

		public string HiLo(Domino lead, int trump)
		{
			if ( this.IsA(trump) || !this.IsA(lead.Hi(trump)) ) return this.Hi(trump).ToString() + this.Lo(trump).ToString();
			if ( _dots[0] == lead.Hi(trump) ) return _dots[0].ToString() + _dots[1].ToString();
			return _dots[1].ToString() + _dots[0].ToString();
		}

		public void Flip()
		{
			int temp = _dots[0];
			_dots[0] = _dots[1];
			_dots[1] = temp;
		}

		public Point? Position { get; set; }

		/// This is convenient but not really the right place for this
		public string GetPositionStyle()
		{
			if ( Position == null ) return "";
			return string.Format("position: absolute; left: {0}px; top: {1}px;", Position.Value.X, Position.Value.Y);
		}

		public int LeftDot
		{
			get { return _dots[0];  }
		}

		public int RightDot
		{
			get { return _dots[1];  }
		}
	}
}
