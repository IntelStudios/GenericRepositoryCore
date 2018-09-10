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

        void ExecuteQuery(string sqlStatement);
        Task ExecuteQueryAsync(string sqlStatement);

        T ExecuteScalar<T>(IGRQueriable<T> queriable);
        T ExecuteScalar<T>(string sqlStatement);

        Task<T> ExecuteScalarAsync<T>(IGRQueriable<T> queriable);
        Task<T> ExecuteScalarAsync<T>(string sqlStatement);

        int ExecuteCount<T>(IGRQueriable<T> queriable);
        Task<int> ExecuteCountAsync<T>(IGRQueriable<T> queriable);
        #endregion

        #region Update/insert/delete methods
        void Update<T>(IGRUpdatable<T> updatable);
        void Insert<T>(IGRUpdatable<T> updatable);
        void Delete<T>(IGRDeletable<T> deletable);
        GRExecutionStatistics Execute<T>(IGRUpdatable<T> updatable);
        Task<GRExecutionStatistics> ExecuteAsync<T>(IGRUpdatable<T> updatable);
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
        T GetItemFromSP<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        Task<T> GetItemFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        List<T> GetListFromSP<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        Task<List<T>> GetListFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        Task<GRJoinedList> GetJoinedListFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, Dictionary<string, Type> types, int timeout = -1);
        Task<List<SqlParameter>> ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        DataSet GetDataSetFromSP(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        DataTable GetDataTableFromSP(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters, int timeout = -1);
        Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, int timeout = -1);
        Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1);
        Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters, int timeout = -1);
        Task<MemoryStream> GetMemoryStreamFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, CommandBehavior? commandBehavior = null);
        Task<Stream> GetZipStreamFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, CommandBehavior? commandBehavior = null, string fileName = "default");
        Task<Tuple<string, DataSet>> GetQueryStoreStatementResultSetAsync(string statement, bool returnTables = true);
        #endregion

        # region Scalar functions methods
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
        GRJoinedList ExecuteJoinQuery(IGRQueriable queriable);
        Task<GRJoinedList> ExecuteJoinQueryAsync(IGRQueriable queriable);
        #endregion
    }
}
