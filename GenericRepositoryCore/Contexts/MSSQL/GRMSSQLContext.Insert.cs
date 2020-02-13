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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        public override void EnqueueInsert<T>(IGRUpdatable<T> updatable)
        {
            contextQueue.Add(new GRContextQueueItem()
            {
                Item = updatable,
                Action = GRContextQueueAction.Insert
            });
        }

        private GRExecutionStatistics InsertEntity<T>(IGRUpdatable<T> updatable)
        {
            GRDataTypeHelper.ApplyAutoProperties(updatable.Entity, GRAutoValueApply.BeforeInsert, updatable.Repository);
            string[] propertiesToStore = GetPropertiesToSave(updatable, GRContextQueueAction.Insert);
            GRExecutionStatistics updateResult = ExecuteInsertCommand(updatable, propertiesToStore);
            GRDataTypeHelper.ApplyAutoProperties(updatable.Entity, GRAutoValueApply.AfterInsert, updatable.Repository);
            updatable.ExecutionStats = updateResult;
            return updateResult;
        }

        private async Task<GRExecutionStatistics> InsertEntityAsync<T>(IGRUpdatable<T> updatable)
        {
            GRDataTypeHelper.ApplyAutoProperties(updatable.Entity, GRAutoValueApply.BeforeInsert, updatable.Repository);
            string[] propertiesToStore = GetPropertiesToSave(updatable, GRContextQueueAction.Insert);
            GRExecutionStatistics updateResult = await ExecuteInsertCommandAsync(updatable, propertiesToStore);
            GRDataTypeHelper.ApplyAutoProperties(updatable.Entity, GRAutoValueApply.AfterInsert, updatable.Repository);
            updatable.ExecutionStats = updateResult;
            return updateResult;
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
                values.Add(string.Format("@{0}{1}{2}", SqlParamPreffix, insertStatement.Params.Count, SqlParamSuffix));

                if (GRDataTypeHelper.HasAttribute(nonKeyProperty.PropertyInfo, typeof(GRJSONAttribute)))
                {
                    object value = nonKeyProperty.PropertyInfo.GetValue(updatable.Entity);
                    string json = JsonConvert.SerializeObject(value);

                    insertStatement.Params.Add(new GRStatementParam
                    {
                        Value = json,
                        Property = nonKeyProperty
                    });
                }
                else
                {
                    insertStatement.Params.Add(new GRStatementParam
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

        private GRExecutionStatistics ExecuteInsertCommand<T>(IGRUpdatable<T> insert, string[] properties = null)
        {
            GRUpdateStatement insertStatement = BuildInsertStatement<T>(insert, properties);

            this.ValidateModel(insert.Entity, insertStatement.Params.Select(v => v.Property).ToList());

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

            this.ValidateModel(insert.Entity, insertStatement.Params.Select(v => v.Property).ToList());

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
    }
}
