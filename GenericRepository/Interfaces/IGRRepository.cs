using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GenericRepository.Interfaces
{
    public interface IGRRepository
    {
        int GRCount();
        Task<int> GRCountAsync();

        void PrepareForSave();
        Task PrepareForSaveAsync();
    }

    public interface IGRRepository<T> : IGRRepository
    {
        T GRGet(object key);
        Task<T> GRGetAsync(object key);

        IGRUpdatable<T> GRUpdate(T entity);

        IGRUpdatable<T> GRInsert(T entity);

        IGRDeletable<T> GRDelete(T entity);
        IGRDeletable<T> GRDelete();
        IGRDeletable<R> GRDelete<R>();

        IGRQueriable<T> GRWhere(params Expression<Func<T, bool>>[] conditions);
        IGRQueriable<T> GROrderBy(params Expression<Func<T, object>>[] properties);
        IGRQueriable<T> GROrderByDescending(params Expression<Func<T, object>>[] properties);
        IGRQueriable<T> GRTake(int count);
        IGRQueriable<T> GRInclude(params Expression<Func<T, object>>[] properties);
        IGRQueriable<T> GRExclude(params Expression<Func<T, object>>[] properties);

        IGRQueriable<U> GRLeftJoin<U>(Expression<Func<T, object>> property1, Expression<Func<U, object>> property2) where U : new();

        IGRQueriable<T> GRAll();

        List<T> GRToList();
        Task<List<T>> GRToListAsync();
    }
}
