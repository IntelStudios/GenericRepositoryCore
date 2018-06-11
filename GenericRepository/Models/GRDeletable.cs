using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace GenericRepository.Models
{
    public class GRDeletable<T> : GRUpdatable, IGRDeletable<T>
    {
        IGRContext context;
        public List<GRCommandParam<T>> CommandParams { get; protected set; }
        public List<GRCommandParamWhere<T>> WhereClauses
        {
            get
            {
                return CommandParams
                        .Where(p => p is GRCommandParamWhere<T>)
                        .Select(p => p as GRCommandParamWhere<T>)
                        .ToList();
            }
        }

        public T Entity
        {
            get; private set;
        }

        public IGRRepository Repository
        {
            get; private set;
        }

        public GRDeletable(IGRContext context, T entity, IGRRepository repository) : base(typeof(T))
        {
            this.context = context;
            this.Entity = entity;
            this.Repository = repository;
            CommandParams = new List<GRCommandParam<T>>();
            Structure = GRDataTypeHelper.GetDBStructure(typeof(T));
        }

        public IGRDeletable<T> GRWhere(params Expression<Func<T, bool>>[] conditions)
        {
            foreach (var condition in conditions)
            {
                this.CommandParams.Add(new GRCommandParamWhere<T>(condition));
            }

            return this;
        }

        public GRExecutionStatistics GRExecute()
        {
            return context.Execute(this);
        }

        public async Task<GRExecutionStatistics> GRExecuteAsync()
        {
            return await context.ExecuteAsync(this);
        }
    }
}
