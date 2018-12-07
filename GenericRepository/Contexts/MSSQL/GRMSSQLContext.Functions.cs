using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        public override T ExecuteScalarFunction<T>(string functionName, List<SqlParameter> parameters)
        {
            string fnCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            T ret = default(T);

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
                    ret = ret.Replace(param.ParameterName, GRDataTypeHelper.GetValueString(param.Value != DBNull.Value ? param.Value : null));
                }
            }

            return ret;
        }
    }
}
