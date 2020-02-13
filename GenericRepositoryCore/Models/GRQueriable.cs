using GenericRepository.Enums;
using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public abstract class GRQueriable : IGRQueriable
    {
        public virtual Type Type { get; protected set; }
        public virtual IGRContext Context { get; protected set; }
        public GRDBStructure Structure { set; get; }

        public string Prefix { get; set; }
        public bool HasTemporaryPrefix { get; internal set; } = false;

        public IGRRepository Repository
        {
            get; protected set;
        }
        public GRExecutionStatistics ExecutionStats { get; set; }
        public abstract bool HasNoLock { get; }

        #region JOIN methods
        public GRJoinedQueriable Joined { get; set; }
        public GRJoinedQueriable JoiningQueriable { get; set; }

        bool IsPrefixAvailable(string prefix)
        {
            if (prefix == Prefix)
            {
                return false;
            }

            if (JoiningQueriable == null)
            {
                return true;
            }

            return JoiningQueriable.Queriable.IsPrefixAvailable(prefix);
        }

        protected string GeneratePrefix()
        {
            const string baseName = "t";

            if (IsPrefixAvailable(baseName))
            {
                return baseName;
            }

            int cntr = 2;

            while (true)
            {
                string prefix = baseName + cntr++;

                if (IsPrefixAvailable(prefix))
                {
                    return prefix;
                }
            }
        }
        #endregion
    }


    public class GRQueriable<T> : GRQueriable, IGRQueriable<T>
    {
        #region Fields & properties
        int? limit = null;
        public List<GRCommandParam<T>> QueryParams { get; private set; }
        #endregion

        #region Constructors
        public GRQueriable() : this(null, null)
        {

        }
        public GRQueriable(IGRContext context) : this(context, null)
        {

        }
        public GRQueriable(IGRContext context, IGRRepository repository)
        {
            this.Type = typeof(T);
            this.Context = context;
            this.Repository = repository;
            QueryParams = new List<GRCommandParam<T>>();
            Structure = GRDataTypeHelper.GetDBStructure(typeof(T));
        }
        #endregion

        #region Query execution methods
        public IEnumerator<T> GetEnumerator()
        {
            return GRToList().GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        public List<T> GRToList()
        {
            List<T> result = Context.ExecuteQuery(this).ToList();
            return result;
        }

        public async Task<List<T>> GRToListAsync()
        {
            List<T> result = (await Context.ExecuteQueryAsync(this)).ToList();
            return result;
        }

        public T GRFirstOrDefault()
        {
            this.QueryParams.Add(new GRCommandParamLimit<T>(1));
            List<T> result = GRToList();
            return result.Any() ? result.First() : default(T);
        }

        public async Task<T> GRFirstOrDefaultAsync()
        {
            this.QueryParams.Add(new GRCommandParamLimit<T>(1));
            List<T> result = await GRToListAsync();
            return result.Any() ? result.First() : default(T);
        }

        public int GRCount()
        {
            int count = Context.ExecuteCount(this);
            return count;
        }

        public async Task<int> GRCountAsync()
        {
            int count = await Context.ExecuteCountAsync(this);
            return count;
        }

        public GRTable GRToTable()
        {
            GRTable result = Context.ExecuteJoinQuery(this);
            return result;
        }

        public async Task<GRTable> GRToTableAsync()
        {
            GRTable result = await Context.ExecuteJoinQueryAsync(this);
            return result;
        }
        #endregion

        #region Query build methods
        public IGRQueriable<T> GRWhere(params Expression<Func<T, bool>>[] conditions)
        {
            foreach (var condition in conditions)
            {
                this.QueryParams.Add(new GRCommandParamWhere<T>(condition));
            }

            return this;
        }

        public IGRQueriable<T> GRInclude(params Expression<Func<T, object>>[] propertyExpressions)
        {
            foreach (var propertyExpression in propertyExpressions)
            {
                string propertyName = GRDataTypeHelper.GetPropertyName(propertyExpression);
                GRDBProperty property = Structure[propertyName];
                QueryParams.Add(new GRCommandParamInclude<T>(property));
            }
            return this;
        }

        public IGRQueriable<T> GRExcludeAll()
        {
            QueryParams.Add(new GRCommandParamExcludeAll<T>());
            return this;
        }

        public IGRQueriable<T> GRExclude(params Expression<Func<T, object>>[] propertyExpressions)
        {
            foreach (var propertyExpression in propertyExpressions)
            {
                string propertyName = GRDataTypeHelper.GetPropertyName(propertyExpression);
                GRDBProperty property = Structure[propertyName];
                QueryParams.Add(new GRCommandParamExclude<T>(property));
            }
            return this;
        }
        public IGRQueriable<T> GRDistinct()
        {
            this.QueryParams.Add(new GRCommandParamDistinct<T>());
            return this;
        }
        public IGRQueriable<T> GRTake(int count)
        {
            if (limit.HasValue) throw new GRQueryBuildFailedException("Take has been already set!");

            limit = count;

            this.QueryParams.Add(new GRCommandParamLimit<T>(count));

            return this;
        }

        public IGRQueriable<T> GRNoLock()
        {
            QueryParams.Add(new GRCommandParamNoLock<T>());
            return this;
        }
        #endregion

        #region Ordering
        public IGRQueriable<T> GROrderBy(params Expression<Func<T, object>>[] propertyExpressions)
        {
            foreach (var propertyExpression in propertyExpressions)
            {
                string properyName = GRDataTypeHelper.GetPropertyName(propertyExpression);
                GRDBProperty prop = Structure[properyName];
                this.QueryParams.Add(new GRCommandParamOrder<T>(prop, Enums.GRQueryOrderDirection.Ascending));
            }

            return this;
        }

        public IGRQueriable<T> GROrderByDescending(params Expression<Func<T, object>>[] properties)
        {
            foreach (var property in properties)
            {
                string properyName = GRDataTypeHelper.GetPropertyName(property);
                GRDBProperty prop = this.Structure[properyName];
                this.QueryParams.Add(new GRCommandParamOrder<T>(prop, Enums.GRQueryOrderDirection.Descending));
            }

            return this;
        }
        #endregion

        #region Joining
        public IGRQueriable<U> GRLeftJoin<U>(Expression<Func<T, object>> joiningLocalProperty, Expression<Func<U, object>> joinedProperty) where U : new()
        {
            GRQueriable<U> joinedQueriable = PrepareJoinQueriable(joiningLocalProperty, joinedProperty, GRJoinType.LeftJoin);
            return joinedQueriable;
        }

        public IGRQueriable<U> GRRightJoin<U>(Expression<Func<T, object>> joiningLocalProperty, Expression<Func<U, object>> joinedProperty) where U : new()
        {
            GRQueriable<U> joinedQueriable = PrepareJoinQueriable(joiningLocalProperty, joinedProperty, GRJoinType.RightJoin);
            return joinedQueriable;
        }

        public IGRQueriable<U> GRInnerJoin<U>(Expression<Func<T, object>> joiningLocalProperty, Expression<Func<U, object>> joinedProperty) where U : new()
        {
            GRQueriable<U> joinedQueriable = PrepareJoinQueriable(joiningLocalProperty, joinedProperty, GRJoinType.InnerJoin);
            return joinedQueriable;
        }

        public IGRQueriable<U> GRFullOuterJoin<U>(Expression<Func<T, object>> joiningLocalProperty, Expression<Func<U, object>> joinedProperty) where U : new()
        {
            GRQueriable<U> joinedQueriable = PrepareJoinQueriable(joiningLocalProperty, joinedProperty, GRJoinType.FullOuterJoin);
            return joinedQueriable;
        }

        private GRQueriable<U> PrepareJoinQueriable<U>(Expression<Func<T, object>> joiningLocalProperty, Expression<Func<U, object>> joinedProperty, GRJoinType type) where U : new()
        {
            GRQueriable<U> joinedQueriable = new GRQueriable<U>(Context, Repository);

            joinedQueriable.JoiningQueriable = new GRJoinedQueriable
            {
                Queriable = this,
                Type = type,
                SourcePropertyName = GRDataTypeHelper.GetPropertyName(joiningLocalProperty),
                TargetPropertyName = GRDataTypeHelper.GetPropertyName(joinedProperty)
            };

            Joined = new GRJoinedQueriable
            {
                Queriable = joinedQueriable,
                Type = type,
                SourcePropertyName = GRDataTypeHelper.GetPropertyName(joinedProperty),
                TargetPropertyName = GRDataTypeHelper.GetPropertyName(joiningLocalProperty)
            };

            if (string.IsNullOrEmpty(this.Prefix))
            {
                this.Prefix = GeneratePrefix();
                this.HasTemporaryPrefix = true;
            }

            joinedQueriable.Prefix = joinedQueriable.GeneratePrefix();
            joinedQueriable.HasTemporaryPrefix = true;

            return joinedQueriable;
        }
        #endregion

        #region Query params methods
        public bool HasExcludedProperties
        {
            get
            {
                return ExcludedProperties.Count > 0;
            }
        }

        public bool HasForceExcludedProperties
        {
            get
            {
                return ForceExcludedProperties.Count > 0;
            }
        }

        public bool HasExcludedAllProperties
        {
            get
            {
                return QueryParams
                    .Where(p => p is GRCommandParamExcludeAll<T>)
                    .Any();
            }
        }

        public bool HasIncludedProperties
        {
            get
            {
                return IncludedProperties.Count > 0;
            }
        }

        public List<GRCommandParamExclude<T>> ExcludedProperties
        {
            get
            {
                return QueryParams
                    .Where(p => p is GRCommandParamExclude<T>)
                    .Select(p => p as GRCommandParamExclude<T>)
                    .ToList();
            }
        }

        public List<GRCommandParamForceExclude<T>> ForceExcludedProperties
        {
            get
            {
                return QueryParams
                    .Where(p => p is GRCommandParamForceExclude<T>)
                    .Select(p => p as GRCommandParamForceExclude<T>)
                    .ToList();
            }
        }

        public List<GRCommandParamInclude<T>> IncludedProperties
        {
            get
            {
                return QueryParams
                    .Where(p => p is GRCommandParamInclude<T>)
                    .Select(p => p as GRCommandParamInclude<T>)
                    .ToList();
            }
        }

        public bool ContainsExcludedProperty(string propertyName)
        {
            List<GRCommandParamExclude<T>> properties = ExcludedProperties;
            properties = properties.Where(p => p.Property.PropertyInfo.Name == propertyName).ToList();
            return properties.Count > 0;
        }

        public bool ContainsIncludedProperty(string propertyName)
        {
            List<GRCommandParamInclude<T>> properties = IncludedProperties;
            properties = properties.Where(p => p.Property.PropertyInfo.Name == propertyName).ToList();
            return properties.Count > 0;
        }

        public bool HasLimit
        {
            get {
                return Limit.HasValue;
            }
        }

        public bool IsDistinct
        {
            get
            {
                bool isDistinct = QueryParams
                    .Where(p => p is GRCommandParamDistinct<T>)
                    .Any();

                return isDistinct;
            }
        }

        public int? Limit
        {
            get
            {
                List<GRCommandParamLimit<T>> limits = QueryParams
                    .Where(p => p is GRCommandParamLimit<T>)
                    .Select(p => p as GRCommandParamLimit<T>)
                    .ToList();
                GRCommandParamLimit<T> queryLimit = limits.LastOrDefault();

                return queryLimit?.Count;
            }
        }

        public override bool HasNoLock
        {
            get
            {
                bool hasNoLock = QueryParams
                    .Where(p => p is GRCommandParamNoLock<T>)
                    .Any();

                return hasNoLock;
            }
        }

        public List<GRCommandParamWhere<T>> WhereClauses
        {
            get
            {
                return QueryParams
                        .Where(p => p is GRCommandParamWhere<T>)
                        .Select(p => p as GRCommandParamWhere<T>)
                        .ToList();
            }
        }

        public List<GRCommandParamOrder<T>> OrderByProperties
        {
            get
            {
                return QueryParams
                    .Where(p => p is GRCommandParamOrder<T>)
                    .Select(p => p as GRCommandParamOrder<T>)
                    .ToList();
            }
        }
        #endregion
    }
}
