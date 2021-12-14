using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using FortyTwoLib.Logging;

namespace FortyTwoAccess
{
	public abstract class LinqToSqlDataAccessBase<TDataContext> : SqlDataAccessBase
		where TDataContext : DataContext
	{
		private TDataContext _context;
		private readonly Func<SqlConnection, TDataContext> _contextFactory;
		private readonly ILog _log;
		private readonly int _commandTimeout;

		public LinqToSqlDataAccessBase(Func<SqlConnection, TDataContext> dataContextFactory, string connStr, ILog log, int commandTimeout, bool logSql)
			: base(connStr)
		{
			_contextFactory = dataContextFactory;
			_log = log;
			_commandTimeout = commandTimeout;
			LogSql = logSql;
		}

		public LinqToSqlDataAccessBase(Func<SqlConnection, TDataContext> contextFactory, string connStr, ILog log)
			: this(contextFactory, connStr, log, 30, false)
		{
		}

		public bool LogSql { get; set; }

		public int CommandTimeout
		{
			get { return _commandTimeout; }
		}

		public TDataContext Context
		{
			get
			{
				if (_context == null)
				{
					GetConnection();
					_context = _contextFactory(_conn);
					_context.CommandTimeout = _commandTimeout;
					_context.Transaction = _tran;
				}
				return _context;
			}
		}

		public override void BeginTransaction()
		{
			base.BeginTransaction();
			if (_context != null)
			{
				_context.Transaction = _tran;
			}
		}

		public override void Commit()
		{
			base.Commit();
			if (_tranCount == 0)
			{
				if (_context != null)
				{
					_context.Transaction = null;
				}
			}
		}

		public override void Rollback()
		{
			base.Rollback();
			if (_context != null)
			{
				_context.Transaction = null;
			}
		}

		protected List<TItem> GetBlockAsList<TItem>(IQueryable<TItem> query, int pageIndex, int pageSize, string orderBy)
			where TItem : class
		{
			query = query.OrderBy(orderBy);

			bool forceLog = false;
			var sw = new StringWriter();
			Context.Log = sw;
			List<TItem> ret;
			try
			{
				if (pageSize == int.MaxValue)
				{
					ret = query.ToList();
				}
				else
				{
					// This is required due to a bug in Take() which results in an additional SQL statement
					// be executed for each Transaction row if Skip is called with 0
					var compQuery = CompiledQuery.Compile<TDataContext, int, int, IEnumerable<TItem>>
						((dc, skip, take) => query.Skip(skip).Take(take));
					ret = compQuery.Invoke(Context, (pageIndex - 1) * pageSize, pageSize).ToList();
				}
			}
			catch (Exception ex)
			{
				forceLog = true;
				_log.WriteError("GetBlockAsList failed", ex);
				throw;
			}
			finally
			{
				Context.Log = null;
				sw.Close();
				WriteSqlToLog(sw.ToString(), forceLog);
			}

			return ret;
		}

		void WriteSqlToLog(string sql, bool forceLog)
		{
			System.Diagnostics.Debug.WriteLine(sql);
			if (!(LogSql || forceLog)) return;
			_log.WriteInfo(sql);
		}

		public int GetCount<T>(IQueryable<T> query)
		{
			bool forceLog = false;
			var sw = new StringWriter();
			Context.Log = sw;
			try
			{
				return query.Count();
			}
			catch (Exception ex)
			{
				forceLog = true;
				_log.WriteError("Count failed", ex);
				throw;
			}
			finally
			{
				Context.Log = null;
				sw.Close();
				WriteSqlToLog(sw.ToString(), forceLog);
			}
		}

		public List<T> ToList<T>(IQueryable<T> query)
		{
			bool forceLog = false;
			var sw = new StringWriter();
			Context.Log = sw;
			try
			{
				return query.ToList();
			}
			catch (Exception ex)
			{
				forceLog = true;
				_log.WriteError("ToList failed", ex);
				throw;
			}
			finally
			{
				Context.Log = null;
				sw.Close();
				WriteSqlToLog(sw.ToString(), forceLog);
			}
		}

		protected Dictionary<TKey, TItem> GetBlockAsDictionary<TItem, TKey>(IQueryable<TItem> query, int pageIndex, int pageSize, string orderBy, Func<TItem, TKey> keySelector)
			where TItem : class
		{
			query = query.OrderBy(orderBy);

			bool forceLog = false;
			var sw = new StringWriter();
			Context.Log = sw;
			Dictionary<TKey, TItem> ret;
			try
			{
				if (pageSize == int.MaxValue)
				{
					ret = query.ToDictionary(keySelector);
				}
				else
				{
					// This is requiered due to a bug in Take() which results in an additional SQL statement
					// be executed for each Transaction row if Skip is called with 0
					var compQuery = CompiledQuery.Compile<TDataContext, int, int, IEnumerable<TItem>>
						((dc, skip, take) => query.Skip(skip).Take(take));
					ret = compQuery.Invoke(Context, (pageIndex - 1) * pageSize, pageSize).ToDictionary(keySelector);
				}
			}
			catch (Exception ex)
			{
				forceLog = true;
				_log.WriteError("GetBlockAsDictionary", ex);
				throw;
			}
			finally
			{
				Context.Log = null;
				sw.Close();
				WriteSqlToLog(sw.ToString(), forceLog);
			}
			return ret;
		}

		/// <summary>
		/// Used with "starts with" queries
		/// </summary>
		protected string LeftN1(string s)
		{
			// Not the greatest but works for now
			return s.Substring(0, s.Length - 1);
		}

		/// <summary>
		/// Use with "contains" queries
		/// </summary>
		protected string MidN2(string s)
		{
			// Not the greatest but works for now
			return s.Substring(1, s.Length - 2);
		}
	}
}