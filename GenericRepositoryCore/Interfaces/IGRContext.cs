using GenericRepository.Enums;
using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace GenericRepository.Interfaces
{
    public interface IGRContext : IDisposable
    {
        #region Query methods
        List<T> ExecuteQuery<T>(IGRQueriable<T> queriable);
        Task<List<T>> ExecuteQueryAsync<T>(IGRQueriable<T> queriable);

        List<T> ExecuteQuery<T>(string sqlStatement);
        Task<List<T>> ExecuteQueryAsync<T>(string sqlStatement);

        void ExecuteNonQuery(string sqlStatement);
        Task ExecuteNonQueryAsync(string sqlStatement);

        string ExecuteNonQuery(string sqlStatement, bool returnMessage);
        Task<string> ExecuteNonQueryAsync(string sqlStatement, bool returnMessage);

        T ExecuteScalar<T>(IGRQueriable<T> queriable);
        T ExecuteScalar<T>(string sqlStatement);

        Task<T> ExecuteScalarAsync<T>(IGRQueriable<T> queriable);
        Task<T> ExecuteScalarAsync<T>(string sqlStatement);

        int ExecuteCount<T>(IGRQueriable<T> queriable);
        Task<int> ExecuteCountAsync<T>(IGRQueriable<T> queriable);

        DataSet GetDataSetFromCommand(string commandString);
        DataSet GetDataSetFromCommand(string commandString, int timeout);
        GRDataSet GetDataSetFromCommand(string commandString, bool returnMessage);
        GRDataSet GetDataSetFromCommand(string commandString, bool returnMessage, int timeout);
        #endregion

        #region Update/insert methods
        /// <summary>
        /// Puts entity into the insert/update/delete queue. Changes will be written into the DB just after calling any of SaveChanges... methods.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="updatable"></param>
        void EnqueueUpdate<T>(IGRUpdatable<T> updatable);

        /// <summary>
        /// Puts entity into the insert/update/delete queue. Entity will be written into the DB just after calling any of SaveChanges... methods.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="updatable"></param>
        void EnqueueInsert<T>(IGRUpdatable<T> updatable);

        Task UpdateAsync<T>(IGRUpdatable<T> updatable);

        Task InsertAsync<T>(IGRUpdatable<T> updatable);

        /// <summary>
        /// Removes entity from queue and immediately inserts/updates it synchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="updatable"></param>
        /// <returns></returns>
        GRExecutionStatistics Execute<T>(IGRUpdatable<T> updatable);

        /// <summary>
        /// Removes entity from queue and immediately inserts/updates it asynchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="updatable"></param>
        /// <returns></returns>
        Task<GRExecutionStatistics> ExecuteAsync<T>(IGRUpdatable<T> updatable);
        #endregion

        #region Delete methods
        /// <summary>
        /// Puts entity into the insert/update/delete queue. Entity will be deleted from DB just after calling any of SaveChanges... methods.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="updatable"></param>
        void EnqueueDelete<T>(IGRDeletable<T> deletable);
        GRExecutionStatistics Execute<T>(IGRDeletable<T> deletable);
        Task<GRExecutionStatistics> ExecuteAsync<T>(IGRDeletable<T> deletable);
        #endregion

        #region Save methods
        void SaveChanges();
        Task SaveChangesAsync();
        void SaveChangesInTransaction();
        Task SaveChangesInTransactionAsync();
        #endregion

        #region Transactions
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        #endregion

        #region Stored procedures methods
        #region Executing SP without parsing result
        Task ExecuteSPAsync(string storedProcedureName);
        Task ExecuteSPAsync(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler);
        Task ExecuteSPAsync(string storedProcedureName, int timeout);
        Task ExecuteSPAsync(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters);
        Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler);
        Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout);
        Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Executing SP with output params
        Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName);
        Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, int timeout);
        Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters);
        Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout);

        Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion
        
        #region Getting single entity from SP with single JSON output param
        Task<T> GetEntityFromSPWithSingleJsonOutParamAsync<T>(string storedProcedureName, List<SqlParameter> parameters);
        #endregion

        #region Getting entities from SP with single JSON output param
        Task<List<T>> GetEntitiesFromSPWithSingleJsonOutParamAsync<T>(string storedProcedureName, List<SqlParameter> parameters);
        #endregion
               
        #region Getting single value from SP
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, int timeout);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout);

        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);

        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, int timeout);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout);

        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Getting list of values from SP
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, int timeout);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout);

        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);

        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, int timeout);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout);

        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Getting single entity from SP
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, int timeout);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout);

        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);

        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, int timeout);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout);

        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler);
        Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Getting list of entities from SP
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, int timeout);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout);

        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);

        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, int timeout);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout);

        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler);
        Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);


        Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, GRPropertyCollection properties);
        Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, GRPropertyCollection properties, SqlInfoMessageEventHandler infoMessageHandler);

        Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, GRPropertyCollection properties, int timeout);
        Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, GRPropertyCollection properties, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Data sets and data tables
        DataSet GetDataSetFromSP(string storedProcedureName, List<SqlParameter> parameters);
        DataSet GetDataSetFromSP(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);

        DataTable GetDataTableFromSP(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters);
        DataTable GetDataTableFromSP(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters, int timeout = -1);

        Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, int timeout = -1);
        Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters, int timeout = -1);
        #endregion

        #region Streams
        Task<MemoryStream> GetMemoryStreamFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, CommandBehavior? commandBehavior = null);
        Task<Stream> GetZipStreamFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, CommandBehavior? commandBehavior = null, string fileName = "default"); 
        #endregion
        #endregion

        #region Scalar functions methods
        T ExecuteScalarFunction<T>(string functionName, List<SqlParameter> parameters);
        Task<T> ExecuteScalarFunctionAsync<T>(string functionName, List<SqlParameter> parameters);
        #endregion

        #region Logging methods
        void RegisterLogger(IGRContextLogger log, GRContextLogLevel level);
        #endregion

        #region Cache
        Dictionary<string, object> Cache { get; }
        #endregion

        #region Join
        GRTable ExecuteJoinQuery(IGRQueriable queriable);
        Task<GRTable> ExecuteJoinQueryAsync(IGRQueriable queriable);
        #endregion
    }
}
