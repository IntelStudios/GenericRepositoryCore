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
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Helpers
{
    public class GRDataTypeHelper
    {
        static Dictionary<Type, GRDBStructure> structureCache = new Dictionary<Type, GRDBStructure>();

        #region Analysing DB structure
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

        public static bool HasAttribute(PropertyInfo property, Type attributeType)
        {
            return Attribute.GetCustomAttribute(property, attributeType) != null;
        }

        public static bool IsAutoInsertProperty(PropertyInfo property)
        {
            GRAutoValueAttribute autoAttr = Attribute.GetCustomAttribute(property, typeof(GRAutoValueAttribute)) as GRAutoValueAttribute;

            if (autoAttr == null) return false;

            return autoAttr.Apply.HasFlag(GRAutoValueApply.BeforeInsert);
        }

        public static bool IsAutoProperty(PropertyInfo property)
        {
            GRAutoValueAttribute autoAttr = property.GetCustomAttribute<GRAutoValueAttribute>();
            return autoAttr != null;
        }

        public static bool IsAutoUpdateProperty(PropertyInfo property)
        {
            GRAutoValueAttribute autoAttr = Attribute.GetCustomAttribute(property, typeof(GRAutoValueAttribute)) as GRAutoValueAttribute;

            if (autoAttr == null) return false;

            return autoAttr.Apply.HasFlag(GRAutoValueApply.BeforeUpdate);
        }

        public static GRDBStructure GetDBStructure(object obj)
        {
            return GetDBStructure(obj.GetType());
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
            GRDBProperty property = structure.Properties.Where(p=>p.PropertyInfo.Name == prop.Name).FirstOrDefault();
            return property;
        }

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

        #region Getting object values
        public static object GetMemberValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            object result = getter();
            return result;
        }
        public static string GetExpressionValueString(Expression exp)
        {
            object value = GetExpressionValue(exp);
            return GetObjectValueString(value);
        }
        public static object GetExpressionValue(Expression exp)
        {
            if (exp is System.Linq.Expressions.ConstantExpression)
            {
                ConstantExpression constExmp = exp as ConstantExpression;
                return constExmp.Value;
            }

            if (exp is System.Linq.Expressions.NewExpression)
            {
                NewExpression newExp = exp as NewExpression;
                object[] args = newExp.Arguments.Select(e => GetExpressionValue(e)).ToArray();
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

                object value = GetMemberValue(memExp);
                return value;
            }

            if (exp is System.Linq.Expressions.UnaryExpression)
            {
                UnaryExpression unExp = exp as UnaryExpression;
                return GetExpressionValue(unExp.Operand);
            }

            throw new GRUnsuportedExpressionException(exp);
        }
        public static string GetExpressionPropertyName<T>(Expression<Func<T, object>> exp)
        {
            if (exp.Body is MemberExpression)
            {
                MemberExpression memExp = exp.Body as MemberExpression;
                return memExp.Member.Name;
            }

            var expression = (MemberExpression)((UnaryExpression)exp.Body).Operand;
            string memberName = expression.Member.Name;
            return memberName;
        }
        public static string GetObjectValueString(PropertyInfo propertyInfo, object obj)
        {
            object value = propertyInfo.GetValue(obj);

            return GetObjectValueString(obj);
        }
        public static string GetObjectValueString(object obj)
        {
            if (obj == null) return "NULL";

            if (obj.GetType() == typeof(DateTime)) return string.Format("'{0}'", ((DateTime)obj).ToString(CultureInfo.InvariantCulture));
            if (obj.GetType() == typeof(int)) return obj.ToString();
            if (obj.GetType() == typeof(long)) return obj.ToString();
            if (obj.GetType() == typeof(double)) return ((double)obj).ToString(CultureInfo.InvariantCulture);
            if (obj.GetType() == typeof(decimal)) return ((decimal)obj).ToString(CultureInfo.InvariantCulture);
            if (obj.GetType() == typeof(string)) return string.Format("'{0}'", obj.ToString());
            if (obj.GetType() == typeof(bool)) return (((bool)obj) ? 1 : 0).ToString();
            if (obj.GetType() == typeof(Guid)) return string.Format("'{0}'", obj.ToString());
            if (obj.GetType() == typeof(JObject)) return string.Format("'{0}'", obj.ToString());
            if (obj.GetType() == typeof(TimeSpan)) return string.Format("'{0}'", ((TimeSpan)obj).ToString());

            if (obj is Enum) return ((int)obj).ToString();

            if (obj is PropertyInfo)
            {
                MemberInfo member = obj as MemberInfo;
                string columnName = GetDBColumnName(member.Name, member.DeclaringType);
                return columnName;
            }

            if (obj is Stream)
            {
                return string.Format("<Binary stream of {0} B>", (obj as Stream).Length);
            }
            
            if (obj is byte[])
            {
                return string.Format("<Byte array of {0} B>", (obj as byte[]).Length);
            }

            throw new GRUnsuportedDataTypeException(obj.GetType());
        }
        public static object GetPropertyAutoValue(PropertyInfo modelProperty, object sourceObject)
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
            return string.Format("{0} = {1}", property.DBColumnName, GetObjectValueString(property.PropertyInfo, obj));
        }
        #endregion
    }
}
