using System;
using System.Diagnostics;

namespace FortyTwoLib.Logging
{
	public class TraceLogSink : ILogSink
	{
		public TraceEventType MinSeverityToLog { get; set; }

		public TraceLogSink(TraceEventType minSeverityToLog)
		{
			MinSeverityToLog = minSeverityToLog;
		}

		public TraceLogSink()
			: this(TraceEventType.Verbose)
		{
		}

		public void Write(TraceEventType severity, string message, string category, Exception ex)
		{
			if (ex != null)
			{
				message += "\r\nException: " + ex.Message + "\r\n" + ex.StackTrace;
			}
			Trace.WriteLine(severity.ToString().Substring(0, 4) + ": " + message);
		}
	}

	/// <summary>
	/// This is here primarily for MS Test which doesn't seem to display trace output via resharper test runenr
	/// </summary>
	public class ConsoleLogSink : ILogSink
	{
		public TraceEventType MinSeverityToLog { get; set; }

		public ConsoleLogSink(TraceEventType minSeverityToLog)
		{
			MinSeverityToLog = minSeverityToLog;
		}

		public ConsoleLogSink()
			: this(TraceEventType.Verbose)
		{
		}

		public void Write(TraceEventType severity, string message, string category, Exception ex)
		{
			if (ex != null)
			{
				message += "\r\nException: " + ex.Message + "\r\n" + ex.StackTrace;
			}
			Console.WriteLine(severity.ToString().Substring(0, 4) + ": " + message);
		}
	}
}