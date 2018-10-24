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
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public class GRMSSQLContext : GRContext, IGRContext, IDisposable
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
        List<GRContextQueueItem> contextQueue;

        // shared connection and transaction used only for transactions
        SqlConnection sqlConnection = null;
        SqlTransaction sqlTransaction = null;

        protected readonly System.Threading.SemaphoreSlim semaphoreConnection = new System.Threading.SemaphoreSlim(1);
        #endregion

        #region Constructors & Lifecycle methods
        public GRMSSQLContext(string connectionString)
        {
            this.connectionString = connectionString;
            contextQueue = new List<GRContextQueueItem>();
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

        #region Query/select data methods
        const string parseResultLineMethodName = "ParseQueryResultLine";
        MethodInfo parseResultLineMethod = typeof(GRMSSQLContext).GetMethod(parseResultLineMethodName, BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(List<GRDBProperty>), typeof(SqlDataReader), typeof(string) }, null);
        public override async Task<GRJoinedList> ExecuteJoinQueryAsync(IGRQueriable leafQueriable)
        {
            IGRQueriable rootQueriable = GetRootQueriable(leafQueriable);
            GRMergedQueryStatement queryStatement = BuildJoinedQueryStatement(rootQueriable);

            GRJoinedList ret = new GRJoinedList()
            {
                Items = new List<GRJoinedListItem>()
            };

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            GRJoinedListItem listItem = new GRJoinedListItem();
                            foreach (var statement in queryStatement.Statements)
                            {
                                if (statement.IsExcluded) continue;
                                MethodInfo genericMethod = parseResultLineMethod.MakeGenericMethod(statement.Type);
                                object obj = genericMethod.Invoke(this, new object[] { statement.ReturnColumns, reader, statement.Prefix });
                                listItem.Objects.Add(obj);
                            }
                            ret.Items.Add(listItem);
                        }

                        rootQueriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                    }
                }
                LogSuccessfulQueryStats(rootQueriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous join query.");
                GRExecutionStatistics stats = new GRExecutionStatistics(null, queryStatement.Statement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }
        public override GRJoinedList ExecuteJoinQuery(IGRQueriable leafQueriable)
        {
            IGRQueriable rootQueriable = GetRootQueriable(leafQueriable);
            GRMergedQueryStatement queryStatement = BuildJoinedQueryStatement(rootQueriable);

            GRJoinedList ret = new GRJoinedList()
            {
                Items = new List<GRJoinedListItem>()
            };

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GRJoinedListItem listItem = new GRJoinedListItem();

                            foreach (var statement in queryStatement.Statements)
                            {
                                if (statement.IsExcluded) continue;
                                MethodInfo genericMethod = parseResultLineMethod.MakeGenericMethod(statement.Type);
                                object obj = genericMethod.Invoke(this, new object[] { statement.ReturnColumns, reader, statement.Prefix });
                                listItem.Objects.Add(obj);
                            }
                            ret.Items.Add(listItem);
                        }

                        rootQueriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                    }
                }
                LogSuccessfulQueryStats(rootQueriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing synchronous join query.");
                GRExecutionStatistics stats = new GRExecutionStatistics(null, queryStatement.Statement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }
        private static IGRQueriable GetRootQueriable(IGRQueriable leafQueriable)
        {
            IGRQueriable rootQueriable = leafQueriable;

            while (rootQueriable.JoiningQueriable != null)
            {
                rootQueriable = rootQueriable.JoiningQueriable.Queriable;
            }

            return rootQueriable;
        }
        public override List<T> ExecuteQuery<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement(queriable);

            List<T> ret = null;

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ret = ParseQueryResult<T>(queryStatement, reader, queriable.Prefix);
                    }
                    ret.ForEach(t => ApplyAutoProperties<T>(t, GRAutoValueApply.AfterSelect, queriable.Repository));
                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                }
                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing synchronous select query for entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics stats = new GRExecutionStatistics(null, queryStatement.Statement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }
        public override List<T> ExecuteQuery<T>(string sqlStatement)
        {
            IGRQueriable<T> queriable = new GRQueriable<T>();

            GRQueryStatement queryStatement = new GRQueryStatement
            {
                ReturnColumns = GetQueryColumns(GRDataTypeHelper.GetDBStructure(typeof(T)), queriable),
                Statement = sqlStatement
            };

            List<T> ret = null;

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ret = ParseQueryResult<T>(queryStatement, reader, queriable.Prefix);
                    }
                    ret.ForEach(t => ApplyAutoProperties<T>(t, GRAutoValueApply.AfterSelect, queriable.Repository));
                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                }
                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing synchronous select query for entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics stats = new GRExecutionStatistics(null, queryStatement.Statement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }
        public override async Task<List<T>> ExecuteQueryAsync<T>(string sqlStatement)
        {
            IGRQueriable<T> queriable = new GRQueriable<T>();

            GRQueryStatement queryStatement = new GRQueryStatement
            {
                ReturnColumns = GetQueryColumns(GRDataTypeHelper.GetDBStructure(typeof(T)), queriable),
                Statement = sqlStatement
            };

            List<T> ret = null;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        ret = ParseQueryResult<T>(queryStatement, reader, queriable.Prefix);
                    }

                    ret.ForEach(t => ApplyAutoProperties<T>(t, GRAutoValueApply.AfterSelect, queriable.Repository));
                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                }
                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing sql query '{1}' select query for entity '{0}'.", typeof(T).Name, sqlStatement);
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
        public override void ExecuteQuery(string sqlStatement)
        {
            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = new SqlCommand(sqlStatement, connection))
                {
                    command.Transaction = sqlTransaction;

                    command.ExecuteNonQuery();
                }
                LogSuccessfulQueryStats(ParseQueryStatistics(connection, sqlStatement));
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing synchronous sql query '{0}'.", sqlStatement);
                GRExecutionStatistics stats = new GRExecutionStatistics(null, sqlStatement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }
        public override async Task ExecuteQueryAsync(string sqlStatement)
        {
            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(sqlStatement, connection))
                {
                    command.Transaction = sqlTransaction;

                    await command.ExecuteNonQueryAsync();
                }
                LogSuccessfulQueryStats(ParseQueryStatistics(connection, sqlStatement));
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing sql query '{0}'.", sqlStatement);
                GRExecutionStatistics stats = new GRExecutionStatistics(null, sqlStatement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }
        public override async Task<List<T>> ExecuteQueryAsync<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement<T>(queriable);

            List<T> ret = null;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        ret = ParseQueryResult<T>(queryStatement, reader, queriable.Prefix);
                    }

                    ret.ForEach(t => ApplyAutoProperties<T>(t, GRAutoValueApply.AfterSelect, queriable.Repository));
                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                }
                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous select query for entity '{0}'.", typeof(T).Name);
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

        public override T ExecuteScalar<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement<T>(queriable);

            List<T> ret;

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();
                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ret = ParseQueryResult<T>(queryStatement, reader, queriable.Prefix);

                        if (!ret.Any())
                            return default(T);
                    }

                    ApplyAutoProperties<T>(ret.First(), GRAutoValueApply.AfterSelect, queriable.Repository);
                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                }
                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing synchronous select query for entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics stats = new GRExecutionStatistics(null, queryStatement.ReadableStatement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret.First();
        }
        public override T ExecuteScalar<T>(string sqlStatement)
        {
            IGRQueriable<T> queriable = new GRQueriable<T>();

            GRQueryStatement queryStatement = new GRQueryStatement
            {
                ReturnColumns = GetQueryColumns(GRDataTypeHelper.GetDBStructure(typeof(T)), queriable),
                Statement = sqlStatement
            };

            object ret;

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();
                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);

                    ret = command.ExecuteScalar();

                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);

                    if (ret == DBNull.Value)
                        return default(T);
                }

                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing synchronous select query for entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics stats = new GRExecutionStatistics(null, queryStatement.ReadableStatement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return (T)Convert.ChangeType(ret, typeof(T));
        }

        public override async Task<T> ExecuteScalarAsync<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement<T>(queriable);

            List<T> ret;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        ret = ParseQueryResult<T>(queryStatement, reader, queriable.Prefix);

                        if (!ret.Any())
                        {
                            return default(T);
                        }
                    }

                    ApplyAutoProperties<T>(ret.First(), GRAutoValueApply.AfterSelect, queriable.Repository);
                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);
                }
                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous select query for entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics stats = new GRExecutionStatistics(null, queryStatement.ReadableStatement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret.First();
        }
        public override async Task<T> ExecuteScalarAsync<T>(string sqlStatement)
        {
            IGRQueriable<T> queriable = new GRQueriable<T>();

            GRQueryStatement queryStatement = new GRQueryStatement
            {
                ReturnColumns = GetQueryColumns(GRDataTypeHelper.GetDBStructure(typeof(T)), queriable),
                Statement = sqlStatement
            };

            object ret;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(queryStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(queryStatement, command);

                    ret = await command.ExecuteScalarAsync();

                    queriable.ExecutionStats = ParseQueryStatistics(connection, queryStatement);

                    if (ret == DBNull.Value)
                        return default(T);
                }

                LogSuccessfulQueryStats(queriable.ExecutionStats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous select query for entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics stats = new GRExecutionStatistics(null, queryStatement.ReadableStatement, null);
                LogFailedQueryStats(stats, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, stats, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return (T)Convert.ChangeType(ret, typeof(T));
        }

        public override int ExecuteCount<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement<T>(queriable, true);

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
                string errMessage = string.Format("An unknown error occured while executing synchronous count query for entity '{0}'.", typeof(T).Name);
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
            GRQueryStatement queryStatement = BuildQueryStatement<T>(queriable, true);

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
        public GRQueryStatement BuildQueryStatement<T>(IGRQueriable<T> queriable, bool isCountQuery = false)
        {
            GRQueryStatement statement = BuildSimpleQueryStatement(queriable, isCountQuery);
            return statement;
        }
        public GRMergedQueryStatement BuildJoinedQueryStatement(IGRQueriable queriable)
        {
            List<GRQueryStatement> statements = new List<GRQueryStatement>();

            IGRQueriable current = queriable;

            statements.Add(BuildQueryStatement(queriable));

            while (current.Joined != null)
            {
                GRQueryStatement statement = BuildQueryStatement(current.Joined.Queriable);

                statements.Add(statement);

                current = current.Joined.Queriable;
            }

            return MergeStatements(statements);
        }
        protected virtual GRQueryStatement BuildQueryStatement(IGRQueriable queriable)
        {
            string methodName = "BuildQueryStatement";
            MethodInfo method = this.GetType().GetMethod(methodName);

            MethodInfo genericMethod = method.MakeGenericMethod(queriable.Type);
            object res = genericMethod.Invoke(this, new object[] { queriable, false });
            return (GRQueryStatement)res;
        }
        private GRMergedQueryStatement MergeStatements(List<GRQueryStatement> statements)
        {
            GRMergedQueryStatement ret = new GRMergedQueryStatement()
            {
                TableName = statements.First().TableName
            };

            StringBuilder commandString = new StringBuilder();

            List<string> columns = statements.Where(s => !string.IsNullOrEmpty(s.ColumnsString)).Select(s => s.ColumnsString).ToList();
            commandString.Append("SELECT ");

            if (statements.Where(s => s.IsDistinct).Any()) commandString.Append("DISTINCT ");

            commandString.Append(string.Join(", ", columns));
            commandString.AppendFormat(" FROM [{0}] AS {1} ", statements.First().TableName, statements.First().Prefix);

            if (statements.First().HasNoLock) commandString.Append("with (NOLOCK) ");

            List<string> joins = statements.Where(s => !string.IsNullOrEmpty(s.JoiningString)).Select(s => s.JoiningString).ToList();
            commandString.Append(string.Join(" ", joins));

            List<string> wheres = new List<string>();

            ret.Statements = new List<GRQueryStatement>();

            foreach (var statement in statements)
            {
                ret.Statements.Add(statement);

                if (string.IsNullOrEmpty(statement.WhereString)) continue;

                string where = statement.WhereString;

                for (int i = 0; i < statement.ParamColumns.Count; i++)
                {
                    string oldName = string.Format("@{0}{1}{2}", SqlParamPreffix, i, SqlParamSuffix);
                    string newName = string.Format("@{0}{1}{2}", SqlParamPreffix, ret.ParamColumns.Count, SqlParamSuffix);
                    where = where.Replace(oldName, newName);

                    ret.ParamColumns.Add(statement.ParamColumns[i]);
                }

                wheres.Add(where);
            }

            if (wheres.Any())
            {
                commandString.Append(" WHERE ");
                if (wheres.Count == 1)
                {
                    commandString.Append(wheres.First());
                }
                else
                {
                    commandString.Append(string.Join(" AND ", wheres.Select(w => string.Format("({0})", w))));
                }
            }

            List<string> orders = new List<string>();

            foreach (var statement in statements)
            {
                if (string.IsNullOrEmpty(statement.OrderString)) continue;

                string order = statement.OrderString;

                for (int i = 0; i < statement.ParamColumns.Count; i++)
                {
                    string oldName = string.Format("@{0}{1}{2}", SqlParamPreffix, i, SqlParamSuffix);
                    string newName = string.Format("@{0}{1}{2}", SqlParamPreffix, ret.ParamColumns.Count, SqlParamSuffix);
                    order = order.Replace(oldName, newName);

                    ret.ParamColumns.Add(statement.ParamColumns[i]);
                }

                orders.Add(order);
            }

            if (orders.Any())
            {
                commandString.Append(" ORDER BY ");
                if (orders.Count == 1)
                {
                    commandString.Append(orders.First());
                }
                else
                {
                    commandString.Append(string.Join(", ", orders.Select(w => string.Format("{0}", w))));
                }
            }

            ret.Statement = commandString.ToString();

            return ret;
        }
        public GRQueryStatement BuildSimpleQueryStatement<T>(IGRQueriable<T> queriable, bool isCountQuery)
        {
            GRQueryStatement queryCommand = new GRQueryStatement
            {
                IsCountQuery = isCountQuery,
                TableName = queriable.Structure.TableName,
                Prefix = queriable.Prefix,
                Type = typeof(T),
                IsExcluded = queriable.HasExcludedAllProperties,
                IsDistinct = queriable.IsDistinct,
                HasNoLock = queriable.HasNoLock
            };

            string queryCommandString = null;

            List<GRDBProperty> selectColumns = null;

            try
            {
                // constructing SELECT <TOP> <COLUMNS> FROM <TABLE>
                if (isCountQuery)
                {
                    queryCommandString = string.Format("SELECT COUNT(1) FROM [{0}]", queriable.Structure.TableName);
                }
                else
                {
                    selectColumns = GetQueryColumns(GRDataTypeHelper.GetDBStructure(typeof(T)), queriable);

                    string selectColumnList = string.Join(", ", selectColumns.Select(c => GetPrefixedSelectColumn(queriable.Prefix, c.DBColumnName)));

                    if (!queryCommand.IsExcluded)
                    {
                        queryCommand.ColumnsString = selectColumnList;
                    }

                    if (queriable.HasLimit)
                    {
                        selectColumnList = string.Format("TOP {0} {1}", queriable.Limit, selectColumnList);
                    }

                    queryCommandString = string.Format("SELECT {2}{0} FROM [{1}]", selectColumnList, queriable.Structure.TableName, queryCommand.IsDistinct ? "DISTINCT " : string.Empty);

                    if (!string.IsNullOrEmpty(queriable.Prefix))
                    {
                        queryCommandString += string.Format(" AS {0}", queriable.Prefix);
                    }

                    if (queriable.HasNoLock)
                        queryCommandString += " with (NOLOCK)";
                }
            }
            catch (Exception exc)
            {
                throw new GRQueryBuildFailedException(exc, "An error occured while constructing list of columns to query.");
            }

            try
            {
                // constructing WHERE clause
                if (queriable.WhereClauses != null && queriable.WhereClauses.Any())
                {
                    List<string> whereConditions = new List<string>();

                    foreach (var whereClause in queriable.WhereClauses)
                    {
                        string where = BuildQueryConditions(whereClause.Expression.Body, queryCommand, queriable.Prefix);
                        whereConditions.Add(where);
                    }

                    if (whereConditions.Count == 1)
                    {
                        queryCommandString += string.Format(" WHERE {0} ", whereConditions.First());
                    }
                    else if (whereConditions.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(" WHERE ");
                        for (int i = 0; i < whereConditions.Count; i++)
                        {
                            if (i > 0) sb.Append(" AND ");
                            sb.AppendFormat("({0})", whereConditions[i]);
                        }
                        queryCommandString += sb.ToString();
                    }
                    queryCommand.WhereString = string.Join(" AND ", whereConditions);
                }
            }
            catch (Exception exc)
            {
                throw new GRQueryBuildFailedException(exc, "An error occured while constructing WHERE clause of query.");
            }

            if (queriable.Joined != null)
            {
                queryCommand.JoiningString = string.Format(" {0} JOIN [{1}] as {2}{5} on {3} = {4}",
                    queriable.Joined.TypeName,
                    queriable.Joined.Queriable.Structure.TableName,
                    queriable.Joined.Queriable.Prefix,
                    GetPrefixedColumn(queriable.Prefix, queriable.Joined.TargetPropertyName),
                    GetPrefixedColumn(queriable.Joined.Queriable.Prefix, queriable.Joined.SourcePropertyName),
                    queriable.Joined.Queriable.HasNoLock ? " with (NOLOCK)" : string.Empty
                );
            }

            try
            {
                // constructing ORDER BY clause
                if (!isCountQuery && queriable.OrderByProperties != null && queriable.OrderByProperties.Any())
                {
                    List<string> orderColumns = new List<string>();

                    foreach (var orderClause in queriable.OrderByProperties)
                    {
                        if (orderClause.Direction == GRQueryOrderDirection.Ascending)
                        {
                            //orderColumns.Add(orderClause.Property.DBColumnName);
                            orderColumns.Add(GetPrefixedColumn(queriable.Prefix, orderClause.Property.DBColumnName));

                        }
                        else
                        {
                            //orderColumns.Add(orderClause.Property.DBColumnName + " DESC");
                            orderColumns.Add(GetPrefixedColumn(queriable.Prefix, orderClause.Property.DBColumnName) + " DESC");
                        }
                    }

                    queryCommandString += string.Format(" ORDER BY {0}", string.Join(", ", orderColumns));

                    queryCommand.OrderString = string.Join(", ", orderColumns);
                }
            }
            catch (Exception exc)
            {
                throw new GRQueryBuildFailedException(exc, "An error occured while constructing ORDER BY clause of query.");
            }

            if (!queryCommand.IsExcluded)
            {
                queryCommand.Statement = queryCommandString;
                queryCommand.ReturnColumns = selectColumns;
            }

            return queryCommand;
        }
        string GetPrefixedColumn(string prefix, string propertyName)
        {
            return string.IsNullOrEmpty(prefix) ? string.Format("[{0}]", propertyName) : string.Format("{0}.[{1}]", prefix, propertyName);
        }
        string GetPrefixedResultingColumn(string prefix, string propertyName)
        {
            return string.IsNullOrEmpty(prefix) ? propertyName : string.Format("{0}_{1}", prefix, propertyName);
        }
        string GetPrefixedSelectColumn(string prefix, string propertyName)
        {
            return string.IsNullOrEmpty(prefix) ? string.Format("[{0}]", propertyName) : string.Format("{0}.[{1}] AS {0}_{1}", prefix, propertyName);
        }
        string BuildQueryConditions(Expression exp, GRStatement queryCommand, string prefix)
        {
            #region AND
            if (exp.NodeType == ExpressionType.And || exp.NodeType == ExpressionType.AndAlso)
            {
                BinaryExpression binExp = exp as BinaryExpression;
                return string.Format("({0}) AND ({1})", BuildQueryConditions(binExp.Left, queryCommand, prefix), BuildQueryConditions(binExp.Right, queryCommand, prefix));
            }
            #endregion

            #region OR
            if (exp.NodeType == ExpressionType.Or || exp.NodeType == ExpressionType.OrElse)
            {
                BinaryExpression binExp = exp as BinaryExpression;
                return string.Format("({0}) OR ({1})", BuildQueryConditions(binExp.Left, queryCommand, prefix), BuildQueryConditions(binExp.Right, queryCommand, prefix));
            }
            #endregion

            #region NOT
            if (exp.NodeType == ExpressionType.Not)
            {
                UnaryExpression unExp = exp as UnaryExpression;

                if (unExp.Operand is MemberExpression && unExp.Operand.Type == typeof(bool))
                {
                    return string.Format("{0} = 0", (unExp.Operand as MemberExpression).Member.Name);
                }
                else
                {
                    return string.Format("NOT ({0})", BuildQueryConditions(unExp.Operand, queryCommand, prefix));
                }
            }
            #endregion

            #region Methods
            if (exp.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methodCall = exp as MethodCallExpression;

                if (methodCall.Method.Name == "IsNullOrEmpty")
                {
                    string propertyName = GRDataTypeHelper.GetExpressionValueString(methodCall.Arguments.First());
                    string ret = string.Format("{0} IS NULL OR {0} = ''", GetPrefixedColumn(prefix, propertyName));
                    return ret;
                }

                if (methodCall.Object.Type == typeof(String))
                {
                    string value = (string)GRDataTypeHelper.GetExpressionValue(methodCall.Arguments.First());
                    string propertyName = GRDataTypeHelper.GetExpressionValueString(methodCall.Object);

                    if (methodCall.Method.Name == "StartsWith")
                    {
                        value = value + "%";
                    }
                    if (methodCall.Method.Name == "EndsWith")
                    {
                        value = "%" + value;
                    }
                    if (methodCall.Method.Name == "Contains")
                    {
                        value = "%" + value + "%";
                    }

                    string param = "@" + SqlParamPreffix + queryCommand.ParamColumns.Count + SqlParamSuffix;

                    GRDBStructure structure = GRDataTypeHelper.GetDBStructure(methodCall.Object);
                    GRDBProperty prop = structure.Properties.Where(p => p.PropertyInfo.Name == propertyName).FirstOrDefault();

                    queryCommand.ParamColumns.Add(new GRStatementValue()
                    {
                        Value = value,
                        Property = prop
                    });

                    string ret = string.Format("{0} LIKE {1}", GetPrefixedColumn(prefix, propertyName), param);
                    return ret;
                }

                #region Cointains
                if (typeof(IEnumerable).IsAssignableFrom(methodCall.Object.Type))
                {
                    if (methodCall.Method.Name == "Contains")
                    {
                        IEnumerable en = null;
                        List<string> values = new List<string>();

                        if (methodCall.Object is MemberExpression)
                        {
                            MemberExpression memExp = methodCall.Object as MemberExpression;
                            object enumerable = GRDataTypeHelper.GetMemberValue(memExp);
                            en = enumerable as IEnumerable;
                        }
                        else
                        {
                            object enumerable = GRDataTypeHelper.GetExpressionValue(methodCall.Object);
                            en = enumerable as IEnumerable;
                        }


                        if (methodCall.Arguments.First() is MethodCallExpression)
                        {
                            MethodCallExpression callExp = methodCall.Arguments.First() as MethodCallExpression;
                        }

                        string dbColumnName = GRDataTypeHelper.GetExpressionValueString(methodCall.Arguments.First());

                        foreach (var value in en)
                        {
                            values.Add(GRDataTypeHelper.GetObjectValueString(value));
                        }

                        // OPTIMIZETT: This query shoud not be send to a database
                        if (values.Count > 0)
                        {
                            string ret = string.Format("{0} IN ({1})", GetPrefixedColumn(prefix, dbColumnName), string.Join(", ", values));
                            return ret;
                        }
                        else
                        {
                            return "1 = 2";
                        }
                    }

                    throw new GRInvalidOperationException(string.Format("Method '{0}' called on collection is not supported yet.", methodCall.Method.Name));
                }
                #endregion
            }
            #endregion

            if (exp is ConstantExpression)
            {
                ConstantExpression constExp = exp as ConstantExpression;
                if (constExp.Value is bool && Convert.ToBoolean(constExp.Value)) return "1 = 1";
            }

            if (exp is BinaryExpression)
            {
                BinaryExpression binExp = exp as BinaryExpression;
                return GetQueryConditionClause(binExp, exp.NodeType, queryCommand, prefix);
            }

            if (exp is MemberExpression && exp.Type == typeof(bool))
            {
                return string.Format("{0} = 1", GetPrefixedColumn(prefix, (exp as MemberExpression).Member.Name));
            }

            throw new GRUnsuportedOperatorException(exp.NodeType.ToString());
        }

        string GetQueryConditionClause(BinaryExpression binExp, ExpressionType operation, GRStatement commandStructure, string prefix)
        {
            object left = GRDataTypeHelper.GetExpressionValue(binExp.Left);
            object right = GRDataTypeHelper.GetExpressionValue(binExp.Right);

            if (left is PropertyInfo && right is PropertyInfo)
            {
                throw new GRUnsuportedOperatorException("Could not compare two properties.");
            }

            if (!(left is PropertyInfo) && !(right is PropertyInfo))
            {
                throw new GRUnsuportedOperatorException("Could not compare two properties.");
            }

            string cmd = null;

            if (left is PropertyInfo)
            {
                if (right == null)
                {
                    if (operation == ExpressionType.Equal)
                    {
                        cmd = string.Format("{0} IS NULL", GetPrefixedColumn(prefix, GRDataTypeHelper.GetExpressionValueString(binExp.Left)));
                    }
                    else if (operation == ExpressionType.NotEqual)
                    {
                        cmd = string.Format("{0} IS NOT NULL", GetPrefixedColumn(GRDataTypeHelper.GetExpressionValueString(binExp.Left), prefix));
                    }
                    else
                    {
                        throw new GRUnsuportedOperatorException("Could not compare NULL value.");
                    }
                }
                else
                {
                    cmd = string.Format("{0} {2} {1}",
                        GetPrefixedColumn(prefix, GRDataTypeHelper.GetExpressionValueString(binExp.Left)),
                        "@" + SqlParamPreffix + commandStructure.ParamColumns.Count + SqlParamSuffix,
                        GetQueryConditionSymbol(operation));

                    GRDBProperty prop = GRDataTypeHelper.GetDBProperty(left as PropertyInfo);
                    commandStructure.ParamColumns.Add(new GRStatementValue
                    {
                        Property = prop,
                        Value = right
                    });
                }
                return cmd;
            }
            else
            {
                if (left == null)
                {
                    if (operation == ExpressionType.Equal)
                    {
                        cmd = string.Format("{0} IS NULL", GetPrefixedColumn(prefix, GRDataTypeHelper.GetExpressionValueString(binExp.Right)));
                    }
                    else if (operation == ExpressionType.NotEqual)
                    {
                        cmd = string.Format("{0} IS NOT NULL", GetPrefixedColumn(prefix, GRDataTypeHelper.GetExpressionValueString(binExp.Right)));
                    }
                    else
                    {
                        throw new GRUnsuportedOperatorException("Could not compare NULL value.");
                    }
                }
                else
                {
                    cmd = string.Format("{1} {2} {0}",
                        GetPrefixedColumn(prefix, GRDataTypeHelper.GetExpressionValueString(binExp.Right)),
                        "@" + SqlParamPreffix + commandStructure.ParamColumns.Count + SqlParamSuffix,
                        GetQueryConditionSymbol(operation));

                    GRDBProperty prop = GRDataTypeHelper.GetDBProperty(right as PropertyInfo);
                    commandStructure.ParamColumns.Add(new GRStatementValue
                    {
                        Property = prop,
                        Value = left
                    });
                }
                return cmd;
            }
        }
        string GetQueryConditionSymbol(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "!=";
                default:
                    throw new GRUnsuportedOperatorException(type.ToString());
            }
        }
        private List<T> ParseQueryResult<T>(GRQueryStatement queryCommand, SqlDataReader reader, string prefix)
        {
            return ParseQueryResult<T>(queryCommand.ReturnColumns, reader, prefix);
        }
        private List<T> ParseQueryResult<T>(List<GRDBProperty> returnColumns, SqlDataReader reader, string prefix)
        {
            List<T> ret = new List<T>();

            try
            {
                while (reader.Read())
                {
                    T t = Activator.CreateInstance<T>();
                    foreach (var property in returnColumns)
                    {
                        string columnName = GetPrefixedResultingColumn(prefix, property.DBColumnName);
                        object value = null;

                        try
                        {
                            value = reader[columnName];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            if (IsNullableProperty(property)) continue;
                            throw new GRUnknownColumnException(columnName);
                        }

                        if (value.Equals(DBNull.Value))
                        {
                            continue;
                        }

                        if (property.PropertyInfo.PropertyType == typeof(Stream))
                        {
                            Stream stream = new MemoryStream(value as byte[]);
                            value = stream;
                        }

                        if (GRDataTypeHelper.HasAttribute(property.PropertyInfo, typeof(GRJSONAttribute)))
                        {
                            value = JsonConvert.DeserializeObject((string)value, property.PropertyInfo.PropertyType);
                        }

                        property.PropertyInfo.SetValue(t, value);
                    }

                    ApplyAutoProperties<T>(t, GRAutoValueApply.AfterSelect, null);

                    ret.Add(t);
                }
            }
            finally
            {
                reader.Close();
            }

            return ret;
        }

        bool IsNullableProperty(GRDBProperty prop)
        {
            Type underType = Nullable.GetUnderlyingType(prop.PropertyInfo.PropertyType);
            return underType != null;
        }

        /// <summary>
        /// Returns if all columns are set to DBNULL.Value
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="prefix"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        private bool IsAllNull(SqlDataReader reader, string prefix, List<GRDBProperty> columns)
        {
            prefix += "_";

            // no columns to parse
            if (columns == null || columns.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                if (!columnName.StartsWith(prefix)) continue;
                if (reader[columnName] != DBNull.Value) return false;
            }
            return true;
        }

        private T ParseQueryResultLine<T>(List<GRDBProperty> columns, SqlDataReader reader, string prefix)
        {
            if (IsAllNull(reader, prefix, columns))
            {
                return default(T);
            }

            T t = Activator.CreateInstance<T>();
            foreach (var property in columns)
            {
                string columnName = GetPrefixedResultingColumn(prefix, property.DBColumnName);
                object value = null;

                try
                {
                    value = reader[columnName];
                }
                catch (IndexOutOfRangeException)
                {
                    if (IsNullableProperty(property)) continue;
                    throw new GRUnknownColumnException(columnName);
                }

                if (value.Equals(DBNull.Value))
                {
                    continue;
                }

                if (property.PropertyInfo.PropertyType == typeof(Stream))
                {
                    Stream stream = new MemoryStream(value as byte[]);
                    value = stream;
                }

                if (GRDataTypeHelper.HasAttribute(property.PropertyInfo, typeof(GRJSONAttribute)))
                {
                    value = JsonConvert.DeserializeObject((string)value, property.PropertyInfo.PropertyType);
                }

                property.PropertyInfo.SetValue(t, value);
            }

            ApplyAutoProperties<T>(t, GRAutoValueApply.AfterSelect, null);

            return t;
        }
        public GRExecutionStatistics ParseQueryStatistics(SqlConnection connection, GRQueryStatement queryCommand)
        {
            return ParseQueryStatistics(connection, queryCommand.ReadableStatement);
        }
        public GRExecutionStatistics ParseQueryStatistics(SqlConnection connection, string statement)
        {
            IDictionary dbStats = connection.RetrieveStatistics();
            connection.ResetStatistics();

            long executionTime = (long)dbStats[SqlKeyExecutionTime];
            long numberOfRows = (long)dbStats[SqlKeyNumberOfRows];

            return new GRExecutionStatistics(numberOfRows, statement, executionTime);
        }
        List<GRDBProperty> GetQueryColumns<T>(GRDBStructure structure, IGRQueriable<T> queriable)
        {
            if (queriable.HasIncludedProperties && queriable.HasExcludedProperties)
            {
                throw new GRQueryBuildFailedException("Query cannot contain both INCLUDE and EXCLUDE commands.");
            }

            if (queriable.HasIncludedProperties)
            {
                return queriable.IncludedProperties.Select(p => p.Property).ToList();
            }

            List<GRDBProperty> selectProperties = new List<GRDBProperty>();

            for (int i = 0; i < structure.Properties.Count; i++)
            {
                if (queriable.ContainsExcludedProperty(structure.Properties[i].PropertyInfo.Name))
                {
                    continue;
                }

                selectProperties.Add(structure.Properties[i]);
            }

            return selectProperties;
        }
        private static void ReplaceQueryAttributes(GRStatement statement, SqlCommand command)
        {
            statement.ReadableStatement = statement.Statement;

            for (int i = 0; i < statement.ParamColumns.Count; i++)
            {
                string paramKey = "@" + SqlParamPreffix + i + SqlParamSuffix;
                object value = statement.ParamColumns[i].Value;

                if (value is Stream)
                {
                    statement.ReadableStatement = statement.ReadableStatement.Replace(paramKey, string.Format("<Binary stream of {0} B>", (value as Stream).Length));
                }
                if (value is byte[])
                {
                    statement.ReadableStatement = statement.ReadableStatement.Replace(paramKey, string.Format("<Byte array of {0} B>", (value as byte[]).Length));
                }
                else
                {
                    statement.ReadableStatement = statement.ReadableStatement.Replace(paramKey, GRDataTypeHelper.GetObjectValueString(value));
                }

                if (value == null)
                {
                    value = DBNull.Value;
                }

                SqlParameter param = command.Parameters.AddWithValue(paramKey, value);

                if (statement.ParamColumns[i].Property.IsBinary)
                {
                    param.DbType = DbType.Binary;
                }
            }
        }
        #endregion

        #region Logging methods
        private void LogSuccessfulQueryStats(GRExecutionStatistics executionStats)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Executed in {0} ms, returned {1} entities:", executionStats.ExecutionTime, executionStats.AffectedRows);
            if (executionStats != null && executionStats.ExecutionCommand != null)
            {
                sb.AppendLine();
                sb.Append(executionStats.ExecutionCommand);
            }
            LogDebug(sb.ToString());
        }

        private void LogSuccessfulUpdateStats(GRExecutionStatistics executionStats)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Executed in {0} ms, updated {1} entities:", executionStats.ExecutionTime, executionStats.AffectedRows);
            if (executionStats != null && executionStats.ExecutionCommand != null)
            {
                sb.AppendLine();
                sb.Append(executionStats.ExecutionCommand);
            }
            LogDebug(sb.ToString());
        }

        private void LogSuccessfulDeleteStats(GRExecutionStatistics executionStats)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Executed in {0} ms, deleted {1} entities:", executionStats.ExecutionTime, executionStats.AffectedRows);
            if (executionStats != null && executionStats.ExecutionCommand != null)
            {
                sb.AppendLine();
                sb.Append(executionStats.ExecutionCommand);
            }
            LogDebug(sb.ToString());
        }

        private void LogFailedQueryStats(GRExecutionStatistics executionStats, string errMessage, Exception exc)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(errMessage);
            if (executionStats != null && executionStats.ExecutionCommand != null)
            {
                sb.AppendLine();
                sb.Append(executionStats.ExecutionCommand);
            }
            LogError(exc, sb.ToString());
        }

        private void LogFailedUpdateStats(GRExecutionStatistics executionStats, string errMessage, Exception exc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(errMessage);
            if (executionStats != null && executionStats.ExecutionCommand != null)
            {
                sb.AppendLine();
                sb.Append(executionStats.ExecutionCommand);
            }
            LogError(exc, sb.ToString());
        }
        #endregion

        #region Auxiliary methods

        SqlConnection GetSqlConnection()
        {
            semaphoreConnection.Wait();

            try
            {
                // if no transaction is in progress, return new connection
                if (sqlTransaction == null)
                {
                    SqlConnection nonTransConn = new SqlConnection(connectionString);
                    nonTransConn.StatisticsEnabled = true;
                    nonTransConn.Open();
                    nonTransConn.InfoMessage += NonTransConn_InfoMessage;

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

        private void NonTransConn_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            LogDebug(e.Message);
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
                    nonTransConn.StatisticsEnabled = true;
                    await nonTransConn.OpenAsync();
                    nonTransConn.InfoMessage += NonTransConn_InfoMessage;

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

        public void GRValidate<T>(T entity, List<GRDBProperty> properties)
        {

            List<GRModelError> errors = new List<GRModelError>();
            ValidationContext context = new ValidationContext(entity, null, null);

            foreach (var property in properties)
            {
                context.MemberName = property.PropertyInfo.Name;
                object value = property.PropertyInfo.GetValue(entity);
                ICollection<ValidationResult> result = new List<ValidationResult>();
                bool isValid = Validator.TryValidateProperty(value, context, result);

                if (!isValid)
                {
                    errors.Add(new GRModelError(property.PropertyInfo, value, result));
                }
            }

            if (errors.Any())
            {
                throw new GRModelNotValidException(errors);
            }
        }

        public override GRExecutionStatistics Execute<T>(IGRDeletable<T> deletable)
        {
            if (deletable.Entity != null)
            {
                GRContextQueueItem item = contextQueue.Where(i => i.Item == deletable).Single();
                contextQueue.Remove(item);
            }
            return DeleteEntity(deletable);
        }

        public override async Task<GRExecutionStatistics> ExecuteAsync<T>(IGRDeletable<T> deletable)
        {
            if (deletable.Entity != null)
            {
                GRContextQueueItem item = contextQueue.Where(i => i.Item == deletable).Single();
                contextQueue.Remove(item);
            }
            return await DeleteEntityAsync(deletable);
        }

        public override GRExecutionStatistics Execute<T>(IGRUpdatable<T> updatable)
        {
            GRContextQueueItem item = contextQueue.Where(i => i.Item == updatable).Single();

            contextQueue.Remove(item);

            updatable.Repository.PrepareForSave();

            if (item.Action == GRContextQueueAction.Insert)
            {
                return InsertEntity(updatable);
            }
            else
            {
                return UpdateEntity(updatable);
            }
        }

        public override async Task<GRExecutionStatistics> ExecuteAsync<T>(IGRUpdatable<T> updatable)
        {
            GRContextQueueItem item = contextQueue.Where(i => i.Item == updatable).Single();

            contextQueue.Remove(item);

            await updatable.Repository.PrepareForSaveAsync();

            if (item.Action == GRContextQueueAction.Insert)
            {
                return await InsertEntityAsync(updatable);
            }
            else
            {
                return await UpdateEntityAsync(updatable);
            }
        }

        string[] GetPropertiesToStore<T>(IGRUpdatable<T> update, GRContextQueueAction action)
        {
            string[] propertiesToStore = null;

            List<string> autoUpdatePropertyNames = null;

            if (action == GRContextQueueAction.Insert)
            {
                autoUpdatePropertyNames = update.Structure.AutoInsertProperties
                    .Select(p => p.PropertyInfo.Name).ToList();
            }
            else
            {
                autoUpdatePropertyNames = update.Structure.AutoUpdateProperties
                    .Select(p => p.PropertyInfo.Name).ToList();
            }

            // updating all properties
            if (!update.HasIncludedProperties && !update.HasExcludedProperties && !update.HasForceExcludedProperties)
            {
                return null;
            }
            // updating only selected properties + autoproperties
            else if (!update.HasExcludedProperties && !update.HasForceExcludedProperties)
            {
                propertiesToStore = update.IncludedProperties.Select(p => p.Property.PropertyInfo.Name).ToArray();
                propertiesToStore = propertiesToStore.Union(autoUpdatePropertyNames).ToArray();
                return propertiesToStore;
            }
            else
            {
                List<string> excludedProperties = update.ExcludedProperties.Select(p => p.Property.PropertyInfo.Name).ToList();
                propertiesToStore = update.Structure.NonKeyProperties
                    .Where(p => !excludedProperties.Contains(p.PropertyInfo.Name))
                    .Select(p => p.PropertyInfo.Name)
                    .ToArray();
                propertiesToStore = propertiesToStore.Union(autoUpdatePropertyNames).ToArray();
                propertiesToStore = propertiesToStore.Except(update.ForceExcludedProperties.Select(p => p.Property.PropertyInfo.Name)).ToArray();
                return propertiesToStore;
            }
        }

        void ApplyAutoProperties<T>(T entity, GRAutoValueApply apply, object source)
        {
            GRDBStructure structure = GRDataTypeHelper.GetDBStructure(entity);

            foreach (var autoSelectProperty in structure.AutoProperties)
            {
                GRAutoValueAttribute autoAttr = autoSelectProperty.PropertyInfo.GetCustomAttribute<GRAutoValueAttribute>();
                if (!autoAttr.Apply.HasFlag(apply)) continue;

                #region GRAttributeAttribute
                if (autoAttr is GRAttributeAttribute)
                {
                    GRAttributeAttribute atAttr = autoAttr as GRAttributeAttribute;
                    Attribute entityAttribute = entity.GetType().GetCustomAttribute(atAttr.Attribute);
                    PropertyInfo attributeProperty = entityAttribute.GetType().GetProperty(atAttr.PropertyName);
                    object value = attributeProperty.GetValue(entityAttribute);
                    autoSelectProperty.PropertyInfo.SetValue(entity, value);
                }
                #endregion

                #region GRIDPropertyAttribute
                if (autoAttr is GRIDPropertyAttribute)
                {
                    GRIDPropertyAttribute idAttr = autoAttr as GRIDPropertyAttribute;
                    PropertyInfo thisProperty = autoSelectProperty.PropertyInfo;
                    PropertyInfo idProperty;

                    if (structure.HasIdentityProperty)
                    {
                        idProperty = structure.IdentityProperty.PropertyInfo;
                    }
                    else
                    {
                        var idProps = structure.Type
                            .GetProperties()
                            .Where(p => p.Name != autoSelectProperty.PropertyInfo.Name && (p.Name.EndsWith("Id") || p.Name.EndsWith("ID")))
                            .ToList();
                        if (idProps.Count == 0)
                            throw new GRAttributeApplicationFailedException(autoAttr, "No ID columns were found.");
                        if (idProps.Count > 1)
                            throw new GRAttributeApplicationFailedException(autoAttr, "Too many ID columns were found.");

                        idProperty = idProps.First();
                    }

                    if (idAttr.Direction == GRAutoValueDirection.In)
                    {
                        thisProperty.SetValue(entity, idProperty.GetValue(entity));
                    }
                    else
                    {
                        idProperty.SetValue(entity, thisProperty.GetValue(entity));
                    }
                }
                #endregion

                #region GRPropertyAttribute
                if (autoAttr is GRPropertyAttribute)
                {
                    GRPropertyAttribute propAttr = autoAttr as GRPropertyAttribute;

                    PropertyInfo thisProperty = autoSelectProperty.PropertyInfo;
                    PropertyInfo otherProperty = entity.GetType().GetProperty(propAttr.PropertyName);

                    if (propAttr.Direction == GRAutoValueDirection.In)
                    {
                        thisProperty.SetValue(entity, otherProperty.GetValue(entity));
                    }
                    else
                    {
                        otherProperty.SetValue(entity, thisProperty.GetValue(entity));
                    }
                }
                #endregion

                #region GRRepositoryPropertyAttribute
                if (autoAttr is GRRepositoryPropertyAttribute)
                {
                    if (source == null)
                    {
                        LogWarning("Auto attribute '{0}' could not be applied, source object was not provided!", autoAttr);
                        continue;
                    };

                    GRRepositoryPropertyAttribute propAttr = autoAttr as GRRepositoryPropertyAttribute;

                    PropertyInfo thisProperty = autoSelectProperty.PropertyInfo;
                    PropertyInfo repositoryProperty = source.GetType().GetProperty(propAttr.PropertyName);

                    if (propAttr.Direction == GRAutoValueDirection.In)
                    {
                        object value = repositoryProperty.GetValue(source);
                        thisProperty.SetValue(entity, value);
                    }
                    else
                    {
                        object value = thisProperty.GetValue(entity);
                        repositoryProperty.SetValue(source, value);
                    }
                }
                #endregion

                #region GRCurrentDatetimeAttribute
                if (autoAttr is GRCurrentDatetimeAttribute)
                {
                    GRCurrentDatetimeAttribute datetimeAttr = autoAttr as GRCurrentDatetimeAttribute;
                    autoSelectProperty.PropertyInfo.SetValue(entity, DateTime.Now);
                }
                #endregion

                #region GRNewGuidAttribute
                if (autoAttr is GRNewGuidAttribute)
                {
                    GRNewGuidAttribute guidAttr = autoAttr as GRNewGuidAttribute;
                    autoSelectProperty.PropertyInfo.SetValue(entity, Guid.NewGuid());
                }
                #endregion
            }
        }
        #endregion

        #region Update data methods
        public override void Update<T>(IGRUpdatable<T> updatable)
        {
            contextQueue.Add(new GRContextQueueItem()
            {
                Item = updatable,
                Action = GRContextQueueAction.Update
            });
        }

        private GRExecutionStatistics UpdateEntity<T>(IGRUpdatable<T> updatable)
        {
            ApplyAutoProperties(updatable.Entity, GRAutoValueApply.BeforeUpdate, updatable.Repository);
            string[] propertiesToStore = GetPropertiesToStore(updatable, GRContextQueueAction.Update);
            GRExecutionStatistics updateResult = ExecuteUpdateCommand(updatable, propertiesToStore);
            updatable.ExecutionStats = updateResult;
            return updateResult;
        }

        private async Task<GRExecutionStatistics> UpdateEntityAsync<T>(IGRUpdatable<T> updatable)
        {
            ApplyAutoProperties(updatable.Entity, GRAutoValueApply.BeforeUpdate, updatable.Repository);
            string[] propertiesToStore = GetPropertiesToStore(updatable, GRContextQueueAction.Insert);
            GRExecutionStatistics updateResult = await ExecuteUpdateCommandAsync(updatable, propertiesToStore);
            updatable.ExecutionStats = updateResult;
            return updateResult;
        }

        GRUpdateStatement BuildUpdateStatement<T>(IGRUpdatable<T> update, string[] propertiesToUpdate)
        {
            CallPreSaveMethods(update.Entity, GRPreSaveActionType.Update);

            GRUpdateStatement updateStatement = new GRUpdateStatement();

            if (update.Structure.KeyProperties.Count == 0)
            {
                throw new GRQueryBuildFailedException(string.Format("Entity '{0}' has not defined any keys.", update.Structure.Type));
            }

            // constructing DV assignments => Column = @Param
            List<string> valueAssignments = new List<string>();

            foreach (var nonKeyProperty in update.Structure.NonKeyProperties)
            {
                if (GRDataTypeHelper.HasAttribute(nonKeyProperty.PropertyInfo, typeof(GRInsertOnlyAttribute))) continue;
                if (propertiesToUpdate != null && !propertiesToUpdate.Any(p => nonKeyProperty.PropertyInfo.Name == p)) continue;
                valueAssignments.Add(string.Format("[{0}] = @{1}{2}{3}", nonKeyProperty.DBColumnName, SqlParamPreffix, updateStatement.ParamColumns.Count, SqlParamSuffix));

                if (GRDataTypeHelper.HasAttribute(nonKeyProperty.PropertyInfo, typeof(GRJSONAttribute)))
                {
                    object value = nonKeyProperty.PropertyInfo.GetValue(update.Entity);
                    string json = JsonConvert.SerializeObject(value);

                    updateStatement.ParamColumns.Add(new GRStatementValue
                    {
                        Value = json,
                        Property = nonKeyProperty
                    });
                }
                else
                {
                    updateStatement.ParamColumns.Add(new GRStatementValue
                    {
                        Property = nonKeyProperty,
                        Value = nonKeyProperty.PropertyInfo.GetValue(update.Entity)
                    });
                }

                updateStatement.ReturnColumns.Add(nonKeyProperty);
            }

            if (!valueAssignments.Any())
            {
                throw new GRQueryBuildFailedException("There are no values to update.");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("UPDATE [{0}] SET ", update.Structure.TableName);
            sb.Append(string.Join(", ", valueAssignments));
            sb.AppendFormat(" WHERE ");

            for (int i = 0; i < update.Structure.KeyProperties.Count; i++)
            {
                if (i > 0) sb.Append(" AND ");
                GRDBProperty prop = update.Structure.KeyProperties[i];
                sb.AppendFormat("[{0}] = @{1}{2}{3}", prop.DBColumnName, SqlParamPreffix, updateStatement.ParamColumns.Count, SqlParamSuffix);

                updateStatement.ParamColumns.Add(new GRStatementValue
                {
                    Value = prop.PropertyInfo.GetValue(update.Entity),
                    Property = prop
                });
            }

            updateStatement.Statement = sb.ToString();

            return updateStatement;
        }

        private void CallPreSaveMethods<T>(T entity, GRPreSaveActionType action)
        {
            GRDBStructure structure = GRDataTypeHelper.GetDBStructure<T>();

            List<MethodInfo> methods = action == GRPreSaveActionType.Insert ? structure.BeforeInsertMethods : structure.BeforeUpdateMethods;

            foreach (var method in methods)
            {
                try
                {
                    method.Invoke(entity, null);
                }
                catch (Exception exc)
                {
                    throw new GRPreSaveException(exc, method, action, typeof(T));
                }
            }
        }

        public GRExecutionStatistics ParseDeleteStatistics(SqlConnection connection, GRDeleteStatement statement)
        {
            IDictionary dbStats = connection.RetrieveStatistics();
            connection.ResetStatistics();

            long executionTime = (long)dbStats[SqlKeyExecutionTime];
            long affectedRows = (long)dbStats[SqlKeyAffectedRows];

            return new GRExecutionStatistics(affectedRows, statement.ReadableStatement, executionTime);
        }

        public GRExecutionStatistics ParseUpdateStatistics(SqlConnection connection, GRUpdateStatement statement)
        {
            IDictionary dbStats = connection.RetrieveStatistics();
            connection.ResetStatistics();

            long executionTime = (long)dbStats[SqlKeyExecutionTime];
            long affectedRows = (long)dbStats[SqlKeyAffectedRows];

            return new GRExecutionStatistics(affectedRows, statement.ReadableStatement, executionTime);
        }

        private GRExecutionStatistics ExecuteUpdateCommand<T>(IGRUpdatable<T> updatable, string[] properties = null)
        {
            GRUpdateStatement updateStatement = BuildUpdateStatement<T>(updatable, properties);

            this.GRValidate(updatable.Entity, updateStatement.ParamColumns.Select(v => v.Property).ToList());

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = new SqlCommand(updateStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(updateStatement, command);
                    int affectedRows = command.ExecuteNonQuery();
                }
                GRExecutionStatistics stats = ParseUpdateStatistics(connection, updateStatement);
                LogSuccessfulUpdateStats(stats);
                return stats;
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while synchronous updating entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics statistics = new GRExecutionStatistics(null, updateStatement.ReadableStatement, null);
                LogFailedUpdateStats(statistics, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, statistics, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }

        private async Task<GRExecutionStatistics> ExecuteUpdateCommandAsync<T>(IGRUpdatable<T> updatable, string[] properties = null)
        {
            GRUpdateStatement updateStatement = BuildUpdateStatement<T>(updatable, properties);

            this.GRValidate(updatable.Entity, updateStatement.ParamColumns.Select(v => v.Property).ToList());

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(updateStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(updateStatement, command);
                    int affectedRows = await command.ExecuteNonQueryAsync();
                }
                GRExecutionStatistics stats = ParseUpdateStatistics(connection, updateStatement);
                LogSuccessfulUpdateStats(stats);
                return stats;
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while asynchronous updating entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics statistics = new GRExecutionStatistics(null, updateStatement.ReadableStatement, null);
                LogFailedUpdateStats(statistics, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, statistics, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }
        #endregion

        #region Delete methods
        public override void Delete<T>(IGRDeletable<T> updatable)
        {
            contextQueue.Add(new GRContextQueueItem()
            {
                Item = updatable,
                Action = GRContextQueueAction.Delete
            });
        }

        private GRExecutionStatistics DeleteEntity<T>(IGRDeletable<T> updatable)
        {
            GRExecutionStatistics deleteResult = ExecuteDeleteCommand(updatable);
            updatable.ExecutionStats = deleteResult;
            return deleteResult;
        }

        private async Task<GRExecutionStatistics> DeleteEntityAsync<T>(IGRDeletable<T> updatable)
        {
            GRExecutionStatistics deleteResult = await ExecuteDeleteCommandAsync(updatable);
            updatable.ExecutionStats = deleteResult;
            return deleteResult;
        }

        private GRExecutionStatistics ExecuteDeleteCommand<T>(IGRDeletable<T> updatable)
        {
            GRDeleteStatement deleteStatement = BuildDeleteStatement<T>(updatable);

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = new SqlCommand(deleteStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(deleteStatement, command);
                    int affectedRows = command.ExecuteNonQuery();
                }
                GRExecutionStatistics stats = ParseDeleteStatistics(connection, deleteStatement);
                LogSuccessfulDeleteStats(stats);
                return stats;
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while synchronous deleting entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics statistics = new GRExecutionStatistics(null, deleteStatement.ReadableStatement, null);
                LogFailedUpdateStats(statistics, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, statistics, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }

        private async Task<GRExecutionStatistics> ExecuteDeleteCommandAsync<T>(IGRDeletable<T> updatable)
        {
            GRDeleteStatement deleteStatement = BuildDeleteStatement<T>(updatable);

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(deleteStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(deleteStatement, command);
                    int affectedRows = await command.ExecuteNonQueryAsync();
                }
                GRExecutionStatistics stats = ParseDeleteStatistics(connection, deleteStatement);
                LogSuccessfulDeleteStats(stats);
                return stats;
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while asynchronous deleting entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics statistics = new GRExecutionStatistics(null, deleteStatement.ReadableStatement, null);
                LogFailedUpdateStats(statistics, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, statistics, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }

        GRDeleteStatement BuildDeleteStatement<T>(IGRDeletable<T> deletable)
        {
            GRDeleteStatement deleteStatement = new GRDeleteStatement();
            deleteStatement.ParamColumns = new List<GRStatementValue>();

            if (deletable.WhereClauses == null || !deletable.WhereClauses.Any())
            {
                throw new GRQueryBuildFailedException(string.Format("Delete command has not specified any conditions and entity '{0}' has not defined any keys.", deletable.Structure.Type));
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("DELETE FROM [{0}]", deletable.Structure.TableName);

            try
            {
                // constructing WHERE clause
                if (deletable.WhereClauses != null && deletable.WhereClauses.Any())
                {
                    List<string> whereConditions = new List<string>();

                    foreach (var whereClause in deletable.WhereClauses)
                    {
                        string where = BuildQueryConditions(whereClause.Expression.Body, deleteStatement, string.Empty);
                        whereConditions.Add(where);
                    }

                    if (whereConditions.Count == 1)
                    {
                        sb.AppendFormat(" WHERE {0} ", whereConditions.First());
                    }
                    else if (whereConditions.Count > 0)
                    {
                        sb.Append(" WHERE ");
                        for (int i = 0; i < whereConditions.Count; i++)
                        {
                            if (i > 0) sb.Append(" AND ");
                            sb.AppendFormat("({0})", whereConditions[i]);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                throw new GRQueryBuildFailedException(exc, "An error occured while constructing WHERE clause of delete statement.");
            }

            deleteStatement.Statement = sb.ToString();

            return deleteStatement;
        }
        #endregion

        #region Insert methods
        public override void Insert<T>(IGRUpdatable<T> updatable)
        {
            contextQueue.Add(new GRContextQueueItem()
            {
                Item = updatable,
                Action = GRContextQueueAction.Insert
            });
        }

        GRUpdateStatement BuildInsertStatement<T>(IGRUpdatable<T> updatable, string[] propertiesToInsert)
        {
            CallPreSaveMethods(updatable.Entity, GRPreSaveActionType.Insert);

            GRUpdateStatement insertStatement = new GRUpdateStatement();

            // constructing DV assignments => Column = @Param
            List<string> columns = new List<string>();
            List<string> values = new List<string>();

            foreach (var nonKeyProperty in updatable.Structure.InsertProperties)
            {
                if (GRDataTypeHelper.HasAttribute(nonKeyProperty.PropertyInfo, typeof(GRUpdateOnlyAttribute))) continue;
                if (propertiesToInsert != null && !propertiesToInsert.Any(p => nonKeyProperty.PropertyInfo.Name == p)) continue;

                columns.Add(nonKeyProperty.DBColumnName);
                values.Add(string.Format("@{0}{1}{2}", SqlParamPreffix, insertStatement.ParamColumns.Count, SqlParamSuffix));

                if (GRDataTypeHelper.HasAttribute(nonKeyProperty.PropertyInfo, typeof(GRJSONAttribute)))
                {
                    object value = nonKeyProperty.PropertyInfo.GetValue(updatable.Entity);
                    string json = JsonConvert.SerializeObject(value);

                    insertStatement.ParamColumns.Add(new GRStatementValue
                    {
                        Value = json,
                        Property = nonKeyProperty
                    });
                }
                else
                {
                    insertStatement.ParamColumns.Add(new GRStatementValue
                    {
                        Property = nonKeyProperty,
                        Value = nonKeyProperty.PropertyInfo.GetValue(updatable.Entity)
                    });
                }
            }

            if (!columns.Any())
            {
                throw new GRQueryBuildFailedException("There are no values to insert.");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO [{0}] (", updatable.Structure.TableName);
            sb.Append(string.Join(", ", columns.Select(c => string.Format("[{0}]", c))));
            sb.Append(") VALUES (");
            sb.Append(string.Join(", ", values));
            sb.Append(")");
            if (updatable.Structure.HasIdentityProperty)
            {
                sb.Append("; SELECT CAST(SCOPE_IDENTITY() AS INT) AS [SCOPE_IDENTITY];");
            }

            insertStatement.Statement = sb.ToString();

            return insertStatement;
        }

        private GRExecutionStatistics InsertEntity<T>(IGRUpdatable<T> updatable)
        {
            ApplyAutoProperties(updatable.Entity, GRAutoValueApply.BeforeInsert, updatable.Repository);
            string[] propertiesToStore = GetPropertiesToStore(updatable, GRContextQueueAction.Insert);
            GRExecutionStatistics updateResult = ExecuteInsertCommand(updatable, propertiesToStore);
            ApplyAutoProperties<T>(updatable.Entity, GRAutoValueApply.AfterInsert, updatable.Repository);
            updatable.ExecutionStats = updateResult;
            return updateResult;
        }

        private async Task<GRExecutionStatistics> InsertEntityAsync<T>(IGRUpdatable<T> updatable)
        {
            ApplyAutoProperties(updatable.Entity, GRAutoValueApply.BeforeInsert, updatable.Repository);
            string[] propertiesToStore = GetPropertiesToStore(updatable, GRContextQueueAction.Insert);
            GRExecutionStatistics updateResult = await ExecuteInsertCommandAsync(updatable, propertiesToStore);
            ApplyAutoProperties<T>(updatable.Entity, GRAutoValueApply.AfterInsert, updatable.Repository);
            updatable.ExecutionStats = updateResult;
            return updateResult;
        }

        private GRExecutionStatistics ExecuteInsertCommand<T>(IGRUpdatable<T> insert, string[] properties = null)
        {
            GRUpdateStatement insertStatement = BuildInsertStatement<T>(insert, properties);

            this.GRValidate(insert.Entity, insertStatement.ParamColumns.Select(v => v.Property).ToList());

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = new SqlCommand(insertStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(insertStatement, command);

                    if (insert.Structure.HasIdentityProperty)
                    {
                        int newEntityId = (int)command.ExecuteScalar();
                        insert.Structure.IdentityProperty.PropertyInfo.SetValue(insert.Entity, newEntityId);
                    }
                    else
                    {
                        command.ExecuteNonQuery();
                    }
                }
                GRExecutionStatistics stats = ParseInsertStatistics(connection, insertStatement);
                LogSuccessfulUpdateStats(stats);
                return stats;
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while synchronous inserting entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics statistics = new GRExecutionStatistics(null, insertStatement.ReadableStatement, null);
                LogFailedUpdateStats(statistics, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, statistics, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }

        private async Task<GRExecutionStatistics> ExecuteInsertCommandAsync<T>(IGRUpdatable<T> insert, string[] properties = null)
        {
            GRUpdateStatement insertStatement = BuildInsertStatement<T>(insert, properties);

            this.GRValidate(insert.Entity, insertStatement.ParamColumns.Select(v => v.Property).ToList());

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = new SqlCommand(insertStatement.Statement, connection))
                {
                    command.Transaction = sqlTransaction;
                    ReplaceQueryAttributes(insertStatement, command);

                    if (insert.Structure.HasIdentityProperty)
                    {
                        int newEntityId = (int)await command.ExecuteScalarAsync();
                        insert.Structure.IdentityProperty.PropertyInfo.SetValue(insert.Entity, newEntityId);
                    }
                    else
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                GRExecutionStatistics stats = ParseInsertStatistics(connection, insertStatement);
                LogSuccessfulUpdateStats(stats);
                return stats;
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while asynchronous inserting entity '{0}'.", typeof(T).Name);
                GRExecutionStatistics statistics = new GRExecutionStatistics(null, insertStatement.ReadableStatement, null);
                LogFailedUpdateStats(statistics, errMessage, exc);
                throw new GRQueryExecutionFailedException(exc, statistics, errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }

        public GRExecutionStatistics ParseInsertStatistics(SqlConnection connection, GRUpdateStatement statement)
        {
            IDictionary dbStats = connection.RetrieveStatistics();
            connection.ResetStatistics();

            long executionTime = (long)dbStats[SqlKeyExecutionTime];
            long affectedRows = (long)dbStats[SqlKeyAffectedRows];

            return new GRExecutionStatistics(affectedRows, statement.ReadableStatement, executionTime);
        }
        #endregion

        #region Save methods
        public override void SaveChangesInTransaction()
        {
            SqlConnection sqlConnection = GetSqlConnection();

            LogDebug("Transaction - start.");

            try
            {
                BeginTransaction();
            }
            catch (Exception exc)
            {
                LogDebug("Async Transaction - start failed: {0}.", exc.Message);
                LogError(exc, "Async Transaction - start failed.");
                throw;
            }

            try
            {
                SaveChanges();
                CommitTransaction();

                LogDebug("Transaction - success.");
            }
            catch
            {
                LogDebug("Transaction - fail.");
                RollbackTransaction();
                throw;
            }
        }

        public override async Task SaveChangesInTransactionAsync()
        {
            LogDebug("Async Transaction - start.");

            try
            {
                BeginTransaction();
            }
            catch (Exception exc)
            {
                LogDebug("Async Transaction - start failed: {0}.", exc.Message);
                LogError(exc, "Async Transaction - start failed.");
                throw;
            }

            try
            {
                await SaveChangesAsync();
                CommitTransaction();

                LogDebug("Async Transaction - success.");
            }
            catch
            {
                LogDebug("Async Transaction - fail.");
                RollbackTransaction();
                throw;
            }
        }

        string GetMethodName(GRContextQueueAction action, bool isAsync)
        {
            if (isAsync)
            {
                return action.ToString() + "EntityAsync";
            }
            else
            {
                return action.ToString() + "Entity";
            }
        }

        public override void SaveChanges()
        {
            try
            {
                List<GRContextQueueItem> currentContextQueue = new List<GRContextQueueItem>(contextQueue);

                foreach (var item in currentContextQueue)
                {
                    string methodName = GetMethodName(item.Action, false);
                    MethodInfo method = typeof(GRMSSQLContext).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo genericMethod = method.MakeGenericMethod(item.Item.Type);
                    genericMethod.Invoke(this, new object[] { item.Item });
                    contextQueue.Remove(item);
                }
            }
            catch (Exception exc)
            {
                ProcessSaveException(exc);
            }
        }

        public override async Task SaveChangesAsync()
        {
            try
            {
                List<GRContextQueueItem> currentContextQueue = new List<GRContextQueueItem>(contextQueue);

                foreach (var item in currentContextQueue)
                {
                    string methodName = GetMethodName(item.Action, true);
                    MethodInfo method = typeof(GRMSSQLContext).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo genericMethod = method.MakeGenericMethod(item.Item.Type);
                    Task task = (Task)genericMethod.Invoke(this, new object[] { item.Item });
                    await task;
                    contextQueue.Remove(item);
                }
            }
            catch (Exception exc)
            {
                ProcessSaveException(exc);
            }
        }

        void ProcessSaveException(Exception exc)
        {
            if (exc is TargetInvocationException)
            {
                ProcessSaveException(exc.InnerException);
                return;
            }

            if (exc is GRModelNotValidException)
            {
                GRModelNotValidException excModel = exc as GRModelNotValidException;
                LogError(exc, excModel.ToString());
            }

            if (exc is GRQueryExecutionFailedException)
            {
                LogError(exc, exc.Message);
            }

            throw exc;
        }
        #endregion

        #region Stored procedures methods

        public override T GetItemFromSP<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            T ret;

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (IsPrimitiveType<T>())
                        {
                            ret = ParseSPResult<T>(reader, null).FirstOrDefault();
                        }
                        else
                        {
                            GRDBStructure structure = GRDataTypeHelper.GetDBStructure(typeof(T));

                            ret = ParseSPResult<T>(structure.Properties, reader, null).FirstOrDefault();
                        }

                        stats = ParseFnSpStatistics(connection, spCommandStatementReadable);
                    }
                }
                LogSuccessfulQueryStats(stats);
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while get sync item from stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<T> GetItemFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            T ret;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (IsPrimitiveType<T>())
                        {
                            ret = ParseSPResult<T>(reader, null).FirstOrDefault();
                        }
                        else
                        {
                            GRDBStructure structure = GRDataTypeHelper.GetDBStructure(typeof(T));

                            ret = ParseSPResult<T>(structure.Properties, reader, null).FirstOrDefault();
                        }

                        stats = ParseFnSpStatistics(connection, spCommandStatementReadable);
                    }
                }
                LogSuccessfulQueryStats(stats);
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while get asynchronous item from stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override List<T> GetListFromSP<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            List<T> ret = null;

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (IsPrimitiveType<T>())
                        {
                            ret = ParseSPResult<T>(reader, null);
                        }
                        else
                        {
                            GRDBStructure structure = GRDataTypeHelper.GetDBStructure(typeof(T));

                            ret = ParseSPResult<T>(structure.Properties, reader, null);
                        }

                        stats = ParseFnSpStatistics(connection, spCommandStatementReadable);
                    }
                }
                LogSuccessfulQueryStats(stats);
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while get sync list from stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<List<T>> GetListFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            List<T> ret = null;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (IsPrimitiveType<T>())
                        {
                            ret = ParseSPResult<T>(reader, null);
                        }
                        else
                        {
                            GRDBStructure structure = GRDataTypeHelper.GetDBStructure(typeof(T));

                            ret = ParseSPResult<T>(structure.Properties, reader, null);
                        }

                        stats = ParseFnSpStatistics(connection, spCommandStatementReadable);
                    }
                }
                LogSuccessfulQueryStats(stats);
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while get asynchronous list from stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<GRJoinedList> GetJoinedListFromSPAsync(string spName, List<SqlParameter> spParams, Dictionary<string, Type> spPrefixes, GRPropertyCollection properties, int timeout = -1)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            GRJoinedList ret = new GRJoinedList() { Items = new List<GRJoinedListItem>() };

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                using (SqlCommand command = CreateSPCommand(connection, spName, spParams, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        var columnNames = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            columnNames.Add(reader.GetName(i));
                        }

                        while (reader.Read())
                        {
                            GRJoinedListItem listItem = new GRJoinedListItem();

                            foreach (var typePair in spPrefixes)
                            {
                                string prefix = typePair.Key;
                                string commonPrefix = prefix + "_";

                                Type type = typePair.Value;

                                List<string> typeColumns = columnNames.Where(c => c.StartsWith(commonPrefix)).ToList();

                                bool allNull = true;

                                foreach (var typeColumn in typeColumns)
                                {
                                    if (reader[typeColumn] != DBNull.Value)
                                    {
                                        allNull = false;
                                        break;
                                    }
                                }

                                if (allNull)
                                {
                                    continue;
                                }

                                List<GRDBProperty> typeProperties = null;

                                // take only selected
                                if (properties != null)
                                {
                                    typeProperties = properties.GetProperties(type);
                                }
                                // take them all
                                else
                                {
                                    GRDBStructure structure = GRDataTypeHelper.GetDBStructure(type);
                                    typeProperties = structure.Properties;
                                }


                                MethodInfo genericMethod = parseResultLineMethod.MakeGenericMethod(type);

                                object obj = genericMethod.Invoke(this, new object[] { typeProperties, reader, prefix });
                                listItem.Objects.Add(obj);
                            }
                            ret.Items.Add(listItem);
                        }

                        stats = ParseFnSpStatistics(connection, spCommandStatementReadable);
                    }
                }
                LogSuccessfulQueryStats(stats);
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while get asynchronous joined list from stored procedure '{0}'.", spName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override DataSet GetDataSetFromSP(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            DataSet ret = new DataSet();

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        da.SelectCommand.CommandTimeout = timeout < 0 ? connection.ConnectionTimeout : timeout;
                        da.Fill(ret);
                    }
                }
                stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                LogSuccessfulQueryStats(stats);

                return ret;
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing synchronous stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }

        public override DataTable GetDataTableFromSP(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters, int timeout = -1)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            DataTable ret = new DataTable();

            SqlConnection connection = null;

            try
            {
                if (parameters != null && returnParameters == null)
                {
                    returnParameters = new List<SqlParameter>();
                }

                connection = GetSqlConnection();

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        ret.Load(dr);
                    }

                    if (parameters != null)
                    {
                        returnParameters.AddRange(parameters.Where(x => x.Direction == ParameterDirection.Output || x.Direction == ParameterDirection.InputOutput));
                    }
                }
                stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                LogSuccessfulQueryStats(stats);

                return ret;
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing synchronous stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }


        public override async Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, int timeout = -1)
        {
            return await GetDataTableFromSPAsync(storedProcedureName, null, null, timeout);
        }
        public override async Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1)
        {
            return await GetDataTableFromSPAsync(storedProcedureName, parameters, null, timeout);
        }
        public override async Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters, int timeout = -1)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            DataTable ret = new DataTable();
            SqlConnection connection = null;

            try
            {
                if (parameters != null && returnParameters == null)
                {
                    returnParameters = new List<SqlParameter>();
                }

                connection = await GetSqlConnectionAsync();

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader dr = await command.ExecuteReaderAsync())
                    {
                        ret.Load(dr);
                    }

                    if (parameters != null)
                    {
                        returnParameters.AddRange(parameters.Where(x => x.Direction == ParameterDirection.Output || x.Direction == ParameterDirection.InputOutput));
                    }
                }
                stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                LogSuccessfulQueryStats(stats);

                return ret;
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }

        public override async Task<List<SqlParameter>> ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout = -1)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            List<SqlParameter> ret = null;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    await command.ExecuteNonQueryAsync();

                    if (parameters != null)
                    {
                        List<SqlParameter> outParams = parameters.Where(x => x.Direction == ParameterDirection.Output || x.Direction == ParameterDirection.InputOutput).ToList();

                        if (outParams.Count > 0)
                        {
                            ret = outParams;
                        }
                    }
                }
                stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                LogSuccessfulQueryStats(stats);
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<MemoryStream> GetMemoryStreamFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, CommandBehavior? commandBehavior = null)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            SqlDataReader sr;
            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, 3600);
                spCommandStatementReadable = GetSPCommandStatement(command);

                if (!commandBehavior.HasValue)
                    sr = await command.ExecuteReaderAsync();
                else
                    sr = await command.ExecuteReaderAsync(commandBehavior.Value);

                stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                LogSuccessfulQueryStats(stats);

                if (await sr.ReadAsync())
                {
                    if (!(await sr.IsDBNullAsync(0)))
                    {
                        Stream stream = sr.GetStream(0);

                        MemoryStream ms = new MemoryStream();
                        stream.CopyTo(ms);

                        ms.Position = 0;

                        return ms;
                    }
                }

                return null;
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }

        public override async Task<Stream> GetZipStreamFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, CommandBehavior? commandBehavior = null, string fileName = "default")
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            SqlDataReader sr;
            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, 3600);
                spCommandStatementReadable = GetSPCommandStatement(command);

                if (!commandBehavior.HasValue)
                    sr = await command.ExecuteReaderAsync();
                else
                    sr = await command.ExecuteReaderAsync(commandBehavior.Value);

                stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                LogSuccessfulQueryStats(stats);

                if (await sr.ReadAsync())
                {
                    if (!(await sr.IsDBNullAsync(0)))
                    {
                        Stream stream = sr.GetStream(0);

                        Stream ret = new MemoryStream();
                        using (ZipOutputStream zip = new ZipOutputStream(ret, true))
                        {
                            zip.PutNextEntry(fileName);
                            stream.CopyTo(zip);
                        }

                        ret.Position = 0;

                        return ret;
                    }
                }

                return null;
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
        }

        private SqlCommand CreateSPCommand(SqlConnection connection, string storedProcedureName, List<SqlParameter> parameters, SqlTransaction trans, int timeout = -1)
        {
            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.CommandText = storedProcedureName;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = timeout < 0 ? connection.ConnectionTimeout : timeout;
            command.Transaction = trans;

            if (parameters != null)
                command.Parameters.AddRange(parameters.ToArray());

            return command;
        }

        private string GetSPCommandStatement(SqlCommand command)
        {
            string ret = string.Format("exec {0}", command.CommandText);

            if (command.Parameters != null)
            {
                foreach (SqlParameter param in command.Parameters)
                {
                    ret += string.Format(" {0}={1},", param.ParameterName, GRDataTypeHelper.GetObjectValueString(param.Value != DBNull.Value ? param.Value : null));
                }

                ret = ret.TrimEnd(new char[] { ',' });
            }

            return ret;
        }

        private List<T> ParseSPResult<T>(SqlDataReader reader, string prefix)
        {
            List<T> ret = new List<T>();

            try
            {
                while (reader.Read())
                {
                    ret.Add((T)Convert.ChangeType(reader[0], typeof(T)));
                }
            }
            finally
            {
                reader.Close();
            }

            return ret;
        }
        private List<T> ParseSPResult<T>(List<GRDBProperty> columns, SqlDataReader reader, string prefix)
        {
            List<T> ret = new List<T>();

            try
            {
                while (reader.Read())
                {
                    T t = Activator.CreateInstance<T>();
                    foreach (var property in columns)
                    {
                        string columnName = GetPrefixedResultingColumn(prefix, property.DBColumnName);
                        object value = null;

                        try
                        {
                            value = reader[columnName];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            if (IsNullableProperty(property)) continue;
                            throw new GRUnknownColumnException(columnName);
                        }

                        if (value.Equals(DBNull.Value))
                        {
                            continue;
                        }

                        if (property.PropertyInfo.PropertyType == typeof(Stream))
                        {
                            Stream stream = new MemoryStream(value as byte[]);
                            value = stream;
                        }

                        if (GRDataTypeHelper.HasAttribute(property.PropertyInfo, typeof(GRJSONAttribute)))
                        {
                            value = JsonConvert.DeserializeObject((string)value, property.PropertyInfo.PropertyType);
                        }

                        property.PropertyInfo.SetValue(t, value);
                    }

                    ApplyAutoProperties<T>(t, GRAutoValueApply.AfterSelect, null);

                    ret.Add(t);
                }
            }
            finally
            {
                reader.Close();
            }

            return ret;
        }

        public GRExecutionStatistics ParseFnSpStatistics(SqlConnection connection, string statement)
        {
            IDictionary dbStats = connection.RetrieveStatistics();
            connection.ResetStatistics();

            long executionTime = (long)dbStats[SqlKeyExecutionTime];
            long numberOfRows = (long)dbStats[SqlKeyNumberOfRows];

            return new GRExecutionStatistics(numberOfRows, statement, executionTime);
        }
        #endregion

        #region Scalar functions methods
        public override T ExecuteScalarFunction<T>(string functionName, List<SqlParameter> parameters)
        {
            string fnCommandStatementReadable = string.Empty;
            GRDBStructure structure = GRDataTypeHelper.GetDBStructure(typeof(T));
            GRExecutionStatistics stats = null;

            T ret = Activator.CreateInstance<T>();
            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();
                using (SqlCommand command = CreateFnCommand(connection, functionName, parameters))
                {
                    fnCommandStatementReadable = GetFnCommandStatement(command);

                    ret = (T)command.ExecuteScalar();
                }

                stats = ParseFnSpStatistics(connection, fnCommandStatementReadable);

                LogSuccessfulQueryStats(stats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing synchronous scalar function '{0}'.", functionName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, fnCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<T> ExecuteScalarFunctionAsync<T>(string functionName, List<SqlParameter> parameters)
        {
            string fnCommandStatementReadable = string.Empty;
            GRDBStructure structure = GRDataTypeHelper.GetDBStructure(typeof(T));
            GRExecutionStatistics stats = null;

            T ret = default(T);

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();
                using (SqlCommand command = CreateFnCommand(connection, functionName, parameters))
                {
                    fnCommandStatementReadable = GetFnCommandStatement(command);

                    object fnRet = await command.ExecuteScalarAsync();
                    ret = (T)fnRet;
                }
                stats = ParseFnSpStatistics(connection, fnCommandStatementReadable);

                LogSuccessfulQueryStats(stats);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing asynchronous scalar function '{0}'.", functionName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, fnCommandStatementReadable, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }

            return ret;
        }

        private SqlCommand CreateFnCommand(SqlConnection connection, string functionName, List<SqlParameter> parameters)
        {
            SqlCommand command = new SqlCommand();
            command.Connection = connection;

            string paramsString = parameters == null ? string.Empty : string.Join(",", parameters.Select(x => x.ParameterName));

            command.CommandText = string.Format("select dbo.{0}({1})", functionName, paramsString);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = connection.ConnectionTimeout;
            command.Transaction = sqlTransaction;

            if (parameters != null)
                command.Parameters.AddRange(parameters.ToArray());

            return command;
        }

        private string GetFnCommandStatement(SqlCommand command)
        {
            string ret = command.CommandText;

            if (command.Parameters != null)
            {
                foreach (SqlParameter param in command.Parameters)
                {
                    ret = ret.Replace(param.ParameterName, GRDataTypeHelper.GetObjectValueString(param.Value != DBNull.Value ? param.Value : null));
                }
            }

            return ret;
        }
        #endregion

        #region Query store methods
        public override async Task<Tuple<string, DataSet>> GetQueryStoreStatementResultSetAsync(string statement, bool returnTables = true)
        {
            GRExecutionStatistics stats = null;

            string message = string.Empty;
            DataSet ds = new DataSet();

            SqlInfoMessageEventHandler infoHandler = (sender, e) =>
            {
                message += e.Message + "\r\n";
            };

            try
            {
                return await Task<Tuple<string, DataSet>>.Factory.StartNew(() =>
                {
                    SqlConnection connection = null;

                    try
                    {
                        connection = GetSqlConnection();

                        using (SqlCommand command = new SqlCommand(statement, connection, sqlTransaction))
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandTimeout = connection.ConnectionTimeout;

                            connection.InfoMessage += infoHandler;
                            if (returnTables)
                            {
                                using (SqlDataAdapter da = new SqlDataAdapter(command))
                                {
                                    da.Fill(ds);
                                }
                            }
                            else
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                        stats = ParseFnSpStatistics(connection, statement);

                        LogSuccessfulQueryStats(stats);

                        return new Tuple<string, DataSet>(message.TrimEnd(Environment.NewLine.ToCharArray()), ds);
                    }
                    finally
                    {
                        DisposeConnection(connection);
                    }
                });
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing query store statement '{0}'.", statement);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, statement, null), errMessage);
            }
        }
        #endregion

        #region Transactions
        public override void BeginTransaction()
        {
            if (sqlTransaction != null)
            {
                throw new GRInvalidOperationException("Could not begin new transaction, another transaction is in progress.");
            }

            semaphoreConnection.Wait();

            try
            {
                // make new shared connection
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                sqlTransaction = sqlConnection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
            }
            finally
            {
                semaphoreConnection.Release();
            }
        }

        public override void CommitTransaction()
        {
            semaphoreConnection.Wait();

            try
            {
                sqlTransaction.Commit();

                // dispose resources
                sqlConnection.Dispose();
                sqlTransaction = null;
                sqlConnection = null;
            }
            finally
            {
                semaphoreConnection.Release();
            }
        }

        public override void RollbackTransaction()
        {
            semaphoreConnection.Wait();

            try
            {
                sqlTransaction.Rollback();

                // dispose resources
                sqlConnection.Dispose();
                sqlTransaction = null;
                sqlConnection = null;
            }
            finally
            {
                semaphoreConnection.Release();
            }
        }
        #endregion
    }
}
