using System;
using System.Data;
using System.Data.SqlClient;

namespace FortyTwoAccess
{
	public delegate void RetryCommandDelegate();

	public class SqlDataAccessBase : IDisposable
	{
		protected SqlConnection _conn;
		protected string _connStr;
		protected SqlTransaction _tran;
		protected int _tranCount;
		protected int _connCount;

		public SqlDataAccessBase(string connStr)
		{
			if (connStr == null) throw new ArgumentNullException("connStr");
			_connStr = connStr;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected SqlConnection GetConnection()
		{
			_connCount++;
			if (_connCount > 1)
			{
				return _conn;
			}
			_conn = new SqlConnection(_connStr);
			_conn.Open();
			return _conn;
		}

		protected void ReleaseConnection()
		{
			if (_connCount <= 0)
			{
				throw new InvalidOperationException("No connection to release.");
			}
			_connCount--;
			if (_connCount > 0)
			{
				return;
			}
			_conn.Close();
			_conn = null;
		}

		public virtual void BeginTransaction()
		{
			GetConnection();
			_tranCount++;
			if (_tranCount > 1)
			{
				return;
			}
			_tran = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
		}

		public virtual void Commit()
		{
			if (_tranCount <= 0)
			{
				throw new InvalidOperationException("No transaction to commit.");
			}
			_tranCount--;
			if (_tranCount == 0)
			{
				_tran.Commit();
				_tran = null;
			}
			ReleaseConnection();
		}

		public string ConnectionString
		{
			get { return _connStr; }
		}

		public virtual void Rollback()
		{
			_connCount -= _tranCount;
			_tranCount = 0;
			if (_tran != null)
			{
				_tran.Rollback();
				_tran = null;
			}
			if (_connCount <= 0)
			{
				_connCount = 0;

				if (_conn != null && _conn.State != ConnectionState.Closed)
				{
					_conn.Close();
				}
				_conn = null;
			}
		}

		public void CommitOrRollback(bool success)
		{
			if (success)
			{
				Commit();
			}
			else
			{
				Rollback();
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="del"></param>
		/// <param name="tries"></param>
		protected void ExecuteWithRetry(RetryCommandDelegate del, int tries)
		{
			for (int count = 0; count < tries; count++)
			{
				try
				{
					del();
					break;
				}
				catch (Exception ex)
				{
					if (count >= tries - 1 || !(ex.Message.ToLower().Contains("deadlock") || ex.Message.ToLower().Contains("timeout")))
					{
						throw;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmd"></param>
		protected void ExecuteNonQuery(SqlCommand cmd)
		{
			ExecuteNonQuery(cmd, 3);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="tries"></param>
		protected void ExecuteNonQuery(SqlCommand cmd, int tries)
		{
			GetConnection();
			try
			{
				cmd.Connection = _conn;
				cmd.Transaction = _tran;
				ExecuteWithRetry(delegate() { cmd.ExecuteNonQuery(); }, tries);
			}
			finally
			{
				ReleaseConnection();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		protected object ExecuteScalar(SqlCommand cmd)
		{
			return ExecuteScalar(cmd, 3);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="tries"></param>
		/// <returns></returns>
		protected object ExecuteScalar(SqlCommand cmd, int tries)
		{
			GetConnection();
			try
			{
				cmd.Connection = _conn;
				cmd.Transaction = _tran;
				object ret = null;
				ExecuteWithRetry(delegate() { ret = cmd.ExecuteScalar(); }, tries);
				return ret;
			}
			finally
			{
				ReleaseConnection();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		protected DataSet ExecuteDataSet(SqlCommand cmd)
		{
			return ExecuteDataSet(cmd, 3);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="tries"></param>
		/// <returns></returns>
		protected DataSet ExecuteDataSet(SqlCommand cmd, int tries)
		{
			GetConnection();
			DataSet ret = new DataSet();
			try
			{
				cmd.Connection = _conn;
				cmd.Transaction = _tran;
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				ExecuteWithRetry(delegate() { da.Fill(ret); }, tries);
			}
			finally
			{
				ReleaseConnection();
			}
			return ret;
		}

		/// <summary>
		/// Adds return value parameter to command
		/// </summary>
		/// <param name="cmd"></param>
		protected void AddReturnValue(SqlCommand cmd)
		{
			cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Int, 4);
			cmd.Parameters["@RETURN_VALUE"].Direction = ParameterDirection.ReturnValue;
		}

		/// <summary>
		/// Adds specified parameter name and value to command handling null
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="paramName"></param>
		/// <param name="value"></param>
		protected void AddWithValue(SqlCommand cmd, string paramName, object value)
		{
			if (value == null)
			{
				cmd.Parameters.AddWithValue(paramName, DBNull.Value);
			}
			else
			{
				cmd.Parameters.AddWithValue(paramName, value);
			}
		}

		public void Dispose()
		{
			if (_conn != null)
			{
				_conn.Dispose();
			}
		}
	}
}