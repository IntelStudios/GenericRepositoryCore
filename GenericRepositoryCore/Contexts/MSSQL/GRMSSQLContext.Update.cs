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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        public override void EnqueueUpdate<T>(IGRUpdatable<T> updatable)
        {
            contextQueue.Add(new GRContextQueueItem()
            {
                Item = updatable,
                Action = GRContextQueueAction.Update
            });
        }

        private GRExecutionStatistics UpdateEntity<T>(IGRUpdatable<T> updatable)
        {
            GRDataTypeHelper.ApplyAutoProperties(updatable.Entity, GRAutoValueApply.BeforeUpdate, updatable.Repository);
            string[] propertiesToStore = GetPropertiesToSave(updatable, GRContextQueueAction.Update);
            GRExecutionStatistics updateResult = ExecuteUpdateCommand(updatable, propertiesToStore);
            updatable.ExecutionStats = updateResult;
            return updateResult;
        }

        private async Task<GRExecutionStatistics> UpdateEntityAsync<T>(IGRUpdatable<T> updatable)
        {
            GRDataTypeHelper.ApplyAutoProperties(updatable.Entity, GRAutoValueApply.BeforeUpdate, updatable.Repository);
            string[] propertiesToStore = GetPropertiesToSave(updatable, GRContextQueueAction.Insert);
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
                valueAssignments.Add(string.Format("[{0}] = @{1}{2}{3}", nonKeyProperty.DBColumnName, SqlParamPreffix, updateStatement.Params.Count, SqlParamSuffix));

                if (GRDataTypeHelper.HasAttribute(nonKeyProperty.PropertyInfo, typeof(GRJSONAttribute)))
                {
                    object value = nonKeyProperty.PropertyInfo.GetValue(update.Entity);
                    string json = JsonConvert.SerializeObject(value);

                    updateStatement.Params.Add(new GRStatementParam
                    {
                        Value = json,
                        Property = nonKeyProperty
                    });
                }
                else
                {
                    updateStatement.Params.Add(new GRStatementParam
                    {
                        Property = nonKeyProperty,
                        Value = nonKeyProperty.PropertyInfo.GetValue(update.Entity)
                    });
                }

                updateStatement.Columns.Add(nonKeyProperty, new GRDBQueryProperty(nonKeyProperty));
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
                sb.AppendFormat("[{0}] = @{1}{2}{3}", prop.DBColumnName, SqlParamPreffix, updateStatement.Params.Count, SqlParamSuffix);

                updateStatement.Params.Add(new GRStatementParam
                {
                    Value = prop.PropertyInfo.GetValue(update.Entity),
                    Property = prop
                });
            }

            updateStatement.Statement = sb.ToString();

            return updateStatement;
        }

        private GRExecutionStatistics ExecuteUpdateCommand<T>(IGRUpdatable<T> updatable, string[] properties = null)
        {
            GRUpdateStatement updateStatement = BuildUpdateStatement<T>(updatable, properties);

            this.ValidateModel(updatable.Entity, updateStatement.Params.Select(v => v.Property).ToList());

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

            this.ValidateModel(updatable.Entity, updateStatement.Params.Select(v => v.Property).ToList());

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
    }
}
