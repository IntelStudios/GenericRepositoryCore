using System;
using System.Data.SqlClient;

namespace GenericRepositoryCore.Models
{
    public class GRSqlParameterAdapter
    {
        public SqlParameter SqlParameter { get; set; }
        public Type OutputType { get; set; }

        public GRSqlParameterAdapter(SqlParameter sqlParameter, Type outputType)
        {
            SqlParameter = sqlParameter;
            OutputType = outputType;
        }
    }
}
