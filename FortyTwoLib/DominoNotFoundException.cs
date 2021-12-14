using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FortyTwoLib
{
	public class DominoNotFoundException : Exception
	{
		public DominoNotFoundException()
		{
		}

		public DominoNotFoundException(string message) : base(message)
		{
		}

		public DominoNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected DominoNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
