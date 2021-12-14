using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FortyTwoLib
{
	public class Bid
	{
		public Bid()
		{
			Amount = -1;
			Trump = SuitEnum.none;
			IsLow = false;
		}

		public int Amount { get; set; }
		public SuitEnum Trump { get; set; }
		public bool IsLow { get; set; }
		public int BidPosition { get; set; }

		public int MakeBidAmount
		{
			get { return Math.Min(42, Amount); }
		}

		public List<SuitEnum> GetvalidSuits()
		{
			return Enum.GetValues(typeof (SuitEnum)).Cast<SuitEnum>()
				.Where(s => (!IsLow || s == SuitEnum.doubles || s == SuitEnum.followMe) && (s != SuitEnum.none || Amount == 0))
				.ToList();
		}
	}
}
