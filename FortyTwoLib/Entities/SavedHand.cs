using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FortyTwoLib.Entities
{
	public class SavedHand
	{
		public int SavedHandId { get; set; }
		public int PlayerId { get; set; }
		public string Hand { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
