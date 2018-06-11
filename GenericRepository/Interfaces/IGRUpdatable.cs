using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GenericRepository.Interfaces
{
    public interface IGRUpdatable
    {
        Type Type { get; }
        IGRRepository Repository { get; }
        GRExecutionStatistics ExecutionStats { get; set; }
        GRDBStructure Structure { get; }

    }
    public interface IGRUpdatable<T> : IGRUpdatable
    {
        T Entity { get; }

        IGRUpdatable<T> GRInclude(params Expression<Func<T, object>>[] properties);
        IGRUpdatable<T> GRExclude(params Expression<Func<T, object>>[] properties);
        IGRUpdatable<T> GRForceExclude(params Expression<Func<T, object>>[] properties);

        GRExecutionStatistics GRExecute();
        Task<GRExecutionStatistics> GRExecuteAsync();

        bool HasForceExcludedProperties { get; }
        bool HasIncludedProperties { get; }
        bool HasExcludedProperties { get; }
        List<GRCommandParamExclude<T>> ExcludedProperties { get; }
        List<GRCommandParamForceExclude<T>> ForceExcludedProperties { get; }
        List<GRCommandParamInclude<T>> IncludedProperties { get; }
        bool ContainsExcludedProperty(string propertyName);
        bool ContainsIncludedProperty(string propertyName);

    }
}
