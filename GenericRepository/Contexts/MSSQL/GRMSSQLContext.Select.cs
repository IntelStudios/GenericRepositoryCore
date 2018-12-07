using GenericRepository.Attributes;
using GenericRepository.Enums;
using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public override List<T> ExecuteQuery<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement(queriable, forceIdentityColumn: false);

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
                        ret = ParseListOfEntities<T>(reader, queriable.Prefix, queryStatement.Columns, applyAutoProperties: true);
                    }
                    ret.ForEach(t => GRDataTypeHelper.ApplyAutoProperties(t, GRAutoValueApply.AfterSelect, queriable.Repository));
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

        public override async Task<List<T>> ExecuteQueryAsync<T>(IGRQueriable<T> queriable)
        {
            GRQueryStatement queryStatement = BuildQueryStatement(queriable, isCountQuery: false, forceIdentityColumn: false);

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
                        ret = ParseListOfEntities<T>(reader, queriable.Prefix, queryStatement.Columns, applyAutoProperties: true);
                    }

                    ret.ForEach(t => GRDataTypeHelper.ApplyAutoProperties(t, GRAutoValueApply.AfterSelect, queriable.Repository));
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

        public override List<T> ExecuteQuery<T>(string sqlStatement)
        {
            IGRQueriable<T> queriable = new GRQueriable<T>();

            GRQueryStatement queryStatement = new GRQueryStatement
            {
                Columns = GetQueryColumns(GRDataTypeHelper.GetDBStructure(typeof(T)), queriable, forceIdentityColumn: false, excluded: false),
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
                        ret = ParseListOfEntities<T>(reader, queriable.Prefix, queryStatement.Columns, applyAutoProperties: true);
                    }
                    ret.ForEach(t => GRDataTypeHelper.ApplyAutoProperties(t, GRAutoValueApply.AfterSelect, queriable.Repository));
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
                Columns = GetQueryColumns(GRDataTypeHelper.GetDBStructure(typeof(T)), queriable, forceIdentityColumn: false, excluded: false),
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
                        ret = ParseListOfEntities<T>(reader, queriable.Prefix, queryStatement.Columns, applyAutoProperties: true);
                    }

                    ret.ForEach(t => GRDataTypeHelper.ApplyAutoProperties(t, GRAutoValueApply.AfterSelect, queriable.Repository));
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


        public override string ExecuteNonQuery(string sqlStatement, bool returnMessage)
        {
            SqlConnection connection = null;
            StringBuilder sb = null;

            try
            {
                connection = GetSqlConnection();

                if (returnMessage)
                {
                    sb = new StringBuilder();

                    connection.InfoMessage += (sender, e) =>
                    {
                        sb.AppendLine(e.Message);
                    };
                }

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

            if (sb == null)
            {
                return null;
            }
            else
            {
                return sb.ToString();
            }
        }

        public override async Task<string> ExecuteNonQueryAsync(string sqlStatement, bool returnMessage)
        {
            SqlConnection connection = null;
            StringBuilder sb = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                if (returnMessage)
                {
                    sb = new StringBuilder();

                    connection.InfoMessage += (sender, e) =>
                    {
                        sb.AppendLine(e.Message);
                    };
                }

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

            if (sb == null)
            {
                return null;
            }
            else
            {
                return sb.ToString();
            }
        }


        public override async Task<GRTable> ExecuteJoinQueryAsync(IGRQueriable leafQueriable)
        {
            IGRQueriable rootQueriable = GetRootQueriable(leafQueriable);
            GRMergedQueryStatement queryStatement = BuildJoinQueryStatement(rootQueriable);

            GRTable ret = new GRTable()
            {
                Rows = new List<GRTableRow>()
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
                            GRTableRow listItem = new GRTableRow();

                            foreach (GRQueryStatement statement in queryStatement.Statements)
                            {
                                if (!statement.Columns.Any() || statement.IsExcluded)
                                {
                                    continue;
                                }

                                object obj = ParseEntity(statement.Type, reader, statement.Prefix, statement.Columns, applyAutoProperties: true);

                                listItem.Items.Add(new GRTableItem
                                {
                                    Entity = obj,
                                    Prefix = statement.HasTemporaryPrefix ? null : statement.Prefix,
                                    Type = statement.Type
                                });

                            }
                            ret.Rows.Add(listItem);
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

        public override GRTable ExecuteJoinQuery(IGRQueriable leafQueriable)
        {
            IGRQueriable rootQueriable = GetRootQueriable(leafQueriable);
            GRMergedQueryStatement queryStatement = BuildJoinQueryStatement(rootQueriable);

            GRTable ret = new GRTable()
            {
                Rows = new List<GRTableRow>()
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
                            GRTableRow listItem = new GRTableRow();

                            foreach (GRQueryStatement statement in queryStatement.Statements)
                            {
                                if (!statement.Columns.Any())
                                {
                                    continue;
                                }

                                object obj = ParseEntity(statement.Type, reader, statement.Prefix, statement.Columns, applyAutoProperties: true);

                                listItem.Items.Add(new GRTableItem
                                {
                                    Entity = obj,
                                    Prefix = statement.HasTemporaryPrefix ? null : statement.Prefix,
                                    Type = statement.Type
                                });
                            }
                            ret.Rows.Add(listItem);
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
    }
}
