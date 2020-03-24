using GenericRepository.Exceptions;
using GenericRepository.Interfaces;
using GenericRepository.Models;
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
        public override void EnqueueDelete<T>(IGRDeletable<T> updatable)
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

        GRDeleteStatement BuildDeleteStatement<T>(IGRDeletable<T> deletable)
        {
            GRDeleteStatement deleteStatement = new GRDeleteStatement();
            deleteStatement.Params = new List<GRStatementParam>();

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

        public override GRExecutionStatistics Execute<T>(IGRDeletable<T> deletable)
        {
            if (deletable.Entity != null)
            {
                contextQueue.Dequeue(deletable);
            }
            return DeleteEntity(deletable);
        }

        public override async Task<GRExecutionStatistics> ExecuteAsync<T>(IGRDeletable<T> deletable)
        {
            if (deletable.Entity != null)
            {
                contextQueue.Dequeue(deletable);
            }
            return await DeleteEntityAsync(deletable);
        }
    }
}
