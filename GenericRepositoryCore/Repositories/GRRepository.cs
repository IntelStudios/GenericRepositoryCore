using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GenericRepository.Repositories
{
    public class GRRepository : IGRRepository
    {
        #region Fields
        protected IGRContext context = null;
        #endregion

        #region Constructors
        public GRRepository(IGRContext context)
        {
            this.context = context;
        }
        #endregion

        #region Query methods
        public virtual R GRGet<R>(object key)
        {
            Expression<Func<R, bool>> lambda = GetGRGetLambda<R>(key);
            return GRWhere(lambda).GRTake(1).FirstOrDefault();
        }

        public virtual async Task<R> GRGetAsync<R>(object key)
        {
            Expression<Func<R, bool>> lambda = GetGRGetLambda<R>(key);
            return await GRWhere<R>(lambda).GRTake(1).GRFirstOrDefaultAsync();
        }

        public IGRQueriable<R> GRWhere<R>(params Expression<Func<R, bool>>[] conditions)
        {
            GRQueriable<R> queryBuilder = new GRQueriable<R>(context, this);
            return queryBuilder.GRWhere(conditions);
        }

        protected Expression<Func<R, bool>> GetGRGetLambda<R>(object key)
        {
            List<GRDBProperty> keys = GRDataTypeHelper.GetDBStructure(typeof(R)).KeyProperties;

            if (keys.Count == 0)
            {
                throw new GRQueryBuildFailedException("GET method cannot be used, type '{0}' has no defined primary keys!", typeof(R));
            }

            if (keys.Count > 1)
            {
                throw new GRQueryBuildFailedException("GET method cannot be used, type '{0}' has defined multiple keys!", typeof(R));
            }

            ParameterExpression argParam = Expression.Parameter(typeof(R), "key");
            Expression nameProperty = Expression.Property(argParam, keys.First().PropertyInfo.Name);
            Expression exp = Expression.Equal(nameProperty, Expression.Constant(key));
            Expression<Func<R, bool>> lambda = Expression.Lambda<Func<R, bool>>(exp, argParam);
            return lambda;
        }

        public IGRQueriable<R> GRAll<R>()
        {
            GRQueriable<R> queryBuilder = new GRQueriable<R>(context, this);
            return queryBuilder;
        }
        #endregion

        #region Insert/Update/Delete methods
        protected List<Expression<Func<R, bool>>> GetGRDeleteLambdas<R>(R entity)
        {
            List<Expression<Func<R, bool>>> ret = new List<Expression<Func<R, bool>>>();
            List<GRDBProperty> keys = GRDataTypeHelper.GetDBStructure(typeof(R)).KeyProperties;

            foreach (var key in keys)
            {
                ParameterExpression argParam = Expression.Parameter(typeof(R), "key");
                Expression nameProperty = Expression.Property(argParam, key.PropertyInfo.Name);

                object value = key.PropertyInfo.GetValue(entity);

                Expression exp = Expression.Equal(nameProperty, Expression.Constant(value));
                Expression<Func<R, bool>> lambda = Expression.Lambda<Func<R, bool>>(exp, argParam);

                ret.Add(lambda);
            }

            return ret;
        }

        public IGRUpdatable<R> GREnqueueUpdate<R>(R entity)
        {
            GRUpdatable<R> updatable = new GRUpdatable<R>(context, entity, this);
            context.EnqueueUpdate(updatable);
            return updatable;
        }

        public IGRUpdatable<R> GREnqueueInsert<R>(R entity)
        {
            GRUpdatable<R> updatable = new GRUpdatable<R>(context, entity, this);
            context.EnqueueInsert(updatable);
            return updatable;
        }

        public IGRDeletable<R> GREnqueueDelete<R>(R entity)
        {
            GRDeletable<R> deletable = new GRDeletable<R>(context, entity, this);
            var whereList = GetGRDeleteLambdas(entity);

            foreach (var whereItem in whereList)
            {
                deletable.GRWhere(whereItem);
            }

            context.EnqueueDelete(deletable);
            return deletable;
        }

        public IGRDeletable<R> GREnqueueDelete<R>()
        {
            GRDeletable<R> queryBuilder = new GRDeletable<R>(context, default(R), this);
            return queryBuilder;
        }
        #endregion

        #region Save events
        public virtual void PrepareForSave()
        {

        }

        public virtual Task PrepareForSaveAsync()
        {
            return Task.FromResult(default(object));
        }
        #endregion

        #region Count sync/async methods
        public int GRCount<R>()
        {
            GRQueriable<R> queryBuilder = new GRQueriable<R>(context, this);
            return context.ExecuteCount<R>(queryBuilder);
        }

        public async Task<int> GRCountAsync<R>()
        {
            GRQueriable<R> queryBuilder = new GRQueriable<R>(context, this);
            return await context.ExecuteCountAsync<R>(queryBuilder);
        }
        #endregion
    }
    public class GRRepository<T> : GRRepository, IGRRepository<T>
    {
        #region Constructors
        public GRRepository(IGRContext context) : base(context)
        {
            this.context = context;
        }
        #endregion

        #region Get entity sync/async methods
        public virtual T GRGet(object key)
        {
            return GRGet<T>(key);
        }

        public virtual Task<T> GRGetAsync(object key)
        {
            return GRGetAsync<T>(key);
        }

        private Expression<Func<T, bool>> GetGRGetLambda(object key)
        {
            return GetGRGetLambda<T>(key);
        }

        private List<Expression<Func<T, bool>>> GetGRDeleteLambdas(T entity)
        {
            return GetGRDeleteLambdas<T>(entity);
        }
        #endregion

        #region Query methods
        public IGRQueriable<T> GRWhere(params Expression<Func<T, bool>>[] conditions)
        {
            return GRWhere<T>(conditions);
        }

        public IGRQueriable<U> GRLeftJoin<U>(Expression<Func<T, object>> property1, Expression<Func<U, object>> property2) where U : new()
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return queryBuilder.GRLeftJoin<U>(property1, property2);
        }

        public IGRQueriable<U> GRRightJoin<U>(Expression<Func<T, object>> property1, Expression<Func<U, object>> property2) where U : new()
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return queryBuilder.GRRightJoin<U>(property1, property2);
        }

        public IGRQueriable<U> GRInnerJoin<U>(Expression<Func<T, object>> property1, Expression<Func<U, object>> property2) where U : new()
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return queryBuilder.GRInnerJoin<U>(property1, property2);
        }

        public IGRQueriable<U> GRFullOuterJoin<U>(Expression<Func<T, object>> property1, Expression<Func<U, object>> property2) where U : new()
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return queryBuilder.GRFullOuterJoin<U>(property1, property2);
        }

        public IGRQueriable<T> GROrderBy(params Expression<Func<T, object>>[] properties)
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return queryBuilder.GROrderBy(properties);
        }

        public IGRQueriable<T> GROrderByDescending(params Expression<Func<T, object>>[] properties)
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return queryBuilder.GROrderByDescending(properties);
        }

        public IGRQueriable<T> GRTake(int count)
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return queryBuilder.GRTake(count);
        }

        public IGRQueriable<T> GRAll()
        {
            return GRAll<T>();
        }

        public IGRQueriable<T> GRInclude(params Expression<Func<T, object>>[] properties)
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return queryBuilder.GRInclude(properties);
        }

        public IGRQueriable<T> GRExclude(params Expression<Func<T, object>>[] properties)
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return queryBuilder.GRExclude(properties);
        }        
        #endregion

        #region ToList sync/async methods
        public virtual List<T> GRToList()
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return context.ExecuteQuery(queryBuilder);
        }

        public virtual async Task<List<T>> GRToListAsync()
        {
            GRQueriable<T> queryBuilder = new GRQueriable<T>(context, this);
            return await context.ExecuteQueryAsync(queryBuilder);
        }
        #endregion

        #region Count sync/async methods
        public int GRCount()
        {
            return GRCount<T>();
        }

        public Task<int> GRCountAsync()
        {
            return GRCountAsync<T>();
        }
        #endregion

        #region Insert/Update/Delete methods
        public IGRUpdatable<T> GREnqueueUpdate(T entity)
        {
            return GREnqueueUpdate<T>(entity);
        }

        public IGRUpdatable<T> GREnqueueInsert(T entity)
        {
            return GREnqueueInsert<T>(entity);
        }

        public IGRDeletable<T> GREnqueueDelete(T entity)
        {
            GRDeletable<T> deletable = new GRDeletable<T>(context, entity, this);
            var whereList = GetGRDeleteLambdas(entity);

            foreach (var whereItem in whereList)
            {
                deletable.GRWhere(whereItem);
            }

            context.EnqueueDelete(deletable);
            return deletable;
        }

        public IGRDeletable<T> GREnqueueDelete()
        {
            GRDeletable<T> queryBuilder = new GRDeletable<T>(context, default(T), this);
            return queryBuilder;
        }
        #endregion
    }
}
