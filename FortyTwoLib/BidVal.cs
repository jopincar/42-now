using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FortyTwoLib
{
	public class BidVal
	{
		public int Loss { get; set; }
		public double Probability { get; set; }
		public SuitEnum Suit { get; set; }
		public bool Low { get; set; }

		public BidVal BestOf(BidVal other)
		{
			if ( other.Loss < this.Loss || (other.Loss == this.Loss && other.Probability > this.Probability) )
			{
				return other;
			}
			return this;
		}
	}
}
