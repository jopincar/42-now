using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace FortyTwoLib
{
	public class MailBox
	{
		private static readonly MailBox _instance = new MailBox();

		private Dictionary<string, Queue<Message>> _boxes;

		private MailBox()
		{
			_boxes = new Dictionary<string, Queue<Message>>();
		}

		public static MailBox GetInstance()
		{
			return _instance;
		}

		public void Send(string address, Message msg)
		{
			lock(_instance)
			{
				Queue<Message> messages;
				if ( !_boxes.TryGetValue(address, out messages) )
				{
					messages = new Queue<Message>();
					_boxes[address] = messages;
				}
				messages.Enqueue(msg);
			}
		}

		public Message GetMessage(string address)
		{
			lock(_instance)
			{
				Queue<Message> messages;
				if ( !_boxes.TryGetValue(address, out messages) ) return null;
				if ( messages.Count == 0 ) return null;
				return messages.Dequeue();
			}
		}

		public void Reset()
		{
			lock (_instance)
			{
				_boxes = new Dictionary<string, Queue<Message>>();	
			}
				
		}
	}
}
