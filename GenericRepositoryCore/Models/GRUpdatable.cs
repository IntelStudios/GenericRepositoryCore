using GenericRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using GenericRepository.Helpers;

namespace GenericRepository.Models
{
    public abstract class GRUpdatable
    {
        public Type Type { get; private set; }
        public GRDBStructure Structure { set; get; }

        internal GRUpdatable(Type type)
        {
            this.Type = type;
        }

        public GRExecutionStatistics ExecutionStats { get; set; }
    }
    public class GRUpdatable<T> : GRUpdatable, IGRUpdatable<T>
    {
        IGRContext context;
        public List<GRCommandParam<T>> CommandParams { get; protected set; }

        public T Entity
        {
            get; private set;
        }

        public IGRRepository Repository
        {
            get; private set;
        }

        internal GRUpdatable(IGRContext context, T entity, IGRRepository repository) : base(typeof(T))
        {
            this.context = context;
            this.Entity = entity;
            this.Repository = repository;
            CommandParams = new List<GRCommandParam<T>>();
            Structure = GRDataTypeHelper.GetDBStructure(typeof(T));
        }

        public GRExecutionStatistics GRExecute()
        {
            return context.Execute(this);
        }

        public async Task<GRExecutionStatistics> GRExecuteAsync()
        {
            return await context.ExecuteAsync(this);
        }

        public IGRUpdatable<T> GRExclude(params Expression<Func<T, object>>[] propertyExpressions)
        {
            foreach (var propertyExpression in propertyExpressions)
            {
                string propertyName = GRDataTypeHelper.GetPropertyName(propertyExpression);
                GRDBProperty property = Structure[propertyName];
                CommandParams.Add(new GRCommandParamExclude<T>(property));
            }
            return this;
        }

        public IGRUpdatable<T> GRForceExclude(params Expression<Func<T, object>>[] propertyExpressions)
        {
            foreach (var propertyExpression in propertyExpressions)
            {
                string propertyName = GRDataTypeHelper.GetPropertyName(propertyExpression);
                GRDBProperty property = Structure[propertyName];
                CommandParams.Add(new GRCommandParamForceExclude<T>(property));
            }
            return this;
        }

        public IGRUpdatable<T> GRInclude(params Expression<Func<T, object>>[] propertyExpressions)
        {
            foreach (var propertyExpression in propertyExpressions)
            {
                string propertyName = GRDataTypeHelper.GetPropertyName(propertyExpression);
                GRDBProperty property = Structure[propertyName];
                CommandParams.Add(new GRCommandParamInclude<T>(property));
            }
            return this;
        }

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
                return CommandParams
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
                return CommandParams
                    .Where(p => p is GRCommandParamExclude<T>)
                    .Select(p => p as GRCommandParamExclude<T>)
                    .ToList();
            }
        }

        public List<GRCommandParamForceExclude<T>> ForceExcludedProperties
        {
            get
            {
                return CommandParams
                    .Where(p => p is GRCommandParamForceExclude<T>)
                    .Select(p => p as GRCommandParamForceExclude<T>)
                    .ToList();
            }
        }

        public List<GRCommandParamInclude<T>> IncludedProperties
        {
            get
            {
                return CommandParams
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

        public async Task GRUpdateAsync()
        {
            await context.UpdateAsync(this);
        }

        public async Task GRInsertAsync()
        {
            await context.InsertAsync(this);
        }
    }
}
