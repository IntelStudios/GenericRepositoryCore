using GenericRepository.Interfaces;
using GenericRepository.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        public GRExecutionStatistics ParseQueryStatistics(SqlConnection connection, GRQueryStatement queryCommand)
        {
            return ParseQueryStatistics(connection, queryCommand.ReadableStatement);
        }

        public GRExecutionStatistics ParseQueryStatistics(SqlConnection connection, string statement)
        {
            IDictionary dbStats = connection.RetrieveStatistics();
            connection.ResetStatistics();

            long executionTime = (long)dbStats[SqlKeyExecutionTime];
            long numberOfRows = (long)dbStats[SqlKeyNumberOfRows];

            return new GRExecutionStatistics(numberOfRows, statement, executionTime);
        }

        public GRExecutionStatistics ParseUpdateStatistics(SqlConnection connection, GRUpdateStatement statement)
        {
            IDictionary dbStats = connection.RetrieveStatistics();
            connection.ResetStatistics();

            long executionTime = (long)dbStats[SqlKeyExecutionTime];
            long affectedRows = (long)dbStats[SqlKeyAffectedRows];

            return new GRExecutionStatistics(affectedRows, statement.ReadableStatement, executionTime);
        }

        public GRExecutionStatistics ParseInsertStatistics(SqlConnection connection, GRUpdateStatement statement)
        {
            IDictionary dbStats = connection.RetrieveStatistics();
            connection.ResetStatistics();

            long executionTime = (long)dbStats[SqlKeyExecutionTime];
            long affectedRows = (long)dbStats[SqlKeyAffectedRows];

            return new GRExecutionStatistics(affectedRows, statement.ReadableStatement, executionTime);
        }

        public GRExecutionStatistics ParseDeleteStatistics(SqlConnection connection, GRDeleteStatement statement)
        {
            IDictionary dbStats = connection.RetrieveStatistics();
            connection.ResetStatistics();

            long executionTime = (long)dbStats[SqlKeyExecutionTime];
            long affectedRows = (long)dbStats[SqlKeyAffectedRows];

            return new GRExecutionStatistics(affectedRows, statement.ReadableStatement, executionTime);
        }
        public GRExecutionStatistics ParseFnSpStatistics(SqlConnection connection, string statement)
        {
            IDictionary dbStats = connection.RetrieveStatistics();
            connection.ResetStatistics();

            long executionTime = (long)dbStats[SqlKeyExecutionTime];
            long numberOfRows = (long)dbStats[SqlKeyNumberOfRows];

            return new GRExecutionStatistics(numberOfRows, statement, executionTime);
        }
    }
}
