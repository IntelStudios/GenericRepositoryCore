using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        public override T ExecuteScalar<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement(queriable, isCountQuery: false, forceIdentityColumn: false);
            T ret;
            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();
                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);

                    object scalar = command.ExecuteScalar();

                    if (scalar == DBNull.Value)
                    {
                        return default(T);
                    }

                    ret = (T)scalar;

                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                }
                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing scalar command ({0}).", typeof(T).Name);
                GRExecutionStatistics stats = new GRExecutionStatistics(null, queryStatement.ReadableStatement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<T> ExecuteScalarAsync<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement(queriable, isCountQuery: false, forceIdentityColumn: false);
            T ret;
            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);

                    object scalar = await command.ExecuteScalarAsync();

                    if (scalar == DBNull.Value)
                    {
                        return default(T);
                    }

                    ret = (T)scalar;

                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                }
                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous scalar command ({0}).", typeof(T).Name);
                GRExecutionStatistics stats = new GRExecutionStatistics(null, queryStatement.ReadableStatement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override T ExecuteScalar<T>(string sqlStatement)
        {
            SqlConnection connection = null;
            T ret;
            GRExecutionStatistics stats = null;

            try
            {
                connection = GetSqlConnection();
                using (SqlCommand command = new SqlCommand(sqlStatement, connection))
                {
                    command.Transaction = sqlTransaction;

                    object scalar = command.ExecuteScalar();

                    if (scalar == DBNull.Value)
                    {
                        return default(T);
                    }

                    ret = (T)scalar;

                    stats = ParseQueryStatistics(connection, sqlStatement);
                }

                LogSuccessfulQueryStats(stats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing scalar command ({0}).", typeof(T).Name);
                stats = new GRExecutionStatistics(null, sqlStatement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<T> ExecuteScalarAsync<T>(string sqlStatement)
        {
            SqlConnection connection = null;
            T ret;
            GRExecutionStatistics stats = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(sqlStatement, connection))
                {
                    command.Transaction = sqlTransaction;

                    object scalar = await command.ExecuteScalarAsync();

                    if (scalar == DBNull.Value)
                    {
                        return default(T);
                    }

                    ret = (T)scalar;

                    stats = ParseQueryStatistics(connection, sqlStatement);
                }

                LogSuccessfulQueryStats(stats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous scalar command ({0}).", typeof(T).Name);
                stats = new GRExecutionStatistics(null, sqlStatement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override int ExecuteCount<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement<T>(queriable, isCountQuery: true, forceIdentityColumn: false);

            int count;

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    ReplaceQueryAttributes(queryStatement, command);
                    count = (int)command.ExecuteScalar();
                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                }
                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing synchronous count statement for entity '{0}'.", typeof(T).Name);
                LogFailedQueryStats(queriable.ExecutionStats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, queriable.ExecutionStats = new GRExecutionStatistics(null, queryStatement.ReadableStatement, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return count;
        }

        public override async Task<int> ExecuteCountAsync<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement<T>(queriable, isCountQuery: true, forceIdentityColumn: false);

            int count;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    ReplaceQueryAttributes(queryStatement, command);
                    count = (int)await command.ExecuteScalarAsync();
                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                }
                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous count query for entity '{0}'.", typeof(T).Name);
                LogFailedQueryStats(queriable.ExecutionStats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, queriable.ExecutionStats = new GRExecutionStatistics(null, queryStatement.ReadableStatement, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return count;
        }
    }
}
