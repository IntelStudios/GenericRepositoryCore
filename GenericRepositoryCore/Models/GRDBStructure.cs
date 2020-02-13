using GenericRepository.Attributes;
using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GenericRepository.Models
{
    public class GRDBStructure
    {
        Dictionary<string, GRDBProperty> propDictionary;
        List<GRDBProperty> propList;
        Dictionary<GRDBProperty, GRDBQueryProperty> retPropList;

        public string TableName { get; private set; }

        public Type Type { get; private set; }

        static readonly Type[] insertTypes = { typeof(GRBeforeInsertAttribute), typeof(GRBeforeSaveAttribute) };
        static readonly Type[] updateTypes = { typeof(GRBeforeUpdateAttribute), typeof(GRBeforeSaveAttribute) };

        public List<GRDBProperty> Properties
        {
            get
            {
                return propList;
            }
        }

        public Dictionary<GRDBProperty, GRDBQueryProperty> RetProperties
        {
            get
            {
                return retPropList;
            }
        }

        public GRDBProperty this[string propertyName]
        {
            get
            {
                if (!propDictionary.ContainsKey(propertyName))
                {
                    return null;
                }
                return propDictionary[propertyName];
            }
        }

        private GRDBStructure(Type type)
        {
            this.Type = type;
            this.TableName = GRDataTypeHelper.GetDBTableName(type);
            AnalyzeType();
        }

        public static GRDBStructure Create(Type type)
        {
            return new GRDBStructure(type);
        }

        List<GRDBProperty> keyProperties = null;
        List<GRDBProperty> nonKeyProperties = null;
        List<GRDBProperty> insertProperties = null;

        List<GRDBProperty> autoUpdateProperties = null;
        List<GRDBProperty> autoInsertProperties = null;
        List<GRDBProperty> autoProperties = null;

        public List<MethodInfo> BeforeUpdateMethods { private set; get; }
        public List<MethodInfo> BeforeInsertMethods { private set; get; }

        public List<GRDBProperty> KeyProperties
        {
            get
            {
                if (keyProperties == null)
                {
                    keyProperties = propDictionary.Values.Where(p => p.IsPrimaryKeyAutoIncremented || p.IsPrimaryKey).ToList();
                }
                return keyProperties;
            }
        }

        public List<GRDBProperty> NonKeyProperties
        {
            get
            {
                if (nonKeyProperties == null)
                {
                    nonKeyProperties = propDictionary.Values.Where(p => !(p.IsPrimaryKeyAutoIncremented || p.IsPrimaryKey)).ToList();
                }
                return nonKeyProperties;
            }
        }

        public List<GRDBProperty> InsertProperties
        {
            get
            {
                if (insertProperties == null)
                {
                    insertProperties = propDictionary.Values.Where(p => !p.IsPrimaryKeyAutoIncremented).ToList();
                }
                return insertProperties;
            }
        }

        public List<GRDBProperty> AutoUpdateProperties
        {
            get
            {
                if (autoUpdateProperties == null)
                {
                    autoUpdateProperties = propDictionary.Values.Where(p => p.IsAutoUpdateProperty).ToList();
                }
                return autoUpdateProperties;
            }
        }

        public List<GRDBProperty> AutoInsertProperties
        {
            get
            {
                if (autoInsertProperties == null)
                {
                    autoInsertProperties = propDictionary.Values.Where(p => p.IsAutoInsertProperty).ToList();
                }
                return autoInsertProperties;
            }
        }

        public List<GRDBProperty> AutoProperties
        {
            get
            {
                if (autoProperties == null)
                {
                    autoProperties = this.Type.GetProperties()
                        .Where(p => GRDataTypeHelper.IsAutoProperty(p))
                        .Select(p => new GRDBProperty(p))
                        .ToList();
                }
                return autoProperties;
            }
        }

        public GRDBProperty IdentityProperty { private set; get; }

        public bool HasIdentityProperty
        {
            get
            {
                return IdentityProperty != null;
            }
        }

        private void AnalyzeType()
        {
            AnalyzeProperties();
            AnalyzeMethods();
        }

        private void AnalyzeMethods()
        {
            BeforeInsertMethods = new List<MethodInfo>();
            BeforeUpdateMethods = new List<MethodInfo>();

            MethodInfo[] methods = this.Type.GetMethods();

            foreach (var method in methods)
            {
                bool hasInsertAttribute = GRDataTypeHelper.HasOneOfAttributes(method, insertTypes);
                bool hasUpdateAttribute = GRDataTypeHelper.HasOneOfAttributes(method, updateTypes);

                if (hasInsertAttribute)
                {
                    BeforeInsertMethods.Add(method);
                }

                if (hasUpdateAttribute)
                {
                    BeforeUpdateMethods.Add(method);
                }
            }
        }

        private void AnalyzeProperties()
        {
            PropertyInfo[] typeProperties = Type.GetProperties();

            propDictionary = new Dictionary<string, GRDBProperty>();

            foreach (PropertyInfo typeProperty in typeProperties)
            {
                // ignoring [GRIgnore] properties
                if (GRDataTypeHelper.HasAttribute(typeProperty, typeof(GRIgnoreAttribute)))
                {
                    continue;
                }

                // ignoring static properties
                MethodInfo getMethod = typeProperty.GetGetMethod();
                if (getMethod.IsStatic)
                {
                    continue;
                }

                GRDBProperty grProperty = new GRDBProperty(typeProperty);

                propDictionary.Add(grProperty.PropertyInfo.Name, grProperty);
            }

            IdentityProperty = propDictionary.Values.Where(p => p.IsPrimaryKeyAutoIncremented).FirstOrDefault();

            propList = propDictionary.Values.ToList();

            retPropList = new Dictionary<GRDBProperty, GRDBQueryProperty>();

            propList.ForEach(p =>
            {
                retPropList.Add(p, new GRDBQueryProperty(p, false));
            });
        }

        public override string ToString()
        {
            return $"GRDBStructure of {Type} (table {TableName})";
        }
    }
}
