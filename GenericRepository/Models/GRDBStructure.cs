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
        List<GRDBProperty> properties;

        public string TableName { get; private set; }

        public Type Type { get; private set; }

        static readonly Type[] insertTypes = { typeof(GRBeforeInsertAttribute), typeof(GRBeforeSaveAttribute) };
        static readonly Type[] updateTypes = { typeof(GRBeforeUpdateAttribute), typeof(GRBeforeSaveAttribute) };

        public List<GRDBProperty> Properties
        {
            get
            {
                return properties;
            }
        }

        public GRDBProperty this[string propertyName]
        {
            get
            {
                return properties.Where(p => p.PropertyInfo.Name == propertyName).Single();
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
                    keyProperties = properties.Where(p => p.IsPrimaryKeyAutoIncremented || p.IsPrimaryKey).ToList();
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
                    nonKeyProperties = properties.Where(p => !(p.IsPrimaryKeyAutoIncremented || p.IsPrimaryKey)).ToList();
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
                    insertProperties = properties.Where(p => !p.IsPrimaryKeyAutoIncremented).ToList();
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
                    autoUpdateProperties = properties.Where(p => GRDataTypeHelper.IsAutoUpdateProperty(p.PropertyInfo)).ToList();
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
                    autoInsertProperties = properties.Where(p => GRDataTypeHelper.IsAutoInsertProperty(p.PropertyInfo)).ToList();
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
                    autoProperties = Type.GetProperties()
                        .Where(p => GRDataTypeHelper.IsAutoProperty(p))
                        .Select(p => new GRDBProperty()
                        {
                            PropertyInfo = p
                        }).ToList();
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

            properties = new List<GRDBProperty>();

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

                GRDBProperty property = new GRDBProperty();
                property.DBColumnName = GRDataTypeHelper.GetDBColumnName(typeProperty);
                property.IsPrimaryKey = GRDataTypeHelper.HasAttribute(typeProperty, typeof(GRPrimaryKeyAttribute));
                property.IsPrimaryKeyAutoIncremented = GRDataTypeHelper.HasAttribute(typeProperty, typeof(GRAIPrimaryKey));
                property.PropertyInfo = typeProperty;

                properties.Add(property);
            }

            IdentityProperty = properties.Where(p => p.IsPrimaryKeyAutoIncremented).FirstOrDefault();
        }
    }
}
