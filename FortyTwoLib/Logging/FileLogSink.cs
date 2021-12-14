using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace FortyTwoLib.Logging
{
	public class FileLogSink : ILogSink
	{
		private readonly string _fileName;
		public TraceEventType MinSeverityToLog { get; set; }

		public FileLogSink(string fileName, TraceEventType minSeverityToLog)
		{
			_fileName = fileName;
			MinSeverityToLog = minSeverityToLog;
		}

		public FileLogSink(string fileName)
			: this(fileName, TraceEventType.Information)
		{
		}

		public FileLogSink()
			: this(AppConfig.GetSetting("LogFile", @"logs\forty-two.log"))
		{
		}

		public void Write(TraceEventType severity, string message, string category, Exception ex)
		{
			if (ex != null)
			{
				message += "\r\nException: " + ex.Message + "\r\n" + ex.StackTrace;
			}
			lock ( _fileName )
			{
				try
				{
					using ( var streamWriter = File.AppendText(_fileName) )
					{
						streamWriter.WriteLine("{0}{1}: {2}", GetLinePrefix(), severity.ToString().Substring(0, 1), message);
						streamWriter.Flush();
					}
				}
				catch {} // Need to keep going if this fails.
			}
		}

		private string GetLinePrefix()
		{
			var builder = new StringBuilder(DateTime.Now.ToString("yy-MM-dd HH:mm:ss"), 45);
			builder.Append(" PID:");
			builder.Append(Process.GetCurrentProcess().Id.ToString("00000"));
			builder.Append(" TID:");
			builder.Append(Thread.CurrentThread.GetHashCode().ToString("00000"));
			return builder.ToString();
		}
	}
}