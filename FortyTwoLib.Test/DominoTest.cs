using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FortyTwoLib.Test
{
	[TestClass]
	public class DominoTest
	{
		[TestMethod]
		public void GreaterThanTest()
		{
			var lead = new Domino("31");
			var next = new Domino("22");
			Assert.AreEqual(false, next.GreaterThan(lead, lead, (int) SuitEnum.followMe, false));
		}

		[TestMethod]
		public void SuitOrderInitTest()
		{
			Assert.AreEqual(SuitOrders.Get(SuitEnum.sixes, 6), new Domino("66"));
		}
	}
}
