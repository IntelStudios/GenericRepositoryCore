using GenericRepository.Exceptions;
using GenericRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        public override void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted)
        {
            if (sqlTransaction != null)
            {
                throw new GRInvalidOperationException("Could not begin new transaction, another transaction is in progress.");
            }

            semaphoreConnection.Wait();

            try
            {
                // make new shared connection
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                sqlTransaction = sqlConnection.BeginTransaction(isolationLevel);
            }
            finally
            {
                semaphoreConnection.Release();
            }
        }

        public override void CommitTransaction()
        {
            semaphoreConnection.Wait();

            try
            {
                sqlTransaction.Commit();                
            }
            finally
            {
                // dispose resources
                sqlConnection.Dispose();
                sqlTransaction = null;
                sqlConnection = null;

                semaphoreConnection.Release();
            }
        }

        public override void RollbackTransaction()
        {
            semaphoreConnection.Wait();

            try
            {
                sqlTransaction.Rollback();                
            }
            finally
            {
                // dispose resources
                sqlConnection.Dispose();
                sqlTransaction = null;
                sqlConnection = null;

                semaphoreConnection.Release();
            }
        }
    }
}
