using GenericRepository.Attributes;
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        private T ParseValue<T>(SqlDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                T ret = (T)Convert.ChangeType(reader[0], typeof(T));
                return ret;
            }
            else
            {
                T ret = (T)Convert.ChangeType(reader[columnName], typeof(T));
                return ret;
            }
        }

        private List<T> ParseListOfValues<T>(SqlDataReader reader, string columnName)
        {
            List<T> ret = new List<T>();

            while (reader.Read())
            {
                ret.Add(ParseValue<T>(reader, columnName));
            }

            return ret;
        }

        private T ParseEntity<T>(SqlDataReader reader, string prefix, Dictionary<GRDBProperty, GRDBQueryProperty> properties, bool applyAutoProperties)
        {
            return (T)ParseEntity(typeof(T), reader, prefix, properties, applyAutoProperties);
        }

        private object ParseEntity(Type type, SqlDataReader reader, string prefix, Dictionary<GRDBProperty, GRDBQueryProperty> properties, bool applyAutoProperties)
        {
            if (properties == null)
            {
                properties = GRDataTypeHelper.GetDBStructure(type).RetProperties;
            }

            // if nothing is to parse, returning
            if (!properties.Any())
            {
                return null;
            }

            // if all values are null, also entity is null
            if (IsNullRow(prefix, reader, properties))
            {
                return null;
            }

            object entity = GRInstantiator.CreateInstance(type);

            foreach (KeyValuePair<GRDBProperty, GRDBQueryProperty> pair in properties)
            {
                GRDBProperty property = pair.Key;
                GRDBQueryProperty retProperty = pair.Value;

                string columnName = GRDataTypeHelper.GetDBPrefixedResultingColumnName(prefix, property.DBColumnName);

                if (retProperty.IsTemporary)
                {
                    continue;
                }

                object value = null;

                try
                {
                    value = reader[columnName];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new GRUnknownColumnException(property.PropertyInfo.Name, columnName);
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

                property.PropertyInfo.SetValue(entity, value);
            }

            if (entity != null && applyAutoProperties)
            {
                GRDataTypeHelper.ApplyAutoProperties(entity, GRAutoValueApply.AfterSelect);
            }

            return entity;
        }

        private bool IsNullRow(string prefix, SqlDataReader reader, Dictionary<GRDBProperty, GRDBQueryProperty> properties)
        {
            foreach (KeyValuePair<GRDBProperty, GRDBQueryProperty> pair in properties)
            {
                string columnName = GRDataTypeHelper.GetDBPrefixedResultingColumnName(prefix, pair.Key.DBColumnName);

                try
                {
                    if (!reader[columnName].Equals(DBNull.Value))
                    {
                        return false;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw new GRUnknownColumnException(pair.Key.PropertyInfo.Name, columnName);
                }
            }

            return true;
        }

        private List<T> ParseListOfEntities<T>(SqlDataReader reader, string prefix, Dictionary<GRDBProperty, GRDBQueryProperty> properties, bool applyAutoProperties)
        {
            List<T> ret = new List<T>();

            while (reader.Read())
            {
                T t = ParseEntity<T>(reader, prefix, properties, applyAutoProperties);
                ret.Add(t);
            }

            return ret;
        }
    }
}
