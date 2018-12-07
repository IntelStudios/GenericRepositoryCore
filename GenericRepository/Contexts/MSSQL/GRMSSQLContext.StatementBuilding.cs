using GenericRepository.Enums;
using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        protected const string methodName = "CallBuildQueryStatement";
        protected static MethodInfo method = typeof(GRMSSQLContext).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

        protected virtual GRQueryStatement BuildQueryStatement(IGRQueriable queriable, bool forceIdentityColumn)
        {
            MethodInfo genericMethod = method.MakeGenericMethod(queriable.Type);
            object statement = genericMethod.Invoke(this, new object[] { queriable, false, forceIdentityColumn });
            GRQueryStatement ret = (GRQueryStatement)statement;
            return ret;
        }

        /// <summary>
        /// Do NOT remove this metod, it's called 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queriable"></param>
        /// <param name="isCountQuery"></param>
        /// <returns></returns>
        protected virtual GRQueryStatement CallBuildQueryStatement<T>(IGRQueriable<T> queriable, bool isCountQuery, bool forceIdentityColumn)
        {
            return BuildQueryStatement(queriable, isCountQuery, forceIdentityColumn);
        }

        protected virtual GRQueryStatement BuildQueryStatement<T>(IGRQueriable<T> queriable, bool isCountQuery, bool forceIdentityColumn)
        {
            GRQueryStatement queryCommand = new GRQueryStatement
            {
                IsCountQuery = isCountQuery,
                TableName = queriable.Structure.TableName,
                Prefix = queriable.Prefix,
                HasTemporaryPrefix = queriable.HasTemporaryPrefix,
                Type = typeof(T),
                IsDistinct = queriable.IsDistinct,
                HasNoLock = queriable.HasNoLock
            };

            StringBuilder queryCommandString = new StringBuilder();
            Dictionary<GRDBProperty, GRDBQueryProperty> selectColumns = null;

            try
            {
                // constructing SELECT <TOP> <COLUMNS> FROM <TABLE>
                if (isCountQuery)
                {
                    queryCommandString.AppendFormat("SELECT COUNT(1) FROM [{0}]", queriable.Structure.TableName);
                }
                else
                {
                    selectColumns = GetQueryColumns(GRDataTypeHelper.GetDBStructure(typeof(T)), queriable, forceIdentityColumn, queriable.HasExcludedAllProperties);

                    //string selectColumnList = string.Join(", ", selectColumns.Select(c => GRDataTypeHelper.GetDBPrefixedSelectColumnName(queriable.Prefix, c.Property.DBColumnName)));
                    string selectColumnList = string.Join(", ", selectColumns.Select(c => GRDataTypeHelper.GetDBPrefixedSelectColumnName(queriable.Prefix, c.Value.Property.DBColumnName)));

                    queryCommand.ColumnsString = selectColumnList;

                    if (queriable.HasLimit)
                    {
                        selectColumnList = string.Format("TOP {0} {1}", queriable.Limit, selectColumnList);
                    }

                    queryCommandString.AppendFormat("SELECT {2}{0} FROM [{1}]", selectColumnList, queriable.Structure.TableName, queryCommand.IsDistinct ? "DISTINCT " : string.Empty);

                    if (!string.IsNullOrEmpty(queriable.Prefix))
                    {
                        queryCommandString.AppendFormat(" AS {0}", queriable.Prefix);
                    }

                    if (queriable.HasNoLock)
                    {
                        queryCommandString.Append(" with (NOLOCK)");
                    }
                }
            }
            catch (Exception exc)
            {
                throw new GRQueryBuildFailedException(exc, "An error occured while constructing list of columns to query.");
            }

            try
            {
                // constructing WHERE clause
                if (queriable.WhereClauses != null && queriable.WhereClauses.Any())
                {
                    List<string> whereConditions = new List<string>();

                    foreach (var whereClause in queriable.WhereClauses)
                    {
                        string where = BuildQueryConditions(whereClause.Expression.Body, queryCommand, queriable.Prefix);
                        whereConditions.Add(where);
                    }

                    if (whereConditions.Count == 1)
                    {
                        queryCommandString.AppendFormat(" WHERE {0} ", whereConditions.First());
                    }
                    else if (whereConditions.Count > 0)
                    {
                        queryCommandString.Append(" WHERE ");
                        for (int i = 0; i < whereConditions.Count; i++)
                        {
                            if (i > 0)
                            {
                                queryCommandString.Append(" AND ");
                            }
                            queryCommandString.AppendFormat("({0})", whereConditions[i]);
                        }
                    }
                    queryCommand.WhereString = string.Join(" AND ", whereConditions);
                }
            }
            catch (Exception exc)
            {
                throw new GRQueryBuildFailedException(exc, "An error occured while constructing WHERE clause of query.");
            }

            if (queriable.Joined != null)
            {
                queryCommand.JoiningString = string.Format(" {0} JOIN [{1}] as {2}{5} on {3} = {4}",
                    queriable.Joined.TypeName,
                    queriable.Joined.Queriable.Structure.TableName,
                    queriable.Joined.Queriable.Prefix,
                    GRDataTypeHelper.GetDBPrefixedColumnName(queriable.Prefix, queriable.Joined.TargetPropertyName),
                    GRDataTypeHelper.GetDBPrefixedColumnName(queriable.Joined.Queriable.Prefix, queriable.Joined.SourcePropertyName),
                    queriable.Joined.Queriable.HasNoLock ? " with (NOLOCK)" : string.Empty
                );
            }

            try
            {
                // constructing ORDER BY clause
                if (!isCountQuery && queriable.OrderByProperties != null && queriable.OrderByProperties.Any())
                {
                    List<string> orderColumns = new List<string>();

                    foreach (var orderClause in queriable.OrderByProperties)
                    {
                        if (orderClause.Direction == GRQueryOrderDirection.Ascending)
                        {
                            orderColumns.Add(GRDataTypeHelper.GetDBPrefixedColumnName(queriable.Prefix, orderClause.Property.DBColumnName));

                        }
                        else
                        {
                            orderColumns.Add(GRDataTypeHelper.GetDBPrefixedColumnName(queriable.Prefix, orderClause.Property.DBColumnName) + " DESC");
                        }
                    }

                    queryCommandString.AppendFormat(" ORDER BY {0}", string.Join(", ", orderColumns));

                    queryCommand.OrderString = string.Join(", ", orderColumns);
                }
            }
            catch (Exception exc)
            {
                throw new GRQueryBuildFailedException(exc, "An error occured while constructing ORDER BY clause of query.");
            }

            queryCommand.IsExcluded = queriable.HasExcludedAllProperties;
            queryCommand.Statement = queryCommandString.ToString();
            queryCommand.Columns = selectColumns;

            return queryCommand;
        }

        protected virtual GRMergedQueryStatement BuildJoinQueryStatement(IGRQueriable queriable)
        {
            List<GRQueryStatement> statements = new List<GRQueryStatement>();

            IGRQueriable current = queriable;

            GRQueryStatement joiningStatement = BuildQueryStatement(queriable, forceIdentityColumn: false);

            statements.Add(joiningStatement);

            while (current.Joined != null)
            {
                GRQueryStatement joinedStatement = BuildQueryStatement(current.Joined.Queriable, forceIdentityColumn: true);

                statements.Add(joinedStatement);

                current = current.Joined.Queriable;
            }

            return MergeJoinQueryStatements(statements);
        }

        private GRMergedQueryStatement MergeJoinQueryStatements(List<GRQueryStatement> statements)
        {
            GRMergedQueryStatement ret = new GRMergedQueryStatement()
            {
                TableName = statements.First().TableName
            };

            StringBuilder commandString = new StringBuilder();

            List<string> columns = statements.Where(s => !string.IsNullOrEmpty(s.ColumnsString)).Select(s => s.ColumnsString).ToList();
            commandString.Append("SELECT ");

            if (statements.Where(s => s.IsDistinct).Any())
            {
                commandString.Append("DISTINCT ");
            }

            commandString.Append(string.Join(", ", columns));
            commandString.AppendFormat(" FROM [{0}] AS {1} ", statements.First().TableName, statements.First().Prefix);

            if (statements.First().HasNoLock)
            {
                commandString.Append("with (NOLOCK) ");
            }

            List<string> joins = statements.Where(s => !string.IsNullOrEmpty(s.JoiningString)).Select(s => s.JoiningString).ToList();
            commandString.Append(string.Join(" ", joins));

            List<string> wheres = new List<string>();

            ret.Statements = new List<GRQueryStatement>();

            foreach (var statement in statements)
            {
                ret.Statements.Add(statement);

                if (string.IsNullOrEmpty(statement.WhereString)) continue;

                string where = statement.WhereString;

                for (int i = 0; i < statement.Params.Count; i++)
                {
                    string oldName = string.Format("@{0}{1}{2}", SqlParamPreffix, i, SqlParamSuffix);
                    string newName = string.Format("@{0}{1}{2}", SqlParamPreffix, ret.Params.Count, SqlParamSuffix);
                    where = where.Replace(oldName, newName);

                    ret.Params.Add(statement.Params[i]);
                }

                wheres.Add(where);
            }

            if (wheres.Any())
            {
                commandString.Append(" WHERE ");
                if (wheres.Count == 1)
                {
                    commandString.Append(wheres.First());
                }
                else
                {
                    commandString.Append(string.Join(" AND ", wheres.Select(w => string.Format("({0})", w))));
                }
            }

            List<string> orders = new List<string>();

            foreach (var statement in statements)
            {
                if (string.IsNullOrEmpty(statement.OrderString)) continue;

                string order = statement.OrderString;

                for (int i = 0; i < statement.Params.Count; i++)
                {
                    string oldName = string.Format("@{0}{1}{2}", SqlParamPreffix, i, SqlParamSuffix);
                    string newName = string.Format("@{0}{1}{2}", SqlParamPreffix, ret.Params.Count, SqlParamSuffix);
                    order = order.Replace(oldName, newName);

                    ret.Params.Add(statement.Params[i]);
                }

                orders.Add(order);
            }

            if (orders.Any())
            {
                commandString.Append(" ORDER BY ");
                if (orders.Count == 1)
                {
                    commandString.Append(orders.First());
                }
                else
                {
                    commandString.Append(string.Join(", ", orders.Select(w => string.Format("{0}", w))));
                }
            }

            ret.Statement = commandString.ToString();

            return ret;
        }

        private string BuildQueryConditions(Expression exp, GRStatement queryCommand, string prefix)
        {
            #region AND
            if (exp.NodeType == ExpressionType.And || exp.NodeType == ExpressionType.AndAlso)
            {
                BinaryExpression binExp = exp as BinaryExpression;
                return string.Format("({0}) AND ({1})", BuildQueryConditions(binExp.Left, queryCommand, prefix), BuildQueryConditions(binExp.Right, queryCommand, prefix));
            }
            #endregion

            #region OR
            if (exp.NodeType == ExpressionType.Or || exp.NodeType == ExpressionType.OrElse)
            {
                BinaryExpression binExp = exp as BinaryExpression;
                return string.Format("({0}) OR ({1})", BuildQueryConditions(binExp.Left, queryCommand, prefix), BuildQueryConditions(binExp.Right, queryCommand, prefix));
            }
            #endregion

            #region NOT
            if (exp.NodeType == ExpressionType.Not)
            {
                UnaryExpression unExp = exp as UnaryExpression;

                if (unExp.Operand is MemberExpression && unExp.Operand.Type == typeof(bool))
                {
                    return string.Format("{0} = 0", (unExp.Operand as MemberExpression).Member.Name);
                }
                else
                {
                    return string.Format("NOT ({0})", BuildQueryConditions(unExp.Operand, queryCommand, prefix));
                }
            }
            #endregion

            #region Methods
            if (exp.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methodCall = exp as MethodCallExpression;

                if (methodCall.Method.Name == "IsNullOrEmpty")
                {
                    string propertyName = GRDataTypeHelper.GetValueString(methodCall.Arguments.First());
                    string ret = string.Format("{0} IS NULL OR {0} = ''", GRDataTypeHelper.GetDBPrefixedColumnName(prefix, propertyName));
                    return ret;
                }

                if (methodCall.Object.Type == typeof(String))
                {
                    string value = (string)GRDataTypeHelper.GetValue(methodCall.Arguments.First());
                    string propertyName = GRDataTypeHelper.GetValueString(methodCall.Object);

                    if (methodCall.Method.Name == "StartsWith")
                    {
                        value = value + "%";
                    }
                    if (methodCall.Method.Name == "EndsWith")
                    {
                        value = "%" + value;
                    }
                    if (methodCall.Method.Name == "Contains")
                    {
                        value = "%" + value + "%";
                    }

                    string param = "@" + SqlParamPreffix + queryCommand.Params.Count + SqlParamSuffix;

                    GRDBStructure structure = GRDataTypeHelper.GetDBStructure(methodCall.Object);
                    GRDBProperty prop = structure[propertyName];

                    queryCommand.Params.Add(new GRStatementParam()
                    {
                        Value = value,
                        Property = prop
                    });

                    string ret = string.Format("{0} LIKE {1}", GRDataTypeHelper.GetDBPrefixedColumnName(prefix, propertyName), param);
                    return ret;
                }

                #region Cointains
                if (typeof(IEnumerable).IsAssignableFrom(methodCall.Object.Type))
                {
                    if (methodCall.Method.Name == "Contains")
                    {
                        IEnumerable en = null;
                        List<string> values = new List<string>();

                        if (methodCall.Object is MemberExpression)
                        {
                            MemberExpression memExp = methodCall.Object as MemberExpression;
                            object enumerable = GRDataTypeHelper.GetValue(memExp);
                            en = enumerable as IEnumerable;
                        }
                        else
                        {
                            object enumerable = GRDataTypeHelper.GetValue(methodCall.Object);
                            en = enumerable as IEnumerable;
                        }


                        if (methodCall.Arguments.First() is MethodCallExpression)
                        {
                            MethodCallExpression callExp = methodCall.Arguments.First() as MethodCallExpression;
                        }

                        string dbColumnName = GRDataTypeHelper.GetValueString(methodCall.Arguments.First());

                        foreach (var value in en)
                        {
                            values.Add(GRDataTypeHelper.GetValueString(value));
                        }

                        // OPTIMIZETT: This query shoud not be send to a database
                        if (values.Count > 0)
                        {
                            string ret = string.Format("{0} IN ({1})", GRDataTypeHelper.GetDBPrefixedColumnName(prefix, dbColumnName), string.Join(", ", values));
                            return ret;
                        }
                        else
                        {
                            return "1 = 2";
                        }
                    }

                    throw new GRInvalidOperationException(string.Format("Method '{0}' called on collection is not supported yet.", methodCall.Method.Name));
                }
                #endregion
            }
            #endregion

            if (exp is ConstantExpression)
            {
                ConstantExpression constExp = exp as ConstantExpression;
                if (constExp.Value is bool && Convert.ToBoolean(constExp.Value)) return "1 = 1";
            }

            if (exp is BinaryExpression)
            {
                BinaryExpression binExp = exp as BinaryExpression;
                return BuildQueryBinaryConditions(binExp, exp.NodeType, queryCommand, prefix);
            }

            if (exp is MemberExpression && exp.Type == typeof(bool))
            {
                return string.Format("{0} = 1", GRDataTypeHelper.GetDBPrefixedColumnName(prefix, (exp as MemberExpression).Member.Name));
            }

            throw new GRUnsuportedOperatorException(exp.NodeType.ToString());
        }

        private string BuildQueryBinaryConditions(BinaryExpression binExp, ExpressionType operation, GRStatement commandStructure, string prefix)
        {
            object left = GRDataTypeHelper.GetValue(binExp.Left);
            object right = GRDataTypeHelper.GetValue(binExp.Right);

            if (left is PropertyInfo && right is PropertyInfo)
            {
                throw new GRUnsuportedOperatorException("Could not compare two properties.");
            }

            if (!(left is PropertyInfo) && !(right is PropertyInfo))
            {
                throw new GRUnsuportedOperatorException("Could not compare two properties.");
            }

            string cmd = null;

            if (left is PropertyInfo)
            {
                if (right == null)
                {
                    if (operation == ExpressionType.Equal)
                    {
                        cmd = string.Format("{0} IS NULL", GRDataTypeHelper.GetDBPrefixedColumnName(prefix, GRDataTypeHelper.GetValueString(binExp.Left)));
                    }
                    else if (operation == ExpressionType.NotEqual)
                    {
                        cmd = string.Format("{0} IS NOT NULL", GRDataTypeHelper.GetDBPrefixedColumnName(GRDataTypeHelper.GetValueString(binExp.Left), prefix));
                    }
                    else
                    {
                        throw new GRUnsuportedOperatorException("Could not compare NULL value.");
                    }
                }
                else
                {
                    cmd = string.Format("{0} {2} {1}",
                        GRDataTypeHelper.GetDBPrefixedColumnName(prefix, GRDataTypeHelper.GetValueString(binExp.Left)),
                        "@" + SqlParamPreffix + commandStructure.Params.Count + SqlParamSuffix,
                        GetQueryConditionSymbol(operation));

                    GRDBProperty prop = GRDataTypeHelper.GetDBProperty(left as PropertyInfo);
                    commandStructure.Params.Add(new GRStatementParam
                    {
                        Property = prop,
                        Value = right
                    });
                }
                return cmd;
            }
            else
            {
                if (left == null)
                {
                    if (operation == ExpressionType.Equal)
                    {
                        cmd = string.Format("{0} IS NULL", GRDataTypeHelper.GetDBPrefixedColumnName(prefix, GRDataTypeHelper.GetValueString(binExp.Right)));
                    }
                    else if (operation == ExpressionType.NotEqual)
                    {
                        cmd = string.Format("{0} IS NOT NULL", GRDataTypeHelper.GetDBPrefixedColumnName(prefix, GRDataTypeHelper.GetValueString(binExp.Right)));
                    }
                    else
                    {
                        throw new GRUnsuportedOperatorException("Could not compare NULL value.");
                    }
                }
                else
                {
                    cmd = string.Format("{1} {2} {0}",
                        GRDataTypeHelper.GetDBPrefixedColumnName(prefix, GRDataTypeHelper.GetValueString(binExp.Right)),
                        "@" + SqlParamPreffix + commandStructure.Params.Count + SqlParamSuffix,
                        GetQueryConditionSymbol(operation));

                    GRDBProperty prop = GRDataTypeHelper.GetDBProperty(right as PropertyInfo);
                    commandStructure.Params.Add(new GRStatementParam
                    {
                        Property = prop,
                        Value = left
                    });
                }
                return cmd;
            }
        }

        protected virtual string GetQueryConditionSymbol(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "!=";
                default:
                    throw new GRUnsuportedOperatorException(type.ToString());
            }
        }

        protected virtual Dictionary<GRDBProperty, GRDBQueryProperty> GetQueryColumns<T>(GRDBStructure structure, IGRQueriable<T> queriable, bool forceIdentityColumn, bool excluded)
        {
            if (queriable.HasIncludedProperties && queriable.HasExcludedProperties)
            {
                throw new GRQueryBuildFailedException("Query cannot contain both INCLUDE and EXCLUDE commands.");
            }

            Dictionary<GRDBProperty, GRDBQueryProperty> ret = new Dictionary<GRDBProperty, GRDBQueryProperty>();

            if (!excluded)
            {
                if (queriable.HasIncludedProperties)
                {
                    queriable.IncludedProperties.ForEach(p =>
                    {
                        ret.Add(p.Property, new GRDBQueryProperty(p.Property));
                    });
                }
                else
                {
                    for (int i = 0; i < structure.Properties.Count; i++)
                    {
                        if (queriable.ContainsExcludedProperty(structure.Properties[i].PropertyInfo.Name))
                        {
                            continue;
                        }

                        ret.Add(structure.Properties[i], new GRDBQueryProperty(structure.Properties[i]));
                    }
                }
            }

            if (forceIdentityColumn)
            {
                foreach (GRDBProperty key in structure.KeyProperties)
                {
                    if (!ret.ContainsKey(key))
                    {
                        ret.Add(key, new GRDBQueryProperty(key, true));
                    }
                }
            }

            return ret;
        }

        protected virtual void ReplaceQueryAttributes(GRStatement statement, SqlCommand command)
        {
            statement.ReadableStatement = statement.Statement;

            for (int i = 0; i < statement.Params.Count; i++)
            {
                string paramKey = "@" + SqlParamPreffix + i + SqlParamSuffix;
                object value = statement.Params[i].Value;

                if (value is Stream)
                {
                    statement.ReadableStatement = statement.ReadableStatement.Replace(paramKey, string.Format("<Binary stream of {0} B>", (value as Stream).Length));
                }
                if (value is byte[])
                {
                    statement.ReadableStatement = statement.ReadableStatement.Replace(paramKey, string.Format("<Byte array of {0} B>", (value as byte[]).Length));
                }
                else
                {
                    statement.ReadableStatement = statement.ReadableStatement.Replace(paramKey, GRDataTypeHelper.GetValueString(value));
                }

                if (value == null)
                {
                    value = DBNull.Value;
                }

                SqlParameter param = command.Parameters.AddWithValue(paramKey, value);

                if (statement.Params[i].Property.IsBinary)
                {
                    param.DbType = DbType.Binary;
                }
            }
        }
    }
}
