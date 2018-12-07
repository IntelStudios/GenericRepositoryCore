using GenericRepository.Interfaces;
using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        private void LogSuccessfulQueryStats(GRExecutionStatistics executionStats)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Executed in {0} ms, returned {1} entities:", executionStats.ExecutionTime, executionStats.AffectedRows);
            if (executionStats != null && executionStats.ExecutionCommand != null)
            {
                sb.AppendLine();
                sb.Append(executionStats.ExecutionCommand);
            }
            LogDebug(sb.ToString());
        }

        private void LogSuccessfulUpdateStats(GRExecutionStatistics executionStats)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Executed in {0} ms, updated {1} entities:", executionStats.ExecutionTime, executionStats.AffectedRows);
            if (executionStats != null && executionStats.ExecutionCommand != null)
            {
                sb.AppendLine();
                sb.Append(executionStats.ExecutionCommand);
            }
            LogDebug(sb.ToString());
        }

        private void LogSuccessfulDeleteStats(GRExecutionStatistics executionStats)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Executed in {0} ms, deleted {1} entities:", executionStats.ExecutionTime, executionStats.AffectedRows);
            if (executionStats != null && executionStats.ExecutionCommand != null)
            {
                sb.AppendLine();
                sb.Append(executionStats.ExecutionCommand);
            }
            LogDebug(sb.ToString());
        }

        private void LogFailedQueryStats(GRExecutionStatistics executionStats, string errMessage, Exception exc)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(errMessage);
            if (executionStats != null && executionStats.ExecutionCommand != null)
            {
                sb.AppendLine();
                sb.Append(executionStats.ExecutionCommand);
            }
            LogError(exc, sb.ToString());
        }

        private void LogFailedUpdateStats(GRExecutionStatistics executionStats, string errMessage, Exception exc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(errMessage);
            if (executionStats != null && executionStats.ExecutionCommand != null)
            {
                sb.AppendLine();
                sb.Append(executionStats.ExecutionCommand);
            }
            LogError(exc, sb.ToString());
        }
    }
}
