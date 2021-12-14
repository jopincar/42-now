using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using FortyTwoLib.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FortyTwoLib.Test
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class HandTest
	{
		public HandTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void Blah()
		{
			var log = new Log();
			var _rand = new Random();
			log.AddSink(new ConsoleLogSink());
			var table = new Table(log)
			{
				Name = "John's Table",
				TableId = 1,
			};
			table.AddPlayer(new Player(log)
			{
				Name = "John",
				PlayerId = 1,
				Hand = new Hand("52,50,22,21,60,53,51"),
				NetworkAddress = "1",
			}, 0);
			int id = _rand.Next();
			table.AddPlayer(new BotPlayer(log)
			{
				Name = "Monica",
				PlayerId = id,
				Hand = new Hand("41,32,42,40,63,61,44"),
				NetworkAddress = id.ToString(),
			}, 1);
			id = _rand.Next();
			table.AddPlayer(new BotPlayer(log)
			{
				Name = "Alex",
				PlayerId = _rand.Next(),
				Hand = new Hand("11,65,33,62,00,30,43"),
				NetworkAddress = id.ToString(),
			}, 2);
			id = _rand.Next();
			table.AddPlayer(new BotPlayer(log)
			{
				Name = "Lila",
				PlayerId = _rand.Next(),
				Hand = new Hand("31,64,20,54,10,66,55"),
				Bid = new Bid { Amount = 32, Trump = SuitEnum.followMe, IsLow = false },
				NetworkAddress = id.ToString(),
			}, 3);
			table.WhoseTurn = 3;
			table.SetBidWinner(3);
		}

		[TestMethod]
		public void MustPlayRequiresSecondPlayerToPlayLead()
		{
			var lead = new Domino("31");
			var hand = new Hand("52,50,22,21,60,53,51");
			var mustPlay = hand.MustPlay(lead, (int) SuitEnum.followMe);
			Assert.IsNotNull(mustPlay);
			Assert.AreEqual(true, mustPlay.Contains(new Domino("53")));
			Assert.AreEqual(true, !mustPlay.Contains(new Domino("22")));

		}
	}
}
