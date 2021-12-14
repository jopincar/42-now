using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FortyTwoLib.Logging;
using FortyTwoLib.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FortyTwoLib.Test
{
	[TestClass]
	public class AccessTest
	{
		private PlayerProvider _provider;

		[TestInitialize()]
		public void Init()
		{
			_provider = new PlayerProvider();
		}

		[TestMethod]
		public void PlayerProvider()
		{
			_provider.ValidateUser("john", "abcd1234");
		}
	}
}
