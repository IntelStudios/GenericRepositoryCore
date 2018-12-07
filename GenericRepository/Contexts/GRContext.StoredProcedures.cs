using GenericRepository.Interfaces;
using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public abstract partial class GRContext : IGRContext
    {
        private const int defaultTimeout = -1;

        #region Executing SP without parsing result
        public virtual Task ExecuteSPAsync(string storedProcedureName)
        {
            return ExecuteSPAsync(storedProcedureName, null, defaultTimeout);
        }
        public virtual Task ExecuteSPAsync(string storedProcedureName, int timeout)
        {
            return ExecuteSPAsync(storedProcedureName, null, timeout);
        }
        public virtual Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters)
        {
            return ExecuteSPAsync(storedProcedureName, parameters, defaultTimeout);
        }
        public abstract Task ExecuteSPAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout);
        #endregion

        #region Executing SP with output params
        public virtual Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName)
        {
            return ExecuteSPWithOutParamsAsync(storedProcedureName, null, defaultTimeout);
        }
        public virtual Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, int timeout)
        {
            return ExecuteSPWithOutParamsAsync(storedProcedureName, null, timeout);
        }
        public virtual Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters)
        {
            return ExecuteSPWithOutParamsAsync(storedProcedureName, parameters, defaultTimeout);
        }
        public abstract Task<List<SqlParameter>> ExecuteSPWithOutParamsAsync(string storedProcedureName, List<SqlParameter> parameters, int timeout);
        #endregion

        #region Getting single value from SP
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, int timeout)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, null, timeout);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, null, parameters, timeout);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, columnName, null, defaultTimeout);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, int timeout)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, columnName, null, timeout);
        }
        public virtual Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters)
        {
            return GetValueFromSPAsync<T>(storedProcedureName, columnName, parameters, defaultTimeout);
        }
        public abstract Task<T> GetValueFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout);
        #endregion

        #region Getting list of values from SP
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, int timeout)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, null, timeout);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, null, parameters, timeout);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, columnName, null, defaultTimeout);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, int timeout)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, columnName, null, timeout);
        }
        public virtual Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters)
        {
            return GetValuesFromSPAsync<T>(storedProcedureName, columnName, parameters, defaultTimeout);
        }
        public abstract Task<List<T>> GetValuesFromSPAsync<T>(string storedProcedureName, string columnName, List<SqlParameter> parameters, int timeout);
        #endregion

        #region Getting single entity from SP
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, int timeout)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, null, timeout);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, null, parameters, timeout);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, prefix, null, defaultTimeout);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, int timeout)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, prefix, null, timeout);
        }
        public virtual Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters)
        {
            return GetEntityFromSPAsync<T>(storedProcedureName, prefix, parameters, defaultTimeout);
        }
        public abstract Task<T> GetEntityFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout);
        #endregion

        #region Getting list of entities from SP
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, null, defaultTimeout);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, int timeout)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, null, timeout);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, parameters, defaultTimeout);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, List<SqlParameter> parameters, int timeout)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, null, parameters, timeout);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, prefix, null, defaultTimeout);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, int timeout)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, prefix, null, timeout);
        }
        public virtual Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters)
        {
            return GetEntitiesFromSPAsync<T>(storedProcedureName, prefix, parameters, defaultTimeout);
        }
        public abstract Task<List<T>> GetEntitiesFromSPAsync<T>(string storedProcedureName, string prefix, List<SqlParameter> parameters, int timeout);

        public virtual Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> spParams, GRPropertyCollection properties)
        {
            return GetEntitiesFromSPAsync(storedProcedureName, spParams, properties, defaultTimeout);
        }
        public abstract Task<GRTable> GetEntitiesFromSPAsync(string storedProcedureName, List<SqlParameter> spParams, GRPropertyCollection properties, int timeout);
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
