using System;
using System.Collections.Generic;

namespace GenericRepository.Models
{
    public class GRStatementParam
    {
        public GRDBProperty Property { get; set; }
        public object Value { get; set; }
    }
    public abstract class GRStatement
    {
        public Dictionary<GRDBProperty, GRDBQueryProperty> Columns { set; get; }
        public List<GRStatementParam> Params { get; set; }
        public string Statement { get; set; }
        public string ReadableStatement { get; set; }       
        public string ColumnsString { set; get; }
        public Type Type { get; set; }

        public GRStatement()
        {
            Params = new List<GRStatementParam>();
            Columns = new Dictionary<GRDBProperty, GRDBQueryProperty>();
        }
    }
    public class GRQueryStatement : GRStatement
    {
        public string TableName { get; set; }
        public string Prefix { get; set; }
        public bool IsCountQuery { get; set; }
        public string WhereString { set; get; }
        public string JoiningString { set; get; }
        public bool IsDistinct { get; internal set; }
        public string OrderString { get; internal set; }
        public bool HasNoLock { get; internal set; }
        public bool HasTemporaryPrefix { get; internal set; }
        public bool IsExcluded { get; internal set; }
    }
    public class GRUpdateStatement : GRStatement
    {

    }

    public class GRDeleteStatement : GRStatement
    {
    }

    public class GRMergedQueryStatement : GRQueryStatement
    {
        public List<GRQueryStatement> Statements { get; set; }
    }
}
