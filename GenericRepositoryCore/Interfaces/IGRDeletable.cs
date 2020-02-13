using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GenericRepository.Interfaces
{
    public interface IGRDeletable<T> : IGRUpdatable
    {
        T Entity { get; }
        IGRDeletable<T> GRWhere(params Expression<Func<T, bool>>[] conditions);
        List<GRCommandParamWhere<T>> WhereClauses { get; }
        GRExecutionStatistics GRExecute();
        Task<GRExecutionStatistics> GRExecuteAsync();
    }
}
