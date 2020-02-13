using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        public override async Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;
            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                if (infoMessageHandler != null)
                {
                    connection.InfoMessage += infoMessageHandler;
                }

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                    stats = ParseFnSpStatistics(connection, spCommandStatementReadable);
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
                string errMessage = string.Format("An unknown error occured while executing stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                if (infoMessageHandler != null)
                {
                    connection.InfoMessage -= infoMessageHandler;
                }

                DisposeConnection(connection);
            }
        }

        public override async Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            T ret;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                if (infoMessageHandler != null)
                {
                    connection.InfoMessage += infoMessageHandler;
                }

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            return default(T);
                        }

                        ret = ParseValue<T>(reader, columnName);
                        stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                        await reader.NextResultAsync();
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
                string errMessage = string.Format("An unknown error occured while getting value from stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                if (infoMessageHandler != null)
                {
                    connection.InfoMessage -= infoMessageHandler;
                }

                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            List<T> ret = null;

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                if (infoMessageHandler != null)
                {
                    connection.InfoMessage += infoMessageHandler;
                }

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        ret = ParseListOfValues<T>(reader, columnName);
                        stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                        await reader.NextResultAsync();
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
                string errMessage = string.Format("An unknown error occured while getting list of values from stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                if (infoMessageHandler != null)
                {
                    connection.InfoMessage -= infoMessageHandler;
                }

                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            T ret;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                if (infoMessageHandler != null)
                {
                    connection.InfoMessage += infoMessageHandler;
                }

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            return default(T);
                        }

                        ret = ParseEntity<T>(reader, prefix, null, false);
                        stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                        await reader.NextResultAsync();
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
                string errMessage = string.Format("An unknown error occured while getting entity from stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                if (infoMessageHandler != null)
                {
                    connection.InfoMessage -= infoMessageHandler;
                }

                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            List<T> ret = null;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                if (infoMessageHandler != null)
                {
                    connection.InfoMessage += infoMessageHandler;
                }

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        ret = ParseListOfEntities<T>(reader, prefix, null, false);
                        stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                        await reader.NextResultAsync();
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
                string errMessage = string.Format("An unknown error occured while getting list of entities from stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                if (infoMessageHandler != null)
                {
                    connection.InfoMessage -= infoMessageHandler;
                }

                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, GRPropertyCollection properties, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            GRTable ret = new GRTable
            {
                Rows = new List<GRTableRow>()
            };

            SqlConnection connection = null;

            try
            {
                connection = GetSqlConnection();

                if (infoMessageHandler != null)
                {
                    connection.InfoMessage += infoMessageHandler;
                }

                using (SqlCommand command = CreateSPCommand(connection, storedProcedureName, parameters, sqlTransaction, timeout))
                {
                    spCommandStatementReadable = GetSPCommandStatement(command);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            GRTableRow row = new GRTableRow();

                            foreach (GRPropertyCollectionItem property in properties)
                            {
                                object obj = ParseEntity(property.Type, reader, property.Prefix, property.Properties, property.ApplyAutoProperties);

                                row.Items.Add(new GRTableItem
                                {
                                    Entity = obj,
                                    Prefix = property.Prefix,
                                    Type = property.Type
                                });
                            }

                            ret.Rows.Add(row);
                        }
                        stats = ParseFnSpStatistics(connection, spCommandStatementReadable);

                        await reader.NextResultAsync();
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
                string errMessage = string.Format("An unknown error occured while getting list of multiple entities from stored procedure '{0}'.", storedProcedureName);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, spCommandStatementReadable, null), errMessage);
            }
            finally
            {
                if (infoMessageHandler != null)
                {
                    connection.InfoMessage -= infoMessageHandler;
                }

                DisposeConnection(connection);
            }

            return ret;
        }

        public override async Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            string spCommandStatementReadable = string.Empty;
            GRExecutionStatistics stats = null;

            List<SqlParameter> ret = null;

            SqlConnection connection = null;

            try
            {
                connection = await GetSqlConnectionAsync();

                if (infoMessageHandler != null)
                {
                    connection.InfoMessage += infoMessageHandler;
                }

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
                if (infoMessageHandler != null)
                {
                    connection.InfoMessage -= infoMessageHandler;
                }

                DisposeConnection(connection);
            }

            return ret;
        }

     
        #region Data sets and tables
        public override GRDataSet GetDataSetFromCommand(string commandString, bool returnMessage, int timeout)
        {
            GRDataSet ret = new GRDataSet()
            {
                DataSet = new DataSet()
            };

            SqlConnection connection = null;
            GRExecutionStatistics stats = null;
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

                using (SqlCommand command = new SqlCommand(commandString))
                {
                    command.Connection = connection;

                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        da.SelectCommand.CommandTimeout = timeout < 0 ? connection.ConnectionTimeout : timeout;
                        da.Fill(ret.DataSet);
                    }
                }

                stats = ParseFnSpStatistics(connection, commandString);

                LogSuccessfulQueryStats(stats);

                if (sb != null)
                {
                    ret.Message = sb.ToString();
                }

                return ret;
            }
            catch (SqlException exc)
            {
                LogFailedQueryStats(stats, exc.Message, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, commandString, null), exc.Message);
            }
            catch (Exception exc)
            {
                string errMessage = string.Format("An unknown error occured while executing command '{0}'.", commandString);
                LogFailedQueryStats(stats, errMessage, exc);

                throw new GRQueryExecutionFailedException(exc, new GRExecutionStatistics(null, commandString, null), errMessage);
            }
            finally
            {
                DisposeConnection(connection);
            }
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
        #endregion

        #region Streams
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
        #endregion

        #region Helper methods
        private SqlCommand CreateSPCommand(SqlConnection connection, string storedProcedureName, List<SqlParameter> parameters, SqlTransaction trans, int timeout)
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
                    ret += string.Format(" {0}={1},", param.ParameterName, GRDataTypeHelper.GetValueString(param.Value != DBNull.Value ? param.Value : null));
                }

                ret = ret.TrimEnd(new char[] { ',' });
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        List<string> GetColumnNames(SqlDataReader reader)
        {
            List<string> columnNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }
            return columnNames;
        }
        #endregion
    }
}
