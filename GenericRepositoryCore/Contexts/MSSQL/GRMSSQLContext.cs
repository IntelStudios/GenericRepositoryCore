using GenericRepository.Attributes;
using GenericRepository.Enums;
using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        #region Constants
        const string SqlParamPreffix = "P";
        const string SqlParamSuffix = "W";
        const string SqlKeyExecutionTime = "ExecutionTime";
        const string SqlKeyAffectedRows = "IduRows";
        const string SqlKeyNumberOfRows = "SelectRows";
        #endregion

        #region Fields
        string connectionString;
        private readonly bool statisticsEnabled;

        // queue of entyties to insert/update/delete
        List<GRContextQueueItem> contextQueue;

        // shared connection and transaction used only for transactions
        SqlConnection sqlConnection = null;
        SqlTransaction sqlTransaction = null;

        protected readonly System.Threading.SemaphoreSlim semaphoreConnection = new System.Threading.SemaphoreSlim(1);
        #endregion

        #region Constructors & Lifecycle methods
        public GRMSSQLContext(string connectionString, bool statisticsEnabled = true)
        {
            this.connectionString = connectionString;
            contextQueue = new List<GRContextQueueItem>();
            this.statisticsEnabled = statisticsEnabled;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (sqlConnection != null)
            {
                try
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                catch
                {
                }
            }
        }

        private void DisposeConnection(SqlConnection connection)
        {
            // if sqlTransaction is in progress, connection is shared and cannot be disposed
            if (sqlTransaction != null) return;

            if (connection != null)
            {
                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch
                {
                }
            }
        }
        #endregion

        #region Getting connection
        SqlConnection GetSqlConnection()
        {
            semaphoreConnection.Wait();

            try
            {
                // if no transaction is in progress, return new connection
                if (sqlTransaction == null)
                {
                    SqlConnection nonTransConn = new SqlConnection(connectionString);
                    nonTransConn.StatisticsEnabled = statisticsEnabled;
                    nonTransConn.Open();
                  
                    return nonTransConn;
                }
                else
                // if transaction is in progress, return shared connection
                {
                    return sqlConnection;
                }
            }
            finally
            {
                semaphoreConnection.Release();
            }
        }
        async Task<SqlConnection> GetSqlConnectionAsync()
        {
            try
            {
                await semaphoreConnection.WaitAsync();

                // if no transaction is in progress, return new connection
                if (sqlTransaction == null)
                {
                    SqlConnection nonTransConn = new SqlConnection(connectionString);
                    nonTransConn.StatisticsEnabled = statisticsEnabled;
                    await nonTransConn.OpenAsync();

                    return nonTransConn;
                }
                else
                // if transaction is in progress, return shared connection
                {
                    return sqlConnection;
                }
            }
            finally
            {
                semaphoreConnection.Release();
            }
        }
        #endregion;
    }
}
