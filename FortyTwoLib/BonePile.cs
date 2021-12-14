using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FortyTwoLib
{
	public class BonePile : List<Domino>
	{
		private string[] bones = new[] {
			"66", "65", "64", "63", "62", "61", "60",
			"55", "54", "53", "52", "51", "50",
			"44", "43", "42", "41", "40",
			"33", "32", "31", "30",
			"22", "21", "20",
			"11", "10",
			"00"
		};

		public BonePile(string[] dotList)
		{
			foreach ( string s in dotList )
			{
				s.Split(new[] {',', ' '}).ToList().ForEach(dots => this.Add(new Domino(dots)));
			}
		}

		public BonePile(int shuffleCount, Random rand, string hand)
		{
			for ( int i = 0; i < shuffleCount; i++ )
			{
				var src = rand.Next(28);
				var dst = rand.Next(28);
				var temp = bones[dst];
				bones[dst] = bones[src];
				bones[src] = temp;
			}

			// If we have a fixed hand for player 0, arrange it
			if ( hand != null )
			{
				var handBones = hand.Split(new[] {'x'});
				for (int i = 0; i < 7; i++)
				{
					var mirror = handBones[i].Substring(1, 1) + handBones[i].Substring(0, 1);
					for ( int j = i; j < 28; j++ )
					{
						if ( handBones[i] == bones[j] || mirror == bones[j] )
						{
							bones[j] = bones[i];
							bones[i] = handBones[i].CompareTo(mirror) > 0 ? handBones[i] : mirror;
							break;
						}
					}
				}
			}

			bones.ToList().ForEach(dots => this.Add(new Domino(dots)));
		}

		public Hand GetHandForSeat(int seat)
		{
			var hand = new Hand();
			for (int j = 0; j < 7; j++)
			{
				var dom = this[(seat * 7) + j];
				dom.Position = new Point();
				hand.Add(dom);
			}
			return hand;
		}

	}
}
