using GenericRepository.Attributes;
using GenericRepository.Exceptions;
using GenericRepository.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Helpers
{
    public class GRDataTypeHelper
    {
        static Dictionary<Type, GRDBStructure> structureCache = new Dictionary<Type, GRDBStructure>();

        #region DB Names
        public static string GetDBColumnName(PropertyInfo property)
        {
            GRColumnNameAttribute attr = (GRColumnNameAttribute)property.GetCustomAttribute<GRColumnNameAttribute>();
            string columnName = attr == null ? property.Name : attr.ColumnName;
            return columnName;
        }
        public static string GetDBColumnName<T>(string propertyName)
        {
            PropertyInfo property = typeof(T).GetProperty(propertyName);
            return GetDBColumnName(property);
        }
        public static string GetDBColumnName(string propertyName, Type elementType)
        {
            PropertyInfo property = elementType.GetProperty(propertyName);
            return GetDBColumnName(property);
        }
        public static string GetDBTableName(Type type)
        {
            GRTableNameAttribute attr = (GRTableNameAttribute)Attribute.GetCustomAttribute(type, typeof(GRTableNameAttribute));
            string tableName = attr == null ? type.Name : attr.TableName;
            return tableName;
        }
        public static string GetDisplayName(Type type)
        {
            GRDisplayNameAttribute attr = (GRDisplayNameAttribute)Attribute.GetCustomAttribute(type, typeof(GRDisplayNameAttribute));
            string displayName = attr == null ? type.Name : attr.DisplayName;
            return displayName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDBPrefixedColumnName(string prefix, string propertyName)
        {
            return string.IsNullOrEmpty(prefix) ? string.Format("[{0}]", propertyName) : string.Format("{0}.[{1}]", prefix, propertyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDBPrefixedResultingColumnName(string prefix, string propertyName)
        {
            return string.IsNullOrEmpty(prefix) ? propertyName : string.Format("{0}_{1}", prefix, propertyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDBPrefixedSelectColumnName(string prefix, string propertyName)
        {
            return string.IsNullOrEmpty(prefix) ? string.Format("[{0}]", propertyName) : string.Format("{0}.[{1}] AS {0}_{1}", prefix, propertyName);
        }
        #endregion

        #region Entitity names
        public static string GetEntityDisplayName(Type type)
        {
            string entityName = GRDataTypeHelper.GetDisplayName(type);
            return entityName;
        }

        public static string GetEntitiesDisplayName(Type type)
        {
            string entityName = GRDataTypeHelper.GetDisplayName(type);
            string plural = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(CultureInfo.GetCultureInfo("en")).Pluralize(entityName);
            return entityName;
        }
        #endregion

        #region Attributes
        public static bool HasAttribute(PropertyInfo property, Type attributeType)
        {
            return Attribute.GetCustomAttribute(property, attributeType) != null;
        }
        public static bool HasOneOfAttributes(MethodInfo method, Type[] attributeTypes)
        {
            if (attributeTypes == null)
            {
                return false;
            }

            foreach (var attribute in attributeTypes)
            {
                if (HasAttribute(method, attribute))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool HasAttribute(MethodInfo method, Type attributeType)
        {
            return Attribute.GetCustomAttribute(method, attributeType) != null;
        }
        #endregion

        #region Auto properties
        public static bool IsAutoProperty(PropertyInfo property)
        {
            GRAutoValueAttribute autoAttr = property.GetCustomAttribute<GRAutoValueAttribute>();
            return autoAttr != null;
        }
        public static bool IsAutoInsertProperty(PropertyInfo property)
        {
            GRAutoValueAttribute autoAttr = Attribute.GetCustomAttribute(property, typeof(GRAutoValueAttribute)) as GRAutoValueAttribute;

            if (autoAttr == null) return false;

            return autoAttr.Apply.HasFlag(GRAutoValueApply.BeforeInsert);
        }
        public static bool IsAutoUpdateProperty(PropertyInfo property)
        {
            GRAutoValueAttribute autoAttr = Attribute.GetCustomAttribute(property, typeof(GRAutoValueAttribute)) as GRAutoValueAttribute;

            if (autoAttr == null) return false;

            return autoAttr.Apply.HasFlag(GRAutoValueApply.BeforeUpdate);
        }

        public static void ApplyAutoProperties(object entity, GRAutoValueApply apply)
        {
            ApplyAutoProperties(entity, apply, null);
        }

        public static void ApplyAutoProperties(object entity, GRAutoValueApply apply, object autoPropertiesSource)
        {
            GRDBStructure structure = GRDataTypeHelper.GetDBStructure(entity);

            foreach (var autoSelectProperty in structure.AutoProperties)
            {
                GRAutoValueAttribute autoAttr = autoSelectProperty.PropertyInfo.GetCustomAttribute<GRAutoValueAttribute>();
                if (!autoAttr.Apply.HasFlag(apply)) continue;

                #region GRAttributeAttribute
                if (autoAttr is GRAttributeAttribute)
                {
                    GRAttributeAttribute atAttr = autoAttr as GRAttributeAttribute;
                    Attribute entityAttribute = entity.GetType().GetCustomAttribute(atAttr.Attribute);
                    PropertyInfo attributeProperty = entityAttribute.GetType().GetProperty(atAttr.PropertyName);
                    object value = attributeProperty.GetValue(entityAttribute);
                    autoSelectProperty.PropertyInfo.SetValue(entity, value);
                }
                #endregion

                #region GRIDPropertyAttribute
                if (autoAttr is GRIDPropertyAttribute)
                {
                    GRIDPropertyAttribute idAttr = autoAttr as GRIDPropertyAttribute;
                    PropertyInfo thisProperty = autoSelectProperty.PropertyInfo;
                    PropertyInfo idProperty;

                    if (structure.HasIdentityProperty)
                    {
                        idProperty = structure.IdentityProperty.PropertyInfo;
                    }
                    else
                    {
                        var idProps = structure.Type
                            .GetProperties()
                            .Where(p => p.Name != autoSelectProperty.PropertyInfo.Name && (p.Name.EndsWith("Id") || p.Name.EndsWith("ID")));

                        if (!idProps.Any())
                        {
                            throw new GRAttributeApplicationFailedException(autoAttr, "No ID columns were found.");
                        }

                        if (idProps.Count() > 1)
                        {
                            throw new GRAttributeApplicationFailedException(autoAttr, "Too many ID columns were found.");
                        }

                        idProperty = idProps.First();
                    }

                    if (idAttr.Direction == GRAutoValueDirection.In)
                    {
                        thisProperty.SetValue(entity, idProperty.GetValue(entity));
                    }
                    else
                    {
                        idProperty.SetValue(entity, thisProperty.GetValue(entity));
                    }
                }
                #endregion

                #region GRPropertyAttribute
                if (autoAttr is GRPropertyAttribute)
                {
                    GRPropertyAttribute propAttr = autoAttr as GRPropertyAttribute;

                    PropertyInfo thisProperty = autoSelectProperty.PropertyInfo;
                    PropertyInfo otherProperty = entity.GetType().GetProperty(propAttr.PropertyName);

                    if (propAttr.Direction == GRAutoValueDirection.In)
                    {
                        thisProperty.SetValue(entity, otherProperty.GetValue(entity));
                    }
                    else
                    {
                        otherProperty.SetValue(entity, thisProperty.GetValue(entity));
                    }
                }
                #endregion

                #region GRRepositoryPropertyAttribute
                if (autoAttr is GRRepositoryPropertyAttribute)
                {
                    if (autoPropertiesSource == null)
                    {
                        continue;
                    };

                    GRRepositoryPropertyAttribute propAttr = autoAttr as GRRepositoryPropertyAttribute;

                    PropertyInfo thisProperty = autoSelectProperty.PropertyInfo;
                    PropertyInfo repositoryProperty = autoPropertiesSource.GetType().GetProperty(propAttr.PropertyName);

                    if (propAttr.Direction == GRAutoValueDirection.In)
                    {
                        object value = repositoryProperty.GetValue(autoPropertiesSource);
                        thisProperty.SetValue(entity, value);
                    }
                    else
                    {
                        object value = thisProperty.GetValue(entity);
                        repositoryProperty.SetValue(autoPropertiesSource, value);
                    }
                }
                #endregion

                #region GRCurrentDatetimeAttribute
                if (autoAttr is GRCurrentDatetimeAttribute)
                {
                    GRCurrentDatetimeAttribute datetimeAttr = autoAttr as GRCurrentDatetimeAttribute;
                    autoSelectProperty.PropertyInfo.SetValue(entity, DateTime.Now);
                }
                #endregion

                #region GRNewGuidAttribute
                if (autoAttr is GRNewGuidAttribute)
                {
                    GRNewGuidAttribute guidAttr = autoAttr as GRNewGuidAttribute;
                    autoSelectProperty.PropertyInfo.SetValue(entity, Guid.NewGuid());
                }
                #endregion
            }
        }
        #endregion

        #region Structure & properties
        public static GRDBStructure GetDBStructure(object obj)
        {
            return GetDBStructure(obj.GetType());
        }
        public static GRDBStructure GetDBStructure<T>()
        {
            return GetDBStructure(typeof(T));
        }
        public static GRDBStructure GetDBStructure(Type type)
        {
            lock (structureCache)
            {
                if (!structureCache.ContainsKey(type))
                {
                    GRDBStructure structure = GRDBStructure.Create(type);
                    structureCache.Add(type, structure);
                }

                return structureCache[type];
            }
        }
        public static GRDBProperty GetDBProperty(Type type, string propertyName)
        {
            GRDBStructure structure = GetDBStructure(type);
            GRDBProperty property = structure.Properties.Where(p => p.PropertyInfo.Name == propertyName).FirstOrDefault();
            return property;
        }
        public static GRDBProperty GetDBProperty(PropertyInfo prop)
        {
            GRDBStructure structure = GetDBStructure(prop.DeclaringType);
            GRDBProperty property = structure.Properties.Where(p => p.PropertyInfo.Name == prop.Name).FirstOrDefault();
            return property;
        }
        internal static GRDBProperty GetDBProperty<T>(Expression<Func<T, object>> propExp)
        {
            string propertyName = GetPropertyName(propExp);
            GRDBStructure structure = GetDBStructure(typeof(T));
            return structure[propertyName];
        }
        public static string GetPropertyName<T>(Expression<Func<T, object>> exp)
        {
            if (exp.Body is MemberExpression)
            {
                MemberExpression memExp = exp.Body as MemberExpression;
                return memExp.Member.Name;
            }

            MemberExpression expression = (MemberExpression)((UnaryExpression)exp.Body).Operand;
            string memberName = expression.Member.Name;
            return memberName;
        }
        public static bool IsNullableProperty(GRDBProperty prop)
        {
            Type underType = Nullable.GetUnderlyingType(prop.PropertyInfo.PropertyType);
            return underType != null;
        }
        #endregion

        #region Getting object values
        public static object GetValue(MemberExpression member)
        {
            UnaryExpression objectMember = Expression.Convert(member, typeof(object));
            Expression<Func<object>> getterLambda = Expression.Lambda<Func<object>>(objectMember);
            Func<object> getter = getterLambda.Compile();
            object result = getter();
            return result;
        }
        public static string GetValueString(Expression exp)
        {
            object value = GetValue(exp);
            return GetValueString(value);
        }
        public static object GetValue(Expression exp)
        {
            if (exp is System.Linq.Expressions.ConstantExpression)
            {
                ConstantExpression constExmp = exp as ConstantExpression;
                return constExmp.Value;
            }

            if (exp is System.Linq.Expressions.NewExpression)
            {
                NewExpression newExp = exp as NewExpression;
                object[] args = newExp.Arguments.Select(e => GetValue(e)).ToArray();
                object obj = newExp.Constructor.Invoke(args);
                return obj;
            }

            if (exp is System.Linq.Expressions.MemberExpression)
            {
                MemberExpression memExp = exp as MemberExpression;

                if (memExp.Expression != null && memExp.Expression.NodeType == ExpressionType.Parameter)
                {
                    return memExp.Member;
                }

                object value = GetValue(memExp);
                return value;
            }

            if (exp is System.Linq.Expressions.UnaryExpression)
            {
                UnaryExpression unExp = exp as UnaryExpression;
                return GetValue(unExp.Operand);
            }

            throw new GRUnsuportedExpressionException(exp);
        }
        public static string GetValueString(PropertyInfo propertyInfo, object obj)
        {
            object value = propertyInfo.GetValue(obj);

            return GetValueString(obj);
        }

        /// <summary>
        /// Returns invariant string representation of <em>value</em>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetValueString(object value)
        {
            if (value == null) return "NULL";

            if (value.GetType() == typeof(DateTime)) return string.Format("'{0}'", ((DateTime)value).ToString(CultureInfo.InvariantCulture));
            if (value.GetType() == typeof(int)) return value.ToString();
            if (value.GetType() == typeof(long)) return value.ToString();
            if (value.GetType() == typeof(double)) return ((double)value).ToString(CultureInfo.InvariantCulture);
            if (value.GetType() == typeof(decimal)) return ((decimal)value).ToString(CultureInfo.InvariantCulture);
            if (value.GetType() == typeof(string)) return string.Format("'{0}'", value.ToString());
            if (value.GetType() == typeof(bool)) return (((bool)value) ? 1 : 0).ToString();
            if (value.GetType() == typeof(Guid)) return string.Format("'{0}'", value.ToString());
            if (value.GetType() == typeof(JObject)) return string.Format("'{0}'", value.ToString());
            if (value.GetType() == typeof(TimeSpan)) return string.Format("'{0}'", ((TimeSpan)value).ToString());

            if (value is Enum) return ((int)value).ToString();

            if (value is PropertyInfo)
            {
                MemberInfo member = value as MemberInfo;
                string columnName = GetDBColumnName(member.Name, member.DeclaringType);
                return columnName;
            }

            if (value is Stream)
            {
                return string.Format("<Binary stream of {0} B>", (value as Stream).Length);
            }
            
            if (value is byte[])
            {
                return string.Format("<Byte array of {0} B>", (value as byte[]).Length);
            }

            throw new GRUnsuportedDataTypeException(value.GetType());
        }
        public static object GetAutoValue(PropertyInfo modelProperty, object sourceObject)
        {
            GRAutoValueAttribute dbUpdateAttribute = Attribute.GetCustomAttribute(modelProperty, typeof(GRAutoValueAttribute)) as GRAutoValueAttribute;

            if (dbUpdateAttribute is GRRepositoryPropertyAttribute)
            {
                GRRepositoryPropertyAttribute repositoryAttribute = dbUpdateAttribute as GRRepositoryPropertyAttribute;
                PropertyInfo sourceProperty = sourceObject.GetType().GetProperty(repositoryAttribute.PropertyName);
                object sourceValue = sourceProperty.GetValue(sourceObject);
                return sourceValue;
            }

            if (dbUpdateAttribute is GRCurrentDatetimeAttribute)
            {
                return DateTime.Now;
            }

            throw new GRUnsuportedAttributeException(dbUpdateAttribute);
        }
        public static string GetValueAssigningString(GRDBProperty property, object obj)
        {
            return string.Format("{0} = {1}", property.DBColumnName, GetValueString(property.PropertyInfo, obj));
        }
        #endregion

        #region Getting object type
        public static bool IsPrimitiveType<T>()
        {
            if (typeof(T).IsPrimitive ||
                typeof(T) == typeof(String) ||
                typeof(T) == typeof(DateTime) ||
                typeof(T) == typeof(DateTime?) ||
                typeof(T) == typeof(int?) ||
                typeof(T) == typeof(long?) ||
                typeof(T) == typeof(Guid) ||
                typeof(T) == typeof(Guid?))
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}
