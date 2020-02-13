using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GenericRepository.Interfaces
{
    public interface IGRQueriable
    {
        string Prefix { get; set; }
        bool HasTemporaryPrefix { get; }
        GRJoinedQueriable Joined { get; set; }
        GRJoinedQueriable JoiningQueriable { get; set; }
        Type Type { get; }
        IGRContext Context { get; }
        GRDBStructure Structure { get; }
        GRExecutionStatistics ExecutionStats { get; set; }
        IGRRepository Repository { get; }
        bool HasNoLock { get; }
    }

    public interface IGRQueriable<T> : IGRQueriable, IEnumerable<T>
    {
        IGRQueriable<T> GRWhere(params Expression<Func<T, bool>>[] conditions);

        IGRQueriable<U> GRLeftJoin<U>(
            Expression<Func<T, object>> property1, 
            Expression<Func<U, object>> property2
            ) where U : new();

        IGRQueriable<U> GRRightJoin<U>(
            Expression<Func<T, object>> property1,
            Expression<Func<U, object>> property2
            ) where U : new();

        IGRQueriable<U> GRInnerJoin<U>(
            Expression<Func<T, object>> property1,
            Expression<Func<U, object>> property2
            ) where U : new();

        IGRQueriable<U> GRFullOuterJoin<U>(
            Expression<Func<T, object>> property1,
            Expression<Func<U, object>> property2
            ) where U : new();

        GRTable GRToTable();
        Task<GRTable> GRToTableAsync();

        bool IsDistinct { get; }
        bool HasLimit { get; }
        int? Limit { get; }
        
        List<GRCommandParamWhere<T>> WhereClauses { get; }
        List<GRCommandParamOrder<T>> OrderByProperties { get; }
        bool HasForceExcludedProperties { get; }
        bool HasIncludedProperties { get; }
        bool HasExcludedProperties { get; }
        List<GRCommandParamExclude<T>> ExcludedProperties { get; }
        List<GRCommandParamForceExclude<T>> ForceExcludedProperties { get; }
        List<GRCommandParamInclude<T>> IncludedProperties { get; }
        bool ContainsExcludedProperty(string propertyName);
        bool ContainsIncludedProperty(string propertyName);
        bool HasExcludedAllProperties { get; }

        IGRQueriable<T> GROrderBy(params Expression<Func<T, object>>[] properties);
        IGRQueriable<T> GROrderByDescending(params Expression<Func<T, object>>[] properties);
        IGRQueriable<T> GRTake(int count);
        IGRQueriable<T> GRInclude(params Expression<Func<T, object>>[] properties);
        IGRQueriable<T> GRExclude(params Expression<Func<T, object>>[] properties);
        IGRQueriable<T> GRExcludeAll();
        IGRQueriable<T> GRDistinct();

        IGRQueriable<T> GRNoLock();

        T GRFirstOrDefault();
        Task<T> GRFirstOrDefaultAsync();

        List<T> GRToList();
        Task<List<T>> GRToListAsync();

        int GRCount();
        Task<int> GRCountAsync();
    }
}
