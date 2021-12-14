using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FortyTwoLib.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FortyTwoLib.Test
{
	[TestClass]
	public class CalcTest
	{
		private Log _log;
		private Calc _calc;

		[TestInitialize()]
		public void Init()
		{
			_log = new Log();
			_log.AddSink(new TraceLogSink());
			_calc = new Calc();
		}

		[TestMethod]
		public void BidEvalSuitDoesNotChangeHand()
		{
			string dotList = "43,64,10,51,66,20,60";
			var hand = new Hand(dotList);
			var calc = new BidCalc();
			var result2 = calc.BidEvalSuit(hand, SuitEnum.blanks, _log);
			var result3 = calc.BidEvalSuit(hand, SuitEnum.sixes, _log);
			var hand2 = new Hand(dotList);
			foreach ( var d in hand2 )
			{
				Assert.AreEqual(true, hand.Contains(d));
			}

		}

		[TestMethod]
		public void BidEvalSuitReturnsSuitChecked()
		{
			var hand = new Hand("55,31,30,10,65,51,00");
			for ( SuitEnum trumpSuit = SuitEnum.followMe; trumpSuit <= SuitEnum.doubles; trumpSuit++ )
			{
				var result = new BidCalc().BidEvalSuit(hand, trumpSuit, _log);
				Assert.AreEqual(result.Suit, trumpSuit);
			}
		}

		[TestMethod]
		public void BidEvalSuitExpectedLossCantExceed42()
		{
			var hand = new Hand("33,32,51,50,20,43,65");
			var result = new BidCalc().BidEvalSuit(hand, SuitEnum.fives, _log);
			Assert.AreEqual(true, result.Loss <= 42);
		}

		[TestMethod]
		public void CoveredSixFourDoesNotLose10()
		{
			var hand = new Hand("66,64,55,11,15,12,10");
			var result = new BidCalc().BidEvalSuit(hand, SuitEnum.ones, _log);
			Assert.AreEqual(0, result.Loss);
		}

		[TestMethod]
		public void DontBidSuitNotInHand()
		{
			var hand = new Hand("11,10,31,52,53,60,65");
			var result = new BidCalc().BidEvalSuit(hand, SuitEnum.fours, _log);
			Assert.AreEqual(41, result.Loss);
		}

		[TestMethod]
		public void DoublesAsTrumpIncreasesOffs()
		{
			var hand = new Hand("33,66,22,44,54,42,62");
			var result = new BidCalc().BidEvalSuit(hand, SuitEnum.doubles, _log);
			Assert.AreEqual(13, result.Loss);
		}

		[TestMethod]
		public void BidEvalSuitTest()
		{
			var hand = new Hand("30,32,35,36,66,65,62");
			var result = new BidCalc().BidEvalSuit(hand, SuitEnum.sixes, _log);
		}
	}
}
