using GenericRepository.Interfaces;
using GenericRepository.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public abstract partial class GRContext : IGRContext
    {
        private const int defaultTimeout = -1;

        #region Executing SP without parsing result
        public virtual Task ExecuteSPAsync(string storedProcedureName)
        {
            return ExecuteSPAsync(storedProcedureName, null, defaultTimeout, null);
        }
        public virtual Task ExecuteSPAsync(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return ExecuteSPAsync(storedProcedureName, null, defaultTimeout, infoMessageHandler);
        }
        public virtual Task ExecuteSPAsync(string storedProcedureName, int timeout)
        {
            return ExecuteSPAsync(storedProcedureName, null, timeout, null);
        }
        public virtual Task ExecuteSPAsync(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return ExecuteSPAsync(storedProcedureName, null, timeout, infoMessageHandler);
        }
        public virtual Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters)
        {
            return ExecuteSPAsync(storedProcedureName, parameters, defaultTimeout, null);
        }
        public virtual Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return ExecuteSPAsync(storedProcedureName, parameters, defaultTimeout, infoMessageHandler);
        }
        public virtual Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return ExecuteSPAsync(storedProcedureName, parameters, timeout, null);
        }
        public abstract Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Executing SP with output params
        public virtual Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName)
        {
            return ExecuteSPWithOutParamsAsync(storedProcedureName, null, defaultTimeout, null);
        }
        public virtual Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return ExecuteSPWithOutParamsAsync(storedProcedureName, null, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, int timeout)
        {
            return ExecuteSPWithOutParamsAsync(storedProcedureName, null, timeout, null);
        }
        public virtual Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return ExecuteSPWithOutParamsAsync(storedProcedureName, null, timeout, infoMessageHandler);
        }
        public virtual Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters)
        {
            return ExecuteSPWithOutParamsAsync(storedProcedureName, parameters, defaultTimeout, null);
        }
        public virtual Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return ExecuteSPWithOutParamsAsync(storedProcedureName, parameters, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return ExecuteSPWithOutParamsAsync(storedProcedureName, parameters, timeout, null);
        }
        public abstract Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Getting single entity from SP with single JSON output param

        public virtual Task<T> GetEntityFromSPWithSingleJsonOutParamAsync<T>(string storedProcedureName, List<SqlParameter> parameters)
        {
            return ExecuteSPWithSingleJsonOutParamAsync<T>(storedProcedureName, parameters, defaultTimeout, null);
        }
        #endregion

        #region Getting entities from SP with single JSON output param
        public virtual Task<List<T>> GetEntitiesFromSPWithSingleJsonOutParamAsync<T>(string storedProcedureName, List<SqlParameter> parameters)
        {
            return ExecuteSPWithSingleJsonOutParamAsync<List<T>>(storedProcedureName, parameters, defaultTimeout, null);
        }

        public abstract Task<T> ExecuteSPWithSingleJsonOutParamAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Getting single value from SP
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout, null);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, int timeout)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, null, timeout, null);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, null, timeout, infoMessageHandler);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout, null);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, parameters, timeout, null);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, parameters, timeout, infoMessageHandler);
        }

        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, columnName, null, defaultTimeout, null);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, columnName, null, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, int timeout)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, columnName, null, timeout, null);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, columnName, null, timeout, infoMessageHandler);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, columnName, parameters, defaultTimeout, null);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, columnName, parameters, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, columnName, parameters, timeout, null);
        }
        public abstract Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Getting list of values from SP
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout, null);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, int timeout)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, null, timeout, null);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, null, timeout, infoMessageHandler);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout, null);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, parameters, timeout, null);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, parameters, timeout, infoMessageHandler);
        }

        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, columnName, null, defaultTimeout, null);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, columnName, null, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, int timeout)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, columnName, null, timeout, null);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, columnName, null, timeout, infoMessageHandler);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, columnName, parameters, defaultTimeout, null);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, columnName, parameters, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, columnName, parameters, timeout, null);
        }
        public abstract Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Getting single entity from SP
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout, null);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, int timeout)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, null, timeout, null);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, null, timeout, infoMessageHandler);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout, null);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, parameters, timeout, null);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, parameters, timeout, infoMessageHandler);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, prefix, null, defaultTimeout, null);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, prefix, null, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, int timeout)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, prefix, null, timeout, null);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, prefix, null, timeout, infoMessageHandler);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, prefix, parameters, defaultTimeout, null);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, prefix, parameters, timeout, null);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, prefix, parameters, defaultTimeout, infoMessageHandler);
        }
        public abstract Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Getting list of entities from SP
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout, null);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, int timeout)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, null, timeout, null);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, null, timeout, infoMessageHandler);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout, null);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, parameters, timeout, null);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, parameters, timeout, null);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, prefix, null, defaultTimeout, null);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, prefix, null, defaultTimeout, null);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, int timeout)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, prefix, null, timeout, null);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, int timeout, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, prefix, null, timeout, null);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, prefix, parameters, defaultTimeout, null);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, prefix, parameters, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, prefix, parameters, timeout, null);
        }
        public abstract Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion 

        #region Getting GRTable of entities from SP
        public virtual Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> spParams, GRPropertyCollection properties)
        {
            return GetEntitiesFromSPAsync(storedProcedureName, spParams, properties, defaultTimeout, null);
        }
        public virtual Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> spParams, GRPropertyCollection properties, SqlInfoMessageEventHandler infoMessageHandler)
        {
            return GetEntitiesFromSPAsync(storedProcedureName, spParams, properties, defaultTimeout, infoMessageHandler);
        }
        public virtual Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> spParams, GRPropertyCollection properties, int timeout)
        {
            return GetEntitiesFromSPAsync(storedProcedureName, spParams, properties, timeout, null);
        }
        public abstract Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> spParams, GRPropertyCollection properties, int timeout, SqlInfoMessageEventHandler infoMessageHandler);
        #endregion

        #region Getting data sets and tables from SP
        public virtual DataSet GetDataSetFromSP(string storedProcedureName, List<SqlParameter> parameters)
        {
            return GetDataSetFromSP(storedProcedureName, parameters, defaultTimeout);
        }
        public abstract DataSet GetDataSetFromSP(string storedProcedureName, List<SqlParameter> parameters, int timeout);
        #endregion

        #region Getting data tables from SP
        public virtual DataTable GetDataTableFromSP(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters)
        {
            return GetDataTableFromSP(storedProcedureName, parameters, returnParameters, defaultTimeout);
        }
        public abstract DataTable GetDataTableFromSP(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters, int timeout);

        public virtual Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName)
        {
            return GetDataTableFromSPAsync(storedProcedureName, null, null, defaultTimeout);
        }
        public virtual Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, int timeout)
        {
            return GetDataTableFromSPAsync(storedProcedureName, null, null, timeout);
        }
        public virtual Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters)
        {
            return GetDataTableFromSPAsync(storedProcedureName, parameters, null, defaultTimeout);
        }
        public virtual Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return GetDataTableFromSPAsync(storedProcedureName, parameters, null, timeout);
        }
        public virtual Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters)
        {
            return GetDataTableFromSPAsync(storedProcedureName, parameters, returnParameters, defaultTimeout);
        }
        public abstract Task<DataTable> GetDataTableFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, List<SqlParameter> returnParameters, int timeout);
        #endregion

        #region Getting streams SP
        public abstract Task<MemoryStream> GetMemoryStreamFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, CommandBehavior? commandBehavior = null);
        public abstract Task<Stream> GetZipStreamFromSPAsync(string storedProcedureName, List<SqlParameter> parameters, CommandBehavior? commandBehavior = null, string fileName = "default");
        #endregion
    }
}
