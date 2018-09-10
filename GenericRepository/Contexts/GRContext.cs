using GenericRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenericRepository.Enums;
using GenericRepository.Models;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Web;

namespace GenericRepository.Contexts
{
    public abstract class GRContext : IGRContext
    {
        protected Dictionary<GRContextLogLevel, List<IGRContextLogger>> loggers = null;

        public GRContext()
        {
            loggers = new Dictionary<GRContextLogLevel, List<IGRContextLogger>>();
            cache = new Dictionary<string, object>();
        }

        #region Logging methods
        protected bool HasAttachedDebugLogger = false;
        protected bool HasAttachedErrorLogger = false;

        public void RegisterLogger(IGRContextLogger log, GRContextLogLevel level)
        {
            foreach (Enum value in Enum.GetValues(level.GetType()))
            {
                if (level.HasFlag(value))
                {
                    GRContextLogLevel enumValue = (GRContextLogLevel)value;
                    if (!loggers.ContainsKey(enumValue))
                    {
                        switch (enumValue)
                        {
                            case GRContextLogLevel.Debug:
                                HasAttachedDebugLogger = true;
                                break;
                            case GRContextLogLevel.Error:
                                HasAttachedErrorLogger = true;
                                break;
                        }

                        loggers.Add(enumValue, new List<IGRContextLogger>());
                    }

                    loggers[enumValue].Add(log);
                }
            }
        }

        protected void LogDebug(string message, params object[] args)
        {
            if (!loggers.ContainsKey(GRContextLogLevel.Debug)) return;

            foreach (var logger in loggers[GRContextLogLevel.Debug])
            {
                logger.LogDebug(message, args);
            } 
        }

        protected void LogWarning(string message, params object[] args)
        {
            if (!loggers.ContainsKey(GRContextLogLevel.Warning)) return;

            foreach (var logger in loggers[GRContextLogLevel.Warning])
            {
                logger.LogWarning(message, args);
            }
        }

        protected void LogError(string message, params object[] args)
        {
            if (!loggers.ContainsKey(GRContextLogLevel.Error)) return;

            foreach (var logger in loggers[GRContextLogLevel.Error])
            {
                logger.LogError(message, args);
            }
        }

        protected void LogError(Exception exc, string message, params object[] args)
        {
            if (!loggers.ContainsKey(GRContextLogLevel.Error)) return;

            foreach (var logger in loggers[GRContextLogLevel.Error])
            {
                logger.LogError(exc, message, args);
            }
        }
        #endregion

        #region Query methods
        public abstract List<T> ExecuteQuery<T>(IGRQueriable<T> query);
        public abstract Task<List<T>> ExecuteQueryAsync<T>(IGRQueriable<T> query);
        public abstract List<T> ExecuteQuery<T>(string sqlStatement);
        public abstract Task<List<T>> ExecuteQueryAsync<T>(string sqlStatement);
        public abstract void ExecuteQuery(string sqlStatement);
        public abstract Task ExecuteQueryAsync(string sqlStatement);
        public abstract T ExecuteScalar<T>(IGRQueriable<T> queriable);
        public abstract T ExecuteScalar<T>(string sqlStatement);
        public abstract Task<T> ExecuteScalarAsync<T>(IGRQueriable<T> queriable);
        public abstract Task<T> ExecuteScalarAsync<T>(string sqlStatement);

        public abstract int ExecuteCount<T>(IGRQueriable<T> query);
        public abstract Task<int> ExecuteCountAsync<T>(IGRQueriable<T> query);
        #endregion

        #region Update/insert/delete methods
        public abstract void Update<T>(IGRUpdatable<T> updatable);
        public abstract void Delete<T>(IGRDeletable<T> deletable);
        public abstract void Insert<T>(IGRUpdatable<T> updatable);
        public abstract GRExecutionStatistics Execute<T>(IGRUpdatable<T> updatable);
        public abstract Task<GRExecutionStatistics> ExecuteAsync<T>(IGRUpdatable<T> updatable);
        public abstract GRExecutionStatistics Execute<T>(IGRDeletable<T> deletable);
        public abstract Task<GRExecutionStatistics> ExecuteAsync<T>(IGRDeletable<T> deletable);
        #endregion

        #region Save methods
        public abstract void SaveChanges();
        public abstract void SaveChangesInTransaction();
        public abstract Task SaveChangesAsync();
        public abstract Task SaveChangesInTransactionAsync();
        #endregion

        #region Stored procedures methods
        public abstract T GetItemFromSP<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        public abstract Task<T> GetItemFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        public abstract List<T> GetListFromSP<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        public abstract Task<List<T>> GetListFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameter, int timeout = -1);
        public abstract Task<GRJoinedList> GetJoinedListFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, Dictionary<string, Type> types, int timeout = -1);
        public abstract Task<List<SqlParameter>> ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        public abstract DataSet GetDataSetFromSP(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        public abstract DataTable GetDataTableFromSP(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters, int timeout = -1);

        public abstract Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, int timeout = -1);
        public abstract Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        public abstract Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters, int timeout = -1);

        public abstract Task<MemoryStream> GetMemoryStreamFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, CommandBehavior? commandBehavior = null);
        public abstract Task<Stream> GetZipStreamFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, CommandBehavior? commandBehavior = null, string fileName = "default");
        #endregion

        #region Scalar functions methods
        public abstract T ExecuteScalarFunction<T>(string functionName, List<SqlParameter> parameters);
        public abstract Task<T> ExecuteScalarFunctionAsync<T>(string functionName, List<SqlParameter> parameters);
        #endregion

        #region Query store methods
        public abstract Task<Tuple<string, DataSet>> GetQueryStoreStatementResultSetAsync(string statement, bool returnTables = true);
        #endregion

        #region Transactions
        public abstract void BeginTransaction();
        public abstract void CommitTransaction();
        public abstract void RollbackTransaction();

        #endregion

        #region Cache
        Dictionary<string, object> cache;
        public Dictionary<string, object> Cache
        {
            get
            {
                return cache;
            }
        }
        #endregion

        #region Join
        public abstract GRJoinedList ExecuteJoinQuery(IGRQueriable queriable);
        public abstract Task<GRJoinedList> ExecuteJoinQueryAsync(IGRQueriable queriable);
        #endregion

        protected bool IsPrimitiveType<T>()
        {
            if (typeof(T).IsPrimitive || 
                typeof(T) == typeof(String) || 
                typeof(T) == typeof(DateTime) ||
                typeof(T) == typeof(DateTime?) ||
                typeof(T) == typeof(int?) || 
                typeof(T) == typeof(long?) ||
                typeof(T) == typeof(Guid) ||
                typeof(T) == typeof(Guid?))
            {
                return true;
            }

            return false;
        }

        public virtual void Dispose()
        {
        }
    }
}
