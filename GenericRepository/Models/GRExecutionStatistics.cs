using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public class GRExecutionStatistics
    {
        public long? ExecutionTime { get; }
        public string ExecutionCommand { get; }
        public long? AffectedRows { get; }

        public GRExecutionStatistics(long? affectedRows, string executionCommand, long? executionTime)
        {
            this.ExecutionCommand = executionCommand;
            this.ExecutionTime = executionTime;
            this.AffectedRows = affectedRows;
        }

        public GRExecutionStatistics(GRExecutionStatistics statistics)
        {
            this.ExecutionCommand = statistics.ExecutionCommand;
            this.ExecutionTime = statistics.ExecutionTime;
            this.AffectedRows = statistics.AffectedRows;
        }
    }
}
