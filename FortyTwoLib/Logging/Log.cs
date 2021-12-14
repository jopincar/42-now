using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FortyTwoLib.Logging
{
	public interface ILogSink
	{
		void Write(TraceEventType severity, string message, string category, Exception ex);
		TraceEventType MinSeverityToLog { get; set; }
	}

	public interface ILog
	{
		void Write(TraceEventType severity, string message, string category, Exception ex);
		void WriteVerbose(string message);
		void WriteInfo(string message);
		void WriteWarning(string message);
		void WriteWarning(string message, Exception ex);
		void WriteError(string message);
		void WriteError(string message, Exception ex);
	}

	public class Log : ILog
	{
		public List<ILogSink> Sinks { get; protected set; }

		public Log()
		{
			Sinks = new List<ILogSink> { new ConsoleLogSink() };
		}

		public Log(ILogSink sink)
			: this()
		{
			Sinks.Add(sink);
		}

		public void AddSink(ILogSink sink)
		{
			Sinks.Add(sink);
		}

		public virtual void Write(TraceEventType severity, string message, string category, Exception ex)
		{
			foreach (var sink in Sinks)
			{
				if (severity > sink.MinSeverityToLog) continue; // TraceEventType has error < warn < info < verbose
				sink.Write(severity, message, category, ex);
			}
		}

		public void WriteVerbose(string message)
		{
			Write(TraceEventType.Verbose, message, "", null);
		}

		public void WriteInfo(string message)
		{
			Write(TraceEventType.Information, message, "", null);
		}

		public void WriteWarning(string message)
		{
			WriteWarning(message, null);
		}

		public void WriteWarning(string message, Exception ex)
		{
			Write(TraceEventType.Warning, message, "", ex);
		}

		public void WriteError(string message)
		{
			WriteError(message, null);
		}

		public void WriteError(string message, Exception ex)
		{
			Write(TraceEventType.Error, message, "", ex);
		}
	}
}