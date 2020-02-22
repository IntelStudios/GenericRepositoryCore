using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GenericRepository.Interfaces
{
    public interface IGRRepository
    {
        R GRGet<R>(object key);
        Task<R> GRGetAsync<R>(object key);

        IGRQueriable<R> GRWhere<R>(params Expression<Func<R, bool>>[] conditions);
        public IGRQueriable<R> GRAll<R>();

        #region Insert/Update/Delete methods
        IGRUpdatable<R> GREnqueueUpdate<R>(R entity);
        IGRUpdatable<R> GREnqueueInsert<R>(R entity);
        IGRDeletable<R> GREnqueueDelete<R>(R entity);
        IGRDeletable<R> GREnqueueDelete<R>();
        #endregion

        #region Save events
        void PrepareForSave();
        Task PrepareForSaveAsync();
        #endregion

        #region Count sync/async methods
        int GRCount<R>();
        Task<int> GRCountAsync<R>();
        #endregion
    }

    public interface IGRRepository<T> : IGRRepository
    {
        int GRCount();
        Task<int> GRCountAsync();

        T GRGet(object key);
        Task<T> GRGetAsync(object key);

        IGRUpdatable<T> GREnqueueUpdate(T entity);

        IGRUpdatable<T> GREnqueueInsert(T entity);

        IGRDeletable<T> GREnqueueDelete(T entity);
        IGRDeletable<T> GREnqueueDelete();

        IGRQueriable<T> GRWhere(params Expression<Func<T, bool>>[] conditions);

        IGRQueriable<T> GROrderBy(params Expression<Func<T, object>>[] properties);
        IGRQueriable<T> GROrderByDescending(params Expression<Func<T, object>>[] properties);

        IGRQueriable<T> GRTake(int count);

        IGRQueriable<T> GRInclude(params Expression<Func<T, object>>[] properties);
        IGRQueriable<T> GRExclude(params Expression<Func<T, object>>[] properties);

        IGRQueriable<U> GRLeftJoin<U>(Expression<Func<T, object>> property1, Expression<Func<U, object>> property2) where U : new();
        IGRQueriable<U> GRRightJoin<U>(Expression<Func<T, object>> property1, Expression<Func<U, object>> property2) where U : new();
        IGRQueriable<U> GRInnerJoin<U>(Expression<Func<T, object>> property1, Expression<Func<U, object>> property2) where U : new();
        IGRQueriable<U> GRFullOuterJoin<U>(Expression<Func<T, object>> property1, Expression<Func<U, object>> property2) where U : new();

        IGRQueriable<T> GRAll();

        List<T> GRToList();
        Task<List<T>> GRToListAsync();
    }
}
