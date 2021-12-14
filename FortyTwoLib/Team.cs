using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FortyTwoLib
{
	public class Team
	{
		public int Marks { get; set; }
		public List<Trick> Tricks { get; private set; }

		public Team()
		{
			Marks = 0;
			Tricks = new List<Trick>();
		}

		public int CountValue()
		{
			return Tricks.Sum(t => t.CountValue() + 1);
		}

		public void StartMark()
		{
			Tricks = new List<Trick>();
		}
	}
}
