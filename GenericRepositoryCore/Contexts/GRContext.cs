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
using GenericRepository.Helpers;
using System.Reflection;
using GenericRepository.Attributes;
using System.Linq;
using GenericRepository.Exceptions;

namespace GenericRepository.Contexts
{
    public abstract partial class GRContext : IGRContext
    {
        protected Dictionary<GRContextLogLevel, List<IGRContextLogger>> loggers = null;

        #region Constructors & destructors
        public GRContext()
        {
            loggers = new Dictionary<GRContextLogLevel, List<IGRContextLogger>>();
            cache = new Dictionary<string, object>();
        }

        public virtual void Dispose()
        {
        }
        #endregion

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

        public abstract string ExecuteNonQuery(string sqlStatement, bool returnMessage);
        public virtual void ExecuteNonQuery(string sqlStatement)
        {
            ExecuteNonQuery(sqlStatement, false);
        }
        public abstract Task<string> ExecuteNonQueryAsync(string sqlStatement, bool returnMessage);
        public virtual Task ExecuteNonQueryAsync(string sqlStatement)
        {
            return ExecuteNonQueryAsync(sqlStatement, false);
        }

        public abstract T ExecuteScalar<T>(IGRQueriable<T> queriable);
        public abstract T ExecuteScalar<T>(string sqlStatement);
        public abstract Task<T> ExecuteScalarAsync<T>(IGRQueriable<T> queriable);
        public abstract Task<T> ExecuteScalarAsync<T>(string sqlStatement);

        public abstract int ExecuteCount<T>(IGRQueriable<T> query);
        public abstract Task<int> ExecuteCountAsync<T>(IGRQueriable<T> query);
        #endregion

        #region Update/insert/delete methods
        public abstract void EnqueueUpdate<T>(IGRUpdatable<T> updatable);
        public abstract void EnqueueDelete<T>(IGRDeletable<T> deletable);
        public abstract void EnqueueInsert<T>(IGRUpdatable<T> updatable);
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

        #region Scalar functions methods
        public abstract T ExecuteScalarFunction<T>(string functionName, List<SqlParameter> parameters);
        public abstract Task<T> ExecuteScalarFunctionAsync<T>(string functionName, List<SqlParameter> parameters);
        #endregion

        #region Transactions
        public abstract void BeginTransaction();
        public abstract void CommitTransaction();
        public abstract void RollbackTransaction();

        #endregion

        #region Join
        public abstract GRTable ExecuteJoinQuery(IGRQueriable queriable);
        public abstract Task<GRTable> ExecuteJoinQueryAsync(IGRQueriable queriable);
        #endregion

        #region Cache
        protected Dictionary<string, object> cache;
        public Dictionary<string, object> Cache
        {
            get
            {
                return cache;
            }
        }
        #endregion

        #region Data sets
        public virtual DataSet GetDataSetFromCommand(string commandString)
        {
            return GetDataSetFromCommand(commandString, defaultTimeout);
        }
        public virtual DataSet GetDataSetFromCommand(string commandString, int timeout)
        {
            GRDataSet dataSet = GetDataSetFromCommand(commandString, false, timeout);
            return dataSet.DataSet;
        }
        public virtual GRDataSet GetDataSetFromCommand(string commandString, bool returnMessage)
        {
            return GetDataSetFromCommand(commandString, returnMessage, defaultTimeout);
        }
        public abstract GRDataSet GetDataSetFromCommand(string commandString, bool returnMessage, int timeout);
        #endregion
    }
}
