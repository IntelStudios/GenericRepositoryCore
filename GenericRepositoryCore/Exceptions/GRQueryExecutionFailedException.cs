using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Exceptions
{
    public class GRQueryExecutionFailedException : ApplicationException
    {
        public GRExecutionStatistics ExecutionStats { get; private set; }

        public GRQueryExecutionFailedException(string message) : base(message)
        {

        }
        public GRQueryExecutionFailedException(Exception innerExeption, GRExecutionStatistics executionStats, string message) : base(message, innerExeption)
        {
            this.ExecutionStats = executionStats;
        }

        public GRQueryExecutionFailedException(Exception innerExeption, string message) : base(message, innerExeption)
        {
        }

        public GRQueryExecutionFailedException(GRExecutionStatistics executionStats, string message) : base(message)
        {
            this.ExecutionStats = executionStats;
        }
    }
}
